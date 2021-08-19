using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;
using System.IO;
namespace MultiplayerPlugin
{
    public class GameManager
    {
        private NetworkManager networkManager;
        private GameWorld gameWorld;
        private UnitManager unitManager;
        private NavGrid navGrid;
        private Pathfinding pathfinding;
        public static GameData gameData;
        //private Dictionary<ushort, Entities.PlayerBaseModel> players;

        private Thread worldUpdateThread;
        private bool updateWorld;
        private Thread pathfindingThread;
        private bool keepPathfinding;
        
        private float networkDeltaTime;
        private DateTime lastWorldUpdateTime;
        private DateTime gameStartTime;
        private Stopwatch stopwatch;

        private string mapName;
        
        public GameManager(NetworkManager networkManager, NetworkedPlayer[] networkedPlayers, string mapName)
        {
            worldUpdateThread = null;
            updateWorld = false;
            pathfindingThread = null;
            keepPathfinding = false;
            this.networkManager = networkManager;

            this.mapName = mapName;            

            unitManager = new UnitManager();
            gameWorld = new GameWorld();
            navGrid = new NavGrid(mapName+".txt");
            pathfinding = new Pathfinding(navGrid);

            gameData = new Default2v2();

            Messages.Server.WorldInfo worldInfoMessage = new Messages.Server.WorldInfo();

            gameWorld.CreatePlayerBases(networkedPlayers);
            worldInfoMessage.models_Count = networkedPlayers.Length;
            worldInfoMessage.playerIDs = new NetworkIdentity[networkedPlayers.Length];
            worldInfoMessage.playerModels = new Entities.NetworkedPlayerModel[networkedPlayers.Length];
            worldInfoMessage.baseIDs = new NetworkIdentity[networkedPlayers.Length];
            worldInfoMessage.baseModels = new Entities.PlayerBaseModel[networkedPlayers.Length];
            for (int i = 0; i < networkedPlayers.Length; i++)
            {
                worldInfoMessage.playerIDs[i] = networkedPlayers[i].networkID;
                worldInfoMessage.playerModels[i] = networkedPlayers[i].model;
                PlayerBase playerBase = gameWorld.GetPlayerBase(networkedPlayers[i].networkID.ID);
                worldInfoMessage.baseIDs[i] = playerBase.networkID;
                worldInfoMessage.baseModels[i] = playerBase.model;
            }

            //gameWorld.CreateResources();

            //

            networkManager.SendMessageToAll(worldInfoMessage, Messages.Server.WorldInfo.Tag, DarkRift.SendMode.Reliable);
            

            networkManager.SendMessageToAll(new Messages.Server.StartGame(), Messages.Server.StartGame.Tag, DarkRift.SendMode.Reliable);
            StartGame();
        }

        

        public void StartGame()
        {
            gameStartTime = DateTime.Now;
            lastWorldUpdateTime = DateTime.Now;
            networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
            stopwatch = new Stopwatch();

            updateWorld = true;            
            worldUpdateThread = new Thread(HandleWorldUpdate);
            worldUpdateThread.Start();

            //keepPathfinding = true;
            //pathfindingThread = new Thread(HandlePathfinding);
            //pathfindingThread.Start();
        }
        //void HandlePathfinding()
        //{
        //    while(keepPathfinding)
        //    {
        //        pathfinding.FindAllPathsParallel();

        //        Thread.Sleep(50);
        //    }
        //}
        void HandleWorldUpdate()
        {
            while (updateWorld)
            {
                //stopwatch.Restart();
                pathfinding.FindAllPathsParallel();
                unitManager.UpdateUnits(networkDeltaTime);
                if (gameWorld.HasUpdate())
                {
                    Messages.Server.WorldUpdate worldUpdateMessage = gameWorld.GetUpdateMessage();
                    worldUpdateMessage.timeSinceStartup = (float)DateTime.Now.Subtract(gameStartTime).TotalSeconds;
                    //stopwatch.Stop();

                    networkManager.SendMessageToAll(worldUpdateMessage, Messages.Server.WorldUpdate.Tag, DarkRift.SendMode.Unreliable);
                    gameWorld.ClearChanges();
                }
                networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
                lastWorldUpdateTime = DateTime.Now;

                Thread.Sleep(50);
            }
        }       
        public void OnPlayerSpawnUnit(NetworkIdentity callingPlayerID, Entities.BattleUnitModel.UnitType unitType)
        {
            PlayerBase playerBase = gameWorld.GetPlayerBase(callingPlayerID.ID);
            // check cost
            BattleUnit unit = unitManager.CreateUnitWithPlayerAuthority(callingPlayerID, unitType);
            unit.InitPathfinding(navGrid, pathfinding, navGrid.grid.GetNode(new GridPosition(5, 5)));
            unit.InitWorldChanges(gameWorld);

            Messages.Server.SpawnUnit spawnUnitMessage = new Messages.Server.SpawnUnit();
            spawnUnitMessage.owningPlayerID = unit.owningPlayerID;
            spawnUnitMessage.unitID = unit.networkID;
            spawnUnitMessage.unitModel = unit.model;
            //networkManager.SendPlayerSpawnUnit(unit);
            networkManager.SendMessageToAll(spawnUnitMessage, Messages.Server.SpawnUnit.Tag, DarkRift.SendMode.Reliable);
        }
        public void OnPlayerMoveUnit(NetworkIdentity callingPlayerID, Messages.Client.MoveUnit message)
        {
            BattleUnit unit = unitManager.GetUnit(message.unitID.ID);
            if(unit.owningPlayerID.ID == callingPlayerID.ID)
            {
                Console.WriteLine(navGrid.GetNodeCenterWorld( navGrid.grid.GetNode(new GridPosition(message.nodeXCoord, message.nodeYCoord))).ToString());
                unit.SetDestination(navGrid.grid.GetNode(new GridPosition(message.nodeXCoord, message.nodeYCoord)));
            }
        }
    }
}
