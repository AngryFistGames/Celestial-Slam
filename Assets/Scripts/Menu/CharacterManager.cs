using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public int numberOfUsers;
    //a list for all players and player types (AI or another person)
    public List<PlayerBase> players = new List<PlayerBase>();
    //a list for each character
    public List<CharacterBase> characterList = new List<CharacterBase>();
    
    //a function to find the character based on its ID
   public CharacterBase ReturnCharacterWithID(string id)
    {
        //set a variable for the character's ID
        CharacterBase retVal = null;
        //Set the variable to the character's ID based on where it is placed
        for (int i = 0; i < characterList.Count; i++)
        {
            if (string.Equals(characterList[i].charID,id) )
            {
                retVal = characterList[i];
                break;
            }
        }
        //return that number
        return retVal;
    }

    public PlayerBase ReturnPlayerFromStates(StateManager states)
    {
        //set a variable for the player's ID
        PlayerBase retVal = null;
        //Set the variable to the player's ID based the type of player
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerStates == states)
            {
                retVal = players[i];
                break;
            }
        }
        //return that number
        return retVal;
    }

    public static CharacterManager instance;
    public static CharacterManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    

}

[System.Serializable]
public class CharacterBase
{
    public string charID;
    public GameObject prefab;
}

[System.Serializable]
public class PlayerBase
{
    public string playerID;
    public string inputID;
    public PlayerType playerType;
    public bool hasCharacter;
    public GameObject playerPrefab;
    public StateManager playerStates;
    public int score;

    //list the types of players the game can have
    public enum PlayerType
    {
        //A human person
        User,
        //A computer controlled character
        AI,
        //Two computer controlled characters
        Simulation
    }
}
