using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicationContract;
using LiteNetLib;
using UnityEngine;

namespace TestLiteLib
{
    class Game
    {
        private Dictionary<ObjectType, List<Object>> _staticObjects;
        private Dictionary<ObjectType, List<DynamicObject>> _dynamicObjects;
        private List<Player> _players;
        private List<Object> _allObjects;
        private NetHandler _netHandlerRef;
        private List<NetUpdate> _updateEvents;
        private CollisionHandler _collisionHandler;
        private AbilityManager _abilityManager;
        private AgentManager _agentManager;
        private SpawnHandler _spawnHandler;


        public Game(NetHandler netHandler)
        {
            _updateEvents = new List<NetUpdate>();
            _collisionHandler = new CollisionHandler();
            _allObjects = new List<Object>();
            _abilityManager = new AbilityManager();
            _spawnHandler = new SpawnHandler();
            _agentManager = new AgentManager();
            _players = new List<Player>();

            InstansiateObjectTypes();
            _netHandlerRef = netHandler;
        }

        private void InstansiateObjectTypes()
        {
            _staticObjects = new Dictionary<ObjectType, List<Object>>();
            _dynamicObjects = new Dictionary<ObjectType, List<DynamicObject>>();
            for (int i = 0; i < (int)ObjectType.Num_ObjectType; i++)
            {
                _staticObjects.Add((ObjectType)i, new List<Object>());
                _dynamicObjects.Add((ObjectType)i, new List<DynamicObject>());
            }
        }

        public void Update(float dt)
        {
            _collisionHandler.CallCollisionEvents();

            Server.StartFunctionalityTime("Dynamic Update");
            UpdateDynamicObjects(dt);
            Server.StopFunctionalityTime();
            UpdateAgentManager(dt);
            UpdateAbilityManager();

            HandleDeletionOfObjects();
            
            Server.StartFunctionalityTime("Checking/Call Collision Events");
            _collisionHandler.CheckCollision();
            Server.StopFunctionalityTime();


            HandleNetwork(dt);
        }

        private void UpdateAgentManager(float dt)
        {
            _agentManager.Update(dt, _players);

            if (_agentManager.HasCastedAbility())
            {
                foreach (var c in _agentManager.GetNewlyCastedAbilities())
                    _abilityManager.AddAbilityToQueue(c.CastData, c.Caster);

                _agentManager.ClearNewlyCastedAbilities();
            }

            if (_agentManager.HasSpawnAgent())
            {
                foreach (var c in _agentManager.GetNewlySpawnedAgents())
                {
                    AddDynamicObject(ObjectType.Agent, c);

                    //_netHandlerRef.SpawnObjForAllPlayers(c.OnPlayerConnectedMessage());
                }
                _abilityManager.ClearSpawnQueue();
            }
        }

        private void UpdateAbilityManager()
        {




            if (!_abilityManager.HasNewAbilitySpawnData()) return;

            foreach (var c in _abilityManager.GetNewSpawnAbilities())
                AddDynamicObject(ObjectType.Ability, c);

            foreach(var c in _abilityManager.GetNewSpawnMessages())
                _netHandlerRef.SpawnObjForAllPlayers(c);

            _abilityManager.ClearSpawnQueue();
        }

        private void UpdateDynamicObjects(float dt)
        {
            
            foreach (var obj in _dynamicObjects.Values)
            {
                foreach (var dObj in obj)
                {

                    dObj.Update(dt);
                    if (dObj.HasUpdatedEvents())
                    {
                        _updateEvents.AddRange(dObj.GetUpdateEvents());
                        dObj.ClearUpdateEvents();
                    }

                }
            }
        }

        private void HandleNetwork(float dt)
        {

            Server.StartFunctionalityTime("Adding new Net Updates");
            if (_updateEvents.Any())
            {
                foreach (var c in _updateEvents)
                    _netHandlerRef.AddNetUpdateToAllplayers(c);

                _updateEvents.Clear();
            }
            Server.StopFunctionalityTime();

            Server.StartFunctionalityTime("Transmitting Updates");
            _netHandlerRef.Update(dt);
            Server.StopFunctionalityTime();

        }

        private void HandleDeletionOfObjects()
        {
            foreach (var obj in _dynamicObjects)
            {
                for (int i = obj.Value.Count - 1; i >= 0; i--)
                {
                    if (!obj.Value[i].ShouldBeDeleted) continue;

                    OnDeleteDynamicObject(obj.Value[i]);
                }

            }

        }

        public void OnPlayerConnected(NetPeer peer)
        {
            Console.WriteLine("Player Connected :)");
            _netHandlerRef.PlayerConnected(peer);
            var player = SpawnPlayer(peer);
            _players.Add(player);
            SpawnWorldForPlayer(peer);
            _netHandlerRef.SpawnObjForAllExceptPlayer(player.OnPlayerConnectedMessage(), peer.ConnectId);
        }

        public void OnFireballSpawn(ClientSpawnFireballData clientSpawnFireballData)
        {
            //var player = (Player) _dynamicObjects[ObjectType.Player]
            //    .FirstOrDefault(x => x.OwnerID == clientSpawnFireballData.GetSender().ConnectId);
            //_abilityManager.AddAbilityToQueue();

            //Fireball fireball = new Fireball(clientSpawnFireballData, player);
            //AddDynamicObject(ObjectType.Dynamic, fireball);
            //_netHandlerRef.SpawnObjForAllPlayers(fireball.OnPlayerConnectedMessage());
        }

        public void OnClientCastAbility(ClientCastAbilityData clientCastAbilityData)
        {
            _abilityManager.AddAbilityToQueue(clientCastAbilityData,
                GetPlayerFromConnectionID(clientCastAbilityData.GetSender().ConnectId));
        }

        private Player GetPlayerFromConnectionID(long connectionID)
        {
            return (Player)_dynamicObjects[ObjectType.Player]
                .FirstOrDefault(x => x.OwnerID == connectionID);
        }

        /// <summary>
        /// This function should spawn all the Server Objects for a client
        /// </summary>
        /// <param name="peer"></param>
        private void SpawnWorldForPlayer(NetPeer peer)
        {
            foreach (var c in _allObjects)
            {
                var spawnObj = c.OnPlayerConnectedMessage(peer);
                if (spawnObj != null)
                    _netHandlerRef.SpawnObjForPlayer(spawnObj, peer.ConnectId);
            }
        }

        public void OnPlayerDisconnected(NetPeer peer)
        {
            
            Console.WriteLine("Player Disconnected :(");
            _netHandlerRef.PlayerDisconnected(peer);
            var playerObject = _dynamicObjects[ObjectType.Player].FirstOrDefault(x => x.OwnerID == peer.ConnectId);
            _players.Remove((Player)playerObject);
            OnDeleteDynamicObject(playerObject);
        }

        private void OnDeleteStaticObject(Object obj)
        {
            _netHandlerRef.AddNetUpdateToAllplayers(
                new DeleteObjectData(obj.ObjectID),
                SendOptions.ReliableUnordered);
            _collisionHandler.RemoveCollider(obj);

            for (int i = 0; i < _allObjects.Count; i++)
            {
                if (_allObjects[i].ObjectID == obj.ObjectID)
                {
                    _allObjects.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < _staticObjects.Count; i++)
            {
                var objType = (ObjectType)i;
                for (int y = 0; y < _staticObjects[objType].Count; y++)
                {
                    if (_staticObjects[objType][y].ObjectID == obj.ObjectID)
                    {
                        _staticObjects[objType].RemoveAt(y);
                        return;
                    }
                }
            }
        }


        private void OnDeleteDynamicObject(Object obj)
        {
            _netHandlerRef.AddNetUpdateToAllplayers(
                new DeleteObjectData(obj.ObjectID),
                SendOptions.ReliableUnordered);
            _collisionHandler.RemoveCollider(obj);

            for (int i = 0; i < _allObjects.Count; i++)
            {
                if (_allObjects[i].ObjectID == obj.ObjectID)
                {
                    _allObjects.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < _dynamicObjects.Count; i++)
            {
                var objType = (ObjectType)i;
                for (int y = 0; y < _dynamicObjects[objType].Count; y++)
                {
                    if (_dynamicObjects[objType][y].ObjectID == obj.ObjectID)
                    {
                        _dynamicObjects[objType].RemoveAt(y);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns a player on the Server And on all the Clients, also assigns ownership to the spawning player 
        /// </summary>
        /// <param name="peer"></param>
        private Player SpawnPlayer(NetPeer peer)
        {
            var player = new Player(peer);
            AddDynamicObject(ObjectType.Player, player);
            return player;
        }

        

        private void AddDynamicObject(ObjectType type, DynamicObject dynamicObject)
        {
            _allObjects.Add(dynamicObject);
            _dynamicObjects[type].Add(dynamicObject);
            _collisionHandler.AddCollider(dynamicObject);
        }
        private void AddStaticObject(ObjectType type, Object staticObject)
        {
            _allObjects.Add(staticObject);
            _staticObjects[type].Add(staticObject);
            _collisionHandler.AddCollider(staticObject);
        }

        public void UpdateClicktoPosition(ClickToPosition clickToPosition)
        {
            var objToUpdate = GetDynamicObjectWithID(clickToPosition.objectID);

            if (!IsOwner(objToUpdate, clickToPosition.GetSender())) return;
            if (objToUpdate != null)
            {
                objToUpdate.AssignMoveToPosition(clickToPosition.position);
            }
        }

        private bool IsOwner(Object obj, NetPeer sender)
        {
            return (sender.ConnectId == obj.OwnerID);
        }

        private DynamicObject GetDynamicObjectWithID(long objectID)
        {
            foreach (var dyn in _dynamicObjects)
            {
                var obj = dyn.Value.FirstOrDefault(x => x.ObjectID == objectID);
                if (obj != null) return obj;
            }

            return null;
        }

        private DynamicObject GetDynamicObjectWithID(long objectID, ObjectType type)
        {
            return _dynamicObjects[type].FirstOrDefault(x => x.ObjectID == objectID);
        }

        private Object GetStaticObjectWithID(long objectID)
        {
            foreach (var dyn in _staticObjects)
            {
                var obj = dyn.Value.FirstOrDefault(x => x.ObjectID == objectID);
                if (obj != null) return obj;
            }

            return null;
        }

        private Object GetStaticObjectWithID(long objectID, ObjectType type)
        {
            return _staticObjects[type].FirstOrDefault(x => x.ObjectID == objectID);
        }
    }
}

