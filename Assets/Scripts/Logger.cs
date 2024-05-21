using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Logger : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField inputField;
    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("Unity Services Initialized");
        AuthenticationService.Instance.SignedIn += FinishLogging;
    }

    private async void FinishLogging()
    {
        var name = inputField.text;
        if (string.IsNullOrEmpty(name))
        {
            name = inputField.placeholder.GetComponent<TMPro.TMP_Text>().text;
        }
        await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
        AuthenticationService.Instance.SignedIn -= FinishLogging;
        //Print Player data
        Debug.Log(AuthenticationService.Instance.PlayerId);
        Debug.Log("User signed in");
        Debug.Log($"Name: {AuthenticationService.Instance.PlayerName}, ID: {AuthenticationService.Instance.PlayerId}");
        SceneManager.LoadScene("Main Menu");
    }

    public async void Connect()
    {
        var auth = AuthenticationService.Instance;
        if (auth.IsSignedIn) return;
        await auth.SignInAnonymouslyAsync();
    }
}
