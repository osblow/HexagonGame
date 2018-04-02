using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Osblow.HexProto;

namespace Osblow.Net.Client
{
    public class CmdRequest
    {
        static ProtoSerializer s_serializer;
        

        public static void Login(string name)
        {
            LoginRequest request = new LoginRequest();
            request.name = name;
            request.platform = Application.platform.ToString();

            SerializeAndSend(Cmd.LoginRequest, request);
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
            
            Globals.Instance.GameClient.Send(data.ToArray());
        }


        static CmdRequest()
        {
            s_serializer = new ProtoSerializer();
        }
    }
}
