using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Shape;

namespace Gen{
  using Edge = List<Vector2>;
  using Polygon = List<Vector2>;
  using Section = List<List<Vector2>>;

  // procedural mesh generator
  public static class MeshMaker{

    // which triangulate algorithm to use
    public static System.Func<Polygon,List<int>> Triangulate;
    // terrian altitude query at given point (experimental)
    public static System.Func<Vector2,float> TerrainAt = delegate (Vector2 pos){
      return 0;
    };

    // pack mesh + material into new object
    public static GameObject ToObject(Mesh mesh, string matname){
      GameObject g = new GameObject();
      g.AddComponent<MeshFilter>().mesh = mesh;
      g.AddComponent<MeshRenderer> ().material = MaterialManager.GetMaterialByName(matname);
      return g;
    }

    // extrude section (2D) into a 3D mesh
    public static Mesh Extrude(Section section, float h){
      Vector3[] vtx;
      int[] tri;
      Mesh mesh = new Mesh();

      Polygon poly = PolyTool.Clean(SectTool.ToPoly (section));
      //center = PolyTool.Centroid (poly);
      poly = PolyTool.Recenter (poly, new Vector2 (0, 0));

      List<Vector3> ptbottom = poly.Select (v => new Vector3(v.x,TerrainAt(v),v.y)).ToList();
      List<Vector3> pttop = poly.Select (v => new Vector3(v.x,TerrainAt(v)+h,v.y)).ToList();

      List<int> idxbottom = Triangulate (poly);
      List<int> idxtop = idxbottom.Select(x => x+poly.Count).ToList();
      idxbottom.Reverse ();

      List<int> idxside = new List<int> ();
      for (int i = 0; i < poly.Count; i++) {
        idxside.Add (i);
        idxside.Add ((i+1)%poly.Count);
        idxside.Add (i + poly.Count);

        idxside.Add ((i + 1) % poly.Count);
        idxside.Add ((i + 1) % poly.Count + poly.Count);
        idxside.Add (i + poly.Count);
      }
      List<Vector3> allpts = new List<Vector3> ();
      allpts.AddRange (ptbottom);
      allpts.AddRange (pttop);

      List<int> alltris = new List<int> ();
      alltris.AddRange (idxbottom);
      alltris.AddRange (idxtop);
      alltris.AddRange (idxside);

      vtx = allpts.ToArray ();
      tri = alltris.ToArray ();

      mesh.vertices = vtx;
      mesh.triangles = tri;

      return mesh;
    }

    // make a random sphere-like blob thing
    public static Mesh Blob(Vector3 size){
      Vector3[] vtx;
      int[] tri;
      Mesh mesh = new Mesh();
      float seed = Random.value;
      int reso = 10;
      float rough = 2;
      List<Vector3> allpts = new List<Vector3> ();
      List<int> alltris = new List<int> ();
      for (int i = 0; i < reso; i++) {
        float p = ((float)i) / (reso-1);
        float y = size.y * p -size.y/2;
        for (int j = 0; j < reso; j++) {
          float a = j * 2 * Mathf.PI / (float)reso;
          float rx = size.x / 2;
          float rz = size.z / 2;
          float r = (rx * rz) / 
            Mathf.Sqrt (Mathf.Pow (rz * Mathf.Cos (a), 2) + Mathf.Pow (rx * Mathf.Sin (a), 2));
          float m = Mathf.Sqrt (1 - Mathf.Pow (2 * p - 1, 2));
          float x = Mathf.Cos (a) * r * m;
          float z = Mathf.Sin (a) * r * m;

          Vector3 pt = new Vector3 (x, y, z);
          pt *= Mathf.PerlinNoise (seed+(pt.x/size.x+0.5f)*rough, seed+(pt.z/size.z+0.5f)*rough);
          pt *= Mathf.PerlinNoise ((pt.y/size.y+0.5f)*rough, seed);
          allpts.Add (pt);
        }
      }
      for (int i = 1; i < reso; i++) {
        for (int j = 0; j < reso; j++){
          alltris.Add (reso * (i - 1) + j);
          alltris.Add (reso * i + (j - 1 + reso) % reso);
          alltris.Add (reso * i + j);

          alltris.Add (reso * (i - 1) + (j - 1 + reso) % reso);
          alltris.Add (reso * i + (j - 1 + reso) % reso);
          alltris.Add (reso * (i - 1) + j);
        }
      }
      vtx = allpts.ToArray ();
      tri = alltris.ToArray ();

      mesh.vertices = vtx;
      mesh.triangles = tri;

      return mesh;
    }

  }
}


