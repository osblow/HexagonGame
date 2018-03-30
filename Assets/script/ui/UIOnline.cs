using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Osblow.Net.Server;

namespace Osblow
{
    class UIOnline : UIBase
    {
        public static string s_roomName = "";

        public override string PrefabPath
        {
            get
            {
                return "ui/OnlineView";
            }
        }


        private Transform m_roomListRoot;
        private GameObject m_roomItemTpl;

        private InputField m_roomName;

        private Dictionary<string, Dictionary<int, GameConf>> m_gameConfs
            = new Dictionary<string, Dictionary<int, GameConf>>();

        public override void Start()
        {
            m_roomListRoot = GetWidget("RoomList/Viewport/Content").transform;
            m_roomItemTpl = GetWidget("RoomList/Viewport/Content/Item");

            m_roomName = GetWidget("CreatePanel/InputField").GetComponent<InputField>();

            AddClickEvent(GetWidget("CreatePanel/CreateBtn"), OnClickCreate);

            AddMsgEvent(MsgType.OnFindServer, OnGetRoomConf);
        }

        public void OnGetRoomConf(object[] args)
        {
            string address = args[0].ToString();
            int port = (int)args[1];
            GameConf conf = args[2] as GameConf;

            if (conf == null)
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
            GameObject new_item = GameObject.Instantiate(m_roomItemTpl, m_roomListRoot);
            new_item.transform.Find("name").GetComponent<Text>().text = conf.Name;
            new_item.transform.Find("map").GetComponent<Text>().text = conf.MapType.ToString();
            new_item.transform.Find("mem").GetComponent<Text>().text = conf.MemCount.ToString();
            new_item.transform.Find("force").GetComponent<Text>().text = conf.ForceKill.ToString();
            new_item.SetActive(true);
        }

        public void OnClickCreate(GameObject obj, object[] args)
        {
            string roomName = m_roomName.text;
            s_roomName = roomName;
            Debug.Log("创建房间: " + roomName);
            
            Globals.Instance.UIManager.RemoveUI(this);
            Globals.Instance.UIManager.CreateUI<UICreateRoom>();
        }
    }
}
