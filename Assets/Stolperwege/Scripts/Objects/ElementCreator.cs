using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementCreator : InstantiableSmartwatchObject 
{

    public new static string PrefabPath = "StolperwegeElements/ElementCreator";
    public new static string Icon = "\xf247";

    private CircleMenu _menu;
    private CircleMenu Menu
    { 
        get
        {
            if (_menu == null)
            {
                _menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();
                Hashtable types = new Hashtable();
                foreach (string type in SceneController.GetInterface<StolperwegeInterface>().TypeSystemTable.Keys)
                    if (type.StartsWith("org.hucompute"))
                        types.Add(type.Replace("org.hucompute.publichistory.datastore.typesystem.", ""), SceneController.GetInterface<StolperwegeInterface>().TypeSystemTable[type]);
                _menu.transform.SetParent(transform);
                _menu.transform.localPosition = Vector3.zero;
                BoxCollider collider = gameObject.GetComponent<BoxCollider>();
                collider.size = new Vector3(3, 3, 0.05f);
                collider.isTrigger = true;
                _menu.Init((string key, object o) => {
                    GameObject dummy = SceneController.GetInterface<StolperwegeInterface>().CreateElementDummy("org.hucompute.publichistory.datastore.typesystem." + key);
                    dummy.transform.position = _menu.transform.position;
                }, types, false);

            }
            return _menu;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        ShrinkedSize = Vector3.one * 0.075f;
        ExpandedSize = Vector3.one * 0.15f;
        OnFocus = Menu.CheckCursor;
        OnPointer = Menu.CheckCursor;
        BlockRotationOnFocus = true;
        BlockRotationOnPointer = true;
        LookAtUserOnHold = true;
        KeepYRotation = true;
    }
}
