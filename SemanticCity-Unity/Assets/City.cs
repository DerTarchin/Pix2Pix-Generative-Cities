using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Shape;


namespace Gen{
  using Edge = List<Vector2>;
  using Polygon = List<Vector2>;
  using Section = List<List<Vector2>>;

  // city generator
  public static class City{

    // data structure for roads
    public class Road{
      public Edge edge;
      public float width;
      public Road(Edge edge, float width){
        this.edge = edge;
        this.width = width;
      }
    }

    // catch nans & friends
    public static bool BadVal(float f){
      if (float.IsNaN (f)) {return true;}
      if (float.IsNegativeInfinity (f)) {return true;}
      if (float.IsPositiveInfinity (f)) {return true;}
      if (float.IsInfinity (f)) {return true;}
      return false;
    }

    // generate city instance
    public static GameObject Generate(){
      // output gameobject
      GameObject gout = new GameObject ();
      gout.name = "CITY";

      // block plans
      List<Section> sections = new List<Section>();
      // building plans
      List<Section> units = new List<Section>();
      // roads
      List<Road> roads = new List<Road> ();

      // city size
      float tsize = 200;

      Section bedrock = SectTool.Box(0,0,tsize,tsize);
      sections.Add(bedrock);

      // drawing functions

      System.Action<Section,float,string> MkExtrudeSect = 
      delegate(Section s, float h, string matname) {
        Vector2 c = PolyTool.Centroid (SectTool.ToPoly (s));
        Mesh m = MeshMaker.Extrude (s, h);
        GameObject g = MeshMaker.ToObject (m, matname);

        if (!BadVal(c.x) ){ 
          g.transform.position = new Vector3 (c.x, 0, c.y);
        }
        g.name = matname;
        g.transform.parent = gout.transform;
      };

      System.Action<Vector3,Vector3,Quaternion,string> MkCube
      = 
        delegate(Vector3 pos, Vector3 size, Quaternion rot, string matname) {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.transform.localPosition = pos;
        g.transform.localScale = size;
        g.transform.localRotation = rot;
        g.GetComponent<Renderer>().material = MaterialManager.GetMaterialByName(matname);
        g.name = matname;
        g.transform.parent = gout.transform;
      };

      System.Action<Vector3,Vector3,Quaternion,string> MkBlob = 
        delegate(Vector3 pos, Vector3 size, Quaternion rot, string matname) {
        GameObject g = MeshMaker.ToObject(MeshMaker.Blob(size),matname);
        g.transform.localPosition = pos;
        g.transform.localScale = size;
        g.transform.localRotation = rot;
        g.GetComponent<Renderer>().material = MaterialManager.GetMaterialByName(matname);
        g.name = matname;
        g.transform.parent = gout.transform;
      };

      System.Action<Vector2> MkTree = 
        delegate(Vector2 pos) {
        float w = 0.07f * Random.Range(0.8f,1.5f);
        float h = Random.Range(0.8f,1.5f);
        MkCube (new Vector3 (pos.x, h / 2, pos.y), 
          new Vector3 (w, h, w), 
          Quaternion.Euler(new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f)
          )),
          "VEGETATION");
        for (int i = 0; i < 5; i++){
          float r= Random.Range(1f,2f);
          MkBlob (new Vector3 (pos.x, h, pos.y) 
                + new Vector3(
              Random.Range(-0.5f,0.5f),
              Random.Range(-0.2f,0.8f),
              Random.Range(-0.5f,0.5f)), 
            new Vector3(
              r,r,r
            ),
            Random.rotation,
            "VEGETATION");
        }
      };
      System.Action<Vector2> MkStreetLight = 
        delegate(Vector2 pos) {
          float w = 0.05f;
          float h = 1.2f;
          MkCube (new Vector3 (pos.x, h / 2, pos.y), 
            new Vector3 (w, h, w), 
            Quaternion.identity,
            "POLE");
          MkCube (new Vector3 (pos.x, h, pos.y), 
            new Vector3 (0.1f, 0.15f, 0.1f), 
            Quaternion.identity,
            "STATIC");
        };
      System.Action<Vector2> MkSign = 
        delegate(Vector2 pos) {
        float w = 0.05f;
        float h = 1f;
        MkCube (new Vector3 (pos.x, h / 2, pos.y), 
          new Vector3 (w, h, w), 
          Quaternion.identity,
          "POLE");
        for (int i = 0; i < Random.Range(1,4); i++){
          MkCube (new Vector3 (pos.x, h - i * 0.3f, pos.y+0.03f), 
            new Vector3 (0.24f, 0.24f, 0.02f), 
            Quaternion.identity,
            "TRAFFIC_SIGN");
        }
      };
      System.Action<Vector2> MkTrafficLight = 
        delegate(Vector2 pos) {
        float w = 0.05f;
        float h = 1f;
        MkCube (new Vector3 (pos.x, h / 2, pos.y), 
          new Vector3 (w, h, w), 
          Quaternion.identity,
          "POLE");

        MkCube (new Vector3 (pos.x, h, pos.y), 
          new Vector3 (0.1f, 0.3f, 0.08f), 
          Quaternion.identity,
          "TRAFFIC_LIGHT");
        
      };
      System.Action<Vector2> MkTrafficLightBig = 
        delegate(Vector2 pos) {
        float w = 0.05f;
        float h = 2f;
        float l = Random.Range(1,4);
        float r = Random.Range(0f,Mathf.PI*2);
        Quaternion R = Quaternion.Euler(new Vector3(0,90-r*180/Mathf.PI,0));
        MkCube (new Vector3 (pos.x, h / 2, pos.y), 
          new Vector3 (w, h, w), 
          Quaternion.identity,
          "POLE");
        MkCube (new Vector3 (pos.x + (l/2) * Mathf.Cos(r), h , pos.y + (l/2) * Mathf.Sin(r)), 
          new Vector3 (w, w, l), 
          R,
          "POLE");

        for (float i = 0; i < l; i+=0.5f){
          if (Random.value < 0.6f || i == 0){
            MkCube (new Vector3 (pos.x + (l - i) * Mathf.Cos(r), h, pos.y + (l - i) * Mathf.Sin(r)), 
              new Vector3 (0.1f, 0.3f, 0.08f), 
              R,
              "TRAFFIC_LIGHT");
          }else{
            MkCube (new Vector3 (pos.x + (l - i) * Mathf.Cos(r), h, pos.y + (l - i) * Mathf.Sin(r)) 
              + R * new Vector3(0.03f,0,0f), 
              new Vector3 (0.02f, 0.24f, 0.24f), 
              R,
              "TRAFFIC_SIGN");
          }
        }
        for (int i = 0; i < Random.Range(1,3); i++){
          MkCube (new Vector3 (pos.x, h/2 - i * 0.3f, pos.y+0.03f), 
            new Vector3 (0.24f, 0.24f, 0.02f), 
            Quaternion.identity,
            "TRAFFIC_SIGN");
        }

      };

      MeshMaker.Triangulate = PolyTool.QkTriangulate;
      MkExtrudeSect (bedrock, 0.05f, "GROUND");
        
      for (int i = 0; i < 10; i++) {
        List<Edge> edges;
        sections = SectTool.SplitAll (sections,out edges);

        for (int j = 0; j < edges.Count; j++) {
          roads.Add(new Road(edges[j],6-(float)i/2));
        }
      }

      // generate structures along roads
      for (int i = 0; i < roads.Count; i++) {
        MeshMaker.Triangulate = PolyTool.TbTriangulate;
        Section s = EdgeTool.Tubify(roads[i].edge,roads[i].width/2);

        MkExtrudeSect (s, 0.07f, "ROAD");

        Edge e0 = s[0];
        Edge e1 = s[1];

        Section s0 = EdgeTool.Tubify(e0,roads[i].width/2);
        Section s1 = EdgeTool.Tubify(e1,roads[i].width/2);
        MkExtrudeSect (s0, 0.06f, "SIDEWALK");
        MkExtrudeSect (s1, 0.06f, "SIDEWALK");

        System.Func<Vector2,bool> isCross = delegate(Vector2 pt) {
          for (int n = 0; n < roads.Count; n++) {
            if (roads [n].edge.Count >= 1) {
              Edge e = roads [n].edge;
              if (Vector2.Distance (pt, e [0]) < 3 ||
                  Vector2.Distance (pt, e [e.Count - 1]) < 3) {
                return true;
              }
            }
          }
          return false;
        };

        for (int j = 2; j < Mathf.Max (0, Mathf.Min (e0.Count, e1.Count) - 2); j++) {
          int reso = 24;
          for (int k = 0; k < reso; k++) {
            Vector2 p0 = Vector2.Lerp (e0 [j], e0 [j + 1], ((float)k)/reso);
            Vector2 p1 = Vector2.Lerp (e1 [j], e1 [j + 1], ((float)k)/reso);

            bool ic0 = isCross (p0);
            bool ic1 = isCross (p1);

            if (k % 8 == 0) {
              if (!ic0) {
                MkStreetLight (p0);
              } else if (Random.value < 0.3f){
                if (Random.value < 0.5f) {
                  MkTrafficLightBig (p0);
                } else {
                  MkTrafficLight (p0);
                }
              }
              if (!ic1) {
                MkStreetLight (p1);
              } else if (Random.value < 0.3f){
                if (Random.value < 0.5f) {
                  MkTrafficLightBig (p1);
                } else {
                  MkTrafficLight (p1);
                }
              }
            } else if (k % 8 == 4) {
              if (!ic0) {
                MkTree (p0);
              }
              if (!ic1) {
                MkTree (p1);
              }
            } else if (Random.value < 0.05f) {
              if (!ic0) {
                MkSign (p0);
              }
              if (!ic1) {
                MkSign (p1);
              }

            }
          }
        }
       
      }

      for (int i = 0; i < sections.Count; i++) {
        units.AddRange (SectTool.GridSection (sections [i]));
      }
        
      for (int i = 0; i < units.Count; i++) {

        Vector2 c = PolyTool.Centroid (SectTool.ToPoly (units [i]));

        float d2c = Vector2.Distance (c, new Vector2 (tsize / 2, tsize / 2));

        float a = SectTool.EstArea (units [i]);

        float h = Mathf.PerlinNoise (c.x*0.1f, c.y*0.1f)*200/Mathf.Pow(d2c,0.5f)/Mathf.Pow(a,0.5f) + Random.Range(-5.0f,5.0f);
        MeshMaker.Triangulate = PolyTool.QkTriangulate;
        MkExtrudeSect (units[i],Mathf.Max(h,1f),"BUILDING");
      }
      for (int i = 0; i < 4000; i++) {
        GameObject car = new GameObject();
        car.transform.parent = gout.transform;
        car.AddComponent<Car> ();
        car.GetComponent<Car> ().roads = roads;
      }
      return gout;
    }
  }
}

