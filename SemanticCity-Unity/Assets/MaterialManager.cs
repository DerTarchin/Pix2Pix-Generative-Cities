using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

// manages materials conforming to cityscape's color coding
public static class MaterialManager {

  // generate a cube instance of each material
	public static void MaterialInstances(){
		IEnumerable<string> info = Directory.GetFiles("./Assets/Resources/LABELED_MATERIALS")
			.Select(Path.GetFileName);
		int counter = 0;
		foreach (string f in info) {
			if (f.EndsWith (".mat")) {
				string matname = f.Split ('.')[0];
				Material mat = (Material)Resources.Load("LABELED_MATERIALS/"+matname, typeof(Material));
				GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
				cube.name = "MAT_" + matname;
				cube.GetComponent<Renderer> ().material = mat;
				cube.transform.position = new Vector3 (counter,0,0);
				counter++;
			}
		}
	}

  // fetch material by name, eg. "BUILDING"
	public static Material GetMaterialByName(string s){
		Transform mp = GameObject.Find ("LABELED_MATERIALS").transform;
		return mp.Find("MAT_" + s).GetComponent<Renderer>().material;
	}
}