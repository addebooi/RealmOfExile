using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestLiteLib
{
    class CollisionBox2D : BaseCollider
    {
        public override CollisionShape collisionShape => CollisionShape.Box;
        private Vector3 _position;
        public Vector2 _size;
        public sides BoxSides;
        public bool IsEnabled;
        


        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                BoxSides.NeedUpdate = true;
            }
        }
        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                BoxSides.NeedUpdate = true;
            }
        }

        public CollisionBox2D(Object obj, Vector3 pos, Vector2 size, CollisionType collisionType = CollisionType.Static, bool isEnabled = true ) 
            : base(obj, collisionType, isEnabled)
        {
            this._position = pos;
            this._size = size;
            this.BoxSides = new sides(pos, size);
        }

        public class sides
        {
            public float Top;
            public float Right;
            public float Bot;
            public float Left;
            public bool NeedUpdate;

            public sides(Vector2 position, Vector2 size)
            {
                UpdateSides(position, size);
            }

            public void UpdateSides(Vector2 position, Vector2 size)
            {
                this.Top = position.y + size.y;
                this.Bot = position.y - size.y;
                this.Right = position.x + size.x;
                this.Left = position.x - size.x;
                this.NeedUpdate = false;
            }


        }
        public override bool IsTouching(BaseCollider other)
        {
            if (other.collisionShape == CollisionShape.Box)
                return CheckBoxToBoxCollision(this, (CollisionBox2D)other);
            else if (other.collisionShape == CollisionShape.Circle)
                return CheckCircleToBoxCollision(this, (CollisionCircle2D)other);
            return false;
        }
    }
 }
