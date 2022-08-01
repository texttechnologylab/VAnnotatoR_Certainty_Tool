using MathHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VectorComparers;

public class MeshSurface : InteractiveObject
{

    public Dictionary<Vector3, float> VertexAngleMap { get; private set; }
    public Vector3[] Vertices { get; private set; }
    public Mesh Mesh { get; private set; }
    public Vector3 Center { get; private set; }
    public float SurfaceRadius { get; private set; }
    public GameObject Surface { get; private set; }
    private MeshFilter Filter;
    private MeshCollider Collider;
    private LineRenderer Lines;
    private LineRenderer Arrow;
    private GameObject Cone;
    public enum SurfaceType { Triangular, Rectangular };
    public SurfaceType SurfaceShape;
    public SurfaceToolInterface._DrawType DrawType;
    public Vector3 Normal;

    private void InternInit(Vector3 normal, Vector3[] vertices)
    {
        Grabable = true;
        DestroyOnObjectRemover = true;
        DrawType = SurfaceToolInterface._DrawType.Rectangular;

        Vertices = vertices;
        Normal = normal;
        List<Vector3> LineList = new List<Vector3>(vertices);
        LineList.Add(LineList[0]);

        Lines = GetComponent<LineRenderer>();
        if (Lines == null) Lines = gameObject.AddComponent<LineRenderer>();
        Lines.positionCount = LineList.Count;
        Lines.SetPositions(LineList.ToArray());
        Lines.material = (Material)(Instantiate(Resources.Load("Surface2Info/materials/SurfaceGridMaterial")));
        Lines.widthMultiplier = 0.01f;
        Lines.useWorldSpace = true;
        Lines.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Lines.receiveShadows = false;

        OnClick = () => {
            if (SurfaceShape == SurfaceType.Triangular || SceneController.GetInterface<SurfaceToolInterface>().CanConnectSurface) return;
            SceneController.GetInterface<SurfaceToolInterface>().Active = true;

            //if (StolperwegeHelper.player.IOEditor.ObjectToEdit != null &&
            //    StolperwegeHelper.player.IOEditor.ObjectToEdit.name.Equals(name))
            //{
            //    StolperwegeHelper.player.IOEditor.Active = false;
            //    return;
            //}
            //StolperwegeHelper.player.IOEditor.SetGameObject(this);
        };

        AsyncClick = ActualizeConnector;

        //if (GetComponent<CustomInteractiveObject>() != null && GetComponent<CustomInteractiveObject>() != this)
        //    Destroy(GetComponent<CustomInteractiveObject>());
    }

    public void Init(Vector3 normal, Vector3[] vertices)
    {
        Center = Vector3.zero;

        SurfaceShape = (vertices.Length == 3) ? SurfaceType.Triangular : SurfaceType.Rectangular;

        VertexAngleMap = new Dictionary<Vector3, float>();

        if (SurfaceShape == SurfaceType.Triangular)
        {
            Vector3 A = vertices[0];
            Vector3 B = vertices[1];
            Vector3 C = vertices[2];

            VertexAngleMap.Add(A, Mathf.Abs(Vector3.Dot((B - A).normalized, (C - A).normalized)));
            VertexAngleMap.Add(B, Mathf.Abs(Vector3.Dot((A - B).normalized, (C - B).normalized)));
            VertexAngleMap.Add(C, Mathf.Abs(Vector3.Dot((A - C).normalized, (B - C).normalized)));
            Center = A + B + C;
        } else
        {
            foreach (Vector3 v in vertices)
            {
                VertexAngleMap.Add(v, 0);
                Center += v;
            }
                
        }
            

        Center /= vertices.Length;

        InternInit(normal, vertices);

        if (GetComponent<MeshRenderer>() == null) DrawObject(new List<Vector3>(vertices));

    }

    public void Init(Vector3 normal, Vector3 center, float radius, GameObject surface)
    {
       
        Center = center;
        Normal = normal;
        SurfaceRadius = radius;
        Surface = surface;
        SurfaceShape = SurfaceType.Rectangular;

        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

        InternInit(normal, vertices);
    }

    PointFinger finger; SimpleGazeCursor gaze; GameObject mergedSurfaces; InteractiveObject lastHit;
    private IEnumerator ActualizeConnector() 
    {
        if (!SceneController.GetInterface<SurfaceToolInterface>().CanConnectSurface)
        {
            if (Arrow != null) Destroy(Arrow);
            if (Cone != null) Destroy(Cone);
            yield break;
        }
        finger = StolperwegeHelper.User.PointerHand;
        if (finger == null)
        {
            gaze = StolperwegeHelper.Gaze;
            Arrow = gaze.GetComponent<LineRenderer>();
            if (Arrow == null) Arrow = gaze.gameObject.AddComponent<LineRenderer>();
        } else
        {
            Arrow = finger.gameObject.GetComponent<LineRenderer>();
            if (Arrow == null) Arrow = finger.gameObject.AddComponent<LineRenderer>();
        }
        
        Arrow.positionCount = 9;
        Vector3[] points = new Vector3[9];
        Vector3 start = (finger != null) ? finger.RaySphere.transform.position : gaze.Hit.point;
        Arrow.SetPositions(points);
        Arrow.material = (Material)(Instantiate(Resources.Load("Surface2Info/materials/SurfaceGridMaterial")));
        Arrow.widthMultiplier = 0.01f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;
        Vector3 end, middle; Color color;
        Cone = (GameObject)(Instantiate(Resources.Load("Surface2Info/prefabs/Cone")));
        bool match = false;
        //while (!InputInterface.getButtonUp(InputInterface.ControllerType.RHAND, InputInterface.ButtonType.STICK))
        while (!SteamVR_Actions.default_click.GetStateUp(StolperwegeHelper.User.ControlHandType))
        {
            finger = StolperwegeHelper.User.PointerHand;
            if (finger == null) gaze = StolperwegeHelper.Gaze;
            end = (finger != null) ? finger.RaySphere.transform.position : gaze.Hit.point;
            middle = (start + end) / 2 + Normal;
            lastHit = (finger != null) ? finger.Hit : gaze.lastHit;
            match = lastHit != null && !lastHit.Equals(this) && lastHit is MeshSurface && IsCompatible((MeshSurface)lastHit);
            color = (match) ? Color.green : Color.red;
            Arrow.material.SetColor("_Color", color);            
            points = BezierCurve.CalculateCurvePoints(start, middle, end, points);
            Cone.GetComponent<Renderer>().material.SetColor("_Color", color);
            Cone.transform.forward = points[points.Length - 1] - points[points.Length - 2];
            Cone.transform.position = points[points.Length - 1] - Cone.transform.forward * Cone.GetComponent<Renderer>().bounds.size.z / 2;
            Cone.transform.forward = points[points.Length - 1] - points[points.Length - 2];
            points[points.Length - 1] = Cone.transform.position;
            Arrow.SetPositions(points);            
            yield return null;
        }
        Destroy(Arrow);
        Destroy(Cone);
        if (match) CalculateVertices();
    }

    private void CalculateVertices()
    {
        MeshSurface other = (MeshSurface)lastHit;
        Vector3[] newVerts;
        if (SurfaceShape == SurfaceType.Triangular)
        {
            int firstVIndex = 0; Vector3 first = Vector3.zero, second = Vector3.zero, third = Vector3.zero, last = Vector3.zero;
            bool firstFound = false, thirdFound = false;
            for (int i = 0; i < Vertices.Length; i++)
            {
                if (Vertices[i] != other.Vertices[0] &&
                    Vertices[i] != other.Vertices[1] &&
                    Vertices[i] != other.Vertices[2])
                {
                    first = Vertices[i];
                    firstVIndex = i;
                    firstFound = true;
                    break;
                }
            }

            if (firstVIndex == 0)
            {
                second = Vertices[1];
                last = Vertices[2];
            }
            else if (firstVIndex == 1)
            {
                second = Vertices[2];
                last = Vertices[0];
            }
            else
            {
                second = Vertices[0];
                last = Vertices[1];
            }

            for (int i = 0; i < other.Vertices.Length; i++)
            {
                if (other.Vertices[i] != second &&
                    other.Vertices[i] != last)
                {
                    thirdFound = true;
                    third = other.Vertices[i];
                    break;
                }
            }

            if (!firstFound || !thirdFound)
            {
                Debug.Log("Combining meshes failed...");
                return;
            }
            newVerts = new Vector3[] { first, second, third, last };
        }
        else
        {
            HashSet<Vector3> vertSet = new HashSet<Vector3>(Vertices);
            vertSet.SymmetricExceptWith(other.Vertices);

            Quaternion toWorldFw = Quaternion.FromToRotation(Normal, Vector3.forward);

            Dictionary<Vector3, Vector3> vertMap = new Dictionary<Vector3, Vector3>();

            Vector3 c = Vector3.zero;
            foreach (Vector3 v in vertSet)
                c += v;

            c /= vertSet.Count;
            foreach (Vector3 v in vertSet)
                vertMap.Add(toWorldFw * (v - c), v);
            
            ClockwiseVector3Comparer comp = new ClockwiseVector3Comparer();
            List<Vector3> keys = new List<Vector3>(vertMap.Keys);
            keys.Sort(comp);

            newVerts = new Vector3[keys.Count];
            for (int i=0; i< newVerts.Length; i++)
               newVerts[i] = vertMap[keys[i]];

        }

        mergedSurfaces = new GameObject(name + "+" + lastHit.name);
        mergedSurfaces.AddComponent<MeshSurface>().Init(Normal, newVerts);
        mergedSurfaces.transform.position = mergedSurfaces.GetComponent<MeshSurface>().Center;
        Destroy(lastHit.gameObject);
        Destroy(gameObject);
    }

    private bool IsCompatible(MeshSurface other)
    {
        if (Vector3.Angle(Normal, other.Normal) >= 1 || 
            SurfaceShape != other.SurfaceShape) return false;
        int equalPoints = 0;
        foreach (Vector3 vert in VertexAngleMap.Keys)
            foreach (Vector3 otherVert in other.VertexAngleMap.Keys)
                if ((vert - otherVert).magnitude < 0.001f && (SurfaceShape == SurfaceType.Rectangular ||
                    (VertexAngleMap[vert] > 0.1f && other.VertexAngleMap[otherVert] > 0.1f)))
                    equalPoints += 1;
                
        return equalPoints == 2;
    }

    private void DrawObject(List<Vector3> vertexList)
    {
        
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        Mesh = new Mesh();
        Mesh.name = "ObjectMesh";

        Vector3 drawNormal = Normal.normalized;

        List<int> triangles = new List<int>();
        Vector3[] normals = new Vector3[vertexList.Count * 4];

        SurfaceRadius = 0;

        for (int i = 0; i < vertexList.Count; i++)
        {
            vertexList[i] -= Center;
            SurfaceRadius += vertexList[i].magnitude;
        }

        SurfaceRadius /= vertexList.Count;

        // add the vertices again to the list for back side and translate them a bit on the drawnormal for depth
        vertexList.AddRange(vertexList);
        for (int i = vertexList.Count / 2; i < vertexList.Count; i++)
            vertexList[i] += drawNormal * 0.005f;

        // create triangles for each sides
        for (int i = 1; i < vertexList.Count / 2 - 1; i++)
            triangles.AddRange(new int[] { i, i + 1, 0 });

        for (int i = 1; i < vertexList.Count / 2 - 1; i++)
            triangles.AddRange(new int[] { i + vertexList.Count / 2, vertexList.Count / 2, i + vertexList.Count / 2 + 1 });

        Plane plane;
        Vector3 up = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // add vertices again and create the sides

        vertexList.AddRange(vertexList);

        for (int i = vertexList.Count / 2; i < vertexList.Count * 3 / 4; i++)
            if (i == vertexList.Count * 3 / 4 - 1)
            {
                triangles.AddRange(new int[] { i, i + vertexList.Count / 4, vertexList.Count / 2,
                                                   vertexList.Count / 2, i + vertexList.Count / 4, i + 1 });
                plane = new Plane(vertexList[i], vertexList[i + vertexList.Count / 4], vertexList[vertexList.Count / 2]);
                if (plane.normal.y > up.y) up = plane.normal;

            }
            else
            {
                triangles.AddRange(new int[] { i, i + vertexList.Count / 4, i + 1, i + 1, i + vertexList.Count / 4, i + vertexList.Count / 4 + 1 });
                plane = new Plane(vertexList[i], vertexList[i + vertexList.Count / 4], vertexList[i + 1]);
                if (plane.normal.y > up.y) up = plane.normal;
            }

        Quaternion rot = Quaternion.LookRotation(drawNormal, up);
        Quaternion rotDiff = transform.rotation * Quaternion.Inverse(rot);

        for (int i = 0; i < vertexList.Count; i++)
            vertexList[i] = rotDiff * vertexList[i];

        Mesh.vertices = vertexList.ToArray();
        Mesh.normals = normals;
        Mesh.triangles = triangles.ToArray();
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = Mesh;

        MeshCollider coll = gameObject.AddComponent<MeshCollider>();
        coll.convex = true;
        coll.isTrigger = true;        
        coll.skinWidth = 0;

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.enabled = false;

        transform.localScale = Vector3.one;
        transform.rotation = rot;

        Surface = gameObject;
    }
}
