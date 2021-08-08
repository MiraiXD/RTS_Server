using DarkRift;
using System;

public class Messages
{
    public class Player
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
    }
    public class Server
    {
        public class WorldUpdate : IDarkRiftSerializable
        {
            public const ushort Tag = 1105;
            public float x, z;            
            public void Deserialize(DeserializeEvent e)
            {
                x = e.Reader.ReadSingle();
                z = e.Reader.ReadSingle();
            }

            public void Serialize(SerializeEvent e)
            {
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
            public Entities.Player[] connectedPlayers;
            public void Deserialize(DeserializeEvent e)
            {
                maxPlayers = e.Reader.ReadInt32();
                connectedPlayers_Size = e.Reader.ReadInt32();
                connectedPlayers = new Entities.Player[connectedPlayers_Size];
                for (int i = 0; i < connectedPlayers_Size; i++)
                {
                    var player = e.Reader.ReadSerializable<Entities.Player>();
                    connectedPlayers[i] = player;
                }
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(maxPlayers);
                e.Writer.Write(connectedPlayers_Size);
                foreach (Entities.Player p in connectedPlayers)
                {
                    e.Writer.Write<Entities.Player>(p);
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
    }

}
public class Entities
{
    public class Player : IDarkRiftSerializable
    {
        public ushort ID;
        public string playerName;
        public bool isReady;

        public Player() { }
        public Player(ushort ID, string playerName)
        {
            this.ID = ID;
            this.playerName = playerName;
        }
        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
            playerName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(playerName);
        }
    }
}