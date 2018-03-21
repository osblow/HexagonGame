using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class BroadCast
{
    private static Socket sock;
    private static IPEndPoint[] ieps;
    private static byte[] data;
    public static void Start()
    {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
        ProtocolType.Udp);
        //255.255.255.255
        ieps = new IPEndPoint[BroadConstant.BroadcastPorts.Length];
        for (int i = 0; i < ieps.Length; i++)
        {
            ieps[i] = new IPEndPoint(IPAddress.Broadcast, BroadConstant.BroadcastPorts[i]);
        }
       
        string hostname = Dns.GetHostName();
        data = Encoding.ASCII.GetBytes(hostname);

        sock.SetSocketOption(SocketOptionLevel.Socket,
        SocketOptionName.Broadcast, 1);

        Thread t = new Thread(BroadcastMessage);
        t.Start();
        //sock.Close();
    }

    private static void BroadcastMessage()
    {
        while (true)
        {
            for (int i = 0; i < ieps.Length; i++)
            {
                sock.SendTo(data, ieps[i]);
            }
            Thread.Sleep(2000);
        }

    }

}
