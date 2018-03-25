using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Osblow
{
    public class HexManager : ObjectBase
    {
        private const string c_hex_res = "model/hex";
        private const float c_sqrt_3 = 1.717f;

        private const float c_targetAlpha = 0.75f;

        private Hexagon[][] m_hexagons;


        public void ClearAll()
        {
            for (int i = 0; i < m_hexagons.Length; i++)
            {
                for (int j = 0; j < m_hexagons[i].Length; j++)
                {
                    if (m_hexagons[i][j] != null)
                    {
                        m_hexagons[i][j].DestroyImmediate();
                    }
                }
            }
        }

        public void Restart()
        {
            ClearAll();

            Init();
        }

        public void UpdateAllBalance(Hexagon exclude = null)
        {
            if (m_hexagons == null)
            {
                return;
            }

            for (int i = 0; i < m_hexagons.Length; i++)
            {
                for (int j = 0; j < m_hexagons[i].Length; j++)
                {
                    if (m_hexagons[i][j] != null && m_hexagons[i][j].IsActive() && m_hexagons[i][j] != exclude)
                    {
                        m_hexagons[i][j].UpdateBalance();
                    }
                }
            }
        }

        //public void OnSelectTarget(Hexagon target)
        //{
        //    if (m_hexagons == null)
        //    {
        //        return;
        //    }

        //    for (int i = 0; i < m_hexagons.Length; i++)
        //    {
        //        for (int j = 0; j < m_hexagons[i].Length; j++)
        //        {
        //            if (m_hexagons[i][j] != null && m_hexagons[i][j].IsActive())
        //            {
        //                Color theColor = m_hexagons[i][j].Model.GetComponent<Renderer>().material.color;
        //                theColor.a = m_hexagons[i][j] == target ? c_targetAlpha : 1f;
        //                m_hexagons[i][j].Model.GetComponent<Renderer>().material.color = theColor;
        //            }
        //        }
        //    }
        //}

        public bool HasColor(OpType op)
        {
            if (op == OpType.Both || op == OpType.Pass)
            {
                return true;
            }


            for (int i = 0; i < m_hexagons.Length; i++)
            {
                for (int j = 0; j < m_hexagons[i].Length; j++)
                {
                    if (m_hexagons[i][j] != null && m_hexagons[i][j].IsActive())
                    {
                        if (m_hexagons[i][j] is GameHexagon && ((GameHexagon)m_hexagons[i][j]).OpType == op)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public override void Start()
        {
            base.Start();
            Init();
        }



        int m_size = 11;
        int[] m_columnForRaws = { 6, 7, 8, 9, 10, 11, 10, 9, 8, 7, 6 };
        int[] m_columnOffsetForRaws = { 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5 };
        
        void LoadConf()
        {
            if(Globals.Instance.GameConf.MapType == MapType.Small)
            {
                m_size = 7;
            }
            else if(Globals.Instance.GameConf.MapType == MapType.Middle)
            {
                m_size = 9;
            }
            else if(Globals.Instance.GameConf.MapType == MapType.Big)
            {
                m_size = 11;
            }

            m_columnForRaws = new int[m_size];
            for (int i = 0; i < m_size; i++)
            {
                m_columnForRaws[i] = m_size - Mathf.Abs(m_size / 2 - i);
                m_columnOffsetForRaws[i] = Mathf.Max(0, i - m_size / 2);
            }
        }
        

        void Init()
        {
            LoadConf();




            m_hexagons = new Hexagon[m_size][];
            for (int r = 0; r < m_size; r++)
            {
                m_hexagons[r] = new Hexagon[m_size];
            }


            int i = 0, j = m_columnOffsetForRaws[i];
            m_hexagons[i][j] = new SolidHexagon(Vector3.zero);
            for (; i < m_hexagons.Length; i++)
            {
                j = m_columnOffsetForRaws[i];
                if (i > 0)
                {
                    m_hexagons[i][j] = HexFactory.CreateHexagon(HexType.Solid, m_hexagons[i - 1][j], HexDir.LeftBack, i + "-" + j);
                }
                ++j;

                for (; j < m_columnOffsetForRaws[i] + m_columnForRaws[i]; j++)
                {
                    if (i == 0 || i == m_hexagons.Length - 1 || j == 0 || j == m_columnOffsetForRaws[i] + m_columnForRaws[i] - 1)
                    {
                        m_hexagons[i][j] = HexFactory.CreateHexagon(HexType.Solid, m_hexagons[i][j - 1], HexDir.Right, i + "-" + j);
                    }
                    else
                    {
                        m_hexagons[i][j] = HexFactory.CreateHexagon(HexType.Game, m_hexagons[i][j - 1], HexDir.Right, i + "-" + j);
                    }
                }


            }

            for (int k = 0; k < m_hexagons.Length; k++)
            {
                for (int x = 0; x < m_hexagons[k].Length; x++)
                {
                    Hexagon theHex = m_hexagons[k][x];
                    if (theHex == null)
                    {
                        continue;
                    }

                    // left forward
                    if (k > 0 && x > 0 && m_hexagons[k - 1][x - 1] != null)
                    {
                        theHex.SetSibling(HexDir.LeftForward, m_hexagons[k - 1][x - 1]);
                    }

                    // right forward
                    if (k > 0 && m_hexagons[k - 1][x] != null)
                    {
                        theHex.SetSibling(HexDir.RightForward, m_hexagons[k - 1][x]);
                    }

                    // left
                    if (x > 0 && m_hexagons[k][x - 1] != null)
                    {
                        theHex.SetSibling(HexDir.Left, m_hexagons[k][x - 1]);
                    }

                    // right
                    if (x < m_hexagons.Length - 1 && m_hexagons[k][x + 1] != null)
                    {
                        theHex.SetSibling(HexDir.Right, m_hexagons[k][x + 1]);
                    }

                    // left back
                    if (k < m_hexagons.Length - 1 && m_hexagons[k + 1][x] != null)
                    {
                        theHex.SetSibling(HexDir.LeftBack, m_hexagons[k + 1][x]);
                    }

                    // right back
                    if (k < m_hexagons.Length - 1 && x < m_hexagons[k].Length - 1 && m_hexagons[k + 1][x + 1] != null)
                    {
                        theHex.SetSibling(HexDir.RightBack, m_hexagons[k + 1][x + 1]);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public class Constants
    {
        public static string c_wallHex_res = "model/wallHex";
        public static string c_gameHex_res = "model/gameHex";

        public static float c_sqrt_3 = 1.717f;
    }

    public enum HexType
    {
        None = 0,
        Game = 1,
        Solid = 2,
    }

    public class HexFactory
    {
        public static Hexagon CreateHexagon(HexType type, Hexagon sibling, HexDir dirToSibling, string name)
        {
            Hexagon result = null;

            if (type == HexType.Game)
            {
                result = new GameHexagon(sibling.GetSiblingPos(dirToSibling));
                result.Obj.name = name;
            }
            else if (type == HexType.Solid)
            {
                result = new SolidHexagon(sibling.GetSiblingPos(dirToSibling));
                result.Obj.name = name;
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
        //public static implicit operator bool(Hexagon hex)
        //{
        //    return hex != null && hex.Obj && hex.m_enabled;
        //}

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

                Obj.transform.position = m_originPos - Vector3.up * m_heightOffset;

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


        public GameObject Obj = null;
        public GameObject Model;
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


        public Hexagon(Vector3 pos)
        {
            m_originPos = pos;
            Obj = GameObject.Instantiate(Resources.Load(m_resPath) as GameObject, pos, Quaternion.identity);
            Model = Obj.transform.Find("hexagon/Cylinder").gameObject;
            // friction
            for (HexDir i = HexDir.Left; i <= HexDir.LeftBack; i++)
            {
                m_frictions.Add(i, Random.Range(0.1f, 0.15f));
            }
        }

        public Vector3 GetSiblingPos(HexDir dir)
        {
            switch (dir)
            {
                case HexDir.Left:
                    return Obj.transform.position + Vector3.left;
                case HexDir.LeftForward:
                    return Obj.transform.position + Vector3.left * 0.5f + Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                case HexDir.RightForward:
                    return Obj.transform.position - Vector3.left * 0.5f + Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                case HexDir.Right:
                    return Obj.transform.position - Vector3.left;
                case HexDir.RightBack:
                    return Obj.transform.position - Vector3.left * 0.5f - Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                case HexDir.LeftBack:
                    return Obj.transform.position + Vector3.left * 0.5f - Vector3.forward * Constants.c_sqrt_3 * 0.5f;
                default:
                    return Vector3.zero;
            }
        }

        public virtual Hexagon CreateSibling<T>(HexDir dir, string index_name, System.Func<System.Type, Vector3, T> newFunc)
            where T : Hexagon, new()
        {
            T t = newFunc(typeof(T), GetSiblingPos(dir));
            t.Obj.name = index_name;
            //t.Obj.transform.position = GetSiblingPos(dir);
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
            return Obj && m_enabled;
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
            if (Obj)
            {
                GameObject.Destroy(Obj, 5.0f);
            }
        }

        public void DestroyImmediate()
        {
            if (Obj)
            {
                GameObject.DestroyImmediate(Obj);
            }
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

        //private void SearchSurroundedHex(Hexagon sibling, HexDir dir)
        //{
        //    Hexagon cur = sibling;
        //    HexDir curDir = GetNextDir(GetOppositeDir(dir), false);
        //    HexDir startDir = GetNextDir(curDir, false);


        //    int limit = 10;
        //    while(curDir != startDir && limit-- > 0)
        //    {
        //        if (null == cur || !cur.m_siblingHexes.ContainsKey(curDir))// || null == cur.m_siblingHexes[curDir])
        //        {
        //            break;
        //        }

        //        if (m_siblingHexes.ContainsKey(curDir))
        //        {
        //            continue;
        //        }

        //        HexDir theDir = GetOppositeDir(GetNextDir(GetNextDir(curDir)));
        //        AddOrReplaceSibling(theDir, cur.m_siblingHexes[curDir], true, false);

        //        cur = cur.m_siblingHexes[curDir];
        //        curDir = GetNextDir(curDir);
        //    }
        //}

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

        private static Color s_whiteColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        private static Color s_greenColor = new Color(0f, 0.4f, 0, 1.0f);


        public GameHexagon(Vector3 pos) : base(pos)
        {
            //Model.GetComponent<CustomCollider>().OnClick += OnClick;
            Model.GetComponent<CustomCollider>().ParentHexagon = this;
            Model.GetComponent<MeshCollider>().isTrigger = true;

            // random color
            OpType = (OpType)Random.Range(1, 3); // between green and white
            Color randColor = OpType == OpType.White ? s_whiteColor : s_greenColor;
            Model.GetComponent<Renderer>().material.color = randColor;
        }

        //public void OnClick()
        //{
        //    if (Globals.Instance.GameStep == GameStep.SelectingMain)
        //    {
        //        Model.GetComponent<Renderer>().material.color = Color.red;
        //        Globals.Instance.OnSelectMain(this);
        //    }
        //    else
        //    {
        //        Globals.Instance.OnSelectTarget(this);
        //        //m_enabled = false;
        //        //UpdateBalance();
        //    }
        //}
        public void OnBecomeMain()
        {
            Model.GetComponent<Renderer>().material.color = Color.red;
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
                    kval.Value.HeightOffset += Mathf.Min(/*staticFric_move + */dynamicFric_move, scaledStrength * c_maxHeightOffset);
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

                Model.AddComponent<Rigidbody>();
                Model.GetComponent<MeshCollider>().isTrigger = false;

                m_siblingHexes.Clear();
                m_enabled = false;
                Destroy();

                //Globals.Instance.HexManager.UpdateAllBalance(this);

                
                // 游戏结束 
                if (this == Globals.Instance.MainHex)
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

        public SolidHexagon(Vector3 pos) : base(pos) { }

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
