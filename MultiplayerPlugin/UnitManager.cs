using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class UnitManager
    {
        private Dictionary<ushort, BattleUnit> units_Dictionary;
        private List<BattleUnit> units_List;
        public UnitManager()
        {
            units_Dictionary = new Dictionary<ushort, BattleUnit>();
            units_List = new List<BattleUnit>();
        }
        public void Update(float deltaTime)
        {
            foreach(var unit in units_List)
            {
                unit.Update(deltaTime);
            }
        }

        public BattleUnit CreateUnitWithPlayerAuthority(NetworkIdentity owningPlayerID, Entities.BattleUnitModel.UnitType unitType)
        {
            var unit = new BattleUnit(unitType);
            unit.owningPlayerID = owningPlayerID;
            return unit;
        }
    }
}
