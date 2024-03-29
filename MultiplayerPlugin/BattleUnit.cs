﻿using System;
using System.Collections.Generic;
using System.Text;
using Roy_T.AStar.Graphs;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
namespace MultiplayerPlugin
{
    public abstract class BattleUnit
    {
        public NetworkIdentity networkID;
        public NetworkIdentity owningPlayerID;
        public Entities.BattleUnitModel model;

        public SphereCollider obstacleAvoidanceCollider;

        private GameWorld gameWorld;
        private NavMeshAgent agent;        

        public BattleUnit(Entities.BattleUnitModel.UnitType unitType)//, int currentHealth, int maxHealth, int healthPerLevel, int healthRegen, int healthRegenPerLevel, int attackDamage, int attackDamagePerLevel, float attackSpeed, float attackSpeedPerLevel, int abilityPower, int abilityPowerPerLevel, int armor, int armorPerLevel, int magicResist, int magicResistPerLevel, float movementSpeed, float critChance, int attackRange, float cooldownReduction, float lifesteal)
        {            
            networkID = new NetworkIdentity();
            networkID.GenerateID();

            model = new Entities.BattleUnitModel();// unitType, currentHealth, maxHealth, healthPerLevel, healthRegen, healthRegenPerLevel, attackDamage, attackDamagePerLevel, attackSpeed, attackSpeedPerLevel, abilityPower, abilityPowerPerLevel, armor, armorPerLevel, magicResist, magicResistPerLevel, movementSpeed, critChance, attackRange, cooldownReduction, lifesteal);            
            model.unitType = unitType;

            obstacleAvoidanceCollider = new SphereCollider(1f);
            Physics.SubscribeForCollisionEvents(obstacleAvoidanceCollider);
            obstacleAvoidanceCollider.onTriggerEnter += OnTriggerEnter;
            obstacleAvoidanceCollider.onTriggerExit += OnTriggerExit;
        }

        private void OnTriggerExit(Collider other)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("On TRIGGER EXIT " + other);
            Console.WriteLine();
            Console.WriteLine();
            agent.RemoveObstacle(other);
        }

        private void OnTriggerEnter(Collider other)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("On TRIGGER ENTER" + other);
            Console.WriteLine();
            Console.WriteLine();
            agent.AddObstacle(other);
        }

        private void OnPositionChanged(float xPos, float yPos, float zPos)
        {            
            gameWorld.AddPositionChange(networkID, xPos, yPos, zPos);
        }

        private void OnCurrentHealthChanged(int currentHealth)
        {
            gameWorld.AddHealthChange(networkID, currentHealth);
        }

        public void InitPathfinding(Vector3 startingPosition)
        {
            agent = new NavMeshAgent(startingPosition, model.movementSpeed.Value);
            model.position.Set(agent.currentPosition.x, agent.currentPosition.y, agent.currentPosition.z);
        }
        public void InitWorldChanges(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
            model.currentHealth.onChanged += OnCurrentHealthChanged;
            model.position.onChanged += OnPositionChanged;
        }
        public void Update(float deltaTime)
        {
            if (agent.UpdateAgent(deltaTime))
            {
                model.position.Set(agent.currentPosition.x, agent.currentPosition.y, agent.currentPosition.z);
            }

            obstacleAvoidanceCollider.Update(agent.currentPosition);
        }
        public void SetDestination(Vector3 worldPosition)
        {
            agent.SetDestination(worldPosition);
        }
    }
}
