using System;
using System.Collections;
using System.Collections.Generic;

namespace Osblow
{
    public class ObjectBase
    {
        #region BASE
        public virtual void Start()
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                m_children[i].Start();
            }
        }

        public virtual void Update(float delta)
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                m_children[i].Update(delta);
            }
        }

        public virtual void Destroy()
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                m_children[i].Destroy();
            }
        }
        #endregion


        #region 父子关系
        protected List<ObjectBase> m_children = new List<ObjectBase>();
        protected ObjectBase m_parent;

        protected ObjectBase AddChild(ObjectBase child)
        {
            m_children.Add(child);
            child.m_parent = this;
            child.Start();

            return child;
        }

        protected void RemoveChild(ObjectBase child, bool cleanUp = true)
        {
            m_children.Remove(child);

            if (cleanUp)
            {
                child.Destroy();
            }
        }
        #endregion


        #region MSG
        private MsgManager m_msgManager = new MsgManager();


        protected void AddMsgEvent(MsgType type, Action callback)
        {
            m_msgManager.AddMsg(type, callback);
        }

        protected void RemoveMsgEvent(MsgType type, Action callback)
        {
            m_msgManager.RemoveMsg(type, callback);
        }

        protected void AddMsgEvent(MsgType type, Action<object[]> callback)
        {
            m_msgManager.AddMsg(type, callback);
        }

        protected void RemoveMsgEvent(MsgType type, Action<object[]> callback)
        {
            m_msgManager.RemoveMsg(type, callback);
        }

        public void SendMessage(MsgType type, params object[] args)
        {
            m_msgManager.Dispatch(type, args);

            for (int i = 0; i < m_children.Count; i++)
            {
                m_children[i].SendMessage(type, args);
            }
        }

        /// <summary>
        /// 向所有物体发送消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        protected void Broadcast(MsgType type, params object[] args)
        {
            Globals.Instance.SendMessage(type, args);
        }
        #endregion
    }
}
