// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Multiplayer Client
// Sprint 8: Client-side networking, state management, and UI integration
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ElectionEmpire.Multiplayer.Network;

namespace ElectionEmpire.Multiplayer
{
    #region Client Configuration
    
    [Serializable]
    public class ClientConfig
    {
        public string ServerAddress = "localhost";
        public int ServerPort = 7777;
        public float ConnectionTimeoutSeconds = 10f;
        public float HeartbeatIntervalSeconds = 5f;
        public float ReconnectDelaySeconds = 3f;
        public int MaxReconnectAttempts = 5;
        public bool AutoReconnect = true;
    }
    
    #endregion
    
    #region Client State
    
    /// <summary>
    /// Client connection states.
    /// </summary>
    public enum ClientState
    {
        Disconnected,
        Connecting,
        Authenticating,
        Connected,
        InLobby,
        InMatchmaking,
        InGame,
        Reconnecting
    }
    
    /// <summary>
    /// Local representation of lobby state.
    /// </summary>
    public class LocalLobbyState
    {
        public string LobbyId;
        public string LobbyName;
        public string HostId;
        public bool IsHost;
        
        public List<LocalPlayerInfo> Players = new List<LocalPlayerInfo>();
        public List<string> Spectators = new List<string>();
        public LobbySettings Settings;
        
        public bool IsReady;
        public bool IsStarting;
        
        public void UpdateFromMessage(LobbyUpdateMessage msg)
        {
            LobbyId = msg.LobbyId;
            HostId = msg.HostId;
            IsStarting = msg.IsStarting;
            
            Players.Clear();
            foreach (var p in msg.Players)
            {
                Players.Add(new LocalPlayerInfo
                {
                    PlayerId = p.PlayerId,
                    DisplayName = p.DisplayName,
                    IsReady = p.IsReady,
                    IsHost = p.IsHost
                });
            }
            
            Spectators = new List<string>(msg.Spectators);
        }
    }
    
    public class LocalPlayerInfo
    {
        public string PlayerId;
        public string DisplayName;
        public bool IsReady;
        public bool IsHost;
        public int Latency;
    }
    
    /// <summary>
    /// Local representation of game state.
    /// </summary>
    public class LocalGameState
    {
        public string SessionId;
        public int CurrentTurn;
        public int CurrentYear;
        public string CurrentPhase;
        public int TurnTimeRemaining;
        
        public List<GamePlayerState> Players = new List<GamePlayerState>();
        public string LocalPlayerId;
        
        public bool IsMyTurn;
        public bool HasSubmittedActions;
        
        public byte[] LastStateSnapshot;
        public string LastChecksum;
    }
    
    public class GamePlayerState
    {
        public string PlayerId;
        public string DisplayName;
        public int SlotIndex;
        public bool IsConnected;
        public bool HasSubmittedTurn;
        public int Score;
        public int CurrentOffice;
        public float ApprovalRating;
    }
    
    #endregion
    
    /// <summary>
    /// Main multiplayer client for Election Empire.
    /// Handles connection, lobbies, matchmaking, and game synchronization.
    /// </summary>
    public class MultiplayerClient : IDisposable
    {
        #region Fields
        
        private readonly ClientConfig _config;
        private ClientState _state;
        
        // Identity
        private string _playerId;
        private string _displayName;
        private string _authToken;
        
        // Connection
        private INetworkTransport _transport;
        private ConnectionQuality _connectionQuality;
        private MessageSequencer _sequencer;
        private float _lastHeartbeat;
        private int _reconnectAttempts;
        
        // State
        private LocalLobbyState _lobbyState;
        private LocalGameState _gameState;
        private Queue<NetworkMessage> _messageQueue;
        
        // Matchmaking
        private string _currentQueueType;
        private float _queueStartTime;
        
        #endregion
        
        #region Events
        
        // Connection events
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnConnectionError;
        public event Action OnReconnecting;
        public event Action OnReconnected;
        
        // Lobby events
        public event Action<List<GameLobby>> OnLobbyListReceived;
        public event Action<LocalLobbyState> OnLobbyJoined;
        public event Action<LocalLobbyState> OnLobbyUpdated;
        public event Action OnLobbyLeft;
        public event Action<string, string> OnLobbyChatReceived;
        
        // Matchmaking events
        public event Action<float> OnQueueUpdate;
        public event Action<PotentialMatch> OnMatchFound;
        public event Action OnMatchCancelled;
        
        // Game events
        public event Action<LocalGameState> OnGameStarted;
        public event Action<int, int, string> OnTurnStarted;
        public event Action<int> OnTurnEnded;
        public event Action<GameAction> OnActionReceived;
        public event Action<string, List<PlayerFinalResult>> OnGameEnded;
        public event Action OnDesyncDetected;
        
        // System events
        public event Action<int> OnLatencyUpdated;
        public event Action<string> OnError;
        
        #endregion
        
        #region Properties
        
        public ClientState State => _state;
        public string PlayerId => _playerId;
        public string DisplayName => _displayName;
        public bool IsConnected => _state >= ClientState.Connected;
        public bool IsInLobby => _state == ClientState.InLobby;
        public bool IsInGame => _state == ClientState.InGame;
        public int Latency => _connectionQuality?.CurrentLatency ?? 0;
        public LocalLobbyState LobbyState => _lobbyState;
        public LocalGameState GameState => _gameState;
        
        #endregion
        
        #region Constructor
        
        public MultiplayerClient(ClientConfig config = null)
        {
            _config = config ?? new ClientConfig();
            _connectionQuality = new ConnectionQuality();
            _sequencer = new MessageSequencer();
            _messageQueue = new Queue<NetworkMessage>();
            _state = ClientState.Disconnected;
        }
        
        #endregion
        
        #region Connection
        
        /// <summary>
        /// Connect to the multiplayer server.
        /// </summary>
        public async Task<bool> Connect(string playerId, string displayName, string authToken = null)
        {
            if (_state != ClientState.Disconnected)
            {
                Debug.LogWarning("[MultiplayerClient] Already connected or connecting");
                return false;
            }
            
            _playerId = playerId;
            _displayName = displayName;
            _authToken = authToken;
            _state = ClientState.Connecting;
            
            try
            {
                // Initialize transport (placeholder - would be actual network implementation)
                _transport = new MockNetworkTransport();
                
                bool connected = await _transport.Connect(_config.ServerAddress, _config.ServerPort, 
                    _config.ConnectionTimeoutSeconds);
                
                if (!connected)
                {
                    _state = ClientState.Disconnected;
                    OnConnectionError?.Invoke("Failed to connect to server");
                    return false;
                }
                
                // Send connect message
                var connectMsg = new ConnectMessage
                {
                    PlayerId = playerId,
                    DisplayName = displayName,
                    ClientVersion = Application.version,
                    Region = GetRegion()
                };
                
                SendMessage(connectMsg);
                
                // Authenticate if token provided
                if (!string.IsNullOrEmpty(authToken))
                {
                    _state = ClientState.Authenticating;
                    var authMsg = new AuthenticateMessage
                    {
                        AuthToken = authToken,
                        SessionId = Guid.NewGuid().ToString()
                    };
                    SendMessage(authMsg);
                }
                
                _state = ClientState.Connected;
                _reconnectAttempts = 0;
                OnConnected?.Invoke();
                
                Debug.Log($"[MultiplayerClient] Connected as {displayName}");
                return true;
            }
            catch (Exception ex)
            {
                _state = ClientState.Disconnected;
                OnConnectionError?.Invoke(ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect(string reason = null)
        {
            if (_state == ClientState.Disconnected) return;
            
            var disconnectMsg = new DisconnectMessage { Reason = reason ?? "Client disconnected" };
            SendMessage(disconnectMsg);
            
            _transport?.Disconnect();
            _state = ClientState.Disconnected;
            _lobbyState = null;
            _gameState = null;
            
            OnDisconnected?.Invoke(reason ?? "Disconnected");
        }
        
        private async void AttemptReconnect()
        {
            if (!_config.AutoReconnect) return;
            if (_reconnectAttempts >= _config.MaxReconnectAttempts)
            {
                OnConnectionError?.Invoke("Max reconnection attempts reached");
                _state = ClientState.Disconnected;
                return;
            }
            
            _state = ClientState.Reconnecting;
            _reconnectAttempts++;
            OnReconnecting?.Invoke();
            
            await Task.Delay((int)(_config.ReconnectDelaySeconds * 1000));
            
            bool success = await Connect(_playerId, _displayName, _authToken);
            
            if (success)
            {
                OnReconnected?.Invoke();
                
                // Rejoin lobby/game if applicable
                if (_lobbyState != null)
                {
                    JoinLobby(_lobbyState.LobbyId);
                }
            }
        }
        
        private string GetRegion()
        {
            // Would detect actual region
            return "US-East";
        }
        
        #endregion
        
        #region Update Loop
        
        /// <summary>
        /// Process network updates. Call from Update().
        /// </summary>
        public void Update()
        {
            if (_state == ClientState.Disconnected) return;
            
            // Process incoming messages
            ProcessIncomingMessages();
            
            // Send heartbeat
            if (Time.time - _lastHeartbeat > _config.HeartbeatIntervalSeconds)
            {
                SendHeartbeat();
                _lastHeartbeat = Time.time;
            }
            
            // Update queue time
            if (_state == ClientState.InMatchmaking)
            {
                OnQueueUpdate?.Invoke(Time.time - _queueStartTime);
            }
            
            // Check for connection issues
            if (_transport != null && !_transport.IsConnected && _state != ClientState.Disconnected)
            {
                AttemptReconnect();
            }
        }
        
        private void ProcessIncomingMessages()
        {
            if (_transport == null) return;
            
            while (_transport.HasPendingMessages)
            {
                var data = _transport.ReceiveMessage();
                if (data == null) break;
                
                try
                {
                    var message = MessageSerializer.Deserialize(data);
                    
                    if (_sequencer.ProcessSequence(message))
                    {
                        HandleMessage(message);
                    }
                    
                    // Process any pending ordered messages
                    foreach (var pending in _sequencer.GetPendingInOrder())
                    {
                        HandleMessage(pending);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MultiplayerClient] Error processing message: {ex.Message}");
                }
            }
        }
        
        private void SendHeartbeat()
        {
            var ping = new PingMessage
            {
                ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            SendMessage(ping);
        }
        
        #endregion
        
        #region Message Handling
        
        private void HandleMessage(NetworkMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Pong:
                    HandlePong((PongMessage)message);
                    break;
                    
                case MessageType.LobbyUpdate:
                    HandleLobbyUpdate((LobbyUpdateMessage)message);
                    break;
                    
                case MessageType.LobbyChat:
                    HandleLobbyChat((LobbyChatMessage)message);
                    break;
                    
                case MessageType.GameStart:
                    HandleGameStart((GameStartMessage)message);
                    break;
                    
                case MessageType.TurnStart:
                    HandleTurnStart((TurnStartMessage)message);
                    break;
                    
                case MessageType.TurnEnd:
                    HandleTurnEnd((TurnEndMessage)message);
                    break;
                    
                case MessageType.GameAction:
                    HandleGameAction((GameActionMessage)message);
                    break;
                    
                case MessageType.GameEnd:
                    HandleGameEnd((GameEndMessage)message);
                    break;
                    
                case MessageType.StateSnapshot:
                    HandleStateSnapshot((StateSnapshotMessage)message);
                    break;
                    
                case MessageType.MatchFound:
                    HandleMatchFound((MatchFoundMessage)message);
                    break;
                    
                case MessageType.Error:
                    HandleError((ErrorMessage)message);
                    break;
            }
        }
        
        private void HandlePong(PongMessage msg)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int latency = (int)(now - msg.ClientTimestamp);
            _connectionQuality.RecordLatency(latency);
            OnLatencyUpdated?.Invoke(latency);
        }
        
        private void HandleLobbyUpdate(LobbyUpdateMessage msg)
        {
            if (_lobbyState == null)
            {
                _lobbyState = new LocalLobbyState();
            }
            
            _lobbyState.UpdateFromMessage(msg);
            _lobbyState.IsHost = msg.HostId == _playerId;
            
            OnLobbyUpdated?.Invoke(_lobbyState);
        }
        
        private void HandleLobbyChat(LobbyChatMessage msg)
        {
            OnLobbyChatReceived?.Invoke(msg.SenderName, msg.Message);
        }
        
        private void HandleGameStart(GameStartMessage msg)
        {
            _gameState = new LocalGameState
            {
                SessionId = msg.SessionId,
                CurrentTurn = 1,
                CurrentYear = msg.StartingYear,
                CurrentPhase = "Setup",
                LocalPlayerId = _playerId
            };
            
            foreach (var p in msg.Players)
            {
                _gameState.Players.Add(new GamePlayerState
                {
                    PlayerId = p.PlayerId,
                    DisplayName = p.DisplayName,
                    SlotIndex = p.SlotIndex,
                    IsConnected = true
                });
            }
            
            _state = ClientState.InGame;
            OnGameStarted?.Invoke(_gameState);
        }
        
        private void HandleTurnStart(TurnStartMessage msg)
        {
            if (_gameState == null) return;
            
            _gameState.CurrentTurn = msg.Turn;
            _gameState.CurrentPhase = msg.Phase;
            _gameState.TurnTimeRemaining = msg.TimeLimitSeconds;
            _gameState.HasSubmittedActions = false;
            
            // Reset player submission status
            foreach (var p in _gameState.Players)
            {
                p.HasSubmittedTurn = false;
            }
            
            OnTurnStarted?.Invoke(msg.Turn, msg.Year, msg.Phase);
        }
        
        private void HandleTurnEnd(TurnEndMessage msg)
        {
            if (_gameState == null) return;
            
            foreach (var summary in msg.PlayerSummaries)
            {
                var player = _gameState.Players.FirstOrDefault(p => p.PlayerId == summary.PlayerId);
                if (player != null)
                {
                    player.Score += summary.ScoreChange;
                }
            }
            
            OnTurnEnded?.Invoke(msg.Turn);
        }
        
        private void HandleGameAction(GameActionMessage msg)
        {
            if (msg.SenderId == _playerId) return; // Ignore own actions
            
            var action = new GameAction
            {
                ActionId = msg.ActionId,
                PlayerId = msg.SenderId,
                ActionType = msg.ActionType,
                Turn = msg.Turn
            };
            
            OnActionReceived?.Invoke(action);
        }
        
        private void HandleGameEnd(GameEndMessage msg)
        {
            _state = ClientState.InLobby;
            OnGameEnded?.Invoke(msg.WinnerId, msg.Results);
            _gameState = null;
        }
        
        private void HandleStateSnapshot(StateSnapshotMessage msg)
        {
            if (_gameState == null) return;
            
            // Verify checksum
            if (!string.IsNullOrEmpty(_gameState.LastChecksum) && 
                _gameState.LastChecksum != msg.Checksum)
            {
                OnDesyncDetected?.Invoke();
                RequestResync();
            }
            
            _gameState.LastStateSnapshot = msg.CompressedState;
            _gameState.LastChecksum = msg.Checksum;
        }
        
        private void HandleMatchFound(MatchFoundMessage msg)
        {
            var match = new PotentialMatch
            {
                MatchId = msg.MatchId,
                RaceType = msg.RaceType
            };
            
            foreach (var p in msg.Players)
            {
                match.Players.Add(new QueuedPlayer
                {
                    PlayerId = p.PlayerId,
                    DisplayName = p.DisplayName,
                    SkillRating = p.SkillRating
                });
            }
            
            OnMatchFound?.Invoke(match);
        }
        
        private void HandleError(ErrorMessage msg)
        {
            OnError?.Invoke($"Error {msg.ErrorCode}: {msg.ErrorText}");
        }
        
        #endregion
        
        #region Lobby Operations
        
        /// <summary>
        /// Create a new lobby.
        /// </summary>
        public void CreateLobby(string lobbyName, int maxPlayers, LobbySettings settings = null)
        {
            if (!IsConnected) return;
            
            var msg = new CreateLobbyMessage
            {
                LobbyName = lobbyName,
                MaxPlayers = maxPlayers,
                Visibility = LobbyVisibility.Public,
                SettingsData = SerializeSettings(settings)
            };
            
            SendMessage(msg);
            _state = ClientState.InLobby;
            
            _lobbyState = new LocalLobbyState
            {
                LobbyName = lobbyName,
                IsHost = true,
                Settings = settings ?? new LobbySettings()
            };
        }
        
        /// <summary>
        /// Join an existing lobby.
        /// </summary>
        public void JoinLobby(string lobbyId, string password = null)
        {
            if (!IsConnected) return;
            
            var msg = new JoinLobbyMessage
            {
                LobbyId = lobbyId,
                Password = password,
                AsSpectator = false
            };
            
            SendMessage(msg);
            _state = ClientState.InLobby;
        }
        
        /// <summary>
        /// Leave current lobby.
        /// </summary>
        public void LeaveLobby()
        {
            if (!IsInLobby) return;
            
            var msg = new NetworkMessage.LeaveLobbyMessage();
            SendMessage(msg);
            
            _lobbyState = null;
            _state = ClientState.Connected;
            OnLobbyLeft?.Invoke();
        }
        
        /// <summary>
        /// Set ready status in lobby.
        /// </summary>
        public void SetReady(bool isReady)
        {
            if (!IsInLobby) return;
            
            _lobbyState.IsReady = isReady;
            
            var msg = new PlayerReadyMessage { IsReady = isReady };
            SendMessage(msg);
        }
        
        /// <summary>
        /// Send chat message to lobby.
        /// </summary>
        public void SendLobbyChat(string message)
        {
            if (!IsInLobby) return;
            
            var msg = new LobbyChatMessage
            {
                Message = message,
                SenderName = _displayName
            };
            
            SendMessage(msg);
        }
        
        /// <summary>
        /// Start the game (host only).
        /// </summary>
        public void StartGame()
        {
            if (!IsInLobby || !_lobbyState.IsHost) return;
            
            // Send start request
            var msg = new GameStartRequestMessage();
            SendMessage(msg);
        }
        
        private byte[] SerializeSettings(LobbySettings settings)
        {
            if (settings == null) return null;
            return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(settings));
        }
        
        #endregion
        
        #region Matchmaking Operations
        
        /// <summary>
        /// Enter matchmaking queue.
        /// </summary>
        public void EnterQueue(QueueType queueType, RaceType preferredRace)
        {
            if (!IsConnected) return;
            
            _state = ClientState.InMatchmaking;
            _currentQueueType = queueType.ToString();
            _queueStartTime = Time.time;
            
            var msg = new EnterQueueMessage
            {
                QueueType = queueType,
                PreferredRace = preferredRace
            };
            
            SendMessage(msg);
        }
        
        /// <summary>
        /// Leave matchmaking queue.
        /// </summary>
        public void LeaveQueue()
        {
            if (_state != ClientState.InMatchmaking) return;
            
            var msg = new LeaveQueueMessage();
            SendMessage(msg);
            
            _state = ClientState.Connected;
            OnMatchCancelled?.Invoke();
        }
        
        /// <summary>
        /// Accept a found match.
        /// </summary>
        public void AcceptMatch(string matchId)
        {
            var msg = new MatchAcceptMessage { MatchId = matchId };
            SendMessage(msg);
        }
        
        /// <summary>
        /// Decline a found match.
        /// </summary>
        public void DeclineMatch(string matchId)
        {
            var msg = new MatchDeclineMessage { MatchId = matchId };
            SendMessage(msg);
            _state = ClientState.InMatchmaking;
        }
        
        #endregion
        
        #region Game Operations
        
        /// <summary>
        /// Submit a game action.
        /// </summary>
        public void SubmitAction(string actionType, Dictionary<string, object> parameters)
        {
            if (!IsInGame) return;
            
            var msg = new GameActionMessage
            {
                ActionId = Guid.NewGuid().ToString(),
                ActionType = actionType,
                Turn = _gameState.CurrentTurn,
                ActionData = SerializeParameters(parameters)
            };
            
            SendMessage(msg);
        }
        
        /// <summary>
        /// Submit end of turn.
        /// </summary>
        public void SubmitTurn()
        {
            if (!IsInGame || _gameState.HasSubmittedActions) return;
            
            _gameState.HasSubmittedActions = true;
            
            var msg = new SubmitTurnMessage { Turn = _gameState.CurrentTurn };
            SendMessage(msg);
        }
        
        /// <summary>
        /// Request state resync.
        /// </summary>
        public void RequestResync()
        {
            if (!IsInGame) return;
            
            var msg = new SyncRequestMessage
            {
                LastKnownTurn = _gameState.CurrentTurn,
                LastKnownChecksum = _gameState.LastChecksum
            };
            
            SendMessage(msg);
        }
        
        /// <summary>
        /// Send in-game chat.
        /// </summary>
        public void SendGameChat(string message, bool teamOnly = false)
        {
            if (!IsInGame) return;
            
            var msg = new GameChatMessage
            {
                Message = message,
                TeamOnly = teamOnly
            };
            
            SendMessage(msg);
        }
        
        private byte[] SerializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters == null) return null;
            return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new SerializableDict(parameters)));
        }
        
        #endregion
        
        #region Messaging
        
        private void SendMessage(NetworkMessage message)
        {
            if (_transport == null || !_transport.IsConnected) return;
            
            message.SenderId = _playerId;
            message.SequenceNumber = _sequencer.GetNextSequence();
            
            var data = MessageSerializer.Serialize(message);
            _transport.SendMessage(data, message.Reliability);
        }
        
        #endregion
        
        #region Disposal
        
        public void Dispose()
        {
            Disconnect("Client disposed");
            _transport?.Dispose();
        }
        
        #endregion
    }
    
    #region Additional Message Types
    
    // Additional messages not defined in NetworkMessages.cs
    
    public class PlayerReadyMessage : NetworkMessage
    {
        public bool IsReady;
        public PlayerReadyMessage() : base(MessageType.PlayerReady) { }
        public override void Serialize(BinaryWriter writer) { writer.Write(IsReady); }
        public override void Deserialize(BinaryReader reader) { IsReady = reader.ReadBoolean(); }
    }
    
    public class GameStartRequestMessage : NetworkMessage
    {
        public GameStartRequestMessage() : base(MessageType.GameStart) { }
        public override void Serialize(BinaryWriter writer) { }
        public override void Deserialize(BinaryReader reader) { }
    }
    
    public class EnterQueueMessage : NetworkMessage
    {
        public QueueType QueueType;
        public RaceType PreferredRace;
        public EnterQueueMessage() : base(MessageType.EnterQueue) { }
        public override void Serialize(BinaryWriter writer) 
        { 
            writer.Write((byte)QueueType);
            writer.Write((byte)PreferredRace);
        }
        public override void Deserialize(BinaryReader reader) 
        { 
            QueueType = (QueueType)reader.ReadByte();
            PreferredRace = (RaceType)reader.ReadByte();
        }
    }
    
    public class LeaveQueueMessage : NetworkMessage
    {
        public LeaveQueueMessage() : base(MessageType.LeaveQueue) { }
        public override void Serialize(BinaryWriter writer) { }
        public override void Deserialize(BinaryReader reader) { }
    }
    
    public class MatchAcceptMessage : NetworkMessage
    {
        public string MatchId;
        public MatchAcceptMessage() : base(MessageType.MatchAccept) { }
        public override void Serialize(BinaryWriter writer) { writer.Write(MatchId ?? ""); }
        public override void Deserialize(BinaryReader reader) { MatchId = reader.ReadString(); }
    }
    
    public class MatchDeclineMessage : NetworkMessage
    {
        public string MatchId;
        public MatchDeclineMessage() : base(MessageType.MatchDecline) { }
        public override void Serialize(BinaryWriter writer) { writer.Write(MatchId ?? ""); }
        public override void Deserialize(BinaryReader reader) { MatchId = reader.ReadString(); }
    }
    
    public class SubmitTurnMessage : NetworkMessage
    {
        public int Turn;
        public SubmitTurnMessage() : base(MessageType.TurnEnd) { }
        public override void Serialize(BinaryWriter writer) { writer.Write(Turn); }
        public override void Deserialize(BinaryReader reader) { Turn = reader.ReadInt32(); }
    }
    
    public class GameChatMessage : NetworkMessage
    {
        public string Message;
        public bool TeamOnly;
        public GameChatMessage() : base(MessageType.GameChat) { }
        public override void Serialize(BinaryWriter writer) 
        { 
            writer.Write(Message ?? "");
            writer.Write(TeamOnly);
        }
        public override void Deserialize(BinaryReader reader) 
        { 
            Message = reader.ReadString();
            TeamOnly = reader.ReadBoolean();
        }
    }
    
    #endregion
    
    #region Network Transport Interface
    
    /// <summary>
    /// Interface for network transport layer.
    /// </summary>
    public interface INetworkTransport : IDisposable
    {
        bool IsConnected { get; }
        bool HasPendingMessages { get; }
        
        Task<bool> Connect(string address, int port, float timeout);
        void Disconnect();
        void SendMessage(byte[] data, Reliability reliability);
        byte[] ReceiveMessage();
    }
    
    /// <summary>
    /// Mock transport for testing.
    /// </summary>
    public class MockNetworkTransport : INetworkTransport
    {
        public bool IsConnected { get; private set; }
        public bool HasPendingMessages => _incomingQueue.Count > 0;
        
        private Queue<byte[]> _incomingQueue = new Queue<byte[]>();
        
        public Task<bool> Connect(string address, int port, float timeout)
        {
            IsConnected = true;
            return Task.FromResult(true);
        }
        
        public void Disconnect()
        {
            IsConnected = false;
        }
        
        public void SendMessage(byte[] data, Reliability reliability)
        {
            // Mock: would send to server
            Debug.Log($"[MockTransport] Sending {data.Length} bytes");
        }
        
        public byte[] ReceiveMessage()
        {
            return _incomingQueue.Count > 0 ? _incomingQueue.Dequeue() : null;
        }
        
        public void SimulateIncoming(byte[] data)
        {
            _incomingQueue.Enqueue(data);
        }
        
        public void Dispose() { }
    }
    
    #endregion
    
    #region Helpers
    
    [Serializable]
    public class SerializableDict
    {
        public List<string> Keys = new List<string>();
        public List<string> Values = new List<string>();
        
        public SerializableDict() { }
        
        public SerializableDict(Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                Keys.Add(kvp.Key);
                Values.Add(kvp.Value?.ToString() ?? "");
            }
        }
    }
    
    // Namespace fix for NetworkMessage inner class reference
    namespace Network
    {
        public class LeaveLobbyMessage : NetworkMessage
        {
            public LeaveLobbyMessage() : base(MessageType.LeaveLobby) { }
            public override void Serialize(BinaryWriter writer) { }
            public override void Deserialize(BinaryReader reader) { }
        }
    }
    
    #endregion
}
