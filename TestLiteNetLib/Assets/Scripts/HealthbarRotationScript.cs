using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthbarRotationScript : MonoBehaviour
{
    public Vector3 lookAtPosVecotor;
	// Use this for initialization
	void Awake ()
	{
	    var cameras = GameObject.FindGameObjectsWithTag("Camera");

	    if (cameras != null && cameras.Any())
	    {
	        foreach (var cam in cameras)
	        {
	            var camObj = cam.GetComponent<Camera>();
	            if (camObj != null && camObj.enabled)
	            {
	                lookAtPosVecotor = camObj.transform.parent.position - camObj.transform.position;
	                break;
	            }
	        }
	    }
	}
	
	// Update is called once per frame
	void Update ()
	{
	    this.transform.LookAt(transform.position + lookAtPosVecotor);
	}
}
