using System.Collections;
using Text2Scene.NeuralNetwork;
using TMPro;
using UnityEditor;
using UnityEngine;
using Valve.VR;

namespace Text2Scene
{
    /// <summary>
    /// Can be used to send documents to the resource manager via the editor
    /// </summary>
    public class TextObject : InteractiveObject
    {
        [Header("TextObject Area")]
        public string title = "Beispiel";
        [TextArea]
        public string text = "Four chairs are placed around a table";
        private TextMeshPro textComponent;
        public AnnotationDocument Document = null;
        private bool added = false;

        public override void Start()
        {
            base.Start();
            Debug.Log(title + text);
            OnDistanceGrab = PositionObject;
            textComponent = transform.GetChild(0).GetChild(3).GetChild(1).GetChild(1).GetComponent<TextMeshPro>();
            //((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().Examples.Add(this);
        }

        private void Update()
        {
            //if(!added)
            //{
            //    try
            //    {
            //        VRResourceData data = new VRResourceData(title, "", null, "txt", 0, GetInstanceID(), DateTime.Now, DateTime.Now, VRData.SourceType.InScene);
            //        data.TextContent = text;
            //        Debug.Log(title + GetInstanceID());
            //        ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().Examples.Add(data); //this
            //        added = true;
            //    } catch (NullReferenceException e)
            //    {

            //    }
            //}
            //if (text != null && textComponent.text != text) textComponent.text = title;
            //if (transform.parent != null) transform.GetChild(0).gameObject.SetActive(false);
            //else transform.GetChild(0).gameObject.SetActive(true);
        }

        public IEnumerator PositionObject()
        {
            //_draggedByPointer = true;
            transform.SetParent(StolperwegeHelper.User.PointerHand.transform);
            transform.position = StolperwegeHelper.PointerSphere.transform.position;
            float actualDistance, newDistance, touchInput; Vector3 lookDir;
            while (SteamVR_Actions.default_grab.GetState(StolperwegeHelper.User.PointerHandType))
            {
                lookDir = StolperwegeHelper.User.CenterEyeAnchor.transform.position;
                lookDir.y = transform.position.y;
                transform.LookAt(lookDir);
                touchInput = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y;
                if (touchInput != 0)
                {
                    actualDistance = (StolperwegeHelper.User.PointerHand.transform.position - transform.position).magnitude;
                    newDistance = Mathf.Max(0.5f, Mathf.Min(actualDistance + touchInput, 5));
                    transform.position -= StolperwegeHelper.PointerPath.transform.up * (newDistance - actualDistance) / 10;
                }
                yield return null;
            }
            transform.SetParent(null);
        }
    }

    [CustomEditor(typeof(TextObject))]
    public class TextObjectEditor : Editor
    {
        private TextAnnotatorInterface TextAnnotator
        {
            get { return SceneController.GetInterface<TextAnnotatorInterface>(); }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TextObject tO = (TextObject)target;
            if (GUILayout.Button("Send TextObjectData to Textannotator"))
            {
                NN_Helper.TextInputInteractionInScene.textInteraction.getXmi(tO.text, TextAnnotator, tO.title);
            }
        }
    }
}