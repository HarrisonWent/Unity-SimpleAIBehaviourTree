using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC_UI : MonoBehaviour
{
    private InfoPanel MyInfoPanel = null;
    public GameObject InfoPanelPrefab;
    public Color MyPanelColour = Color.red;
    public string MyName = "Unnamed";
    public Image MyFollowUI;

    public void SpawnMyPanel()
    {
        if (!GameObject.Find("Panel(PlayerList)")) 
        { 
            Debug.LogError("Can't find the Player List UI Panel!");
            return; 
        }

        MyInfoPanel = Instantiate(InfoPanelPrefab, GameObject.Find("Panel(PlayerList)").transform).GetComponent<InfoPanel>();
        MyInfoPanel.transform.localScale = Vector3.one;

        MyInfoPanel.MyPanel.color = MyPanelColour;
        MyInfoPanel.MyName.text = MyName;

    }

    public InfoPanel GetMyPanel()
    {
        if (!MyInfoPanel) { Debug.LogError("Info panel not yet set up!"); }

        return MyInfoPanel;
    }

    void Start()
    {
        MyFollowUI.rectTransform.SetParent(GameObject.Find("Panel(GuardUI)").transform);
        MyFollowUI.rectTransform.localScale = Vector3.one;
        MyFollowUI.rectTransform.sizeDelta = Vector2.zero;
        MyFollowUI.rectTransform.localRotation = Quaternion.identity;
    }
    void Update()
    {
        MyFollowUI.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
    }
    
    //Used to update the UI displays
    public enum States
    {
        Chase,
        Investigate,
        Patrol,
        Sleep
    }
    public Sprite Chase, Investigate, Patrol, Sleep;
    public void SetToVisualState(States NewState)
    {
        switch (NewState)
        {
            case States.Chase:
                MyInfoPanel.MyStateIcon.sprite = Chase;
                MyFollowUI.sprite = Chase;
                MyInfoPanel.MyStateName.text = "Current state: Chase";
                break;
            case States.Investigate:
                MyInfoPanel.MyStateIcon.sprite = Investigate;
                MyFollowUI.sprite = Investigate;
                MyInfoPanel.MyStateName.text = "Current state: Investigate";
                break;
            case States.Patrol:
                MyInfoPanel.MyStateIcon.sprite = Patrol;
                MyFollowUI.sprite = Patrol;
                MyInfoPanel.MyStateName.text = "Current state: Patrol";
                break;
            case States.Sleep:
                MyInfoPanel.MyStateIcon.sprite = Sleep;
                MyFollowUI.sprite = Sleep;
                MyInfoPanel.MyStateName.text = "Current state: Sleep";
                break;

        }
    }
}
