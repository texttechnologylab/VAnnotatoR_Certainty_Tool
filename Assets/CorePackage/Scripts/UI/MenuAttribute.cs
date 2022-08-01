using System;

public class MenuAttribute : Attribute
{

    public string PrefabPath { get; private set; }
    public bool IsMainMenuComponent { get; private set; }

    public MenuAttribute(string prefabPath, bool isMainMenuComponent=false)
    {
        if (string.IsNullOrEmpty(prefabPath))
            throw new ArgumentOutOfRangeException(nameof(prefabPath));

        PrefabPath = prefabPath;
        IsMainMenuComponent = isMainMenuComponent;
    }
}
