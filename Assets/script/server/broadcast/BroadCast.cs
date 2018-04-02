using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Osblow.Net.Server
{
    class BroadCastServer
    {
        private static bool s_hasSent = false;

        private static Socket s_sock;
        private static IPEndPoint[] s_ieps;
        private static Thread s_sendThread;

        private static GameConf s_gameConf;


        public static void Start(GameConf gameConf)
        {
            if (s_hasSent)
            {
                return;
            }

            s_gameConf = gameConf;

            s_sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            //255.255.255.255
            s_ieps = new IPEndPoint[BroadConstant.BroadcastPorts.Length];
            for (int i = 0; i < s_ieps.Length; i++)
            {
                s_ieps[i] = new IPEndPoint(IPAddress.Broadcast, BroadConstant.BroadcastPorts[i]);
            }

            s_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            s_sendThread = new Thread(BroadcastMessage);
            s_sendThread.Start();
            s_hasSent = true;
            //sock.Close();
        }

        public static void Stop()
        {
            if (!s_hasSent)
            {
                return;
            }

            s_sendThread.Abort();
            s_sock.Close();
            s_hasSent = false;
        }

        private static void BroadcastMessage()
        {
            while (true)
            {
                for (int i = 0; i < s_ieps.Length; i++)
                {
                    //UnityEngine.Debug.Log("send to port " + s_ieps[i]);
                    byte[] data = CmdBroadcast.SendRoomConf(Globals.GetPrivateIP(), 4050, s_gameConf);
                    s_sock.SendTo(data, s_ieps[i]);
                }
                Thread.Sleep(2000);
            }

        }
    }
}