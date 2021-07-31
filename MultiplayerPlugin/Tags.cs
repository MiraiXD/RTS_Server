using DarkRift;
using System.Collections.Generic;
//public class Tags
//{
//    public const ushort PlayerConnectTag = 1000;
//    public const ushort PlayerDisconnectTag = 1001;
//    public class Player
//    {
//        public const ushort Hello = 1002;
//    }
//    public class Server
//    {
//        public const ushort ConnectedPlayers = 1100;
//        public const ushort LoadMap = 1101;
//    }

//}
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
        public class Update : IDarkRiftSerializable
        {
            public const ushort Tag = 1102;
            public string m1;
            public string m2;
            public string m3;
            public void Deserialize(DeserializeEvent e)
            {
                m1 = e.Reader.ReadString();
                m2 = e.Reader.ReadString();
                m3 = e.Reader.ReadString();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(m1);
                e.Writer.Write(m2);
                e.Writer.Write(m3);
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
        //public bool isReady { get; set; }
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