using UnityEngine;
using Osblow.HexProto;
using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    class CmdRequest
    {
        static ProtoSerializer s_serializer;

        private static void LoginRequest(Player player, object dataObj)
        {
            Debug.Log("login request");
            LoginRequest request = dataObj as LoginRequest;
            //Globals.Instance.SendMessage(MsgType.OnPlayerEnter, request.name, request.platform);
            //CmdResponse.LoginResponse(player, Globals.Instance.GameServer.GameManager.Owner.GUID, new List<Member>(), null);
            Globals.Instance.GameServer.GameManager.OnPlayerLogin(player);
        }


        public static void Handle(Player player, byte[] dataBytes)
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


                Execute(player, cmd, dataBytes, index, dataLen);

                index += (dataLen + 1);
                if (index >= dataBytes.Length)
                {
                    break;
                }
            }
        }

        static void Execute(Player player, short cmd, byte[] data, int index, int length)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data, index, length))
            {
                object receieved = new object();
                receieved = s_serializer.Deserialize(stream, null, s_handlers[cmd].ProtoType);
                // 延迟执行，与主线程同步
                Globals.Instance.AsyncInvokeMng.Events.Add(delegate
                {
                    s_handlers[cmd].Handler(player, receieved);
                });
            }
        }



        static Dictionary<short, ProtoHandle> s_handlers;


        static CmdRequest()
        {
            s_serializer = new ProtoSerializer();

            s_handlers = new Dictionary<short, ProtoHandle>()
            {
                { Cmd.LoginRequest, new ProtoHandle() { ProtoType=typeof(LoginRequest), Handler=LoginRequest } },
            };
        }

        struct ProtoHandle
        {
            public Type ProtoType;
            public Action<Player, object> Handler;
        }
    }
}
