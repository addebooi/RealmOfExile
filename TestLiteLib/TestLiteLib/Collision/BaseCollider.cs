using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestLiteLib
{
    class BaseCollider
    {
        
        public bool IsEnabled;
        private Dictionary<long, BaseCollider> NewCollisions;
        private Dictionary<long, BaseCollider> CurrentlyCollidingWith;
        private Dictionary<long, BaseCollider> OldCollisions;

        public Object ColliderObject;
        public long ColliderObjectID;
        public CollisionType collisionType;
        public virtual CollisionShape collisionShape => CollisionShape.Box;
        public Vector3 _position;

        public virtual Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
            }
        }

        public BaseCollider(Object obj, CollisionType collisionType, bool isEnabled = true)
        {
            this.NewCollisions = new Dictionary<long, BaseCollider>();
            this.CurrentlyCollidingWith = new Dictionary<long, BaseCollider>();
            this.OldCollisions = new Dictionary<long, BaseCollider>();
            this.ColliderObject = obj;
            this.ColliderObjectID = obj.ObjectID;
            this.collisionType = collisionType;
            this.IsEnabled = isEnabled;
        }

        public void AddNewCollision(BaseCollider other)
        {
            this.NewCollisions.Add(other.ColliderObjectID, other);
        }

        public void RemoveNewCollision(BaseCollider other)
        {
            this.NewCollisions.Remove(other.ColliderObjectID);
        }
        public void AddCurrentlyColliding(BaseCollider other)
        {
            this.CurrentlyCollidingWith.Add(other.ColliderObjectID, other);
        }

        public void RemoveCurrentlyCollidingWith(BaseCollider other)
        {
            this.CurrentlyCollidingWith.Remove(other.ColliderObjectID);
        }
        public void AddOldCollision(BaseCollider other)
        {
            this.OldCollisions.Add(other.ColliderObjectID, other);
        }

        public void RemoveOldCollisions(BaseCollider other)
        {
            this.OldCollisions.Remove(other.ColliderObjectID);
        }
        public bool IsCurrentlyCollidingWith(BaseCollider other)
        {
            return this.CurrentlyCollidingWith.ContainsKey(other.ColliderObjectID);
        }

        public void CallCollisionEvents()
        {
            if (NewCollisions.Any())
            {
                foreach (var newCollision in NewCollisions)
                {
                    ColliderObject.OnCollisionEnter(newCollision.Value.ColliderObject, newCollision.Value.collisionType);
                }
                NewCollisions.Clear();
            }

            if (CurrentlyCollidingWith.Any())
            {
                foreach (var currentlyColliding in CurrentlyCollidingWith)
                {
                    ColliderObject.OnCollisionStay(currentlyColliding.Value.ColliderObject, currentlyColliding.Value.collisionType);
                }
            }

            if (OldCollisions.Any())
            {
                foreach (var oldCollision in OldCollisions)
                {
                    ColliderObject.OnCollisionLeave(oldCollision.Value.ColliderObject, oldCollision.Value.collisionType);
                }
                OldCollisions.Clear();
            }

        }

        protected bool CheckCircleToCircleCollision(CollisionCircle2D self, CollisionCircle2D other)
        {
            return Mathf.Pow(other.Position.x - self.Position.x, 2) +
                   Mathf.Pow(self.Position.z - other.Position.z, 2) <=
                   Mathf.Pow(other.Radius + self.Radius, 2);
        }

        protected bool CheckCircleToBoxCollision(CollisionBox2D self, CollisionCircle2D other)
        {
            return CheckCircleToBoxCollision(other, self);
        }
        protected bool CheckCircleToBoxCollision(CollisionCircle2D self, CollisionBox2D other)
        {
            // clamp(value, min, max) - limits value to the range min..max
            var boxSides = other.BoxSides;
            if (other.BoxSides.NeedUpdate)
            {
                boxSides.UpdateSides(new Vector2(other.Position.x, other.Position.z), other.Size);
            }

            //var DeltaX = self.Position.x - Mathf.Max(RectX, Min(CircleX, RectX + RectWidth));
            //var DeltaY = self.Position.z - Mathf.Max(RectY, Min(CircleY, RectY + RectHeight));
            //return (DeltaX * DeltaX + DeltaY * DeltaY) < (CircleRadius * CircleRadius);

            //Find the closest point to the circle within the rectangle
            float closestX = Mathf.Clamp(self.Position.x, boxSides.Left, boxSides.Right);
            float closestY = Mathf.Clamp(self.Position.z, boxSides.Top, boxSides.Bot);

            // Calculate the distance between the circle's center and this closest point
            float distanceX = self.Position.x - closestX;
            float distanceY = self.Position.z - closestY;

            // If the distance is less than the circle's radius, an intersection occurs
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (self.Radius * self.Radius);
        }
        protected bool CheckBoxToBoxCollision(CollisionBox2D self, CollisionBox2D other)
        {
            var otherSides = other.BoxSides;
            var selfSides = self.BoxSides;
            if (self.BoxSides.NeedUpdate)
            {
                selfSides.UpdateSides(new Vector2(self.Position.x, self.Position.z), self.Size);
            }

            if (other.BoxSides.NeedUpdate)
            {
                otherSides.UpdateSides(new Vector2(other.Position.x, other.Position.z), other.Size);
            }
            

            return !(otherSides.Left > selfSides.Right
                     || otherSides.Right < selfSides.Left
                     || otherSides.Top < selfSides.Bot
                     || otherSides.Bot > selfSides.Top);
            return false;
        }

        public virtual bool IsTouching(BaseCollider otherBox)
        {
            return false;
        }
    }
}
