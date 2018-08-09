using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CommunicationContract;
using UnityEngine;

namespace TestLiteLib
{
    public enum CollisionShape
    {
        Circle,
        Box
    }
    public enum CollisionType
    {
        Player,
        Static,
        Fireball,
        Agent,
        NumCollisionTypes
    }
    class CollisionHandler
    {
        public static bool EnableDebugCollision = true;
        private List<IConctract> newCollisions;
        public IEnumerable<IConctract> GetDebugCollisions()
        {
            
            if (EnableDebugCollision)
            {
                if (newCollisions.Any())
                {
                    return newCollisions;
                }
                return null;
            }
            return null;
        }

        public void ClearDebugCollisionList()
        {
            this.newCollisions.Clear();
        }


        //Bool represents if the CollisionType can touch its owner:
        //Example: Player CANT Collide with his own fireball, hence false
        private List<Tuple<CollisionType, CollisionType, bool>> _avaibleCollisions =
            new List<Tuple<CollisionType, CollisionType, bool>>()
            {
                new Tuple<CollisionType, CollisionType, bool>(CollisionType.Fireball, CollisionType.Static, true),
                new Tuple<CollisionType, CollisionType, bool>(CollisionType.Player, CollisionType.Fireball, false),
                new Tuple<CollisionType, CollisionType, bool>(CollisionType.Player, CollisionType.Agent, false),
                new Tuple<CollisionType, CollisionType, bool>(CollisionType.Agent, CollisionType.Fireball, false),
            };

        private bool IsAvaibleCollision(int collisionTypeIndex1, int collisionTypeIndex2)
        {
            var col1 = (CollisionType) collisionTypeIndex1;
            var col2 = (CollisionType) collisionTypeIndex2;
            foreach (var c in _avaibleCollisions)
            {
                if ((c.Item1 == col1 && c.Item2 == col2) || (c.Item1 == col2 && c.Item2 == col1))
                    return true;
            }

            return false;
        }

        private bool ValidateOwnerCollision(int collisionTypeIndex1, int collisionTypeIndex2)
        {
            var col1 = (CollisionType)collisionTypeIndex1;
            var col2 = (CollisionType)collisionTypeIndex2;
            foreach (var c in _avaibleCollisions)
            {
                if ((c.Item1 == col1 && c.Item2 == col2) || (c.Item1 == col2 && c.Item2 == col1))
                    return c.Item3;
            }
            return true;
        }

        //public struct CollisionDataStruct

        //{
        //    public Object NetObject;
        //    public long ObjectID;
        //    public BaseCollider Collider;
        //    public CollisionType CollisionType;

        //    public CollisionDataStruct(Object netobject)
        //    {
        //        this.NetObject = netobject;
        //        this.ObjectID = netobject.ObjectID;
        //        this.Collider = netobject.Collider;
        //        this.CollisionType = netobject.Collider.collisionType;
        //    }
        //}

        private List<BaseCollider> _colliders;
        private List<BaseCollider>[] _colliderMapper;
        private ContactFilter2D _collisionFilter2D;
        private List<Tuple<BaseCollider, BaseCollider>> _currentColliders;
        private List<BaseCollider> _collidersLastUpdate;
        private List<Tuple<long, Vector2>> PositionLastFrame;

        public CollisionHandler()
        {
            this._colliders = new List<BaseCollider>();
            this._colliderMapper = new List<BaseCollider>[(int)CollisionType.NumCollisionTypes];

            for (int i = 0; i < _colliderMapper.Length; i++)
            {
                _colliderMapper[i] = new List<BaseCollider>();
            }

            this._collisionFilter2D = new ContactFilter2D();
            this._currentColliders = new List<Tuple<BaseCollider, BaseCollider>>();
            this._collidersLastUpdate = new List<BaseCollider>();
            this.newCollisions =new List<IConctract>();
            PositionLastFrame = new List<Tuple<long, Vector2>>();
        }

        public void AddCollider(Object netObject)
        {
            if (!_colliders.Any(x=>x.ColliderObjectID == netObject.ObjectID))
            {
                var newCollider = netObject.Collider;
                _colliders.Add(newCollider);
                _colliderMapper[(int)newCollider.collisionType].Add(newCollider);

                //if (EnableDebugCollision)
                //{
                //    //????
                //    newCollisions.Add(new UpdateVariableData(VariableDataType.Vector2, "HitboxPosition", newCollider.ObjectID,
                //        newCollider.Collider.Position));

                //    newCollisions.Add(new UpdateVariableData(VariableDataType.Vector2, "HitboxSize", newCollider.ObjectID,
                //        newCollider.Collider.Size));
                //}
            }
        }

        public void RemoveCollider(Object netObject)
        {
            foreach (var c in _colliders)
            {
                if (c.IsCurrentlyCollidingWith(netObject.Collider))
                {
                    c.RemoveCurrentlyCollidingWith(netObject.Collider);
                    c.AddOldCollision(netObject.Collider);
                }
            }

            for (int i = 0; i < _colliders.Count; i++)
            {
                if (_colliders[i].ColliderObjectID == netObject.ObjectID)
                {
                    _colliders.RemoveAt(i);
                    break;
                }
            }

            for (int mapperIndex = 0; mapperIndex < (int) CollisionType.NumCollisionTypes; mapperIndex++)
            {
                for (int i = 0; i < _colliderMapper[mapperIndex].Count; i++)
                {
                    if (_colliderMapper[mapperIndex][i].ColliderObjectID == netObject.ObjectID)
                    {
                        _colliderMapper[mapperIndex].RemoveAt(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// We want to know which is new
        /// Update currently colliding
        /// 
        /// </summary>
        public void CheckCollision()
        {
            if(_currentColliders.Any()) _currentColliders.Clear();


            for (int i = 0; i < _colliderMapper.Length; i++)
            {
                for (int w = i; w < _colliderMapper.Length; w++)
                {
                    if(!IsAvaibleCollision(i,w)) continue;
                    for (int x = 0; x < _colliderMapper[i].Count; x++)
                    {
                        for (int y = 0; y < _colliderMapper[w].Count; y++)
                        {
                            if(i==w && x==y) continue;
                            var colObj1 = _colliderMapper[i][x];
                            var colObj2 = _colliderMapper[w][y];
                            //If im the owner, and i'm not allowed to touch my own creation, continue
                            if (colObj1.ColliderObject.OwnerID ==
                                colObj2.ColliderObject.OwnerID &&
                                !ValidateOwnerCollision(i, w)) continue;

                            if (colObj1.IsTouching(colObj2))
                            {
                                _currentColliders.Add(new Tuple<BaseCollider, BaseCollider>
                                    (colObj1, colObj2));
                                if (!colObj1.IsCurrentlyCollidingWith(colObj2))
                                {
                                    colObj1.AddNewCollision(colObj2);
                                    colObj1.AddCurrentlyColliding(colObj2);
                                }

                                if (!colObj2.IsCurrentlyCollidingWith(colObj1))
                                {
                                    colObj2.AddNewCollision(colObj1);
                                    colObj2.AddCurrentlyColliding(colObj1);
                                }

                                //var d = _colliderMapper[i][x].CollisionType.ToString();
                                //var f = _colliderMapper[w][y].CollisionType.ToString();
                                //Console.WriteLine($"Collision Between: {d} AND {f}\n");
                            }
                            else if (colObj1.IsCurrentlyCollidingWith(colObj2))
                            {
                                _currentColliders.Add(new Tuple<BaseCollider, BaseCollider>
                                    (colObj1, colObj2));
                                colObj1.RemoveCurrentlyCollidingWith(colObj2);
                                colObj2.RemoveCurrentlyCollidingWith(colObj1);

                                colObj1.AddOldCollision(colObj2);
                                colObj2.AddOldCollision(colObj1);
                            }
                        }
                    }
                }
            }

        }


        public void CallCollisionEvents()
        {
            foreach (var c in _currentColliders)
            {
                c.Item1?.CallCollisionEvents();
                c.Item2?.CallCollisionEvents();
            }

            if(_currentColliders.Any())_currentColliders.Clear();
        }

        //private void CallCollisionOnObject(CollisionDataStruct cds1, CollisionDataStruct cds2)
        //{

        //    switch (cds1.CollisionType)
        //    {
        //        case CollisionType.Player:
        //            cds2.NetObject.OnPlayerCollision(cds1.NetObject);
        //            break;
        //        case CollisionType.Static:
        //            cds2.NetObject.OnStaticCollision(cds1.NetObject);
        //            break;
        //        case CollisionType.Fireball:
        //            cds2.NetObject.OnFireballCollision(cds1.NetObject);
        //            break;
        //    }
        //}

        //public IEnumerable<IConctract> GetDebugContracts()
        //{
        //    List<CollisionDataStruct> listToUpdate = new List<CollisionDataStruct>();
        //    foreach (var c in _colliders)
        //    {
        //        if (!PositionLastFrame.Any(x => x.Item1 == c.ObjectID))
        //        {
        //            PositionLastFrame.Add(new Tuple<long, Vector3>(c.ObjectID, c.NetObject.Position));
        //            listToUpdate.Add(c);
        //        }
        //        else
        //        {
        //            for (int i = PositionLastFrame.Count - 1; i >= 0; i--)
        //            {
        //                if (PositionLastFrame[i].Item1 == c.ObjectID)
        //                {
        //                    if (PositionLastFrame[i].Item2 != c.NetObject.Position)
        //                    {
        //                        listToUpdate.Add(c);
        //                        PositionLastFrame[i] = new Tuple<long, Vector3>(c.ObjectID, c.NetObject.Position);
        //                    }
                                
        //                    break;
        //                }
        //            }
        //        }
                   
        //    }
        //    List<UpdateVariableData> debugData = new List<UpdateVariableData>();
        //    foreach (var c in listToUpdate)
        //    {
        //        debugData.Add(new UpdateVariableData(VariableDataType.Vector2, "HitboxPosition", c.ObjectID,
        //            new Vector2(c.Collider.Position.x, c.Collider.Position.z)));

        //        debugData.Add(new UpdateVariableData(VariableDataType.Vector2, "HitboxSize", c.ObjectID,
        //            c.Collider.Size));
        //    }

        //    return debugData;
        //}
    }
}
