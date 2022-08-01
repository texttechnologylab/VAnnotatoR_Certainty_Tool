using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Menu(PrefabPath + "StandardMenuItem", true)]
public class InventoryMenu : MenuScript
{

    // TODO Overwrite the CalculateSubMenuLayout for displaying objects
    public override void Start()
    {
        base.Start();

        SetNameAndIcon("Inventory", "\xf49e");
        GameObject backpackController = new GameObject("InventoryController");
        backpackController.AddComponent<InventoryController>();
        backpackController.transform.SetParent(transform);
        OnClick = SubmenuController;
    }
}
