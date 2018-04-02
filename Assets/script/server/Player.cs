using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;


namespace Osblow.Net.Server
{
    public class Player : ObjectBase
    {
        private Socket m_session;
        private int m_guid;
        public int GUID
        {
            get { return m_guid; }
        }

        private NetBuffer m_buffer = new NetBuffer();


        public Player(Socket session, int guid)
        {
            m_session = session;
            m_guid = guid;
        }

        public void Send(byte[] data)
        {
            if (!IsConnected())
            {
                return;
            }

            // Begin sending the data to the remote device.     
            m_session.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), m_session);
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

        public void Receive()
        {
            try
            {
                // Create the state object.     
                TCPState state = new TCPState(m_session);

                // Begin receiving the data from the remote device.     
                m_session.BeginReceive(state.Buffer, 0, TCPState.BuffSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket     
                // from the asynchronous state object.     
                TCPState state = (TCPState)ar.AsyncState;
                Socket client = state.Socket;

                if (!IsConnected())
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
                            CmdRequest.Handle(this, m_buffer.Buffer.ToArray());
                            m_buffer.Clear();
                        }
                    }
                    else
                    {
                        if (m_buffer.CheckComplete())
                        {
                            CmdRequest.Handle(this, m_buffer.Buffer.ToArray());

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
            m_session.Shutdown(SocketShutdown.Both);
            m_session.Close();
        }

        public void ForceClose()
        {
            m_session.Close();
        }


        private bool IsConnected()
        {
            return m_session.Connected;
        }
    }
}