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
        GameConf conf = new GameConf();
        conf.Name = Globals.Instance.OnlineView.RoomName.text;
        conf.MapType = (MapType)MapOption.value;
        conf.MemCount = MemCountOption.value + 2;
        conf.ForceKill = ForceToggle.isOn;


        if (Globals.Instance.IsOnline)
        {
            BroadCast.Start(conf);
        }
        else
        {
            Globals.Instance.EnterGame(conf);
        }
    }
}
