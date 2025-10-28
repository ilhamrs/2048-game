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
using Facebook.Unity;

public class ProfileManager : MonoBehaviour
{
    public string playerID;
    public bool isLinkToUnityID = false, isLinkToFacebook = false;
    public Button linkUnityButton, linkFbButton;
    public TextMeshProUGUI playerIDText, isLinkToUnityText, playerFbEmail;
    // Start is called before the first frame update
    void Start()
    {
        RefreshProfile();
    }

    public async Task RefreshProfile()
    {
        await checkAccountAsync();

        playerID = AuthenticationService.Instance.PlayerId;
        playerIDText.text = playerID;

        isLinkToUnityID = AuthenticationService.Instance.IsSignedIn;
        linkUnityButton.gameObject.SetActive(!isLinkToUnityID);
        isLinkToUnityText.text = isLinkToUnityID ? "linked" : "";

        // isLinkToFacebook = HasFacebook();
        linkFbButton.gameObject.SetActive(!isLinkToFacebook);
        if (isLinkToFacebook)
        {
            var profile = FB.Mobile.CurrentProfile();
            playerFbEmail.text = profile.Email;

            FB.API("/me?fields=name,email", HttpMethod.GET, OnProfileDataReceived);
        }
        else
        {
            playerFbEmail.text = "";
        }
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
    bool HasFacebook()
    {
        return FB.IsLoggedIn;
    }
    async Task checkAccountAsync()
    {
        try
        {
            // Get the PlayerInfo object for the currently signed-in player.
            PlayerInfo playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();

            // The list of linked accounts is stored in the Identities property.
            List<Identity> linkedIdentities = playerInfo.Identities;

            Debug.Log($"Player ID: {playerInfo.Id}");
            Debug.Log($"Linked Accounts Found: {linkedIdentities.Count}");

            foreach (var identity in linkedIdentities)
            {
                Debug.Log($"Provider: {identity.TypeId}, External ID: {identity.UserId}");
                if (identity.TypeId == "facebook" || identity.TypeId == "facebook.com")
                {
                    isLinkToFacebook = true;
                }
            }
        }
        catch (AuthenticationException ex)
        {
            // Handle authentication-specific errors, like the player not being signed in.
            Debug.LogError($"Authentication error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle other potential exceptions.
            Debug.LogError($"General error: {ex.Message}");
        }
    }
    string userName, userEmail, userId;
    Texture2D profilePicture;
    private void OnProfileDataReceived(IGraphResult result)
    {
        if (string.IsNullOrEmpty(result.Error))
        {
            var data = result.ResultDictionary;

            if (data.ContainsKey("name"))
                userName = data["name"].ToString();

            if (data.ContainsKey("email"))
                userEmail = data["email"].ToString();

            Debug.Log($"Nama: {userName}");
            Debug.Log($"Email: {userEmail}");
            Debug.Log($"UserID: {userId}");

            playerFbEmail.text = userEmail;

            // Ambil foto profil
            // StartCoroutine(GetProfilePicture());
        }
        else
        {
            Debug.LogError("Gagal mengambil data profil Facebook: " + result.Error);
        }
    }
    private IEnumerator GetProfilePicture()
    {
        string url = $"https://graph.facebook.com/{userId}/picture?type=large";
        WWW www = new WWW(url);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            profilePicture = www.texture;
            Debug.Log("🖼️ Foto profil berhasil diambil!");

            // Contoh: tampilkan di UI (kalau ada Image component)
            // GetComponent<UnityEngine.UI.RawImage>().texture = profilePicture;
        }
        else
        {
            Debug.LogError("Gagal mengambil foto profil: " + www.error);
        }
    }
}