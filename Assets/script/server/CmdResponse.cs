using System;
using System.Collections.Generic;
using Osblow.HexProto;

namespace Osblow.Net.Server
{
    public class CmdResponse
    {
        static ProtoSerializer s_serializer;


        public static void LoginResponse(Player player, int ownerGuid, List<Player> players, GameConf conf)
        {
            LoginResponse response = new LoginResponse();
            response.guid = player.GUID;
            foreach(Player p in players)
            {
                Member m = new Member();
                m.guid = p.GUID;
                m.name = p.Name;
                response.members.Add(m);
            }

            RoomConf roomConf = new RoomConf();
            roomConf.name = conf.Name;
            roomConf.mapType = (int)conf.MapType;
            roomConf.maxMemCount = conf.MemCount;
            roomConf.forceKill = conf.ForceKill;
            response.roomConf = roomConf;
            response.ownerGuid = ownerGuid;

            SerializeAndSend(player, Cmd.LoginResponse, response);
        }

        public static void StartGame(Player player)
        {
            SerializeAndSend(player, Cmd.StartGame);
        }

        public static void EndGame(Player player)
        {
            SerializeAndSend(player, Cmd.EndGame);
        }

        public static void MainHex(Player player, int x, int y)
        {
            BroadcastMainHex proto = new BroadcastMainHex();
            proto.x = x;
            proto.y = y;
            SerializeAndSend(player, Cmd.MainHex, proto);
        }

        public static void CurrentPlayer(Player player, int guid)
        {
            SerializeAndSend(player, Cmd.CurrentPlayer, guid);
        }

        public static void RandomOperation(Player player, OpType opType)
        {

        }

        public static void StartHitting(Player player, OpType opType)
        {

        }

        public static void SetScore(Player player, int score, int quality)
        {

        }

        public static void Alert(Player player, string message)
        {
            SerializeAndSend(player, Cmd.Alert, message);
        }

        public static void HammerKnock(Player player, float strength)
        {

        }

        static void SerializeAndSend(Player player, short cmd, object dataObj=null)
        {
            byte[] data = null;

            if (dataObj != null)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    s_serializer.Serialize(stream, dataObj);
                    data = stream.ToArray();
                }
            }

            Send(player, cmd, data);
        }

        static void Send(Player player, short cmd, byte[] sendData)
        {
            List<byte> data = new List<byte>();

            byte head = 0x64;
            int length = sendData == null ? 0 : sendData.Length;
            byte tail = 0x65;

            data.Add(head);
            data.AddRange(BitConverter.GetBytes(cmd));
            data.AddRange(BitConverter.GetBytes(length));
            if(sendData != null) data.AddRange(sendData);
            data.Add(tail);
            
            player.Send(data.ToArray());
        }


        static CmdResponse()
        {
            s_serializer = new ProtoSerializer();
        }
    }
}
