using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectionPoint : MonoBehaviour {

    /// <summary>
    /// Stellt eine mögliche Relation oder ein Attribut einer Element Maske dar
    /// </summary>

    private object _value;
    public object Value {
        get
        {
            if(_value != null && _value is GameObject && ((GameObject)_value).GetComponent<NewElement>().Referent != null)
            {
                return ((GameObject)_value).GetComponent<NewElement>().Referent.ID;
            }

            return _value;
        }

        set
        {
            _value = value;

            if (Mandatory() && (_value == null || ((_value.GetType() == typeof(string)) && ((string)_value).Length <= 0)))
                Ready = false;
            else
                Ready = true;

            if(value!= null) LabelValueText = Value.ToString();
        }
    }

    public bool Property { get; set; }
    private bool _ready = true;
    public bool Ready
    {
        get
        {
            return _ready;
        }

        set
        {
            _ready = value;

            if (_ready)
            {
                GetComponent<MeshRenderer>().material.color = StolperwegeHelper.GUCOLOR.LICHTBLAU;
                if(GetComponentInParent<NewElement>().Ready) GetComponentInParent<NewElement>().ChangeColor(Color.green);
            }
            else
            {
                GetComponent<MeshRenderer>().material.color = Color.red;
                GetComponentInParent<NewElement>().ChangeColor(Color.red);
            }
                
        }

    }


    private GameObject _label;
    private GameObject _editText;
    private GameObject _relation;
    private CircleMenu _menu;
    // never used
    //private int startLength;

    private StolperwegeInterface.RelationType _type;
    public StolperwegeInterface.RelationType Type {
        get
        {
            return _type;
        }

        set
        {
            _type = value;

            setLabel(_type.title + "\n[" + _type.to.Name + "]");
            Property = _type.type == StolperwegeInterface.RelType.PROPERTY;

            Ready = !Mandatory(); 
        }
    }

    private bool Mandatory()
    {
        string typeStr = GetComponentInParent<NewElement>().Type;
        if (!SceneController.GetInterface<StolperwegeInterface>().ApiTable.Contains(typeStr)) return false;
        Hashtable apiType = (Hashtable)SceneController.GetInterface<StolperwegeInterface>().ApiTable[typeStr];

        if (!apiType.Contains(Type.title)) return false;

        return (bool)apiType[Type.title];
    }

    public void setLabel(string str)
    {
        if(_label == null)_label = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeLabel")));
        _label.GetComponent<TextMeshPro>().text = str;
        // never used
        //startLength = label.GetComponent<TextMeshPro>().text.Length;
        _label.GetComponent<InverseRotation>().hover = gameObject;
        _label.GetComponent<InverseRotation>().Scale = 0.2f;
        _label.GetComponent<InverseRotation>().Textsize = 0.5f;
    }

    private string LabelValueText
    {
        set
        {
            _label.GetComponent<TextMeshPro>().text = Type.title + "\n[" + Type.to.Name + "]" + "\n" + value;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PointFingerColliderScript>() != null)
        {

            if (_editText != null) Destroy(_editText);
            StolperwegeRelationAnchor anchor;

            if (Property)
            {
                if (Type.to == typeof(bool))
                {
                    if (Value is bool)
                        Value = !((bool)Value);
                    else
                        Value = true;

                    
                }
                else if ((anchor = other.GetComponentInChildren<StolperwegeRelationAnchor>()) != null)
                {
                    StolperwegeWordObject stText;
                    if ((stText = anchor.Relation.StartAnchor.transform.parent.GetComponent<StolperwegeWordObject>()) != null)
                    {
                        if (Type.to == typeof(string))
                        {
                            Value = stText.Text;
                        }

                        Destroy(anchor);
                    }

                    StolperwegeTextObject stTextObj;
                    if ((stTextObj = anchor.Relation.StartAnchor.transform.parent.GetComponent<StolperwegeTextObject>()) != null)
                    {
                        if (Type.to == typeof(string))
                        {
                            Value = stText.Referent.Value;
                        }

                        Destroy(anchor);
                    }
                }

                else if (Type.to == typeof(string) || Type.to == typeof(double) || Type.to == typeof(int))
                {

                    StolperwegeHelper.VRWriter.Interface.DoneClicked += (string str) => {
                        Value = str;
                        LabelValueText = str;
                    };
     
                    StolperwegeHelper.VRWriter.Active = true;

                    
                    _editText = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/EditTextObject")));

                    _editText.transform.parent = transform.parent;
                    _editText.transform.localPosition = transform.localPosition * 1.5f;
                    _editText.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
                    _editText.transform.localScale = Vector3.one * 0.75f;
                    _editText.GetComponentInChildren<KeyboardEditText>().OnlyActiveAllowed = true;

                    //_editText.GetComponentInChildren<KeyboardEditText>().ActivateWriter();                    
                    if (Value != null)
                        _editText.GetComponentInChildren<KeyboardEditText>().Text = Value.ToString();
                    if (!(Type.to == typeof(string)))
                        _editText.GetComponentInChildren<KeyboardEditText>().IsNumberField = true;

                    StolperwegeHelper.VRWriter.Inputfield = _editText.GetComponentInChildren<KeyboardEditText>();

                    _editText.GetComponentInChildren<KeyboardEditText>().OnCommit = (string s, GameObject go) =>
                    {
                        Value = s;
                        LabelValueText = s;
                        StolperwegeHelper.VRWriter.Active = false;
                        Destroy(_editText);
                    };
                }
            }
            else
            {
                if ((anchor = other.GetComponentInChildren<StolperwegeRelationAnchor>()) != null)
                {

                    StolperwegeObject obj;
                    if ((obj = anchor.Relation.StartAnchor.parent.GetComponent<StolperwegeObject>()) != null)
                    {
                        if ((obj.Referent.GetType() == Type.to || obj.Referent.GetType().IsSubclassOf(Type.to))&& !(obj is StolperwegeWordObject))
                        {
                            anchor.transform.SetParent(transform,false);
                            anchor.Relation.type = Type;
                            anchor.Relation.PathConnected = true;
                            Value = obj.Referent.ID;
                        }
                    }

                    return;
                }

                GameObject path = (GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"));

                Transform start = path.transform.Find("StartAnchor").transform, end = path.transform.Find("EndAnchor").transform;

                start.parent = transform;
                start.localPosition = Vector3.zero;
                end.parent = other.transform;
                end.localPosition = Vector3.zero;
                path.transform.Find("Cylinder").GetComponent<CapsuleCollider>().enabled = false;

                path.GetComponent<StolperwegeRelation>().DoOnClick = () =>
                {
                    if (_menu != null) return;

                    List<System.Type> subtypes = SceneController.GetInterface<StolperwegeInterface>().GetSubtypes(Type.to);
                    subtypes.Add(Type.to);

                    if (subtypes.Count > 1)
                    {
                        _menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

                        _menu.transform.parent = path.transform;

                        Hashtable types = new Hashtable();
                        foreach (System.Type sType in subtypes)
                        {
                            string type = SceneController.GetInterface<StolperwegeInterface>().TypeToString(sType);
                            if (type.StartsWith("org.hucompute"))
                                types.Add(type.Replace("org.hucompute.publichistory.datastore.typesystem.", ""), SceneController.GetInterface<StolperwegeInterface>().TypeClassTable[type]);
                        }


                        _menu.Init((string key, object o) =>
                        {
                            Debug.Log(o.GetType());
                            CreateNewDummy((System.Type)o);
                            Destroy(_menu.gameObject);
                        }, types);

                        _menu.transform.position = _relation.GetComponent<StolperwegeRelation>().EndAnchor.position;
                        _menu.transform.forward = -StolperwegeHelper.CenterEyeAnchor.transform.forward;
                        _menu.transform.localScale = Vector3.one * 0.2f;
                    }
                    else
                        CreateNewDummy(Type.to);
                };
                StartCoroutine(path.GetComponent<StolperwegeRelation>().HandlePathSetup());
                _relation = path;
            }
            
        }
    }

    

    private void CreateNewDummy(System.Type type)
    {
        GameObject dummy = SceneController.GetInterface<StolperwegeInterface>().CreateElementDummy(type);
        dummy.transform.position = _relation.GetComponent<StolperwegeRelation>().EndAnchor.GetComponentInParent<PointFinger>().transform.position;
        _relation.GetComponent<StolperwegeRelation>().EndAnchor.transform.parent = dummy.transform;
        _relation.GetComponent<StolperwegeRelation>().type = Type;
        Value = dummy;
    }

    private void OnDestroy()
    {
        Destroy(_label);
        Destroy(gameObject);
    }

    public StolperwegeObject[] RelatedObjects
    {
        get
        {
            List<StolperwegeObject> objs = new List<StolperwegeObject>();

            foreach (StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
                if (anchor.OtherEnd.GetComponentInParent<StolperwegeObject>() != null)
                    objs.Add(anchor.OtherEnd.GetComponentInParent<StolperwegeObject>());

            return objs.ToArray();
        }
    }
}
