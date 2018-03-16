using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// split camera for stereo.
// enable/disable the object this script is attached to
// to toggle stereo/mono

public class SplitCam : MonoBehaviour {

	// Use this for initialization
	void Start () {
    Camera cam1 = transform.parent.GetComponent<Camera> ();
    cam1.rect = new Rect (0,0,0.5f,1);
    Camera cam2 = GetComponent<Camera> ();
    cam2.rect = new Rect (0.5f,0,1,1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
