using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Osblow.Net.Server
{
    class BroadCastServer
    {
        private static Socket s_sock;
        private static IPEndPoint[] s_ieps;
        private static GameConf s_conf;


        public static void Start(GameConf conf)
        {
            s_conf = conf;

            s_sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            //255.255.255.255
            s_ieps = new IPEndPoint[BroadConstant.BroadcastPorts.Length];
            for (int i = 0; i < s_ieps.Length; i++)
            {
                s_ieps[i] = new IPEndPoint(IPAddress.Broadcast, BroadConstant.BroadcastPorts[i]);
            }

            s_sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            Thread t = new Thread(BroadcastMessage);
            t.Start();
            //sock.Close();
        }

        private static void BroadcastMessage()
        {
            while (true)
            {
                for (int i = 0; i < s_ieps.Length; i++)
                {
                    UnityEngine.Debug.Log("send to port " + s_ieps[i]);
                    s_sock.SendTo(GameConf.Pack(s_conf), s_ieps[i]);
                }
                Thread.Sleep(2000);
            }

        }

    }
}