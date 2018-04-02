using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;

namespace Osblow.Net.Server
{
    public class GameServer : ObjectBase
    {
        private Dictionary<int, Player> m_clients = new Dictionary<int, Player>();

        class ServerState
        {
            TcpListener Server;

            public ServerState(TcpListener server)
            {
                Server = server;
            }
        }

        public GameServer(string address, int port)
        {
            IPAddress localAddr = IPAddress.Parse(address);
            //TcpListener server = new TcpListener(localAddr, port);
            IPEndPoint iPEnd = new IPEndPoint(localAddr, port);
            Socket listener = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(iPEnd);
            listener.Listen(100);

            listener.BeginAccept(OnConnected, listener);
            //server.BeginAcceptSocket(OnConnected, new ServerState(server));
        }

        private void OnConnected(IAsyncResult ar)
        {
            Debug.Log("已连接");

            Socket server = (Socket)ar.AsyncState;
            Socket client_sock = server.EndAccept(ar);

            int guid = client_sock.GetHashCode();
            Player player = new Player(client_sock, guid);

            m_clients.Add(guid, player);

            try
            {
                player.Receive();
                //MsgMng.Dispatch(MsgType.Connected);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally { }
        }
        
        public void CloseAll()
        {
            foreach (Player player in m_clients.Values)
            {
                player.Close();
            }

            m_clients.Clear();
        }

        public void ForceClose()
        {
            foreach (Player player in m_clients.Values)
            {
                player.ForceClose();
            }

            m_clients.Clear();
        }
    }
}
