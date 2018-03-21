using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomView : MonoBehaviour
{
    public Dropdown MapOption;
    public Dropdown MemCountOption;
    public Toggle ForceToggle;
    

	public void OnClickConfirm()
    {
        //Debug.Log((MapType)(MapOption.value));
        //Debug.Log(MemCountOption.value+2);
        //Debug.Log(ForceToggle.isOn);
        if (Globals.Instance.IsOnline)
        {
            BroadCast.Start();
        }
        else
        {
            GameConf conf = new GameConf();
            conf.MapType = (MapType)MapOption.value;
            conf.MemCount = MemCountOption.value + 2;
            conf.ForceKill = ForceToggle.isOn;

            Globals.Instance.EnterGame(conf);
        }
    }
}
