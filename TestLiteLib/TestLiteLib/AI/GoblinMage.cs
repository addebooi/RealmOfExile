using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;
using UnityEngine;

namespace TestLiteLib
{
    class GoblinMage : BaseAgent
    {
        public GoblinMage()
        {
            this.speed = 2;
            this.UpdateMaxAndCurrentHealth(35);
        }


        public override void Update(float dt)
        {
            if (!HasTarget) return;

            if(Vector3.Distance(this.Position, Target.Position)< AttackRange)
                Attack(Target);

            

        }

        public override void Attack(LivingEntity entity)
        {
            var castData
                = new ClientCastAbilityData((short)AbilityType.Fireball, GetDirectionTowards(entity));
        }

        public override void Patrol()
        {
            throw new NotImplementedException();
        }
    }
}
