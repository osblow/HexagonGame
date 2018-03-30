using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    public GameObject Model;

    private float m_curStrength = 0;
    private Coroutine m_knockCoroutine = null;


    public void SetStrength(float strength)
    {
        m_curStrength = strength;
        Model.transform.localRotation = Quaternion.Euler(0, 0, -90+strength * 90);
    }

    public void Knock()
    {
        if (m_knockCoroutine != null)
        {
            StopCoroutine(m_knockCoroutine);
        }
        m_knockCoroutine = StartCoroutine(KnockImp());
    }

	IEnumerator KnockImp()
    {
        float time = 0;
        while(time < 0.1f)
        {
            time += Time.deltaTime;
            SetStrength(Mathf.Lerp(m_curStrength, 0, time / 0.1f));
            yield return null;
        }

        SetStrength(0);
    }
}
