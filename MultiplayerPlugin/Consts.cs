using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public abstract class GameData
    {
        protected Dictionary<string, Dictionary<string, object>> data;
        public const int Tag = 5;
        public GameData()
        {
            data = new Dictionary<string, Dictionary<string, object>>();            
        }
        public T Get<T>(string structure, string item)
        {
            var items = data[structure];
            return (T)items[item];            
        }
    }
    public class Default2v2 : GameData
    {
        public const string PLAYER_BASE = "PlayerBase";        
        public const string RESOURCE_SMALL = "ResourceSmall";
        public const string RESOURCE_BIG = "ResourceBig";
        public const string UNIT_KNIGHT = "Knight";

        public const string START_VALUE = "StartValue";        
        public const string CHANGE_PER_SEC = "ChangePerSec";

        public const string HEALTH = "Health";
        public const string GOLD = "Gold";
        public const string IRON = "Iron";
        public const string WOOD = "Wood";
        public const string CRYSTALS = "Crystals";

        //public struct Resource
        //{
        //    public Entities.ResourceModel.Size size;
        //    public Region region;
        //}
        //public Resource southWest_Big = new Resource() { size = Entities.ResourceModel.Size.Big, region = Region.SouthWest };
        //public Resource southEast_Big = new Resource() { size = Entities.ResourceModel.Size.Big, region = Region.SouthEast };
        //public Resource northEast_Big = new Resource() { size = Entities.ResourceModel.Size.Big, region = Region.NorthEast};
        //public Resource northWest_Big = new Resource() { size = Entities.ResourceModel.Size.Big, region = Region.NorthWest };
        

        protected Dictionary<string, object> playerBase;
        protected Dictionary<string, object> unit_knight;
        protected Dictionary<string, object> resource_small;
        protected Dictionary<string, object> resource_big;
        public Default2v2() : base()
        {
            playerBase = new Dictionary<string, object>();
            playerBase.Add(START_VALUE + HEALTH, 1000);
            playerBase.Add(START_VALUE + GOLD, 100);
            playerBase.Add(START_VALUE + IRON, 100);
            playerBase.Add(START_VALUE + WOOD, 100);
            playerBase.Add(START_VALUE + CRYSTALS, 100);

            unit_knight = new Dictionary<string, object>();
            unit_knight.Add(START_VALUE+HEALTH, 100);

            resource_small = new Dictionary<string, object>();            
            resource_small.Add(CHANGE_PER_SEC, 2);

            resource_big = new Dictionary<string, object>();            
            resource_big.Add(CHANGE_PER_SEC, 5);

            data.Add(PLAYER_BASE, playerBase);
            data.Add(UNIT_KNIGHT, unit_knight);
            data.Add(RESOURCE_SMALL, resource_small);
            data.Add(RESOURCE_BIG, resource_big);
        }
    }

}
