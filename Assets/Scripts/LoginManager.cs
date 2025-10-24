using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System;
using UnityEngine.SceneManagement;

namespace Unity.Services.Authentication.PlayerAccounts.Samples
{
    public class LoginManager : MonoBehaviour
    {
        private async void Awake()
        {
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

            SceneManager.LoadScene(1);
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
                if (!AuthenticationService.Instance.IsSignedIn) {
                    Debug.Log("signin up with unity player account");
                    await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                    Debug.Log("Successfully signed in with unity player account");
                    return;
                }
                //player udh login dan mau link account unity
                if (!HasUnityID())
                {
                    Debug.Log("Linking anonymous account to unity...");
                    await LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                    Debug.Log("Successfully linked anonymous account");
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
        public async void UnlinkUnity() {
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
}