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
                unitType = (Entities.BattleUnitModel.UnitType)e.Reader.ReadUInt16();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write((ushort)unitType);
            }
        }
        public class MoveUnit : IDarkRiftSerializable
        {
            public const ushort Tag = 1005;
            public NetworkIdentity unitID;
            public int nodeXCoord;
            public int nodeYCoord;
            public void Deserialize(DeserializeEvent e)
            {
                unitID = e.Reader.ReadSerializable<NetworkIdentity>();
                nodeXCoord = e.Reader.ReadInt32();
                nodeYCoord = e.Reader.ReadInt32();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(unitID);
                e.Writer.Write(nodeXCoord);
                e.Writer.Write(nodeYCoord);
            }
        }


    }
    public class Server
    {
        public class WorldUpdate : IDarkRiftSerializable
        {
            public const ushort Tag = 1105;
            public float timeSinceStartup;

            public int changeCount;
            public ushort[] IDs;
            public SingleWorldChange[] changes;

            public void Deserialize(DeserializeEvent e)
            {
                timeSinceStartup = e.Reader.ReadSingle();
                changeCount = e.Reader.ReadInt32();
                IDs = new ushort[changeCount];
                changes = new SingleWorldChange[changeCount];
                for (int i = 0; i < changeCount; i++)
                {
                    IDs[i] = e.Reader.ReadUInt16();
                    changes[i] = e.Reader.ReadSerializable<SingleWorldChange>();
                }
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(timeSinceStartup);
                e.Writer.Write(changeCount);
                for (int i=0; i<changeCount;i++)
                {
                    e.Writer.Write(IDs[i]);
                    e.Writer.Write(changes[i]);
                }
                
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
            public NetworkIdentity[] IDs;
            public Entities.NetworkedPlayerModel[] connectedPlayers;
            public void Deserialize(DeserializeEvent e)
            {
                maxPlayers = e.Reader.ReadInt32();
                connectedPlayers_Size = e.Reader.ReadInt32();
                IDs = new NetworkIdentity[connectedPlayers_Size];
                connectedPlayers = new Entities.NetworkedPlayerModel[connectedPlayers_Size];
                for (int i = 0; i < connectedPlayers_Size; i++)
                {
                    IDs[i] = e.Reader.ReadSerializable<NetworkIdentity>();
                    connectedPlayers[i] = e.Reader.ReadSerializable<Entities.NetworkedPlayerModel>();
                }
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(maxPlayers);
                e.Writer.Write(connectedPlayers_Size);
                for (int i = 0; i < connectedPlayers_Size; i++)
                {
                    e.Writer.Write(IDs[i]);
                    e.Writer.Write(connectedPlayers[i]);
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
                owningPlayerID = e.Reader.ReadSerializable<NetworkIdentity>();
                unitModel = e.Reader.ReadSerializable<Entities.BattleUnitModel>();
                unitID = e.Reader.ReadSerializable<NetworkIdentity>();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(owningPlayerID);
                e.Writer.Write(unitModel);
                e.Writer.Write(unitID);
            }
        }
        public class WorldInfo : IDarkRiftSerializable
        {
            public const ushort Tag = 1107;
            public int models_Count;
            public NetworkIdentity[] playerIDs;
            public Entities.NetworkedPlayerModel[] playerModels;
            public NetworkIdentity[] baseIDs;
            public Entities.PlayerBaseModel[] baseModels;

            public void Deserialize(DeserializeEvent e)
            {
                models_Count = e.Reader.ReadInt32();
                //playerModels = new NetworkedObject<Entities.NetworkedPlayerModel>[models_Count];
                //baseModels = new NetworkedObject<Entities.PlayerBaseModel>[models_Count];
                playerIDs = new NetworkIdentity[models_Count];
                playerModels = new Entities.NetworkedPlayerModel[models_Count];
                baseIDs = new NetworkIdentity[models_Count];
                baseModels = new Entities.PlayerBaseModel[models_Count];
                for (int i = 0; i < models_Count; i++)
                {
                    playerIDs[i] = e.Reader.ReadSerializable<NetworkIdentity>();
                    playerModels[i] = e.Reader.ReadSerializable<Entities.NetworkedPlayerModel>();
                    baseIDs[i] = e.Reader.ReadSerializable<NetworkIdentity>();
                    baseModels[i] = e.Reader.ReadSerializable<Entities.PlayerBaseModel>();
                }
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(models_Count);
                for (int i = 0; i < models_Count; i++)
                {
                    e.Writer.Write(playerIDs[i]);
                    e.Writer.Write(playerModels[i]);
                    e.Writer.Write(baseIDs[i]);
                    e.Writer.Write(baseModels[i]);
                }
            }
        }
    }

}
public class Entities
{
    public class NetworkedPlayerModel : IDarkRiftSerializable
    {
        // public NetworkIdentity networkID;
        public string playerName;
        public bool isReady;

        public NetworkedPlayerModel() { }
        public NetworkedPlayerModel(string playerName)
        {
            //networkID = new NetworkIdentity(ID);
            this.playerName = playerName;
        }
        public void Deserialize(DeserializeEvent e)
        {
            //networkID = new NetworkIdentity();
            //networkID.Deserialize(e);
            playerName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            //networkID.Serialize(e);
            e.Writer.Write(playerName);
        }
    }
    public class PlayerBaseModel : IDarkRiftSerializable
    {
        //public NetworkIdentity networkID;
        public string playerName;
        public Region region;

        public NumericComponent<int> maxHealth;
        public NumericComponent<int> currentHealth;
        public NumericComponent<int> currentGold;
        public NumericComponent<int> currentIron;
        public NumericComponent<int> currentWood;
        public NumericComponent<int> currentCrystals;
        public PlayerBaseModel() { }
        public PlayerBaseModel(string playerName, Region region, int maxHealth, int startingGold, int startingIron, int startingWood, int startingCrystals)
        {
            //networkID = new NetworkIdentity();
            //networkID.GenerateID();

            this.playerName = playerName;
            this.region = region;
            this.maxHealth = new NumericComponent<int>(maxHealth);
            this.currentHealth = new NumericComponent<int>(maxHealth);
            this.currentGold = new NumericComponent<int>(startingGold);
            this.currentIron = new NumericComponent<int>(startingIron);
            this.currentWood = new NumericComponent<int>(startingWood);
            this.currentCrystals = new NumericComponent<int>(startingCrystals);
        }
        public void Deserialize(DeserializeEvent e)
        {
            //networkID = new NetworkIdentity();
            //networkID.Deserialize(e);
            playerName = e.Reader.ReadString();
            region = (Region)e.Reader.ReadUInt16();

            maxHealth = new NumericComponent<int>(e.Reader.ReadInt32());
            currentHealth = new NumericComponent<int>(e.Reader.ReadInt32());
            currentGold = new NumericComponent<int>(e.Reader.ReadInt32());
            currentIron = new NumericComponent<int>(e.Reader.ReadInt32());
            currentWood = new NumericComponent<int>(e.Reader.ReadInt32());
            currentCrystals = new NumericComponent<int>(e.Reader.ReadInt32());
        }

        public void Serialize(SerializeEvent e)
        {
            //networkID.Serialize(e);
            e.Writer.Write(playerName);
            e.Writer.Write((ushort)region);

            e.Writer.Write(maxHealth.Value);
            e.Writer.Write(currentHealth.Value);
            e.Writer.Write(currentGold.Value);
            e.Writer.Write(currentIron.Value);
            e.Writer.Write(currentWood.Value);
            e.Writer.Write(currentCrystals.Value);
        }
    }
    public class BattleUnitModel : IDarkRiftSerializable
    {
        public enum UnitType { Infantry, Knight, WaterMage }

        public UnitType unitType;
        public NumericComponent3<float> position;
        public NumericComponent<int> currentHealth;
        public NumericComponent<int> maxHealth;
        public NumericComponent<int> healthPerLevel;
        public NumericComponent<float> healthRegen;
        public NumericComponent<float> healthRegenPerLevel;
        public NumericComponent<float> attackDamage;
        public NumericComponent<float> attackDamagePerLevel;
        public NumericComponent<float> attackSpeed;
        public NumericComponent<float> attackSpeedPerLevel;
        public NumericComponent<float> abilityPower;
        public NumericComponent<float> abilityPowerPerLevel;
        public NumericComponent<float> armor;
        public NumericComponent<float> armorPerLevel;
        public NumericComponent<float> magicResist;
        public NumericComponent<float> magicResistPerLevel;
        public NumericComponent<float> movementSpeed;
        public NumericComponent<float> critChance;
        public NumericComponent<int> attackRange;
        public NumericComponent<float> cooldownReduction;
        public NumericComponent<float> lifesteal;
        public BattleUnitModel() { }
        //public BattleUnitModel(UnitType unitType, int currentHealth, int maxHealth, int healthPerLevel, int healthRegen, int healthRegenPerLevel, int attackDamage, int attackDamagePerLevel, float attackSpeed, float attackSpeedPerLevel, int abilityPower, int abilityPowerPerLevel, int armor, int armorPerLevel, int magicResist, int magicResistPerLevel, float movementSpeed, float critChance, int attackRange, float cooldownReduction, float lifesteal)
        //{
        //    this.unitType = unitType;
        //    this.currentHealth = new NumericComponent<int>(currentHealth);
        //    this.maxHealth = new NumericComponent<int>(maxHealth);
        //    this.healthPerLevel = new NumericComponent<int>(healthPerLevel);
        //    this.healthRegen = new NumericComponent<int>(healthRegen);
        //    this.healthRegenPerLevel = new NumericComponent<int>(healthRegenPerLevel);
        //    this.attackDamage = new NumericComponent<int>(attackDamage);
        //    this.attackDamagePerLevel = new NumericComponent<int>(attackDamagePerLevel);
        //    this.attackSpeed = new NumericComponent<float>(attackSpeed);
        //    this.attackSpeedPerLevel = new NumericComponent<float>(attackSpeedPerLevel);
        //    this.abilityPower = new NumericComponent<int>(abilityPower);
        //    this.abilityPowerPerLevel = new NumericComponent<int>(abilityPowerPerLevel);
        //    this.armor = new NumericComponent<int>(armor);
        //    this.armorPerLevel = new NumericComponent<int>(armorPerLevel);
        //    this.magicResist = new NumericComponent<int>(magicResist);
        //    this.magicResistPerLevel = new NumericComponent<int>(magicResistPerLevel);
        //    this.movementSpeed = new NumericComponent<float>(movementSpeed);
        //    this.critChance = new NumericComponent<float>(critChance);
        //    this.attackRange = new NumericComponent<int>(attackRange);
        //    this.cooldownReduction = new NumericComponent<float>(cooldownReduction);
        //    this.lifesteal = new NumericComponent<float>(lifesteal);
        //}

        public void Deserialize(DeserializeEvent e)
        {
            unitType = (UnitType)e.Reader.ReadUInt16();

            position = new NumericComponent3<float>(e.Reader.ReadSingle(), e.Reader.ReadSingle(), e.Reader.ReadSingle());

            currentHealth = new NumericComponent<int>(e.Reader.ReadInt32());
            maxHealth = new NumericComponent<int>(e.Reader.ReadInt32());
            healthPerLevel = new NumericComponent<int>(e.Reader.ReadInt32());
            healthRegen = new NumericComponent<float>(e.Reader.ReadSingle());
            healthRegenPerLevel = new NumericComponent<float>(e.Reader.ReadSingle());
            attackDamage = new NumericComponent<float>(e.Reader.ReadSingle());
            attackDamagePerLevel = new NumericComponent<float>(e.Reader.ReadSingle());
            attackSpeed = new NumericComponent<float>(e.Reader.ReadSingle());
            attackSpeedPerLevel = new NumericComponent<float>(e.Reader.ReadSingle());
            abilityPower = new NumericComponent<float>(e.Reader.ReadSingle());
            abilityPowerPerLevel = new NumericComponent<float>(e.Reader.ReadSingle());
            armor = new NumericComponent<float>(e.Reader.ReadSingle());
            armorPerLevel = new NumericComponent<float>(e.Reader.ReadSingle());
            magicResist = new NumericComponent<float>(e.Reader.ReadSingle());
            magicResistPerLevel = new NumericComponent<float>(e.Reader.ReadSingle());
            movementSpeed = new NumericComponent<float>(e.Reader.ReadSingle());
            critChance = new NumericComponent<float>(e.Reader.ReadSingle());
            attackRange = new NumericComponent<int>(e.Reader.ReadInt32());
            cooldownReduction = new NumericComponent<float>(e.Reader.ReadSingle());
            lifesteal = new NumericComponent<float>(e.Reader.ReadSingle());
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write((ushort)unitType);

            e.Writer.Write(position.FirstValue);
            e.Writer.Write(position.SecondValue);
            e.Writer.Write(position.ThirdValue);

            e.Writer.Write(currentHealth.Value);
            e.Writer.Write(maxHealth.Value);
            e.Writer.Write(healthPerLevel.Value);
            e.Writer.Write(healthRegen.Value);
            e.Writer.Write(healthRegenPerLevel.Value);
            e.Writer.Write(attackDamage.Value);
            e.Writer.Write(attackDamagePerLevel.Value);
            e.Writer.Write(attackSpeed.Value);
            e.Writer.Write(attackSpeedPerLevel.Value);
            e.Writer.Write(abilityPower.Value);
            e.Writer.Write(abilityPowerPerLevel.Value);
            e.Writer.Write(armor.Value);
            e.Writer.Write(armorPerLevel.Value);
            e.Writer.Write(magicResist.Value);
            e.Writer.Write(magicResistPerLevel.Value);
            e.Writer.Write(movementSpeed.Value);
            e.Writer.Write(critChance.Value);
            e.Writer.Write(attackRange.Value);
            e.Writer.Write(cooldownReduction.Value);
            e.Writer.Write(lifesteal.Value);
        }
    }
    public class ResourceModel : IDarkRiftSerializable
    {
        public enum Type { Gold, Iron, Wood, MagicCrystals }
        public enum Size { Small, Big }

        //public NetworkIdentity networkID;
        public Type type;
        public Size size;
        public int incomePerSecond;
        public void Deserialize(DeserializeEvent e)
        {
            incomePerSecond = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(incomePerSecond);
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
public class NumericComponent<T> where T : IComparable<T>
{
    private T _value;
    public T Value { get { return _value; } set { if (_value.CompareTo(value) != 0) { _value = value; onChanged?.Invoke(_value); } } }
    public Action<T> onChanged;
    public NumericComponent(T startValue)
    {
        _value = startValue;
    }
}
public class NumericComponent3<T> where T : IComparable<T>
{
    private T _firstValue;
    public T FirstValue { get { return _firstValue; } }//set { if (_firstValue.CompareTo(value) != 0) { _firstValue = value; onChanged?.Invoke(_firstValue, _secondValue,_thirdValue); } } }
    private T _secondValue;
    public T SecondValue { get { return _secondValue; } }// set { if (_secondValue.CompareTo(value) != 0) { _secondValue= value; onChanged?.Invoke(_firstValue, _secondValue,_thirdValue); } } }
    private T _thirdValue;
    public T ThirdValue { get { return _thirdValue; } } // set { if (_thirdValue.CompareTo(value) != 0) { _thirdValue= value; onChanged?.Invoke(_firstValue,_secondValue,_thirdValue); } } }
    public void Set(T first, T second, T third)
    {
        if (first.CompareTo(_firstValue) != 0 || second.CompareTo(_secondValue) != 0 || third.CompareTo(_thirdValue) != 0) onChanged?.Invoke(first, second, third);
        _firstValue = first;
        _secondValue = second;
        _thirdValue = third;
    }
    public Action<T,T,T> onChanged;
    public NumericComponent3(T startFirstValue, T startSecondValue, T startThirdValue)
    {
        _firstValue = startFirstValue;
        _secondValue = startSecondValue;
        _thirdValue = startThirdValue;
    }
}
public enum Region { South, West, North, East, SouthWest, SouthEast, NorthEast, NorthWest }
public class SingleWorldChange : IDarkRiftSerializable
{    
    public float xPos, yPos, zPos;
    public int currentHealth;

    public void Deserialize(DeserializeEvent e)
    {
        xPos = e.Reader.ReadSingle();
        yPos = e.Reader.ReadSingle();
        zPos = e.Reader.ReadSingle();
        currentHealth = e.Reader.ReadInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(xPos);
        e.Writer.Write(yPos);
        e.Writer.Write(zPos);
        e.Writer.Write(currentHealth);
    }
}

