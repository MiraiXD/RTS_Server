using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using Microsoft.Playfab.Gaming.GSDK.CSharp;

using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;
using Roy_T.AStar.Paths;
using System.Threading.Tasks;
using System.Diagnostics;
namespace MultiplayerPlugin
{
    public class NetworkManager : Plugin
    {
        public override bool ThreadSafe => false;
        public override Version Version => new Version(1, 0, 0);
        public Dictionary<ushort, NetworkedPlayer> players;

        Dictionary<ushort, Action<Message, MessageReceivedEventArgs>> clientMessage_Actions;
        DateTime startDateTime;
        bool sessionIdAssigned;
        int maxPlayers = 2;
        string map = "Default_4_players";

        private GameManager gameManager;
           
        public NetworkManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {        
            players = new Dictionary<ushort, NetworkedPlayer>();
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;

            GameserverSDK.RegisterShutdownCallback(OnShutdown);
            GameserverSDK.RegisterHealthCallback(OnHealthCheck);
            sessionIdAssigned = false;

            clientMessage_Actions = new Dictionary<ushort, Action<Message, MessageReceivedEventArgs>>();
            clientMessage_Actions.Add(Messages.Client.Hello.Tag, OnPlayerHelloMessage);
            clientMessage_Actions.Add(Messages.Client.ReadyToStartGame.Tag, OnPlayerReadyToStartGameMessage);
            clientMessage_Actions.Add(Messages.Client.SpawnUnit.Tag, OnPlayerSpawnUnitMessage);
            clientMessage_Actions.Add(Messages.Client.MoveUnits.Tag, OnPlayerMoveUnitMessage);

            // Connect to PlayFab agent
            GameserverSDK.Start();
            if (GameserverSDK.ReadyForPlayers())
            {
                // returns true on allocation call, player about to connect
            }
            else
            {
                // returns false when server is being terminated
            }            
            //gameManager = new GameManager(this, players.Values.ToArray(), map);
        }

        private void OnPlayerMoveUnitMessage(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                Messages.Client.MoveUnits clientMessage = reader.ReadSerializable<Messages.Client.MoveUnits>();
                NetworkIdentity callingPlayerID = players[e.Client.ID].networkID;                
                gameManager.OnPlayerMoveUnits(callingPlayerID, clientMessage);
            }
        }
        private void OnPlayerSpawnUnitMessage(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader()) {
                Messages.Client.SpawnUnit clientMessage = reader.ReadSerializable<Messages.Client.SpawnUnit>();
                NetworkIdentity callingPlayerID = players[e.Client.ID].networkID;                
                var unitType = clientMessage.unitType;
                gameManager.OnPlayerSpawnUnit(callingPlayerID, unitType);
            }
        }       
        private void OnPlayerReadyToStartGameMessage(Message message, MessageReceivedEventArgs e)
        {
            players[e.Client.ID].model.isReady = true;

            if (ClientManager.GetAllClients().Length < maxPlayers) return;

            bool allReady = true;
            foreach (IClient client in ClientManager.GetAllClients())
            {
                if (!players[client.ID].model.isReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                gameManager = new GameManager(this, players.Values.ToArray(), map);                              
            }
        }
        private void OnPlayerHelloMessage(Message playerMessage, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = playerMessage.GetReader())
            {
                Messages.Client.Hello helloMessage = reader.ReadSerializable<Messages.Client.Hello>();
                string playerName = helloMessage.playerName;

                //Entities.NetworkedPlayerModel newPlayer = new Entities.NetworkedPlayerModel(e.Client.ID, playerName);
                NetworkedPlayer newPlayer = new NetworkedPlayer(e.Client.ID, playerName);
                players.Add(e.Client.ID, newPlayer);

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    Messages.Server.ConnectedPlayers message = new Messages.Server.ConnectedPlayers();
                    message.maxPlayers = maxPlayers;
                    message.connectedPlayers_Size = players.Count;
                    message.IDs = new NetworkIdentity[message.connectedPlayers_Size];
                    message.connectedPlayers = new Entities.NetworkedPlayerModel[message.connectedPlayers_Size];
                    int i = 0;
                    foreach (var p in players.Values)
                    {
                        message.IDs[i] = p.networkID;
                        message.connectedPlayers[i] = p.model;
                        i++;
                    }

                    writer.Write(message);
                    using (Message serverMessage = Message.Create(Messages.Server.ConnectedPlayers.Tag, writer))
                    {
                        SendToAll(serverMessage, SendMode.Reliable);
                    }
                }
                Thread.Sleep(100);
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    Messages.Server.LoadMap message = new Messages.Server.LoadMap();
                    message.mapName = map;
                    writer.Write(message);
                    using (Message serverMessage = Message.Create(Messages.Server.LoadMap.Tag, writer))
                    {
                        e.Client.SendMessage(serverMessage, SendMode.Reliable);
                    }
                }
            }


            UpdatePlayFabPlayers();
        }
        public void SendMessageToAll(IDarkRiftSerializable message, ushort tag, SendMode sendMode)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<IDarkRiftSerializable>(message);
                using (Message m = Message.Create(tag, writer))
                {
                    SendToAll(m, sendMode);
                }
            }
        }
        private void SendToAll(Message message, SendMode sendMode)
        {
            foreach (var client in ClientManager.GetAllClients()) client.SendMessage(message, sendMode);
        }
        void OnShutdown()
        {
            Environment.Exit(1);
        }

        bool OnHealthCheck()
        {
            // How long has server been active in seconds?
            float awakeTime;

            if (!sessionIdAssigned)
            {
                awakeTime = 0f;
            }
            else
            {
                awakeTime = (float)(DateTime.Now - startDateTime).TotalSeconds;
            }

            // Get server info
            // If session ID has been assigned, server is active
            IDictionary<string, string> config = GameserverSDK.getConfigSettings();
            if (config.TryGetValue(GameserverSDK.SessionIdKey, out string sessionId))
            {
                // If this is the first session assignment, start the activated timer
                if (!sessionIdAssigned)
                {
                    startDateTime = DateTime.Now;
                    sessionIdAssigned = true;
                }
            }
            // If server has been awake for over 10 mins, and no players connected, and the PlayFab server is not in standby (no session id assigned): begin shutdown
            if (awakeTime > 60f && players.Count <= 0 && sessionIdAssigned)
            {
                OnShutdown();
                return false;
            }

            return true;
        }
        void UpdatePlayFabPlayers()
        {
            List<ConnectedPlayer> listPfPlayers = new List<ConnectedPlayer>();
            foreach (KeyValuePair<ushort, NetworkedPlayer> player in players)
            {
                listPfPlayers.Add(new ConnectedPlayer(player.Value.model.playerName));
            }

            GameserverSDK.UpdateConnectedPlayers(listPfPlayers);
        }
        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // Remove player from connected players
            e.Client.MessageReceived -= OnMessageReceived;
            players.Remove(e.Client.ID);

            // Tell all clients about player disconnection
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                var playerDisconnectedMessage = new Messages.Server.PlayerDisconnected() { ID = e.Client.ID };
                writer.Write(playerDisconnectedMessage);

                using (Message message = Message.Create(Messages.Server.PlayerDisconnected.Tag, writer))
                {
                    SendToAll(message, SendMode.Reliable);                    
                }
            }

            UpdatePlayFabPlayers();
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessageReceived;
        }
        void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                if (clientMessage_Actions.TryGetValue(message.Tag, out Action<Message, MessageReceivedEventArgs> action))
                {
                    action(message, e);
                }
                else
                {
                    Console.WriteLine("No such tag!");
                }
            }
        }
        
        
    }
}
