using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Collections;
using System;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections;
using Unity.Services.Authentication;
using Newtonsoft.Json;

namespace MultiplayerHandlers
{
    public class SessionHandler : IDisposable
    {
        public Lobby ConnectedLobby;
        private Allocation _allocation = null;
        private JoinAllocation _joinAllocation = null;
        private NetworkDriver _driver = default;
        private NetworkConnection _connection = default;
        private NativeList<NetworkConnection> _connections = default;
        public event Action<NetworkMessage> DataReceived;
        public event Action PlayerConnected;
        private bool isHeartbeat = false;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public async Task CreateSessionAsync(string LobbyName, int MaxPlayers, CreateLobbyOptions options = null)
        {
            _allocation = await Relay.Instance.CreateAllocationAsync(MaxPlayers);
            var code = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            if (options == null)
            {
                options = new();
                options.Data = new();
            }
            options.Data["AllocationCode"] = new DataObject(DataObject.VisibilityOptions.Public, code);

            _connections = new NativeList<NetworkConnection>(MaxPlayers, Allocator.Persistent);
            ConnectedLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, options);
            Debug.Log("Session Created, Code: " + code);
            Bind();
            Debug.Log($"Bound: {_driver.Bound}, Created: {_driver.IsCreated}, Listening: {_driver.Listening}");
        }
        public async Task<Lobby> JoinSessionByIdAsync(string LobbyID)
        {
            Debug.Log("Joining Session");
            ConnectedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(LobbyID);
            var allocationCode = ConnectedLobby.Data["AllocationCode"].Value;

            Debug.Log($"Join Code: {allocationCode}");
            try
            {
                _joinAllocation = await Relay.Instance.JoinAllocationAsync(allocationCode);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            if (_joinAllocation == null)
            {
                await LobbyService.Instance.RemovePlayerAsync(ConnectedLobby.Id, AuthenticationService.Instance.PlayerId);
                return null;
            }
            Debug.Log("Binding...");
            Bind();
            return ConnectedLobby;
        }
        public async Task JoinSessionByCodeAsync(string LobbyCode)
        {
            ConnectedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(LobbyCode);
            var allocationCode = ConnectedLobby.Data["AllocationCode"].ToString();
            _joinAllocation = await Relay.Instance.JoinAllocationAsync(allocationCode);
            Bind();
        }
        private void Bind()
        {
            if (ConnectedLobby == null) throw new System.Exception("Lobby not created");
            if (_allocation == null && _joinAllocation == null) throw new System.Exception("Allocation not created");
            if (_allocation != null) BindHost();
            if (_joinAllocation != null) BindJoin();
        }
        private void BindJoin()
        {
            if (ConnectedLobby == null) throw new System.Exception("Lobby not created");
            if (_joinAllocation == null) throw new System.Exception("Allocation not created");
            var relayServerData = new RelayServerData(_joinAllocation, "udp");

            // Create NetworkSettings using the Relay server data.
            var settings = new NetworkSettings();
            settings.WithRelayParameters(ref relayServerData);

            // Create the Player's NetworkDriver from the NetworkSettings object.
            _driver = NetworkDriver.Create(settings);

            // Bind to the Relay server.
            if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
            {
                throw new System.Exception("Player client failed to bind");
            }
            else
            {
                Debug.Log("Player client bound to Relay server");
            }
            Debug.Log("Connecting to Host...");
            _connection = _driver.Connect();
        }
        private void BindHost()
        {
            if (ConnectedLobby == null) throw new System.Exception("Lobby not created");
            if (_allocation == null) throw new System.Exception("Allocation not created");

            var relayServerData = new RelayServerData(_allocation, "udp");

            var settings = new NetworkSettings();
            settings.WithRelayParameters(ref relayServerData);

            _driver = NetworkDriver.Create(settings);
            if (_driver.Bind(NetworkEndpoint.AnyIpv4) != 0)
            {
                throw new System.Exception("Host client failed to bind");
            }
            else
            {
                if (_driver.Listen() != 0)
                {
                    throw new System.Exception("Host client failed to listen");
                }
                else
                {
                    Debug.Log("Host client bound to Relay server");
                }
            }
        }
        public void SendMessage(NetworkMessage message, NetworkConnection except = default)
        {
            Debug.Log($"Message: {message.data as string} Type: {message.MessageType}");
            if (_allocation == null && _joinAllocation == null) throw new System.Exception("Allocation not created");
            if (_allocation == null)
            {
                if (_driver.BeginSend(_connection, out var writer) == 0)
                {
                    string data = JsonConvert.SerializeObject(message, settings);
                    writer.WriteFixedString4096(data);
                    _driver.EndSend(writer);
                }
            }
            else
            {
                DataReceived?.Invoke(message);
                if (_connections.Length == 0)
                {
                    Debug.LogError("No players connected to send messages to.");
                    return;
                }
                for (int i = 0; i < _connections.Length; i++)
                {
                    if (_connections[i] == except) continue;
                    if (_driver.BeginSend(_connections[i], out var writer) == 0)
                    {
                        string data = JsonConvert.SerializeObject(message, settings);
                        writer.WriteFixedString4096(data);
                        _driver.EndSend(writer);
                    }

                }
            }
        }
        public void UpdateLoop()
        {
            if (!(_driver.IsCreated && _driver.Bound))
            {
                return;
            }
            Debug.Log(_driver.Listening);
            _driver.ScheduleUpdate().Complete();
            if (IsHost)
            {
                AcceptConnections();
                CleanStaleConnection();
                ProcessEvents();
            }
            else
            {
                ProcessEventsPerConnection(_connection);
            }

        }
        public void StartHeartbeat(MonoBehaviour monoBehaviour, float seconds)
        {
            isHeartbeat = true;
            if (monoBehaviour == null)
            {
                isHeartbeat = false;
                throw new System.Exception("MonoBehaviour not found");
            }
            monoBehaviour.StartCoroutine(Heartbeat(ConnectedLobby.Id, seconds));
        }

        private IEnumerator Heartbeat(string lobbyId, float seconds)
        {
            var delay = new WaitForSecondsRealtime(seconds);
            while (isHeartbeat)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }
        private void ProcessEvents()
        {
            for (int i = 0; i < _connections.Length; i++)
            {

                ProcessEventsPerConnection(_connections[i]);
                // Resolve event queue.
            }
        }
        private void ProcessEventsPerConnection(NetworkConnection connection)
        {
            if (connection.IsCreated == false) return;
            NetworkEvent.Type eventType;
            while ((eventType = connection.PopEvent(_driver, out var stream)) != NetworkEvent.Type.Empty)
            {
                switch (eventType)
                {
                    // Handle Relay events.
                    case NetworkEvent.Type.Data:
                        var data = JsonConvert.DeserializeObject<NetworkMessage>(stream.ReadFixedString4096().ToString(), settings);
                        if (!IsHost) DataReceived?.Invoke(data);
                        if (IsHost) SendMessage(data,connection);
                        break;

                    // Handle Connect events.
                    case NetworkEvent.Type.Connect:
                        Debug.Log("Player connected to the Host");
                        PlayerConnected?.Invoke();
                        break;

                    // Handle Disconnect events.
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Player got disconnected from the Host");
                        connection = default(NetworkConnection);
                        Dispose();
                        break;
                    case NetworkEvent.Type.Empty:
                        Debug.Log("WTF");
                        break;
                }
            }
        }
        private void AcceptConnections()
        {
            if (_allocation == null) throw new System.Exception("Allocation not created");
            NetworkConnection incomingConnection;
            while ((incomingConnection = _driver.Accept()) != default(NetworkConnection))
            {
                // Adds the requesting Player to the serverConnections list.
                // This also sends a Connect event back the requesting Player,
                // as a means of acknowledging acceptance.
                Debug.Log("Accepted an incoming connection.");
                _connections.Add(incomingConnection);
            }
        }
        private void CleanStaleConnection()
        {
            if (_allocation == null) throw new System.Exception("Allocation not created");
            for (int i = 0; i < _connections.Length; i++)
            {
                if (!_connections[i].IsCreated)
                {
                    Debug.Log("Stale connection removed");
                    _connections.RemoveAt(i);
                    --i;
                }
            }
        }

        public void Dispose()
        {
            _driver.Disconnect(_connection);
            _connection.Disconnect(_driver);
            _driver.Dispose();
            _connections.Dispose();
            _allocation = null;
            _joinAllocation = null;
            isHeartbeat = false;
            DataReceived = null;
        }

        public bool IsHost => _allocation != null;

    }
}
[Serializable]
public struct NetworkMessage
{
    public string PlayerName;
    public string MessageType;
    public object data;
    public NetworkMessage(object data, string messageType)
    {
        PlayerName = AuthenticationService.Instance.PlayerName;
        this.data = data;
        MessageType = messageType;
    }
    public override string ToString()
    {
        return data.ToString();
    }
}
