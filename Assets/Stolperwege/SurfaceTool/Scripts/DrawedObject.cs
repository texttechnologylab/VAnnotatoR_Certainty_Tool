using HTC.UnityPlugin.StereoRendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawedObject : InteractiveObject {
    
    public Vector3 Center { get; private set; }
    public float SurfaceRadius { get; private set; }
    public GameObject Surface { get; private set; }

    public void Init(SurfaceToolInterface._DrawType drawType, Vector3 normal, Vector3 center, float radius, GameObject surface)
    {
        //Grabable = true;
        //DestroyOnObjectRemover = true;
        //SurfaceToolInterface Interface = SceneController.GetInterface<SurfaceToolInterface>();
        //OnClick = () => {
        //    if (!Interface.IOEditor.Active) Interface.IOEditor.Active = true;
            
        //    if (Interface.IOEditor.ObjectToEdit != null &&
        //        Interface.IOEditor.ObjectToEdit.name.Equals(name))
        //    {
        //        Interface.IOEditor.Active = false;
        //        return;
        //    }
        //    Interface.IOEditor.SetGameObject(this);
        //};
        //PartsToHighlight = new List<Renderer> { GetComponent<MeshRenderer>() };
        //SearchForParts = false;
        //DrawType = drawType;
        //Center = center;
        //Normal = normal;
        //SurfaceRadius = radius;
        //Surface = surface;
        //if (GetComponent<CustomInteractiveObject>() != null)
        //    Destroy(GetComponent<CustomInteractiveObject>());
        //if (!Interface.InSurfaceMode && Interface.IOEditor.Active)
        //    Interface.IOEditor.SetGameObject(this);
    }   

}
