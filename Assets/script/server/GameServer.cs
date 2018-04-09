using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;

namespace Osblow.Net.Server
{
    public class GameServer : ObjectBase
    {
        private GameManager m_gameManager;

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
            m_gameManager = AddChild(new GameManager()) as GameManager;

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
            
            m_gameManager.AddPlayer(guid, player);

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
            m_gameManager.CloseAll();
        }

        public void ForceClose()
        {
            m_gameManager.ForceCloseAll();
        }
    }
}
