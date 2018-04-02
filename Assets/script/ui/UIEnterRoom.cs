using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Osblow
{
    class UIEnterRoom : UIBase
    {
        public override string PrefabPath
        {
            get
            {
                return "ui/EnterRoomView";
            }
        }

        private Transform m_roomListRoot;
        private GameObject m_roomItemTpl;


        public override void Start()
        {
            m_roomListRoot = GetWidget("RoomList/Viewport/Content").transform;
            m_roomItemTpl = GetWidget("RoomList/Viewport/Content/Item");

            AddMsgEvent(MsgType.OnPlayerEnter, OnPlayerEnter);
        }

        private void OnPlayerEnter(object[] args)
        {
            CreateItem(args[0].ToString() + " " + args[1].ToString());
        }

        private void CreateItem(string name)
        {
            GameObject newItem = GameObject.Instantiate(m_roomItemTpl, m_roomListRoot);
            newItem.transform.Find("name").GetComponent<Text>().text = name;
            newItem.SetActive(true);
        }
    }
}
