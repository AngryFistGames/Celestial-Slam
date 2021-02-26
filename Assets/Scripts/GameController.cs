using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance { get; private set; }
    public  playerSelect[] playersInFight = new playerSelect[4];
    public bool online;
    public int players;
    public int localPlayers;
    public int timeLimit;
    public bool isTimed;
    public int activeMenu;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if (SceneManager.GetActiveScene().name != "IntroScreen")
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SceneChange (int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
