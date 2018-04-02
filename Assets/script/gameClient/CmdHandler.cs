using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Osblow.HexProto;

namespace Osblow.Net.Client
{
    public class CmdHandler
    {
        static ProtoSerializer s_serializer;

        
        private static void LoginResponse(object dataObj)
        {
            Debug.Log("login success");

        }


        public static void Handle(byte[] dataBytes)
        {
            if (dataBytes.Length < 6)
            {
                return;
            }

            int index = 0;
            // 处理粘包
            while (true)
            {
                byte flag = dataBytes[index];
                if (flag != 0x64)
                {
                    return;
                }
                index += 1;

                short cmd = BitConverter.ToInt16(dataBytes, index);
                index += 2;

                int dataLen = BitConverter.ToInt32(dataBytes, index);
                index += 4;

                if (index + dataLen + 1 > dataBytes.Length)
                {
                    break;
                }


                Execute(cmd, dataBytes, index, dataLen);

                index += (dataLen + 1);
                if (index >= dataBytes.Length)
                {
                    break;
                }
            }
        }

        static void Execute(short cmd, byte[] data, int index, int length)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data, index, length))
            {
                object receieved = new object();
                receieved = s_serializer.Deserialize(stream, null, s_handlers[cmd].ProtoType);
                // 延迟执行，与主线程同步
                Globals.Instance.AsyncInvokeMng.Events.Add(delegate
                {
                    s_handlers[cmd].Handler(receieved);
                });
            }
        }



        static Dictionary<short, ProtoHandle> s_handlers;


        static CmdHandler()
        {
            s_serializer = new ProtoSerializer();

            s_handlers = new Dictionary<short, ProtoHandle>()
            {
                { Cmd.LoginResponse, new ProtoHandle() { ProtoType=typeof(LoginResponse), Handler=LoginResponse } },
            };
        }

        struct ProtoHandle
        {
            public Type ProtoType;
            public Action<object> Handler;
        }
    }
}
