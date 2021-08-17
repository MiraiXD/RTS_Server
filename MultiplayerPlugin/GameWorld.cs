using System;
using System.Collections.Generic;

namespace MultiplayerPlugin
{
    public class GameWorld
    {
        private GameData gameData;
        private UnitManager unitManager;
        private Messages.Server.WorldUpdate worldUpdateMessage;
        private Dictionary<ushort, PlayerBase> playerBases;
        
        
        public GameWorld(UnitManager unitManager)
        {            
            this.unitManager = unitManager;            
            worldUpdateMessage = new Messages.Server.WorldUpdate();

            gameData = new Default2v2();
        }
        public Messages.Server.WorldUpdate Update(float deltaTime)
        {
            unitManager.Update(deltaTime);

            return worldUpdateMessage;
        }

        internal void CreatePlayerBases(Entities.PlayerNetworkModel[] networkedPlayers)
        {
            playerBases = new Dictionary<ushort, PlayerBase>();
            List<Region> regions = new List<Region>() { Region.NorthEast, Region.NorthWest, Region.SouthEast, Region.SouthWest };

            int startingHP = gameData.Get<int>(Default2v2.PLAYER_BASE, Default2v2.START_VALUE + Default2v2.HEALTH);
            int startingGold = gameData.Get<int>(Default2v2.PLAYER_BASE, Default2v2.START_VALUE + Default2v2.GOLD);
            int startingIron = gameData.Get<int>(Default2v2.PLAYER_BASE, Default2v2.START_VALUE + Default2v2.IRON);
            int startingWood = gameData.Get<int>(Default2v2.PLAYER_BASE, Default2v2.START_VALUE + Default2v2.WOOD);
            int startingCrystals = gameData.Get<int>(Default2v2.PLAYER_BASE, Default2v2.START_VALUE + Default2v2.CRYSTALS);

            foreach (var player in networkedPlayers)
            {                
                int randomIndex = new Random().Next(0, regions.Count);
                Region region = regions[randomIndex];
                regions.RemoveAt(randomIndex);

                string playerName = player.playerName;

                PlayerBase playerBase = new PlayerBase(playerName,region,startingHP, startingGold,startingIron,startingWood,startingCrystals); ;
                playerBases.Add(player.networkID.ID, playerBase);
            }

        }
        internal PlayerBase GetPlayerBase(ushort ID) => playerBases[ID];
        
    }
}
