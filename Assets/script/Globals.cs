using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GameStep
{
    NotStart = 0,
    SelectingMain = 1,
    Start = 2,
    SelectingTarget = 3,
    SelectingStrength = 4,
    Hitting = 5,
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


    public Canvas Canvas;
    public HexManager HexManager;
    public GameView GameView;


    private Hexagon m_mainHex;
    private Hexagon m_curTargetHex;
    private const float c_strenghtSensitivity = 1.0f;

    // Use this for initialization
    void Start ()
    {
        Init();
	}

    private void Init()
    {
        GameStep = GameStep.NotStart;
        HexManager = CreateInstance<HexManager>();
        CreateGameUI();

        GameStep = GameStep.SelectingMain;
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

    private void CreateGameUI()
    {
        GameObject uiObj = Instantiate(Resources.Load("ui/GameView") as GameObject);
        uiObj.transform.SetParent(Canvas.transform, false);
        GameView = uiObj.GetComponent<GameView>();
    }


    public void OnSelectMain(Hexagon hex)
    {
        m_mainHex = hex;

        GameStep = GameStep.SelectingTarget;
    }

    public void OnSelectTarget(Hexagon hex)
    {
        m_curTargetHex = hex;
        HexManager.OnSelectTarget(hex);
        GameStep = GameStep.SelectingStrength;
    }

	// Update is called once per frame
	void Update ()
    {
		if(GameStep == GameStep.Hitting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                float strength = GameView.StopSliderMoving() * c_strenghtSensitivity;
                m_curTargetHex.OnHit(strength);

                GameView.ResetSlider();
                HexManager.OnSelectTarget(null);
                GameStep = GameStep.SelectingTarget;
            }
        }
	}
}
