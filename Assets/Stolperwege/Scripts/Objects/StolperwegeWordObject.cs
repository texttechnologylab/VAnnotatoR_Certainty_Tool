using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StolperwegeWordObject : StolperwegeObject {

    TextMeshPro textMesh;
    GameObject background;


    public DiscourseReferent ConnectedReferent { get; set; }
    public StolperwegeSubTextObject SubText { get; set; }

	// Use this for initialization
	public override void Awake () {
        base.Awake();
        textMesh = GetComponentInChildren<TextMeshPro>();
        textMesh.color = Color.black;
        background = transform.Find("Cube").gameObject;
        //blockOutline = true;
	}

    public override void Start()
    {
        base.Start();

        OnClick = PointerClick;
    }

    float lastClickTime = 0;
    private void PointerClick()
    {
        if (Time.time - lastClickTime < 1) return;
        lastClickTime = Time.time;

        InteractiveObject grabbedObj = StolperwegeHelper.LeftFist.GrabedObject;
        if (grabbedObj == null) grabbedObj = StolperwegeHelper.RightFist.GrabedObject;

        if(grabbedObj == null || !(grabbedObj is StolperwegeObject) || !(((StolperwegeObject)grabbedObj).Referent is DiscourseReferent))
        {
            if(SubText.MainTextObject.ActivatedPredicate != null)
            {
                SubText.MainTextObject.ActivatedPredicate.ShowAddRolesMenu(this);               
            }
            else
            {
                CreateDRMenu();
            }
            return;
        }

        StartCoroutine(RelateWithDR((DiscourseReferent)((StolperwegeObject)grabbedObj).Referent));
    }

    public IEnumerator CreateDRForWord()
    {
        Hashtable paramsDR = new Hashtable();

        paramsDR.Add("begin", Range.x);
        paramsDR.Add("end", Range.y);
        paramsDR.Add("value", Text);

        yield return StolperwegeInterface.CreateElement("discoursereferent", paramsDR, (StolperwegeElement se) => { ConnectedReferent = (DiscourseReferent)se; });

        if (ConnectedReferent == null) yield break;

        yield return ConnectedReferent.AddStolperwegeRelationAsynch(Referent, StolperwegeInterface.EquivalentRelation);
    }

    private IEnumerator RelateWithDR(DiscourseReferent dr) {
        foreach (StolperwegeObject stObj in GetComponentsInChildren<StolperwegeObject>())
            if (stObj.Referent == dr) yield break;

        if(ConnectedReferent == null)
        {
            yield return CreateDRForWord();

            if (ConnectedReferent == null) yield break;
        }

        yield return ConnectedReferent.AddStolperwegeRelationAsynch(dr, StolperwegeInterface.EquivalentRelation);

        SubText.MainTextObject.AddStolperwegeElement(this, dr);
    }

    public void RemoveDR(StolperwegeObject stObj)
    {

        foreach(StolperwegeElement se in ConnectedReferent.GetRelatedElementsByType(StolperwegeInterface.EquivalentRelation))
        {
            if(se == stObj.Referent)
            {
                se.RmvStolperwegeRelation(ConnectedReferent, StolperwegeInterface.EquivalentRelation);
                SubText.MainTextObject.RmvStolperwegeElement(stObj, this);

                return;
            }
        }
    }

    float time;

    public Vector2 Range { get; set; }

    public override void ResetObject()
    {
        transform.parent = null;
        gameObject.SetActive(false);
    }

    public string Text {

        get
        {
            return textMesh.text;
        }

        set
        {
            string val = value.Replace("\n", " ");
            if (val.Length <= 0) return;

            if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();

            textMesh.text = val;

            textMesh.ForceMeshUpdate();
            StartCoroutine(UpdateMesh());
        }

    }

    private bool _highlighted = false;

    public bool Highlighted
    {
        get
        {
            return _highlighted;
        }

        set
        {
            if (_highlighted == value) return;

            _highlighted = value;

            Color c = (_highlighted) ? new Color(0, 0, 1) : new Color(1, 1, 1);

            transform.Find("Cube").GetComponent<MeshRenderer>().material.color = c;

            foreach(StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
            {
                anchor.Relation.Highlight = value;
            }
        }
    }

    private static CircleMenu menu;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PointFingerColliderScript>() != null)
        {
            StolperwegeRelationAnchor anchor = other.GetComponentInChildren<StolperwegeRelationAnchor>();

            if (anchor != null)
            {
                //StolperwegeWordObject word = anchor.OtherEnd.GetComponentInParent<StolperwegeWordObject>();

                //if (word != null && word.Referent == this.Referent)
                //{
                //    if (word.Range.x == Range.y + 1 || word.Range.y == Range.x - 1)
                //        Merge(new StolperwegeWordObject[] { word, this });
                //}

                return;
            }

            GameObject path = (GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"));

            Transform start = path.transform.Find("StartAnchor").transform, end = path.transform.Find("EndAnchor").transform;

            start.parent = transform;
            start.localPosition = Vector3.back * -0.14f;
            end.parent = other.transform;
            end.localPosition = Vector3.zero;

            path.GetComponent<StolperwegeRelation>().DoOnClick = () =>
            {
               
                if (StolperwegeHelper.LeftFinger.GetComponentInChildren<StolperwegeRelationAnchor>() != null && StolperwegeHelper.RightFinger.GetComponentInChildren<StolperwegeRelationAnchor>() != null)
                {
                    StolperwegeRelationAnchor rightAnchor = (StolperwegeHelper.LeftFinger.transform == end.transform.parent)?
                        StolperwegeHelper.RightFinger.GetComponentInChildren<StolperwegeRelationAnchor>() : StolperwegeHelper.LeftFinger.GetComponentInChildren<StolperwegeRelationAnchor>();
                    StolperwegeWordObject word = rightAnchor.OtherEnd.GetComponentInParent<StolperwegeWordObject>();
                    if (word != null && Range.x < word.Range.x && word.Referent == Referent)
                    {
                        NewElement newDummy = StolperwegeInterface.CreateElementDummy("org.hucompute.publichistory.datastore.typesystem.DiscourseReferent").GetComponent<NewElement>();

                        Vector3 pos = (rightAnchor.transform.position + end.transform.position) / 2;

                        newDummy.transform.position = pos;

                        string value = "";

                        foreach (StolperwegeWordObject w in ((StolperwegeTextObject)Referent.StolperwegeObject).GetWords((int)Range.x, (int)word.Range.y))
                            value += w.Text + " ";

                        newDummy.SetProperty("value", value);

                        end.transform.parent = newDummy.transform;
                        end.transform.localPosition = Vector3.zero;

                        //Destroy(Helper.rightFinger.GetComponentInChildren<StolperwegeRelationAnchor>().Relation.GetComponentInChildren<CircleMenu>().gameObject);

                        rightAnchor.transform.parent = newDummy.transform;
                        rightAnchor.transform.localPosition = Vector3.zero;


                        newDummy.UploadThis();
                    }

                    return;
                }

                CreateDRMenu();
            };

            return;
        }

        base.OnTriggerEnter(other);

        //StolperwegeWordObject wordObj = other.GetComponent<StolperwegeWordObject>();
        //if (wordObj != null && wordObj.Referent == this.Referent && wordObj.grabbing.Count > 0 && grabbing.Count > 0)
        //{
        //    if (wordObj.Range.x == Range.y+1|| wordObj.Range.y == Range.x -1)
        //        Merge(new StolperwegeWordObject[]{ wordObj,this});
        //}
    }



    public void CreateDRMenu()
    {
        if (menu != null) return;
        menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

        //menu.transform.parent = path.transform;

        //Hashtable types = new Hashtable();
        //foreach (string type in StolperwegeInferface.typeSystemTable.Keys)
        //    if (type.StartsWith("org.hucompute"))
        //        types.Add(type.Replace("org.hucompute.publichistory.datastore.typesystem.", ""), StolperwegeInferface.typeSystemTable[type]);

        Hashtable types = new Hashtable();
        types.Add("DiscourseReferent", StolperwegeInterface.TypeSystemTable["DiscourseReferent"]);
        types.Add("Predicate", StolperwegeInterface.TypeSystemTable["Predicate"]);

        menu.Init((string key, object o) => {
            if (key.Contains("Referent"))
            {
                CircleMenu menuDR = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

                //Hashtable types = new Hashtable();
                //foreach (string type in StolperwegeInferface.typeSystemTable.Keys)
                //    if (type.StartsWith("org.hucompute"))
                //        types.Add(type.Replace("org.hucompute.publichistory.datastore.typesystem.", ""), StolperwegeInferface.typeSystemTable[type]);

                Hashtable typesDR = new Hashtable();

                HashSet<string> outTypes = ConnectedReferent.getOutgoingTypes("org.hucompute.publichistory.datastore.typesystem.DiscourseReferent");

                foreach (string str in outTypes)
                {
                    typesDR.Add(str, StolperwegeInterface.TypeClassTable[str]);
                }

                menuDR.Init((string key2, object o2) => {
                    GameObject dummy = StolperwegeInterface.CreateElementDummy("org.hucompute.publichistory.datastore.typesystem." + key2);
                    dummy.transform.position = menuDR.transform.position;
                    Destroy(menuDR.gameObject);
                }, typesDR);

                menuDR.transform.position = menu.transform.position;
                menuDR.transform.forward = -StolperwegeHelper.CenterEyeAnchor.transform.forward;
                menuDR.transform.localScale = Vector3.one * 0.2f;

                Destroy(menu.gameObject);
            }
            else
            {
                StartCoroutine(Proposition.CreateForPredicate((StolperwegeText)Referent, Text, (int)Range.x, (int)Range.y, (Predicate p) => {
                    SubText.MainTextObject.AddPredicate(p);
                }));
            }
            
        }, types);

        StolperwegeHelper.PlaceInFrontOfUser(menu.transform, 0.4f);
        menu.transform.localScale = Vector3.one * 0.2f;
    }

    private IEnumerator UpdateMesh()
    {
        yield return new WaitForEndOfFrame();

        if (background == null) background = transform.Find("Cube").gameObject;

        background.transform.localPosition = Vector3.forward * 0.15f;
        background.transform.localScale = new Vector3(textMesh.textBounds.extents.x + 0.1f, textMesh.textBounds.extents.y, 0.1f) * 2;
        GetComponent<BoxCollider>().size = background.transform.localScale;
        GetComponent<BoxCollider>().center = background.transform.localPosition;
    }

    public override ExpandView OnExpand()
    {
        ExpandView result = base.OnExpand();
        GameObject text = ExpandView.createText(Referent.Value);
        text.GetComponent<TextMeshPro>().overflowMode = TextOverflowModes.Ellipsis;
        text.GetComponent<TextMeshPro>().margin = new Vector4(0, 0, 0, -15);

        text.transform.SetParent(result.transform,false);
        text.transform.localPosition = new Vector3(0.01f, -1f, 0.35f);
        text.transform.localEulerAngles = Vector3.right * 90;
        text.transform.localScale = new Vector3(-0.045f, 0.045f,0.01f);
        result.ScaleMultiplier = new Vector2(0.5f, 0.5f);
        result.StartScale = result.transform.localScale;

        //transform.localPosition = Vector3.down * 0.5f;
        transform.localEulerAngles = Vector3.zero;
        
        return result;
    }

    public static StolperwegeWordObject Merge(StolperwegeWordObject[] words)
    {

        Vector3 pos = Vector3.zero;
        string str = "";

        Array.Sort(words, (StolperwegeWordObject obj1, StolperwegeWordObject obj2)=> { return obj1.Range.x.CompareTo(obj2.Range.x); });

        foreach(StolperwegeWordObject word in words)
        {
            pos += word.transform.position;
            str += word.Text + " ";
        }

        pos /= words.Length;

        str.Remove(str.Length - 1);

        for (int i = 1; i < words.Length; i++)
            Destroy(words[i].gameObject);

        words[0].transform.position = pos;
        words[0].Text = str;

        words[0].Range = new Vector2(words[0].Range.x, words[words.Length - 1].Range.y);

        return words[0];
    }

    private void OnDisable()
    {
        foreach (StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
        {
            anchor.transform.parent = Referent.StolperwegeObject.transform;
            anchor.transform.localPosition = Vector3.zero;
        }
            
    }
}
