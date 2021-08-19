using System;
using System.Collections.Generic;

namespace MultiplayerPlugin
{
    public class GameWorld
    {                
        private Messages.Server.WorldUpdate worldUpdateMessage;
        private Dictionary<ushort, PlayerBase> playerBases;
        private Dictionary<ushort,SingleWorldChange> worldChanges;
        
        public GameWorld()
        {                        
            worldChanges = new Dictionary<ushort, SingleWorldChange>();
            worldUpdateMessage = new Messages.Server.WorldUpdate();            
        }
        public void AddHealthChange(NetworkIdentity networkID, int currentHealth)
        {
            if (worldChanges.TryGetValue(networkID.ID, out var change))
            {
                change.currentHealth = currentHealth;
            }
            else
            {
                SingleWorldChange newChange = new SingleWorldChange() { currentHealth=currentHealth };
                worldChanges.Add(networkID.ID, newChange);
            }
        }
        public void AddPositionChange(NetworkIdentity networkID, float xPos, float yPos, float zPos)
        {
            if(worldChanges.TryGetValue(networkID.ID, out var change))
            {
                change.xPos = xPos;
                change.yPos = yPos;
                change.zPos = zPos;
            }
            else
            {
                SingleWorldChange newChange = new SingleWorldChange() { xPos = xPos, yPos = yPos, zPos = zPos };
                worldChanges.Add(networkID.ID, newChange);
            }
        }
        public bool HasUpdate()
        {
            return worldChanges.Count > 0;
        }
        public Messages.Server.WorldUpdate GetUpdateMessage()
        {
            int changesCount = worldChanges.Count;
            worldUpdateMessage.changeCount = changesCount;
            worldUpdateMessage.IDs = new ushort[changesCount];
            worldUpdateMessage.changes = new SingleWorldChange[changesCount];
            int i = 0;
            foreach(var kvp in worldChanges)
            {
                worldUpdateMessage.IDs[i] = kvp.Key;
                worldUpdateMessage.changes[i] = kvp.Value;
                i++;
            }
            return worldUpdateMessage;
        }
        public void ClearChanges() => worldChanges.Clear();

        internal void CreatePlayerBases(NetworkedPlayer[] networkedPlayers)
        {
            playerBases = new Dictionary<ushort, PlayerBase>();
            List<Region> regions = new List<Region>() { Region.NorthEast, Region.NorthWest, Region.SouthEast, Region.SouthWest };

            int startingHP = GameManager.gameData.Get<int>(GameData.PLAYER_BASE, GameData.START_VALUE + GameData.HEALTH);
            int startingGold = GameManager.gameData.Get<int>(GameData.PLAYER_BASE, GameData.START_VALUE + GameData.GOLD);
            int startingIron = GameManager.gameData.Get<int>(GameData.PLAYER_BASE, GameData.START_VALUE + GameData.IRON);
            int startingWood = GameManager.gameData.Get<int>(GameData.PLAYER_BASE, GameData.START_VALUE + GameData.WOOD);
            int startingCrystals = GameManager.gameData.Get<int>(GameData.PLAYER_BASE, GameData.START_VALUE + GameData.CRYSTALS);

            foreach (var player in networkedPlayers)
            {                
                int randomIndex = new Random().Next(0, regions.Count);
                Region region = regions[randomIndex];
                regions.RemoveAt(randomIndex);

                string playerName = player.model.playerName;

                PlayerBase playerBase = new PlayerBase(playerName,region,startingHP, startingGold,startingIron,startingWood,startingCrystals); ;
                playerBases.Add(player.networkID.ID, playerBase);
            }

        }
        internal PlayerBase GetPlayerBase(ushort ID) => playerBases[ID];
        
    }
}
