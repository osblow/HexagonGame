using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;


namespace Osblow.Net.Server
{
    public class BroadCastReciever
    {
        static bool s_hasStarted = false;
        static Socket s_sock;
        static Thread s_thread;
        static int cur_tryPortIndex = 1;

        public static Action<string, int, GameConf> OnGetConf = null;

        public static void Start(int port = 9050)
        {
            if (s_hasStarted)
            {
                return;
            }

            try
            {
                s_sock = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, port);
                s_sock.Bind(iep);

                s_thread = new Thread(Recieve);
                s_thread.Start();
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
            catch (Exception)
            {
                throw;
            }

            s_hasStarted = true;
        }

        private static void Recieve()
        {
            while (true)
            {
                //Debug.Log("Ready to receive…");

                byte[] data = new byte[1024];
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                int recv = s_sock.ReceiveFrom(data, ref ep);
                //if (Globals.Instance != null)
                //{
                //    //Globals.Instance.AsyncInvokeMng.Events.Add(
                //    //    delegate { OnGetConf(((IPEndPoint)ep).Address.ToString(), ((IPEndPoint)ep).Port, GameConf.Unpack(data)); });
                //    Globals.Instance.AsyncInvokeMng.Events.Add(delegate 
                //    {
                //        Globals.Instance.SendMessage(MsgType.OnFindServer, 
                //            ((IPEndPoint)ep).Address.ToString(), 
                //            ((IPEndPoint)ep).Port, GameConf.Unpack(data));
                //    });
                //}
                CmdBroadcastRecv.Handle(data);

                //Debug.LogFormat("received: {0}bytes from: {1}:{2}", data.Length, ((IPEndPoint)ep).Address, ((IPEndPoint)ep).Port);
                Thread.Sleep(2000);
            }
        }

        public static void Stop()
        {
            if (!s_hasStarted)
            {
                return;
            }

            s_thread.Abort();
            s_sock.Close();
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