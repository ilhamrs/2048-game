using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Authentication.PlayerAccounts;
using System;
using Unity.Services.Core;

public class ProfileManager : MonoBehaviour
{
    public string playerID;
    public bool isLinkToUnityID = false;
    public Button linkUnityButton;
    public TextMeshProUGUI playerIDText, isLinkToUnityText;
    // Start is called before the first frame update
    void Start()
    {
        RefreshProfile();
    }

    public void RefreshProfile()
    {
        playerID = AuthenticationService.Instance.PlayerId;
        playerIDText.text = playerID;

        isLinkToUnityID = AuthenticationService.Instance.IsSignedIn;
        linkUnityButton.gameObject.SetActive(!isLinkToUnityID);
        isLinkToUnityText.text = isLinkToUnityID ? "linked" : "";
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Home");
    }
    public async void StartUnitySignInAsync()
    {
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            SignInOrLinkWithUnity();
            return;
        }

        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);

        }
    }
    async void SignInOrLinkWithUnity()
    {
        try
        {

            //player blm login samsek
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("signin up with unity player account");
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully signed in with unity player account");

                return;
            }
            //player udh login anonim dan mau link account unity
            if (!HasUnityID())
            {
                Debug.Log("Linking anonymous account to unity...");
                await LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully linked anonymous account");
                return;
            }

            //player udh login anonim dan udh link account unity
            Debug.Log("player is already signed in to their unity player account");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);

        }
    }
    bool HasUnityID()
    {
        return AuthenticationService.Instance.PlayerInfo.GetUnityId() != null;
    }
    private async Task LinkWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
            Debug.Log("Link is successful.");

            RefreshProfile();
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

}