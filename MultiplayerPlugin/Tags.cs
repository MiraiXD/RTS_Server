using DarkRift;
using System;

public class Messages
{
    public class Client
    {
        public class Hello : IDarkRiftSerializable
        {
            public const ushort Tag = 1002;
            public ushort id { get; set; }
            public string playerName { get; set; }
            public void Deserialize(DeserializeEvent e)
            {
                playerName = e.Reader.ReadString();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(playerName);
            }
        }
        public class ReadyToStartGame : IDarkRiftSerializable
        {
            public const ushort Tag = 1003;
            public void Deserialize(DeserializeEvent e) { }
            public void Serialize(SerializeEvent e) { }
        }
        public class SpawnUnit : IDarkRiftSerializable
        {
            public const ushort Tag = 1004;
            public Entities.BattleUnitModel.UnitType unitType;
            public void Deserialize(DeserializeEvent e)
            {
                unitType = (Entities.BattleUnitModel.UnitType) e.Reader.ReadUInt16();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write((ushort)unitType);
            }
        }

    }
    public class Server
    {
        public class WorldUpdate : IDarkRiftSerializable
        {
            public const ushort Tag = 1105;
            public double timeSinceStartup;            
            public float x, z;            
            public void Deserialize(DeserializeEvent e)
            {
                timeSinceStartup = e.Reader.ReadDouble();                
                x = e.Reader.ReadSingle();
                z = e.Reader.ReadSingle();                
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(timeSinceStartup);                
                e.Writer.Write(x);
                e.Writer.Write(z);                
            }
        }
        public class StartGame : IDarkRiftSerializable
        {
            public const ushort Tag = 1104;
            public void Deserialize(DeserializeEvent e) { }
            public void Serialize(SerializeEvent e) { }
        }
        public class LoadMap : IDarkRiftSerializable
        {
            public const ushort Tag = 1103;
            public string mapName;
            public void Deserialize(DeserializeEvent e)
            {
                mapName = e.Reader.ReadString();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(mapName);
            }
        }
        public class ConnectedPlayers : IDarkRiftSerializable
        {
            public const ushort Tag = 1100;
            public int maxPlayers;
            public int connectedPlayers_Size;
            public Entities.PlayerNetworkModel[] connectedPlayers;
            public void Deserialize(DeserializeEvent e)
            {
                maxPlayers = e.Reader.ReadInt32();
                connectedPlayers_Size = e.Reader.ReadInt32();
                connectedPlayers = new Entities.PlayerNetworkModel[connectedPlayers_Size];
                for (int i = 0; i < connectedPlayers_Size; i++)
                {
                    var player = e.Reader.ReadSerializable<Entities.PlayerNetworkModel>();
                    connectedPlayers[i] = player;
                }
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(maxPlayers);
                e.Writer.Write(connectedPlayers_Size);
                foreach (Entities.PlayerNetworkModel p in connectedPlayers)
                {
                    e.Writer.Write<Entities.PlayerNetworkModel>(p);
                }
            }
        }
        public class PlayerDisconnected : IDarkRiftSerializable
        {
            public const ushort Tag = 1101;
            public ushort ID;
            public void Deserialize(DeserializeEvent e)
            {
                e.Reader.ReadUInt16();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(ID);
            }
        }
        public class SpawnUnit : IDarkRiftSerializable
        {
            public const ushort Tag = 1106;
            public NetworkIdentity owningPlayerID;
            public Entities.BattleUnitModel unitModel;
            public NetworkIdentity unitID;
            public void Deserialize(DeserializeEvent e)
            {
                owningPlayerID = new NetworkIdentity();
                owningPlayerID.Deserialize(e);
                unitModel = new Entities.BattleUnitModel();
                unitModel.Deserialize(e);
                unitID = new NetworkIdentity();
                unitID.Deserialize(e);
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(owningPlayerID);
                e.Writer.Write(unitModel);
                e.Writer.Write(unitID);
            }
        }
    }

}
public class Entities
{
    public class PlayerNetworkModel : IDarkRiftSerializable
    {
        //public ushort ID;
        public NetworkIdentity networkID;
        public string playerName;
        public bool isReady;

        public PlayerNetworkModel() { }
        public PlayerNetworkModel(ushort ID, string playerName)
        {
            //this.ID = ID;
            networkID = new NetworkIdentity(ID);
            this.playerName = playerName;
        }
        public void Deserialize(DeserializeEvent e)
        {
            //ID = e.Reader.ReadUInt16();
            networkID.Deserialize(e);
            playerName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            networkID.Serialize(e);
            e.Writer.Write(playerName);
        }
    }
    public class PlayerBaseModel : IDarkRiftSerializable
    {
        public int currentGold;
        public void Deserialize(DeserializeEvent e)
        {
            currentGold = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(currentGold);
        }
    }
    public class BattleUnitModel : IDarkRiftSerializable
    {
        public enum UnitType { Infantry,Knight,WaterMage}
        public UnitType unitType;
        public void Deserialize(DeserializeEvent e)
        {
            unitType = (UnitType) e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write((ushort)unitType);
        }
    }
}
public class NetworkIdentity : IDarkRiftSerializable, IComparable<NetworkIdentity>
{
    public ushort ID { get; private set; }
    private static ushort counter;
    static NetworkIdentity()
    {
        counter = 100; // start IDs from 100 
    }
    public void GenerateID()
    {
        ID = counter++;
    }
    public NetworkIdentity() { }        
    public NetworkIdentity(ushort ID)
    {
        this.ID = ID;
    }

    public void Deserialize(DeserializeEvent e)
    {
        ID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ID);
    }

    public int CompareTo(NetworkIdentity other)
    {
        if (other.ID == this.ID) return 0;
        else return -1;
    }
}