using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    public class GameManager : ObjectBase
    {
        public Dictionary<int, Player> m_players;

        public HexManager HexManager;

        public GameStep GameStep;
        public OpType OpType = OpType.Pass;
        public int CurPlayer = 1;

        public GameConf GameConf;


        public override void Start()
        {
            base.Start();

            m_players = new Dictionary<int, Player>();
            HexManager = AddChild(new HexManager()) as HexManager;

            GameStep = GameStep.NotStart;
        }

        public void AddPlayer(int guid, Player player)
        {
            if (m_players.ContainsKey(guid))
            {
                m_players[guid].Close();
                m_players[guid].Destroy();
            }

            m_players[guid] = player;
        }


        public void ToGame()
        {
            CurPlayer = 1;
            
            GameStep = GameStep.SelectingMain;
        }

        public void EndGame()
        {
            HexManager.ClearAll();
        }

        public void Restart()
        {
            m_curTargetHex = null;
            m_singleTargetHex = null;
            m_mainHex = null;
            HexManager.Restart();
            CurPlayer = 1;
            

            GameStep = GameStep.SelectingMain;
        }

        public void OnGameOver()
        {
            GameStep = GameStep.GameOver;
        }

        public void OnStartHitting()
        {
            if (OpType == OpType.Pass)
            {
                // 此轮跳过，延迟1秒到下一位

                CmdResponse.RandomOperation(null, OpType);

                // todo: 延迟
                NextPlayer();
                return;
            }

            // 检查是否有选定的颜色,如果已经没有相应的颜色，则再次随机
            if (OpType != OpType.Both && !HexManager.HasColor(OpType))
            {
                // 重新随机

                return;
            }

            GameStep = GameStep.Hitting;
            CmdResponse.StartHitting(null, OpType);
        }

        private void NextPlayer(bool isGood = false)
        {
            ++CurPlayer;
            if (CurPlayer > GameConf.MemCount)
            {
                CurPlayer = 1;
            }

            CmdResponse.CurrentPlayer(null, 0);

            string goodOrNot = isGood ? "GOOD!!!\n" : "";
            CmdResponse.SetScore(null, 0, 0);
        }



        public bool IsOnline = false;

        private GameHexagon m_mainHex;
        public GameHexagon MainHex { get { return m_mainHex; } }

        private GameHexagon m_curTargetHex;
        private GameHexagon m_singleTargetHex; // 强制模式下，每轮必须砸掉一个且是唯一一个，这里保存第一次砸中的格子
        private const float c_strenghtSensitivity = 1.0f;
        
        public override void Destroy()
        {
            base.Destroy();
            // temp
            BroadCastServer.Stop();
            BroadCastReciever.Stop();
        }
        

        public void SetMain(int x, int y)
        {
            m_mainHex = HexManager.GetHexagon(x, y) as GameHexagon;
            m_mainHex.IsMain = true;
            GameStep = GameStep.Start;

            // 开始随机
            // GameStep = GameStep.RandomOperation;
        }


        private bool m_isHitting = false;
        
        public void StartHammer(int x, int y)
        {
            if (m_curTargetHex != null)
            {
                return;
            }

            GameHexagon theHex = HexManager.GetHexagon(x, y) as GameHexagon;

            // 在强制模式下，必须砸同一个格子
            if (GameConf.ForceKill && m_singleTargetHex != null && m_singleTargetHex != theHex)
            {
                CmdResponse.Alert(null, "必须砸同一个格子！");
                return;
            }

            if (theHex != null && theHex != m_mainHex && CheckColor(theHex, OpType))
            {
                m_curTargetHex = theHex;
                m_singleTargetHex = theHex;

                // 重置力度
            }
        }

        private void OnHammerUpdate()
        {
            if (!m_isHitting)
            {
                return;
            }

            // 模拟力度变化 
        }

        public void EndHammer()
        {
            if (m_curTargetHex == null)
            {
                return;
            }

            float strength = 0;// UIManager.GetUIByType<UIGame>().StopSliderMoving() * c_strenghtSensitivity;
            //if (m_curTargetHex == GetHittingHex())
            {
                //if(strength > 0.95f)
                //{
                //    GameView.ShowTips("GOOD !!!");
                //}

                // 锤子
                CmdResponse.HammerKnock(null, strength);
            }
            //else
            //{
            //    m_curTargetHex = null;
            //    m_singleTargetHex = null;
            //}

            // 重置力度

            // 下一轮
            m_curTargetHex.OnHit(strength);
            HexManager.UpdateAllBalance();

            // 如果是强制砸落，则必须当前块掉落再进行下一轮
            if (!(m_curTargetHex.IsActive() && GameConf.ForceKill))
            {
                //
                if (GameStep != GameStep.GameOver)
                {
                    GameStep = GameStep.RandomOperation;

                    CmdResponse.CurrentPlayer(null, 0);
                }
                m_singleTargetHex = null;
            }

            m_curTargetHex = null;
        }
        
        private static bool CheckColor(GameHexagon hex, OpType op)
        {
            if (op == OpType.Pass)
            {
                return false;
            }
            else if (op == OpType.Both)
            {
                return true;
            }
            else
            {
                if (hex.OpType != op)
                {
                    CmdResponse.Alert(null, string.Format("<color=yellow>必须砸{0}颜色块！</color>", UIConstants.s_ops[op]));
                }

                return hex.OpType == op;
            }
        }
        

        public void CloseAll()
        {
            foreach(Player player in m_players.Values)
            {
                player.Close();
            }
            m_players.Clear();
        }

        public void ForceCloseAll()
        {
            foreach(Player player in m_players.Values)
            {
                player.ForceClose();
            }
            m_players.Clear();
        }
    }
}
