using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// run everything
public class Game : MonoBehaviour {

	void Start () {
    
    Gen.City.Generate ().transform.localScale *= 4;

	}

	// Update is called once per frame
	void Update () {



	}
}
