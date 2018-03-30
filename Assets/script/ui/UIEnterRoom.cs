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
        }

        private void OnPlayerEnter(object[] args)
        {
            CreateItem("hehhe");
        }

        private void CreateItem(string name)
        {
            GameObject newItem = GameObject.Instantiate(m_roomItemTpl, m_roomListRoot);
            newItem.transform.Find("name").GetComponent<Text>().text = name;
            newItem.SetActive(true);
        }
    }
}
