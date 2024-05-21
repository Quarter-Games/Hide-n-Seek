using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUIHandler : MonoBehaviour
{
    public static LobbyUIHandler SelectedHandler { get; private set; }
    [SerializeField] private TMPro.TextMeshProUGUI lobbyName;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyPlayers;
    [SerializeField] private TMPro.TextMeshProUGUI lobbySlots;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyCode;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyVersion;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyEnvironment;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyCreatedAt;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyUpdatedAt;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyMetadata;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyPrivate;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyMaxPlayers;
    [SerializeField] private TMPro.TextMeshProUGUI lobbyId;
    public Lobby lobby;
    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        if (lobby == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        lobbyName.text = lobby.Name;
        lobbyPlayers.text = (lobby.MaxPlayers - lobby.AvailableSlots).ToString();
        lobbySlots.text = lobby.AvailableSlots.ToString();
        lobbyCode.text = lobby.Id;
        lobbyVersion.text = "Delete This";
        lobbyEnvironment.text = "Delete This";
        lobbyCreatedAt.text = lobby.Created.ToString();
        lobbyUpdatedAt.text = lobby.LastUpdated.ToString();
        lobbyMetadata.text = "Delete This";
        lobbyPrivate.text = lobby.IsLocked.ToString();
        lobbyMaxPlayers.text = lobby.MaxPlayers.ToString();
        lobbyId.text = "Delete This";
    }
    public void Select()
    {
        if (lobby == null)
            return;
        SelectedHandler = this;
    }
}
