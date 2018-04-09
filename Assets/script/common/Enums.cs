using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osblow
{
    public enum GameStep
    {
        NotStart = 0,             // 未开始
        SelectingMain = 1,        // 选择主格子
        Start = 2,                // 游戏开始
        RandomOperation = 3,      // 随机决定操作
        Hitting = 4,              // 砸
        GameOver = 6,             // 当局结束
    }

    public enum OpType
    {
        Pass = 0,
        White = 1,
        Green = 2,
        Both = 3,
    }

    public enum MapType
    {
        Small = 0,
        Middle = 1,
        Big = 2,
    }
}
