using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Osblow
{
    class UIConstants
    {
        public static Dictionary<OpType, string> s_ops = new Dictionary<OpType, string>
        {
            { OpType.Pass, "跳过" },
            { OpType.White, "白色" },
            { OpType.Green, "绿色" },
            { OpType.Both, "任意" },
        };

        public static Dictionary<OpType, float> s_probabilities = new Dictionary<OpType, float>
        {
            { OpType.Pass, 0.15f },
            { OpType.White, 0.5f },
            { OpType.Green, 0.85f },
            { OpType.Both, 1f },
        };

        public static Dictionary<GameStep, string> s_tips = new Dictionary<GameStep, string>
        {
            { GameStep.NotStart, "等待开始..." },
            { GameStep.SelectingMain, "选择主格子" },
            { GameStep.Start, "游戏开始" },
            { GameStep.RandomOperation, "转动轮盘选择操作" },
            { GameStep.Hitting, "在相应格子按住不放，盯准力度条，选择合适的时机放开。\n也可以把鼠标移到其它地方来取消" },
            { GameStep.GameOver, "游戏结束" },
        };

        public static Dictionary<MapType, string> s_mapName = new Dictionary<MapType, string>
        {
            { MapType.Small, "小" },
            { MapType.Middle, "中" },
            { MapType.Big, "大" },
        };
    }
}
