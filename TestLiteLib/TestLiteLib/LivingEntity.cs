using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;
using LiteNetLib;
using UnityEngine;

namespace TestLiteLib
{
    class LivingEntity : DynamicObject
    {
        private float _currentHealth;
        public float CurrentHealth
        {
            get { return _currentHealth; }
            set { _currentHealth = value; }
        }

        private float _maxHealth;
        public float MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                _maxHealth = value;
                if (_currentHealth >= MaxHealth)
                    _currentHealth = MaxHealth;
            }
        }
        public bool IsDead;
        



        public LivingEntity()
        {
            this.MaxHealth = 1;
            this.CurrentHealth = 1;
            this.IsDead = false;
        }

        public bool IsReadyToCastAbility(AbilityType abilityType)
        {
            return true;
        }

        protected void UpdateMaxAndCurrentHealth(float Health)
        {
            this.MaxHealth = Health;
            this.CurrentHealth = MaxHealth;
        }

        public virtual void TakeDamage(float Damage)
        {
            this.CurrentHealth -= Damage;

            if (this.CurrentHealth <= 0)
                OnDead();

            this.AddMessage(new UpdateVariableData(VariableDataType.Float, "Health", ObjectID, CurrentHealth),
                SendOptions.ReliableUnordered, 2);
        }

        public virtual void OnDead()
        {
            IsDead = true;
        }
        public virtual void Die()
        {

        }

        public Vector3 GetDirectionTowards(Object obj)
        {
            return (obj.Position - this.Position).normalized;
        }
    }
}
