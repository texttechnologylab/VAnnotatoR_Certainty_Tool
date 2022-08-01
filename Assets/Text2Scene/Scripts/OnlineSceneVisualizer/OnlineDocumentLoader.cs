using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using MathHelper;


public class OnlineDocumentLoader : MonoBehaviour
{
    string documentid = "27146";
    //public string userid;
    public Transform objectContainer;

    TextAnnotatorInterface textAnnotatorInterface;
    ResourceManagerInterface resourceManagerInterface;
    UMAManagerInterface umaManagerInterface;
    ShapeNetInterface shapeNetInterface;


    void Start()
    {
        StolperwegeHelper.CenterEyeAnchor = gameObject;

        Init_Interfaces();

        Login();

        StartCoroutine(Load_Document());

        //StartCoroutine(Create_Scene());
    }

    private void Init_Interfaces()
    {
        Debug.Log("Init Interfaces");
        textAnnotatorInterface = gameObject.AddComponent<TextAnnotatorInterface>();
        StartCoroutine(textAnnotatorInterface.Initialize());

        shapeNetInterface = gameObject.AddComponent<ShapeNetInterface>();
        StartCoroutine(shapeNetInterface.Initialize());

        resourceManagerInterface = gameObject.AddComponent<ResourceManagerInterface>();
        textAnnotatorInterface.ResourceManager = resourceManagerInterface;

        umaManagerInterface = gameObject.AddComponent<UMAManagerInterface>();


    }

    private void Login()
    {
        Debug.Log("Login");
        StartCoroutine(textAnnotatorInterface.AutoLogin());
    }

    private IEnumerator Load_Document()
    {
        Debug.Log("Wait for Authorization ...");

        if (!textAnnotatorInterface.Authorized)
            yield return StartCoroutine(textAnnotatorInterface.StartAuthorization());

        Debug.Log("Load Document: " + documentid);
        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_cas, documentid);
        

        while (textAnnotatorInterface.ActualDocument == null || (textAnnotatorInterface.ActualDocument.CasId.Equals(documentid) && !textAnnotatorInterface.ActualDocument.ViewsLoaded))
            yield return null;
        Debug.Log("Document loaded");

        //foreach (string view in textAnnotatorInterface.ActualDocument.Views)
        //    Debug.Log(view);


        textAnnotatorInterface.FireJSONCommand(TextAnnotatorInterface.CommandType.open_tool, textAnnotatorInterface.ActualDocument.CasId, null, null, null, "https://authority.hucompute.org/user/306211");

        while (!textAnnotatorInterface.ActualDocument.DocumentCreated)
            yield return null;

        Debug.Log("View loaded");

        StartCoroutine(Create_Scene());
    }


    bool _loading_obj = false;
    private IEnumerator Create_Scene()
    {
        while (!textAnnotatorInterface.ActualDocument.DocumentCreated)
            yield return null;


        AnnotationDocument doc = textAnnotatorInterface.ActualDocument.Document;
        

        if (!umaManagerInterface.IsInitialized) umaManagerInterface.Initialize();
        // load the 3D-representation of all spatial-entities
        List<IsoEntity> entities = new List<IsoEntity>(doc.GetElementsOfTypeInRange<IsoEntity>(doc.Begin, doc.End, true));
        foreach (IsoEntity entity in entities)
        {
            // check the type of the object, if it is a ShapeNet object, make sure it is loaded
            if (entity.Object_ID != null && !entity.Object_ID.Equals("null"))
            {
                // Make sure the entity is not part of an UMA
                if (!UMAISOEntity.AVATAR_TYPE_NAMES.Contains(entity.Object_ID))
                {
                    CreateObject(entity);
                    while (_loading_obj)
                        yield return null;
                }
            }
        }

        // get all exisiting links and set them up between entities
        List<IsoLink> links = new List<IsoLink>(doc.GetElementsOfTypeInRange<IsoLink>(doc.Begin, doc.End, true));
        umaManagerInterface.AttributeEntityLinks = new List<IsoMetaLink>();
        foreach (IsoLink link in links)
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
                umaManagerInterface.AttributeEntityLinks.Add((IsoMetaLink)link);
                continue;
            }

            GameObject _objectInstance = new GameObject("Link_" + link.ID);
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveLinkObject>().Init(link);
            _objectInstance.transform.parent = objectContainer;

        }

        umaManagerInterface.BuildAvatars();
    }


    private void CreateObject(IsoEntity spatialEntity)
    {
        if (spatialEntity.Object_ID == null)
        {
            Debug.LogWarning("Entity: " + spatialEntity.ID + " doeas not has an Object_ID!");
            _loading_obj = false;
            return;
        }

        if (UMAISOEntity.AVATAR_TYPE_NAMES.Contains(spatialEntity.Object_ID))
            return;

        GameObject obj = null;
        if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_VOLUME))
            obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractCube")));

        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_AREA))
            obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractArea")));

        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_LINE))
            obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractLine")));
        
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_POINT))
            obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractPoint")));

        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_CYLINDER))
            obj = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractCylinder")));
        else
            LoadObject(spatialEntity);

        if (obj != null)
        {
            obj.SetActive(true);
            obj.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, false, false);
            obj.transform.position = spatialEntity.Position.Vector;
            obj.transform.rotation = spatialEntity.Rotation.Quaternion;
            obj.transform.localScale = spatialEntity.Scale.Vector;
            spatialEntity.Object3D = obj;
            obj.transform.parent = objectContainer;
        }
    }

    private void LoadObject(IsoEntity entity)
    {
        _loading_obj = true;
        Debug.Log("Load Object: " + entity.Object_ID);
        //ShapeNetInterface ShapeNetInterface = gameObject.GetComponent<ShapeNetInterface>();

        ShapeNetModel shapeObj = shapeNetInterface.ShapeNetModels[entity.Object_ID];

        StartCoroutine(shapeNetInterface.GetModel((string)shapeObj.ID, (path) =>
        {
            Debug.Log("Scale & Reorientate Obj");
            GameObject GameObject = ObjectLoader.LoadObject(path + "\\" + shapeObj.ID + ".obj", path + "\\" + shapeObj.ID + ".mtl");
            GameObject GhostObject = ObjectLoader.Reorientate_Obj(GameObject, shapeObj.Up, shapeObj.Front, shapeObj.Unit);

            BoxCollider _collider = GhostObject.AddComponent<BoxCollider>();
            _collider.size = shapeObj.AlignedDimensions / 100;
            //_collider.center = Vector3.up * _collider.size.y / 2;

            LineRenderer lines = GhostObject.AddComponent<LineRenderer>();
            lines.enabled = false;


            GameObject colliderDisplay = (GameObject)(Instantiate(Resources.Load("Prefabs/Frames/FrameCube")));
            colliderDisplay.transform.parent = GhostObject.transform;
            colliderDisplay.transform.localScale = _collider.size + new Vector3(0.001f, 0.001f, 0.001f); //Prevents Flickering
            colliderDisplay.transform.position = _collider.center;// + new Vector3(0f, 0.001f, 0f);
            colliderDisplay.SetActive(true);
            colliderDisplay.name = "FrameCube";


            GhostObject.AddComponent<InteractiveShapeNetObject>().Init(entity, false, true);

            GhostObject.transform.position = entity.Position.Vector;
            GhostObject.transform.rotation = entity.Rotation.Quaternion;
            GhostObject.transform.localScale = entity.Scale.Vector;
            GhostObject.transform.parent = objectContainer;
            //entity.Object3D = GhostObject;
            _loading_obj = false;

        }));
    }
}
