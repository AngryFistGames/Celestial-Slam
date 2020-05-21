using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    public GameObject startText;
    float timer;
    bool loadingLevel;
    bool init;

    public int activeElement;
    public GameObject menuObj;
    public ButtonRef[] menuOptions;

    // Start is called before the first frame update
    void Start()
    {
        menuObj.SetActive(false);
        GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        //add a flickerin effect to the start screen
         timer += Time.deltaTime;
         if (timer > 0.6f)
         {
           timer = 0;
           startText.SetActive(!startText.activeInHierarchy);
         }
         */
         //If the player press the Start or Option button
         if (Input.GetKeyUp(KeyCode.JoystickButton9) || Input.GetKeyUp(KeyCode.JoystickButton1) || Input.GetKeyUp(KeyCode.Space))
        {
            init = true;
            startText.SetActive(false);
            menuObj.SetActive(true);
        }
         else
        {
            if(!loadingLevel)
            {
               // menuOptions[activeElement].selected = true;
                //if the player moves the left stick to one of the options
                if (Input.GetKeyUp(KeyCode.JoystickButton10))
                {
                    menuOptions[activeElement].selected = false;

                    if(activeElement >  0)
                    {
                        activeElement--;
                    }

                    else
                    {
                        activeElement = menuOptions.Length - 1;
                    }
                }
                if (Input.GetKeyUp(KeyCode.JoystickButton10))
                {
                    menuOptions[activeElement].selected = false;
                    if(activeElement < menuOptions.Length - 1)
                    {
                        activeElement++;
                    }
                    else
                    {
                        activeElement = 0;
                    }
                }
                //if the player uses the Start or Options button load the character select level 
                if (Input.GetKeyUp(KeyCode.JoystickButton9))
                {
                    Debug.Log("Select your character");
                    loadingLevel = true;
                    StartCoroutine("LoadLevel");
                    menuOptions[activeElement].transform.localScale *= 1.2f;
                }
            }
        }
         //quit the game if the PS button is pressed
         if (Input.GetKeyUp(KeyCode.JoystickButton12))
        {
            Application.Quit();
        }
    }

    void HandleSelectedOption()
    {
        switch(activeElement)
        {
            case 0:
                CharacterManager.GetInstance().numberOfUsers = 1;
                break;
            case 1:
                CharacterManager.GetInstance().numberOfUsers = 2;
                CharacterManager.GetInstance().players[1].playerType = PlayerBase.PlayerType.User;
                break;
            //may need to change this
            
            case 2:
                CharacterManager.GetInstance().numberOfUsers = 3;
                CharacterManager.GetInstance().players[1].playerType = PlayerBase.PlayerType.User;
                CharacterManager.GetInstance().players[2].playerType = PlayerBase.PlayerType.User;
                break;
            case 3:
                CharacterManager.GetInstance().numberOfUsers = 4;
                CharacterManager.GetInstance().players[1].playerType = PlayerBase.PlayerType.User;
                CharacterManager.GetInstance().players[2].playerType = PlayerBase.PlayerType.User;
                CharacterManager.GetInstance().players[3].playerType = PlayerBase.PlayerType.User;
                CharacterManager.GetInstance().players[4].playerType = PlayerBase.PlayerType.User;
                break;
                
       }
    }

    IEnumerator LoadLevel()
    {
        HandleSelectedOption();
        yield return new WaitForSeconds(0.6f);
        //SceneManager.LoadSceneAsync("SampleSelectScreen",LoadSceneMode.Single);
        GetComponent<GameManager>();
    }

}
