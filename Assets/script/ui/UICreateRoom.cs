using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Osblow.Net.Server;

namespace Osblow
{
    class UICreateRoom : UIBase
    {
        public override string PrefabPath
        {
            get
            {
                return "ui/CreateRoomView";
            }
        }

        private Dropdown m_mapOption;
        private Dropdown m_memCountOption;
        private Toggle m_forceToggle;

        public override void Start()
        {
            m_mapOption = GetWidget("MapOption").GetComponent<Dropdown>();
            m_memCountOption = GetWidget("MemCountOption").GetComponent<Dropdown>();
            m_forceToggle = GetWidget("ForceToggle").GetComponent<Toggle>();

            AddClickEvent(GetWidget("Confirm"), OnClickConfirm);
        }

        


        public void OnClickConfirm(GameObject obj, object[] args)
        {
            GameConf conf = new GameConf();
            conf.Name = UIOnline.s_roomName;
            conf.MapType = (MapType)m_mapOption.value;
            conf.MemCount = m_memCountOption.value + 2;
            conf.ForceKill = m_forceToggle.isOn;
            Globals.Instance.GameConf = conf;

            Globals.Instance.UIManager.RemoveUI(this);
            if (Globals.Instance.IsOnline)
            {
                BroadCastServer.Start(conf);
                Globals.Instance.UIManager.CreateUI<UIEnterRoom>();
            }
            else
            {
                Globals.Instance.ToGame();
            }
        }
    }
}
