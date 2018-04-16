using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osblow.Net
{
    public class Cmd
    {
        public static short BroadcastRoomConf = 0x1001; // UDP广播房间信息

        public static short LoginRequest = 0x2001; // 玩家登录
        public static short LoginResponse = 0x2002; // 玩家登录返回

        public static short StartGame = 0x3001; // 游戏开始，房主开始选择目标格子
        public static short EndGame = 0x3002; // 游戏结束
        public static short CurrentPlayer = 0x3003; // 广播当前玩家
        public static short MainHex = 0x3004; // 主格子

        public static short Alert = 0x3101; // 弹窗提示
    }
}
