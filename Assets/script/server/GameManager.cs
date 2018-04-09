using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    class GameManager : ObjectBase
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
                uiGame.ShowWheel(false);
                uiGame.ShowWheelLater(true, 0.5f);

                NextPlayer();
                return;
            }

            // 检查是否有选定的颜色,如果已经没有相应的颜色，则再次随机
            if (OpType != OpType.Both && !HexManager.HasColor(OpType))
            {
                uiGame.ShowWheel(false);
                uiGame.ShowWheelLater(true, 0.5f);
                UIManager.SendMessage(MsgType.OnShowTips, "颜色无效，再转一回吧");

                return;
            }

            GameStep = GameStep.Hitting;
            uiGame.ShowWheel(false);
        }

        private void NextPlayer(bool isGood = false)
        {
            ++CurPlayer;
            if (CurPlayer > GameConf.MemCount)
            {
                CurPlayer = 1;
            }

            UIGame uiGame = UIManager.GetUIByType<UIGame>();
            uiGame.SetCurPlayer(CurPlayer);

            string goodOrNot = isGood ? "GOOD!!!\n" : "";
            UIManager.SendMessage(MsgType.OnShowTips, goodOrNot + "下回合: 玩家" + CurPlayer);
        }



        public bool IsOnline = false;

        private GameHexagon m_mainHex;
        public GameHexagon MainHex { get { return m_mainHex; } }

        private GameHexagon m_curTargetHex;
        private GameHexagon m_singleTargetHex; // 强制模式下，每轮必须砸掉一个且是唯一一个，这里保存第一次砸中的格子
        private const float c_strenghtSensitivity = 1.0f;

        public override void Update(float delta)
        {
            base.Update(delta);

            HandleHitting();
        }

        
        public override void Destroy()
        {
            base.Destroy();
            // temp
            BroadCastServer.Stop();
            BroadCastReciever.Stop();
        }

        private void HandleHitting()
        {
            if (GameStep == GameStep.SelectingMain)
            {
                HandleMain();
            }
            else if (GameStep == GameStep.Hitting)
            {
                HandleTarget();
            }
        }

        private void HandleMain()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Hexagon theHex = GetHittingHex();
                if (theHex != null)
                {
                    m_mainHex = theHex as GameHexagon;
                    m_mainHex.OnBecomeMain();
                    GameStep = GameStep.Start;

                    UIManager.GetUIByType<UIGame>().ShowWheelLater(true, 1);
                    GameStep = GameStep.RandomOperation;
                }
            }
        }

        private void HandleTarget()
        {
            // 按下时选择一一个格子，并开始模拟力度
            if (Input.GetMouseButtonDown(0))
            {
                if (m_curTargetHex != null)
                {
                    return;
                }

                GameHexagon theHex = GetHittingHex();

                // 在强制模式下，必须砸同一个格子
                if (GameConf.ForceKill && m_singleTargetHex != null && m_singleTargetHex != theHex)
                {
                    UIManager.SendMessage(MsgType.OnShowTips, "必须砸同一个格子！");
                    return;
                }

                if (theHex != null && theHex != m_mainHex && CheckColor(theHex, OpType))
                {
                    m_curTargetHex = theHex;
                    m_singleTargetHex = theHex;
                    UIManager.GetUIByType<UIGame>().StartSliderMoving();

                    Hammer.transform.position = m_curTargetHex.Obj.transform.position + Vector3.up;
                }
            }

            // 抬起时加判断，如果是按在同一个格子上，则为砸下，否则为取消
            if (Input.GetMouseButtonUp(0))
            {
                if (m_curTargetHex == null)
                {
                    return;
                }

                float strength = UIManager.GetUIByType<UIGame>().StopSliderMoving() * c_strenghtSensitivity;
                if (m_curTargetHex == GetHittingHex())
                {
                    //if(strength > 0.95f)
                    //{
                    //    GameView.ShowTips("GOOD !!!");
                    //}

                    // 锤子
                    Hammer.Knock();

                    CoroutineManager.StopAllCoroutines();
                    CoroutineManager.StartCoroutine(UpdateAllHexesLater(strength));
                }
                else
                {
                    m_curTargetHex = null;
                    m_singleTargetHex = null;
                }

                UIManager.GetUIByType<UIGame>().ResetSlider();
                //HexManager.OnSelectTarget(null);
            }
        }

        IEnumerator UpdateAllHexesLater(float lastStrength)
        {
            yield return new WaitForSeconds(0.1f);

            m_curTargetHex.OnHit(lastStrength);
            HexManager.UpdateAllBalance();

            // 如果是强制砸落，则必须当前块掉落再进行下一轮
            if (!(m_curTargetHex.IsActive() && GameConf.ForceKill))
            {
                //
                if (GameStep != GameStep.GameOver)
                {
                    GameStep = GameStep.RandomOperation;
                    UIManager.GetUIByType<UIGame>().ShowWheelLater(true, 1f);

                    NextPlayer(lastStrength > 0.95f);
                }
                m_singleTargetHex = null;
            }

            m_curTargetHex = null;
        }

        private static bool CheckColor(GameHexagon hex, OpType op)
        {
            if (op == OpType.Pass)
            {
                Globals.Instance.UIManager.SendMessage(MsgType.OnShowTips, "此次跳过");
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
                    Globals.Instance.UIManager.SendMessage(MsgType.OnShowTips,
                        string.Format("<color=yellow>必须砸{0}颜色块！</color>", UIConstants.s_ops[op]));
                }

                return hex.OpType == op;
            }
        }

        private GameHexagon GetHittingHex()
        {
            GameHexagon result = null;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("GameHex")))
            {
                // Hexagon对象
                result = hit.transform.GetComponent<CustomCollider>().ParentHexagon as GameHexagon;
            }

            return result;
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
