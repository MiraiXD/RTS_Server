using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
namespace MultiplayerPlugin
{
    public class GameManager
    {
        private NetworkManager networkManager;
        private GameWorld gameWorld;
        private UnitManager unitManager;
        private Dictionary<ushort, Entities.PlayerBaseModel> players;

        private Thread worldUpdateThread;
        private bool updateWorld;
        private float networkDeltaTime;
        private DateTime lastWorldUpdateTime;
        private DateTime gameStartTime;
        private Stopwatch stopwatch;
        public GameManager(NetworkManager networkManager, Entities.PlayerNetworkModel[] networkedPlayers)
        {
            this.networkManager = networkManager;
            unitManager = new UnitManager();
            gameWorld = new GameWorld(unitManager);

            players = new Dictionary<ushort, Entities.PlayerBaseModel>();
            foreach(var networkModel in networkedPlayers)
            {
                Entities.PlayerBaseModel player = new Entities.PlayerBaseModel();
                players.Add(networkModel.networkID.ID, player);
            }

            worldUpdateThread = null;
            updateWorld = false;
        }
        public void StartGame()
        {
            gameStartTime = DateTime.Now;
            lastWorldUpdateTime = DateTime.Now;
            networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
            updateWorld = true;
            stopwatch = new Stopwatch();
            worldUpdateThread = new Thread(HandleWorldUpdate);
            worldUpdateThread.Start();
        }
        void HandleWorldUpdate()
        {
            while (updateWorld)
            {
                stopwatch.Restart();
                Messages.Server.WorldUpdate worldUpdateMessage = gameWorld.Update(networkDeltaTime);
                stopwatch.Stop();

                networkManager.SendWorldUpdate(worldUpdateMessage);

                networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
                lastWorldUpdateTime = DateTime.Now;

                Thread.Sleep(50);
            }
        }
        //void UpdateWorld(float deltaTime)
        //{
        //    w.Restart();
        //    Parallel.For(0, 50, (index) =>
        //    {
        //        PathFinder f = new PathFinder();
        //        f.FindPath(new GridPosition(0, 0), new GridPosition(129, 129), grid);
        //    });
        //    w.Stop();
        //    worldUpdateMessage.timeSinceStartup = w.ElapsedMilliseconds;
        //    //worldUpdateMessage.timeSinceStartup = DateTime.Now.Subtract(serverStartTime).TotalSeconds;

        //    worldUpdateMessage.x += 1f * deltaTime;
        //}

        public void OnPlayerSpawnUnit(NetworkIdentity callingPlayerID, Entities.BattleUnitModel.UnitType unitType)
        {
            Entities.PlayerBaseModel playerBase = players[callingPlayerID.ID];
            // check cost
            var unit = unitManager.CreateUnitWithPlayerAuthority(callingPlayerID, unitType);
            networkManager.SendPlayerSpawnUnit(unit);
        }
    }
}
