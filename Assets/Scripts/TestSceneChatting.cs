using MultiplayerHandlers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestSceneChatting : MonoBehaviour
{
    [SerializeField] GameObject viewPort;
    [SerializeField] TMPro.TMP_InputField inputField;
    [SerializeField] TMPro.TMP_Text chatTextPrefab;
    [SerializeField] PlayerController playerControllerPrefab;
    [SerializeField] List<PlayerController> Players;

    private Lobby CurrentSession;
    private SessionHandler Handler;

    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented
    };

    private void OnEnable()
    {
        inputField.onEndEdit.AddListener(OnEndEdit);
        Handler = MainMenuManager.Handler;
        Handler.StartHeartbeat(this, 5f);
        CurrentSession = Handler.ConnectedLobby;
        Handler.DataReceived += OnDataReceived;
        Handler.PlayerConnected += OnPlayerConnected;
        if (Handler.IsHost)
        {
            NewPlayer(AuthenticationService.Instance.PlayerName);
        }
    }

    private void OnPlayerConnected()
    {
        Handler.SendMessage(new(AuthenticationService.Instance.PlayerName, "PlayerJoinedToServer"));
    }

    private void OnDataReceived(NetworkMessage data)
    {
        switch (data.MessageType)
        {
            case "Chat":
                HandleChatMessage(data);
                break;
            case "PlayerJoined":
                HandlePlayerJoined(data);
                break;
            case "PlayerLeft":
                HandlePlayerLeft(data);
                break;
            case "PlayerPosition":
                HandlePlayerPosition(data);
                break;
            case "PlayerJoinedToServer":
                HandlePlayerServerJoin(data);
                break;
            default:
                Debug.Log("Unknown message type: " + data.MessageType);
                break;
        }
    }

    private void HandlePlayerServerJoin(NetworkMessage data)
    {
        if (!Handler.IsHost) return;
        string _name = data.PlayerName;
        NewPlayer(_name);
        List<string> playerNames = Players.Select(p => p.PlayerName).ToList();
        var message = new NetworkMessage(JsonConvert.SerializeObject(playerNames, settings), "PlayerJoined");
        Handler.SendMessage(message);
    }
    public void NewPlayer(string _name)
    {
        PlayerController player = Instantiate(playerControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player.name = _name;
        player.PlayerName = _name;
        player.isThisPlayer = (_name == AuthenticationService.Instance.PlayerName);
        Players.Add(player);
    }
    private void HandlePlayerPosition(NetworkMessage data)
    {
        string name = data.PlayerName;
        Vector3 position = JsonConvert.DeserializeObject<Vector3>(data.data as string, settings);
        PlayerController player = Players.FirstOrDefault(p => p.PlayerName == name);
        if (player != null)
        {
            player.transform.position = position;
        }
    }

    private void HandlePlayerLeft(NetworkMessage data)
    {
        string name = data.PlayerName;
        PlayerController player = Players.FirstOrDefault(p => p.PlayerName == name);
        if (player != null)
        {
            Players.Remove(player);
            Destroy(player.gameObject);
        }
    }

    private void HandlePlayerJoined(NetworkMessage data)
    {
        var playerNames = JsonConvert.DeserializeObject<List<string>>(data.data as string, settings);
        foreach (var name in playerNames)
        {
            if (Players.Any(p => p.name == name))
            {
                continue;
            }
            NewPlayer(name);
        }
    }

    public void HandleChatMessage(NetworkMessage data)
    {
        string name = data.PlayerName;
        Instantiate(chatTextPrefab, viewPort.transform).text = name + ": " + data.data as string;
    }

    private void OnEndEdit(string arg0)
    {
        List<string> MyChatMessages = new List<string>();
        string msg = !string.IsNullOrEmpty(inputField.text) ? inputField.text : inputField.placeholder.GetComponent<Text>().text;
        Handler.SendMessage(new(msg, "Chat"));
    }

    private void OnDisable()
    {
        Handler.SendMessage(new("I have left the lobby", "PlayerLeft"));
        inputField.onEndEdit.RemoveListener(OnEndEdit);
        Handler.DataReceived -= OnDataReceived;
        Handler.Dispose();
    }
    private void FixedUpdate()
    {
        //if (Time.fixedTime % 0.1f < 0.08f) return;
        UpdatePlayerPosition();
        Handler.UpdateLoop();
    }
    /// <summary>
    /// Sends message about player position to the server
    /// </summary>
    private void UpdatePlayerPosition()
    {
        PlayerController player = Players.FirstOrDefault(p => p.isThisPlayer);
        if (player != null)
        {
            Handler.SendMessage(new(JsonConvert.SerializeObject(player.transform.position, settings), "PlayerPosition"));
        }
    }
}
