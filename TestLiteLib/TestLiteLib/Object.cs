using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CommunicationContract;
using LiteNetLib;

namespace TestLiteLib
{
    class Object
    {
        private static long ObjectIDCounter = 1;
        public long ObjectID { get; set; }
        public long OwnerID { get; set; }

        protected Vector3 _position;
        public Vector3 Position
        {
            get { return _position;}
            set
            {
                _position = value;
                if (Collider.IsEnabled)
                {
                    Collider.Position = _position;
                }
            }
        }

        public float Height { get; set; }

        public Vector3 Rotation { get; set; }
        public Vector3 scale { get; set; }
        public Vector3 Direction { get; set; }
        public BaseCollider Collider { get; set; }

        public Object()
        {
            this.OwnerID = 0;
            this._position = new Vector3(0,0,0);
            this.Rotation = new Vector3(0, 0, 0);
            this.scale = new Vector3(1, 1, 1);
            this.Direction = new Vector3(0, 0,0);
            this.ObjectID = ObjectIDCounter++;
            this.Collider = new CollisionBox2D(this, Position, new Vector2(0.5f, 0.5f));
        }

        public virtual IConctract OnPlayerConnectedMessage(NetPeer peer)
        {
            return null;
        }

        public virtual IConctract OnPlayerConnectedMessage()
        {
            return null;
        }

        public virtual void OnCollisionStay(Object other, CollisionType collisionType)
        {

        }

        public virtual void OnCollisionEnter(Object other, CollisionType collisionType)
        {

        }
        public virtual void OnCollisionLeave(Object other, CollisionType collisionType)
        {

        }
    }
}
