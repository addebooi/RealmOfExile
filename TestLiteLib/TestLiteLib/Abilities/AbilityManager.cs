using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;

namespace TestLiteLib
{
    class AbilityManager
    {
        private List<BaseAbility> _abilitySpawnQueue;
        private List<IConctract> _abilitySpawnMessageQueue;

        private bool _hasNewAbilitySpawnData;


        public IEnumerable<IConctract> GetNewSpawnMessages()
        {
            return _abilitySpawnMessageQueue;
        }

        public IEnumerable<BaseAbility> GetNewSpawnAbilities()
        {
            return _abilitySpawnQueue;
        }

        public bool HasNewAbilitySpawnData()
        {
            return _hasNewAbilitySpawnData;
        }

        public void ClearSpawnQueue()
        {
            this._abilitySpawnQueue.Clear();
            this._abilitySpawnMessageQueue.Clear();
        }

        public AbilityManager()
        {
            this._abilitySpawnQueue = new List<BaseAbility>();
            this._abilitySpawnMessageQueue = new List<IConctract>();
        }

        public void AddAbilityToQueue(ClientCastAbilityData clientCastAbilityData, LivingEntity caster)
        {
            if (!caster.IsReadyToCastAbility((AbilityType) clientCastAbilityData.AbilityType)) return;

            var newAbility = GetNewAbility(clientCastAbilityData, caster);
            _abilitySpawnQueue.Add(newAbility);
            _abilitySpawnMessageQueue.Add(newAbility.OnPlayerConnectedMessage());
            _hasNewAbilitySpawnData = true;
        }

        private BaseAbility GetNewAbility(ClientCastAbilityData clientCastAbilityData, LivingEntity caster)
        {
            switch ((AbilityType)clientCastAbilityData.AbilityType)
            {
                case AbilityType.Fireball:
                    return new Fireball(clientCastAbilityData, caster);
                    break;
            }

            return null;
        }


    }
}
