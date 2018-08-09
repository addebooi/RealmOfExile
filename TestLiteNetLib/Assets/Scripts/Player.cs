using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CommunicationContract;
using LiteNetLib;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class Player : MonoBehaviour
{
    const int layerMask = 1 << 9;

    private Camera _camera;
    RaycastHit hit;
    private NetworkObject _netObject;
    public Animator animator;
    private Vector3 cameraVectorFromPlayer;
    private Quaternion cameraRotation;

    private Vector3 moveToPosition;
    private const float _baseTolerance = 0.02f;
    private float TOLERANCE = 0.01f;

    private float _speed;

    private float _health;

    public bool IsDead;
    public float speed
    {
        get { return _speed; }
        set
        {
            _speed = value;
            this.TOLERANCE = _baseTolerance * speed;
        }
    }

    private Quaternion externalRotationUpdate;
    private bool newExternalRotationUpdate;

    public HealthComponent healthComponent;
    // Use this for initialization
    void Start ()
    {
        _camera = GetComponentInChildren<Camera>();
        _netObject = this.GetComponent<NetworkObject>();
        animator = this.GetComponent<Animator>();
        healthComponent = this.GetComponent<HealthComponent>();
        healthComponent.RegisterOnDeath(OnDeath);
        moveToPosition = transform.position;
        cameraVectorFromPlayer = _camera.transform.position - transform.position;
        cameraRotation = _camera.transform.rotation;
        this.speed = 10;
    }

    public void OnDeath()
    {
        animator.SetBool("IsDead", healthComponent.IsDead());
    }
	
	// Update is called once per frame
	void Update ()
	{

        
	    if (healthComponent.IsDead()) return;

        bool isMoving = Vector3.Distance(transform.position, moveToPosition) > TOLERANCE;

        if (isMoving)
	    {
	        UpdateMoveToPosition(Time.deltaTime);
           animator.SetBool("IsRunning", true);
        }
        else
            animator.SetBool("IsRunning", false);

	    Vector3 relativePos = new Vector3(moveToPosition.x, 0, moveToPosition.z)
	                          - new Vector3(transform.position.x, 0, transform.position.z);


        if (relativePos != Vector3.zero && isMoving)
        {
            Quaternion rotation = Quaternion.LookRotation(relativePos);
            transform.rotation = rotation;
        }
	    else if (newExternalRotationUpdate)
	    {
	        newExternalRotationUpdate = false;
	        transform.rotation = externalRotationUpdate;
	    }



        if (!_netObject.getIsOwner()) return;
	    _camera.transform.position = transform.position + cameraVectorFromPlayer;
	    _camera.transform.rotation = cameraRotation;
        CheckForMovement();
	    
        Debug.DrawLine(transform.position, transform.position + cameraVectorFromPlayer);


	}

    void LateUpdate()
    {
        _camera.transform.rotation = cameraRotation;
    }

    private void UpdateMoveToPosition(float dt)
    {
        this.transform.position += (moveToPosition - transform.position).normalized * speed * dt;
    }

    private void CheckForMovement()
    {
        if(hit.point!= Vector3.zero)
            Debug.DrawRay(transform.position, (hit.point - transform.position).normalized * hit.distance, Color.yellow);

        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 100, layerMask))
            {
                moveToPosition = hit.point;
                _netObject.AddUpdateEvent(new ClickToPosition(_netObject.getObjectID(),
                    moveToPosition), SendOptions.ReliableOrdered, _netObject.getObjectID());
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 100, layerMask))
            {
                
                var fireballDirection = (hit.point - transform.position).normalized;
                _netObject.AddUpdateEvent(new ClientCastAbilityData(0,fireballDirection), 
                    SendOptions.ReliableUnordered);
                //_netObject.AddUpdateEvent(new ClientSpawnFireballData(fireballDirection),
                //    SendOptions.ReliableUnordered);
            }
        }
    }


    public void PlayerSpawn(SpawnPlayerData spawnPlayerData)
    {
        this.speed = spawnPlayerData.speed;
        var tempHealthComp = GetComponent<HealthComponent>();
        healthComponent.Health = spawnPlayerData.Health;
        healthComponent.MaxHealth = spawnPlayerData.MaxHealth;
        
        if (!_netObject.getIsOwner())
        {
            GetComponentInChildren<Camera>().enabled = false;
        }
    }

    public void CastSkill(Vector3 direction)
    {
        var dir = new Vector3(transform.position.x + direction.x, 0, transform.position.z + direction.z) -
                  new Vector3(transform.position.x, 0, transform.position.z);
        var rotation = Quaternion.LookRotation(direction);
        newExternalRotationUpdate = true;
        externalRotationUpdate = rotation;
        animator.SetBool("IsAttacking", true);
    }

    public void AttackEnded()
    {
        animator.SetBool("IsAttacking", false);
    }

    public void UpdateMoveToPosition(ClickToPosition moveToPosition)
    {
        this.moveToPosition = moveToPosition.position;
    }

    public void UpdatePositionInterpolateData(UpdatePositionInterpolateData updatePositionInterpolateData)
    {
        if (Vector3.Distance(this.transform.position, updatePositionInterpolateData.position) > 2f)
        {
            this.transform.position = updatePositionInterpolateData.position;
        }
    }

    public void VariableUpdate(UpdateVariableData updateVariableData)
    {
        //Got variable Update!
        UpdateVariableHander.SetUpdateVariableData(this, updateVariableData);
    }


}

public class UpdateVariableHander
{
    public static void SetUpdateVariableData(object objToChange, UpdateVariableData updateVariableData)
    {

        if (updateVariableData.variableName == "Health")
        {
            int k = 0;
        }
        var propertyInfo = objToChange.GetType().GetProperty(updateVariableData.variableName);
        if (propertyInfo != null)
        {
            var methods = propertyInfo.GetAccessors(true);

            foreach (var c in methods)
            {
                if(c.ReturnType != typeof(void)) continue;

                c.Invoke(objToChange, new object[] {GetUpdateVariableData(updateVariableData)});
                return;
            }
        }

        var fieldInfo = objToChange.GetType().GetField(updateVariableData.variableName);
        if (fieldInfo != null)
            fieldInfo.SetValue(objToChange, GetUpdateVariableData(updateVariableData));
    }

    public static object GetUpdateVariableData(UpdateVariableData updateVariableData)
    {
        switch (updateVariableData.variabeleDataType)
        {
            case VariableDataType.Bool:
                return updateVariableData.variableDataBool;
                break;
            case VariableDataType.Float:
                return updateVariableData.variableDataFloat;
                break;
            case VariableDataType.Long:
                return updateVariableData.variableDataLong;
                break;
            case VariableDataType.String:
                return updateVariableData.variableDataString;
                break;
            case VariableDataType.Vector2:
                return updateVariableData.variableDataVector2;
                break;
        }

        return null;
    }
}