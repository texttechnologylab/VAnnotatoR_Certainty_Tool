using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMAWardrobeItem
{
    public string Name { get; private set; }
    public string Identifier { get; private set; }
    
    public UMAWardrobeItem(string name, string identifier)
    {
        Name = name;
        Identifier = identifier;
    }

    public static UMAWardrobeItem Default { get { return new UMAWardrobeItem("Default", "None"); } }

    public static string ItemToString(UMAWardrobeItem item)
    {
        return item.Name;
    }
}
