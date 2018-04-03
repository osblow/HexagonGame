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

        private Socket m_listener;


        public GameServer(string address, int port)
        {
            IPAddress localAddr = IPAddress.Parse(address);
            IPEndPoint iPEnd = new IPEndPoint(localAddr, port);
            m_listener = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_listener.Bind(iPEnd);
            m_listener.Listen(100);

            Accept();
        }

        private void Accept()
        {
            m_listener.BeginAccept(OnConnected, m_listener);
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
            finally
            {
                Accept();
            }
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
