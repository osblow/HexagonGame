using System;
using System.Collections.Generic;

namespace Osblow
{
	public class MsgManager
	{
		private Dictionary<MsgType, List<Delegate>> m_msgList;


		public MsgManager ()
		{
			m_msgList = new Dictionary<MsgType, List<Delegate>> ();
		}


		public void AddMsg(MsgType type, Action callback)
		{
			if (!m_msgList.ContainsKey (type))
			{
				m_msgList.Add (type, new List<Delegate>());
			}

			m_msgList[type].Add(callback);
		}

        public void AddMsg(MsgType type, Action<object[]> callback)
        {
            if (!m_msgList.ContainsKey(type))
            {
                m_msgList.Add(type, new List<Delegate>());
            }

            m_msgList[type].Add(callback);
        }

		public void RemoveMsg(MsgType type, Action callback)
        {
            if (!m_msgList.ContainsKey(type))
            {
                return;
            }

            m_msgList[type].Remove(callback);
        }

        public void RemoveMsg(MsgType type, Action<object[]> callback)
        {
            if (!m_msgList.ContainsKey(type))
            {
                return;
            }

            m_msgList[type].Remove(callback);
        }

        public void Dispatch(MsgType type, params object[] args)
        {
            if (!m_msgList.ContainsKey(type))
            {
                return;
            }

            for (int i = 0; i < m_msgList[type].Count; i++)
            {
                if (m_msgList[type][i] is Action)
                {
                    ((Action)m_msgList[type][i]).Invoke();
                }
                else
                {
                    ((Action<object[]>)m_msgList[type][i]).Invoke(args);
                }
            }
        }
	}
}

