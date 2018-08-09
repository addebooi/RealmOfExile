using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace CommunicationContract
{
    public class ClientSpawnFireballData : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.ClientSpawnFireballData;
        public Vector3 direction { get; set; }

        public ClientSpawnFireballData(NetDataReader reader, NetPeer sender) : base(sender)
        {
            direction = reader.GetVector3();
        }

        public ClientSpawnFireballData(Vector3 direction)
        {
            this.direction = direction;
        }

        public NetDataWriter FetchWriter()
        {
            var writer = new NetDataWriter();
            AppendWriter(writer);
            return writer;
        }

        public override void AppendWriter(NetDataWriter writer)
        {
            writer.Put((short)msgType);
            writer.Put(direction);
        }
    }

    public class SpawnFireballData : SpawnData, IConctract
    {
        public override MessageType msgType => MessageType.SpawnFireballData;
        public float speed { get; set; }
        public float damage { get; set; }
        public Vector3 Direction { get; set; }

        public NetDataWriter FetchWriter()
        {
            var writer = base.FetchWriter();
            AppendWriter(writer);
            return writer;
        }
        public override void AppendWriter(NetDataWriter writer)
        {
            base.AppendWriter(writer);
            writer.Put(speed);
            writer.Put(damage);
            writer.Put(Direction);
        }


        public SpawnFireballData(NetDataReader reader, NetPeer sender) : base(reader, sender)
        {
            speed = reader.GetFloat();
            damage = reader.GetFloat();
            Direction = reader.GetVector3();
        }

        public SpawnFireballData(long owner, long objectID,
            Vector3 pos, Vector3 rot, Vector3 scale, float speed, float damage, Vector3 direction)
            : base(ObjectType.Dynamic, owner, objectID, pos, rot, scale)
        {
            this.speed = speed;
            this.damage = damage;
            this.Direction = direction;
        }
    }

    public class SpawnPlayerData : SpawnData, IConctract
    {
        public override MessageType msgType => MessageType.SpawnPlayerData;
        public bool IsOwner { get; set; }
        public float speed { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }

        public NetDataWriter FetchWriter()
        {
            var writer = base.FetchWriter();
            AppendWriter(writer);
            return writer;
        }
        public override void AppendWriter(NetDataWriter writer)
        {
            base.AppendWriter(writer);
            writer.Put(IsOwner);
            writer.Put(speed);
            writer.Put(Health);
            writer.Put(MaxHealth);
        }


        public SpawnPlayerData(NetDataReader reader, NetPeer sender) : base(reader, sender)
        {
            IsOwner = reader.GetBool();
            speed = reader.GetFloat();
            Health = reader.GetFloat();
            MaxHealth = reader.GetFloat();
        }

        public SpawnPlayerData(long owner, long objectID,
            Vector3 pos, float height, Vector3 rot, Vector3 scale, bool isOwner, float speed, float health, float maxHealth)
            : base(ObjectType.Dynamic, owner, objectID, pos, rot, scale)
        {
            this.IsOwner = isOwner;
            this.speed = speed;
            this.Health = health;
            this.MaxHealth = maxHealth;
        }
    }
    public class SpawnData : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.SpawnData;
        public ObjectType spawnType { get; set; }
        public long owner { get; set; }
        public long objectID { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }

        public SpawnData(NetDataReader reader, NetPeer sender) : base(sender)
        {
            spawnType = (ObjectType)reader.GetShort();
            owner = reader.GetLong();
            objectID = reader.GetLong();
            position = reader.GetVector3();
            rotation = reader.GetVector3();
            scale = reader.GetVector3();
        }

        public SpawnData(ObjectType type, long owner, long objectID, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            this.spawnType = type;
            this.owner = owner;
            this.objectID = objectID;
            this.position = pos;
            this.rotation = rot;
            this.scale = scale;
        }

        public NetDataWriter FetchWriter()
        {
            var writer = new NetDataWriter();
            AppendWriter(writer);
            return writer;
        }
        public override void AppendWriter(NetDataWriter writer)
        {
            writer.Put((short)msgType);
            writer.Put((short)spawnType);
            writer.Put(owner);
            writer.Put(objectID);
            writer.Put(position);
            writer.Put(rotation);
            writer.Put(scale);
        }
    }

}
