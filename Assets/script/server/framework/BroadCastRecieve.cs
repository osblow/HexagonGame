using System;using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

class BroadCastReciever
{
    public static void Start()
    {
        Socket sock = new Socket(AddressFamily.InterNetwork,
        SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint iep =
        new IPEndPoint(IPAddress.Any, 9050);
        sock.Bind(iep);
        EndPoint ep = (EndPoint)iep;
        Debug.Log("Ready to receive…");

        byte[] data = new byte[1024];
        int recv = sock.ReceiveFrom(data, ref ep);
        string stringData = Encoding.ASCII.GetString(data, 0, recv);

        Debug.LogFormat("received: {0} from: {1}", stringData, ep.ToString());
        sock.Close();
    }
}