using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class GameWorld
    {
        private UnitManager unitManager;
        private Messages.Server.WorldUpdate worldUpdateMessage;
        public GameWorld(UnitManager unitManager)
        {
            this.unitManager = unitManager;
            worldUpdateMessage = new Messages.Server.WorldUpdate();
        }
        public Messages.Server.WorldUpdate Update(float deltaTime)
        {
            unitManager.Update(deltaTime);

            return worldUpdateMessage;
        }
    }
}
