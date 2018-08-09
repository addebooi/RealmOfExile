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
    class DynamicObject : Object
    {
        private const float _baseTolerance = 0.01f;
        private float TOLERANCE = 0.01f;

        private float _speed;
        public float speed
        {
            get { return _speed;}
            set
            {
                _speed = value;
                this.TOLERANCE = _baseTolerance * speed;
            }
        }

        public Vector3 moveToPosition { get; set; }
        public Vector3 moveToDirection { get; set; }

        protected Dictionary<int, int>_updateEventsForObjectsMapper;
        protected List<NetUpdate> UpdateEvents;

        private bool _hasUpdatedEvent;
        public bool ShouldBeDeleted;

        protected bool UpdateWithDirection;
        protected bool UpdateWithMoveToPosition;


        protected float syncPositionEveryMS;
        protected float currentSyncPositionEveryMS;

        protected List<BaseEffect> effects;

        public DynamicObject()
        {
            speed = 1;
            UpdateEvents = new List<NetUpdate>();
            _updateEventsForObjectsMapper = new Dictionary<int, int>();
            syncPositionEveryMS = 100;
            currentSyncPositionEveryMS = 0;
        }


        public virtual void Update(float dt)
        {
            currentSyncPositionEveryMS += dt;
            if(UpdateWithMoveToPosition)
                UpdateMoveToPosition(dt);

            if (UpdateWithDirection)
                UpdateDirectionPosition(dt);
        }

        private void UpdateDirectionPosition(float dt)
        {
            this.Position += Direction * speed * dt;
            var positionDat = new UpdatePositionInterpolateData(ObjectID, Position);
            AddMessage(positionDat, SendOptions.Unreliable, 1);
        }

        private void UpdateMoveToPosition(float dt)
        {

            this.Position += (moveToPosition - Position).normalized * speed * dt;
            var positionDat = new UpdatePositionInterpolateData(ObjectID, Position);
            AddMessage(positionDat, SendOptions.Unreliable, 1);

            if (Vector3.Distance(Position, moveToPosition) < TOLERANCE)
                UpdateWithMoveToPosition = false;
        }

        protected void AddMessage(IConctract message, SendOptions sendOption)
        {
            var netUpdate = new NetUpdate(message, sendOption);
            AddMessage(netUpdate);
        }

        protected void AddMessage(NetUpdate netUpdate)
        {
            UpdateEvents.Add(netUpdate);
            _hasUpdatedEvent = true;
        }

        protected void AddMessage(IConctract message, SendOptions sendOption, int messageID)
        {
            //if (currentSyncPositionEveryMS < syncPositionEveryMS) return;
            //currentSyncPositionEveryMS = 0;

            if (_updateEventsForObjectsMapper.ContainsKey(messageID))
            {
                this.UpdateEvents[_updateEventsForObjectsMapper[messageID]].Conctract = message;
                this.UpdateEvents[_updateEventsForObjectsMapper[messageID]].SendOption = sendOption;
            }
            else
            {
                AddMessage(message, sendOption);
                _updateEventsForObjectsMapper.Add(messageID, UpdateEvents.Count - 1);
            }
        }

        //protected void AddMessage(IConctract message, SendOptions sendOption, long objectID)
        //{
        //    if (_updateEventsForObjectsMapper.Any(x => x.Item1 == objectID))
        //    {
        //        this.UpdateEvents[_updateEventsForObjectsMapper.FirstOrDefault(x => x.Item1 == objectID).Item2] =
        //            new Tuple<IConctract, SendOptions>(message, sendOption);
        //    }
        //    else
        //    {
        //        AddMessage(message, sendOption);
        //        _updateEventsForObjectsMapper.Add(new Tuple<long, int>(objectID, UpdateEvents.Count - 1));
        //    }
        //}

        public IEnumerable<NetUpdate> GetUpdateEvents()
        {
            return UpdateEvents;
        }

        public bool HasUpdatedEvents()
        {
            return _hasUpdatedEvent;
        }

        public void ClearUpdateEvents()
        {
            UpdateEvents.Clear();
            _updateEventsForObjectsMapper.Clear();
            _hasUpdatedEvent = false;
        }

        public void AssignMoveToPosition(Vector3 moveTo)
        {
            this.moveToPosition = moveTo;
            UpdateWithMoveToPosition = true;
            AddMessage(new ClickToPosition(this.ObjectID, moveTo), SendOptions.ReliableUnordered, 4);
        }
    }
}
