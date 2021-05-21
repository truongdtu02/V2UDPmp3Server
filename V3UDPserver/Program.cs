using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UDP_send_packet_frame;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace V2UDPmp3Server
{
    class Program
    {
        public static List<client_IPEndPoint> clientList;



        static void Main(string[] args)
        {
            //Set directory where app should look for FFmpeg 
            //FFmpeg.ExecutablesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg");
            //Get latest version of FFmpeg. It's great idea if you don't know if you had installed FFmpeg.
            //await FFmpeg.GetLatestVersion();



            clientList = new List<client_IPEndPoint>()
            {
                 new client_IPEndPoint(){ ID_client = "20154023", On = true, NumSend = 1},
                 new client_IPEndPoint(){ ID_client = "20164023", On = false},
                 new client_IPEndPoint(){ ID_client = "00000001", On = true},
                 new client_IPEndPoint(){ ID_client = "00000002", On = true},
                 new client_IPEndPoint(){ ID_client = "00000003", On = true},
                 new client_IPEndPoint(){ ID_client = "00000004", On = true},
                 new client_IPEndPoint(){ ID_client = "00000005", On = true},
                 new client_IPEndPoint(){ ID_client = "00000006", On = true},
                 new client_IPEndPoint(){ ID_client = "00000007", On = true},
                 new client_IPEndPoint(){ ID_client = "00000008", On = true},
                 new client_IPEndPoint(){ ID_client = "00000009", On = true},
                 new client_IPEndPoint(){ ID_client = "sim", On = true, NumSend = 1},
            };

            string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string subPath = "converted"; // Your code goes here
            subPath = Path.Combine(curPath, subPath);

            /*
            //check if it is exits delete
            //if (Directory.Exists(subPath)) Directory.Delete(subPath, true);

            //Directory.CreateDirectory(subPath); //create*/

            //ffmpeg.exe -i E:\b1.mp3 -codec:a libmp3lame -b:a 48k -ac 1 -ar 24000 D:\b1mono.mp3
            bool converterDone = false;
            Thread nethos = new Thread(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "ffmpeg",
                    CreateNoWindow = false,
                    //Arguments = $"-y -i {soundList[0].FilePath} -codec:a libmp3lame -b:a 8k -ac 1 -ar 24000 {Path.Combine(curPath, "b18k.mp3")}",
                };

                DirectoryInfo di = new DirectoryInfo(curPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    if (file.Extension == ".mp3")
                    {
                        Process proc = new Process();
                        startInfo.Arguments = $"-y -i {Path.Combine(curPath, file.Name)} -b:a 48k -ac 1 -ar 24000 {Path.Combine(subPath, file.Name)}";
                        proc.StartInfo = startInfo;
                        proc.Start();
                        proc.WaitForExit();
                        proc.Close();
                    }
                }
                converterDone = true;
            });
            //check if it is not exist create and convert
            if (!Directory.Exists(subPath))
            {
                Directory.CreateDirectory(subPath); //create*/         
                nethos.Start();
            }

            //wait until converter is done
            while (true)
            {
                if (converterDone) break;
                Thread.Sleep(1000);
            }
            Console.WriteLine("Done converter");

            List<soundTrack> soundList = new List<soundTrack>();
            DirectoryInfo di = new DirectoryInfo(subPath);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Extension == ".mp3")
                {
                    soundList.Add(new soundTrack() { FilePath = Path.Combine(subPath, file.Name) });
                }
            }

            //launch
            UDPsocket udpSocket = new UDPsocket();
            udpSocket.launchUDPsocket(soundList, clientList);

            control(udpSocket);
        }
        private static IEnumerable GetFilesToConvert(string directoryPath)
        {
            //Return all files excluding mp4 because I want convert it to mp4
            return new DirectoryInfo(directoryPath).GetFiles().Where(x => x.Extension == ".mp3");
        }

        static async void RunConversion(string inPath, string outPath)
        {
            //Set directory where app should look for FFmpeg 
            //FFmpeg.SetExecutablesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg");
            FFmpeg.SetExecutablesPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg"));
            //Get latest version of FFmpeg. It's great idea if you don't know if you had installed FFmpeg.
            FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

            Queue filesToConvert = new Queue();
            DirectoryInfo di = new DirectoryInfo(inPath);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Extension == ".mp3") filesToConvert.Enqueue(file);
            }
            await Console.Out.WriteLineAsync($"Find {filesToConvert.Count} files to convert.");

            //string filePath = Path.Combine("C:", "samples", "SampleVideo.mp4");
            //string inputMp3Path = Path.Combine("E:", "bai1.mp3");
            //IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputMp3Path);

            //while (filesToConvert.(out FileInfo fileToConvert))
            foreach(FileInfo fileToConvert in filesToConvert)
            {
                Console.WriteLine("1");
                //Save file to the same location with changed extension
                //string outputFileName = Path.ChangeExtension(fileToConvert.FullName, ".mp4");
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(fileToConvert.DirectoryName);
                //var mediaInfo = await MediaInfo.Get(fileToConvert);
                //var videoStream = mediaInfo.VideoStreams.First();
                var audioStream = mediaInfo.AudioStreams.First();

                audioStream.SetBitrate(8000)
                    .SetChannels(1)
                    .SetSampleRate(24000)
                    .SetCodec(AudioCodec.mp3);
                ////Change some parameters of video stream
                ////videoStream
                ////    Rotate video counter clockwise
                ////    .Rotate(RotateDegrees.CounterClockwise)
                ////    Set size to 480p
                ////    .SetSize(VideoSize.Hd480)
                ////    Set codec which will be used to encode file.If not set it's set automatically according to output file extension
                ////   .SetCodec(VideoCodec.H264);

                //Create new conversion object
                var conversion = FFmpeg.Conversions.New()
                    //Add audio stream to output file
                    .AddStream(audioStream)
                    //Set output file path
                    .SetOutput(outPath)
                    //SetOverwriteOutput to overwrite files. It's useful when we already run application before
                    .SetOverwriteOutput(true)
                    //Disable multithreading
                    .UseMultiThread(false)
                    //Set conversion preset. You have to chose between file size and quality of video and duration of conversion
                    .SetPreset(ConversionPreset.UltraFast);

                //Add log to OnProgress
                //conversion.OnProgress += async (sender, args) =>
                //{
                //    //Show all output from FFmpeg to console
                //    await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%] {fileToConvert.Name}");
                //};
                //Start conversion
                conversion.Start();

                Console.WriteLine($"Finished converion file [{fileToConvert.Name}]");
            }
        }

        static void control(UDPsocket udpSocket)
        {
            var statusNow = udpSocket.Status;
            int currentTime = udpSocket.TimePlaying_song_s; //second
            int duration = 0;
            int song_ID = 0; //order of song in soundList 
            Console.Title = "Project truyen thanh!!!";
            Console.OutputEncoding = Encoding.UTF8;
            int cursor = Console.CursorTop;
            Console.WriteLine("Status: {0}", statusNow); //line 2-7
            Console.WriteLine("Song: {0}", song_ID); //line 3-6
            Console.WriteLine("Current time play: "); //line 4-19
            Console.WriteLine("Duration: 0:0"); //line 5-10
            Console.WriteLine("Send time: {0,2}", clientList[0].NumSend); //line 6-11

            Console.WriteLine("Lệnh:");
            Console.WriteLine(" 1:Play/ Resume");
            Console.WriteLine(" 2:Pause");
            Console.WriteLine(" 3:Next");
            Console.WriteLine(" 4:Previous");
            Console.WriteLine(" 5:Stop");
            Console.WriteLine(" 6:Increase send time");
            Console.WriteLine(" 7:Decrease send time");
            Console.Write("Nhập số tương ứng để tiến hành điều khiển: "); //line 10


            //thread update status every 1s
            Thread displayStatus = new Thread(() =>
            {
                while (true)
                {
                    if (statusNow != udpSocket.Status)
                    {
                        statusNow = udpSocket.Status;
                        Console.SetCursorPosition(8, cursor);
                        Console.Write(statusNow + "    ");
                    }
                    if (song_ID != udpSocket.SongID)
                    {
                        song_ID = udpSocket.SongID;
                        Console.SetCursorPosition(6, cursor + 1);
                        Console.Write(song_ID);
                    }

                    currentTime = udpSocket.TimePlaying_song_s;
                    Console.SetCursorPosition(19, cursor + 2);
                    Console.Write("{0,2}:{1,2}", currentTime / 60, currentTime % 60);

                    if (duration != udpSocket.Duration_song_s)
                    {
                        duration = udpSocket.Duration_song_s;
                        Console.SetCursorPosition(10, cursor + 3);
                        Console.Write("{0,2}:{1,2}", duration / 60, duration % 60);
                    }
                    //Console.SetCursorPosition(43, 9);
                    Thread.Sleep(1000);
                }
            });
            displayStatus.Priority = ThreadPriority.BelowNormal;
            displayStatus.Start();

            //thread control for test
            Thread readControl = new Thread(() =>
            {
                while (true)
                {
                    //Console.SetCursorPosition(43, 9);
                    var control = Console.ReadKey(true);
                    switch (control.KeyChar)
                    {
                        case '1': //play/resume
                            //
                            if (statusNow == UDPsocket.status_enum.STOP) //play
                            {
                                udpSocket.UDPsocketSend();
                            }
                            else if (statusNow == UDPsocket.status_enum.PAUSE) //resume
                            {
                                udpSocket.controlThreadSend(2);//resume
                            }
                            break;
                        case '2': //pause
                            //
                            if (statusNow == UDPsocket.status_enum.PLAY) //play
                            {
                                udpSocket.controlThreadSend(1);//pause
                            }
                            break;
                        case '3': //next
                            if (statusNow != UDPsocket.status_enum.STOP)
                            {
                                udpSocket.controlThreadSend(3);
                                //udpSocket.ClientList[0].NumSend = 2;
                            }
                            break;
                        case '4': //previous
                            if (statusNow != UDPsocket.status_enum.STOP)
                            {
                                udpSocket.controlThreadSend(4);
                            }
                            break;
                        case '5': //stop
                            //
                            if (statusNow != UDPsocket.status_enum.STOP)
                            {
                                udpSocket.controlThreadSend(5);//stop
                            }
                            break;
                        case '6': //increase send time
                            clientList[0].NumSend++;
                            Console.SetCursorPosition(11, cursor + 4);
                            Console.Write("{0,2}", clientList[0].NumSend);
                            break;
                        case '7': //decrease send time
                            if (clientList[0].NumSend > 1)
                            {
                                clientList[0].NumSend--;
                                Console.SetCursorPosition(11, cursor + 4);
                                Console.Write("{0,2}", clientList[0].NumSend);
                            }
                            break;
                    }
                }
            });
            readControl.Priority = ThreadPriority.Lowest;
            readControl.Start();
        }
    }
}

