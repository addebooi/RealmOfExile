using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;

namespace TestLiteLib
{
    enum AbilityType
    {
        Fireball,
        NumAbilityTypes
    }
    class BaseAbility : DynamicObject
    {
        public AbilityType abilityType;
        public LivingEntity Caster;

        public BaseAbility()
        {

        }

        public BaseAbility(ClientCastAbilityData clientCastData, Player caster)
        {
            
        }

    }
}
