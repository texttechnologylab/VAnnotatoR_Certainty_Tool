using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceRecognizer : MonoBehaviour {

    public static float PREC = 100000000f;

    public enum SurfaceAngleFilter { With90Deg, All };
    public SurfaceAngleFilter AngleFilter;

    public enum SurfaceSizeFilter { Big, All };
    public SurfaceSizeFilter SizeFilter;
    
    public Dictionary<GameObject, int> ProcessedObjects { get; private set; }
    public HashSet<MeshSurface> Surfaces;

    public List<MeshSurfaceContainer> Containers { get; private set; }

    public HUDMenuStatusBox StatusBox { get { return SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StatusBox; } }
    
    private GameObject _selectedObject;
    public GameObject SelectedObject
    {
        get { return _selectedObject; }
        set
        {
            if (value == _selectedObject) _selectedObject = null;
            else _selectedObject = value;
            SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StartRecognition.Active = _selectedObject != null && !ProcessedObjects.ContainsKey(_selectedObject);
            SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StopRecognition.Active = false;
            SceneController.GetInterface<SurfaceToolInterface>().DrawUI.CleanObject.Active = _selectedObject != null && ProcessedObjects.ContainsKey(_selectedObject);
            ActualizeObjectInfo();
        }
    }

    public bool InterruptProcessing;
    public bool Processing { get; private set; }

	// Use this for initialization
	void Start () {
        Containers = new List<MeshSurfaceContainer>();
        Surfaces = new HashSet<MeshSurface>();
        ProcessedObjects = new Dictionary<GameObject, int>();
    }
	

    List<GameObject> infoSurfaces; GameObject plane; MeshFilter[] renderedParts;
    Vector3 v1, v2, v3, dir, u, v, w;
    private int mCnt, maxMeshes, tCnt, maxTriangles;
    public IEnumerator ProcessNextObject()
    {
        Processing = true;
        if (!ProcessedObjects.ContainsKey(SelectedObject))
            ProcessedObjects.Add(SelectedObject, 0);        

        infoSurfaces = new List<GameObject>();
        
        renderedParts = SelectedObject.GetComponentsInChildren<MeshFilter>();        

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        Mesh mesh; int counter = 0, checkedTris = 0;
        maxMeshes = renderedParts.Length;
        SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StopRecognition.Active = true;
        for (mCnt=0; mCnt< renderedParts.Length; mCnt++)
        {
            maxTriangles = renderedParts[mCnt].mesh.triangles.Length;
            if (InterruptProcessing) break;
            for (tCnt=0; tCnt< renderedParts[mCnt].mesh.triangles.Length-2; tCnt+=3)
            {
                checkedTris += 1;
                mesh = renderedParts[mCnt].mesh;
                v1 = renderedParts[mCnt].transform.TransformPoint(mesh.vertices[mesh.triangles[tCnt]]);
                v2 = renderedParts[mCnt].transform.TransformPoint(mesh.vertices[mesh.triangles[tCnt + 1]]);
                v3 = renderedParts[mCnt].transform.TransformPoint(mesh.vertices[mesh.triangles[tCnt + 2]]);

                u = v2 - v1;
                v = v3 - v1;
                w = v3 - v2;
                bool has90Deg = Mathf.Abs(Vector3.Dot(u, v)) <= 0.1f || Mathf.Abs(Vector3.Dot(w, v)) <= 0.1f || Mathf.Abs(Vector3.Dot(u, w)) <= 0.1f;
                dir = Vector3.zero;
                dir.x = (u.y * v.z) - (u.z * v.y);
                dir.y = (u.z * v.x) - (u.x * v.z);
                dir.z = (u.x * v.y) - (u.y * v.x);
                dir = dir.normalized;
                dir = new Vector3((int)(dir.x * PREC) / PREC, 
                                  (int)(dir.y * PREC) / PREC, 
                                  (int)(dir.z * PREC) / PREC);
                ActualizeObjectInfo();
                if (InterruptProcessing) break;
                if ((AngleFilter == SurfaceAngleFilter.All && !has90Deg) || 
                    (SizeFilter == SurfaceSizeFilter.Big && (Vector3.Cross(u, v).magnitude / 2 < 1 || u.magnitude < 1 || v.magnitude < 1 || w.magnitude < 1)))
                    continue;
                
                plane = new GameObject("InfoPlane_" + counter++);
                plane.AddComponent<MeshSurface>().Init(dir, new Vector3[] { v3, v2, v1 });
                plane.transform.SetParent(renderedParts[mCnt].transform, true);
                plane.transform.position = plane.GetComponent<MeshSurface>().Center;
                ProcessedObjects[SelectedObject] += 1;                
                yield return null;
            }
        }
        Processing = false;
        InterruptProcessing = false;
        SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StartRecognition.ButtonOn = false;
        SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StartRecognition.GetComponent<Collider>().enabled = true;
        SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StartRecognition.Active = SelectedObject != null && !ProcessedObjects.ContainsKey(SelectedObject);
        SceneController.GetInterface<SurfaceToolInterface>().DrawUI.CleanObject.Active = SelectedObject != null && ProcessedObjects.ContainsKey(SelectedObject);
        SceneController.GetInterface<SurfaceToolInterface>().DrawUI.StopRecognition.Active = false;    
        ActualizeObjectInfo();
    }

    MeshSurface[] oldSurfaces;
    // CustomInteractiveObject[] oldObjects;
    public void CleanObject()
    {        
        //oldObjects = SelectedObject.GetComponentsInChildren<CustomInteractiveObject>();
        //oldSurfaces = SelectedObject.GetComponentsInChildren<MeshSurface>();
        //if (oldSurfaces != null)
        //    for (int i = 0; i < oldSurfaces.Length; i++)
        //        Destroy(oldSurfaces[i].gameObject);
        //if (oldObjects != null)
        //    for (int i = 0; i < oldObjects.Length; i++)
        //        Destroy(oldObjects[i].gameObject);
        //ProcessedObjects.Remove(SelectedObject);
        //ActualizeObjectInfo();
    }

    public void ActualizeObjectInfo()
    {
        if (SelectedObject == null)
        {
            StatusBox.SetFocus("No object in focus.");
            StatusBox.TurnOffStatus();
            return;
        }
        if (ProcessedObjects.ContainsKey(SelectedObject))
        {
            StatusBox.SetFocus("In progress: " + SelectedObject.name);
            if (Processing)
            {
                StatusBox.SetStatus1("Processing submeshes: " + (mCnt + 1)  + " of " + maxMeshes);
                StatusBox.SetStatus2("Submesh progress: " + ((int)((float)tCnt / maxTriangles * 100)) + "%");
            }
            else
            {
                StatusBox.SetStatus1("Detected triangles: " + ProcessedObjects[SelectedObject]);
                if (ProcessedObjects[SelectedObject] > 0)
                    StatusBox.Status2.gameObject.SetActive(false);
                else
                    StatusBox.SetStatus2("Try an another setup.");
            }
        } else
        {
            StatusBox.SetFocus("In focus: " + SelectedObject.name);
            StatusBox.TurnOffStatus();
            StatusBox.SetStatus1("No info available. Click to start analysis.");
        }
        
        
    }


}
