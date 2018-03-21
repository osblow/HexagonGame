using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 因为只有主线程可访问场景资源，异步调用全部在这里注册，然后帧末执行
/// </summary>
public class AsyncInvokeMng : MonoBehaviour
{
    //public event Action EventsToAct;

    public List<Action> Events = new List<Action>();


    public void LateUpdate()
    {
        //if (null != EventsToAct)
        //{
        //    EventsToAct.Invoke();
        //    EventsToAct = null;
        //}

        for (int i = 0; i < Events.Count; i++)
        {
            //if(Events[i].Target != null)
            {
                Events[i].Invoke();
            }
        }
        ClearAll();
    }

    float m_gcTimer = 0;

    public void ClearAll()
    {
        //EventsToAct = null;

        if (Events.Count > 0) Events.Clear();
        m_gcTimer += Time.deltaTime;
        if(m_gcTimer > 15)
        {
            GC.Collect();
            m_gcTimer = 0;
        }
    }
}
