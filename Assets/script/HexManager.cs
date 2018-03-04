using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexManager : MonoBehaviour {

    private const string c_hex_res = "model/hex";
    private const float c_sqrt_3 = 1.717f;

    private List<GameObject> m_hexes = new List<GameObject>();


	// Use this for initialization
	void Start () {
        Init();
	}
	
    void Init()
    {
        //Hexagon hex1 = new Hexagon();

        //for (HexDir i = HexDir.Left; i <= HexDir.LeftBack; i++)
        //{
        //    Hexagon hex2 = new Hexagon();
        //    hex2.Obj.transform.position = hex1.GetSiblingPos(i);
        //    hex2.Obj.name = i.ToString();
        //}

        // create walls outside
        List<SolidHexagon> wallHexes = new List<SolidHexagon>();
        wallHexes.Add(new SolidHexagon());
        wallHexes[0].SetInvalidDir(new HexDir[] { HexDir.Left, HexDir.LeftForward, HexDir.RightForward, HexDir.LeftBack });
        for (int i = 1; i < 6; i++)
        {
            SolidHexagon temp_wall = wallHexes[i - 1].CreateSibling(HexDir.Right) as SolidHexagon;
            temp_wall.SetInvalidDir(new HexDir[] { HexDir.RightForward});
            wallHexes.Add(temp_wall);
        }
        wallHexes[5].SetInvalidDir(new HexDir[] { HexDir.Right });

        for (int i = 6; i < 11; i++)
        {
            SolidHexagon temp_wall = wallHexes[i - 1].CreateSibling(HexDir.RightBack) as SolidHexagon;
            temp_wall.SetInvalidDir(new HexDir[] { HexDir.Right});
            wallHexes.Add(temp_wall);
        }
        wallHexes[10].SetInvalidDir(new HexDir[] { HexDir.RightBack });

        for (int i = 11; i < 16; i++)
        {
            SolidHexagon temp_wall = wallHexes[i - 1].CreateSibling(HexDir.LeftBack) as SolidHexagon;
            temp_wall.SetInvalidDir(new HexDir[] { HexDir.RightBack });
            wallHexes.Add(temp_wall);
        }
        wallHexes[15].SetInvalidDir(new HexDir[] { HexDir.LeftBack });

        for (int i = 16; i < 21; i++)
        {
            SolidHexagon temp_wall = wallHexes[i - 1].CreateSibling(HexDir.Left) as SolidHexagon;
            temp_wall.SetInvalidDir(new HexDir[] { HexDir.LeftBack });
            wallHexes.Add(temp_wall);
        }
        wallHexes[20].SetInvalidDir(new HexDir[] { HexDir.Left });

        for (int i = 21; i < 26; i++)
        {
            SolidHexagon temp_wall = wallHexes[i - 1].CreateSibling(HexDir.LeftForward) as SolidHexagon;
            temp_wall.SetInvalidDir(new HexDir[] { HexDir.Left });
            wallHexes.Add(temp_wall);
        }
        wallHexes[25].SetInvalidDir(new HexDir[] { HexDir.LeftForward });

        for (int i = 26; i < 31; i++)
        {
            SolidHexagon temp_wall = wallHexes[i - 1].CreateSibling(HexDir.RightForward) as SolidHexagon;
            temp_wall.SetInvalidDir(new HexDir[] { HexDir.LeftForward });
            wallHexes.Add(temp_wall);
        }
        wallHexes[30].SetInvalidDir(new HexDir[] { HexDir.Left, HexDir.LeftForward, HexDir.RightForward, HexDir.Right });






        for (int i = 0; i < wallHexes.Count; i++)
        {
            wallHexes[i].FillSibling();
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
    public GameObject Obj = null;

    protected Dictionary<HexDir, Hexagon> m_siblingHexes = new Dictionary<HexDir, Hexagon>();
    protected virtual string m_resPath
    {
        get
        {
            return "";
        }
    }


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

    public virtual Hexagon CreateSibling(HexDir dir)
    {
        throw new System.NotImplementedException();
    }

    public virtual bool FillSibling()
    {
        throw new System.NotImplementedException();
    }

    public virtual bool IsBalance()
    {
        if(m_siblingHexes.ContainsKey(HexDir.Left) && m_siblingHexes.ContainsKey(HexDir.Right))
        {
            return true;
        }

        if(m_siblingHexes.ContainsKey(HexDir.LeftForward) && m_siblingHexes.ContainsKey(HexDir.RightBack))
        {
            return true;
        }

        if (m_siblingHexes.ContainsKey(HexDir.LeftBack) && m_siblingHexes.ContainsKey(HexDir.RightForward))
        {
            return true;
        }

        return false;
    }

    public void Destroy()
    {
        if (Obj)
        {
            GameObject.Destroy(Obj);
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

        if (null != hex)
        {
            if (setOpposite)
            {
                hex.AddOrReplaceSibling(GetOppositeDir(dir), this, false, searchAround);
            }
            else if(searchAround)
            {
                SearchSurroundedHex(hex, dir);
            }
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

    static HexDir GetOppositeDir(HexDir dir)
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


    public override Hexagon CreateSibling(HexDir dir)
    {
        GameHexagon hex = new GameHexagon();
        hex.Obj.transform.position = GetSiblingPos(dir);
        AddOrReplaceSibling(dir, hex);

        return hex;
    }

    public override bool FillSibling()
    {
        bool everCreated = false;

        for (HexDir i = HexDir.Left; i <= HexDir.LeftBack; i++)
        {
            if (!m_siblingHexes.ContainsKey(i))
            {
                CreateSibling(i);
                everCreated = true;
            }
        }

        return everCreated;
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


    public override Hexagon CreateSibling(HexDir dir)
    {
        SolidHexagon hex = new SolidHexagon();
        hex.Obj.transform.position = GetSiblingPos(dir);
        AddOrReplaceSibling(dir, hex);

        return hex;
    }

    public override bool IsBalance()
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

    private Hexagon CreateGameSibling(HexDir dir)
    {
        GameHexagon hex = new GameHexagon();
        hex.Obj.transform.position = GetSiblingPos(dir);
        AddOrReplaceSibling(dir, hex);

        return hex;
    }

    public override bool FillSibling()
    {
        for (HexDir i = HexDir.Left; i <= HexDir.LeftBack; i++)
        {
            if (!m_siblingHexes.ContainsKey(i))
            {
                CreateGameSibling(i);
            }
        }

        return true;
    }
}
