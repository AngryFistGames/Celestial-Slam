using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour {

    //load the character select screen
    public void BackToMainMenu()
    {
        //when the button is set up with an On Click event
        SceneManager.LoadScene("MainMenu");
    }
}
