using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// util & math for 2D shapes
namespace Shape{

  // edge is a list of 2D coordinates
  using Edge = List<Vector2>;

  // polygon is a list of 2D coordinates
  using Polygon = List<Vector2>;

  // section is a list of edges
  using Section = List<List<Vector2>>;

  // polygon tools
  public static class PolyTool{

    // remove redundent points
    public static Polygon Clean(Polygon poly){
      Polygon npoly = new Polygon ();
      for (int i = 0; i < poly.Count; i++) {
        Vector2 last = poly [(i - 1 + poly.Count) % poly.Count];
        if (Vector2.Distance (last, poly [i]) > 0.0001f) {
          npoly.Add (poly [i]);
        }
      }
      return npoly;
    }
 
    // quick triangulate for pie-like polygons. return flat list of indices
    public static List<int> QkTriangulate(Polygon poly){
      Polygon cpoly = poly;
      List<int> triangles = new List<int> ();
      for (int i = 0; i < cpoly.Count; i++) {
        int i0 = 0;
        int i1 = i;
        int i2 = (i + 1) % cpoly.Count;
        triangles.AddRange (new int[] { i0,i1,i2 });
      }
      return triangles;
    }

    // quick triangulate for tube-like polygons. return flat list of indices
    public static List<int> TbTriangulate(Polygon poly){
      Polygon cpoly = poly;
      int l = cpoly.Count;
      int m = l / 2;
      List<int> triangles = new List<int> ();
      for (int i = 0; i < m; i++) {
        triangles.AddRange (new int[] { i+1,l-1-i,i });
        triangles.AddRange (new int[] { i+1,l-1-i-1,l-1-i });
      }
      return triangles;
    }

    // re-center to centroid position
    public static Polygon Recenter(Polygon poly, Vector2 c){
      Vector2 tran = c - Centroid (poly);
      Polygon npoly = new Polygon ();
      for (int i = 0; i < poly.Count; i++) {
        npoly.Add (poly [i] + tran);
      }
      return npoly;
    }

    // area for simple polygons
    public static float Area(Polygon poly){
      float s = 0;
      for (int i = 0; i < poly.Count ; i++){
        float a = poly[i].x * poly[(i+1)%poly.Count].y;
        float b = poly[(i+1)%poly.Count].x * poly[i].y;
        s += ( a - b );
      }
      return Mathf.Abs(s * 0.5f);
    }

    // point containment test
    public static bool ContainsPt(Polygon poly, Vector2 pt){
      bool c = false;
      for (int i = 0; i < poly.Count; i++) {
        int j = (i - 1 + poly.Count) % poly.Count;
        if ((((poly[i].y <= pt.y) && (pt.y < poly[j].y)) ||
          ((poly[j].y <= pt.y) && (pt.y < poly[i].y))) &&
          (pt.x < (poly[j].x - poly[i].x) * (pt.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
          c = !c;
      }
      return c;
    }

    // calculate centroid
    public static Vector2 Centroid(Polygon poly){
      float cx = 0;
      float cy = 0;
      float A = Area (poly);
      if (A < 0.1f) {
        if (poly.Count > 0) {
          return poly [0];
        } else {
          return Vector2.zero;
        }
      }
      for (int i = 0; i < poly.Count ; i++){
        Vector2 v0 = poly [i];
        Vector2 v1 = poly [(i - 1 + poly.Count) % poly.Count];
        float a = (v0.x * v1.y - v1.x * v0.y);
        cx +=  (v0.x+v1.x)*a;
        cy +=  (v0.y+v1.y)*a;
      }
      return new Vector2 (cx / (6 * A), cy / (6 * A));
    }

  }

  // edge tools
  public static class EdgeTool{

    // estimated length: distance between start and end points
    public static float EstLen(Edge edge){
      return Vector2.Distance (edge [0], edge [edge.Count - 1]);
    }
    // make a curvy edge
    public static Edge MakeCurve(Vector2 p1, Vector2 p2){
      float ang = Mathf.Atan2 (p2.y-p1.y, p2.x-p1.x);
      float d = Vector2.Distance (p1, p2);
      float m = Mathf.Pow(d,2)*0.001f;
      int reso = (int)Mathf.Floor(d/5);
      float rseed = Random.value;
      List<Vector2> nedge = new List<Vector2> ();
      for (int i = 0; i <= reso; i += 1) {
        float x = ((float)i) / ((float)reso);
        Vector2 np = Vector2.Lerp (p1, p2, x);
        float y = (Mathf.PerlinNoise (x*1.5f,rseed) - 1)*2 * Mathf.Sin(x*Mathf.PI);
        Vector2 mp = new Vector2 (
          np.x + Mathf.Cos(ang+Mathf.PI/2)*y*m,
          np.y + Mathf.Sin(ang+Mathf.PI/2)*y*m
        );
        nedge.Add (mp);
      }
      return nedge;
    }
    // quick render using unity line-renderers (for testing)
    public static GameObject QkRender(Edge edge,float width,Color color = new Color()){
      GameObject g = new GameObject ();
      LineRenderer lr = g.AddComponent<LineRenderer> ();
      lr.useWorldSpace = false;
      lr.startWidth = width;
      lr.endWidth = width;
      lr.positionCount = edge.Count;

      for (int i = 0; i < edge.Count; i ++) {
        lr.SetPosition (i, new Vector3 (edge[i].x, 0, edge[i].y));
      }
      g.GetComponent<Renderer> ().material.color = color;
      return g;
    }

    // fatten edge into a tube-like section
    public static Section Tubify(Edge pts, float wid){
      Edge edge0 = new Edge ();
      Edge edge1 = new Edge ();

      for (int i = 1; i < pts.Count-1; i++){
        float w = wid;
        float a1 = Mathf.Atan2(pts[i].y-pts[i-1].y,pts[i].x-pts[i-1].x);
        float a2 = Mathf.Atan2(pts[i].y-pts[i+1].y,pts[i].x-pts[i+1].x);
        float a = (a1+a2)/2;
        if (a < a2){a += Mathf.PI;}
        edge0.Add(new Vector2(pts[i].x+w*Mathf.Cos(a),(pts[i].y+w*Mathf.Sin(a))));
        edge1.Add(new Vector2(pts[i].x-w*Mathf.Cos(a),(pts[i].y-w*Mathf.Sin(a))));
      }
      if (pts.Count >= 2) {
        int l = pts.Count - 1;
        float a3 = Mathf.Atan2 (pts [1].y - pts [0].y, pts [1].x - pts [0].x) - Mathf.PI / 2;
        float a4 = Mathf.Atan2 (pts [l].y - pts [l - 1].y, pts [l].x - pts [l - 1].x) - Mathf.PI / 2;
        float w0 = wid;
        float w1 = wid;
        edge0.Insert (0, new Vector2 (pts [0].x + w0 * Mathf.Cos (a3), (pts [0].y + w0 * Mathf.Sin (a3))));
        edge1.Insert (0, new Vector2 (pts [0].x - w0 * Mathf.Cos (a3), (pts [0].y - w0 * Mathf.Sin (a3))));
        edge0.Add (new Vector2 (pts [l] [0] + w1 * Mathf.Cos (a4), (pts [l] [1] + w1 * Mathf.Sin (a4))));
        edge1.Add (new Vector2 (pts [l] [0] - w1 * Mathf.Cos (a4), (pts [l] [1] - w1 * Mathf.Sin (a4))));
      }
      edge0.Reverse ();
      return new Section (new Edge[]{ edge0, edge1 });
      
    }

  }


  // section tools
  public static class SectTool{

    // cast to polygon
    public static Polygon ToPoly(Section section){
      return (from edge in section
        from pt in edge
        select pt).ToList();
    }

    // make a rectangular section
    public static Section Box(float x, float y, float w, float h){
      Edge e0 = new List<Vector2> (new Vector2[] { new Vector2 (x, y), new Vector2 (x, y + h) });
      Edge e1 = new List<Vector2> (new Vector2[] { new Vector2 (x, y + h), new Vector2 (x + w, y + h) });
      Edge e2 = new List<Vector2> (new Vector2[] { new Vector2 (x + w, y + h), new Vector2 (x + w, y) });
      Edge e3 = new List<Vector2> (new Vector2[] { new Vector2 (x + w, y), new Vector2 (x, y) });
      return new Section (new Edge[]{ e0, e1, e2, e3 });

    }

    // get bounding box
    public static float[] Bound(Section section){
      float xmin = float.PositiveInfinity;
      float xmax = float.NegativeInfinity;
      float ymin = float.PositiveInfinity;
      float ymax = float.NegativeInfinity;
      for (int i = 0; i < section.Count; i++) {
        for (int j = 0; j < section[i].Count; j++){
          if (section[i][j].x < xmin){xmin = section [i][j].x;}
          if (section[i][j].x > xmax){xmax = section [i][j].x;}
          if (section[i][j].y < ymin){ymin = section [i][j].y;}
          if (section[i][j].y > ymax){ymax = section [i][j].y;}
        }    
      }
      return new float[] { xmin, ymin, xmax, ymax };
    }

    // rotate around center by angle
    public static Section Rotate(Section section, Vector2 center, float angle){
      Section nsection = new Section ();
      for (int i = 0; i < section.Count; i++) {
        Edge nedge = new Edge ();
        for (int j = 0; j < section[i].Count; j++){
          Vector2 pt = section [i] [j];
          float ang = Mathf.Atan2 (pt.y - center.y, pt.x - center.x);
          float dist = Vector2.Distance (center, pt);
          ang += angle;
          Vector2 npt = new Vector2 (center.x + dist * Mathf.Cos (ang),
            center.y + dist * Mathf.Sin (ang));
          nedge.Add (npt);
        }  
        nsection.Add (nedge);
      }
      return nsection;
    }

    // quick estimate area: area of the bounding box
    public static float QkEstArea(Section section){
      float[] bd = Bound (section);
      return (bd [2] - bd [0]) * (bd [3] - bd [1])/2f;
    }

    // estimate area using polygon area algorithm
    public static float EstArea(Section section){
      return PolyTool.Area (ToPoly(section));
    }

    // quick render using unity's line-renderer (for testing)
    public static GameObject QkRender(Section section,Color color = new Color()){
      GameObject g = new GameObject ();
      LineRenderer lr = g.AddComponent<LineRenderer> ();
      lr.useWorldSpace = false;
      lr.startWidth = 2f;
      lr.endWidth = 2f;
      lr.positionCount = 0;
      if (section.Count > 0) {
        for (var i = 0; i < section.Count; i++) {
          for (var j = 0; j < section [i].Count; j++) {
            lr.positionCount += 1;
            lr.SetPosition (lr.positionCount-1, new Vector3 (section [i] [j].x, 0, section [i] [j].y));
          }
        }
      }
      g.GetComponent<Renderer> ().material = MaterialManager.GetMaterialByName ("ROAD");//.color = color;// Random.ColorHSV ();
      return g;
    }

    // remove empty/meaningless edges
    public static void Clean(Section section){
      for (int i = section.Count-1; i >= 0; i--) {
        if (section [i].Count < 2) {
          section.RemoveAt (i);
        }
      }
    }

    // shrink toward center by a factor
    public static Section Shrink(Section section,float p){
      Section nsection = new Section ();
      Vector2 c = PolyTool.Centroid (ToPoly (section));
      //Debug.Log (c);
      for (int i = 0; i < section.Count; i++) {
        Edge nedge = new Edge ();
        for (int j = 0; j < section[i].Count; j++){

          Vector2 pt = section [i] [j];
          Vector2 npt = Vector2.Lerp (pt, c, p);
          nedge.Add (npt);
        }  
        nsection.Add (nedge);
      }
      return nsection;
    }

    // shrink toward center by fixed distance
    public static Section ShrinkFixed(Section section,float d){
      Section nsection = new Section ();
      Vector2 c = PolyTool.Centroid (ToPoly (section));
      //Debug.Log (c);
      for (int i = 0; i < section.Count; i++) {
        Edge nedge = new Edge ();
        for (int j = 0; j < section[i].Count; j++){

          Vector2 pt = section [i] [j];
          float dist = Vector2.Distance (pt, c);
          float p = d / dist;
          Vector2 npt = Vector2.Lerp (pt, c, p);
          nedge.Add (npt);
        }  
        nsection.Add (nedge);
      }
      return nsection;
    }

    // split a list of sections into smaller sections
    public static List<Section> SplitAll(List<Section> sections, out List<Edge> edges){
      edges = new List<Edge> ();
      List<Section> nsections = new List<Section> ();
      for (int i = 0; i < sections.Count; i++) {

        int s1 = 0;
        int s2 = 0;
        float maxlen1 = 0;
        float maxlen2 = 0;
        float minlen = float.PositiveInfinity;

        for (int j = 0; j < sections[i].Count; j++){
          float l = EdgeTool.EstLen (sections [i] [j]);
          if (l > maxlen1) {
            s1 = j;
            maxlen1 = l;
          } else if (l > maxlen2) {
            s2 = j;
            maxlen2 = l;
          }
          if (l < minlen) {
            minlen = l;
          }
        }

        if (EstArea(sections[i]) < 100 || minlen < 5) {
          nsections.Add (sections [i]);
          continue;
        }

        if (s2 < s1) {
          int temp = s2;
          s2 = s1;
          s1 = temp;
        }

        Edge e1 = sections [i] [s1];
        Edge e2 = sections [i] [s2];
        int mid1 = (int)Mathf.Floor (e1.Count / 2);
        int mid2 = (int)Mathf.Floor (e2.Count / 2);

        Vector2 p1 = Vector2.Lerp (e1[mid1], e1[(mid1+1)%e1.Count], 0.5f);
        Vector2 p2 = Vector2.Lerp (e2[mid2], e2[(mid2+1)%e2.Count], 0.5f);

        Edge e11 = e1.GetRange (0, mid1);
        Edge e12 = e1.GetRange (mid1+1, e1.Count-mid1-1);
        Edge e21 = e2.GetRange (0, mid2);
        Edge e22 = e2.GetRange (mid2+1, e2.Count-mid2-1);

        e11.Add (p1);
        e12.Insert (0, p1);
        e21.Add (p2);
        e22.Insert (0, p2);

        //Edge en1 = new List<Vector2>(new Vector2[]{p1,p2});
        Edge en1 = EdgeTool.MakeCurve(p1,p2);
        edges.Add (en1);
        Edge en2 = new List<Vector2> ();
        en2.AddRange (en1);
        en2.Reverse ();

        int len = sections [i].Count;

        Section left = new Section();

        left.Add (en2);
        left.Add (e12);
        left.AddRange (sections [i].GetRange ((s1+1)%len, s2-s1-1));
        left.Add (e21);

        Section right = new Section();
        right.Add (en1);
        right.Add (e22);
        right.AddRange(sections [i].GetRange ((s2+1)%len, len-(s2+1)));
        right.AddRange(sections [i].GetRange (0, s1));
        right.Add (e11);

        Clean (left);
        Clean (right);

        nsections.Add (left);
        nsections.Add (right);
      }
      return nsections;

    }

    // return a list of small rectangular sections bounded by given section
    public static List<Section> GridSection(Section section){
      float angr = Mathf.PI * 2 * Random.value;
      Section nsection = Rotate (section, new Vector2 (0,0), angr);
      nsection = ShrinkFixed (nsection, 3f);
      Polygon poly = ToPoly (nsection);

      float[] bd = Bound (nsection);
      List<Section> units = new List<Section>();
      List<List<int>> mtx = new List<List<int>> ();

      float uw = 2;
      float mtxsize = 0;
      for (float y = bd [1]; y < bd [3]; y+=uw) {
        List<int> row = new List<int> ();
        for (float x = bd [0]; x < bd [2]; x+=uw) {
          if (PolyTool.ContainsPt (poly, new Vector2 (x, y)) &&
              PolyTool.ContainsPt (poly, new Vector2 (x + uw, y + uw)) &&
              PolyTool.ContainsPt (poly, new Vector2 (x, y + uw)) &&
              PolyTool.ContainsPt (poly, new Vector2 (x + uw, y))) {
            row.Add (0);
          } else {
            row.Add (-1);
          }
          mtxsize += 1;
        }
        mtx.Add (row);
      }

      List<int[]> bdims = new List<int[]> ();
      for (int i = 0; i < mtxsize*3; i++) {
        int se = (int)Mathf.Floor(0.5f*Mathf.Min((bd[2]-bd[0])/uw,(bd[3]-bd[1])/uw));
        int w = Random.Range (1, se);
        int h = Random.Range (Mathf.Max(w-2,1), Mathf.Min(w+2,se));
        int x0 = Random.Range (0, mtx [0].Count-1);
        int y0 = Random.Range (0, mtx.Count - 1);
        bool islegal = true;
        for (int x = x0; x < x0 + w; x++) {
          for (int y = y0; y < y0 + h; y++) {
            try{
              if (mtx[y][x] != 0){
                islegal = false;
                break;
              }
            }catch (System.ArgumentOutOfRangeException e){
              System.Action<System.ArgumentOutOfRangeException> f = delegate(System.ArgumentOutOfRangeException e1) {
              };
              f(e);
              islegal = false;
              break;
            }
          }
        }
        if (islegal) {
          for (int x = x0; x < x0 + w; x++) {
            for (int y = y0; y < y0 + h; y++) {
              mtx [y] [x] = 1;
            }
          }
          bdims.Add (new int[]{ x0, y0, w, h });
        }

      }

      for (int i = 0; i < bdims.Count; i++) {
        float x = bd[0]+bdims [i] [0] * uw;
        float y = bd[1]+bdims [i] [1] * uw;
        float w = bdims [i] [2] * uw;
        float h = bdims [i] [3] * uw;

        Section nu = Box(x,y,w,h);
        Section nur = Rotate (nu, new Vector2 (0, 0), -angr);
        units.Add (Shrink(nur,0.1f));

      }

      return units;

    }



  }


}

