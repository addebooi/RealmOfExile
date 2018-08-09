using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;
using LiteNetLib;

namespace TestLiteLib
{
    class Fireball : BaseAbility
    {

        public float Damage { get; set; }
        public float LifeTime { get; set; }
        private float currentLifeTime { get; set; }

        public Fireball(ClientCastAbilityData clientCastData, LivingEntity caster)
        {
            this.Damage = 10;
            this.LifeTime = 2;
            currentLifeTime = 0;
            this.speed = 10;
            //this.Collider.collisionType = CollisionType.Fireball;
            this.OwnerID = caster.OwnerID;
            this.Direction = clientCastData.VectorData;
            this.UpdateWithDirection = true;
            this.Position = caster.Position;
            this.Collider = new CollisionCircle2D(this, 0.5f, CollisionType.Fireball);
            this.abilityType = AbilityType.Fireball;
            this.Caster = caster;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            currentLifeTime += dt;

            if (currentLifeTime >= LifeTime)
                ShouldBeDeleted = true;
        }

        public override IConctract OnPlayerConnectedMessage(NetPeer peer)
        {
            return new SpawnFireballData(OwnerID, ObjectID, Position, Rotation, scale, speed, Damage, Direction);
        }

        public override IConctract OnPlayerConnectedMessage()
        {
            return new SpawnFireballData(OwnerID, ObjectID, Position, Rotation, scale, speed, Damage, Direction);
        }

        public override void OnCollisionEnter(Object other, CollisionType collisionType)
        {
            switch (collisionType)
            {
                case CollisionType.Player:
                    playerCollision((Player)other);
                    break;
            }
        }

        private void playerCollision(Player player)
        {
            player.TakeDamage(Damage);
            ShouldBeDeleted = true;
        }
    }
}
