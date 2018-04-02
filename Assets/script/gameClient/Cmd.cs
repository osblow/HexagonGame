﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osblow.Net
{
    public class Cmd
    {
        public static short BroadcastRoomConf = 0x1001; // UDP广播房间信息

        public static short LoginRequest = 0x2001; // 玩家登录
        public static short LoginResponse = 0x2002; // 玩家登录返回
    }
}
