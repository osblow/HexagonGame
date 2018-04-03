using UnityEngine;
using Osblow.Net.Server;
using Osblow.HexProto;

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
            UIPrompt prompt = Globals.Instance.UIManager.CreateUI<UIPrompt>();
            prompt.ConfirmDelegate += delegate (object[] promptArgs)
            {
                Globals.Instance.MyName = promptArgs[0].ToString();

                Globals.Instance.IsOnline = true;
                Globals.Instance.UIManager.RemoveUI(this);
                Globals.Instance.UIManager.CreateUI<UIOnline>();

                BroadCastReciever.Start();
            };
            prompt.Title.text = "请给自己起个名字";
        }
    }
}
