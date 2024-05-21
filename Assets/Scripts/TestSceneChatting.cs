using MultiplayerHandlers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
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
    [SerializeField]
    List<string> AllChatMessages = new List<string>();

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

    }

    private void OnDataReceived(string data)
    {
        Instantiate(chatTextPrefab, viewPort.transform).text = data;
    }



    private void OnEndEdit(string arg0)
    {
        List<string> MyChatMessages = new List<string>();
        string msg = !string.IsNullOrEmpty(inputField.text) ? inputField.text : inputField.placeholder.GetComponent<Text>().text;
        Handler.SendMessage(msg);
    }

    private void OnDisable()
    {
        inputField.onEndEdit.RemoveListener(OnEndEdit);
        Handler.DataReceived -= OnDataReceived;
    }
    private void FixedUpdate()
    {
        Handler.UpdateLoop();
    }
}
