using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Osblow
{
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
}
