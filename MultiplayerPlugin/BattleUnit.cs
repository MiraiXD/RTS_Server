using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class BattleUnit
    {
        public NetworkIdentity ID;
        public NetworkIdentity owningPlayerID;
        public Entities.BattleUnitModel model;        
        public BattleUnit(Entities.BattleUnitModel.UnitType unitType)
        {
            ID = new NetworkIdentity();
            ID.GenerateID();

            model = new Entities.BattleUnitModel();
            model.unitType = unitType;
        }
        public void Update(float deltaTime)
        {

        }
    }
}
