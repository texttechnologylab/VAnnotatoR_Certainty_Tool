using System.Collections;
using HTC.UnityPlugin.StereoRendering;
using MathHelper;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class PointFinger : MonoBehaviour
{

    public abstract bool IsPointing { get;} 
    protected abstract Vector3 PointingDirection { get;} //Zeigerichtung
    protected abstract Vector3 FingerTip { get; } //Position der Fingerspitze
    public abstract bool LeftHand { get; } //True, falls diese Hand die Linke, False, falls es die Rechte ist
    public abstract bool RightHand { get; } //True, falls diese Hand die Linke, False, falls es die Rechte ist
    public abstract bool IsClicking { get; } //True, falls die Aktion des Pointers ausgelöst werden soll
    public abstract bool IsHoldingClick { get; } //True, wenn der Button zum klicken gedrückt wird
    public abstract bool IsReleasing { get; } //True, wenn der Button zum klicken losgelassen wird

    private static Material _lineMat;
    public static Material LineMaterial
    {
        get
        {
            if (_lineMat == null)
                _lineMat = (Material)(Instantiate(Resources.Load("Surface2Info/materials/SurfaceGridMaterial")));
            return _lineMat;
        }
    }

    public Transform RaySphere { get { return StolperwegeHelper.PointerSphere.transform; } }
    private Transform Path { get { return StolperwegeHelper.PointerPath.transform; } }
    public InteractiveObject Hit { get; private set; }
    public GameObject HittedObject { get; private set; }
    protected Vector3 lastPosition;
    protected float lastPositionTime;
    public Ray Ray { get; private set; }
    public RaycastHit RayCastHit;
    public bool DisableRay;
    
    protected virtual void Update()
    {

        if (Equals(StolperwegeHelper.User.PointerHand))
        {

            if (IsPointing && !DisableRay)
            {
                CheckRay();
                if (Hit != null)
                {
                    Hit.OnPointer?.Invoke(RaySphere.transform.position);
                    StolperwegeHelper.User.RotationBlocked = Hit.BlockRotationOnPointer;
                    Hit.Highlight = true;
                    Hit.CheckClick();
                }
            }
            else RayDisable(true);
            
        }
        else
        {
            if (Hit != null)
            {
                Hit.Highlight = false;
                if (Hit.BlockRotationOnPointer) StolperwegeHelper.User.RotationBlocked = false;
                Hit.OnPointerExit();
                Hit = null;
            }
        }
        
    }


    //Aktualisiert den Pointer    
    private void CheckRay()
    {
        if (RaySphere == null) return;
        
        Ray = new Ray(FingerTip, PointingDirection);

        Path.gameObject.SetActive(true);
        RaySphere.gameObject.SetActive(true);

        if (Physics.Raycast(Ray, out RayCastHit, float.PositiveInfinity))
        {
            
            HittedObject = RayCastHit.transform.gameObject;
            lastPosition = RayCastHit.point;
            lastPositionTime = Time.time;

            if (RayCastHit.collider.gameObject != this)
                RaySphere.position = RayCastHit.point;                
            

            if (RayCastHit.collider.gameObject.GetComponent<InteractiveObject>() != null)
            {
                
                if (Hit != RayCastHit.collider.GetComponent<InteractiveObject>())
                {
                    if (Hit != null)
                    {
                        Hit.Highlight = false;
                        Hit.OnPointerExit();
                        Hit = null;
                    }
                    Hit = RayCastHit.collider.GetComponent<InteractiveObject>();
                    Hit.OnPointerEnter(Hit.GetComponent<Collider>());
                }

            } else if (Hit != null)
            {
                Hit.Highlight = false;
                Hit.OnPointerExit();
                Hit = null;
            }

            //Teleportation
            if (IsClicking)
                StolperwegeHelper.User.GetComponent<AvatarController>().Teleport(RayCastHit.point, 0.1f);
            
        }
        else
        {
            if (Hit != null)
            {
                Hit.Highlight = false;
                Hit.OnPointerExit();
                Hit = null;
            }
            RaySphere.position = FingerTip + PointingDirection * 20;
            HittedObject = null;
        }
            

        //Transformiert den Pointer Pfad
        Path.position = RaySphere.position + (transform.position - RaySphere.position) * 0.5f;
        Path.up = (transform.position - RaySphere.position);
        Path.localScale = new Vector3(0.003f, (transform.position - RaySphere.position).magnitude / 2, 0.003f);
    }
    
    RaycastHit _boxHit; Vector3 _boxCenter; int layerMask = 1 << 19;
    public RaycastHit BoxCast(Vector3 boxSize, Quaternion boxRotation)
    {
        _boxCenter = transform.position + Vector3.up * boxSize.y / 2;// + PointingDirection * 0.1f;
        _boxHit = default;
        if (Physics.BoxCast(_boxCenter, boxSize / 2, Ray.direction, out _boxHit, boxRotation, Mathf.Infinity, layerMask))
            return _boxHit;        
        return default;
    }

    private void RayDisable(bool exitLastHit)
    {
        if (RaySphere.gameObject.activeInHierarchy)
        {
            if (exitLastHit && Hit != null)
            {
                Hit.Highlight = false;
                Hit.OnPointerExit();
                Hit = null;
            }
            
            RaySphere.gameObject.SetActive(false);
            Path.gameObject.SetActive(false);
        }
        return;
    }

    public void OnTriggerEnter(Collider other)
    {

        if (IsPointing && other.GetComponent<PointFinger>() != null &&
            other.GetComponent<PointFinger>().IsPointing && LeftHand)
            StolperwegeHelper.User.CreateARDisplay();

        //if (StolperwegeHelper.player.IsRecognizingSurface && other.GetComponent<InteractiveObject>() == null &&
        //    !StolperwegeHelper.player.SurfaceRecognizer.Processing)
        //    StolperwegeHelper.player.SurfaceRecognizer.SelectedObject = hit.transform.gameObject;


    }

    private Vector3 lastRegulatorPosition; private float regulatorRotAngle;
    public void OnTriggerStay(Collider other)
    {
        if (other.name.Equals("Regulator"))
        {
            regulatorRotAngle = Vector3.SignedAngle((lastRegulatorPosition - other.transform.position), (transform.position - other.transform.position), other.transform.up);
            Debug.Log(regulatorRotAngle + " " + other.transform.up);
            other.transform.Rotate(other.transform.up, regulatorRotAngle);
        }
    }

    public static void SetPointerColor(Vector4 color)
    {
        StolperwegeHelper.PointerPath.GetComponent<Renderer>().material.color = color;
        StolperwegeHelper.PointerSphere.GetComponent<Renderer>().material.color = color;
    }

    public static void SetPointerEmissionColor(Vector4 color)
    {
        StolperwegeHelper.PointerPath.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
        StolperwegeHelper.PointerSphere.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    public bool HitsObject(InteractiveObject io)
    {
        return Hit != null && Hit.Equals(io);
    }
}