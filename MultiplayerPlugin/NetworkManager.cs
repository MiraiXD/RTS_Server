using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using Microsoft.Playfab.Gaming.GSDK.CSharp;
namespace MultiplayerPlugin
{
    public class NetworkManager : Plugin
    {
        public override bool ThreadSafe => false;
        public override Version Version => new Version(1, 0, 0);
        public Dictionary<IClient, Entities.Player> players;

        Dictionary<ushort, Action<Message, object, MessageReceivedEventArgs>> clientMessage_Actions;
        DateTime startDateTime;
        bool sessionIdAssigned;
        int maxPlayers = 3;
        string map = "Default_4";

        public NetworkManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            players = new Dictionary<IClient, Entities.Player>();
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;

            GameserverSDK.RegisterShutdownCallback(OnShutdown);
            GameserverSDK.RegisterHealthCallback(OnHealthCheck);
            sessionIdAssigned = false;

            clientMessage_Actions = new Dictionary<ushort, Action<Message, object, MessageReceivedEventArgs>>();
            clientMessage_Actions.Add(Messages.Player.Hello.Tag, OnPlayerHelloMessage);
            clientMessage_Actions.Add(Messages.Player.ReadyToStartGame.Tag, OnPlayerReadyToStartGameMessage);

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
        }

        private void OnPlayerReadyToStartGameMessage(Message message, object sender, MessageReceivedEventArgs e)
        {
            players[e.Client].isReady = true;

            bool allReady = true;
            foreach (IClient client in ClientManager.GetAllClients())
            {
                if (!players[client].isReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(new Messages.Server.StartGame());
                    using (Message m = Message.Create(Messages.Server.StartGame.Tag, writer))
                    {
                        foreach (IClient client in ClientManager.GetAllClients())
                        {
                            client.SendMessage(m, SendMode.Reliable);
                        }
                    }
                }
                StartUpdating();
            }
        }
        private Thread worldUpdateThread;
        private Messages.Server.WorldUpdate worldUpdateMessage;
        float updateRate = 0.02f;
        bool updateWorld = false;
        void StartUpdating()
        {
            worldUpdateMessage = new Messages.Server.WorldUpdate();
            worldUpdateMessage.x = 250f;
            worldUpdateMessage.z = 250f;
            updateWorld = true;
            worldUpdateThread = new Thread(() =>
            {
                while (updateWorld)
                {
                    Update();
                    Thread.Sleep(20);

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(worldUpdateMessage);
                        using (Message message = Message.Create(Messages.Server.WorldUpdate.Tag, writer))
                        {
                            foreach (IClient client in ClientManager.GetAllClients())
                            {
                                client.SendMessage(message, SendMode.Unreliable);
                            }
                        }
                    }
                }
            });
            worldUpdateThread.Start();
        }
        void Update()
        {
            worldUpdateMessage.x += 2f * updateRate;
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
            if (awakeTime > 100f && players.Count <= 0 && sessionIdAssigned)
            {
                OnShutdown();
                return false;
            }

            return true;
        }
        void UpdatePlayFabPlayers()
        {
            List<ConnectedPlayer> listPfPlayers = new List<ConnectedPlayer>();
            foreach (KeyValuePair<IClient, Entities.Player> player in players)
            {
                listPfPlayers.Add(new ConnectedPlayer(player.Value.playerName));
            }

            GameserverSDK.UpdateConnectedPlayers(listPfPlayers);
        }
        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // Remove player from connected players
            e.Client.MessageReceived -= OnMessageReceived;
            players.Remove(e.Client);

            // Tell all clients about player disconnection
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                var playerDisconnectedMessage = new Messages.Server.PlayerDisconnected() { ID = e.Client.ID };
                writer.Write(playerDisconnectedMessage);

                using (Message message = Message.Create(Messages.Server.PlayerDisconnected.Tag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
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
                if (clientMessage_Actions.TryGetValue(message.Tag, out Action<Message, object, MessageReceivedEventArgs> action))
                {
                    action(message, sender, e);
                }
                else
                {
                    Console.WriteLine("No such tag!");
                }
            }
        }
        void OnPlayerHelloMessage(Message playerMessage, object sender, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = playerMessage.GetReader())
            {
                Messages.Player.Hello helloMessage = reader.ReadSerializable<Messages.Player.Hello>();
                string playerName = helloMessage.playerName;

                Entities.Player newPlayer = new Entities.Player(e.Client.ID, playerName);
                players.Add(e.Client, newPlayer);

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    Messages.Server.ConnectedPlayers message = new Messages.Server.ConnectedPlayers();
                    message.maxPlayers = maxPlayers;
                    message.connectedPlayers_Size = players.Count;
                    message.connectedPlayers = new Entities.Player[message.connectedPlayers_Size];
                    int i = 0;
                    foreach (var p in players.Values)
                    {
                        message.connectedPlayers[i++] = p;
                    }

                    writer.Write(message);
                    using (Message serverMessage = Message.Create(Messages.Server.ConnectedPlayers.Tag, writer))
                    {
                        foreach (IClient client in ClientManager.GetAllClients())
                        {
                            client.SendMessage(serverMessage, SendMode.Reliable);
                        }
                    }
                }
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
        //void OnPlayerInformationMessage(object sender, MessageReceivedEventArgs e, Message message)
        //{
        //    using (Message message = e.GetMessage() as Message)
        //    {
        //        if (message.Tag == Tags.PlayerInformationTag)
        //        {
        //            using (DarkRiftReader reader = message.GetReader())
        //            {
        //                string playerName = reader.ReadString();

        //                // Update player information
        //                players[e.Client].playerName = playerName;

        //                // Update all players
        //                using (DarkRiftWriter writer = DarkRiftWriter.Create())
        //                {
        //                    writer.Write(e.Client.ID);
        //                    writer.Write(playerName);

        //                    message.Serialize(writer);
        //                }

        //                foreach (IClient client in ClientManager.GetAllClients())
        //                {
        //                    client.SendMessage(message, e.SendMode);
        //                }
        //            }
        //        }
        //    }
        //}
        //void OnPlayerReadyMessage(Message message, object sender, MessageReceivedEventArgs e)
        //{
        //    using (DarkRiftReader reader = message.GetReader())
        //    {
        //        bool isReady = reader.ReadBoolean();

        //        // Update player ready status and check if all players are ready
        //        players[e.Client].isReady = isReady;
        //        CheckAllReady();
        //    }
        //}

        //void CheckAllReady()
        //{
        //    // Check all clients, if any not ready, then return
        //    foreach (IClient client in ClientManager.GetAllClients())
        //    {
        //        if (!players[client].isReady)
        //        {
        //            return;
        //        }
        //    }

        //    StartGame();
        //}
        //void StartGame()
        //{
        //    // If all are ready, broadcast start game to all clients
        //    using (DarkRiftWriter writer = DarkRiftWriter.Create())
        //    {
        //        using (Message message = Message.Create(Tags.StartGameTag, writer))
        //        {
        //            foreach (IClient client in ClientManager.GetAllClients())
        //            {
        //                client.SendMessage(message, SendMode.Reliable);
        //            }
        //        }
        //    }
        //}
        //void OnPlayerMoveMessage(Message message, object sender, MessageReceivedEventArgs e)
        //{
        //    using (DarkRiftReader reader = message.GetReader())
        //    {
        //        float newX = reader.ReadSingle();
        //        float newY = reader.ReadSingle();
        //        float newZ = reader.ReadSingle();

        //        Player player = players[e.Client];

        //        player.X = newX;
        //        player.Y = newY;
        //        player.Z = newZ;

        //        // send this player's updated position back to all clients except the client that sent the message
        //        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        //        {
        //            writer.Write(player.ID);
        //            writer.Write(player.X);
        //            writer.Write(player.Y);
        //            writer.Write(player.Z);

        //            message.Serialize(writer);
        //        }

        //        foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
        //            client.SendMessage(message, e.SendMode);
        //    }
        //}
    }
}
