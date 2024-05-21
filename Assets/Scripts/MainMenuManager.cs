using MultiplayerHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    public IList<Lobby> lobbies;
    public List<LobbyUIHandler> handlers;
    public static SessionHandler Handler;

    public static Lobby ConnectedLobby { get; private set; }



    private void Awake()
    {

        RefreshLobbyes();
        Handler = new();

    }
    public async void Connect()
    {
        Debug.Log("Connecting");
        var lobby = await Handler.JoinSessionByIdAsync(LobbyUIHandler.SelectedHandler.lobby.Id);
        Debug.Log("Connected to Lobby, Connecting to Server...");

        ConnectedLobby = lobby;
        var code = lobby.LobbyCode;
        Debug.Log("Join Code is: " + code);
        SceneManager.LoadScene(2);
    }
    public async void Create()
    {
        await Handler.CreateSessionAsync("Test", 4);
        SceneManager.LoadScene(2);
    }
    public async void RefreshLobbyes()
    {
        var querrying = await LobbyService.Instance.QueryLobbiesAsync();
        Debug.Log("Lobbies Querried");
        lobbies = querrying.Results;
        for (int i = 0; i < handlers.Count; i++)
        {
            LobbyUIHandler handler = handlers[i];
            if (i < lobbies.Count)
            {
                handler.SetLobby(lobbies[i]);
            }
            else
            {
                handler.SetLobby(null);
            }
        }
    }
}