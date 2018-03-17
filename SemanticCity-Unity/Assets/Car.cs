using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gen;

namespace Gen{
  using Edge = List<Vector2>;
  using Polygon = List<Vector2>;
  using Section = List<List<Vector2>>;


  // appearance and AI for moving things, e.g. cars, bikes, people.
  public class Car : MonoBehaviour {

    public List<City.Road> roads = new List<City.Road>();
    float elev = 0.0f;

    // (road index * node index)
    Vector2Int startpos;
    Vector2Int endpos;
    Vector2Int currpos;
    Vector2Int nextpos;

    // (visited nodes)
    List<Vector2Int> visited = new List<Vector2Int>();

    // counters for lerping along roads
    int ltm = 50;
    int lt = 0;

    // pause frame count
    public int pause = 0;

    // does this thing move on sidewalk?
    bool ispedes = false;

    // random node
    Vector2Int randompos(){
      int i = Random.Range (0, roads.Count);
      Edge e = roads [i].edge;
      int j = Random.Range (0, e.Count);
      return new Vector2Int (i, j);
    }

    // node to 3D location
    Vector3 getpos(Vector2Int pos){
      Vector2 p = roads [pos.x].edge [pos.y];
      if (float.IsNaN(p.x)){
        return new Vector3 (0, elev+0.1f, 0);
      }
      return new Vector3 (p.x, elev+0.1f, p.y);
    }

    // closest node on a different road
    Vector2Int closest(Vector2Int pos){
      float md = float.PositiveInfinity;
      Vector2Int mp = Vector2Int.zero;
      for (int i = 0; i < roads.Count; i++) {
        if (i != currpos.x) {
          for (int j = 0; j < roads [i].edge.Count; j++) {
            float d = Vector3.Distance (getpos(pos), getpos(new Vector2Int(i,j)));
            if (d < md) {
              md = d;
              mp = new Vector2Int (i, j);
            }
          }
        }
      }
      return mp;
    }

    // is node visited?
    bool isvisted(Vector2Int pos){
      for (int i = 0; i < visited.Count; i++){
        if (visited [i].x == pos.x && visited [i].y == pos.y) {
          return true;
        }
      }
      return false;
    }

    // figure out where to go next
    Vector2Int findnext(Vector2Int pos){
      Vector2Int targ1;
      Vector2Int targ2;
      if (pos.y + 1 >= roads [pos.x].edge.Count) {
        targ1 = closest (pos);
      } else {
        targ1 = new Vector2Int (pos.x, pos.y+1);
      }
      if (pos.y == 0) {
        targ2 = closest (pos);
      } else {
        targ2 = new Vector2Int (pos.x, pos.y-1);
      }
      if (isvisted(targ1)){
        return targ2;
      }
      if (isvisted(targ2)){
        return targ1;
      }

      if (Vector3.Distance (getpos (targ1), getpos(endpos)) 
        < Vector3.Distance (getpos (targ2), getpos(endpos))) {
        return targ1;
      } else {
        return targ2;
      }
    }

    // add 4 wheels to a vehicle
    static System.Action<GameObject,Vector3,Vector3, string> AddWheel = 
      delegate(GameObject self, Vector3 pos, Vector3 size, string matname) {
      GameObject b1 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b1.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName (matname);
      b1.transform.parent = self.transform;
      b1.transform.localScale = new Vector3 (size.x, size.y, size.z);
      b1.transform.localPosition = new Vector3 (pos.x, pos.y, pos.z);

      GameObject b2 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b2.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName (matname);
      b2.transform.parent = self.transform;
      b2.transform.localScale = new Vector3 (size.x, size.y, size.z);
      b2.transform.localPosition = new Vector3 (-pos.x, pos.y, pos.z);

      GameObject b3 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b3.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName (matname);
      b3.transform.parent = self.transform;
      b3.transform.localScale = new Vector3 (size.x, size.y, size.z);
      b3.transform.localPosition = new Vector3 (pos.x, pos.y, -pos.z);

      GameObject b4 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b4.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName (matname);
      b4.transform.parent = self.transform;
      b4.transform.localScale = new Vector3 (size.x, size.y, size.z);
      b4.transform.localPosition = new Vector3 (-pos.x, pos.y, -pos.z);
    };

    // draw self as a bus
    System.Action<GameObject> MkBus = delegate(GameObject self) {
      GameObject b = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("BUS");
      float h = Random.Range (1, 3);
      b.transform.parent = self.transform;
      b.transform.localScale = new Vector3 (0.5f, 0.6f * h, 1.5f);
      b.transform.localPosition = new Vector3 (0f, 0.3f * h+0.1f, 0f);
      AddWheel(self,new Vector3(0.2f,0.125f,0.5f),new Vector3 (0.15f, 0.25f, 0.25f), "BUS");

      b.transform.GetComponent<Collider>().isTrigger = true;

    };

    // draw self as a car
    System.Action<GameObject> MkCar = delegate(GameObject self){
      GameObject b0 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b0.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("CAR");
      b0.transform.parent = self.transform;
      b0.transform.localScale = new Vector3 (0.4f, 0.2f, 0.8f);
      b0.transform.localPosition = new Vector3 (0f, 0.15f, 0f);

      GameObject b1 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b1.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("CAR");
      b1.transform.parent = self.gameObject.transform;
      b1.transform.localScale = new Vector3 (0.35f, 0.2f, 0.3f);
      b1.transform.localPosition = new Vector3 (0, 0.3f, 0);

      AddWheel(self,new Vector3(0.15f, 0.1f, 0.25f),new Vector3 (0.15f, 0.2f, 0.2f), "CAR");

      b0.transform.GetComponent<Collider>().isTrigger = true;
      b1.transform.GetComponent<Collider>().isTrigger = true;
    };

    // draw self as a truck
    System.Action<GameObject> MkTruck = delegate(GameObject self){
      GameObject b0 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      float l = 1;
      b0.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("TRUCK");
      b0.transform.parent = self.transform;
      b0.transform.localScale = new Vector3 (0.5f, 0.6f, 0.3f);
      b0.transform.localPosition = new Vector3 (0f, 0.4f, 0.6f * l-0.1f);

      GameObject b1 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b1.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("TRUCK");
      b1.transform.parent = self.gameObject.transform;
      b1.transform.localScale = new Vector3 (0.5f, 0.4f, 1.2f * l);
      b1.transform.localPosition = new Vector3 (0, 0.3f, -0.1f);

      AddWheel(self,new Vector3(0.2f, 0.125f, 0.35f),new Vector3 (0.15f, 0.25f, 0.25f), "TRUCK");

      b0.transform.GetComponent<Collider>().isTrigger = true;
      b1.transform.GetComponent<Collider>().isTrigger = true;
    };

    // draw self as a motorcycle
    System.Action<GameObject> MkMotorBike = delegate(GameObject self){
      GameObject b0 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b0.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("MOTORCYCLE");
      b0.transform.parent = self.transform;
      b0.transform.localScale = new Vector3 (0.08f, 0.1f, 0.4f);
      b0.transform.localPosition = new Vector3 (0f, 0.15f, 0f);

      GameObject b1 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(1.2f,1.2f,1.2f)),"RIDER");
      b1.transform.parent = self.gameObject.transform;
      b1.transform.localPosition = new Vector3(0,0.36f,0.05f);

      GameObject b2 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(2f,3f,1.8f)),"RIDER");
      b2.transform.parent = self.gameObject.transform;
      b2.transform.localPosition = new Vector3(0,0.25f,-0.05f);
      b2.transform.localRotation = Quaternion.Euler(new Vector3(30,0,0));

      GameObject b3 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(1f,3f,1f)),"RIDER");
      b3.transform.parent = self.gameObject.transform;
      b3.transform.localPosition = new Vector3(0,0.25f,0.05f);
      b3.transform.localRotation = Quaternion.Euler(new Vector3(-30,0,0));

      GameObject b4 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b4.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("MOTORCYCLE");
      b4.transform.parent = self.transform;
      b4.transform.localScale = new Vector3 (0.1f, 0.25f, 0.25f);
      b4.transform.localPosition = new Vector3 (0f, 0.125f, 0.15f);

      GameObject b5 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b5.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("MOTORCYCLE");
      b5.transform.parent = self.transform;
      b5.transform.localScale = new Vector3 (0.1f, 0.25f, 0.25f);
      b5.transform.localPosition = new Vector3 (0f, 0.125f, -0.15f);

      GameObject b6 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b6.GetComponent<Renderer> ().enabled = false;
      b6.transform.parent = self.transform;
      b6.transform.localScale = new Vector3 (0.15f, 0.3f, 0.5f);
      b6.transform.localPosition = new Vector3 (0f, 0.15f, 0f);

      b6.transform.GetComponent<Collider>().isTrigger = true;

    };

    // draw self as a bicycle
    System.Action<GameObject> MkBike = delegate(GameObject self){
      GameObject b0 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b0.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("BICYCLE");
      b0.transform.parent = self.transform;
      b0.transform.localScale = new Vector3 (0.08f, 0.1f, 0.4f);
      b0.transform.localPosition = new Vector3 (0f, 0.15f, 0f);

      GameObject b1 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(1.2f,1.2f,1.2f)),"RIDER");
      b1.transform.parent = self.gameObject.transform;
      b1.transform.localPosition = new Vector3(0,0.36f,0.05f);

      GameObject b2 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(2f,3f,1.8f)),"RIDER");
      b2.transform.parent = self.gameObject.transform;
      b2.transform.localPosition = new Vector3(0,0.25f,-0.05f);
      b2.transform.localRotation = Quaternion.Euler(new Vector3(30,0,0));

      GameObject b3 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(1f,3f,1f)),"RIDER");
      b3.transform.parent = self.gameObject.transform;
      b3.transform.localPosition = new Vector3(0,0.25f,0.05f);
      b3.transform.localRotation = Quaternion.Euler(new Vector3(-30,0,0));


      GameObject b4 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b4.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("BICYCLE");
      b4.transform.parent = self.transform;
      b4.transform.localScale = new Vector3 (0.05f, 0.25f, 0.25f);
      b4.transform.localPosition = new Vector3 (0f, 0.125f, 0.15f);

      GameObject b5 = GameObject.CreatePrimitive (PrimitiveType.Sphere);
      b5.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("BICYCLE");
      b5.transform.parent = self.transform;
      b5.transform.localScale = new Vector3 (0.05f, 0.25f, 0.25f);
      b5.transform.localPosition = new Vector3 (0f, 0.125f, -0.15f);

      GameObject b6 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b6.GetComponent<Renderer> ().enabled = false;
      b6.transform.parent = self.transform;
      b6.transform.localScale = new Vector3 (0.15f, 0.3f, 0.5f);
      b6.transform.localPosition = new Vector3 (0f, 0.15f, 0f);

      b6.transform.GetComponent<Collider>().isTrigger = true;

      self.GetComponent<Car>().ispedes = true;
      self.GetComponent<Car>().ltm = 100;
    };

    // draw self as a person
    System.Action<GameObject> MkPerson = delegate(GameObject self){
      GameObject b0 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(1.2f,1.2f,1.2f)),"PERSON");
      b0.transform.parent = self.gameObject.transform;
      b0.transform.localPosition = new Vector3(0,0.35f,0);

      GameObject b1 = MeshMaker.ToObject(MeshMaker.Blob(new Vector3(1.5f,6f,1.5f)),"PERSON");
      b1.transform.parent = self.gameObject.transform;
      b1.transform.localPosition = new Vector3(0,0.05f,0);

      GameObject b2 = GameObject.CreatePrimitive (PrimitiveType.Cube);
      b2.GetComponent<Renderer> ().enabled = false;
      b2.transform.parent = self.gameObject.transform;
      b2.transform.localScale = new Vector3 (0.1f, 0.4f, 0.1f);
      b2.transform.localPosition = new Vector3 (0, 0.2f, 0);

      b2.transform.GetComponent<Collider>().isTrigger = true;

      self.GetComponent<Car>().ispedes = true;
      self.GetComponent<Car>().ltm = 300;
    };

    // traffic accident handling AI
    void OnTriggerEnter(Collider other)
    {
      if (other.transform.parent != null) {
        if (other.gameObject.transform.parent.name == "VEHICLE") {
          pause = Random.Range (0, 10);
          other.gameObject.transform.parent.gameObject.GetComponent<Car> ().pause = 0;
        }
      }
    }

  	// Use this for initialization
  	void Start () {
      lt = Random.Range (0, ltm);
      startpos = randompos ();
      endpos = randompos ();
      currpos = startpos + Vector2Int.zero;
      nextpos = findnext (currpos);
      visited.Add (currpos);
      transform.localPosition = getpos (currpos);

      // turn self into a random vehicle (weighted chance)
      var vehicles = new List<System.Action<GameObject>> ();
      vehicles.Add (MkBus);
      vehicles.Add (MkCar);
      vehicles.Add (MkCar);
      vehicles.Add (MkCar);
      vehicles.Add (MkTruck);
      vehicles.Add (MkBike);
      vehicles.Add (MkPerson);
      vehicles.Add (MkPerson);
      vehicles.Add (MkPerson);
      vehicles.Add (MkPerson);
      vehicles.Add (MkPerson);
      vehicles.Add (MkMotorBike);
      vehicles [Random.Range (0, vehicles.Count)] (gameObject);

      // name used for collision checking
      gameObject.name = "VEHICLE";

      Rigidbody rb = gameObject.AddComponent<Rigidbody> ();
      rb.isKinematic = true;


  	}
  	
  	// Update is called once per frame
  	void Update () {

      // movement AI
      if (pause == 0) {

        Vector3 p0 = getpos (currpos);
        Vector3 p1 = getpos (nextpos);

        Vector3 p = Vector3.Lerp (p0, p1, ((float)lt) / ltm);
        float ang = Mathf.Atan2 (p1.z - p0.z, p1.x - p0.x) + Mathf.PI * 0.5f;
        float rw = roads [currpos.x].width / 4;
        if (ispedes) {
          rw *= 3;
        }
        p += new Vector3 (Mathf.Cos (ang) * rw, 0, Mathf.Sin (ang) * rw);

        transform.localPosition = Vector3.Lerp (transform.localPosition, p, 0.1f);

        transform.localRotation = Quaternion.Lerp (
          transform.localRotation,
          Quaternion.LookRotation (getpos (nextpos) + new Vector3 (0.01f, 0, 0) - getpos (currpos)),
          0.1f
        );
        lt += 1;
        if (lt == ltm) {
          lt = 0;
          Vector2Int temp = nextpos + Vector2Int.zero;
          nextpos = findnext (nextpos);
          currpos = temp;
          visited.Add (temp);
        }

      } else {
        
        pause = Mathf.Max(pause-1,0);
      }
  	}
  }
}