using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Osblow
{
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
            Globals.Instance.CoroutineManager.StartCoroutine(ResetAnim());
        }

        IEnumerator ResetAnim()
        {
            while (m_curValue > 0)
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
            if (!m_enabled)
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
}
