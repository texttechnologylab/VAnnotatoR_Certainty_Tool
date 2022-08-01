using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StolperwegeUnityBuilding : StolperwegeObject {

    bool editMode = false;

    //bool lastTriggerChange = false;
    bool currentTriggerChange = false;
    Collider currentCollider = null;
    int lastFrame = 0;
    public void CallOnTrigger(bool enter, Collider other)
    {
        if (lastFrame != Time.frameCount)
        {
            currentTriggerChange = false;
            lastFrame = Time.frameCount;
        }
            

        currentTriggerChange = currentTriggerChange || enter;
        currentCollider = (currentTriggerChange && !enter)? currentCollider : other;
    }

    public override void Awake()
    {
        base.Awake();

        UseHighlighting = false;

        foreach (Transform child in transform)
            child.gameObject.AddComponent<UnityBuildingSubMesh>();

        OnLongClick += SwitchEditMode;

        OnClick += () =>
        {
            if (editMode)
                CreateUnityPosition(Quaternion.Inverse(transform.rotation) * (StolperwegeHelper.PointerSphere.transform.position - transform.position));
        };
    }

    private void SwitchEditMode()
    {
        editMode = !editMode;

        foreach (UnityPosition uPos in ((UnityBuilding)Referent).GetUnityPositions())
        {
            if (editMode)
                PlaceElement(uPos, uPos.Position);
            else
                uPos.StolperwegeObject.gameObject.SetActive(false);
        }
    }

    private void PlaceElement(StolperwegeElement element, Vector3 pos)
    {
        if (element.StolperwegeObject == null) element.Draw();
        if (element.StolperwegeObject == null || element == Referent) return;

        element.StolperwegeObject.gameObject.SetActive(true);
        element.StolperwegeObject.transform.parent = transform;
        element.StolperwegeObject.transform.localPosition = pos;
        element.StolperwegeObject.SetParentObject(gameObject);

        foreach (StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
            Destroy(anchor);

        if (element is StolperwegeImage) {
            ExpandView ev = element.StolperwegeObject.OnExpand();
            ev.transform.parent.parent = transform;

            ev.transform.parent.LookAt(transform);
            Vector3 rot = ev.transform.parent.localEulerAngles;
            rot.x = 0;
            ev.transform.parent.localEulerAngles = rot;
        }
           
    }

    public void CreateUnityPosition(Vector3 pos)
    {
        Hashtable restParams = new Hashtable();
        restParams.Add("position", pos.x + ";" + pos.y + ";" + pos.z);
        restParams.Add("building", Referent.ID);

        StartCoroutine(StolperwegeInterface.CreateElement("unityposition", restParams, (StolperwegeElement e) => {
            if (editMode) PlaceElement(e, ((UnityPosition)e).Position);
        },true));
    }

    public override void Start()
    {
        base.Start();

        PlaceUnityPositions();
    }

    private void PlaceUnityPositions()
    {
        foreach(UnityPosition pos in ((UnityBuilding)Referent).GetUnityPositions())
        {
            foreach(StolperwegeElement element in pos.GetRelatedElementsByType(StolperwegeInterface.EquivalentRelation))
            {
                PlaceElement(element,pos.Position);
            }
        }
    }

    private ExpandView ActiveExpandView;
    /*
    public override void onPointerEnter(Collider other)
    {
        pointertriggered = true;
    }
    
    public override bool onPointerClick()
    {
        
        if (ActiveExpandView == null)
        {
            ActiveExpandView = onExpand();
            ActiveExpandView.DestroyWhithoutReferent = false;
        }

        GameObject container = ActiveExpandView.transform.parent.gameObject;

        container.transform.position = StolperwegeHelper.player.transform.position + StolperwegeHelper.centerEyeAnchor.transform.forward;

        container.transform.LookAt(centerEye);



        return base.onPointerClick();
    }

    
    */
    /*
    public override void onPointerExit()
    {
        pointertriggered = false;
    }*/

    public override ExpandView OnExpand()
    {

        GameObject container = new GameObject("ExpandViewContainer");
        dragging = false;
        Grabbing.Clear();

        ExpandView expandView = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperewegeExpandView"))).GetComponent<ExpandView>();
        expandView.StObject = this;

        container.transform.position = expandView.transform.position;
        expandView.transform.parent = container.transform;

        container.transform.position = transform.position;
        container.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        container.transform.eulerAngles = new Vector3(0, container.transform.eulerAngles.y + 180, 0);
        Vector3 diff = container.transform.position - StolperwegeHelper.CenterEyeAnchor.transform.position;
        diff.y = 0;
        diff.Normalize();
        container.transform.position += 0.5f * diff;
        UnityBuilding pReferent = ((UnityBuilding)Referent);

        Cite cite = pReferent.GetCitation();

        GameObject citeBox = ExpandView.createContentBox();

        if (cite != null)
        {
            GameObject bCite = ExpandView.createText(cite.HowToCite);
            bCite.transform.parent = citeBox.transform;
            bCite.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.2f + Vector3.forward * -0.4f;
            bCite.transform.localScale = Vector3.up * 0.025f + Vector3.right * 0.025f + Vector3.forward * 0.025f;
            bCite.transform.localEulerAngles = Vector3.right * 90f;
        }

        GameObject bDesc = ExpandView.createText(pReferent.Description);
        bDesc.transform.parent = citeBox.transform;
        bDesc.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.0f + Vector3.forward * 0.25f;
        bDesc.transform.localScale = Vector3.up * 0.025f + Vector3.right * 0.025f + Vector3.forward * 0.025f;
        bDesc.transform.localEulerAngles = Vector3.right * 90f;

        GameObject bName = ExpandView.createText(pReferent.Value);
        bName.transform.parent = citeBox.transform;
        bName.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.0f + Vector3.forward * 0.42f;
        bName.transform.localScale = Vector3.up * 0.045f + Vector3.right * 0.045f + Vector3.forward * 0.045f;
        bName.transform.localEulerAngles = Vector3.right * 90f;
        bName.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;


        expandView.drawComponent(citeBox.transform, ExpandView.LAYOUT.FULL);

        expandView.OnClick += () => { Destroy(container); };

        return expandView;
    }
}
