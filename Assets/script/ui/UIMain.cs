using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Osblow
{
    class UIMain : UIBase
    {
        public override string PrefabPath
        {
            get
            {
                return "ui/MainView";
            }
        }

        public override void Start()
        {
            AddClickEvent(GetWidget("OfflineBtn"), OnClickOffline);
            AddClickEvent(GetWidget("OnlineBtn"), OnClickOnline);
        }

        private void OnClickOffline(GameObject obj, object[] args)
        {
            Globals.Instance.IsOnline = false;
            Globals.Instance.UIManager.RemoveUI(this);
            Globals.Instance.UIManager.CreateUI<UICreateRoom>();
            //gameObject.SetActive(false);
            //Globals.Instance.CreateGameUI();
        }

        private void OnClickOnline(GameObject obj, object[] args)
        {
            Globals.Instance.IsOnline = true;
            Globals.Instance.UIManager.RemoveUI(this);
            //gameObject.SetActive(false);
            //Globals.Instance.CreateOnlineView();

            //BroadCastReciever.Start();
        }
    }
}
