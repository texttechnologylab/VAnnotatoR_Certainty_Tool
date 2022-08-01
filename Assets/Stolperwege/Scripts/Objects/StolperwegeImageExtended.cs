using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolperwegeImageExtended : MonoBehaviour {

    Vector3 enterPos, exitPos, currentPos;

    List<Vector2> positions;


    private void Start()
    {
        positions = new List<Vector2>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PointFingerColliderScript>() != null)
        {
            Vector3 dir = Quaternion.Inverse(transform.rotation) * (other.transform.position - transform.position);
            Debug.Log(dir.y);
            if(dir.y < -0.01f)
            {
                Vector2 pos = Quaternion.Inverse(transform.rotation) * other.transform.position;
                pos = new Vector2(-dir.x, dir.z);
                positions.Add(pos);
                
                UpdateMarkerBox();
                //AddCorner(transform.rotation*pos+transform.position);
                AddCorner(other.transform.position);
                Debug.Log(XYToPixel(pos));
            }
            
        }

        if (other.gameObject.name.Contains("Arm"))
        {
            StartCoroutine(SubImage(other));
        }

    }

    private void AddCorner(Vector3 pos)
    {
        if(Corner == null)
        {
            Corner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Corner.transform.localScale = Vector3.one * 0.03f;
            GameObject parent = new GameObject();
            parent.transform.parent = transform.parent.parent;
            Corner.transform.parent = parent.transform;
            Corner.SetActive(false);
            Corner.GetComponent<MeshRenderer>().material.color = StolperwegeHelper.GUCOLOR.GOETHEBLAU;
            Corner.GetComponent<Collider>().isTrigger = true;
        }

        GameObject cornerSphere = Instantiate(Corner);
        
        cornerSphere.SetActive(true);
        cornerSphere.transform.parent = transform.parent.parent;
        cornerSphere.transform.SetPositionAndRotation(pos, Quaternion.identity);
        cornerSphere.transform.localPosition -= Vector3.forward * cornerSphere.transform.localPosition.z;
    }

    IEnumerator SubImage(Collider other)
    {
        //WWWForm form = new WWWForm();

        //string keys = "?uri=https://upload.wikimedia.org/wikipedia/de/b/b6/HochGustav.jpg"
        //    +"&path="+SVGPath();

        //form.AddField("uri", "https://upload.wikimedia.org/wikipedia/de/b/b6/HochGustav.jpg");
        ////form.AddField("session", "9AB45CD01899DFE20447946E3B33DEE8.jvm1");
        //form.AddField("path", SVGPath());
        //Debug.Log(StolperwegeInterface.WS+ "subimage" + keys);

        //WWW www = new WWW(StolperwegeInterface.WS + "subimage" + keys);

        //yield return www;

        Hashtable keysTab = new Hashtable
        {
            {"uri",transform.parent.parent.GetComponentInChildren<ImageObject>().Referent.ID},
            {"path", SVGPath() }
        };

        ClearMarker();

        StolperwegeInterface.OnCreated onCreated = (StolperwegeElement e) => {
            Debug.Log("related" + e.RelatedElements.Count);
            e.StolperwegeObject.GetComponent<StolperwegeObject>().OnGrab(other);
            other.GetComponent<DragFinger>().GrabedObject = e.StolperwegeObject;
            //e.AddStolperwegeRelation(getStolperwegeUri(), true);
            //e.Object3D.GetComponent<MeshRenderer>().material.mainTexture = www.texture;
        };

        yield return SceneController.GetInterface<StolperwegeInterface>().CreateElement("subimage", keysTab, onCreated, true);
    }


    GameObject MarkerBox = null;
    GameObject Corner = null;
    Mesh mask;
    private void UpdateMarkerBox()
    {
        if(MarkerBox == null)
        {
            mask = new Mesh();
            MarkerBox = GameObject.CreatePrimitive(PrimitiveType.Plane);
            StolperwegeHelper.ChangeBlendMode(MarkerBox.GetComponent<MeshRenderer>().material,"Fade");
            MarkerBox.GetComponent<MeshRenderer>().material.color = StolperwegeHelper.goetheBlauTrans;
            MarkerBox.transform.parent = transform.parent.parent;
            MarkerBox.transform.localPosition = Vector3.forward * -0.01f;
            MarkerBox.transform.localEulerAngles = Vector3.zero;
            MarkerBox.GetComponent<MeshFilter>().mesh = mask;
            MarkerBox.GetComponent<Collider>().enabled = false;
        }

        CreateMesh(false);

        if((Quaternion.Inverse(transform.rotation)*mask.normals[0]).z < 0)
        {
            CreateMesh(true);
        }
    }

    private void CreateMesh(bool flip)
    {
        Vector2[] vert = positions.ToArray();

        System.Array.Reverse(vert);

        Triangulator tr = new Triangulator(vert);
        int[] indices = tr.Triangulate();

        if (flip)
            System.Array.Reverse(indices);
        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vert.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vert[i].x, vert[i].y, 0);
        }

        mask.vertices = vertices;
        mask.triangles = indices;
        mask.RecalculateNormals();
    }

    private Vector2 XYToPixel(Vector2 pos)
    {
        Texture image = GetComponent<MeshRenderer>().material.mainTexture;
        int height = image.height;
        int width = image.width;

        float GOHeight = transform.lossyScale.z;
        float GOWidth = transform.lossyScale.x;

        Debug.Log(height + " " + width + " " + GOHeight + " " + GOWidth);

        Vector3 Center = Quaternion.Inverse(transform.rotation) * transform.position;

        Vector2 topLeft = new Vector2(Center.x - GOWidth / 2, Center.z + GOHeight / 2);
        Vector2 downRight = new Vector2(Center.x + GOWidth / 2, Center.z - GOHeight / 2);

        pos += new Vector2(Center.x,Center.z);

        float u = (topLeft.x - pos.x)/(topLeft.x - downRight.x);
        float v = (topLeft.y - pos.y) / (topLeft.y - downRight.y);

        Vector2 pixelCoords = new Vector2((u * width), v * height);

        return pixelCoords;
    }

    private string SVGPath()
    {
        string path = "M";

        foreach(Vector3 pos in positions)
        {
            Vector2 pixel =  XYToPixel(new Vector2(pos.x, pos.y));

            path += (int)pixel.x + "," + (int)pixel.y;
            path += "L";
        }

        path = path.Substring(0, path.Length - 1);
        path += "Z";

        return path;
    }

    private void ClearMarker()
    {
        positions = new List<Vector2>();

        for (int i = 0; i < transform.parent.parent.childCount; i++)
            if (transform.parent.parent.GetChild(i).name.Contains("Sphere"))
                Destroy(transform.parent.parent.GetChild(i).gameObject);

        UpdateMarkerBox();
    }

    private static int Orientation(Vector2 p1, Vector2 p2, Vector2 p)
    {
        float Orin = (p2.x - p1.x) * (p.y - p1.y) - (p.x - p1.x) * (p2.y - p1.y);

        if (Orin > 0)
            return -1;
        if (Orin < 0)
            return 1;

        return 0;
    }

    private class Vector2Comparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            Vector2 v1 = (Vector2)x;
            Vector2 v2 = (Vector2)y;
            return v1.x.CompareTo(v2.x);
        }
    }
}
