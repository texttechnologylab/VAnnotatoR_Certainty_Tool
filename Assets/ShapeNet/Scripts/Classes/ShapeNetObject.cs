using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class ShapeNetObject : VRData
{

    public HashSet<string> Categories { get; private set; }
    public string PrettyCategories { get; private set; }

    //protected Texture2D _thumbnail;
    // TODO call the LoadThumbnail if thumbnail is null
    // in DataContainer
    // in DataBrowserResource
    // in BuilderConsole
    public Texture2D Thumbnail;
    
    public bool IsDownloaded
    {
        get
        {
            if (this is ShapeNetModel) return SceneController.GetInterface<ShapeNetInterface>().CachedObjectPathMap.ContainsKey((string)ID);
            else return SceneController.GetInterface<ShapeNetInterface>().CachedTexturePathMap.ContainsKey((string)ID);
        }
    }

    public ShapeNetObject(JsonData json)
    {
        ID = json["id"].ToString();
        Name = json.Keys.Contains("name") ? json["name"].ToString() : "no_name";
        Categories = new HashSet<string>();
        if (json.Keys.Contains("categories"))
        {
            PrettyCategories = "";
            for (int i = 0; i < json["categories"].Count; i++)
            {
                Categories.Add(json["categories"][i].ToString());
                PrettyCategories += json["categories"][i].ToString();
                if (i < json["categories"].Count - 1) PrettyCategories += ", ";
            }

        }
    }

    public override void SetupDataContainer(DataContainer container)
    {
        DataContainer = container;
        if (DataContainer == null) return;
        DataContainer.Name.text = Name;
        DataContainer.DataInfoText = Name;
        DataContainer.DataType.text = PrettyCategories;
        DataContainer.DataTextIcon.gameObject.SetActive(false);
        DataContainer.Thumbnail.enabled = true;
        if (Thumbnail != null)
            DataContainer.Thumbnail.material.SetTexture("_MainTex", Thumbnail);
        else
            DataContainer.StartCoroutine(ShapeNetInterface.LoadThumbnail(this, () =>
            {
                if (DataContainer == null) return;
                DataContainer.Thumbnail.material.SetTexture("_MainTex", Thumbnail);
            }));
    }

    public override void Setup3DObject()
    {
        if (Object3D == null)
        {
            Object3D = (GameObject)Object.Instantiate(Resources.Load("Prefabs/DataBrowser/ResourceObject"));
            DataBrowserResource resource = Object3D.GetComponent<DataBrowserResource>();
            resource.Init(this);
            resource.SetIconStatus(false);
            resource.InfoTextBox.text = Name;
            if (Thumbnail != null)
                resource.Renderer.material.SetTexture("_MainTex", Thumbnail);
            else
                resource.StartCoroutine(ShapeNetInterface.LoadThumbnail(this, () =>
                {
                    resource.Renderer.material.SetTexture("_MainTex", Thumbnail);
                }));
        }
    }

    public override string ToString()
    {
        return "Name: " + Name + ", ID: " + ID;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is ShapeNetObject)) return false;
        ShapeNetObject other = (ShapeNetObject)obj;
        return other.ID.Equals(ID);
    }

    public override int GetHashCode()
    {
        return 1213502048 + EqualityComparer<string>.Default.GetHashCode((string)ID);
    }
}
