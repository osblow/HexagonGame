using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineView : MonoBehaviour
{
    public Transform RoomListRoot;
    public GameObject RoomItemTpl;

    public InputField RoomName;
	

    public void OnClickCreate()
    {
        string roomName = RoomName.text;
        Debug.Log("创建房间: " + roomName);

        gameObject.SetActive(false);
        Globals.Instance.CreateGameUI();
    }
}
