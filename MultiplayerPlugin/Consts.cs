using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public abstract class GameData
    {
        public const string PLAYER_BASE = "PlayerBase";
        public const string RESOURCE_SMALL = "ResourceSmall";
        public const string RESOURCE_BIG = "ResourceBig";
        public const string UNIT_KNIGHT = "Knight";

        public const string START_VALUE = "StartValue";
        public const string CHANGE_PER_SEC = "ChangePerSec";

        public const string HEALTH = "Health";
        public const string HEALTH_PER_LEVEL = "HealthPerLevel";
        public const string HEALTH_REGEN = "HealthRegen";
        public const string HEALTH_REGEN_PER_LEVEL = "HealthRegenPerLevel";
        public const string ATTACK_DAMAGE = "AttackDamage";
        public const string ATTACK_DAMAGE_PER_LEVEL = "AttackDamagePerLevel";
        public const string ATTACK_SPEED = "AttackSpeed";
        public const string ATTACK_SPEED_PER_LEVEL = "AttackSpeedPerLevel";
        public const string ABILITY_POWER = "AbilityPower";
        public const string ABILITY_POWER_PER_LEVEL = "AbilityPowerPerLevel";
        public const string ARMOR = "Armor";
        public const string ARMOR_PER_LEVEL = "ArmorPerLevel";
        public const string MAGIC_RESIST = "MagicResist";
        public const string MAGIC_RESIST_PER_LEVEL = "MagicResistPerLevel";
        public const string MOVEMENT_SPEED = "MovementSpeed";
        public const string CRIT_CHANCE = "CritChance";
        public const string ATTACK_RANGE = "AttackRange";        
        public const string COOLDOWN_REDUCTION = "CooldownReduction";        
        public const string LIFE_STEAL = "LifeSteal";        
        
        public const string GOLD = "Gold";
        public const string IRON = "Iron";
        public const string WOOD = "Wood";
        public const string CRYSTALS = "Crystals";

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
            unit_knight.Add(START_VALUE + HEALTH, 100);
            unit_knight.Add(START_VALUE + HEALTH_PER_LEVEL, 50);
            unit_knight.Add(START_VALUE + HEALTH_REGEN, 2f);
            unit_knight.Add(START_VALUE + HEALTH_REGEN_PER_LEVEL, 0.5f);
            unit_knight.Add(START_VALUE + ATTACK_DAMAGE, 50f);
            unit_knight.Add(START_VALUE + ATTACK_DAMAGE_PER_LEVEL, 10f);
            unit_knight.Add(START_VALUE + ATTACK_SPEED, 0.75f);
            unit_knight.Add(START_VALUE + ATTACK_SPEED_PER_LEVEL, 0.1f);
            unit_knight.Add(START_VALUE + ABILITY_POWER, 30f);
            unit_knight.Add(START_VALUE + ABILITY_POWER_PER_LEVEL, 5f);
            unit_knight.Add(START_VALUE + ARMOR, 50f);
            unit_knight.Add(START_VALUE + ARMOR_PER_LEVEL, 10f);
            unit_knight.Add(START_VALUE + MAGIC_RESIST, 50f);
            unit_knight.Add(START_VALUE + MAGIC_RESIST_PER_LEVEL, 10f);
            unit_knight.Add(START_VALUE + MOVEMENT_SPEED,2f);
            unit_knight.Add(START_VALUE + CRIT_CHANCE, 0f);
            unit_knight.Add(START_VALUE + ATTACK_RANGE, 1);
            unit_knight.Add(START_VALUE + COOLDOWN_REDUCTION, 0f);
            unit_knight.Add(START_VALUE + LIFE_STEAL, 0f);

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
