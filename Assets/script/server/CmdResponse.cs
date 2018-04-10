using System;
using System.Collections.Generic;
using Osblow.HexProto;

namespace Osblow.Net.Server
{
    public class CmdResponse
    {
        static ProtoSerializer s_serializer;


        public static void LoginResponse(Player player, int ownerGuid, List<Member> members, RoomConf conf)
        {
            LoginResponse response = new LoginResponse();
            response.guid = player.GUID;
            response.members.AddRange(members);
            response.roomConf = conf;

            SerializeAndSend(player, Cmd.LoginResponse, response);
        }

        public static void CurrentPlayer(Player player, int guid)
        {

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

        }

        public static void HammerKnock(Player player, float strength)
        {

        }

        static void SerializeAndSend(Player player, short cmd, object dataObj)
        {
            byte[] data;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                s_serializer.Serialize(stream, dataObj);
                data = stream.ToArray();
            }

            Send(player, cmd, data);
        }

        static void Send(Player player, short cmd, byte[] sendData)
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
            
            player.Send(data.ToArray());
        }


        static CmdResponse()
        {
            s_serializer = new ProtoSerializer();
        }
    }
}
