using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineView : MonoBehaviour
{
    public Transform RoomListRoot;
    public GameObject RoomItemTpl;

    public InputField RoomName;

    private Dictionary<string, Dictionary<int, GameConf>> m_gameConfs 
        = new Dictionary<string, Dictionary<int, GameConf>>();

    public void Start()
    {
        BroadCastReciever.OnGetConf = OnGetRoomConf;
    }

    public void OnGetRoomConf(string address, int port, GameConf conf)
    {
        if(conf == null)
        {
            return;
        }

        if (m_gameConfs.ContainsKey(address))
        {
            if (m_gameConfs[address].ContainsKey(port))
            {
                return;
            }
        }
        else
        {
            m_gameConfs.Add(address, new Dictionary<int, GameConf>());
        }
        m_gameConfs[address].Add(port, conf);


        CreateItem(conf);
    }

    private void CreateItem(GameConf conf)
    {
        GameObject new_item = Instantiate(RoomItemTpl, RoomListRoot);
        new_item.transform.Find("name").GetComponent<Text>().text = conf.Name;
        new_item.transform.Find("map").GetComponent<Text>().text = conf.MapType.ToString();
        new_item.transform.Find("mem").GetComponent<Text>().text = conf.MemCount.ToString();
        new_item.transform.Find("force").GetComponent<Text>().text = conf.ForceKill.ToString();
        new_item.SetActive(true);
    }

    public void OnClickCreate()
    {
        string roomName = RoomName.text;
        Debug.Log("创建房间: " + roomName);

        gameObject.SetActive(false);
        Globals.Instance.CreateGameUI();
    }
}
