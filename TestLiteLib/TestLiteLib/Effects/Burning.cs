using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLiteLib
{
    class Burning : DurationEffect
    {
        public float DamageTick;

        public Burning(float damageTick, float duration) : base(duration)
        {
            this.DamageTick = damageTick;
        }
    }
}
