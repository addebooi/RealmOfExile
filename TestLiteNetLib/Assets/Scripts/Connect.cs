using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Connect : MonoBehaviour
{

    public string IP = "localhost";

    public InputField input;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
	    input.text = "localhost";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ConnectToServer()
    {
        this.IP = input.text;
        SceneManager.LoadScene("SampleScene");
    }
}
