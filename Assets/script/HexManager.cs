using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexManager : MonoBehaviour
{
    private const string c_hex_res = "model/hex";
    private const float c_sqrt_3 = 1.717f;

    private const float c_targetAlpha = 0.75f;

    private Hexagon[][] m_hexagons;


    public void Restart()
    {
        for (int i = 0; i < m_hexagons.Length; i++)
        {
            for (int j = 0; j < m_hexagons[i].Length; j++)
            {
                if (m_hexagons[i][j])
                {
                    m_hexagons[i][j].DestroyImmediate();
                }
            }
        }

        Init();
    }

    public void UpdateAllBalance(Hexagon exclude = null)
    {
        if(m_hexagons == null)
        {
            return;
        }

        for (int i = 0; i < m_hexagons.Length; i++)
        {
            for (int j = 0; j < m_hexagons[i].Length; j++)
            {
                if (m_hexagons[i][j] && m_hexagons[i][j] != exclude)
                {
                    m_hexagons[i][j].UpdateBalance();
                }
            }
        }
    }

    public void OnSelectTarget(Hexagon target)
    {
        if (m_hexagons == null)
        {
            return;
        }

        for (int i = 0; i < m_hexagons.Length; i++)
        {
            for (int j = 0; j < m_hexagons[i].Length; j++)
            {
                if (m_hexagons[i][j])
                {
                    Color theColor = m_hexagons[i][j].Model.GetComponent<Renderer>().material.color;
                    theColor.a = m_hexagons[i][j] == target ? c_targetAlpha : 1f;
                    m_hexagons[i][j].Model.GetComponent<Renderer>().material.color = theColor;
                }
            }
        }
    }

	// Use this for initialization
	void Start () {
        Init();
	}
	
    void Init()
    {
        int raw = 11;
        int maxColumn = 11;
        int [] columnForRaws = { 6,7,8,9,10,11,10,9,8,7,6 };
        int[] columnOffsetForRaws = { 0,0,0,0,0,0,1,2,3,4,5};

        m_hexagons = new Hexagon[raw][];
        for (int r = 0; r < raw; r++)
        {
            m_hexagons[r] = new Hexagon[maxColumn];
        }


        int i = 0, j = columnOffsetForRaws[i];
        m_hexagons[i][j] = new SolidHexagon(Vector3.zero);
        for (; i < m_hexagons.Length; i++)
        {
            j = columnOffsetForRaws[i];
            if (i > 0)
            {
                m_hexagons[i][j] = HexFactory.CreateHexagon(HexType.Solid, m_hexagons[i - 1][j], HexDir.LeftBack, i + "-" + j);
            }
            ++j;

            for (; j < columnOffsetForRaws[i] + columnForRaws[i]; j++)
            {
                if (i == 0 || i == m_hexagons.Length-1 || j == 0 || j == columnOffsetForRaws[i]+ columnForRaws[i] - 1)
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
                if (k > 0 && x > 0 && m_hexagons[k - 1][x - 1])
                {
                    theHex.SetSibling(HexDir.LeftForward, m_hexagons[k - 1][x - 1]);
                }

                // right forward
                if (k > 0 && m_hexagons[k - 1][x])
                {
                    theHex.SetSibling(HexDir.RightForward, m_hexagons[k - 1][x]);
                }

                // left
                if (x > 0 && m_hexagons[k][x - 1])
                {
                    theHex.SetSibling(HexDir.Left, m_hexagons[k][x - 1]);
                }

                // right
                if (x < m_hexagons.Length - 1 && m_hexagons[k][x + 1])
                {
                    theHex.SetSibling(HexDir.Right, m_hexagons[k][x + 1]);
                }

                // left back
                if (k < m_hexagons.Length - 1 && m_hexagons[k + 1][x])
                {
                    theHex.SetSibling(HexDir.LeftBack, m_hexagons[k + 1][x]);
                }
                
                // right back
                if(k < m_hexagons.Length-1 && x < m_hexagons[k].Length - 1 && m_hexagons[k + 1][x + 1])
                {
                    theHex.SetSibling(HexDir.RightBack, m_hexagons[k + 1][x + 1]);
                }
            }
        }
    }

	// Update is called once per frame
	void Update ()
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

        if(type == HexType.Game)
        {
            result = new GameHexagon(sibling.GetSiblingPos(dirToSibling));
            result.Obj.name = name;
        }
        else if(type == HexType.Solid)
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
    public static implicit operator bool(Hexagon hex)
    {
        return hex != null && hex.Obj && hex.m_enabled;
    }

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

            if(m_heightOffset >= c_maxHeightOffset)
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
            m_frictions.Add(i, Random.Range(0, 0.2f));
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
        where T: Hexagon, new()
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

    
    protected virtual bool IsBalance()
    {
        if(m_siblingHexes.ContainsKey(HexDir.Left) 
            && m_siblingHexes.ContainsKey(HexDir.Right))
        {
            return true;
        }

        if(m_siblingHexes.ContainsKey(HexDir.LeftForward) 
            && m_siblingHexes.ContainsKey(HexDir.RightBack))
        {
            return true;
        }

        if (m_siblingHexes.ContainsKey(HexDir.LeftBack) 
            && m_siblingHexes.ContainsKey(HexDir.RightForward))
        {
            return true;
        }

        if(m_siblingHexes.ContainsKey(HexDir.LeftForward) 
            && m_siblingHexes.ContainsKey(HexDir.Right) 
            && m_siblingHexes.ContainsKey(HexDir.LeftBack))
        {
            return true;
        }

        if(m_siblingHexes.ContainsKey(HexDir.Left)
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
    static HexDir GetNextDir(HexDir dir, bool isClockWise=true)
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
            if(result < HexDir.Left)
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

    private static Color s_whiteColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
    private static Color s_greenColor = new Color(0f, 0.4f, 0, 1.0f);


    public GameHexagon(Vector3 pos) : base(pos)
    {
        Model.GetComponent<CustomCollider>().OnClick += OnClick;
        Model.GetComponent<MeshCollider>().isTrigger = true;

        // random color
        Color randColor = Random.Range(0f, 1f) > 0.5f ? s_whiteColor : s_greenColor;
        Model.GetComponent<Renderer>().material.color = randColor;
    }

    public void OnClick()
    {
        if (Globals.Instance.GameStep == GameStep.SelectingMain)
        {
            Model.GetComponent<Renderer>().material.color = Color.red;
            Globals.Instance.OnSelectMain(this);
        }
        else
        {
            Globals.Instance.OnSelectTarget(this);
            //m_enabled = false;
            //UpdateBalance();
        }
    }

    public override void OnHit(float strength)
    {
        if(strength == 0)
        {
            return;
        }
        HeightOffset += strength * c_maxHeightOffset;

        foreach (KeyValuePair<HexDir, Hexagon> kval in m_siblingHexes)
        {
            if (kval.Value)
            {
                float staticFric_move = (1 - strength) * (1 - strength) * m_frictions[kval.Key]; // simulation
                float dynamicFric_move = m_frictions[kval.Key] * c_maxHeightOffset;
                kval.Value.HeightOffset += (staticFric_move + dynamicFric_move);
            }
        }
    }

    protected override bool IsBalance()
    {
        return base.IsBalance() && m_enabled && m_heightOffset < c_maxHeightOffset;
    }

    public override void UpdateBalance()
    {
        if (!IsBalance())
        {
            foreach (KeyValuePair<HexDir, Hexagon> kval in m_siblingHexes)
            {
                if (kval.Value)
                {
                    kval.Value.RemoveSibling(Hexagon.GetOppositeDir(kval.Key));
                }
            }
            
            Model.AddComponent<Rigidbody>();
            Model.GetComponent<MeshCollider>().isTrigger = false;

            m_siblingHexes.Clear();
            m_enabled = false;
            Destroy();

            Globals.Instance.HexManager.UpdateAllBalance(this);

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
        foreach(HexDir dir in dirs)
        {
            AddOrReplaceSibling(dir, null);
        }
    }
}
