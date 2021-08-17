using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
namespace MultiplayerPlugin
{
    public class BattleUnit
    {
        public NetworkIdentity ID;
        public NetworkIdentity owningPlayerID;
        public Entities.BattleUnitModel model;
        public PathFinder pathFinder;
        public BattleUnit(Entities.BattleUnitModel.UnitType unitType)
        {
            ID = new NetworkIdentity();
            ID.GenerateID();

            model = new Entities.BattleUnitModel();
            model.unitType = unitType;

            pathFinder = new PathFinder();            
        }
        public void Update(float deltaTime)
        {

        }
    }
}
