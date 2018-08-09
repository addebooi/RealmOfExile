using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;

namespace TestLiteLib
{
    abstract class BaseAgent : LivingEntity
    {
        public float Range;
        public float AggroRange;
        public LivingEntity Target;
        public float AttackRange;

                private bool _hasCastedAbility;
        private List<ClientCastAbilityData> _newCastedAbilities;

        public bool HasTarget
        {
            get { return Target != null; }
        }

        public const float ExtraAggroRange = 1.4f;
        public abstract void Attack(LivingEntity entiy);
        public abstract void Patrol();

        protected BaseAgent()
        {
            this.AttackRange = 1;
            this.Range = 1;
            this.AggroRange = 5;
            this.Collider.collisionType = CollisionType.Agent;
        }
        public void AddAbilityToQueue(ClientCastAbilityData data)
        {
            this._newCastedAbilities.Add(data);
            this._hasCastedAbility = true;
        }

        public IEnumerable<ClientCastAbilityData> GetNewlyCastedAbilities()
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
    }
}
