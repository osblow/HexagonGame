using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    public static GameView Instance { get { return s_instance; } }
    private static GameView s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    

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

    public Text ConfText;
    public Text TipsText;
    public Text Optext;
    public Slider StrengthSlider;
    public RectTransform Wheel;
    public Text PlayerText;


    private HitStrength m_hitStrength;
    private RandomWheel m_randomWheel;

    private void Start()
    {
        m_hitStrength = new HitStrength(StrengthSlider);
        m_randomWheel = new RandomWheel(Wheel);
        ShowWheel(false);
    }

    private void Update()
    {
        if(m_hitStrength != null)
        {
            m_hitStrength.Update();
        }

        if(m_randomWheel != null)
        {
            m_randomWheel.Update();
        }
    }

    private static Dictionary<MapType, string> s_mapName = new Dictionary<MapType, string>
    {
        { MapType.Small, "小" },
        { MapType.Middle, "中" },
        { MapType.Big, "大" },
    };
    public void SetRoomConfig(GameConf conf)
    {
        ConfText.text = string.Format(
            "地图: {0}\n人数: {1}\n必须砸落: {2}", 
            s_mapName[conf.MapType], 
            conf.MemCount, 
            conf.ForceKill ? "是" : "否");
    }

    public void SetCurPlayer(int index)
    {
        PlayerText.text = "玩家" + index;
    }

    public void OnClickRandom()
    {
        float rand = Random.Range(0, 100) * 0.01f;
        for (OpType i = OpType.Pass; i <= OpType.Both; i++)
        {
            if (s_probabilities[i] > rand)
            {
                Globals.Instance.OpType = i;
                break;
            }
        }
        //Debug.Log(rand + " " + Globals.Instance.OpType);


        //Globals.Instance.OpType = (OpType)UnityEngine.Random.Range(0, (int)(OpType.Both + 1));
        Optext.text = s_ops[Globals.Instance.OpType];

        m_randomWheel.SetOp(Globals.Instance.OpType);
        m_randomWheel.SetActive(true);
    }


    public void OnClickExit()
    {
        Globals.Instance.ExitGame();
    }

    public void OnClickRestart()
    {
        Globals.Instance.Restart();
    }

    public void OnClickHit()
    {
        //if(Globals.Instance.GameStep != GameStep.SelectingStrength)
        //{
        //    return;
        //}

        //m_hitStrength.SetEnabled(true);
        //Globals.Instance.GameStep = GameStep.Hitting;
    }


    /// <summary>
    /// 力度条开始移动
    /// </summary>
    public void StartSliderMoving()
    {
        m_hitStrength.SetEnabled(true);
    }


    /// <summary>
    /// 力度条停止，并返回当前力度（0-1）
    /// </summary>
    /// <returns>力度</returns>
    public float StopSliderMoving()
    {
        m_hitStrength.SetEnabled(false);

        return m_hitStrength.Value;
    }

    public void ResetSlider()
    {
        m_hitStrength.Reset();
    }


    public void ShowTips()
    {
        if (m_isSpecialText)
        {
            return;
        }

        TipsText.text = s_tips[Globals.Instance.GameStep];

        if(Globals.Instance.GameStep == GameStep.GameOver)
        {
            TipsText.text = s_tips[GameStep.GameOver] + "  玩家"+Globals.Instance.CurPlayer+"输了";
        }
    }

    private bool m_isSpecialText = false;
    public void ShowTips(string tips)
    {
        TipsText.text = tips;
        m_isSpecialText = true;

        CancelInvoke("ResetTips");
        Invoke("ResetTips", 1.5f);
    }

    private void ResetTips()
    {
        m_isSpecialText = false;
        ShowTips();
    }


    public void ShowWheel(bool isShow)
    {
        Wheel.transform.parent.gameObject.SetActive(isShow);
    }

    public void ShowWheelLater(bool isShow, float delay)
    {
        StartCoroutine(ShowWheelLaterImp(isShow, delay));
    }

    private IEnumerator ShowWheelLaterImp(bool isShow, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowWheel(isShow);
    }
}

public class HitStrength
{
    private const float c_accelerate = 3f;

    private bool m_enabled;
    private Slider m_sliderObj;
    private float m_curSpeed = 0;

    private float m_curValue = 0;
    public float Value { get { return m_curValue; } }


    public HitStrength(Slider sliderObj)
    {
        m_sliderObj = sliderObj;
        Reset();
    }

    public void SetEnabled(bool enabled)
    {
        m_enabled = enabled;
    }

    public void Reset()
    {
        Globals.Instance.StartCoroutine(ResetAnim());
    }

    IEnumerator ResetAnim()
    {
        while(m_curValue > 0)
        {
            m_curValue -= Time.deltaTime * 2;
            m_sliderObj.value = m_curValue;
            yield return null;
        }

        m_curSpeed = 0;
        m_curValue = 0;
        m_sliderObj.value = 0;
        Globals.Instance.Hammer.Knock();
    }

    public void Update()
    {
        if(!m_enabled)
        {
            return;
        }

        m_curSpeed += c_accelerate * Time.deltaTime;
        // loop
        if (m_curValue >= 1)
        {
            m_curSpeed *= -1;
        }
        
        m_curValue += m_curSpeed * Time.deltaTime;
        m_sliderObj.value = m_curValue;
        Globals.Instance.Hammer.SetStrength(m_curValue);
    }
}

public class RandomWheel
{
    private static Dictionary<OpType, float[]> s_opAngles = new Dictionary<OpType, float[]>
    {
        { OpType.White, new float[]{ 0f, 90f } },
        { OpType.Green, new float[]{ 90f, 180f } },
        { OpType.Both, new float[]{ 180f, 270f } },
        { OpType.Pass, new float[]{ 270f, 360f } },
    };
    private const float c_accelerate = 6f;
    private const float c_extraRotAngle = 1080f;

    private RectTransform m_wheel;
    private bool m_isActive = false;

    private float m_rotTotalAngle = -1f;
    private float m_rotTotalTime = -1f;


    public RandomWheel(RectTransform wheelTrans)
    {
        m_wheel = wheelTrans;
    }

    public void SetOp(OpType op)
    {
        float randomAngle = Random.Range(s_opAngles[op][0], s_opAngles[op][1]);
        //Debug.Log("Angle= " + randomAngle);

        m_rotTotalAngle = randomAngle + c_extraRotAngle;
        //m_rotTotalTime = Mathf.Sqrt(m_rotTotalAngle / c_accelerate);
        m_rotTotalTime = Mathf.Log(m_rotTotalAngle) / c_accelerate;
        Debug.Log(m_rotTotalTime);

        m_timer = -m_rotTotalTime;
    }

    public void SetActive(bool active)
    {
        m_isActive = active;

        if (!active)
        {
            Reset();
        }
    }

    public void Reset()
    {
        m_wheel.rotation = Quaternion.identity;
    }

    float m_timer = 0;
    public void Update()
    {
        if (!m_isActive)
        {
            return;
        }

        if (m_timer <= 0.5f)
        {
            m_timer += Time.deltaTime;
            //m_wheel.rotation = Quaternion.Euler(0, 0, m_rotTotalAngle - c_accelerate * m_timer * m_timer);
            m_wheel.rotation = Quaternion.Euler(0, 0, m_rotTotalAngle - Mathf.Exp(-m_timer * c_accelerate));
        }
        else
        {
            m_isActive = false;
            Globals.Instance.OnStartHitting();
        }
    }
}
