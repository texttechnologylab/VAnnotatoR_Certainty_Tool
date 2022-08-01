using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR;

public class CircleMenu : InteractiveObject {

    public float DOUBLE_CLICK_INTERVAL = 0.5f;
    private int _count;
    private GameObject[] _subParts;
    // never used
    //private string[] teststr = { "Equivalent", "Place", "Name" };
    private GameObject _selected;
    private GameObject _cursor;
    private GameObject _selector;

    private Hashtable _objects;
    private List<string> _keys;

    public delegate void onSelection(string key, object o);

    public onSelection _execute;

    public Dictionary<string, Color> Colors;

    public bool UseColors { get { return Colors != null; } }

    public override void Awake()
    {
        base.Awake();
        SearchForParts = false;
        Grabable = true;
        OnFocus = CheckCursor;
        OnPointer = CheckCursor;
    }

    public override void Start()
    {
        
    }

    public void Init(onSelection execute, Hashtable objects, bool useCollider=true)
    { 
        _objects = objects;
        _execute = execute;
        _cursor = transform.Find("Sphere").gameObject;
        _cursor.GetComponent<MeshRenderer>().enabled = true;
        
        _count = objects.Count;
        _subParts = new GameObject[_count];
        Generate(useCollider);
    }

    public void Init(onSelection execute, Hashtable objects, Dictionary<string, Color> colors, bool useCollider=true)
    {
        _objects = objects;
        _execute = execute;
        Colors = colors;
        _cursor = transform.Find("Sphere").gameObject;
        _cursor.GetComponent<MeshRenderer>().enabled = true;

        _count = objects.Count;
        _subParts = new GameObject[_count];
        Generate(useCollider);
    }

    private bool _clicked;
    private float _doubleClickTimer;

    void Generate(bool useCollider)
    {
        float deltaAngle = 360f / _count;
        int vertexCount = Mathf.CeilToInt(deltaAngle / 10) * _count;
        float diffAngle = 360f / vertexCount;
        // die auskommentierte Zeile hat bei mir überlappende Teile produziert (bei 10 Unterteilen)
        //float diffAngle = 360f / VertexCount+1;
        //Vector3 start = new Vector3(Random.Range(0, 100) / 100f, Random.Range(0, 100) / 100f, 0);
        Vector3 start = Vector3.up;
        if (_count > 15) start *= 1.5f;
        //start.Normalize();
        if (_objects != null)
        {
            string[] keyArray = new string[_objects.Keys.Count];
            _objects.Keys.CopyTo(keyArray, 0);
            System.Array.Sort(keyArray);
            _keys = new List<string>(keyArray);
        }
        
        int subVertexCount = Mathf.CeilToInt(deltaAngle / diffAngle)+1;
        Object prefab = Resources.Load("StolperwegeElements/StolperwegeText");

        for (int i=0; i<_count; i++)
        {
            GameObject text = (GameObject) Instantiate(prefab);


            GameObject subpart = new GameObject();
            subpart.name = _keys[i].Replace("org.hucompute.publichistory.datastore.typesystem.","");
            subpart.AddComponent<MeshFilter>();
            Material mat = new Material(Shader.Find("Shader Graphs/glow"));
            subpart.AddComponent<MeshRenderer>().material = mat;

            Mesh mesh = new Mesh();

            

            Vector3[] vertices = new Vector3[subVertexCount+1];

            vertices[0] = Vector3.zero;
            //for (float j = i * deltaAngle; j <= (i+1)*deltaAngle; j += diffAngle)
            for(int j=0; j<subVertexCount; j++) 
            {
                Vector3 temp = Quaternion.Euler(0,0, i * deltaAngle + (diffAngle * j)) * start;

                vertices[j+1] = temp;

            }

            int[] indices = new int[subVertexCount*3];

            for (int j = 0; j < subVertexCount; j++)
            {
                indices[j * 3] = 0;
                indices[j * 3 + 1] = j;
                indices[j * 3 + 2] = j + 1;

            }

            mesh.vertices = vertices;
            mesh.triangles = indices;

            subpart.GetComponent<MeshFilter>().mesh = mesh;

            if (i == 0)
            {
                _selector = Instantiate(subpart);
                _selector.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load<Material>("Materials/CircleMenuMaterial"));
                _selector.transform.SetParent(transform);
                _selector.SetActive(false);
            }
            

            subpart.transform.parent = transform;
            subpart.transform.localPosition = Vector3.zero;

            //subpart.AddComponent<InteractiveObject>().activateOutline = true;

            _subParts[i] = subpart;

            text.transform.SetParent(subpart.transform);

            text.GetComponent<TextMeshPro>().text = subpart.name;
            text.GetComponent<TextMeshPro>().enableAutoSizing = true;
            text.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;

            text.transform.localPosition = ((_count == 1)? Vector3.zero :  Quaternion.Euler(0, 0, i * deltaAngle + ((deltaAngle / 2))) * ((start/1.5f))) + Vector3.forward * 0.04f;
            
            Vector3 euler = text.transform.localEulerAngles;
            euler.z = Vector3.Angle(transform.position - text.transform.position, Vector3.right);
            if (subpart.GetComponent<Renderer>().bounds.center.x - transform.position.x < 0)
            {
                euler.y += 180;
                euler.z *= -1;
            }
            else
            {
                euler.z -= 180;
                euler.z *= -1;
                euler.y = 180;
            }

            if (subpart.GetComponent<Renderer>().bounds.center.y - transform.position.y > 0)
                euler.z *= -1;

            if (UseColors)
            {
                subpart.GetComponent<MeshRenderer>().material.SetColor("_Color", Colors[_keys[i]]);
            }                
            else
            {
                subpart.GetComponent<MeshRenderer>().material.SetColor("_Color", (i % 2 == 0) ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.5f, 0.5f, 0.5f));
            }
            subpart.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", StolperwegeHelper.GUCOLOR.LICHTBLAU * 3);

            text.transform.localEulerAngles = euler;

            if (useCollider)
            {
                BoxCollider collider = gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(3, 3, 0.05f);
            }
            //text.transform.localPosition = Vector3.forward * 0.04f;
        }
        
    }

    GameObject part;
    GameObject nearest;
    public void CheckCursor(Vector3 hitPos)
    {
        Vector2 pos = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand);
        pos.x *= -1;
        _cursor.transform.localPosition = pos;
        Vector3 sphere = _cursor.transform.position;

        float dist = float.PositiveInfinity;
        nearest = null;
        if (!(_count > 1 &&  _cursor.transform.localPosition == Vector3.zero))
            for (int i = 0; i < _subParts.Length; i++)
            {
                part = _subParts[i];
                float tempDist = Vector3.Distance(sphere, part.GetComponent<Renderer>().bounds.center);

                if (tempDist < dist)
                {
                    nearest = part;
                    dist = tempDist;
                }
            }

        _selector.SetActive(false);
        for (int i=0; i<_subParts.Length; i++)
        {
            GameObject part = _subParts[i];
            float d = (part != nearest) ? 0 : 0.001f;
            //if (part == nearest) part.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION_ON");
            //else part.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION_ON");
            //part.transform.localPosition = Vector3.forward * d;
            if (part == nearest)
            {
                _selector.SetActive(true);
                _selector.transform.localPosition = part.transform.localPosition + Vector3.forward * 0.001f;
                _selector.transform.localEulerAngles = Vector3.forward * i * 360f / _count;
            }
        }

        _selected = nearest;
        if (_selected == null) return;

        if (SteamVR_Actions.default_trigger.GetStateDown(StolperwegeHelper.User.PointerHandType))
        {
            if (_clicked && _doubleClickTimer <= DOUBLE_CLICK_INTERVAL)
            {
                _clicked = false;
                _doubleClickTimer = 0;
                _execute(_selected.name, _objects[_selected.name]);
            }
            if (!_clicked)
            {
                _clicked = true;
            }
        }

        if (_clicked)
            _doubleClickTimer += Time.deltaTime;

        if (_doubleClickTimer > DOUBLE_CLICK_INTERVAL)
        {
            _clicked = false;
            _doubleClickTimer = 0;
            //Destroy(gameObject);
        }

    }

    private void OnDestroy()
    {
        //player.LockRotation = false;
        Destroy(gameObject);
    }
}
