using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using MathHelper;


public class OfflineDocumentLoader : MonoBehaviour
{
    //string file = "Text2Scene/Scenes/SceneVisualizer/9_0_4_final_4.xml";
    //string file = "Text2Scene/Scenes/SceneVisualizer/ACL21_Example.xml";
    string file = "Text2Scene/Scenes/SceneVisualizer/isa17v2.xml";

    private XNamespace xmiNamespace = "http://www.omg.org/XMI";

    void Start()
    {
        StolperwegeHelper.CenterEyeAnchor = gameObject;
        LoadDocument();
    }


    private void LoadDocument()
    {
        Debug.Log("LoadDocument");

        string m_Path = Application.dataPath;
        Debug.Log("Document Path: " + m_Path + "/" + file);
        StreamReader sR = new StreamReader(m_Path + "/" + file);
        string result = sR.ReadToEnd();
        Debug.Log("XML read.");
        sR.Close();

        XDocument xmi = XDocument.Parse(result);
        StartCoroutine(CreateDocument(xmi));
        //Debug.Log("Parsing done");
    }


    bool _loading_obj = false;
    public IEnumerator CreateDocument(XDocument data)
    {
        Debug.Log("CreateDocument");

        ShapeNetInterface inter = gameObject.AddComponent<ShapeNetInterface>();
        UMAManagerInterface UMAManagerInterface = gameObject.AddComponent<UMAManagerInterface>();
        Debug.Log("ShapeNetInterface Initialize");
        StartCoroutine(inter.Initialize());
        while (!inter.Initialized)
            yield return null;

        Debug.Log("ShapeNetInterface Initialized");

        Debug.Log("Create document");

        XElement doc = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("DocumentAnnotation") || a.Name.LocalName.Contains("DocumentMetaData")).First();

        // id = int.Parse(doc.Attribute(xmiNamespace + "id").Value);
        // begin = int.Parse(doc.Attribute("begin").Value);
        // end = int.Parse(doc.Attribute("end").Value);
        List<XElement> objList;

        Debug.Log("Load Vector3");
        // AnnotationDocument Document = new AnnotationDocument(id, Text);
        Dictionary<int, IsoVector3> vec3List = new Dictionary<int, IsoVector3>();
        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Vec3")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                float x = oId.Attribute("x") != null ? NumericHelper.ParseFloat(oId.Attribute("x").Value) : 0;
                float y = oId.Attribute("y") != null ? NumericHelper.ParseFloat(oId.Attribute("y").Value) : 0;
                float z = oId.Attribute("z") != null ? NumericHelper.ParseFloat(oId.Attribute("z").Value) : 0;
                vec3List.Add(id, new IsoVector3(id, x, y, z, null));
            }
        }

        Debug.Log("Load Vector4");
        Dictionary<int, IsoVector4> vec4List = new Dictionary<int, IsoVector4>();
        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Vec4")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                float x = oId.Attribute("x") != null ? NumericHelper.ParseFloat(oId.Attribute("x").Value) : 0;
                float y = oId.Attribute("y") != null ? NumericHelper.ParseFloat(oId.Attribute("y").Value) : 0;
                float z = oId.Attribute("z") != null ? NumericHelper.ParseFloat(oId.Attribute("z").Value) : 0;
                float w = oId.Attribute("w") != null ? NumericHelper.ParseFloat(oId.Attribute("w").Value) : 0;
                vec4List.Add(id, new IsoVector4(id, x, y, z, w, null));
            }
        }


        Debug.Log("Load SpatialEntities");
        //List<IsoEntity> entityList = new List<IsoEntity>();
        Dictionary<int, IsoEntity> entityDic = new Dictionary<int, IsoEntity>();
        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("SpatialEntity")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                XAttribute obj_id = oId.Attribute("object_id");
                Debug.Log(obj_id);
                if(obj_id != null)
                {
                    int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                    int pos = int.Parse(oId.Attribute("position").Value);
                    int rot = int.Parse(oId.Attribute("rotation").Value);
                    int scale = int.Parse(oId.Attribute("scale").Value);
                    IsoSpatialEntity ent = new IsoSpatialEntity(null, id, 0,0, obj_id.Value, vec3List[pos], vec4List[rot], vec3List[scale], null, null, null, null, IsoSpatialEntity.DimensionType.volume, IsoSpatialEntity.FormType.nom, false, null, null, null, null, false, null, null);
                    entityDic.Add(id, ent);
                    while (_loading_obj)
                        yield return null;
                    
                    CreateObject(ent);
                }
            }
        }

        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("EventPath")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                XAttribute obj_id = oId.Attribute("object_id");
                Debug.Log(obj_id);
                if (obj_id != null)
                {
                    int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                    int pos = int.Parse(oId.Attribute("position").Value);
                    int rot = int.Parse(oId.Attribute("rotation").Value);
                    int scale = int.Parse(oId.Attribute("scale").Value);
                    int startID = int.Parse(oId.Attribute("startID").Value);
                    int endID = int.Parse(oId.Attribute("endID").Value);


                    IsoEntity start = entityDic[startID];
                    IsoEntity end = entityDic[endID];

                    List<Vector3> locationList = new List<Vector3>();

                    GameObject obj = new GameObject();
                    obj.transform.parent = transform;
                    LineRenderer eventPathLineRenderer = obj.AddComponent<LineRenderer>();



                    //eventPathLineRenderer.startColor = StolperwegeHelper.GUCOLOR.ORANGE;
                    //eventPathLineRenderer.endColor = StolperwegeHelper.GUCOLOR.ORANGE;
                    eventPathLineRenderer.material.SetColor("_Color", StolperwegeHelper.GUCOLOR.ORANGE);
                    eventPathLineRenderer.generateLightingData = true;

                    eventPathLineRenderer.startWidth = 0.05f;
                    eventPathLineRenderer.endWidth = 0.025f;
                    eventPathLineRenderer.useWorldSpace = true;

                    locationList.Add(start.Position.Vector);

                    locationList.Add(end.Position.Vector);

                    eventPathLineRenderer.positionCount = locationList.Count;
                    for (int i = 0; i < locationList.Count; i++)
                        eventPathLineRenderer.SetPosition(i, locationList[i]);
                }
            }
        }


        List<IsoLink> linkList = new List<IsoLink>();
        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Meta")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                int figure = int.Parse(oId.Attribute("figure").Value);
                int ground = int.Parse(oId.Attribute("ground").Value);
                string rel_type = oId.Attribute("rel_type").Value;

                IsoEntity f = entityDic[figure];
                IsoEntity g = entityDic[ground];

                IsoMetaLink ent = new IsoMetaLink(null, id, "", "", f, g, null, rel_type);
                linkList.Add(ent);

            }
        }

        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("QsLink")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                int figure = int.Parse(oId.Attribute("figure").Value);
                int ground = int.Parse(oId.Attribute("ground").Value);
                string rel_type = oId.Attribute("rel_type").Value;

                IsoEntity f = entityDic[figure];
                IsoEntity g = entityDic[ground];

                IsoQsLink ent = new IsoQsLink(null, id, "", "", f, g, null, rel_type);
                linkList.Add(ent);

            }
        }

        objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("OLink")).ToList();
        if (objList.Count > 0)
        {
            foreach (XElement oId in objList)
            {
                int id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                int figure = int.Parse(oId.Attribute("figure").Value);
                int ground = int.Parse(oId.Attribute("ground").Value);
                string rel_type = oId.Attribute("rel_type").Value;

                IsoEntity f = entityDic[figure];
                IsoEntity g = entityDic[ground];

                IsoOLink ent = new IsoOLink(null, id, "", "", f, g, null, rel_type, false, "", null);
                linkList.Add(ent);

            }
        }


        UMAManagerInterface.AttributeEntityLinks = new List<IsoMetaLink>();
        foreach (IsoLink link in linkList)
        {
            // get the ground object of the link
            IsoEntity figure = link.Figure;
            IsoEntity ground = link.Ground;
            if (figure == null || ground == null)
            {
                Debug.LogWarning("The figure is not existing anymore.");
                continue;
            }

            if (link is IsoMetaLink && figure is IsoSpatialEntity iFigure && UMAISOEntity.AVATAR_TYPE_NAMES.Contains(iFigure.Object_ID))
            {
                UMAManagerInterface.AttributeEntityLinks.Add((IsoMetaLink)link);
                continue;
            }

        }
        UMAManagerInterface.BuildAvatars();


        foreach (IsoLink link in linkList)
        {
            if (link is IsoOLink || link is IsoQsLink)
            {
                //CreateLink(link);
            }
        }

        Debug.Log("Document created");
        yield break;
    }

    public void CreateLink(IsoLink link)
    {
        GameObject ArrowObject = new GameObject("ArrowLine");
        ArrowObject.transform.parent = transform;

        LineRenderer Arrow = ArrowObject.AddComponent<LineRenderer>();
        Arrow.enabled = true;
        Arrow.material = (Material)(Instantiate(Resources.Load("Materials/UI/GoetheBlauUnlit")));
        Arrow.positionCount = 9;
        Vector3[] _points = new Vector3[9];
        Arrow.enabled = true;
        Arrow.widthMultiplier = 0.005f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;



        GameObject LabelObject = new GameObject("ArrowLabel");
        LabelObject.transform.parent = transform;
        TextMeshPro ArrowLabel = LabelObject.AddComponent<TextMeshPro>();
        ArrowLabel.fontSize = 0.75f;
        ArrowLabel.font = Resources.Load<TMP_FontAsset>("Font/FontAwesomeSolid5");
        ArrowLabel.alignment = TextAlignmentOptions.Center;
        ArrowLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 0.2f);

        ArrowLabel.outlineWidth = 0.2f;


        Color _color;
        if (link.GetType() == typeof(IsoQsLink))
        {
            ArrowLabel.text = "QsLink (" + link.Rel_Type + ")";
            _color = IsoQsLink.ClassColor;
        }
        else if (link.GetType() == typeof(IsoOLink))
        {
            ArrowLabel.text = "OLink (" + link.Rel_Type + ")";
            _color = IsoOLink.ClassColor;
        }
        else if (link.GetType() == typeof(IsoMoveLink))
        {
            ArrowLabel.text = "MoveLink (" + link.Rel_Type + ")";
            _color = IsoOLink.ClassColor;
        }
        else if (link.GetType() == typeof(IsoSrLink))
        {
            ArrowLabel.text = "SrLink (" + link.Rel_Type + ")";
            _color = IsoSrLink.ClassColor;
        }
        else if (link.GetType() == typeof(IsoMetaLink))
        {
            ArrowLabel.text = "MetaLink (" + link.Rel_Type + ")";
            _color = IsoMetaLink.ClassColor;
        }
        else if (link.GetType() == typeof(IsoMLink))
        {
            ArrowLabel.text = "MLink (" + link.Rel_Type + ")";
            _color = IsoMLink.ClassColor;
        }
        else
        {
            ArrowLabel.text = "Link (" + link.Rel_Type + ")";
            _color = Color.black;
        }
        Arrow.material.SetColor("_Color", _color);
        ArrowLabel.color = _color;


        Vector3 figure_center = link.Figure.Position.Vector;
        Vector3 ground_center = link.Ground.Position.Vector;

        Vector3 _middle = (figure_center + ground_center) / 2;

        _points = BezierCurve.CalculateCurvePoints(figure_center, _middle, ground_center, _points);

        if (LabelObject != null && LabelObject.gameObject.activeInHierarchy)
        {
            LabelObject.transform.position = _middle;
            LabelObject.transform.LookAt(new Vector3(0,0.5f, 3f));
            LabelObject.transform.Rotate(Vector3.up * 180, Space.Self);
        }

        Arrow.SetPositions(_points);

    }

 
    public void CreateObject(IsoEntity spatialEntity)
    {
        Debug.Log("Create Object: " + spatialEntity);


        Debug.Log("Create Object: " + spatialEntity.ID);
        if (spatialEntity.Object_ID == null)
        {
            Debug.LogWarning("Entity: " + spatialEntity.ID + " doeas not has an Object_ID!");
            _loading_obj = false;
            return;
        }

        if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_VOLUME))
        {
            Debug.Log("Abvstract Cube!!!");
            GameObject obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractCube")));
            obj.SetActive(true);
            obj.transform.position = spatialEntity.Position.Vector;
            obj.transform.rotation = spatialEntity.Rotation.Quaternion;
            obj.transform.localScale = spatialEntity.Scale.Vector;
            spatialEntity.Object3D = obj;
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_AREA))
        {
            Debug.Log("Abvstract Area!!!");
            GameObject obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractArea")));
            obj.SetActive(true);
            obj.transform.position = spatialEntity.Position.Vector;
            obj.transform.rotation = spatialEntity.Rotation.Quaternion;
            obj.transform.localScale = spatialEntity.Scale.Vector;
            spatialEntity.Object3D = obj;
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_LINE))
        {
            //_objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractLine")));
            //_objectInstance.SetActive(true);
            //_objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_POINT))
        {
            //_objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractPoint")));
            //_objectInstance.SetActive(true);
            //_objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_CYLINDER))
        {
            //_objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractCylinder")));
            //_objectInstance.SetActive(true);
            //_objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (UMAISOEntity.AVATAR_TYPE_NAMES.Contains(spatialEntity.Object_ID))
        {
        }
        else
        {
            LoadObject(spatialEntity);
            //_objectInstance = Instantiate(Builder.SceneBuilderControl.LoadedModels["" + spatialEntity.Object_ID][1]);
            //_objectInstance.SetActive(true);
            //_objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, true);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }

        //_objectInstance.layer = 19;

        //_objectInstance.transform.SetParent(Builder.SceneBuilderControl.ObjectContainer.transform, true);

    }


    public void LoadObject(IsoEntity entity)
    {
        _loading_obj = true;
        Debug.Log("Load Object: " + entity.Object_ID);
        ShapeNetInterface inter = gameObject.GetComponent<ShapeNetInterface>();

        ShapeNetModel shapeObj = inter.ShapeNetModels[entity.Object_ID];

        StartCoroutine(inter.GetModel((string)shapeObj.ID, (path) =>
        {
            Debug.Log("Scale & Reorientate Obj");
            GameObject GameObject = ObjectLoader.LoadObject(path + "\\" + shapeObj.ID + ".obj", path + "\\" + shapeObj.ID + ".mtl");
            GameObject GhostObject = ObjectLoader.Reorientate_Obj(GameObject, shapeObj.Up, shapeObj.Front, shapeObj.Unit);

            BoxCollider _collider = GhostObject.AddComponent<BoxCollider>();
            _collider.size = shapeObj.AlignedDimensions / 100;
            //_collider.center = Vector3.up * _collider.size.y / 2;

            LineRenderer lines = GhostObject.AddComponent<LineRenderer>();
            lines.enabled = false;



            GhostObject.transform.position = entity.Position.Vector;
            GhostObject.transform.rotation = entity.Rotation.Quaternion;
            GhostObject.transform.localScale = entity.Scale.Vector;
            entity.Object3D = GhostObject;
            _loading_obj = false;
            //Builder.SceneBuilderControl.LoadedModels.Add((string)ShapeNetObject.ID, new GameObject[2]);
            //Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][0] = GhostObject;
            //Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][1] = Instantiate(GhostObject);
            //Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][1].SetActive(false);
            //MakeGhostObject(GhostObject);
            //GhostObject.SetActive(ghostActive);
            //_collider.enabled = false;
            //OnModelLoaded();
            //_objectInstance = Instantiate(Instantiate(GhostObject));
            //_objectInstance.SetActive(true);
        }));
    }

}
