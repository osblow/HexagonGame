﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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


public class GameConf
{
    public string Name = "";

    public MapType MapType = MapType.Small;
    public int MemCount = 2;
    public bool ForceKill = true;


    public static byte[] Pack(GameConf conf)
    {
        // namelen,name,map,memcount,force

        List<byte> results = new List<byte>();
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(conf.Name);

        results.AddRange(System.BitConverter.GetBytes(nameBytes.Length));
        results.AddRange(nameBytes);

        results.Add((byte)conf.MapType);
        results.Add((byte)conf.MemCount);
        results.Add((byte)(conf.ForceKill ? 1 : 0));

        return results.ToArray();
    }

    public static GameConf Unpack(byte[] data)
    {
        if (data == null || data.Length < 3)
        {
            return null;
        }

        GameConf conf = new GameConf();

        int nameLen = System.BitConverter.ToInt32(data, 0);
        conf.Name = System.Text.Encoding.UTF8.GetString(data, 4, nameLen);

        conf.MapType = (MapType)data[4 + nameLen];
        conf.MemCount = data[4 + nameLen + 1];
        conf.ForceKill = data[4 + nameLen + 2] == 1;

        return conf;
    }
}

namespace Osblow
{
    public class Globals : ObjectBase
    {
        private static Globals s_instance;
        public static Globals Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new Globals();
                }
                return s_instance;
            }
        }

        private Globals() { }

        
        private GameStep m_gameStep;
        public GameStep GameStep
        {
            set
            {
                m_gameStep = value;
                Broadcast(MsgType.OnShowTips);
            }
            get
            {
                return m_gameStep;
            }
        }
        

        public OpType OpType = OpType.Pass;
        public int CurPlayer = 1;

        public GameConf GameConf;


        public PrefabPool PrefabPool;
        public UIManager UIManager;
        public CoroutineManager CoroutineManager;
        public AsyncInvokeMng AsyncInvokeMng;
        public HexManager HexManager;

        public Hammer Hammer;


        public override void Start()
        {
            PrefabPool = new PrefabPool();
            UIManager = AddChild(new UIManager()) as UIManager;
            CoroutineManager = new GameObject("Coroutine Manager").AddComponent<CoroutineManager>(); // temp
            AsyncInvokeMng = AddChild(new AsyncInvokeMng()) as AsyncInvokeMng;

            GameStep = GameStep.NotStart;
            UIManager.CreateUI<UIMain>();
        }

        public void ToGame()
        {
            CurPlayer = 1;

            Hammer = GameObject.Instantiate(PrefabPool.GetPrefab("model/hammer"), Vector3.one * 100, Quaternion.identity)
                .GetComponent<Hammer>();
            Hammer.gameObject.SetActive(true);

            UIGame uiGame = UIManager.CreateUI<UIGame>();
            uiGame.SetRoomConfig();
            uiGame.SetCurPlayer(CurPlayer);

            // camera
            if (GameConf.MapType == MapType.Small)
            {
                Camera.main.transform.position = new Vector3(1.57f, 9.07f, -5.82f);
            }
            else if (GameConf.MapType == MapType.Middle)
            {
                Camera.main.transform.position = new Vector3(2.18f, 9.07f, -7.01f);
            }
            else if (GameConf.MapType == MapType.Big)
            {
                Camera.main.transform.position = new Vector3(2.58f, 9.07f, -8.2f);
            }

            HexManager = AddChild(new HexManager()) as HexManager;

            

            GameStep = GameStep.SelectingMain;
        }

        public void EndGame()
        {
            UIManager.RemoveUIByType<UIGame>();
            HexManager.ClearAll();
            GameObject.Destroy(Hammer.gameObject);

            UIManager.CreateUI<UICreateRoom>();
        }

        public void Restart()
        {
            m_curTargetHex = null;
            m_singleTargetHex = null;
            m_mainHex = null;
            HexManager.Restart();
            CurPlayer = 1;

            Hammer.transform.position = Vector3.one * 100;
            UIGame gameUI = UIManager.GetUIByType<UIGame>();
            gameUI.ShowWheel(false);
            gameUI.SetCurPlayer(CurPlayer);

            GameStep = GameStep.SelectingMain;
        }

        public void OnGameOver()
        {
            GameStep = GameStep.GameOver;
        }

        public void OnStartHitting()
        {
            UIGame uiGame = UIManager.GetUIByType<UIGame>();

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

            // EXIT
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }


        public void LateUpdate(float delta)
        {
            AsyncInvokeMng.LateUpdate();
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
    }
}
