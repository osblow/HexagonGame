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


            RoomBroadCast roomBroadCast = new RoomBroadCast();
            roomBroadCast.address = "hello";
            roomBroadCast.port = 1111;
            RoomBroadCast.RoomConf conf = new RoomBroadCast.RoomConf();
            conf.name = "hey";
            conf.maxMemCount = 10;
            conf.mapType = RoomBroadCast.RoomConf.MapType.Big;
            conf.forceKill = true;
            roomBroadCast.roomConf = conf;

            Proto pt = new Proto();
            byte[] data;
            using(System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                pt.Serialize(stream, roomBroadCast);
                data = stream.ToArray();
            }
            Debug.Log(data.Length);


            using(System.IO.MemoryStream stream = new System.IO.MemoryStream(data))
            {
                RoomBroadCast receieved = new RoomBroadCast();
                receieved = (RoomBroadCast)pt.Deserialize(stream, null, typeof(RoomBroadCast));
                Debug.LogFormat("{0} {1} {2} {3} {4}", receieved.address, receieved.port, receieved.roomConf.name, receieved.roomConf.mapType, receieved.roomConf.forceKill);

            }
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
            Globals.Instance.UIManager.CreateUI<UIOnline>();

            BroadCastReciever.Start();
        }
    }
}
