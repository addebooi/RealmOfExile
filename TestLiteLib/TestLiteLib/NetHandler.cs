using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;
using LiteNetLib;
using LiteNetLib.Utils;

namespace TestLiteLib
{
    public class NetUpdate
    {
        public IConctract Conctract;
        public SendOptions SendOption;

        public NetUpdate(IConctract contract, SendOptions SendOption)
        {
            this.Conctract = contract;
            this.SendOption = SendOption;
        }
    }
    class NetHandler
    {


        private NetDataWriter _writer;
        private Dictionary<long, NetPeer> _connections;
        private Dictionary<long, List<NetUpdate>> _newNetUpdates;

        private Dictionary<long, NetDataWriter[]> _writerMapper;
        public Server tempServer { get; set; }

        private float timeSinceLastSent;
        private const float sendEveryMS = 10;
        private const int _maxUnreliableDataSize = 400;
        private object _eventLock;
        private object _writeMapperLock;
        public NetHandler()
        {
            _writer = new NetDataWriter();
            _connections = new Dictionary<long, NetPeer>();
            _newNetUpdates = new Dictionary<long, List<NetUpdate>>();
            _writerMapper = new Dictionary<long, NetDataWriter[]>();
            _eventLock = new object();
            _writeMapperLock = new object();
        }


        //public void PositionUpdate(Object obj, bool interpolate = false)
        //{
        //    var posDat = new UpdatePositionData(obj.ObjectID, obj.Position);
        //    AddNetUpdateToAllplayers(posDat, SendOptions.Unreliable);
        //}

        //public void SpawnPlayer(Player player, long playerOwnerID)
        //{
        //    var spawnData = new SpawnData(ObjectType.Player, true, player.Position, player.Rotation, player.scale);
        //    AddNetUpdateToPlayer(playerOwnerID, spawnData, SendOptions.ReliableUnordered);
        //    foreach (var c in _newNetUpdates.Where(x => x.Key != playerOwnerID))
        //    {
        //        SpawnPlayerForPlayer(player, c.Key);
                
        //    }
        //}

        public void SpawnObjForPlayer(IConctract message, long playerToSpawnTo)
        {
            AddNetUpdateToPlayer(playerToSpawnTo, message, SendOptions.ReliableOrdered);
        }

        public void SpawnObjForAllPlayers(IConctract message)
        {
            foreach (var c in _newNetUpdates)
            {
                AddNetUpdateToPlayer(c.Key, message, SendOptions.ReliableOrdered);
            }
        }

        public void SpawnObjForAllExceptPlayer(IConctract message, long playerToExcept)
        {
            foreach (var c in _newNetUpdates.Where(x => x.Key != playerToExcept))
            {
                AddNetUpdateToPlayer(c.Key, message, SendOptions.ReliableOrdered);
            }
        }

        public void AddNetUpdateToPlayer(long playerID, IConctract message, SendOptions sendOption)
        {
            var netUpdate = new NetUpdate(message, sendOption);
            AddNetUpdateToPlayer(playerID, netUpdate);
        }

        public void AddNetUpdateToPlayer(long playerID, NetUpdate netUpdate)
        {
            _newNetUpdates[playerID].Add(netUpdate);
        }

        public void AddNetUpdateToAllExceptPlayer(long playerID, IConctract message, SendOptions sendOption)
        {
            var netUpdate = new NetUpdate(message, sendOption);
            AddNetUpdateToAllExceptPlayer(playerID, netUpdate);
        }

        public void AddNetUpdateToAllExceptPlayer(long playerID, NetUpdate netUpdate)
        {
            foreach (var c in _newNetUpdates)
            {
                if (c.Key != playerID)
                    AddNetUpdateToPlayer(c.Key, netUpdate);
            }
        }
        private void AddNetUpdateToPlayer(NetPeer player, IConctract message, SendOptions sendOption)
        {
            AddNetUpdateToPlayer(player.ConnectId, message, sendOption);
        }

        public void AddNetUpdateToAllplayers(IConctract message, SendOptions sendOption)
        {
            var netUpdate = new NetUpdate(message, sendOption);
            AddNetUpdateToAllplayers(netUpdate);
        }
        public void AddNetUpdateToAllplayers(NetUpdate netUpdate)
        {
            foreach (var c in _newNetUpdates)
            {
                AddNetUpdateToPlayer(c.Key, netUpdate);
            }
        }

        private void RemoveAllNetUpdates()
        {
            foreach (var c in _newNetUpdates)
            {
                c.Value.Clear();
            }
        }

        public void SendAllNetmessages()
        {
            foreach (var c in _newNetUpdates)
            {
                if(!c.Value.Any())continue;
                
                foreach (var n in c.Value)
                {
                    if (n.SendOption == SendOptions.Unreliable || n.SendOption == SendOptions.Sequenced)
                    {
                        if(_writerMapper[c.Key][(int)n.SendOption].Length> _maxUnreliableDataSize)
                            _connections[c.Key].Send(_writerMapper[c.Key][(int)n.SendOption], n.SendOption);

                        n.Conctract.AppendWriter(_writerMapper[c.Key][(int)n.SendOption]);
                    }
                    else
                    {
                        n.Conctract.AppendWriter(_writerMapper[c.Key][(int)n.SendOption]);
                    }
                    
                    
                }

                for (int i = 0; i < 4; i++)
                {
                    if (_writerMapper[c.Key][i].Length > 0)
                    {
                        //var reader = new NetDataReader();
                        //reader.SetSource(_writerMapper[c.Key][i]);
                        //CommunecationContract.CreateEvent(tempServer, null, reader);
                        _connections[c.Key].Send(_writerMapper[c.Key][i], (SendOptions)i);
                        _writerMapper[c.Key][i].Reset();
                    }
                        
                }
                
            }
            RemoveAllNetUpdates();
        }

        public void PlayerConnected(NetPeer netPeer)
        {
            _connections.Add(netPeer.ConnectId, netPeer);
            _newNetUpdates.Add(netPeer.ConnectId, new List<NetUpdate>());
            _writerMapper.Add(netPeer.ConnectId, new NetDataWriter[4]);
            for (int i = 0; i < 4; i++)
            {
                _writerMapper[netPeer.ConnectId][i] = new NetDataWriter();
            }
        }

        public void PlayerDisconnected(NetPeer netPeer)
        {
            _connections.Remove(netPeer.ConnectId);
            _newNetUpdates.Remove(netPeer.ConnectId);
            _writerMapper.Remove(netPeer.ConnectId);
        }

        public void Update(float dt)
        {
            timeSinceLastSent += dt * 1000;
            if (timeSinceLastSent < sendEveryMS) return;
            timeSinceLastSent = 0;
            
            SendAllNetmessages();
        }

    }
}
