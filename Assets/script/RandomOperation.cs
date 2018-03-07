using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomOperation : MonoBehaviour {
    public static string[] s_ops =
    {
        "跳过",
        "白色",
        "绿色",
        "任意",
    };


    public Text Optext;


	public void OnClickRandom()
    {
        Optext.text = s_ops[UnityEngine.Random.Range(0, s_ops.Length)];
    }
}
