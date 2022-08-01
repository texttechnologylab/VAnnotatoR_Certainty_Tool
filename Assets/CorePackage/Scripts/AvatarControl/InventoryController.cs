using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : SubmenuScrollbox 
{

    private Dictionary<System.Type, ArrayList> Items;
    public InventoryMenu Inventory;

	// Use this for initialization
	public override void Start () {
        base.Start();
        Items = new Dictionary<System.Type, ArrayList>();
        StolperwegeHelper.Inventory = this;
    }

    private bool _active;
    public bool isActive
    {
        get
        {
            return _active;
        }

        set
        {

            if (value == _active) return;

            _active = value;

            if (Inventory == null) return;

            if (_active) Inventory.SubmenuController();
            else Inventory.MainMenuCallback();
        }
    }

    public bool ContainsItem(MonoBehaviour script)
    {
        if (!Items.ContainsKey(script.GetType())) return false;
        foreach (Object item in Items[script.GetType()])
        {
            if (item.Equals(script)) return true;
        }
        return false;
    }

    public void AddItem(MonoBehaviour item)
    {
        System.Type scriptType = item.GetType();
        if (!Items.ContainsKey(scriptType)) Items.Add(scriptType, new ArrayList());
        Items[scriptType].Add(item);
        GameObject itemObject = item.gameObject;
        MenuScript script = itemObject.GetComponent<MenuScript>();
        itemObject.transform.parent = transform;
        itemObject.transform.position = transform.position;
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localScale = Vector3.zero;
        script.MenuStartPosition = Vector3.zero;
        script.MenuStartScale = Vector3.zero;
        script.IconCollider.enabled = false;
        script.OnTheMenu = true;
        script.RenderedComponents = itemObject.GetComponentsInChildren<Renderer>(true);
        script.SetComponentRendererStatus(false);
    }

    public void RemoveItem(MonoBehaviour item)
    {
        System.Type scriptType = item.GetType();
        ArrayList itemsOfType = Items[scriptType];
        //((MenuScript)item).onTheMenu = false;
        //((MenuScript)item).ParentMenu = null;
        for (int i=0; i<itemsOfType.Count; i++)
        {
            if (itemsOfType[i].Equals(item))
            {
                itemsOfType.RemoveAt(i);
                break;
            }
        }
        CalculateSubmenuLayout();
        ShowItems();
    }
}
