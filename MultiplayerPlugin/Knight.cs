using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class Knight : BattleUnit
    {
        public Knight() : base(Entities.BattleUnitModel.UnitType.Knight)
        {
        }
    }
}
