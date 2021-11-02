using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public static class UnitManager
    {
        private static Dictionary<ushort, BattleUnit> unitsByID;
        private static List<BattleUnit> unitsList;
        //private ConcurrentBag<BattleUnit> unitsList;
        public static BattleUnit GetUnit(ushort ID) => unitsByID[ID];
        static UnitManager()
        {
            unitsByID = new Dictionary<ushort, BattleUnit>();
            unitsList = new List<BattleUnit>();
        }
        public static void UpdateUnits(float deltaTime)
        {
            foreach (var unit in unitsList)
            {
                unit.Update(deltaTime);
            }
        }

        public static BattleUnit CreateUnitWithPlayerAuthority(NetworkIdentity owningPlayerID, Entities.BattleUnitModel.UnitType unitType)
        {
            BattleUnit unit;
            string keyword;
            switch (unitType)
            {
                case Entities.BattleUnitModel.UnitType.Knight:
                    unit = new Knight();
                    keyword = GameData.UNIT_KNIGHT;
                    break;
                default:
                    return null;
            }
            unit.owningPlayerID = owningPlayerID;
            unit.model.position = new NumericComponent3<float>(0f, 0f, 0f);
            unit.model.maxHealth = new NumericComponent<int>(GameManager.gameData.Get<int>(keyword, GameData.START_VALUE + GameData.HEALTH));
            unit.model.currentHealth = new NumericComponent<int>(unit.model.maxHealth.Value);
            unit.model.healthPerLevel = new NumericComponent<int>(GameManager.gameData.Get<int>(keyword, GameData.START_VALUE + GameData.HEALTH_PER_LEVEL));
            unit.model.healthRegen = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.HEALTH_REGEN));
            unit.model.healthRegenPerLevel = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.HEALTH_REGEN_PER_LEVEL));
            unit.model.attackDamage = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ATTACK_DAMAGE));
            unit.model.attackDamagePerLevel = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ATTACK_DAMAGE_PER_LEVEL));
            unit.model.attackSpeed = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ATTACK_SPEED));
            unit.model.attackSpeedPerLevel = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ATTACK_SPEED_PER_LEVEL));
            unit.model.abilityPower = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ABILITY_POWER));
            unit.model.abilityPowerPerLevel = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ABILITY_POWER_PER_LEVEL));
            unit.model.armor = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ARMOR));
            unit.model.armorPerLevel = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.ARMOR_PER_LEVEL));
            unit.model.magicResist = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.MAGIC_RESIST));
            unit.model.magicResistPerLevel = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.MAGIC_RESIST_PER_LEVEL));
            unit.model.movementSpeed = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.MOVEMENT_SPEED));
            unit.model.critChance = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.CRIT_CHANCE));
            unit.model.attackRange = new NumericComponent<int>(GameManager.gameData.Get<int>(keyword, GameData.START_VALUE + GameData.ATTACK_RANGE));
            unit.model.cooldownReduction = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.COOLDOWN_REDUCTION));
            unit.model.lifesteal = new NumericComponent<float>(GameManager.gameData.Get<float>(keyword, GameData.START_VALUE + GameData.LIFE_STEAL));

            unitsList.Add(unit);
            unitsByID.Add(unit.networkID.ID, unit);

            return unit;
        }
    }
}
