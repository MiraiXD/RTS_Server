using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using System.IO;
namespace MultiplayerPlugin
{
    public class GameManager
    {
        private NetworkManager networkManager;
        private GameWorld gameWorld;
        private UnitManager unitManager;
        //private Dictionary<ushort, Entities.PlayerBaseModel> players;

        private Thread worldUpdateThread;
        private bool updateWorld;
        private float networkDeltaTime;
        private DateTime lastWorldUpdateTime;
        private DateTime gameStartTime;
        private Stopwatch stopwatch;

        private string mapName;
        private Grid grid;
        public GameManager(NetworkManager networkManager, Entities.PlayerNetworkModel[] networkedPlayers, string mapName)
        {
            worldUpdateThread = null;
            updateWorld = false;
            this.networkManager = networkManager;

            this.mapName = mapName;
            grid = CreateGrid(mapName + ".txt");

            unitManager = new UnitManager();
            gameWorld = new GameWorld(unitManager);

            

            Messages.Server.WorldInfo worldInfoMessage = new Messages.Server.WorldInfo();

            gameWorld.CreatePlayerBases(networkedPlayers);
            worldInfoMessage.models_Count = networkedPlayers.Length;
            worldInfoMessage.playerModels = new Entities.PlayerNetworkModel[networkedPlayers.Length];
            worldInfoMessage.baseModels = new Entities.PlayerBaseModel[networkedPlayers.Length];
            for (int i = 0; i < networkedPlayers.Length; i++)
            {
                worldInfoMessage.playerModels[i] = networkedPlayers[i];
                worldInfoMessage.baseModels[i] = gameWorld.GetPlayerBase(networkedPlayers[i].networkID.ID).model;
            }

            //gameWorld.CreateResources();

            //

            networkManager.SendMessageToAll(worldInfoMessage, Messages.Server.WorldInfo.Tag, DarkRift.SendMode.Reliable);
            

            networkManager.SendMessageToAll(new Messages.Server.StartGame(), Messages.Server.StartGame.Tag, DarkRift.SendMode.Reliable);
            StartGame();
        }

        private Grid CreateGrid(string pathToTxt)
        {
            using (StreamReader reader = File.OpenText(pathToTxt))
            {
                string line = reader.ReadLine();
                string widthString = line.Split('=')[1].Trim();
                int width = int.Parse(widthString);

                line = reader.ReadLine();
                string heightString = line.Split('=')[1].Trim();
                int height = int.Parse(heightString);

                line = reader.ReadLine();
                string cellSizeXString = line.Split('=')[1].Trim();
                float cellSizeX = float.Parse(cellSizeXString);

                line = reader.ReadLine();
                string cellSizeYString = line.Split('=')[1].Trim();
                float cellSizeY = float.Parse(cellSizeYString);

                Grid grid = Grid.CreateGridWithLateralAndDiagonalConnections(new GridSize(width, height), new Size(Distance.FromMeters(cellSizeX), Distance.FromMeters(cellSizeY)), Velocity.FromMetersPerSecond(1f));

                line = reader.ReadLine();
                string walkableNodesLengthString = line.Split('=')[1].Trim();
                int walkableNodesLength = int.Parse(walkableNodesLengthString);

                for(int i=0; i<walkableNodesLength;i++)
                {
                    reader.ReadLine();
                }

                line = reader.ReadLine();
                string obstaclesLengthString = line.Split('=')[1].Trim();
                int obstaclesLength = int.Parse(obstaclesLengthString);

                for(int i=0; i<obstaclesLength;i++)
                {
                    line = reader.ReadLine();
                    var coordsString = line.Split(':');
                    int coordX = int.Parse(coordsString[0].Trim());
                    int coordY = int.Parse(coordsString[1].Trim());
                    GridPosition obstaclePosition = new GridPosition(coordX, coordY);
                    grid.DisconnectNode(obstaclePosition);
                }

                return grid;
            }                        
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
                worldUpdateMessage.timeSinceStartup = (float)DateTime.Now.Subtract(gameStartTime).TotalSeconds;
                stopwatch.Stop();

                //networkManager.SendWorldUpdate(worldUpdateMessage);
                networkManager.SendMessageToAll(worldUpdateMessage, Messages.Server.WorldUpdate.Tag, DarkRift.SendMode.Unreliable);

                networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
                lastWorldUpdateTime = DateTime.Now;

                Thread.Sleep(50);
            }
        }       
        public void OnPlayerSpawnUnit(NetworkIdentity callingPlayerID, Entities.BattleUnitModel.UnitType unitType)
        {
            PlayerBase playerBase = gameWorld.GetPlayerBase(callingPlayerID.ID);
            // check cost
            var unit = unitManager.CreateUnitWithPlayerAuthority(callingPlayerID, unitType);
            Messages.Server.SpawnUnit spawnUnitMessage = new Messages.Server.SpawnUnit();
            spawnUnitMessage.owningPlayerID = unit.owningPlayerID;
            spawnUnitMessage.unitID = unit.ID;
            spawnUnitMessage.unitModel = unit.model;
            //networkManager.SendPlayerSpawnUnit(unit);
            networkManager.SendMessageToAll(spawnUnitMessage, Messages.Server.SpawnUnit.Tag, DarkRift.SendMode.Reliable);
        }
    }
}
