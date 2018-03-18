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
    public MapType MapType = MapType.Small;
    public int MemCount = 2;
    public bool ForceKill = true;
}


public class Globals : MonoBehaviour
{
    public static Globals Instance { get { return s_instance; } }
    private static Globals s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    private GameStep m_gameStep;
    public GameStep GameStep
    {
        set
        {
            m_gameStep = value;
            if(GameView != null)
            {
                GameView.ShowTips();
            }
        }
        get
        {
            return m_gameStep;
        }
    }

    public OpType OpType = OpType.Pass;
    public int CurPlayer = 1;

    public GameConf GameConf;


    public Canvas Canvas;
    public HexManager HexManager;
    public Hammer Hammer;

    public MainView MainView;
    public GameView GameView;
    public OnlineView OnlineView;
    public CreateRoomView CreateRoomView;


    public bool IsOnline = false;

    private GameHexagon m_mainHex;
    public GameHexagon MainHex { get { return m_mainHex; } }

    private GameHexagon m_curTargetHex;
    private const float c_strenghtSensitivity = 1.0f;

    // Use this for initialization
    void Start ()
    {
        Init();

        //StartCoroutine(BroadcastTest());
	}

    IEnumerator BroadcastTest()
    {
        BroadCast.Start();
        yield return new WaitForSeconds(1.0f);
        BroadCastReciever.Start();
    }



    private void Init()
    {
        GameStep = GameStep.NotStart;

        GameObject uiObj = Instantiate(Resources.Load("ui/MainView") as GameObject);
        uiObj.transform.SetParent(Canvas.transform, false);
        MainView = uiObj.GetComponent<MainView>();
        //CreateGameUI();
    }
	
    private T CreateInstance<T>()
        where T : MonoBehaviour
    {
        T t;
        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);
        t = obj.AddComponent<T>();
        t.name = typeof(T).ToString();

        return t;
    }

    public void CreateOnlineView()
    {
        GameObject uiObj = Instantiate(Resources.Load("ui/OnlineView") as GameObject);
        uiObj.transform.SetParent(Canvas.transform, false);
        OnlineView = uiObj.GetComponent<OnlineView>();
    }

    public void CreateGameUI()
    {
        GameObject uiObj = Instantiate(Resources.Load("ui/CreateRoomView") as GameObject);
        uiObj.transform.SetParent(Canvas.transform, false);
        CreateRoomView = uiObj.GetComponent<CreateRoomView>();
    }

    public void EnterGame(GameConf conf)
    {
        GameConf = conf;
        CurPlayer = 1;

        CreateRoomView.gameObject.SetActive(false);

        GameObject uiObj = Instantiate(Resources.Load("ui/GameView") as GameObject);
        uiObj.transform.SetParent(Canvas.transform, false);
        GameView = uiObj.GetComponent<GameView>();
        GameView.SetRoomConfig(conf);
        GameView.SetCurPlayer(CurPlayer);

        // camera
        if(conf.MapType == MapType.Small)
        {
            Camera.main.transform.position = new Vector3(1.57f, 9.07f, -5.82f);
        }
        else if(conf.MapType == MapType.Middle)
        {
            Camera.main.transform.position = new Vector3(2.18f, 9.07f, -7.01f);
        }
        else if(conf.MapType == MapType.Big)
        {
            Camera.main.transform.position = new Vector3(2.58f, 9.07f, -8.2f);
        }

        HexManager = CreateInstance<HexManager>();

        Hammer = Instantiate(Resources.Load("model/hammer") as GameObject, Vector3.one * 100, Quaternion.identity)
            .GetComponent<Hammer>();

        GameStep = GameStep.SelectingMain;
    }

    public void ExitGame()
    {
        Destroy(GameView.gameObject);
        HexManager.ClearAll();
        Destroy(Hammer.gameObject);

        CreateRoomView.gameObject.SetActive(true);
    }

    public void Restart()
    {
        m_curTargetHex = null;
        m_mainHex = null;
        HexManager.Restart();
        CurPlayer = 1;

        Hammer.transform.position = Vector3.one * 100;
        GameView.ShowWheel(false);
        GameView.SetCurPlayer(CurPlayer);

        GameStep = GameStep.SelectingMain;
    }

    //public void OnSelectMain(Hexagon hex)
    //{
    //    m_mainHex = hex as GameHexagon;

    //    GameStep = GameStep.Start;
    //    GameStep = GameStep.RandomOperation;
    //}

    //public void OnSelectTarget(Hexagon hex)
    //{
    //    m_curTargetHex = hex;
    //    HexManager.OnSelectTarget(hex);
    //    GameStep = GameStep.SelectingStrength;
    //}

    public void OnGameOver()
    {
        GameStep = GameStep.GameOver;
    }

    public void OnStartHitting()
    {
        if(OpType == OpType.Pass)
        {
            GameView.ShowWheel(false);
            GameView.ShowWheelLater(true, 0.5f);

            NextPlayer();
            return;
        }

        // 检查是否有选定的颜色,如果已经没有相应的颜色，则再次随机
        if(OpType != OpType.Both && !HexManager.HasColor(OpType))
        {
            GameView.ShowWheel(false);
            GameView.ShowWheelLater(true, 0.5f);
            GameView.ShowTips("颜色无效，再转一回吧");

            return;
        }

        GameStep = GameStep.Hitting;
        GameView.ShowWheel(false);
    }

    private void NextPlayer(bool isGood = false)
    {
        ++CurPlayer;
        if(CurPlayer > GameConf.MemCount)
        {
            CurPlayer = 1;
        }

        GameView.SetCurPlayer(CurPlayer);

        string goodOrNot = isGood ? "GOOD!!!\n" : "";
        GameView.ShowTips(goodOrNot + "下回合: 玩家" + CurPlayer);
    }


	// Update is called once per frame
	void Update ()
    {
        HandleHitting();

        // EXIT
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
	}

    private void HandleHitting()
    {
        if(GameStep == GameStep.SelectingMain)
        {
            HandleMain();
        }
        else if(GameStep == GameStep.Hitting)
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

                GameView.ShowWheelLater(true, 1);
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
            if (theHex != null && theHex != m_mainHex && CheckColor(theHex, OpType))
            {
                m_curTargetHex = theHex as GameHexagon;
                GameView.StartSliderMoving();

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

            float strength = GameView.StopSliderMoving() * c_strenghtSensitivity;
            if (m_curTargetHex == GetHittingHex())
            {
                //if(strength > 0.95f)
                //{
                //    GameView.ShowTips("GOOD !!!");
                //}

                // 锤子
                Hammer.Knock();

                StopAllCoroutines();
                StartCoroutine(UpdateAllHexesLater(strength));
            }
            else
            {
                m_curTargetHex = null;
            }
            
            GameView.ResetSlider();
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
                GameView.ShowWheelLater(true, 1f);

                NextPlayer(lastStrength > 0.95f);
            }
        }

        m_curTargetHex = null;
    }

    private static bool CheckColor(GameHexagon hex, OpType op)
    {
        if(op == OpType.Pass)
        {
            Globals.Instance.GameView.ShowTips("此次跳过");
            return false;
        }
        else if(op == OpType.Both)
        {
            return true;
        }
        else
        {
            if(hex.OpType != op)
            {
                Globals.Instance.GameView.ShowTips(
                    string.Format("<color=yellow>必须砸{0}颜色块！</color>", GameView.s_ops[op]));
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
