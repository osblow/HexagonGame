using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Osblow.Net.Client
{
    public class GameClient : ObjectBase
    {
        private Socket m_socket;

        private NetBuffer m_buffer = new NetBuffer();


        public GameClient(string address, int port)
        {
            Connect(address, port);
        }

        void Connect(string address, int port)
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(address);
            IPEndPoint iep = new IPEndPoint(ip, port);

            Debug.Log("Connect to " + address + ":" + port);
            m_socket.BeginConnect(iep, new AsyncCallback(OnConnnect), m_socket);
        }

        void OnConnnect(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                Receive();

                Debug.Log("已连接");
                //MsgMng.Dispatch(MsgType.Connected);
                Globals.Instance.AsyncInvokeMng.Events.Add(delegate 
                {
                    Broadcast(MsgType.OnConnectedServer);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {

            }
        }

        private bool CheckClientAvailable(int guid)
        {
            if (!m_socket.Connected)
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
            m_socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), m_socket);
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

        private void Receive()
        {
            try
            {
                // Create the state object.     
                TCPState state = new TCPState(m_socket);

                // Begin receiving the data from the remote device.     
                m_socket.BeginReceive(state.Buffer, 0, TCPState.BuffSize, 0, new AsyncCallback(ReceiveCallback), state);
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
                    if (m_buffer.TargetLength < 0)
                    {
                        m_buffer.Init(realData);

                        if (m_buffer.CheckComplete())
                        {
                            CmdRequest.Handle(m_buffer.Buffer.ToArray());
                            m_buffer.Clear();
                        }
                    }
                    else
                    {
                        if (m_buffer.CheckComplete())
                        {
                            CmdRequest.Handle(m_buffer.Buffer.ToArray());

                            m_buffer.Clear();
                        }
                        else
                        {
                            m_buffer.Buffer.AddRange(realData);
                        }
                    }
                    Receive();

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
            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Close();
        }

        public void ForceClose()
        {
            m_socket.Close();
        }
    }
}
