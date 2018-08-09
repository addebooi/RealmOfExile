using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace CommunicationContract
{
    public interface IConctract
    {
        NetPeer GetSender();
        NetDataWriter FetchWriter();
        void AppendWriter(NetDataWriter writer);
    }

    public enum MessageType
    {
        ClickToPosition,
        SpawnData,
        UpdatePosition,
        UpdatePositionInterpolate,
        DeleteObject,
        SpawnPlayerData,
        SpawnFireballData,
        ClientSpawnFireballData,
        UpdateVariableData,
        ClientCastAbility
    }

    public enum ObjectType
    {
        Player,
        Dynamic,
        Ability,
        Agent,
        Static,
        
        Num_ObjectType
    }

    public enum VariableDataType
    {
        Float,
        Bool,
        Long,
        String,
        Vector2
    }

    public abstract class BaseData : IConctract
    {
        private NetPeer sender;
        public BaseData(NetPeer peer)
        {
            this.sender = peer;
        }
        public BaseData()
        {
            this.sender = null;
        }
        public NetPeer GetSender()
        {
            return sender;
        }

        public NetDataWriter FetchWriter()
        {
            throw new NotImplementedException();
        }

        public abstract void AppendWriter(NetDataWriter writer);
    }

    public class UpdateVariableData : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.UpdateVariableData;
        public VariableDataType variabeleDataType { get; set; }
        public string variableName { get; set; }
        public long objectID { get; set; }
        public bool variableDataBool {get; set; }
        public float variableDataFloat { get; set; }
        public long variableDataLong { get; set; }
        public string variableDataString { get; set; }
        public Vector2 variableDataVector2 { get; set; }

        public UpdateVariableData(NetDataReader reader, NetPeer sender) : base(sender)
        {

            variabeleDataType = (VariableDataType) reader.GetShort();
            variableName = reader.GetString();
            objectID = reader.GetLong();
            switch (variabeleDataType)
            {
                case VariableDataType.Bool:
                    this.variableDataBool = reader.GetBool();
                    break;
                case VariableDataType.Float:
                    this.variableDataFloat = reader.GetFloat();
                    break;
                case VariableDataType.Long:
                    this.variableDataLong = reader.GetLong();
                    break;
                case VariableDataType.String:
                    this.variableDataString = reader.GetString();
                    break;
                case VariableDataType.Vector2:
                    this.variableDataVector2 = reader.GetVector2();
                    break;
            }
        }

        public UpdateVariableData(VariableDataType type, string variableName, long objectID, bool boolValue)
        {
            SetOtherVariables(type, variableName, objectID);
            this.variableDataBool = boolValue;
        }
        public UpdateVariableData(VariableDataType type, string variableName, long objectID, float floatValue)
        {
            SetOtherVariables(type, variableName, objectID);
            this.variableDataFloat = floatValue;
        }
        public UpdateVariableData(VariableDataType type, string variableName, long objectID, long longValue)
        {
            SetOtherVariables(type, variableName, objectID);
            this.variableDataLong = longValue;
        }
        public UpdateVariableData(VariableDataType type, string variableName, long objectID, string stringValue)
        {
            SetOtherVariables(type, variableName, objectID);
            this.variableDataString = stringValue;
        }
        public UpdateVariableData(VariableDataType type, string variableName, long objectID, Vector2 vector2Value)
        {
            SetOtherVariables(type, variableName, objectID);
            this.variableDataVector2 = vector2Value;
        }

        private void SetOtherVariables(VariableDataType type, string variableName, long objectID)
        {
            this.variabeleDataType = type;
            this.variableName = variableName;
            this.objectID = objectID;
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
            writer.Put((short)variabeleDataType);
            writer.Put(variableName);
            writer.Put(objectID);
            switch (variabeleDataType)
            {
                case VariableDataType.Bool:
                    writer.Put(variableDataBool);
                    break;
                case VariableDataType.Float:
                    writer.Put(variableDataFloat);
                    break;
                case VariableDataType.Long:
                    writer.Put(variableDataLong);
                    break;
                case VariableDataType.String:
                    writer.Put(variableDataString);
                    break;
                case VariableDataType.Vector2:
                    writer.Put(variableDataVector2);
                    break;
            }
        }
    }

    public class DeleteObjectData : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.DeleteObject;
        public long objectID { get; set; }
        public DeleteObjectData(NetDataReader reader, NetPeer sender) : base(sender)
        {
            objectID = reader.GetLong();
        }

        public DeleteObjectData(long objectID)
        {
            this.objectID = objectID;
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
            writer.Put(objectID);
        }
    }

    public class UpdatePositionInterpolateData : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.UpdatePositionInterpolate;
        public long objectID { get; set; }
        public Vector3 position { get; set; }
        public UpdatePositionInterpolateData(NetDataReader reader, NetPeer sender) : base(sender)
        {
            objectID = reader.GetLong();
            position = reader.GetVector3();
        }

        public UpdatePositionInterpolateData(long objectID, Vector3 position)
        {

            this.objectID = objectID;
            this.position = position;
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
            writer.Put(objectID);
            writer.Put(position);
        }
    }

    public class UpdatePositionData  : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.UpdatePosition;
        public long objectID { get; set; }
        public Vector3 position { get; set; }
        public UpdatePositionData(NetDataReader reader, NetPeer sender) : base(sender)
        {
            objectID = reader.GetLong();
            position = reader.GetVector3();
        }

        public UpdatePositionData(long objectID, Vector3 position)
        {

            this.objectID = objectID;
            this.position = position;
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
            writer.Put(objectID);
            writer.Put(position);
        }
    }

    public class ClickToPosition : BaseData, IConctract
    {
        public virtual MessageType msgType => MessageType.ClickToPosition;
        public long objectID { get; set; }
        public Vector3 position { get; set; }

        public ClickToPosition(NetDataReader reader, NetPeer sender) : base(sender)
        {
            objectID = reader.GetLong();
            position = reader.GetVector3();
        }

        public ClickToPosition(long objectID, Vector3 position)
        {

            this.objectID = objectID;
            this.position = position;
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
            writer.Put(objectID);
            writer.Put(position);
        }
    }

    public class CommunecationContract
    {
        public static void CreateEvent(ICommuncationContract contract, NetPeer peer, NetDataReader reader)
        {
            while (!reader.EndOfData)
            {
                switch ((MessageType)reader.GetShort())
                {
                    case MessageType.ClickToPosition:
                        contract.OnClickToPosition(new ClickToPosition(reader, peer));
                        break;
                    case MessageType.SpawnData:
                        contract.OnSpawnData(new SpawnData(reader, peer));
                        break;
                    case MessageType.UpdatePosition:
                        contract.OnUpdatePosition(new UpdatePositionData(reader, peer));
                        break;
                    case MessageType.UpdatePositionInterpolate:
                        contract.OnUpdatePositionInterpolate(new UpdatePositionInterpolateData(reader, peer));
                        break;
                    case MessageType.DeleteObject:
                        contract.OnDeleteObject(new DeleteObjectData(reader, peer));
                        break;
                    case MessageType.SpawnPlayerData:
                        contract.OnSpawnPlayer(new SpawnPlayerData(reader, peer));
                        break;
                    case MessageType.SpawnFireballData:
                        contract.OnSpawnFireballData(new SpawnFireballData(reader, peer));
                        break;
                    case MessageType.ClientSpawnFireballData:
                        contract.OnClientSpawnFireballData(new ClientSpawnFireballData(reader, peer));
                        break;
                    case MessageType.UpdateVariableData:
                        contract.OnVariableUpdate(new UpdateVariableData(reader, peer));
                        break;
                    case MessageType.ClientCastAbility:
                        contract.OnClientCastAbility(new ClientCastAbilityData(reader, peer));
                        break;

                }
            }
        }
    }

    public interface ICommuncationContract
    {
        void OnClickToPosition(ClickToPosition clickToPositionObject);
        void OnSpawnData(SpawnData spawnDataObject);
        void OnUpdatePosition(UpdatePositionData updatePositionData);
        void OnUpdatePositionInterpolate(UpdatePositionInterpolateData updatePositionInterpolateData);
        void OnDeleteObject(DeleteObjectData deleteObjectData);
        void OnSpawnPlayer(SpawnPlayerData spawnPlayerData);
        void OnSpawnFireballData(SpawnFireballData spawnFireBallData);
        void OnClientSpawnFireballData(ClientSpawnFireballData clientSpawnFireballData);
        void OnVariableUpdate(UpdateVariableData updateVariableData);
        void OnClientCastAbility(ClientCastAbilityData clientCastAbilityData);
    }
}
