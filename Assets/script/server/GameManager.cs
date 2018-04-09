using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    class GameManager : ObjectBase
    {
        private Dictionary<int, Player> m_players;


        public override void Start()
        {
            base.Start();

            m_players = new Dictionary<int, Player>();
        }

        public void AddPlayer(int guid, Player player)
        {
            if (m_players.ContainsKey(guid))
            {
                m_players[guid].Close();
                m_players[guid].Destroy();
            }

            m_players[guid] = player;
        }
    }
}
