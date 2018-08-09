using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;

namespace TestLiteLib
{

    class SpawnHandler
    {
        private Dictionary<ObjectType, List<Object>> _staticObjectToSpawn;
        private Dictionary<ObjectType, List<DynamicObject>> _dynamicObjectToSpawn;
        private List<IConctract> _spawnObjectMessageQueue;
        private List<IConctract> _spawnPlayerMessageQueue;
        

        private bool _hasNewSpawnData;

        public SpawnHandler()
        {
            InstansiateObjectTypes();

        }

        private void InstansiateObjectTypes()
        {
            _staticObjectToSpawn = new Dictionary<ObjectType, List<Object>>();
            _dynamicObjectToSpawn = new Dictionary<ObjectType, List<DynamicObject>>();
            for (int i = 0; i < (int)ObjectType.Num_ObjectType; i++)
            {
                _staticObjectToSpawn.Add((ObjectType)i, new List<Object>());
                _dynamicObjectToSpawn.Add((ObjectType)i, new List<DynamicObject>());
            }
        }

        public IEnumerable<IConctract> GetNewSpawnMessages()
        {
            return _spawnObjectMessageQueue;
        }

        public Dictionary<ObjectType, List<Object>> GetNewStaticObjects()
        {
            return _staticObjectToSpawn;
        }

        public Dictionary<ObjectType, List<DynamicObject>> GetNewDynamicObjects()
        {
            return _dynamicObjectToSpawn;
        }

        public bool HasNewAbilitySpawnData()
        {
            return _hasNewSpawnData;
        }

        public void ClearSpawnQueue()
        {
            for (int i = 0; i < (int) ObjectType.Num_ObjectType; i++)
            {
                this._dynamicObjectToSpawn[(ObjectType) i].Clear();
                this._staticObjectToSpawn[(ObjectType) i].Clear();
            }
            this._spawnObjectMessageQueue.Clear();
        }


        public void SpawnNewStaticObject(Object obj, ObjectType objectType)
        {
            _staticObjectToSpawn[objectType].Add(obj);
            AddNetMessage(obj.OnPlayerConnectedMessage());
        }

        public void SpawnNewStaticObject(DynamicObject obj, ObjectType objectType)
        {
            _dynamicObjectToSpawn[objectType].Add(obj);
            AddNetMessage(obj.OnPlayerConnectedMessage());
        }

        private void AddNetMessage(IConctract message)
        {
            this._spawnObjectMessageQueue.Add(message);
        }
    }
}
