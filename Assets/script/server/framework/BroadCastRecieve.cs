using System;using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Net.NetworkInformation;

class BroadCastReciever
{
    static Socket sock;

    static int cur_tryPortIndex = 1;

    public static void Start(int port=9050)
    {
        try
        {
            sock = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep =
            new IPEndPoint(IPAddress.Any, port);
            sock.Bind(iep);
            Debug.Log("Ready to receive…");

            Thread t = new Thread(Recieve);
            t.Start();
            //sock.Close();
        }
        catch (SocketException)
        {
            Debug.Log("端口已占用");
            if(cur_tryPortIndex < BroadConstant.BroadcastPorts.Length)
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
            byte[] data = new byte[1024];
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            int recv = sock.ReceiveFrom(data, ref ep);
            string stringData = Encoding.ASCII.GetString(data, 0, recv);

            Debug.LogFormat("received: {0} from: {1}:{2}", stringData, ((IPEndPoint)ep).Address, ((IPEndPoint)ep).Port);
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