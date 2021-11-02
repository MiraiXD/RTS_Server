using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;
using System.IO;
using System.Collections.Concurrent;

namespace MultiplayerPlugin
{
    public class GameManager
    {
        private NetworkManager networkManager;
        private GameWorld gameWorld;        
        //private NavGrid navGrid;
        //private Pathfinding pathfinding;
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

        private List<Action> syncedActions;

        //private NavMesh navMesh;
        public GameManager(NetworkManager networkManager, NetworkedPlayer[] networkedPlayers, string mapName)
        {
            syncedActions = new List<Action>();
            worldUpdateThread = null;
            updateWorld = false;
            pathfindingThread = null;
            keepPathfinding = false;
            this.networkManager = networkManager;

            this.mapName = mapName;
            
            gameWorld = new GameWorld();
            //navGrid = new NavGrid(mapName + ".txt", Velocity.FromMetersPerSecond(1f));
            //pathfinding = new Pathfinding(navGrid);
            gameData = new Default2v2();

            DecodeMapInfo(mapName + ".txt", Velocity.FromMetersPerSecond(1f));

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


            //networkManager.SendMessageToAll(new Messages.Server.StartGame(), Messages.Server.StartGame.Tag, DarkRift.SendMode.Reliable);
            Messages.Server.StartGame m = new Messages.Server.StartGame();
            m.length = NavMesh.triangles.Length * 3;
            m.vertices = new Vector3[m.length];
            m.triangles = new int[m.length];
            for (int i = 0; i < NavMesh.triangles.Length; i++)
            {
                m.vertices[3 * i + 0] = NavMesh.triangles[i].points[0];
                m.vertices[3 * i + 1] = NavMesh.triangles[i].points[1];
                m.vertices[3 * i + 2] = NavMesh.triangles[i].points[2];
                m.triangles[3 * i + 0] = 3 * i + 0;
                m.triangles[3 * i + 1] = 3 * i + 1;
                m.triangles[3 * i + 2] = 3 * i + 2;
            }
            networkManager.SendMessageToAll(m, Messages.Server.StartGame.Tag, DarkRift.SendMode.Reliable);
            StartGame();
        }

        private void DecodeMapInfo(string pathToFile, Velocity velocity)
        {
            using (StreamReader reader = File.OpenText(pathToFile))
            {
                string line;

                line = reader.ReadLine();
                string mapOriginXString = line.Split(':')[1].Trim();
                float mapOriginX = float.Parse(mapOriginXString);

                line = reader.ReadLine();
                string mapOriginYString = line.Split(':')[1].Trim();
                float mapOriginY = float.Parse(mapOriginYString);

                line = reader.ReadLine();
                string mapOriginZString = line.Split(':')[1].Trim();
                float mapOriginZ = float.Parse(mapOriginZString);

                line = reader.ReadLine();
                string mapWidthString = line.Split(':')[1].Trim();
                float mapWidth = float.Parse(mapWidthString);

                line = reader.ReadLine();
                string mapHeightString = line.Split(':')[1].Trim();
                float mapHeight = float.Parse(mapHeightString);

                line = reader.ReadLine();
                string vertexCountString = line.Split(':')[1].Trim();
                int vertexCount = int.Parse(vertexCountString);
                Vector3[] vertices = new Vector3[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    line = reader.ReadLine();
                    string[] vertexCoordsString = line.Split(':');
                    Vector3 vertex = new Vector3(float.Parse(vertexCoordsString[0].Trim()), float.Parse(vertexCoordsString[1].Trim()), float.Parse(vertexCoordsString[2].Trim()));
                    vertices[i] = vertex;
                }

                line = reader.ReadLine();
                string indexCountString = line.Split(':')[1].Trim();
                int indexCount = int.Parse(indexCountString);
                int[] indices = new int[indexCount];

                for (int i = 0; i < indexCount; i += 3)
                {
                    line = reader.ReadLine();

                    string[] triangleIndicesString = line.Split(':');
                    int indexA = int.Parse(triangleIndicesString[0].Trim());
                    int indexB = int.Parse(triangleIndicesString[1].Trim());
                    int indexC = int.Parse(triangleIndicesString[2].Trim());
                    indices[i] = indexA;
                    indices[i + 1] = indexB;
                    indices[i + 2] = indexC;
                }
                MapManager.Init(new Vector3(mapOriginX, mapOriginY, mapOriginZ), mapWidth, mapHeight);
                NavMesh.Init(vertices, indices, velocity);
                MapManager.AssignTrianglesToSectors(NavMesh.triangles);        
            }
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


        }
        private void CompleteSyncedActions()
        {
            lock (syncedActions)
            {
                foreach (var action in syncedActions)
                    action.Invoke();

                syncedActions.Clear();
            }
        }
        private void HandleWorldUpdate()
        {
            while (updateWorld)
            {
                //stopwatch.Restart();
                CompleteSyncedActions();                
                NavMesh.FindAllPathsParallel();
                Physics.CheckCollisions();
                UnitManager.UpdateUnits(networkDeltaTime);
                

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
            lock (syncedActions)
            {
                syncedActions.Add(() =>
                {
                    PlayerBase playerBase = gameWorld.GetPlayerBase(callingPlayerID.ID);
                    // check cost
                    BattleUnit unit = UnitManager.CreateUnitWithPlayerAuthority(callingPlayerID, unitType);
                    Vector3 startingPosition = MapManager.mapOrigin + new Vector3(5f, 0f, 5f);
                    unit.InitPathfinding(startingPosition);
                    unit.InitWorldChanges(gameWorld);

                    SendPlayerSpawnUnit(unit);
                });
            }
        }
        public void SendPlayerSpawnUnit(BattleUnit unit)
        {
            Messages.Server.SpawnUnit spawnUnitMessage = new Messages.Server.SpawnUnit();
            spawnUnitMessage.owningPlayerID = unit.owningPlayerID;
            spawnUnitMessage.unitID = unit.networkID;
            spawnUnitMessage.unitModel = unit.model;
            networkManager.SendMessageToAll(spawnUnitMessage, Messages.Server.SpawnUnit.Tag, DarkRift.SendMode.Reliable);
        }
        public void OnPlayerMoveUnits(NetworkIdentity callingPlayerID, Messages.Client.MoveUnits message)
        {
            lock (syncedActions)
            {
                syncedActions.Add(() =>
                {
                    for (int i = 0; i < message.unitsCount; i++)
                    {
                        BattleUnit unit = UnitManager.GetUnit(message.unitIDs[i].ID);
                        if (unit.owningPlayerID.ID == callingPlayerID.ID)
                        {
                            //unit.SetDestination(navGrid.grid.GetNode(new GridPosition(message.nodeXCoords[i], message.nodeYCoords[i])));
                            Vector3 position = new Vector3(message.worldPositionX, message.worldPositionY, message.worldPositionZ);
                            unit.SetDestination(position);
                        }
                        else
                            Console.WriteLine("WRONG ID");
                    }
                });
            }
        }
    }
}










//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using Roy_T.AStar.Grids;
//using Roy_T.AStar.Primitives;
//using System.IO;
//using System.Collections.Concurrent;

//namespace MultiplayerPlugin
//{
//    public class GameManager
//    {
//        private NetworkManager networkManager;
//        private GameWorld gameWorld;
//        private UnitManager unitManager;
//        //private NavGrid navGrid;
//        //private Pathfinding pathfinding;
//        public static GameData gameData;
//        //private Dictionary<ushort, Entities.PlayerBaseModel> players;

//        private Thread worldUpdateThread;
//        private bool updateWorld;
//        private Thread pathfindingThread;
//        private bool keepPathfinding;

//        private float networkDeltaTime;
//        private DateTime lastWorldUpdateTime;
//        private DateTime gameStartTime;
//        private Stopwatch stopwatch;

//        private string mapName;

//        private List<Action> syncedActions;

//        //private NavMesh navMesh;
//        public GameManager(NetworkManager networkManager, NetworkedPlayer[] networkedPlayers, string mapName)
//        {
//            syncedActions = new List<Action>();
//            worldUpdateThread = null;
//            updateWorld = false;
//            pathfindingThread = null;
//            keepPathfinding = false;
//            this.networkManager = networkManager;

//            this.mapName = mapName;

//            unitManager = new UnitManager();
//            gameWorld = new GameWorld();
//            //navGrid = new NavGrid(mapName + ".txt", Velocity.FromMetersPerSecond(1f));
//            //pathfinding = new Pathfinding(navGrid);
//            gameData = new Default2v2();

//            DecodeMapInfo(mapName + ".txt", Velocity.FromMetersPerSecond(1f));

//            Messages.Server.WorldInfo worldInfoMessage = new Messages.Server.WorldInfo();

//            gameWorld.CreatePlayerBases(networkedPlayers);
//            worldInfoMessage.models_Count = networkedPlayers.Length;
//            worldInfoMessage.playerIDs = new NetworkIdentity[networkedPlayers.Length];
//            worldInfoMessage.playerModels = new Entities.NetworkedPlayerModel[networkedPlayers.Length];
//            worldInfoMessage.baseIDs = new NetworkIdentity[networkedPlayers.Length];
//            worldInfoMessage.baseModels = new Entities.PlayerBaseModel[networkedPlayers.Length];
//            for (int i = 0; i < networkedPlayers.Length; i++)
//            {
//                worldInfoMessage.playerIDs[i] = networkedPlayers[i].networkID;
//                worldInfoMessage.playerModels[i] = networkedPlayers[i].model;
//                PlayerBase playerBase = gameWorld.GetPlayerBase(networkedPlayers[i].networkID.ID);
//                worldInfoMessage.baseIDs[i] = playerBase.networkID;
//                worldInfoMessage.baseModels[i] = playerBase.model;
//            }

//            //gameWorld.CreateResources();

//            //

//            networkManager.SendMessageToAll(worldInfoMessage, Messages.Server.WorldInfo.Tag, DarkRift.SendMode.Reliable);


//            networkManager.SendMessageToAll(new Messages.Server.StartGame(), Messages.Server.StartGame.Tag, DarkRift.SendMode.Reliable);
//            StartGame();
//        }

//        private void DecodeMapInfo(string pathToFile, Velocity velocity)
//        {
//            using (StreamReader reader = File.OpenText(pathToFile))
//            {
//                string line;

//                line = reader.ReadLine();
//                string mapOriginXString = line.Split(':')[1].Trim();
//                float mapOriginX = float.Parse(mapOriginXString);

//                line = reader.ReadLine();
//                string mapOriginYString = line.Split(':')[1].Trim();
//                float mapOriginY = float.Parse(mapOriginYString);

//                line = reader.ReadLine();
//                string mapOriginZString = line.Split(':')[1].Trim();
//                float mapOriginZ = float.Parse(mapOriginZString);

//                line = reader.ReadLine();
//                string mapWidthString = line.Split(':')[1].Trim();
//                float mapWidth = float.Parse(mapWidthString);

//                line = reader.ReadLine();
//                string mapHeightString = line.Split(':')[1].Trim();
//                float mapHeight = float.Parse(mapHeightString);


//                line = reader.ReadLine();
//                string vertexCountString = line.Split(':')[1].Trim();
//                int vertexCount = int.Parse(vertexCountString);
//                Vector3[] vertices = new Vector3[vertexCount];
//                for (int i = 0; i < vertexCount; i++)
//                {
//                    line = reader.ReadLine();
//                    string[] vertexCoordsString = line.Split(':');
//                    Vector3 vertex = new Vector3(float.Parse(vertexCoordsString[0].Trim()), float.Parse(vertexCoordsString[1].Trim()), float.Parse(vertexCoordsString[2].Trim()));
//                    vertices[i] = vertex;
//                }

//                line = reader.ReadLine();
//                string indexCountString = line.Split(':')[1].Trim();
//                int indexCount = int.Parse(indexCountString);
//                int[] indices = new int[indexCount];
//                for (int i = 0; i < indexCount; i++)
//                {
//                    line = reader.ReadLine();
//                    string[] triangleIndicesString = line.Split(':');
//                    int indexA = int.Parse(triangleIndicesString[0].Trim());
//                    int indexB = int.Parse(triangleIndicesString[1].Trim());
//                    int indexC = int.Parse(triangleIndicesString[2].Trim());
//                    indices[0] = indexA;
//                    indices[1] = indexB;
//                    indices[2] = indexC;
//                }
//                MapManager.Init(new Vector3(mapOriginX, mapOriginY, mapOriginZ), mapWidth, mapHeight);
//                NavMesh.Init(vertices, indices, velocity);
//                MapManager.AssignTrianglesToSectors(NavMesh.triangles);
//            }
//        }

//        public void StartGame()
//        {
//            gameStartTime = DateTime.Now;
//            lastWorldUpdateTime = DateTime.Now;
//            networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
//            stopwatch = new Stopwatch();

//            updateWorld = true;
//            worldUpdateThread = new Thread(HandleWorldUpdate);
//            worldUpdateThread.Start();


//        }
//        private void CompleteSyncedActions()
//        {
//            lock (syncedActions)
//            {
//                foreach (var action in syncedActions)
//                    action.Invoke();

//                syncedActions.Clear();
//                //syncedActions = new ConcurrentBag<Action>();
//            }
//        }
//        private void HandleWorldUpdate()
//        {
//            while (updateWorld)
//            {
//                //stopwatch.Restart();
//                CompleteSyncedActions();
//                pathfinding.FindAllPathsParallel();
//                unitManager.UpdateUnits(networkDeltaTime);

//                if (gameWorld.HasUpdate())
//                {
//                    Messages.Server.WorldUpdate worldUpdateMessage = gameWorld.GetUpdateMessage();
//                    worldUpdateMessage.timeSinceStartup = (float)DateTime.Now.Subtract(gameStartTime).TotalSeconds;
//                    //stopwatch.Stop();

//                    networkManager.SendMessageToAll(worldUpdateMessage, Messages.Server.WorldUpdate.Tag, DarkRift.SendMode.Unreliable);
//                    gameWorld.ClearChanges();
//                }
//                networkDeltaTime = (float)DateTime.Now.Subtract(lastWorldUpdateTime).TotalSeconds;
//                lastWorldUpdateTime = DateTime.Now;

//                Thread.Sleep(50);
//            }
//        }
//        public void OnPlayerSpawnUnit(NetworkIdentity callingPlayerID, Entities.BattleUnitModel.UnitType unitType)
//        {
//            lock (syncedActions)
//            {
//                syncedActions.Add(() =>
//            {
//                PlayerBase playerBase = gameWorld.GetPlayerBase(callingPlayerID.ID);
//                // check cost
//                BattleUnit unit = unitManager.CreateUnitWithPlayerAuthority(callingPlayerID, unitType);
//                unit.InitPathfinding(navGrid, pathfinding, navGrid.grid.GetNode(new GridPosition(5, 5)));
//                unit.InitWorldChanges(gameWorld);

//                SendPlayerSpawnUnit(unit);
//            });
//            }
//        }
//        public void SendPlayerSpawnUnit(BattleUnit unit)
//        {
//            Messages.Server.SpawnUnit spawnUnitMessage = new Messages.Server.SpawnUnit();
//            spawnUnitMessage.owningPlayerID = unit.owningPlayerID;
//            spawnUnitMessage.unitID = unit.networkID;
//            spawnUnitMessage.unitModel = unit.model;
//            networkManager.SendMessageToAll(spawnUnitMessage, Messages.Server.SpawnUnit.Tag, DarkRift.SendMode.Reliable);
//        }
//        public void OnPlayerMoveUnits(NetworkIdentity callingPlayerID, Messages.Client.MoveUnits message)
//        {
//            lock (syncedActions)
//            {
//                syncedActions.Add(() =>
//                {
//                    for (int i = 0; i < message.unitsCount; i++)
//                    {
//                        BattleUnit unit = unitManager.GetUnit(message.unitIDs[i].ID);
//                        if (unit.owningPlayerID.ID == callingPlayerID.ID)
//                        {
//                            unit.SetDestination(navGrid.grid.GetNode(new GridPosition(message.nodeXCoords[i], message.nodeYCoords[i])));
//                        }
//                        else
//                            Console.WriteLine("WRONG ID");
//                    }
//                });
//            }
//        }
//    }
//}
