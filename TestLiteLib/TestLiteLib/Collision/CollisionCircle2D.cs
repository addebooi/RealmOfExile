using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestLiteLib
{
    class CollisionCircle2D : BaseCollider
    {
        public override CollisionShape collisionShape => CollisionShape.Circle;
        private float _radius;

        public float Radius
        {
            get { return this._radius; }

            set { this._radius = value; }
        }

        public CollisionCircle2D(Object obj, float radius, CollisionType collisionType = CollisionType.Static, bool isEnabled = true) : base(obj, collisionType, isEnabled)
        {
            this.Radius = radius;
        }

        //public bool IsTouching(CollisionCircle2D other)
        //{
        //    //(x2 - x1) ^ 2 + (y1 - y2) ^ 2 <= (r1 + r2) ^ 2

        //    return Math.Pow(other.Position.x - Position.x, 2) +
        //           Math.Pow(Position.z - other.Position.z, 2) <=
        //           Math.Pow(other.Radius + Radius, 2);
        //    return false;
        //}

        public override bool IsTouching(BaseCollider other)
        {
            if(other.collisionShape == CollisionShape.Box)
                return CheckCircleToBoxCollision(this, (CollisionBox2D)other);
            else if (other.collisionShape == CollisionShape.Circle)
                return CheckCircleToCircleCollision(this,(CollisionCircle2D) other);
            return false;
        }
    }
}
