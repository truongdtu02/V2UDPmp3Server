using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UDP_send_packet_frame;
using MP3_ADU_namespace;
using System.Linq;

namespace V2UDPmp3Server
{
    class Program
    {
        public static List<client_IPEndPoint> clientList;
        
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");

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
                 new client_IPEndPoint(){ ID_client = "000000010", On = true},
            };

            List<soundTrack> soundList = new List<soundTrack>()
            {
                new soundTrack(){ FilePath = @"E:\bai111.mp3"},
                new soundTrack(){ FilePath = @"E:\bai124k.mp3"}
            };

            List<soundTrack> soundListServer = new List<soundTrack>()
            {
                new soundTrack(){ FilePath = "bai111.mp3"},
                new soundTrack(){ FilePath = "bai124k.mp3"}
                //new soundTrack(){ FilePath = "LoveIsBlue.mp3"}
            };

            UDPsocket udpSocket = new UDPsocket();
            //var _status = udpSocket.Status; //get status (PLAY, PAUSE, STOP)
            string filePath;
            string filePath1 = @"E:/bai111.mp3";
            string filePath2 = "bai111.mp3";
            Console.Write("Server  1: Local, 2: online: ");
            char ch = Console.ReadKey().KeyChar;
            if(ch == '1')
            {
                filePath = filePath1;
            }
            else
            {
                filePath = filePath2;
                soundList = soundListServer;
            }
            //byte[] mp3_buff = File.ReadAllBytes(filePath).Skip(237).ToArray();
            ////MP3_ADU mp3file = new MP3_ADU(mp3_buff, mp3_buff.Length);
            ////int numFrame = 0;

            //ADU_frame adufile = new ADU_frame(mp3_buff, mp3_buff.Length);
            //int aduNumFrame = 0;

            ////FileStream stream = new FileStream(@"E:\test10.mp3", FileMode.Append);

            //List<byte[]> aduFrameList = new List<byte[]>();
            //while (true)
            //{
            //    byte[] aduframe = adufile.ReadNextADUFrame();
            //    if (aduframe != null)
            //    {
            //        aduNumFrame++;
            //        //AppendAllBytes(@"E:\adu.mp3", aduframe);
            //        //stream.Write(aduframe, 0, aduframe.Length);
            //        aduFrameList.Add(aduframe);
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //stream.Close();

            //launch
            //udpSocket.launchUDPsocket(soundList, clientList);
            udpSocket.launchUDPsocket(soundList, clientList);
            //create UDP socket listen from client
            udpSocket.UDPsocketListen();
            //create UDP socket for sending mp3 frame to client
            //udpSocket.UDPsocketSend();
            control(udpSocket);
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

