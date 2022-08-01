using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using LitJson;
using Valve.VR;

public class LocalCacheHandler 
{
    public const string CREDENTIALS = "credentials";
    public const string DEFAULT_POINTER_SIDE = "defaultPointerSide";
    
    public static string CurrentSysUser { get { return System.Security.Principal.WindowsIdentity.GetCurrent().Name; } }

    public static void Reset()
    {
        if (PlayerPrefs.HasKey(CurrentSysUser))
            PlayerPrefs.SetString(CurrentSysUser, "{}");
    }

    /// <summary>
    /// Stores the credentials of a user locally.
    /// </summary>
    /// <param name="space">The login space, where the credentials should be stored.</param>
    /// <param name="loginData">Stores the credentials.</param>
    public static void StoreCredentials(string space, LoginData loginData)
    {
        JsonData json = new JsonData();
        if (PlayerPrefs.HasKey(CurrentSysUser))
            json = JsonMapper.ToObject(PlayerPrefs.GetString(CurrentSysUser));
        if (!json.ContainsKey(CREDENTIALS))
            json[CREDENTIALS] = new JsonData();
        json = json[CREDENTIALS];
        if (!json.ContainsKey(space))
            json[space] = new JsonData();
        json[space]["username"] = loginData.Username;
        json[space]["pw"] = loginData.Password;
        Debug.Log(json.ToJson());
        PlayerPrefs.SetString(CurrentSysUser, json.ToJson());
    }

    /// <summary>
    /// Returns the stored credentials for the requested space, if existing.
    /// </summary>
    /// <param name="space">The login space.</param>
    public static LoginData GetCredentials(string space)
    {
        if (!PlayerPrefs.HasKey(CurrentSysUser)) return null;
        JsonData json = JsonMapper.ToObject(PlayerPrefs.GetString(CurrentSysUser));
        if (!json.ContainsKey(space)) return null;
        JsonData loginObj = json[space];
        return new LoginData(loginObj["username"].ToString(), loginObj["pw"].ToString());
    }

    /// <summary>
    /// Deletes all stored credentials for the actual user.
    /// </summary>
    public static void DeleteCredentials()
    {
        if (!PlayerPrefs.HasKey(CurrentSysUser)) return;
        JsonData json = JsonMapper.ToObject(PlayerPrefs.GetString(CurrentSysUser));
        foreach (Interface iFace in SceneController.Interfaces)
        {
            if (iFace.OnLogin != null && json.ContainsKey(iFace.Name))
                json.Remove(iFace.Name);
        }
        PlayerPrefs.SetString(CurrentSysUser, json.ToJson());
    }

    /// <summary>
    /// Saves on which hand the pointer should be initial for the current user.
    /// </summary>
    public static void SetDefaultPointingHand(SteamVR_Input_Sources side)
    {
        JsonData json = new JsonData();
        if (PlayerPrefs.HasKey(CurrentSysUser))
            json = JsonMapper.ToObject(PlayerPrefs.GetString(CurrentSysUser));
        json[DEFAULT_POINTER_SIDE] = side.ToString();
        PlayerPrefs.SetString(CurrentSysUser, json.ToJson());
    }

    /// <summary>
    /// Returns on which handside the pointer should be initial for the current user.
    /// If there is no such entry, initializes it with the right side.
    /// </summary>
    public static SteamVR_Input_Sources GetDefaultPointerSide()
    {
        if (!PlayerPrefs.HasKey(CurrentSysUser))
            SetDefaultPointingHand(SteamVR_Input_Sources.RightHand);
        JsonData json = JsonMapper.ToObject(PlayerPrefs.GetString(CurrentSysUser));
        if (!json.ContainsKey(DEFAULT_POINTER_SIDE))
            SetDefaultPointingHand(SteamVR_Input_Sources.RightHand);
        json = JsonMapper.ToObject(PlayerPrefs.GetString(CurrentSysUser));
        return (SteamVR_Input_Sources)Enum.Parse(SteamVR_Input_Sources.Any.GetType(), json[DEFAULT_POINTER_SIDE].ToString());
    }

    

}
