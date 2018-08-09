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
    class Player : LivingEntity
    {

        public List<BaseEffect> effects { get; set; }

        public Player(NetPeer peer)
        {
            this.OwnerID = peer.ConnectId;
            this.speed = 10;
            this.UpdateMaxAndCurrentHealth(100);
            this.Collider.collisionType = CollisionType.Player;
            
            //this.Collider = new CollisionCircle2D(this, 0.5f, CollisionType.Player);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override IConctract OnPlayerConnectedMessage(NetPeer peer)
        {
            var isOwner = peer.ConnectId == OwnerID;
            return new SpawnPlayerData(OwnerID, ObjectID, Position, Height, Rotation, scale, isOwner, speed, CurrentHealth, MaxHealth);
        }

        public override IConctract OnPlayerConnectedMessage()
        {
            var isOwner = false;
            return new SpawnPlayerData(OwnerID, ObjectID, Position, Height, Rotation, scale, isOwner, speed, CurrentHealth, MaxHealth);
        }


    }
}
