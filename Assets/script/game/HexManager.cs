using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Osblow
{
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
}
