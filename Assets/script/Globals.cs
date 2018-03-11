using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GameStep
{
    NotStart = 0,
    SelectingTarget = 1,
    Start = 2,
    SelectingStrength = 3,
    Hitting = 4,
}


public class Globals : MonoBehaviour
{
    public static Globals Instance { get { return s_instance; } }
    private static Globals s_instance;

    private void Awake()
    {
        s_instance = this;
    }

    public Canvas Canvas;
    public GameStep GameStep;
    public HexManager HexManager;
    public GameView GameView;

    // Use this for initialization
    void Start ()
    {
        Init();
	}

    private void Init()
    {
        GameStep = GameStep.NotStart;
        HexManager = CreateInstance<HexManager>();
        CreateGameUI();
    }
	
    private T CreateInstance<T>()
        where T : MonoBehaviour
    {
        T t;
        GameObject obj = new GameObject();
        obj.transform.SetParent(transform);
        t = obj.AddComponent<T>();
        t.name = typeof(T).ToString();

        return t;
    }

    private void CreateGameUI()
    {
        GameObject uiObj = Instantiate(Resources.Load("ui/GameView") as GameObject);
        uiObj.transform.SetParent(Canvas.transform, false);
    }

	// Update is called once per frame
	void Update ()
    {
		
	}
}
