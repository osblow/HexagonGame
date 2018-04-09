using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Osblow.Net.Server;
using Osblow.Net.Client;
using System.Net;


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
        public string MyName = "";

        public PrefabPool PrefabPool;
        public UIManager UIManager;
        public CoroutineManager CoroutineManager;
        public AsyncInvokeMng AsyncInvokeMng;
        public HexManager HexManager;

        // net
        public GameServer GameServer;
        public GameClient GameClient;

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


        public void StartServer()
        {
            GameServer = new GameServer(GetPrivateIP(), 4050);
        }

        public void ConnectGame(string address, int port, GameConf conf)
        {
            GameClient = new GameClient(address, port);
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










        //获取内网IP
        public static string GetPrivateIP()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = ipAddress.ToString();
                }
            }
            return AddressIP;
        }
    }
}
