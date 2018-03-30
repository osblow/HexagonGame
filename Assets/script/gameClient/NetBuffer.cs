using System;
using System.Collections.Generic;

namespace Osblow.Net
{
    public class NetBuffer
    {
        public List<byte> Buffer = new List<byte>();
        public int TargetLength = -1;


        public void Init(byte[] data)
        {
            TargetLength = BitConverter.ToUInt16(data, 3);
            Buffer.AddRange(data);
        }

        public void Clear()
        {
            Buffer.Clear();
            TargetLength = -1;
        }

        /// <summary>
        /// 检查数据包完整性
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool CheckComplete()
        {
            //Debug.LogFormat("targetLength:{0}, standardSize:{1}, realSize:{2}", TargetLength, 5 + TargetLength + 1, Buffer.Count);
            if (TargetLength > 0 && 5 + TargetLength + 1 > Buffer.Count)
            {
                return false;
            }

            return true;
        }
    }
}
