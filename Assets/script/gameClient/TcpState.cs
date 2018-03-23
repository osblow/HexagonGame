using UnityEngine;
using System.Collections;
using System.Net.Sockets;

namespace Osblow.Net
{
    public class TCPState
    {
        public const int BuffSize = 65536;
        public byte[] Buffer = new byte[65536];

        private Socket socket = null;

        public Socket Socket
        {
            get { return socket; }
        }

        public TCPState(Socket socket)
        {
            this.socket = socket;
        }
    }
}
