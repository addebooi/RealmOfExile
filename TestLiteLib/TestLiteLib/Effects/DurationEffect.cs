using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLiteLib 
{
    class DurationEffect : BaseEffect
    {
        public float CurrentDuration;
        public float MaxDuration;

        public DurationEffect(float duration)
        {
            this.MaxDuration = duration;
            this.CurrentDuration = MaxDuration;
        }

        public void Update(float dt)
        {
            this.CurrentDuration -= dt;
            if (this.CurrentDuration <= 0)
            {
                IsActive = false;
            }
        }

        public override void OnDurationActive()
        {
            throw new NotImplementedException();
        }

        public override void OnDurationInactive()
        {
            throw new NotImplementedException();
        }
    }
}
