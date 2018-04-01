using Osblow.HexProto;
using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    class CmdRequest
    {
        static Proto s_serializer;

        /// <summary>
        /// 广播房间信息以连接
        /// </summary>
        /// <param name="proto"></param>
        static void RecieveRoomConf(object proto)
        {
            RoomBroadCast roomData = proto as RoomBroadCast;

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


        static CmdRequest()
        {
            s_serializer = new Proto();

            s_handlers = new Dictionary<short, ProtoHandle>()
            {
                { Cmd.BroadcastRoomConf, new ProtoHandle() { ProtoType=typeof(RoomBroadCast), Handler=RecieveRoomConf } },
            };
        }

        struct ProtoHandle
        {
            public Type ProtoType;
            public Action<object> Handler;
        }
    }
}
