using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class PlayerBase
    {
        public Entities.PlayerBaseModel model;

        public PlayerBase(string playerName, Region region, int maxHealth, int startingGold, int startingIron, int startingWood, int startingCrystals)
        {
            model = new Entities.PlayerBaseModel(playerName, region, maxHealth, startingGold, startingIron, startingWood, startingCrystals);
        }
    }
}
