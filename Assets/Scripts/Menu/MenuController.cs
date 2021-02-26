using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.EventSystems;

public class playerSelect
{
    public bool isFighting = false;
    public bool isCPU = false;
    public int difficulty = 0;
    public int fighterSelected = 0;
    public bool ready = false;
}
public class MenuController : MonoBehaviour
{
    public int buttonSelected;
    public int activeMenu = 0;
    public GameObject[] menus;
    public GameObject[] mainButtons;
    public GameObject[] multiplayerButtons;
    public GameObject[] options;
    public GameObject[] fighterButtons;
    public GameObject[] playerSelector;
    Player[] control = new Player[4];
    public Text matchTime;
    public Text maxPlayers;
    float scrollLag = .3f;
    [SerializeField] float[] lag = new float [4];
    float backTimer;
    public GameObject onlineControl;
    public GameObject readyToFight;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        control[0] = ReInput.players.GetPlayer(0);
        control[1] = ReInput.players.GetPlayer(1);
        control[2] = ReInput.players.GetPlayer(2);
        control[3] = ReInput.players.GetPlayer(3);
        buttonSelected = 0;
        GameController.instance.playersInFight = new playerSelect[4];
        GameController.instance.playersInFight[0] = new playerSelect();
        GameController.instance.playersInFight[1] = new playerSelect();
        GameController.instance.playersInFight[2] = new playerSelect();
        GameController.instance.playersInFight[3] = new playerSelect();
        /*

        GameController.instance.playersInFight[0].difficulty = 0;
        GameController.instance.playersInFight[1].difficulty = 0;
        GameController.instance.playersInFight[2].difficulty = 0;
        GameController.instance.playersInFight[3].difficulty = 0;

        GameController.instance.playersInFight[0].ready = false;
        GameController.instance.playersInFight[1].ready = false;
        GameController.instance.playersInFight[2].ready = false;
        GameController.instance.playersInFight[3].ready = false;

        GameController.instance.playersInFight[0].isCPU = false;
        GameController.instance.playersInFight[1].isCPU = false;
        GameController.instance.playersInFight[2].isCPU = false;
        GameController.instance.playersInFight[3].isCPU = false;

        GameController.instance.playersInFight[0].isFighting = false;
        GameController.instance.playersInFight[1].isFighting = false; 
        GameController.instance.playersInFight[2].isFighting = false;
        GameController.instance.playersInFight[3].isFighting = false;
        */
        lag[0] = 0f;
        lag[1] = 0f;
        lag[2] = 0f;
        lag[3] = 0f;
        ChangeMenu(GameController.instance.activeMenu);
    }

    // Update is called once per frame
    void Update()
    {
        for (int l = 0; l < 4; l++)
        {
            lag[l] += Time.deltaTime;
        }
      
        switch (activeMenu)
        {
            case 0:
                if (control[0].GetButtonUp("UISubmit") || control[1].GetButtonUp("UISubmit") || control[2].GetButtonUp("UISubmit") || control[3].GetButtonUp("UISubmit"))
                {
                    ChangeMenu(1);
                }
                break;
            case 1:
                if (control[0].GetButtonUp("UICancel") || control[1].GetButtonUp("UICancel") || control[2].GetButtonUp("UICancel") || control[3].GetButtonUp("UICancel"))
                {
                    Debug.Log("Button Pressed");
                    ChangeMenu(0);
                }
                break;
            case 2:
                if (control[0].GetButtonUp("UICancel") || control[1].GetButtonUp("UICancel") || control[2].GetButtonUp("UICancel") || control[3].GetButtonUp("UICancel"))
                {
                    ChangeMenu(1);
                }
                break;
            case 3:
                if (control[0].GetButtonUp("UICancel") || control[1].GetButtonUp("UICancel") || control[2].GetButtonUp("UICancel") || control[3].GetButtonUp("UICancel"))
                {
                    ChangeMenu(1);
                }
                if (EventSystem.current.currentSelectedGameObject == options[0])
                {
                    if ((control[0].GetAxis("UIHorizontal") > 0.7 && lag[0] > scrollLag) || (control[1].GetAxis("UIHorizontal") > 0.7 && lag[1] > scrollLag) || (control[2].GetAxis("UIHorizontal") > 0.7 && lag[2] > scrollLag) || (control[3].GetAxis("UIHorizontal") > 0.7 && lag[3] > scrollLag))
                    {
                        GameController.instance.timeLimit += 30;
                        if (GameController.instance.timeLimit > 990)
                        {
                            GameController.instance.timeLimit = 0;
                        }
                        for (int l = 0; l < 4; l++)
                        {
                            lag[l] = 0;
                        }
                    }
                   if ((control[0].GetAxis("UIHorizontal") < -0.7 && lag[0] > scrollLag) || (control[1].GetAxis("UIHorizontal") < -0.7 && lag[1] > scrollLag) || (control[2].GetAxis("UIHorizontal") < -0.7 && lag[2] > scrollLag) || (control[3].GetAxis("UIHorizontal") < -0.7 && lag[3] > scrollLag))

                        {
                            GameController.instance.timeLimit -= 30;
                        if (GameController.instance.timeLimit  < 0)
                        {
                            GameController.instance.timeLimit = 990;
                        }
                        for (int l = 0; l < 4; l++)
                        {
                            lag[l] = 0;
                        }
                    }
                    if (GameController.instance.timeLimit == 0)
                    {
                        GameController.instance.isTimed = false;
                        matchTime.text = "No Time Limit";
                    }
                    else
                    {
                        GameController.instance.isTimed = true;
                        matchTime.text = GameController.instance.timeLimit.ToString();
                    }
                }
                if (EventSystem.current.currentSelectedGameObject == options[1])
                {
                    if ((control[0].GetAxis("UIHorizontal") > 0.7 && lag[0] > scrollLag) || (control[1].GetAxis("UIHorizontal") > 0.7 && lag[1] > scrollLag) || (control[2].GetAxis("UIHorizontal") > 0.7 && lag[2] > scrollLag) || (control[3].GetAxis("UIHorizontal") > 0.7 && lag[3] > scrollLag))
                    {
                        GameController.instance.players++;
                        if (GameController.instance.players > 4)
                        {
                            GameController.instance.players = 2;
                        }
                        for (int l = 0; l < 4; l++)
                        {
                            lag[l] = 0;
                        }
                    }

                    if ((control[0].GetAxis("UIHorizontal") < -0.7 && lag[0] > scrollLag) || (control[1].GetAxis("UIHorizontal") < -0.7 && lag[1] > scrollLag) || (control[2].GetAxis("UIHorizontal") < -0.7 && lag[2] > scrollLag) || (control[3].GetAxis("UIHorizontal") < -0.7 && lag[3] > scrollLag))
                    {
                        GameController.instance.players--;
                        if (GameController.instance.players < 2)
                        {
                            GameController.instance.players = 4;
                        }
                        for (int l = 0; l < 4; l++)
                        {
                            lag[l] = 0;
                        }
                    }
                    maxPlayers.text = GameController.instance.players.ToString();
                }
                    break;
            case 4:
                if (control[0].GetButtonUp("UICancel") || control[1].GetButtonUp("UICancel") || control[2].GetButtonUp("UICancel") || control[3].GetButtonUp("UICancel"))
                {
                    ChangeMenu(2);
                }
                break;
            case 5:
                if (control[0].GetButton("UICancel") || control[1].GetButton("UICancel") || control[2].GetButton("UICancel") || control[3].GetButton("UICancel"))
                {
                    backTimer += Time.deltaTime;
                }
                if (backTimer > 3)
                {
                    ChangeMenu(2);
                    playerSelector[0].SetActive(false);
                    playerSelector[1].SetActive(false);
                    playerSelector[2].SetActive(false);
                    playerSelector[3].SetActive(false);
                    GameController.instance.playersInFight[0].isFighting = false;
                    GameController.instance.playersInFight[1].isFighting = false;
                    GameController.instance.playersInFight[2].isFighting = false;
                    GameController.instance.playersInFight[3].isFighting = false;
                    if (GameController.instance.online) {
                        onlineControl.GetComponentInChildren<NetworkController>().Disconnect();
                        GameController.instance.online = false;
                    }
                    backTimer = 0;
                }
                if (GameController.instance.online == false)
                {
                    for (int p = 0; p < 4; p++)
                    {
                        if ((GameController.instance.localPlayers >= 2) && GameController.instance.playersInFight[0].isFighting == GameController.instance.playersInFight[0].ready && GameController.instance.playersInFight[1].isFighting == GameController.instance.playersInFight[1].ready && GameController.instance.playersInFight[2].isFighting == GameController.instance.playersInFight[2].ready && GameController.instance.playersInFight[3].isFighting == GameController.instance.playersInFight[3].ready)
                        {
                            readyToFight.SetActive(true);
                            lag[p] = 0;
                        }
                        else
                        {
                            readyToFight.SetActive(false);
                        }
                        if (control[0].GetButtonDown("UISubmit") || control[1].GetButtonDown("UISubmit") || control[2].GetButtonDown("UISubmit") || control[3].GetButtonDown("UISubmit"))
                            {
                            if ((GameController.instance.localPlayers >= 2) && GameController.instance.playersInFight[0].isFighting == GameController.instance.playersInFight[0].ready && GameController.instance.playersInFight[1].isFighting == GameController.instance.playersInFight[1].ready && GameController.instance.playersInFight[2].isFighting == GameController.instance.playersInFight[2].ready && GameController.instance.playersInFight[3].isFighting == GameController.instance.playersInFight[3].ready)
                                GameController.instance.SceneChange(1);
                            }
                        
                       
                        if (control[p].GetButtonDown("UISubmit"))
                        {
                            if (GameController.instance.playersInFight[p] != null)
                            {
                                if (GameController.instance.playersInFight[p].isFighting)
                                {
                                    GameController.instance.playersInFight[p].ready = true;
                                    playerSelector[p].GetComponent<Button>().interactable = false;
                                    Debug.Log("Fighter Selected!");
                                }
                            }
                            if (!GameController.instance.playersInFight[p].isFighting)
                            {
                                playerSelector[p].SetActive(true);
                                playerSelector[p].transform.position = fighterButtons[GameController.instance.playersInFight[p].fighterSelected].transform.position;

                                GameController.instance.playersInFight[p].isFighting = true;
                                GameController.instance.localPlayers += 1;
                            }
                        }

                         if (control[p].GetButtonDown("UICancel"))
                          {
                                if (!GameController.instance.playersInFight[p].ready)
                                {
                                    playerSelector[p].SetActive(false);
                                    GameController.instance.playersInFight[p].isFighting = false;
                                GameController.instance.localPlayers -= 1;
                            }
                                else
                                {
                                    GameController.instance.playersInFight[p].ready = false;
                                playerSelector[p].GetComponent<Button>().interactable = true;
                            }
                          }

                            if ((control[p].GetAxis("UIHorizontal") >= 0.5f) && (GameController.instance.playersInFight[p].isFighting) && (lag[p] >= scrollLag) && (GameController.instance.playersInFight[p].ready == false))
                            {
                                GameController.instance.playersInFight[p].fighterSelected++;
                            Debug.Log(GameController.instance.playersInFight[p].fighterSelected.ToString());
                                if (GameController.instance.playersInFight[p].fighterSelected > 11)
                                {
                                    GameController.instance.playersInFight[p].fighterSelected = 0;
                                }
                            lag[p] = 0;
                                playerSelector[p].transform.position = fighterButtons[GameController.instance.playersInFight[p].fighterSelected].transform.position;
                            }
                            if ((control[p].GetAxis("UIHorizontal") <= -0.5f) && (GameController.instance.playersInFight[p].isFighting) && (lag[p] >= scrollLag) && (!GameController.instance.playersInFight[p].ready))
                            {
                                GameController.instance.playersInFight[p].fighterSelected--;
                                if (GameController.instance.playersInFight[p].fighterSelected < 0)
                                {
                                    GameController.instance.playersInFight[p].fighterSelected = 11;
                            }
                            lag[p] = 0;

                            playerSelector[p].transform.position = fighterButtons[GameController.instance.playersInFight[p].fighterSelected].transform.position;
                            }

                            if ((control[p].GetAxis("UIVertical") <= -0.5f) && (GameController.instance.playersInFight[p].isFighting) && (lag[p] >= scrollLag) && (!GameController.instance.playersInFight[p].ready))
                            {
                                if (GameController.instance.playersInFight[p].fighterSelected <= 7)
                                {
                                    GameController.instance.playersInFight[p].fighterSelected += 4;
                                }
                                else
                                {
                                    GameController.instance.playersInFight[p].fighterSelected = (GameController.instance.playersInFight[p].fighterSelected - 8);
                            }
                            lag[p] = 0;

                                playerSelector[p].transform.position = fighterButtons[GameController.instance.playersInFight[p].fighterSelected].transform.position;
                            }
                            if ((control[p].GetAxis("UIVertical") >= 0.5f) && (GameController.instance.playersInFight[p].isFighting) && (lag[p] >= scrollLag) && (!GameController.instance.playersInFight[p].ready))
                            {
                                if (GameController.instance.playersInFight[p].fighterSelected >= 4)
                                {
                                    GameController.instance.playersInFight[p].fighterSelected -= 4;
                                }
                                else
                                {
                                    GameController.instance.playersInFight[p].fighterSelected = (GameController.instance.playersInFight[p].fighterSelected + 7);
                            }
                            lag[p] = 0;

                            playerSelector[p].transform.position = fighterButtons[GameController.instance.playersInFight[p].fighterSelected].transform.position;
                            }
                        playerSelector[p].transform.position = fighterButtons[GameController.instance.playersInFight[p].fighterSelected].transform.position;
                    }
                   
                            }
                break;
        }
        onlineControl.SetActive(GameController.instance.online);
    }

  
    public void SetMode(bool online)
    {
        GameController.instance.online = online;
    }
    
    public void ChangeMenu(int menu)
    {
        activeMenu = menu;
        buttonSelected = 0;
        for (int m = 0; m < menus.Length; m++)
        {
            if (m != activeMenu)
            {
                menus[m].SetActive(false);
            }
            else
            {
                menus[m].SetActive(true);
            }
        }

        EventSystem.current.SetSelectedGameObject(null);
        switch (activeMenu)
        {
            case 1:
         EventSystem.current.SetSelectedGameObject(mainButtons[0]);
                break;
            case 2:
        EventSystem.current.SetSelectedGameObject(multiplayerButtons[0]);
                break;
            case 3:
                EventSystem.current.SetSelectedGameObject(options[0]);
                break;
            case 5:
                for (int p = 0; p < 4; p++)
                {
                    GameController.instance.playersInFight[p].isFighting = false;
                    GameController.instance.playersInFight[p].ready = false;
                }
                    break;
            default:
                EventSystem.current.SetSelectedGameObject(null);
                break;
        } 
}
}
