using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Menu(PrefabPath + "MainMenu")]
public class MainMenuScript : MenuScript
{

    public override void Start()
    {
        base.Start();
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        IEnumerable<Type> menus = StolperwegeHelper.GetTypesDerivingFrom<MenuScript>();
        GameObject menuPrefab, menuInstance; MenuAttribute prefabInterface;
        foreach (Type menu in menus)
        {
            prefabInterface = menu.GetCustomAttribute<MenuAttribute>();

            if (prefabInterface == null) throw new Exception("PrefabInterface not decleared for " + menu.Name);
            if (!prefabInterface.IsMainMenuComponent) continue;
            menuPrefab = Resources.Load<GameObject>(prefabInterface.PrefabPath);
            if (menuPrefab == null)
            {
                Debug.LogError($"Prefab of '{menu.Name}' menu is not present at [{prefabInterface.PrefabPath}]!");
                continue;
            }

            menuInstance = Instantiate(menuPrefab);
            menuInstance.AddComponent(menu);
            menuInstance.transform.SetParent(_subMenuControl.transform);
            menuInstance.transform.localRotation = Quaternion.identity;
        }
    }
}