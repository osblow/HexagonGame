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


    public Text Optext;
    public Slider StrengthSlider;

    private HitStrength m_hitStrength;
    private bool m_hitStart = false;

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

        if (m_hitStart)
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_hitStrength.SetEnabled(false);
                Debug.Log(m_hitStrength.Value);
                m_hitStart = false;
            }
        }
    }


    public void OnClickRandom()
    {
        Optext.text = s_ops[UnityEngine.Random.Range(0, s_ops.Length)];
    }

    public void OnClickRestart()
    {
        HexManager.Instance.Restart();
    }

    public void OnClickHit()
    {
        m_hitStart = true;
        m_hitStrength.SetEnabled(true);
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
        Init();
    }

    public void SetEnabled(bool enabled)
    {
        m_enabled = enabled;
    }

    private void Init()
    {
        m_curSpeed = 0;
        m_curValue = 0;
        m_sliderObj.value = m_curValue;
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
