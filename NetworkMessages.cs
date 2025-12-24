// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Network Messages & Synchronization
// Sprint 8: Message protocol, state sync, and network utilities
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace ElectionEmpire.Multiplayer.Network
{
    #region Message Types
    
    /// <summary>
    /// Network message categories.
    /// </summary>
    public enum MessageType : byte
    {
        // Connection
        Connect = 0,
        Disconnect = 1,
        Heartbeat = 2,
        Authenticate = 3,
        
        // Lobby
        CreateLobby = 10,
        JoinLobby = 11,
        LeaveLobby = 12,
        LobbyUpdate = 13,
        PlayerReady = 14,
        KickPlayer = 15,
        LobbyChat = 16,
        LobbySettings = 17,
        
        // Game
        GameStart = 20,
        GameEnd = 21,
        TurnStart = 22,
        TurnEnd = 23,
        GameAction = 24,
        GameState = 25,
        GameChat = 26,
        
        // Sync
        StateSnapshot = 30,
        StateDelta = 31,
        SyncRequest = 32,
        SyncResponse = 33,
        Checksum = 34,
        
        // Matchmaking
        EnterQueue = 40,
        LeaveQueue = 41,
        MatchFound = 42,
        MatchAccept = 43,
        MatchDecline = 44,
        
        // System
        Error = 250,
        Ping = 251,
        Pong = 252,
        ServerInfo = 253,
        ClientInfo = 254,
        Custom = 255
    }
    
    /// <summary>
    /// Message reliability modes.
    /// </summary>
    public enum Reliability : byte
    {
        Unreliable,         // Fire and forget
        UnreliableSequenced,// Latest only
        Reliable,           // Guaranteed delivery
        ReliableOrdered,    // Guaranteed + ordered
        ReliableSequenced   // Guaranteed + latest only
    }
    
    #endregion
    
    #region Base Message
    
    /// <summary>
    /// Base class for all network messages.
    /// </summary>
    [Serializable]
    public abstract class NetworkMessage
    {
        public MessageType Type { get; protected set; }
        public Reliability Reliability { get; set; } = Reliability.Reliable;
        public string SenderId { get; set; }
        public long Timestamp { get; set; }
        public ushort SequenceNumber { get; set; }
        
        protected NetworkMessage(MessageType type)
        {
            Type = type;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
    }
    
    #endregion
    
    #region Connection Messages
    
    public class ConnectMessage : NetworkMessage
    {
        public string PlayerId;
        public string DisplayName;
        public string ClientVersion;
        public string Region;
        
        public ConnectMessage() : base(MessageType.Connect) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId ?? "");
            writer.Write(DisplayName ?? "");
            writer.Write(ClientVersion ?? "");
            writer.Write(Region ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadString();
            DisplayName = reader.ReadString();
            ClientVersion = reader.ReadString();
            Region = reader.ReadString();
        }
    }
    
    public class DisconnectMessage : NetworkMessage
    {
        public string Reason;
        
        public DisconnectMessage() : base(MessageType.Disconnect) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Reason ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            Reason = reader.ReadString();
        }
    }
    
    public class HeartbeatMessage : NetworkMessage
    {
        public long ClientTime;
        public int Latency;
        
        public HeartbeatMessage() : base(MessageType.Heartbeat) 
        {
            Reliability = Reliability.Unreliable;
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientTime);
            writer.Write(Latency);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            ClientTime = reader.ReadInt64();
            Latency = reader.ReadInt32();
        }
    }
    
    public class AuthenticateMessage : NetworkMessage
    {
        public string AuthToken;
        public string SessionId;
        
        public AuthenticateMessage() : base(MessageType.Authenticate) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(AuthToken ?? "");
            writer.Write(SessionId ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            AuthToken = reader.ReadString();
            SessionId = reader.ReadString();
        }
    }
    
    #endregion
    
    #region Lobby Messages
    
    public class CreateLobbyMessage : NetworkMessage
    {
        public string LobbyName;
        public int MaxPlayers;
        public LobbyVisibility Visibility;
        public byte[] SettingsData;
        
        public CreateLobbyMessage() : base(MessageType.CreateLobby) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(LobbyName ?? "");
            writer.Write(MaxPlayers);
            writer.Write((byte)Visibility);
            writer.Write(SettingsData?.Length ?? 0);
            if (SettingsData != null)
                writer.Write(SettingsData);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            LobbyName = reader.ReadString();
            MaxPlayers = reader.ReadInt32();
            Visibility = (LobbyVisibility)reader.ReadByte();
            int settingsLen = reader.ReadInt32();
            if (settingsLen > 0)
                SettingsData = reader.ReadBytes(settingsLen);
        }
    }
    
    public class JoinLobbyMessage : NetworkMessage
    {
        public string LobbyId;
        public string Password;
        public bool AsSpectator;
        
        public JoinLobbyMessage() : base(MessageType.JoinLobby) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(LobbyId ?? "");
            writer.Write(Password ?? "");
            writer.Write(AsSpectator);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            LobbyId = reader.ReadString();
            Password = reader.ReadString();
            AsSpectator = reader.ReadBoolean();
        }
    }
    
    public class LobbyUpdateMessage : NetworkMessage
    {
        public string LobbyId;
        public List<LobbyPlayerInfo> Players;
        public List<string> Spectators;
        public bool IsStarting;
        public string HostId;
        
        public LobbyUpdateMessage() : base(MessageType.LobbyUpdate) 
        {
            Players = new List<LobbyPlayerInfo>();
            Spectators = new List<string>();
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(LobbyId ?? "");
            writer.Write(Players.Count);
            foreach (var p in Players)
            {
                writer.Write(p.PlayerId ?? "");
                writer.Write(p.DisplayName ?? "");
                writer.Write(p.IsReady);
                writer.Write(p.IsHost);
            }
            writer.Write(Spectators.Count);
            foreach (var s in Spectators)
                writer.Write(s ?? "");
            writer.Write(IsStarting);
            writer.Write(HostId ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            LobbyId = reader.ReadString();
            int playerCount = reader.ReadInt32();
            Players.Clear();
            for (int i = 0; i < playerCount; i++)
            {
                Players.Add(new LobbyPlayerInfo
                {
                    PlayerId = reader.ReadString(),
                    DisplayName = reader.ReadString(),
                    IsReady = reader.ReadBoolean(),
                    IsHost = reader.ReadBoolean()
                });
            }
            int specCount = reader.ReadInt32();
            Spectators.Clear();
            for (int i = 0; i < specCount; i++)
                Spectators.Add(reader.ReadString());
            IsStarting = reader.ReadBoolean();
            HostId = reader.ReadString();
        }
    }
    
    public class LobbyPlayerInfo
    {
        public string PlayerId;
        public string DisplayName;
        public bool IsReady;
        public bool IsHost;
    }
    
    public class LobbyChatMessage : NetworkMessage
    {
        public string Message;
        public string SenderName;
        
        public LobbyChatMessage() : base(MessageType.LobbyChat) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Message ?? "");
            writer.Write(SenderName ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            Message = reader.ReadString();
            SenderName = reader.ReadString();
        }
    }
    
    #endregion
    
    #region Game Messages
    
    public class GameStartMessage : NetworkMessage
    {
        public string SessionId;
        public List<GamePlayerInfo> Players;
        public int StartingYear;
        public byte[] InitialState;
        
        public GameStartMessage() : base(MessageType.GameStart) 
        {
            Players = new List<GamePlayerInfo>();
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(SessionId ?? "");
            writer.Write(Players.Count);
            foreach (var p in Players)
            {
                writer.Write(p.PlayerId ?? "");
                writer.Write(p.DisplayName ?? "");
                writer.Write(p.SlotIndex);
                writer.Write(p.CharacterId ?? "");
            }
            writer.Write(StartingYear);
            writer.Write(InitialState?.Length ?? 0);
            if (InitialState != null)
                writer.Write(InitialState);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            SessionId = reader.ReadString();
            int count = reader.ReadInt32();
            Players.Clear();
            for (int i = 0; i < count; i++)
            {
                Players.Add(new GamePlayerInfo
                {
                    PlayerId = reader.ReadString(),
                    DisplayName = reader.ReadString(),
                    SlotIndex = reader.ReadInt32(),
                    CharacterId = reader.ReadString()
                });
            }
            StartingYear = reader.ReadInt32();
            int stateLen = reader.ReadInt32();
            if (stateLen > 0)
                InitialState = reader.ReadBytes(stateLen);
        }
    }
    
    public class GamePlayerInfo
    {
        public string PlayerId;
        public string DisplayName;
        public int SlotIndex;
        public string CharacterId;
    }
    
    public class GameActionMessage : NetworkMessage
    {
        public string ActionId;
        public string ActionType;
        public int Turn;
        public byte[] ActionData;
        
        public GameActionMessage() : base(MessageType.GameAction) 
        {
            Reliability = Reliability.ReliableOrdered;
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ActionId ?? "");
            writer.Write(ActionType ?? "");
            writer.Write(Turn);
            writer.Write(ActionData?.Length ?? 0);
            if (ActionData != null)
                writer.Write(ActionData);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            ActionId = reader.ReadString();
            ActionType = reader.ReadString();
            Turn = reader.ReadInt32();
            int dataLen = reader.ReadInt32();
            if (dataLen > 0)
                ActionData = reader.ReadBytes(dataLen);
        }
    }
    
    public class TurnStartMessage : NetworkMessage
    {
        public int Turn;
        public int Year;
        public string Phase;
        public int TimeLimitSeconds;
        public byte[] TurnState;
        
        public TurnStartMessage() : base(MessageType.TurnStart) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Turn);
            writer.Write(Year);
            writer.Write(Phase ?? "");
            writer.Write(TimeLimitSeconds);
            writer.Write(TurnState?.Length ?? 0);
            if (TurnState != null)
                writer.Write(TurnState);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            Turn = reader.ReadInt32();
            Year = reader.ReadInt32();
            Phase = reader.ReadString();
            TimeLimitSeconds = reader.ReadInt32();
            int stateLen = reader.ReadInt32();
            if (stateLen > 0)
                TurnState = reader.ReadBytes(stateLen);
        }
    }
    
    public class TurnEndMessage : NetworkMessage
    {
        public int Turn;
        public List<PlayerTurnSummary> PlayerSummaries;
        
        public TurnEndMessage() : base(MessageType.TurnEnd) 
        {
            PlayerSummaries = new List<PlayerTurnSummary>();
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Turn);
            writer.Write(PlayerSummaries.Count);
            foreach (var s in PlayerSummaries)
            {
                writer.Write(s.PlayerId ?? "");
                writer.Write(s.ActionsSubmitted);
                writer.Write(s.ScoreChange);
            }
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            Turn = reader.ReadInt32();
            int count = reader.ReadInt32();
            PlayerSummaries.Clear();
            for (int i = 0; i < count; i++)
            {
                PlayerSummaries.Add(new PlayerTurnSummary
                {
                    PlayerId = reader.ReadString(),
                    ActionsSubmitted = reader.ReadInt32(),
                    ScoreChange = reader.ReadInt32()
                });
            }
        }
    }
    
    public class PlayerTurnSummary
    {
        public string PlayerId;
        public int ActionsSubmitted;
        public int ScoreChange;
    }
    
    public class GameEndMessage : NetworkMessage
    {
        public string WinnerId;
        public List<PlayerFinalResult> Results;
        public string EndReason;
        
        public GameEndMessage() : base(MessageType.GameEnd) 
        {
            Results = new List<PlayerFinalResult>();
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(WinnerId ?? "");
            writer.Write(Results.Count);
            foreach (var r in Results)
            {
                writer.Write(r.PlayerId ?? "");
                writer.Write(r.FinalPosition);
                writer.Write(r.FinalScore);
                writer.Write(r.SkillChange);
            }
            writer.Write(EndReason ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            WinnerId = reader.ReadString();
            int count = reader.ReadInt32();
            Results.Clear();
            for (int i = 0; i < count; i++)
            {
                Results.Add(new PlayerFinalResult
                {
                    PlayerId = reader.ReadString(),
                    FinalPosition = reader.ReadInt32(),
                    FinalScore = reader.ReadInt32(),
                    SkillChange = reader.ReadInt32()
                });
            }
            EndReason = reader.ReadString();
        }
    }
    
    public class PlayerFinalResult
    {
        public string PlayerId;
        public int FinalPosition;
        public int FinalScore;
        public int SkillChange;
    }
    
    #endregion
    
    #region Sync Messages
    
    public class StateSnapshotMessage : NetworkMessage
    {
        public int Turn;
        public byte[] CompressedState;
        public string Checksum;
        
        public StateSnapshotMessage() : base(MessageType.StateSnapshot) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Turn);
            writer.Write(CompressedState?.Length ?? 0);
            if (CompressedState != null)
                writer.Write(CompressedState);
            writer.Write(Checksum ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            Turn = reader.ReadInt32();
            int len = reader.ReadInt32();
            if (len > 0)
                CompressedState = reader.ReadBytes(len);
            Checksum = reader.ReadString();
        }
    }
    
    public class StateDeltaMessage : NetworkMessage
    {
        public int FromTurn;
        public int ToTurn;
        public byte[] DeltaData;
        
        public StateDeltaMessage() : base(MessageType.StateDelta) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(FromTurn);
            writer.Write(ToTurn);
            writer.Write(DeltaData?.Length ?? 0);
            if (DeltaData != null)
                writer.Write(DeltaData);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            FromTurn = reader.ReadInt32();
            ToTurn = reader.ReadInt32();
            int len = reader.ReadInt32();
            if (len > 0)
                DeltaData = reader.ReadBytes(len);
        }
    }
    
    public class SyncRequestMessage : NetworkMessage
    {
        public int LastKnownTurn;
        public string LastKnownChecksum;
        
        public SyncRequestMessage() : base(MessageType.SyncRequest) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(LastKnownTurn);
            writer.Write(LastKnownChecksum ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            LastKnownTurn = reader.ReadInt32();
            LastKnownChecksum = reader.ReadString();
        }
    }
    
    #endregion
    
    #region Matchmaking Messages
    
    public class MatchFoundMessage : NetworkMessage
    {
        public string MatchId;
        public List<MatchPlayerInfo> Players;
        public RaceType RaceType;
        public int AcceptTimeoutSeconds;
        
        public MatchFoundMessage() : base(MessageType.MatchFound) 
        {
            Players = new List<MatchPlayerInfo>();
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(MatchId ?? "");
            writer.Write(Players.Count);
            foreach (var p in Players)
            {
                writer.Write(p.PlayerId ?? "");
                writer.Write(p.DisplayName ?? "");
                writer.Write(p.SkillRating);
            }
            writer.Write((byte)RaceType);
            writer.Write(AcceptTimeoutSeconds);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            MatchId = reader.ReadString();
            int count = reader.ReadInt32();
            Players.Clear();
            for (int i = 0; i < count; i++)
            {
                Players.Add(new MatchPlayerInfo
                {
                    PlayerId = reader.ReadString(),
                    DisplayName = reader.ReadString(),
                    SkillRating = reader.ReadInt32()
                });
            }
            RaceType = (RaceType)reader.ReadByte();
            AcceptTimeoutSeconds = reader.ReadInt32();
        }
    }
    
    public class MatchPlayerInfo
    {
        public string PlayerId;
        public string DisplayName;
        public int SkillRating;
    }
    
    #endregion
    
    #region System Messages
    
    public class ErrorMessage : NetworkMessage
    {
        public int ErrorCode;
        public string ErrorText;
        
        public ErrorMessage() : base(MessageType.Error) { }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ErrorCode);
            writer.Write(ErrorText ?? "");
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            ErrorCode = reader.ReadInt32();
            ErrorText = reader.ReadString();
        }
    }
    
    public class PingMessage : NetworkMessage
    {
        public long ClientTimestamp;
        
        public PingMessage() : base(MessageType.Ping) 
        {
            Reliability = Reliability.Unreliable;
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientTimestamp);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            ClientTimestamp = reader.ReadInt64();
        }
    }
    
    public class PongMessage : NetworkMessage
    {
        public long ClientTimestamp;
        public long ServerTimestamp;
        
        public PongMessage() : base(MessageType.Pong) 
        {
            Reliability = Reliability.Unreliable;
        }
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientTimestamp);
            writer.Write(ServerTimestamp);
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            ClientTimestamp = reader.ReadInt64();
            ServerTimestamp = reader.ReadInt64();
        }
    }
    
    #endregion
    
    /// <summary>
    /// Serialization utilities for network messages.
    /// </summary>
    public static class MessageSerializer
    {
        private static readonly Dictionary<MessageType, Func<NetworkMessage>> MessageFactories = new()
        {
            { MessageType.Connect, () => new ConnectMessage() },
            { MessageType.Disconnect, () => new DisconnectMessage() },
            { MessageType.Heartbeat, () => new HeartbeatMessage() },
            { MessageType.Authenticate, () => new AuthenticateMessage() },
            { MessageType.CreateLobby, () => new CreateLobbyMessage() },
            { MessageType.JoinLobby, () => new JoinLobbyMessage() },
            { MessageType.LobbyUpdate, () => new LobbyUpdateMessage() },
            { MessageType.LobbyChat, () => new LobbyChatMessage() },
            { MessageType.GameStart, () => new GameStartMessage() },
            { MessageType.GameAction, () => new GameActionMessage() },
            { MessageType.TurnStart, () => new TurnStartMessage() },
            { MessageType.TurnEnd, () => new TurnEndMessage() },
            { MessageType.GameEnd, () => new GameEndMessage() },
            { MessageType.StateSnapshot, () => new StateSnapshotMessage() },
            { MessageType.StateDelta, () => new StateDeltaMessage() },
            { MessageType.SyncRequest, () => new SyncRequestMessage() },
            { MessageType.MatchFound, () => new MatchFoundMessage() },
            { MessageType.Error, () => new ErrorMessage() },
            { MessageType.Ping, () => new PingMessage() },
            { MessageType.Pong, () => new PongMessage() }
        };
        
        /// <summary>
        /// Serialize a message to bytes.
        /// </summary>
        public static byte[] Serialize(NetworkMessage message, bool compress = false)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Header
            writer.Write((byte)message.Type);
            writer.Write((byte)message.Reliability);
            writer.Write(message.SenderId ?? "");
            writer.Write(message.Timestamp);
            writer.Write(message.SequenceNumber);
            
            // Body
            message.Serialize(writer);
            
            var data = ms.ToArray();
            
            if (compress && data.Length > 256)
            {
                return Compress(data);
            }
            
            return data;
        }
        
        /// <summary>
        /// Deserialize bytes to a message.
        /// </summary>
        public static NetworkMessage Deserialize(byte[] data, bool decompress = false)
        {
            if (decompress)
            {
                data = Decompress(data);
            }
            
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            
            // Header
            var type = (MessageType)reader.ReadByte();
            var reliability = (Reliability)reader.ReadByte();
            var senderId = reader.ReadString();
            var timestamp = reader.ReadInt64();
            var sequence = reader.ReadUInt16();
            
            // Create message
            if (!MessageFactories.TryGetValue(type, out var factory))
            {
                throw new InvalidOperationException($"Unknown message type: {type}");
            }
            
            var message = factory();
            message.Reliability = reliability;
            message.SenderId = senderId;
            message.Timestamp = timestamp;
            message.SequenceNumber = sequence;
            
            // Body
            message.Deserialize(reader);
            
            return message;
        }
        
        private static byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();
            output.WriteByte(1); // Compression flag
            
            using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
            {
                gzip.Write(data, 0, data.Length);
            }
            
            return output.ToArray();
        }
        
        private static byte[] Decompress(byte[] data)
        {
            if (data[0] != 1) return data; // Not compressed
            
            using var input = new MemoryStream(data, 1, data.Length - 1);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }
    
    /// <summary>
    /// Tracks network latency and connection quality.
    /// </summary>
    public class ConnectionQuality
    {
        private readonly Queue<int> _latencySamples;
        private readonly Queue<float> _packetLossSamples;
        private readonly int _maxSamples = 100;
        
        public int CurrentLatency { get; private set; }
        public int AverageLatency => _latencySamples.Any() ? (int)_latencySamples.Average() : 0;
        public int Jitter { get; private set; }
        public float PacketLoss => _packetLossSamples.Any() ? _packetLossSamples.Average() : 0;
        
        public ConnectionQuality()
        {
            _latencySamples = new Queue<int>();
            _packetLossSamples = new Queue<float>();
        }
        
        public void RecordLatency(int latencyMs)
        {
            int previousLatency = CurrentLatency;
            CurrentLatency = latencyMs;
            
            _latencySamples.Enqueue(latencyMs);
            while (_latencySamples.Count > _maxSamples)
                _latencySamples.Dequeue();
            
            // Calculate jitter (variation in latency)
            Jitter = Math.Abs(latencyMs - previousLatency);
        }
        
        public void RecordPacketLoss(float lossRate)
        {
            _packetLossSamples.Enqueue(lossRate);
            while (_packetLossSamples.Count > _maxSamples)
                _packetLossSamples.Dequeue();
        }
        
        public string GetQualityRating()
        {
            if (AverageLatency < 50 && PacketLoss < 0.01f) return "Excellent";
            if (AverageLatency < 100 && PacketLoss < 0.03f) return "Good";
            if (AverageLatency < 200 && PacketLoss < 0.05f) return "Fair";
            return "Poor";
        }
    }
    
    /// <summary>
    /// Handles message sequencing and ordering.
    /// </summary>
    public class MessageSequencer
    {
        private ushort _outgoingSequence;
        private ushort _incomingSequence;
        private readonly Dictionary<ushort, NetworkMessage> _pendingMessages;
        
        public MessageSequencer()
        {
            _pendingMessages = new Dictionary<ushort, NetworkMessage>();
        }
        
        public ushort GetNextSequence()
        {
            return _outgoingSequence++;
        }
        
        public bool ProcessSequence(NetworkMessage message)
        {
            ushort expected = (ushort)(_incomingSequence + 1);
            
            if (message.SequenceNumber == expected)
            {
                _incomingSequence = message.SequenceNumber;
                return true;
            }
            
            if (message.Reliability == Reliability.ReliableOrdered)
            {
                // Queue out-of-order messages
                if (IsNewer(message.SequenceNumber, _incomingSequence))
                {
                    _pendingMessages[message.SequenceNumber] = message;
                }
                return false;
            }
            
            if (message.Reliability == Reliability.ReliableSequenced || 
                message.Reliability == Reliability.UnreliableSequenced)
            {
                // Only process if newer
                if (IsNewer(message.SequenceNumber, _incomingSequence))
                {
                    _incomingSequence = message.SequenceNumber;
                    return true;
                }
                return false;
            }
            
            return true; // Unreliable messages always processed
        }
        
        public List<NetworkMessage> GetPendingInOrder()
        {
            var result = new List<NetworkMessage>();
            
            while (true)
            {
                ushort next = (ushort)(_incomingSequence + 1);
                if (_pendingMessages.TryGetValue(next, out var msg))
                {
                    _pendingMessages.Remove(next);
                    _incomingSequence = next;
                    result.Add(msg);
                }
                else
                {
                    break;
                }
            }
            
            return result;
        }
        
        private bool IsNewer(ushort a, ushort b)
        {
            return (short)(a - b) > 0;
        }
    }
}
