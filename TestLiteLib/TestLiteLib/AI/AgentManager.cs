using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;
using UnityEngine;

namespace TestLiteLib
{

    struct AgentCastData
    {
        public ClientCastAbilityData CastData;
        public BaseAgent Caster;

        public AgentCastData(BaseAgent caster, ClientCastAbilityData castData)
        {
            this.CastData = castData;
            this.Caster = caster;
        }
    }
    class AgentManager
    {
        private List<BaseAgent> _agents;
        private List<AgentCastData> _newCastedAbilities;
        private List<BaseAgent> _newSpawnData;
        private bool _hasCastedAbility;
        private bool _hasSpawnData;

        public AgentManager()
        {
            this._newCastedAbilities = new List<AgentCastData>();
            this._agents = new List<BaseAgent>();
            this._newSpawnData = new List<BaseAgent>();


            AddSpawnToQueue(new GoblinMage());
        }
        public void Update(float dt, IEnumerable<LivingEntity> players)
        {
            foreach (var c in _agents)
            {
                if (c.HasTarget)
                    UpdateAgentWithTarget(c, dt);
                else
                    UpdateAgentWithoutTarget(c, players, dt);
            }
        }

        private void UpdateAgentWithoutTarget(BaseAgent agent, IEnumerable<LivingEntity> players, float dt)
        {
            foreach (var player in players)
            {
                if (Vector3.Distance(agent.Position, player.Position) < agent.AggroRange)
                {
                    agent.Target = player;
                }
            }
        }

        private void UpdateAgentWithTarget(BaseAgent agent, float dt)
        {
            if (agent.HasCastedAbility())
            {
                foreach (var aData in agent.GetNewlyCastedAbilities())
                {
                    AddAbilityToQueue(aData, agent);
                }
                agent.ClearNewlyCastedAbilities();
            }

            if (agent.AggroRange * BaseAgent.ExtraAggroRange < Vector3.Distance(agent.Position, agent.Target.Position))
            {
                agent.Target = null;
            }
        }

        public void AddAbilityToQueue(ClientCastAbilityData data, BaseAgent caster)
        {
            this._newCastedAbilities.Add(new AgentCastData(caster, data));
            this._hasCastedAbility = true;
        }

        public IEnumerable<AgentCastData> GetNewlyCastedAbilities()
        {
            return this._newCastedAbilities;
        }

        public void ClearNewlyCastedAbilities()
        {
            this._hasCastedAbility = false;
            this._newCastedAbilities.Clear();
        }

        public bool HasCastedAbility()
        {
            return _hasCastedAbility;
        }

        public void AddSpawnToQueue(BaseAgent agent)
        {
            this._newSpawnData.Add(agent);
            this._agents.Add(agent);
            this._hasSpawnData = true;
        }

        public IEnumerable<BaseAgent> GetNewlySpawnedAgents()
        {
            return this._newSpawnData;
        }

        public void ClearNewlySpawnedAgents()
        {
            this._hasSpawnData = false;
            this._newSpawnData.Clear();
        }

        public bool HasSpawnAgent()
        {
            return _hasSpawnData;
        }
    }
}
