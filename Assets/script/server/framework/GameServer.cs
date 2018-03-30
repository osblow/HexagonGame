using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;

namespace Osblow.Net.Server
{
    public class GameServer : ObjectBase
    {
        private Dictionary<int, Socket> m_clients = new Dictionary<int, Socket>();


        public GameServer(string address, int port)
        {
            IPAddress localAddr = IPAddress.Parse(address);
            TcpListener server = new TcpListener(localAddr, port);

            server.BeginAcceptSocket(OnConnected, server);
        }

        private void OnConnected(IAsyncResult ar)
        {
            Debug.Log("已连接");

            TcpListener server = (TcpListener)ar.AsyncState;
            Socket client_sock = server.EndAcceptSocket(ar);

            int guid = client_sock.GetHashCode();
            m_clients.Add(guid, client_sock);

            try
            {
                Receive(guid);
                //MsgMng.Dispatch(MsgType.Connected);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally { }
        }

        private bool CheckClientAvailable(int guid)
        {
            if (!m_clients.ContainsKey(guid))
            {
                return false;
            }

            if (!m_clients[guid].Connected)
            {
                return false;
            }
            
            return true;
        }

        public void Send(int guid, byte[] data)
        {
            if (!CheckClientAvailable(guid))
            {
                return;
            }

            // Begin sending the data to the remote device.     
            m_clients[guid].BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), m_clients[guid]);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = handler.EndSend(ar);

                //if (Globals.SceneSingleton<SocketNetworkMng>().MessageQueue.Count > 0)
                //{
                //    Globals.SceneSingleton<SocketNetworkMng>().MessageQueue.Dequeue();
                //    //Debug.Log("已发送..消息队列还剩" +
                //    //    Globals.SceneSingleton<SocketNetworkMng>().MessageQueue.Count + "条");
                //}
                //Debug.LogFormat("Sent {0} bytes to server.", bytesSent);
                //NetworkMng.Instance.DebugStr = string.Format("Sent {0} bytes to server.", bytesSent);
            }
            catch (SocketException e)
            {
                Debug.Log(e.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void Receive(int guid)
        {
            try
            {
                // Create the state object.     
                TCPState state = new TCPState(m_clients[guid]);

                // Begin receiving the data from the remote device.     
                m_clients[guid].BeginReceive(state.Buffer, 0, TCPState.BuffSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }


        //MyBuffer m_buffer = new MyBuffer();
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                TCPState state = (TCPState)ar.AsyncState;
                Socket client = state.Socket;

                if (client == null || !client.Connected)// ||
                                                        //!Globals.SceneSingleton<GameMng>().IsGaming)
                {
                    Debug.Log("<color=red>不想接收 了</color>");
                    return;
                }

                // Read data from the remote device.     
                int bytesRead = client.EndReceive(ar);

                //Debug.Log("get from server, length = " + bytesRead);
                if (bytesRead > 0)
                {

                    byte[] realData = new byte[bytesRead];
                    Array.Copy(state.Buffer, realData, bytesRead);

                    //Globals.SceneSingleton<SocketNetworkMng>().Handler(realData);

                    // 拼接数据包
                    //if (m_buffer.TargetLength < 0)
                    //{
                    //    m_buffer.Init(realData);

                    //    if (m_buffer.CheckComplete())
                    //    {
                    //        Globals.SceneSingleton<SocketNetworkMng>().Handler(m_buffer.Buffer.ToArray());

                    //        m_buffer.Clear();
                    //    }
                    //}
                    //else
                    //{
                    //    if (m_buffer.CheckComplete())
                    //    {
                    //        Globals.SceneSingleton<SocketNetworkMng>().Handler(m_buffer.Buffer.ToArray());

                    //        m_buffer.Clear();
                    //    }
                    //    else
                    //    {
                    //        m_buffer.Buffer.AddRange(realData);
                    //    }
                    //}
                    Receive(client.GetHashCode());

                }
            }
            catch (SocketException e)
            {
                Debug.Log(e.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }



        public void Close()
        {
            foreach (Socket socket in m_clients.Values)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            m_clients.Clear();
        }

        public void ForceClose()
        {
            foreach (Socket socket in m_clients.Values)
            {
                socket.Close();
            }

            m_clients.Clear();
        }
    }
}
