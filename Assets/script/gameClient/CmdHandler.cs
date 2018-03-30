using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace Osblow.Net.Client
{
    public class CmdHandler
    {
        public static void Handle(byte[] data)
        {

        }

        public static T GetProtoInstance<T>(byte[] data, int index)
        {
            T t = default(T);
            ushort length = BitConverter.ToUInt16(data, index);
            index += 2;

            using (MemoryStream ms = new MemoryStream())
            {
                //将消息写入流中
                ms.Write(data, index, length);
                //将流的位置归0
                ms.Position = 0;
                //使用工具反序列化对象
            }

            return t;
        }


        public static T GetProtoInstanceChat<T>(byte[] data, int index)
        {
            T t = default(T);
            int length = BitConverter.ToInt32(data, index);
            index += 4;

            using (MemoryStream ms = new MemoryStream())
            {
                //将消息写入流中
                ms.Write(data, index, length);
                //将流的位置归0
                ms.Position = 0;
                //使用工具反序列化对象

            }

            return t;
        }
    }
}
