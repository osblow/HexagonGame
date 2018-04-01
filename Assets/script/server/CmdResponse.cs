using System;
using System.Collections.Generic;
using Osblow.HexProto;

namespace Osblow.Net.Server
{
    public class CmdResponse
    {
        static Proto s_serializer;


        /// <summary>
        /// 广播房间信息以连接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="gameConf"></param>
        public static void SendRoomConf(string address, int port, GameConf gameConf)
        {
            RoomBroadCast roomBroadCast = new RoomBroadCast();
            roomBroadCast.address = address;
            roomBroadCast.port = port;
            RoomBroadCast.RoomConf conf = new RoomBroadCast.RoomConf();
            conf.name = gameConf.Name;
            conf.maxMemCount = gameConf.MemCount;
            conf.mapType = (RoomBroadCast.RoomConf.MapType)gameConf.MapType;
            conf.forceKill = gameConf.ForceKill;
            roomBroadCast.roomConf = conf;

            SerializeAndSend(Cmd.BroadcastRoomConf, roomBroadCast);
        }

        static void SerializeAndSend(short cmd, object dataObj)
        {
            byte[] data;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                s_serializer.Serialize(stream, dataObj);
                data = stream.ToArray();
            }

            Send(cmd, data);
        }

        static void Send(short cmd, byte[] sendData)
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

            //Osblow.App.Globals.SceneSingleton<SocketNetworkMng>().Send(data.ToArray());
        }


        static CmdResponse()
        {
            s_serializer = new Proto();
        }
    }
}
