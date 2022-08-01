using System.Collections.Generic;
using Text2Scene.NeuralNetwork;
using TMPro;
using UnityEngine;

namespace Text2Scene
{
    /// <summary>
    /// Places ShapeNet objects in the scene and connects them with the isoentity
    /// </summary>
    public class ObjectPlacer : MonoBehaviour
    {
        private List<GameObject> loadedObjects;
        public static SceneBuilder Builder;
        private GameObject GhostObject;
        private ShapeNetModel _shapeNetObject;
        private int counter = -1;

        private void Start()
        {
            loadedObjects = new List<GameObject>();
        }

        /// <summary>
        /// Loads all ShapeNet Objects in the scene
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void LoadModels(int start, int end)
        {
            NN_Helper.TextInputInteractionInScene.objectsPlaced = true;
            Vector3 offset = Vector3.zero;
            for (int i = start; i < end; i++)
            {
                ClassifiedObject entity = NN_Helper.isoSpatialEntities[i];
                StartCoroutine(ShapeNetInterface.ObjectSearchRequest(entity.Praefix + " " + entity.Holonym + " " + entity.DisambiguationWord, (objectList) =>
                {
                    if (objectList.Count > 0)
                    {
                        GhostObject = null;
                        string id = objectList[0];
                        _shapeNetObject = SceneController.GetInterface<ShapeNetInterface>().ShapeNetModels[id];
                        StartCoroutine(SceneController.GetInterface<ShapeNetInterface>().GetModel(id, (path) =>
                        {
                            GameObject _object = ObjectLoader.LoadObject(path + "\\" + _shapeNetObject.ID + ".obj", path + "\\" + _shapeNetObject.ID + ".mtl");
                            if (_object == null)
                            {
                                Debug.Log(entity.Comment + " wasn't found and can't be loaded");
                                return;
                            }
                            GhostObject = ObjectLoader.Reorientate_Obj(_object, _shapeNetObject.Up, _shapeNetObject.Front, _shapeNetObject.Unit);
                            GhostObject.name = entity.Praefix + " " + entity.Holonym + " " + entity.DisambiguationWord;
                            if (loadedObjects.Count == 0) GhostObject.transform.position = new Vector3(StolperwegeHelper.CenterEyeAnchor.transform.position.x, 1, StolperwegeHelper.CenterEyeAnchor.transform.position.z) - StolperwegeHelper.CenterEyeAnchor.transform.right + 2 * StolperwegeHelper.CenterEyeAnchor.transform.forward;
                            else GhostObject.transform.position = loadedObjects[counter].transform.position + 2 * loadedObjects[counter].transform.right;
                            loadedObjects.Add(GhostObject);
                            counter++;
                            BoxCollider _collider = GhostObject.AddComponent<BoxCollider>();
                            _collider.size = _shapeNetObject.AlignedDimensions / 100;

                            GameObject colliderDisplay = (GameObject)(Instantiate(Resources.Load("Prefabs/Frames/FrameCube")));
                            colliderDisplay.transform.position = GhostObject.transform.position;
                            colliderDisplay.transform.parent = GhostObject.transform;
                            colliderDisplay.transform.localScale = _collider.size + new Vector3(0.001f, 0.001f, 0.001f); //Prevents Flickering

                            colliderDisplay.SetActive(true);
                            colliderDisplay.name = "FrameCube";

                            MakeGhostObject(GhostObject);
                            GhostObject.SetActive(true);
                            entity.SetShapeNetObject(GhostObject, id);
                            GhostObject.AddComponent<InteractiveShapeNetObject>().Init(entity, true, true);
                            InteractiveShapeNetObject iO = GhostObject.GetComponent<InteractiveShapeNetObject>();
                            _collider.enabled = true;
                        }));

                        Debug.Log("Finished Execution " + entity.Comment);
                    }
                    else
                    {
                        Debug.Log("No object was found");
                    }
                }));
            }
        }

        public void DestroyObjects()
        {
            loadedObjects.ForEach(x => x.Destroy());
            loadedObjects.Clear();
            counter = -1;
        }

        public void LoadModel(string id, bool ghostActive = true)
        {
            GhostObject = null;
            _shapeNetObject = SceneController.GetInterface<ShapeNetInterface>().ShapeNetModels[id];
            StartCoroutine(SceneController.GetInterface<ShapeNetInterface>().GetModel((string)_shapeNetObject.ID, (path) =>
            {
                GameObject _object = ObjectLoader.LoadObject(path + "\\" + _shapeNetObject.ID + ".obj", path + "\\" + _shapeNetObject.ID + ".mtl");
                GhostObject = ObjectLoader.Reorientate_Obj(_object, _shapeNetObject.Up, _shapeNetObject.Front, _shapeNetObject.Unit);
                GhostObject.transform.parent = Builder.SceneBuilderControl.ObjectContainer.transform;

                BoxCollider _collider = GhostObject.AddComponent<BoxCollider>();
                _collider.size = _shapeNetObject.AlignedDimensions / 100;

                LineRenderer lines = GhostObject.AddComponent<LineRenderer>();
                lines.enabled = false;

                GameObject colliderDisplay = (GameObject)(Instantiate(Resources.Load("Prefabs/Frames/FrameCube")));
                colliderDisplay.transform.parent = GhostObject.transform;
                colliderDisplay.transform.localScale = _collider.size + new Vector3(0.001f, 0.001f, 0.001f); //Prevents Flickering
                colliderDisplay.transform.position = _collider.center;
                colliderDisplay.SetActive(true);
                colliderDisplay.name = "FrameCube";

                Builder.SceneBuilderControl.LoadedModels.Add((string)_shapeNetObject.ID, new GameObject[2]);
                Builder.SceneBuilderControl.LoadedModels[(string)_shapeNetObject.ID][0] = GhostObject;
                Builder.SceneBuilderControl.LoadedModels[(string)_shapeNetObject.ID][1] = Instantiate(GhostObject);
                Builder.SceneBuilderControl.LoadedModels[(string)_shapeNetObject.ID][1].SetActive(false);
                MakeGhostObject(GhostObject);
                GhostObject.SetActive(ghostActive);
                _collider.enabled = false;
            }));
        }

        public static void MakeGhostObject(GameObject go)
        {
            MeshRenderer[] _renderers = go.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].GetComponent<TextMeshPro>() != null ||
                    _renderers[i].GetComponent<TMP_SubMesh>() != null ||
                    !_renderers[i].material.HasProperty("_BaseColor")) continue;
                Color _color = _renderers[i].material.GetColor("_BaseColor");
                _color.a = 0.6f;
                _renderers[i].material.SetColor("_BaseColor", _color);
            }
        }
    }
}