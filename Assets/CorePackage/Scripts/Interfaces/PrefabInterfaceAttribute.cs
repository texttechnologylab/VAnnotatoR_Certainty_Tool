using System;

public class PrefabInterfaceAttribute : Attribute
{
    public string PrefabPath { get; private set; }

    public PrefabInterfaceAttribute(string prefabPath)
    {
        if(string.IsNullOrEmpty(prefabPath))
            throw new ArgumentOutOfRangeException(nameof(prefabPath));
        
        PrefabPath = prefabPath;
    }
}