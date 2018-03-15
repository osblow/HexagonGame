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



    public static string[] s_ops =
    {
        "跳过",
        "白色",
        "绿色",
        "任意",
    };

    public static Dictionary<GameStep, string> s_tips = new Dictionary<GameStep, string>
    {
        { GameStep.NotStart, "等待开始..." },
        { GameStep.SelectingMain, "选择主格子" },
        { GameStep.Start, "游戏开始" },
        { GameStep.SelectingTarget, "请选择目标格子" },
        { GameStep.SelectingStrength, "请点击锤子图标，激活力度条" },
        { GameStep.Hitting, "点击屏幕任何位置，砸啦！" },
    };

    public Text TipsText;
    public Text Optext;
    public Slider StrengthSlider;

    private HitStrength m_hitStrength;

    private void Start()
    {
        m_hitStrength = new HitStrength(StrengthSlider);
    }

    private void Update()
    {
        if(m_hitStrength != null)
        {
            m_hitStrength.Update();
        }
    }


    public void OnClickRandom()
    {
        Optext.text = s_ops[UnityEngine.Random.Range(0, s_ops.Length)];
    }

    public void OnClickRestart()
    {
        //HexManager.Instance.Restart();
    }

    public void OnClickHit()
    {
        if(Globals.Instance.GameStep != GameStep.SelectingStrength)
        {
            return;
        }

        m_hitStrength.SetEnabled(true);
        Globals.Instance.GameStep = GameStep.Hitting;
    }


    /// <summary>
    /// 力度条停止，并返回当前力度（0-1）
    /// </summary>
    /// <returns>力度</returns>
    public float StopSliderMoving()
    {
        m_hitStrength.SetEnabled(false);
        Debug.Log(m_hitStrength.Value);

        return m_hitStrength.Value;
    }

    public void ResetSlider()
    {
        m_hitStrength.Reset();
    }


    public void ShowTips()
    {
        TipsText.text = s_tips[Globals.Instance.GameStep];
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
    }
}
