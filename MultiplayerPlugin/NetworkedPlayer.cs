using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class NetworkedPlayer
    {
        public NetworkIdentity networkID;
        public Entities.NetworkedPlayerModel model;
        public NetworkedPlayer(ushort ID, string playerName)
        {
            networkID = new NetworkIdentity(ID);
            model = new Entities.NetworkedPlayerModel(playerName);
        }
    }
}
