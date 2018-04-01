﻿using System;

namespace Osblow
{
	public enum MsgType
	{
		None = 0,

        OnShowTips = 1000, // 游戏进度发生变化

        OnFindServer = 2000, // 发现服务器
        OnPlayerEnter = 2001, // 有玩家加入

        OnConnectedServer = 2100, // 连接上服务器
	}
}

