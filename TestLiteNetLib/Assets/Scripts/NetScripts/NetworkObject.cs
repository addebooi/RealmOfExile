using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunicationContract;
using LiteNetLib;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{

    private List<Tuple<IConctract, SendOptions>> _updateEvents;
    private List<Tuple<long, int>> _updateEventsForObjectsMapper;
    private bool _hasUpdateEvent;

    public Vector2 HitboxSize;
    public Vector2 HitboxPosition;

    public LineRenderer LineRenderer;

    [SerializeField]
    private long _objectID;

    [SerializeField]
    private bool _isOwner;
    [SerializeField]
    private long _ownerID;


    // Use this for initialization
    void Start ()
    {
        _updateEvents = new List<Tuple<IConctract, SendOptions>>();
        _updateEventsForObjectsMapper = new List<Tuple<long, int>>();
        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.enabled = false;

    }

	
	// Update is called once per frame
	void Update () {
	    //if (HitboxSize != Vector2.zero)
	    //{
	    //    LineRenderer.enabled = true;

	    //    var topLeft = new Vector3(transform.position.x - HitboxSize.x, 0.5f, transform.position.z + HitboxSize.y);
	    //    var topRight = new Vector3(transform.position.x + HitboxSize.x, 0.5f, transform.position.z + HitboxSize.y);

	    //    var botLeft = new Vector3(transform.position.x - HitboxSize.x, 0.5f, transform.position.z - HitboxSize.y);
	    //    var botRight = new Vector3(transform.position.x + HitboxSize.x, 0.5f, transform.position.z - HitboxSize.y);

     //    // var topLeft = new Vector3(HitboxPosition.x - HitboxSize.x, 0.5f, HitboxPosition.y + HitboxSize.y);
     //    //var topRight = new Vector3(HitboxPosition.x + HitboxSize.x, 0.5f, HitboxPosition.y + HitboxSize.y);

     //       //var botLeft = new Vector3(HitboxPosition.x - HitboxSize.x, 0.5f, HitboxPosition.y - HitboxSize.y);
     //       //var botRight = new Vector3(HitboxPosition.x + HitboxSize.x, 0.5f, HitboxPosition.y - HitboxSize.y);
     //       LineRenderer.SetPositions(new Vector3[]
	    //    {
     //           botRight, botLeft, topLeft, topRight
	    //    });
     //   }
	}

    public void AddUpdateEvent(IConctract updateEvent, SendOptions sendOptions)
    {
        this._updateEvents.Add(new Tuple<IConctract, SendOptions>(updateEvent, sendOptions));
        _hasUpdateEvent = true;
    }

    public void AddUpdateEvent(IConctract updateEvent, SendOptions sendOptions, long objectID)
    {
        if (_updateEventsForObjectsMapper.Any(x => x.Item1 == objectID))
        {
            this._updateEvents[_updateEventsForObjectsMapper.FirstOrDefault(x => x.Item1 == objectID).Item2] =
                new Tuple<IConctract, SendOptions>(updateEvent, sendOptions);
        }
        else
        {
            AddUpdateEvent(updateEvent, sendOptions);
            _updateEventsForObjectsMapper.Add(new Tuple<long, int>(objectID, _updateEvents.Count - 1));
        }


    }

    //public void AddObjectUpdateevent(IConctract )

    public void ClearUpdateEvent()
    {
        this._updateEvents.Clear();
        this._updateEventsForObjectsMapper.Clear();
        _hasUpdateEvent = false;
    }

    public bool HasUpdate()
    {
        return _hasUpdateEvent;
    }

    public IEnumerable<Tuple<IConctract, SendOptions>> GetUpdateEvents()
    {
        return this._updateEvents;
    }

    public void SetObjectID(long objectID)
    {
        this._objectID = objectID;
    }

    public long getObjectID()
    {
        return this._objectID;
    }

    public void SetIsOwner(bool owner)
    {
        this._isOwner = owner;
    }

    public bool getIsOwner()
    {
        return this._isOwner;
    }

    public void SetOwnerID(long ownerID)
    {
        this._ownerID = ownerID;
    }

    public long getOwnerID()
    {
        return this._ownerID;
    }



    public void VariableUpdate(UpdateVariableData variableData)
    {
        UpdateVariableHander.SetUpdateVariableData(this, variableData);
    }
}
