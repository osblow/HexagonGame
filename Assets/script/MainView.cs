using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainView : MonoBehaviour
{
	public void OnClickOffline()
    {
        Globals.Instance.IsOnline = false;

        gameObject.SetActive(false);
        Globals.Instance.CreateGameUI();
    }

    public void OnClickOnline()
    {
        Globals.Instance.IsOnline = true;

        gameObject.SetActive(false);
        Globals.Instance.CreateOnlineView();
    }
}
