using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using TMPro;
using UnityEngine.EventSystems;

public class FightHandler : MonoBehaviour
{
    public int sceneNumber;
    float countTimer = 4;
    public bool fightStarted;
    public GameObject[] fighter;
    public GameObject[] hp;
    public Player[] control = new Player[4];
    public GameObject VictoryPanel;
    public TextMeshProUGUI VictorText; 
    public TextMeshProUGUI countText;
    public bool paused;
    public GameObject pause;
    public int activeFighters;
    public GameObject rematch;
    public float intTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        fightStarted = false;
        Debug.Log("Local Fighters: " + GameController.instance.localPlayers);

        control[0] = ReInput.players.GetPlayer(0);
        control[1] = ReInput.players.GetPlayer(1);
        control[2] = ReInput.players.GetPlayer(2);
        control[3] = ReInput.players.GetPlayer(3);

    }

    // Update is called once per frame
    void Update()
    {
        if (fightStarted)
        {
            if (activeFighters < 2)
            {
                Time.timeScale = 0;
                DisplayVictor();
                intTime += 0.01f;
            }
            if (!GameController.instance.online)
            {
                for (int f = 0; f < GameController.instance.localPlayers; f++)
                {
                    if (control[f].GetButtonDown("Pause"))
                    {
                        PauseGame();
                    }
                    if (paused)
                    {
                        if (control[f].GetButton("Attack") && control[f].GetButton("Special"))
                        {
                            if (control[f].GetButtonDown("Jump"))
                            {
                                MenuReturn(2);
                            }
                        }
                    }
                }
                if (intTime > 0.01f)
                {
                    if (control[0].GetButtonUp("Jump") || control[1].GetButtonUp("Jump") || control[2].GetButtonUp("Jump") || control[3].GetButtonUp("Jump"))
                    {
                        Rematch(sceneNumber);
                    }
                    if (control[0].GetButtonUp("Attack") || control[1].GetButtonUp("Attack") || control[2].GetButtonUp("Attack") || control[3].GetButtonUp("Attack"))
                    {
                        MenuReturn(5);
                    }
                    if (control[0].GetButtonUp("Special") || control[1].GetButtonUp("Special") || control[2].GetButtonUp("Special") || control[3].GetButtonUp("Special"))
                    {
                        MenuReturn(1);
                    }
                }
            }
        }
        if (!fightStarted)
        {
            SpawnFighters();
            countTimer -= Time.deltaTime;
            countText.text = "" + (int)countTimer + "";
            if (countTimer <= 0)
            {
                countText.gameObject.SetActive(false);
                fightStarted = true;
            }
        }
    }

    public void DisplayVictor()
    {
        VictoryPanel.SetActive(true);
        if (fighter[0].GetComponent<PlayerTracker>().hitPoints > 0)
        {
            VictorText.text = "Player 1 Wins!";
        }
        if (fighter[1].GetComponent<PlayerTracker>().hitPoints > 0)
        {
            VictorText.text = "Player 2 Wins!";
        }
        if (fighter[2].GetComponent<PlayerTracker>().hitPoints > 0)
        {
            VictorText.text = "Player 3 Wins!";
        }
        if (fighter[3].GetComponent<PlayerTracker>().hitPoints > 0)
        {
            VictorText.text = "Player 4 Wins!";
        }
        
    }
    public void SpawnFighters()
    {
        for (int f = 0; f < GameController.instance.localPlayers; f++)
        {
            fighter[f].SetActive(true);
            hp[f].SetActive(true);
        }
    }
    public void Rematch(int scene)
    {
        GameController.instance.SceneChange(scene);
    }

    public void MenuReturn(int menu)
    {
        GameController.instance.activeMenu = menu;
        GameController.instance.localPlayers = 0;
        GameController.instance.SceneChange(0);
     
    }

    void PauseGame()
    {
        paused = !paused;
        if (paused)
        {
            pause.SetActive(true);
            Time.timeScale = 0;
        }
        if (!paused)
        {
            pause.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
