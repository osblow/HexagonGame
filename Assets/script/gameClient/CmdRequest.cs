using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace Osblow.Game
{
    public class CmdRequest
    {
        static void Serialize(short cmd, byte[] sendData)
        {
            List<byte> data = new List<byte>();

            byte head = 0x64;
            short lengths = (short)sendData.Length;
            ushort length = (ushort)sendData.Length;
            byte tail = 0x64;

            data.Add(head);
            data.AddRange(BitConverter.GetBytes(cmd));
            data.AddRange(BitConverter.GetBytes(length));
            data.AddRange(sendData);
            data.Add(tail);

            //Osblow.App.Globals.SceneSingleton<SocketNetworkMng>().Send(data.ToArray());
        }
    }
}
