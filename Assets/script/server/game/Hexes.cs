using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    public class HexFactory
    {
        public static Hexagon CreateHexagon(HexType type, Hexagon sibling, HexDir dirToSibling, int index_x, int index_y)
        {
            Hexagon result = null;

            if (type == HexType.Game)
            {
                result = new GameHexagon(sibling.GetSiblingPos(dirToSibling));
            }
            else if (type == HexType.Solid)
            {
                result = new SolidHexagon(sibling.GetSiblingPos(dirToSibling));
            }

            return result;
        }
    }




    public enum HexDir
    {
        Left = 1,
        LeftForward = 2,
        RightForward = 3,
        Right = 4,
        RightBack = 5,
        LeftBack = 6,
    }

    public class Hexagon
    {
        protected int m_index_x = 0;
        protected int m_index_y = 0;

        protected const float c_maxHeightOffset = 1;
        protected float m_heightOffset = 0;
        /// <summary>
        /// 
        /// </summary>
        public virtual float HeightOffset
        {
            set
            {
                m_heightOffset = value;
                
                if (m_heightOffset >= c_maxHeightOffset * 0.95f)
                {
                    //UpdateBalance();
                }
            }
            get
            {
                return m_heightOffset;
            }
        }

        
        protected Vector3 m_originPos = Vector3.zero;

        protected Dictionary<HexDir, Hexagon> m_siblingHexes = new Dictionary<HexDir, Hexagon>();
        protected virtual string m_resPath
        {
            get
            {
                return "";
            }
        }

        protected bool m_enabled = true;
        // friction against six directions
        protected Dictionary<HexDir, float> m_frictions = new Dictionary<HexDir, float>();
        protected Random m_rand;


        public Hexagon(Vector3 pos, int index_x, int index_y)
        {
            m_rand = new Random(DateTime.UtcNow.Second);

            m_originPos = pos;
            m_index_x = index_x;
            m_index_y = index_y;

            // friction
            for (HexDir i = HexDir.Left; i <= HexDir.LeftBack; i++)
            {
                m_frictions.Add(i, m_rand.Next(10, 15) / 100.0f);
            }
        }

        public Vector3 GetSiblingPos(HexDir dir)
        {
            switch (dir)
            {
                case HexDir.Left:
                    return m_originPos + Vector3.left;
                case HexDir.LeftForward:
                    return m_originPos + Vector3.left * 0.5f + Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                case HexDir.RightForward:
                    return m_originPos - Vector3.left * 0.5f + Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                case HexDir.Right:
                    return m_originPos - Vector3.left;
                case HexDir.RightBack:
                    return m_originPos - Vector3.left * 0.5f - Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                case HexDir.LeftBack:
                    return m_originPos + Vector3.left * 0.5f - Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                default:
                    return Vector3.zero;
            }
        }

        public virtual Hexagon CreateSibling<T>(HexDir dir, string index_name, System.Func<System.Type, Vector3, T> newFunc)
            where T : Hexagon, new()
        {
            T t = newFunc(typeof(T), GetSiblingPos(dir));
            m_siblingHexes[dir] = t;
            return t;
        }

        public void SetSibling(HexDir dir, Hexagon hex)
        {
            m_siblingHexes[dir] = hex;
        }

        public void RemoveSibling(HexDir dir)
        {
            m_siblingHexes.Remove(dir);
        }


        public virtual bool IsActive()
        {
            return m_enabled;
        }

        protected virtual bool IsBalance()
        {
            if (m_siblingHexes.ContainsKey(HexDir.Left)
                && m_siblingHexes.ContainsKey(HexDir.Right))
            {
                return true;
            }

            if (m_siblingHexes.ContainsKey(HexDir.LeftForward)
                && m_siblingHexes.ContainsKey(HexDir.RightBack))
            {
                return true;
            }

            if (m_siblingHexes.ContainsKey(HexDir.LeftBack)
                && m_siblingHexes.ContainsKey(HexDir.RightForward))
            {
                return true;
            }

            if (m_siblingHexes.ContainsKey(HexDir.LeftForward)
                && m_siblingHexes.ContainsKey(HexDir.Right)
                && m_siblingHexes.ContainsKey(HexDir.LeftBack))
            {
                return true;
            }

            if (m_siblingHexes.ContainsKey(HexDir.Left)
                && m_siblingHexes.ContainsKey(HexDir.RightForward)
                && m_siblingHexes.ContainsKey(HexDir.RightBack))
            {
                return true;
            }

            return false;
        }

        public virtual void UpdateBalance()
        {
            if (!IsBalance())
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            m_enabled = false;
        }

        public virtual void OnHit(float strength)
        {

        }

        protected void AddOrReplaceSibling(HexDir dir, Hexagon hex, bool setOpposite = true, bool searchAround = true)
        {
            if (m_siblingHexes.ContainsKey(dir))
            {
                if (null != m_siblingHexes[dir]) m_siblingHexes[dir].Destroy();
                m_siblingHexes[dir] = hex;
            }
            else
            {
                m_siblingHexes.Add(dir, hex);
            }
        }


        /// <summary>
        /// Get next direction, default is clockwise
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        static HexDir GetNextDir(HexDir dir, bool isClockWise = true)
        {
            HexDir result = dir;

            if (isClockWise)
            {
                result += 1;
                if (result > HexDir.RightBack)
                {
                    result = HexDir.Left;
                }
            }
            else
            {
                result -= 1;
                if (result < HexDir.Left)
                {
                    result = HexDir.LeftBack;
                }
            }

            return result;
        }

        protected static HexDir GetOppositeDir(HexDir dir)
        {
            switch (dir)
            {
                case HexDir.Left:
                    return HexDir.Right;
                case HexDir.LeftForward:
                    return HexDir.RightBack;
                case HexDir.RightForward:
                    return HexDir.LeftBack;
                case HexDir.Right:
                    return HexDir.Left;
                case HexDir.RightBack:
                    return HexDir.LeftForward;
                case HexDir.LeftBack:
                    return HexDir.RightForward;
                default:
                    return 0;
            }
        }
    }

    public class GameHexagon : Hexagon
    {
        protected override string m_resPath
        {
            get
            {
                return Constants.c_gameHex_res;
            }
        }

        public OpType OpType = OpType.Both;

        public bool IsMain = false;


        public GameHexagon(Vector3 pos, int index_x, int index_y) : base(pos, index_x, index_y)
        {
            // random color
            OpType = (OpType)m_rand.Next(1, 3); // between green and white
        }
        
        public override void OnHit(float strength)
        {
            if (strength == 0)
            {
                return;
            }
            // 为了设初始值，将力度从0-1缩放平移到0.1-1
            float scaledStrength = 0.1f + strength * 0.9f;
            HeightOffset += scaledStrength * c_maxHeightOffset;

            foreach (KeyValuePair<HexDir, Hexagon> kval in m_siblingHexes)
            {
                if (kval.Value != null && kval.Value.IsActive())
                {
                    //float staticFric_move = (1 - strength) * (1 - strength) * m_frictions[kval.Key]; // simulation
                    float dynamicFric_move = (1 - strength) * m_frictions[kval.Key] * c_maxHeightOffset;
                    kval.Value.HeightOffset += Math.Min(/*staticFric_move + */dynamicFric_move, scaledStrength * c_maxHeightOffset);
                }
            }
        }

        protected override bool IsBalance()
        {
            return base.IsBalance() && m_enabled && m_heightOffset < c_maxHeightOffset * 0.95f;
        }

        public override void UpdateBalance()
        {
            if (!IsBalance())
            {
                foreach (KeyValuePair<HexDir, Hexagon> kval in m_siblingHexes)
                {
                    if (kval.Value != null && kval.Value.IsActive())
                    {
                        kval.Value.RemoveSibling(Hexagon.GetOppositeDir(kval.Key));
                    }
                }

                // 再分别调用邻居的刷新，如果有死亡将一起下落
                foreach (KeyValuePair<HexDir, Hexagon> kval in m_siblingHexes)
                {
                    if (kval.Value != null && kval.Value.IsActive())
                    {
                        kval.Value.UpdateBalance();
                    }
                }

                m_siblingHexes.Clear();
                Destroy();

                //Globals.Instance.HexManager.UpdateAllBalance(this);


                // 游戏结束 
                if (IsMain)
                {
                    Globals.Instance.OnGameOver();
                }
            }
        }
    }

    public class SolidHexagon : Hexagon
    {
        protected override string m_resPath
        {
            get
            {
                return Constants.c_wallHex_res;
            }
        }

        public SolidHexagon(Vector3 pos, int index_x, int index_y) : base(pos, index_x, index_y) { }

        public override float HeightOffset
        {
            get
            {
                return base.HeightOffset;
            }

            set
            {

            }
        }


        protected override bool IsBalance()
        {
            return true;
        }

        public void SetInvalidDir(HexDir[] dirs)
        {
            foreach (HexDir dir in dirs)
            {
                AddOrReplaceSibling(dir, null);
            }
        }
    }
}
