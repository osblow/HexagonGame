using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexManager : MonoBehaviour
{
    public static HexManager Instance { get { return s_instance; } }
    private static HexManager s_instance;

    private void Awake()
    {
        s_instance = this;
    }




    public bool HasTarget = false;

    private const string c_hex_res = "model/hex";
    private const float c_sqrt_3 = 1.717f;

    private Hexagon[][] m_hexagons;



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
        m_hexagons[i][j] = new SolidHexagon();
        for (; i < m_hexagons.Length; i++)
        {
            j = columnOffsetForRaws[i];
            if (i > 0)
            {
                m_hexagons[i][j] = m_hexagons[i-1][j].CreateSibling<SolidHexagon>(HexDir.LeftBack, i + "-" + j);
            }
            ++j;

            for (; j < columnOffsetForRaws[i] + columnForRaws[i]; j++)
            {
                if (i == 0 || i == m_hexagons.Length-1 || j == 0 || j == columnOffsetForRaws[i]+ columnForRaws[i] - 1)
                {
                    m_hexagons[i][j] = m_hexagons[i][j - 1].CreateSibling<SolidHexagon>(HexDir.Right, i + "-" + j);
                }
                else
                {
                    m_hexagons[i][j] = m_hexagons[i][j - 1].CreateSibling<GameHexagon>(HexDir.Right, i + "-" + j);
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



    public GameObject Obj = null;

    protected Dictionary<HexDir, Hexagon> m_siblingHexes = new Dictionary<HexDir, Hexagon>();
    protected virtual string m_resPath
    {
        get
        {
            return "";
        }
    }

    protected bool m_enabled = true;

    public Hexagon()
    {
        Obj = GameObject.Instantiate(Resources.Load(m_resPath) as GameObject);
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

    public virtual Hexagon CreateSibling<T>(HexDir dir, string index_name)
        where T: Hexagon, new()
    {
        T t = new T();
        t.Obj.transform.position = GetSiblingPos(dir);
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

    private void SearchSurroundedHex(Hexagon sibling, HexDir dir)
    {
        Hexagon cur = sibling;
        HexDir curDir = GetNextDir(GetOppositeDir(dir), false);
        HexDir startDir = GetNextDir(curDir, false);
        

        int limit = 10;
        while(curDir != startDir && limit-- > 0)
        {
            if (null == cur || !cur.m_siblingHexes.ContainsKey(curDir))// || null == cur.m_siblingHexes[curDir])
            {
                break;
            }

            if (m_siblingHexes.ContainsKey(curDir))
            {
                continue;
            }

            HexDir theDir = GetOppositeDir(GetNextDir(GetNextDir(curDir)));
            AddOrReplaceSibling(theDir, cur.m_siblingHexes[curDir], true, false);

            cur = cur.m_siblingHexes[curDir];
            curDir = GetNextDir(curDir);
        }
    }

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

    private GameObject m_model;


    public GameHexagon() : base()
    {
        m_model = Obj.transform.Find("hexagon/Cylinder").gameObject;
        m_model.GetComponent<CustomCollider>().OnClick += OnClick;
        m_model.GetComponent<MeshCollider>().isTrigger = true;

        // random color
        Color randColor = Random.Range(0f, 1f) > 0.5f ? Color.white * 0.7f : Color.green * 0.4f;
        m_model.GetComponent<Renderer>().material.color = randColor;
    }

    public void OnClick()
    {
        if (!HexManager.Instance.HasTarget)
        {
            m_model.GetComponent<Renderer>().material.color = Color.red;
            HexManager.Instance.HasTarget = true;
        }
        else
        {
            m_enabled = false;
            UpdateBalance();
        }
    }

    protected override bool IsBalance()
    {
        return base.IsBalance() && m_enabled;
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
            
            m_model.AddComponent<Rigidbody>();
            m_model.GetComponent<MeshCollider>().isTrigger = false;

            m_siblingHexes.Clear();
            m_enabled = false;
            Destroy();

            HexManager.Instance.UpdateAllBalance(this);
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
