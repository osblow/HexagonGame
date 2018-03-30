using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;


namespace Osblow.Net.Client
{
    public class SocketNetworkMng : MonoBehaviour
    {
        private Dictionary<short, Action<byte[], int>> m_hadlerDic = new Dictionary<short, Action<byte[], int>>();

        
        private MyClient m_client;

        private void Awake()
        {
            //m_client = new MyClient(Globals.Instance.Settings.SocketUrl, 
            //    Globals.Instance.Settings.SocketPort);

            StartCoroutine(AutoReconnect());


            
        }


        public void ForceClose()
        {
            m_client.ForceClose();
        }

        public void Handler(byte[] data)
        {
            if (data.Length < 6)
            {
                return;
            }
            

            int foo = 0;

            int index = 0;
            // 处理粘包
            while (true)
            {
                byte flag = data[index];
                if (flag != 0x64)
                {
                    return;
                }
                index += 1;
                ushort dataLen = BitConverter.ToUInt16(data, index + 2);

                if (foo >= 1)
                {
                    int a = 0;
                }
                
                //Globals.SceneSingleton<AsyncInvokeMng>().EventsToAct += delegate ()
                //{
                //    Debug.LogFormat("socket: {0},,,,{1},,,{2}", data.Length, index + 4 + dataLen + 1, dataLen);
                //};

                if(index + 4 + dataLen + 1 > data.Length)
                {
                    break;
                }

                

                short cmd = BitConverter.ToInt16(data, index);
                index += 2;
                Execute(cmd, data, index);

                index += (2 + dataLen + 1);
                if(index >= data.Length)
                {
                    break;
                }

                foo += 1;
            }

        }



        public void Execute(short cmd, byte[] data, int index)
        {
            if (!m_hadlerDic.ContainsKey(cmd))
            {
                return;
            }

            m_hadlerDic[cmd].Invoke(data, index);
        }



        //public Queue<byte[]> MessageQueue = new Queue<byte[]>();
        public void Send(byte[] data)
        {
            //MessageQueue.Enqueue(data);

            //Globals.SceneSingleton<AsyncInvokeMng>().Events.Add(delegate ()
            //{
            //    StartCoroutine(AutoSend(data));
            //});

            m_client.Send(data);
        }

        //IEnumerator AutoSend(byte[] data)
        //{
        //    while(MessageQueue.Count > 0)
        //    {
        //        m_client.Send(MessageQueue.Peek());

        //        yield return new WaitForSeconds(3.5f);
        //    }
        //}

        private void OnDestroy()
        {
            if (!m_client.Connected)
            {
                return;
            }

            m_client.Close();
        }
        
        private IEnumerator AutoReconnect()
        {
            while (true)
            {
                yield return new WaitForSeconds(5);

                //if (!Globals.SceneSingleton<GameMng>().IsGaming)
                //{
                //    m_client.ForceClose();
                //    //Destroy(gameObject);
                //    yield break;
                //}

                //if (!m_client.Connected)
                //{
                //    // 如果10次重连没有连接上，则退出到登录界面，将所有状态清空
                //    if(Globals.Instance.ReconnectTime < 0)
                //    {
                //        Globals.SceneSingleton<UIManager>().DestroySingleUI(UIType.TableView);
                //        Globals.SceneSingleton<ContextManager>().Push(new LoginUIContext());

                //        Globals.SceneSingleton<DataMng>().ClearAll();
                //        Globals.SceneSingleton<GameMng>().ClearAll();
                //        //Globals.SceneSingleton<SoundMng>().StopBackSound();
                //        Globals.RemoveSceneSingleton<Osblow.Game.SocketNetworkMng>();

                //        Globals.Instance.SaveLog();

                //        //Destroy(gameObject);
                //        yield break;
                //    }


                //    Debug.Log("正在重新连接....");
                //    Globals.SceneSingleton<ContextManager>().WebBlockUI(true, "正在重新连接...");
                //    m_client.ForceClose();
                //    m_client = new MyClient(Globals.Instance.Settings.SocketUrl,
                //Globals.Instance.Settings.SocketPort);

                //    --Globals.Instance.ReconnectTime;
                //}
                //else
                //{
                //    //Debug.Log(Time.time);
                //    CmdRequest.ClientHeartBeatRequest();
                //}
            }
        }
    }




    class MyClient
    {
        public bool Connected { get { return m_socket != null && m_socket.Connected; } }

        private Socket m_socket;

        private NetBuffer m_buffer = new NetBuffer();


        public MyClient(string addr, int port)
        {
            Connect(addr, port);
        }

        void Connect(string addr, int port)
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(addr);
            IPEndPoint iep = new IPEndPoint(ip, port);
            Debug.Log(port);
            m_socket.BeginConnect(iep, new AsyncCallback(Connect), m_socket);
        }

        void Connect(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                Receive();

                Debug.Log("已连接");
                //MsgMng.Dispatch(MsgType.Connected);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {

            }
        }

        public void Send(byte[] data)
        {
            if (!m_socket.Connected)
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

                if(client == null || !client.Connected)// ||
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
                            CmdHandler.Handle(m_buffer.Buffer.ToArray());
                            m_buffer.Clear();
                        }
                    }
                    else
                    {
                        if (m_buffer.CheckComplete())
                        {
                            CmdHandler.Handle(m_buffer.Buffer.ToArray());

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
