using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangablePartScript : InteractiveObject
{
    public HashSet<string> allowedColors = new HashSet<string>();
    public bool colorSelection = false;
    private List<GameObject> colorBalls = new List<GameObject>();
    public Color originalColor = new Color();
    // Use this for initialization
    public override void Start()
    {
        base.Start();
        Grabable = false;
        ManipulatedByGravity = false;
        gameObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
        originalColor = gameObject.GetComponent<SkinnedMeshRenderer>().material.color;
        //switch (gameObject.name)
        //{
        //    case "maleavatarMesh":
        //        break;
        //    default:
        //        break;
        //}
    }
    // Update is called once per frame
 //   public override void Update ()
 //   {
 //       if (GetComponentInParent<AvatarObject>().IsGrabbed)
 //       {
 //           resetColor();
 //           DestroyColorBalls();
 //       }
 //       base.Update();	
	//}

    protected override void OnTriggerEnter(Collider other)
    {
        AvatarObject parentAvatarObject = GetComponentInParent<AvatarObject>();

        if (other.tag.Equals("leftIndexFinger") || other.tag.Equals("rightIndexFinger"))
        {
            if (!parentAvatarObject.IsGrabbed)
            {
                if (!colorSelection)
                {
                   
                    //AvatarObject[] avatarObjects = parentAvatarObject.parentScript.GetAvatarObjects();
                    //foreach (AvatarObject avatarObject in avatarObjects)
                    //{
                    //    if (avatarObject != parentAvatarObject && avatarObject.currentlyChangeing == true) avatarObject.resetParts();
                    //}


                    if (parentAvatarObject.currentlyChangeing != null)
                    {
                        parentAvatarObject.currentlyChangeing.DestroyColorBalls();
                        parentAvatarObject.currentlyChangeing.resetColor();
                    }
                    HighlightPart();
                    parentAvatarObject.currentlyChangeing = this;
                    int i = 0;
                    colorSelection = true;
                    foreach (string s in allowedColors)
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        go.transform.localScale = new Vector3(0.0045f, 0.0045f, 0.0045f);
                        go.transform.SetParent((StolperwegeHelper.findParentWithTag(parentAvatarObject.parentScript.transform, "leftArm")).transform.Find("ColorBallAnchor"), false);
                        //go.AddComponent<ColorBallScript>();
                        //go.GetComponent<ColorBallScript>().AssignedObject = gameObject;
                        //go.GetComponent<ColorBallScript>().color = s;
                        go.GetComponent<MeshRenderer>().material.color = StolperwegeHelper.ConvertHexToColor(s);
                        go.name = gameObject.name + "_ColorBall_" + s;
                        go.transform.localPosition = new Vector3(0, ((float)(i* 0.006)), 0);                    
                        colorBalls.Add(go);
                        i++;
                    }
                }
                else
                {
                    DestroyColorBalls();
                    resetColor();
                    GetComponentInParent<AvatarObject>().currentlyChangeing = null;
                }
            }  
        }
        base.OnTriggerEnter(other);
    }

    private void HighlightPart()
    {
        originalColor = gameObject.GetComponent<SkinnedMeshRenderer>().material.color;
        gameObject.GetComponent<SkinnedMeshRenderer>().material.color = Color.cyan;
    }

    public void resetColor()
    {
        gameObject.GetComponent<SkinnedMeshRenderer>().material.color = originalColor;
    }

    public void DestroyColorBalls()
    {
        foreach (GameObject go in colorBalls)
        {
            GameObject.Destroy(go);
        }
        colorBalls = new List<GameObject>();
        colorSelection = false; 
        
    }
    
}
