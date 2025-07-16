using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartClassic()
    {
        SceneManager.LoadScene("ClassicGameplay");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
