using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers;
    public List<PlayerInterfaces> p1Interface = new List<PlayerInterfaces>();
    //have all the portraits in an array
    public PortraitInfo[] portraitPrefabs;
    public int maxX;
    public int maxY;
    //the grid to seperate characters
    PortraitInfo[,] charGrid;
    public GameObject portraitCanvas;
    bool loadLevel;
    public bool allPlayersSelected;
    public bool playerSelected;
    CharacterManager charManager;

    #region
    public static SelectScreenManager instance;
    public static SelectScreenManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this; 
    }

    #endregion

    // Use this for initialization
    void Start ()
    {
        //get the reference to the character manager
        charManager = CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;

        //create a new grid
        charGrid = new PortraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        portraitPrefabs = portraitCanvas.GetComponentsInChildren<PortraitInfo>();
        //gain access to all the portraits available
        for (int i =0; i < portraitPrefabs.Length; i++)
        {
            //assign the positions for the portraits
            portraitPrefabs[x].posX += x;
            portraitPrefabs[y].posY += y;

            charGrid[x,y] = portraitPrefabs[i];

            if(x < maxX - 1)
            {
                x++;
            }

            else
            {
                x = 0;
                y++;
            }
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (!loadLevel)
        {
            for (int i =0; i < p1Interface.Count; i++)
            {
                if (i < numberOfPlayers)
                {
                    //if the player presses B or Circle a character has been selected
                    if (Input.GetKeyUp(KeyCode.JoystickButton2 + charManager.players[i].inputID))
                    {
                        p1Interface[i].playerBase.hasCharacter = false;
                    }
                    if (!charManager.players[i].hasCharacter)
                    {
                        p1Interface[i].playerBase = charManager.players[i];

                        HandleSelectorPosition(p1Interface[i]);
                        HandleSelectScreenInput(p1Interface[i], charManager.players[i].inputID);
                        HandleCharacterPreview(p1Interface[i]);
                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;
                }
            }
        }

        if(allPlayersSelected)
        {
            Debug.Log("Loading");
            StartCoroutine("LoadLevel");
            loadLevel = true;
        }

        else
        {
            for (int i =0; i< numberOfPlayers; i++)
            {
                if (charManager.players[i].hasCharacter)
                {
                    playerSelected = true;
                }
                
            }
            allPlayersSelected = true;
        }

	}

    void HandleSelectorPosition(PlayerInterfaces p1)
    {
        p1.selector.SetActive(true);
        //find the active portrait
        p1.activePortrait = charGrid[p1.activeX,p1.activeY];
        //place the selector over the position
        Vector2 selectorPosition = p1.activePortrait.transform.localPosition;
        selectorPosition = selectorPosition + new Vector2(portraitCanvas.transform.localPosition.x, portraitCanvas.transform.localPosition.y);

        p1.selector.transform.localPosition = selectorPosition;
    }

    void HandleSelectScreenInput(PlayerInterfaces p1, string playerID)
    {
        #region
        /*To navigate the grid
         * the active x and y need to change to whichever character is selected
         * */

        float vertical = Input.GetAxis("Vertical" + playerID);

        if (vertical != 0)
        {
            if(!p1.hitInputOnce)
            {
                if (vertical > 0)
                {
                    p1.activeY = (p1.activeY > 0) ? p1.activeY -1 : maxY - 1;
                }
                else
                {
                    p1.activeY = (p1.activeY - 1 < maxY - 1) ?  p1.activeY : 0;
                }
                p1.hitInputOnce = true;
            }
        }
        float horizontal = Input.GetAxis("Horizontal" + playerID);

        if (horizontal != 0)
        {
            if (!p1.hitInputOnce)
            {
                if (horizontal > 0)
                {
                    p1.activeX = (p1.activeX > 0) ? p1.activeX - 1 : maxX - 1;
                }
                else
                {
                    p1.activeX = (p1.activeX - 1 < maxX - 1) ? p1.activeX : 0;
                }
                p1.timerToReset = 0;
                p1.hitInputOnce = true;
            }
        }

        if (vertical == 0 && horizontal == 0)
        {
            p1.hitInputOnce = false;
        }

        if (p1.hitInputOnce)
        {
            p1.timerToReset += Time.deltaTime;

            if (p1.timerToReset > 0.8f)
            {
                p1.hitInputOnce = false;
                p1.timerToReset = 0;
            }
        }
        #endregion
        //if the player presses A or X a character has been selected
        if(Input.GetKeyUp(KeyCode.JoystickButton1 + playerID))
        {
            //play an animation when a character is selected
            //p1.createdCharacter.GetComponentInChildren<Animator>().Play("Something");

            p1.playerBase.playerPrefab = charManager.ReturnCharacterWithID(p1.activePortrait.characterID).prefab;

            p1.playerBase.hasCharacter = true;
        }
        
    }

    void HandleCharacterPreview(PlayerInterfaces p1)
    {
        //if the preview portrait and the active portrait is not the same we need to change the portrait
        if (p1.previewPortrait != p1.activePortrait)
        {
            if (p1.createdCharacter != null)
            {
                Destroy(p1.createdCharacter);
            }
            //create another portrait
            GameObject go = Instantiate
                (CharacterManager.GetInstance().ReturnCharacterWithID(p1.activePortrait.characterID).prefab, p1.charVisPos.position, Quaternion.identity)
                as GameObject;

            p1.createdCharacter = go;

            p1.previewPortrait = p1.activePortrait;

            if (!string.Equals(p1.playerBase.playerID, charManager.players[0].playerID))
            {
                //p1.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }

    IEnumerator LoadLevel()
    {
        //if any of the players are an AI then assign a random character to the prefab
        for(int i =0; i < charManager.players.Count; i++)
        {
            if (charManager.players[i].playerType == PlayerBase.PlayerType.AI)
            {
                if (charManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, portraitPrefabs.Length);

                    charManager.players[i].playerPrefab = charManager.ReturnCharacterWithID(portraitPrefabs[ranValue].characterID).prefab;

                    Debug.Log(portraitPrefabs[ranValue].characterID);
                }
            }
        }
        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("SampleScreen", LoadSceneMode.Single);
    }

    [System.Serializable]
    public class PlayerInterfaces
    {
        //the current active portrait for the player
        public PortraitInfo activePortrait;
        public PortraitInfo previewPortrait;
        //the selected character for the player
        public GameObject selector;
        //the visualization position for player 1
        public Transform charVisPos;
        //the created character for player 1
        public GameObject createdCharacter;
        //the active X and Y entries for the player
        public int activeX;
        public int activeY;
        //smooth out the input
        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;
    }
}
