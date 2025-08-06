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

    public void Start5x5()
    {
        SceneManager.LoadScene("5x5Gameplay");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
    public void ShowInterstitialAd()
    {
        InterstitialAd.Instance?.ShowAd();
    }
}
