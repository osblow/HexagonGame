using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Osblow
{
    public class UIManager : ObjectBase
    {
        private GameObject m_canvas;


        public override void Start()
        {
            base.Start();
            m_canvas = GameObject.Find("Canvas");
        }


        public T CreateUI<T>() where T : UIBase, new()
        {
            T t = new T();
            t.Obj = GameObject.Instantiate(Globals.Instance.PrefabPool.GetPrefab(t.PrefabPath));
            t.Obj.transform.SetParent(m_canvas.transform, false);
            t.Obj.SetActive(true);
            AddChild(t);

            return t;
        }

        public T GetUIByType<T>() where T : UIBase
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                if(m_children[i] is T)
                {
                    return m_children[i] as T;
                }
            }

            return null;
        }

        public void RemoveUI(UIBase ui)
        {
            RemoveChild(ui);
        }

        public void RemoveUIByType<T>() where T : UIBase
        {
            UIBase theUI = GetUIByType<T>();
            if(theUI != null)
            {
                RemoveChild(theUI);
            }
        }
    }
}
