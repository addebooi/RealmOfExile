using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace CommunicationContract
{
    public class ClientCastAbilityData : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.ClientCastAbility;
        public short AbilityType { get; set; }
        public Vector3 VectorData { get; set; }
        

        public ClientCastAbilityData(NetDataReader reader, NetPeer sender) : base(sender)
        {
            AbilityType = reader.GetShort();
            VectorData = reader.GetVector3();

        }

        public ClientCastAbilityData(short abilityType, Vector3 vectorData)
        {
            this.AbilityType = abilityType;
            this.VectorData = vectorData;

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
            writer.Put(AbilityType);
            writer.Put(VectorData);
        }
    }
}
