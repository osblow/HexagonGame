using System;using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Net.NetworkInformation;


namespace Osblow.Net.Server
{
    class BroadCastReciever
    {
        static Socket sock;
        static int cur_tryPortIndex = 1;

        public static Action<string, int, GameConf> OnGetConf = null;

        public static void Start(int port = 9050)
        {
            try
            {
                sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint iep =
                new IPEndPoint(IPAddress.Any, port);
                sock.Bind(iep);

                Thread t = new Thread(Recieve);
                t.Start();
                //sock.Close();
            }
            catch (SocketException)
            {
                Debug.Log("端口已占用");
                if (cur_tryPortIndex < BroadConstant.BroadcastPorts.Length)
                {
                    Debug.Log("start trying " + BroadConstant.BroadcastPorts[cur_tryPortIndex]);
                    Start(BroadConstant.BroadcastPorts[cur_tryPortIndex]);
                    ++cur_tryPortIndex;
                }
                else
                {
                    Debug.LogError("同一台机器最多开10个客户端");
                }
            }
        }

        private static void Recieve()
        {
            while (true)
            {
                Debug.Log("Ready to receive…");

                byte[] data = new byte[1024];
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int recv = sock.ReceiveFrom(data, ref ep);
                if (OnGetConf != null)
                {
                    //Globals.Instance.AsyncInvokeMng.Events.Add(
                    //    delegate { OnGetConf(((IPEndPoint)ep).Address.ToString(), ((IPEndPoint)ep).Port, GameConf.Unpack(data)); });
                    Globals.Instance.AsyncInvokeMng.Events.Add(delegate 
                    {
                        Globals.Instance.SendMessage(MsgType.OnFindServer, 
                            ((IPEndPoint)ep).Address.ToString(), 
                            ((IPEndPoint)ep).Port, GameConf.Unpack(data));
                    });
                }

                Debug.LogFormat("received: {0}bytes from: {1}:{2}", data.Length, ((IPEndPoint)ep).Address, ((IPEndPoint)ep).Port);
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 检查端口是否被占用
        /// </summary>
        //private static bool CheckPortActive(int port)
        //{
        //    IPGlobalProperties ipProp = IPGlobalProperties.GetIPGlobalProperties();
        //    IPEndPoint[] all = ipProp.GetActiveUdpListeners();

        //    if(all == null)
        //    {
        //        return false;
        //    }

        //    for (int i = 0; i < all.Length; i++)
        //    {
        //        Debug.Log(all[i].Port);
        //        if(all[i].Port == port)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}