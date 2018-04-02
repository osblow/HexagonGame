using System;
using System.Collections.Generic;
using Osblow.HexProto;

namespace Osblow.Net.Server
{
    public class CmdBroadcast
    {
        static ProtoSerializer s_serializer;


        /// <summary>
        /// 广播房间信息以连接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="gameConf"></param>
        public static byte[] SendRoomConf(string address, int port, GameConf gameConf)
        {
            RoomBroadCast roomBroadCast = new RoomBroadCast();
            roomBroadCast.address = address;
            roomBroadCast.port = port;
            RoomConf conf = new RoomConf();
            conf.name = gameConf.Name;
            conf.maxMemCount = gameConf.MemCount;
            conf.mapType = (int)gameConf.MapType;
            conf.forceKill = gameConf.ForceKill;
            roomBroadCast.roomConf = conf;

            return Serialize(Cmd.BroadcastRoomConf, roomBroadCast);
        }

        static byte[] Serialize(short cmd, object dataObj)
        {
            byte[] data;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                s_serializer.Serialize(stream, dataObj);
                data = stream.ToArray();
            }

            return Pack(cmd, data);
        }

        static byte[] Pack(short cmd, byte[] sendData)
        {
            List<byte> data = new List<byte>();

            byte head = 0x64;
            int length = sendData.Length;
            byte tail = 0x65;

            data.Add(head);
            data.AddRange(BitConverter.GetBytes(cmd));
            data.AddRange(BitConverter.GetBytes(length));
            data.AddRange(sendData);
            data.Add(tail);

            return data.ToArray();
        }


        static CmdBroadcast()
        {
            s_serializer = new ProtoSerializer();
        }
    }
}
