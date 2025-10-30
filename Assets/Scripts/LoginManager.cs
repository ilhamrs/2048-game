using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication.PlayerAccounts;
using Facebook.Unity;
using EasyPopupSystem;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif


public class LoginManager : MonoBehaviour
{
    private string m_GooglePlayGamesToken;
    private async void Awake()
    {
        InitializeFacebook();

#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
#endif

        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("Services Initializing");
            await UnityServices.InitializeAsync();
        }

        PlayerAccountService.Instance.SignedIn += SignInOrLinkWithUnity;
    }
    // Start is called before the first frame update
    async void Start()
    {
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            Debug.Log("Session token not found");
            return;
        }

        Debug.Log("returning player signing in...");
        await SignInAnonymouslyAsync();

    }

    // Update is called once per frame
    void Update()
    {

    }
    public async void StartAnonymousSignIn()
    {
        await SignInAnonymouslyAsync();
    }
    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            SceneManager.LoadScene(1);

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
    //unity
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
            SceneManager.LoadScene(1);
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
                SceneManager.LoadScene(1);
                return;
            }
            //player udh login dan mau link account unity
            if (!HasUnityID())
            {
                Debug.Log("Linking anonymous account to unity...");
                await LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Successfully linked anonymous account");
                SceneManager.LoadScene(1);
                return;
            }

            //player udh login dan udh link account unity
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
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
            EasyToast.Create("Error", "This user is already linked with another account. Log in instead.", "ToastError", EasyToastPosition.Right, null, 3f, true);
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
    public async void UnlinkUnity()
    {
        try
        {
            Debug.Log("Unlinking unity player account...");
            await AuthenticationService.Instance.UnlinkUnityAsync();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    //facebook
    private void InitializeFacebook()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("failed to initialize the facebook sdk");
        }
    }
    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            //pause
            Time.timeScale = 0;
        }
        else
        {
            //resume
            Time.timeScale = 1;
        }
    }
    public void StartFacebookSignIn()
    {
        // Define the permissions
        var perms = new List<string>() { "public_profile", "email" };

        FB.LogInWithReadPermissions(perms, async result =>
        {
            if (FB.IsLoggedIn)
            {
                var facebookAccessToken = AccessToken.CurrentAccessToken.TokenString;
                Debug.Log($"Facebook Login token: {facebookAccessToken}");

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await SignInWithFacebookAsync(facebookAccessToken);
                }
                else
                {
                    await LinkWithFacebookAsync(facebookAccessToken);
                }

                SceneManager.LoadScene(1);

                // Ambil data profil dari Graph API
                FB.API("/me?fields=name,email", HttpMethod.GET, OnProfileDataReceived);
            }
            else
            {
                Debug.Log("[Facebook Login] User cancelled login");
            }
        });
    }
    async Task SignInWithFacebookAsync(string token)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithFacebookAsync(token);
            Debug.Log("SignIn is successful.");
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
    async Task LinkWithFacebookAsync(string token)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithFacebookAsync(token);
            Debug.Log("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
            EasyToast.Create("Error", "This user is already linked with another account. Log in instead.", "ToastError", EasyToastPosition.Right, null, 3f, true);
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
        return AuthenticationService.Instance.PlayerInfo.GetFacebookId() != null;
    }
    string userName, userEmail, userId;
    private void OnProfileDataReceived(IGraphResult result)
    {
        if (string.IsNullOrEmpty(result.Error))
        {
            var data = result.ResultDictionary;

            if (data.ContainsKey("name"))
                userName = data["name"].ToString();

            if (data.ContainsKey("email"))
                userEmail = data["email"].ToString();

            Debug.Log($"👤 Nama: {userName}");
            Debug.Log($"📧 Email: {userEmail}");
            Debug.Log($"🆔 UserID: {userId}");

            // Ambil foto profil
            // StartCoroutine(GetProfilePicture());
        }
        else
        {
            Debug.LogError("Gagal mengambil data profil Facebook: " + result.Error);
        }
    }
    //google play
#if UNITY_ANDROID
    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    m_GooglePlayGamesToken = code;
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                });
            }
            else
            {
                Debug.Log("Login Unsuccessful");
            }
        });
    }
    public void StartSignInWithGooglePlayGames()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("Not yet authenticated with Google Play Games -- attempting login again");
            LoginGooglePlayGames();
            return;
        }
        SignInOrLinkWithGooglePlayGames();
    }
    private async void SignInOrLinkWithGooglePlayGames()
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogWarning("Authorization code is null or empty!");
            return;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
            SceneManager.LoadScene(1);
        }
        else
        {
            await LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
            SceneManager.LoadScene(1);
        }
    }
    async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("SignIn is successful.");
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
    async Task LinkWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
            Debug.Log("Link is successful.");
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
    
#endif
    public void SignOut()
    {
        Debug.Log("signing out...");
        AuthenticationService.Instance.SignOut();
        PlayerAccountService.Instance.SignOut();
    }
    public void ClearSessionToken()
    {
        Debug.Log("clearing session token...");
        AuthenticationService.Instance.ClearSessionToken();
    }
    public void DeleteAccount()
    {
        Debug.Log("deleting account...");
        AuthenticationService.Instance.DeleteAccountAsync();
    }
}
