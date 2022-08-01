using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarColorManager : MonoBehaviour
{
    public void Awake()
    {
        if (SceneController.GetInterface<StolperwegeInterface>().CurrentUser != null) colorFromActiveConfig();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.F10))
        {
            Debug.Log("You pressed F10 and painted the Avatar!");
            colorFromActiveConfig();
        }
    }
    public void colorFromActiveConfig()
    {
        colorAvatar(SceneController.GetInterface<StolperwegeInterface>().CurrentUser.activeConfig);
    }
    public void colorAvatar(DiscourseReferent config)
    {
        Transform avatar = transform.GetChild(0);
        foreach (DictionaryEntry e in config.Preferences)
        {
            // Debug.Log(e.Key.ToString());
            // Debug.Log(e.Value.ToString());
            bool hasPart = false;
            for (int j = 0; j < avatar.childCount; j++)
            {
                if (avatar.GetChild(j).GetComponent<SkinnedMeshRenderer>() == null) continue;
                if (avatar.GetChild(j).name.Equals((string)e.Key))
                {
                    hasPart = true;
                    break;
                }
            }
            //   Debug.Log(hasPart);
            if (hasPart) ChangeColor(avatar, (string)e.Key, StolperwegeHelper.ConvertHexToColor((string)e.Value));
        }
    }

    /// <summary>Changes the color of any subpart of a Gameobject, that has a SkinnedMeshRenderer component.</summary>
    /// <param name="anchor">The Gameobject</param>
    /// <param name="partName">The subpart</param>
    /// <param name="color">The desired color</param>
    private void ChangeColor(Transform trans, string partName, Color color)
    {
        trans.Find(partName).GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", color);
    }
}
