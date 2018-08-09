using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLiteLib
{
    public enum EffectType
    {
        DoT,
        HoT,
       
    }

    abstract class BaseEffect
    {
        public bool IsActive;

        protected BaseEffect()
        {
            IsActive = true;
        }

        public abstract void OnDurationActive();
        public abstract void OnDurationInactive();

    }
}
