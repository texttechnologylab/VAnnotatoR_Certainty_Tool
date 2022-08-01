using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMAWardrobe
{
    public string SlotName { get; private set; }
    public UMAModel Model { get; private set; }
    public List<UMAWardrobeItem> Items { get; private set; }
    
    public UMAWardrobe(string slotName, UMAModel model, List<UMAWardrobeItem> items)
    {
        SlotName = slotName;
        Model = model;
        Items = new List<UMAWardrobeItem> { UMAWardrobeItem.Default };
        Items.AddRange(items);
    }

    public bool IsCompatible(UMAModel model)
    {
        return Model == null || model == Model;
    }

    public List<string> GetItemNames()
    {
        return Items.ConvertAll(new System.Converter<UMAWardrobeItem, string>(UMAWardrobeItem.ItemToString));
    }

    public UMAWardrobeItem GetItem(string name)
    {
        return Items.Exists(i => i.Name == name) ? Items.Find(i => i.Name == name) : null;
    }

    public UMAWardrobeItem GetFromIdentifier(string identifier)
    {
        return Items.Exists(i => i.Identifier == identifier) ? Items.Find(i => i.Identifier == identifier) : null;
    }
}
