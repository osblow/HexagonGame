using UnityEngine;

namespace Osblow
{
    public class UIBase : ObjectBase
    {
        public virtual string PrefabPath
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public GameObject Obj;

        protected GameObject GetWidget(string childPath)
        {
            return Obj.transform.Find(childPath).gameObject;
        }

        protected void AddClickEvent(GameObject obj, 
            CustomEventTrigger.Callback callback, 
            params object[] args)
        {
            CustomEventTrigger evt = obj.GetComponent<CustomEventTrigger>();
            if (!evt)
            {
                evt = obj.AddComponent<CustomEventTrigger>();
            }

            evt.Args = args;
            evt.Callback_OnClick += callback;
        }

        protected void AddTouchEvent(GameObject obj, 
            CustomEventTrigger.Callback begin_callback, 
            CustomEventTrigger.Callback end_callback, 
            params object[] args)
        {
            CustomEventTrigger evt = obj.GetComponent<CustomEventTrigger>();
            if (!evt)
            {
                evt = obj.AddComponent<CustomEventTrigger>();
            }

            evt.Args = args;
            evt.Callback_OnBeginDrag += begin_callback;
            evt.Callback_OnEndDrag += end_callback;
        }





        public override void Start()
        {
            base.Start();
        }

        public override void Update(float delta)
        {
            base.Update(delta);
        }

        public override void Destroy()
        {
            base.Destroy();
            GameObject.Destroy(Obj);
        }
    }
}
