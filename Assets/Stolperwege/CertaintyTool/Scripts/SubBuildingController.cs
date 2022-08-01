using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubBuildingController : MonoBehaviour
{
    public BuildingController Main { get; protected set; }
    public string Name { get; protected set; }
    public GameObject GameObject { get; protected set; }
    public bool IsDummy { get; protected set; }

    public SubBuildingController[] Children { get; protected set; }

    public bool HasChildren { get; protected set; }

    private Color[] OriginalColors;

    private Material TranparentMaterial;
    private Material[] OriginalMaterials;

    public int? Rating { get; protected set; }

    public bool IsColored { get; protected set; } = false;
    public bool IsSelected { get; protected set; } = false;
    public bool IsSelectedAsChild { get; protected set; } = false;
    public bool IsReferenceSelected = false;
    public bool IsTransparent { get; protected set; } = false;
    public bool IsActive { get; protected set; } = true;
    public bool IsHighlighted { get; protected set; } = false;
    public bool UnEditable { get; protected set; } = false;
    public bool IsAnnoObj = false;
    public bool Refernceable { get; protected set; } = true;

    public enum DisplayType { Default, Transparent, Hidden, Debug }
    public DisplayType DisplayMode;

    protected Color MainHighlightColor = new Color(0f, 0.7f, 0f);
    protected Color ChildHighlightColor = new Color(0.35f, 0f, 0.35f);
    protected Color NeutralHighlightColor = new Color(0.4f, 0.4f, 0.4f);
    protected const float ChildNeutralColorFactor = 0.75f;
    protected Color ReferenceSelectedColor = new Color(0f, .7f, 1f);


    // Init for SubBuildingcontroller, which controls functions of the BuildingController on an per-object basis
    public void Init(BuildingController main)
    {
        Main = main;
        GameObject = main.GameObject;
        Name = transform.gameObject.name;
        ParseObjects();
        if (Children.Length != 0)
        { HasChildren = true; }
        else
        { HasChildren = false; }

        if (transform.gameObject.GetComponent<Renderer>() != null)
        {
            IsDummy = false;
            Material[] materials = transform.gameObject.GetComponent<Renderer>().materials;

            OriginalColors = new Color[materials.Length];


            for (int i = 0; i < materials.Length; i++)
            {
                OriginalColors[i] = materials[i].GetColor("_BaseColor");
                // Debug.Log("Original Color of " + materials[i].name + " has color " + OriginalColors[i].ToString());
            }
            
            OriginalMaterials = materials;

            TranparentMaterial = Resources.Load("Materials/Transparent", typeof(Material)) as Material;

            
            if (transform.gameObject.GetComponent<Collider>() == null)
            {
                Debug.LogWarning("Object " + Name + " has no collider and will be uneditable in VR as a result!");
                UnEditable = true;
            }
        }
        else
        {
            IsDummy = true;
            Debug.Log("SubBuildingController for " + Name + " will be a dummy because Object has no renderer.");
        }
        
    }

    // Init all Children
    protected void ParseObjects()
    {
        Children = new SubBuildingController[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            GameObject childGameObject = child.gameObject;
            Children[i] = childGameObject.AddComponent<SubBuildingController>();
            Children[i].Init(Main);
            i++;
        }

        transform.gameObject.GetComponent<RatingFaker>()?.Init();
    }

    // Set Active. IsActive independent from active required for state determination
    public void SetActive(bool b)
    {
        IsActive = b;
        transform.gameObject.SetActive(b);
    }

    // Colors all objects on/off
    public virtual void ShowColor(bool b)
    {
        IsColored = b;
        UpdateColor();

        foreach (SubBuildingController child in Children)
        {
            child.ShowColor(b);
        }
    }

    // Color Update function
    private void UpdateColor()
    {
        if (IsDummy) return;

        Material[] materials = transform.gameObject.GetComponent<Renderer>().materials;

        
        for (int i = 0; i < materials.Length; i++)
        {
            Color newColor = OriginalColors[i];

            if (IsColored && !Main.ReferenceMode) newColor = CertaintyToolInterface.CalcColor(Rating, Main.RatingMin, Main.RatingMax);
            else if (IsReferenceSelected && Main.ReferenceMode) newColor = ReferenceSelectedColor;

            if (DisplayMode == DisplayType.Transparent) newColor.a = 0.3f; // Make Transparent

            else if (IsSelectedAsChild) // Highlight as child
            {
                //newColor.r += 0.125f;
                //newColor.g += 0.125f;
                //newColor.b += 0.125f;
                materials[i].EnableKeyword("_EMISSION");
                materials[i].SetColor("_EmissionColor", IsColored ? NeutralHighlightColor * ChildNeutralColorFactor : ChildHighlightColor);
            }
            else if (IsSelected) // Highlight
            {
                //newColor.r += 0.25f;
                //newColor.g += 0.25f;
                //newColor.b += 0.25f;
                materials[i].EnableKeyword("_EMISSION");
                materials[i].SetColor("_EmissionColor", IsColored ? NeutralHighlightColor : MainHighlightColor);
            }
            else materials[i].DisableKeyword("_EMISSION");

            // Debug.LogWarning("Color: " + newColor.ToString());

            materials[i].SetColor("_BaseColor", newColor);
        }
    }

    // Sets Rating
    public void SetRating(int? rating, bool overrideLower = false, bool dontUpdate = false)
    {
        _SetRating(rating, overrideLower);
        if (!dontUpdate)
        {
            Main.UpdateVisualization();
            Main.ShowColor(Main.IsColored);
        }
    }



    // Helper Function for SetRating
    public void _SetRating(int? rating, bool overrideLower)
    {
        Rating = rating;
        if (overrideLower)
        {
            foreach (SubBuildingController child in Children)
            {
                child._SetRating(rating, overrideLower);
            }
        }
    }

    public void ClearRating(bool overrideLower = false, bool dontUpdate = false)
    {
        SetRating(null, overrideLower, dontUpdate);
    }

    // Deselect
    public virtual void Deselect()
    {
        IsSelected = false;
        IsSelectedAsChild = false;

        foreach (SubBuildingController child in Children)
        {
            child.Deselect();
        }
    }

    // Select
    public virtual void Select(bool deselect = true)
    {
        if (deselect) Main.Deselect();
        _Select();
        Main.UpdateVisualization();
    }

    // Helper Function for Select
    protected virtual void _Select()
    {
        IsSelected = true;

        foreach (SubBuildingController child in Children)
        {
            child.SelectAsChild();
        }
    }
    // Helper Function for SetRatingBoundaries of BuildingController
    public void _SetRatingBoundaries()
    {
        if (Rating > Main.RatingMax) Rating = Main.RatingMax;
        else if (Rating < Main.RatingMin) Rating = Main.RatingMin;

        foreach (SubBuildingController child in Children)
        {
            child._SetRatingBoundaries();
        }
    }

    // Select but treat as child
    private void SelectAsChild()
    {
        IsSelected = true;
        IsSelectedAsChild = true;

        foreach (SubBuildingController child in Children)
        {
            child.SelectAsChild();
        }
    }

    public virtual void UpdateVisualization()
    {
        foreach (SubBuildingController child in Children)
        {
            child.UpdateVisualization();
        }

        UpdateDisplayType();

        if (IsDummy) return;

        if (DisplayMode == DisplayType.Default)
        {
            IsActive = true;
            SetActive(true);

            // transform.gameObject.GetComponent<Renderer>().material = OriginalMaterials[0];
            Material[] materials = transform.gameObject.GetComponent<Renderer>().materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = OriginalMaterials[i];
            }
            transform.gameObject.GetComponent<Renderer>().materials = materials;
        }
        else if (DisplayMode == DisplayType.Transparent)
        {
            IsActive = true;
            SetActive(true);

            // transform.gameObject.GetComponent<Renderer>().material = TranparentMaterial;
            Material[] materials = transform.gameObject.GetComponent<Renderer>().materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = TranparentMaterial;
            }
            transform.gameObject.GetComponent<Renderer>().materials = materials;
        }
        else if (DisplayMode == DisplayType.Hidden)
        {
            IsActive = false;
            SetActive(false);
        }
        else
        {
            IsActive = true;
            SetActive(true);

            // transform.gameObject.GetComponent<Renderer>().material = OriginalMaterial;
            int numMaterials = transform.gameObject.GetComponent<Renderer>().materials.Length;
            for (int i = 0; i < numMaterials; i++)
            {
                transform.gameObject.GetComponent<Renderer>().materials[i] = OriginalMaterials[i];
                i++;
            }
            // TODO: move debug Color
            transform.gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0f, 1f, 1f);
            return;
        }

        UpdateColor();
    }

    protected virtual void UpdateDisplayType()
    {
        bool childIsActive = false;
        foreach (SubBuildingController child in Children)
        {
            if (child.IsActive)
            {
                childIsActive = true;
                break;
            }
        }

        if ((Main.ReferenceMode || childIsActive && Main.DisplayMode == BuildingController.DisplayType.Hidden && Main.IsInShowBoundaries(Rating)) || (Main.EditMode && IsSelected) ||
            (!Main.EditMode && (Main.IsInShowBoundaries(Rating) || !Main.IsInRatingBoundaries(Rating))))
        {
            DisplayMode = DisplayType.Default;
        }
        else if ((Main.EditMode && !IsSelected) || (!Main.IsInShowBoundaries(Rating) && Main.DisplayMode == BuildingController.DisplayType.Transparent) ||
            (childIsActive && Main.DisplayMode == BuildingController.DisplayType.Hidden && !Main.IsInShowBoundaries(Rating)))
        {
            DisplayMode = DisplayType.Transparent;
        }
        else if (!Main.IsInShowBoundaries(Rating) && Main.DisplayMode == BuildingController.DisplayType.Hidden)
        {
            DisplayMode = DisplayType.Hidden;
        }
        else
        {
            Debug.LogError(Name + " has not reached valid Display Type!");
            DisplayMode = DisplayType.Debug;
        }    
    }
    public int[] CalcNumbers()
    {
        int[] numbers = new int[3];
        numbers[0] = (!IsDummy && Rating == null) ? 1 : 0; // Unrated Objects Num
        numbers[1] = IsDummy ? 1 : 0; // Dummy Objects Num
        numbers[2] = UnEditable ? 1 : 0; // Uneditable Objects Num
        foreach (SubBuildingController SubCo in Children)
        {
            int[] newNumbers = SubCo.CalcNumbers();
                for (int i = 0; i < 3; i++)
                {
                    numbers[i] += newNumbers[i];
                }
        }
        return numbers;
    }

    public GameObject FindChild(string name)
    {
        GameObject go = transform.Find(name)?.gameObject;
        if (go == null)
        {
            foreach (SubBuildingController child in Children)
            {
                go = child.FindChild(name);
                if (go != null) break;
            }
        }
        return go;
    }

    public List<(string, SubBuildingController)> ReturnNameObjTuples()
    {
        List<(string, SubBuildingController)> nameObjList = new List<(string, SubBuildingController)>();
        nameObjList.Add((name, this));
        foreach (SubBuildingController child in Children)
        {
            nameObjList.AddRange(child.ReturnNameObjTuples());
        }
        return nameObjList;
    }

    public string genKey()
    {
        return name + " "
            + transform.localPosition.x.ToString() + " "
            + transform.localPosition.y.ToString() + " "
            + transform.localPosition.z.ToString() + " "
            + transform.localScale.x.ToString() + " "
            + transform.localScale.y.ToString() + " "
            + transform.localScale.z.ToString() + " "
            + transform.localRotation.x.ToString() + " "
            + transform.localRotation.y.ToString() + " "
            + transform.localRotation.z.ToString();
    }

    /// <summary>
    /// Generate Strings for this SubBuildingController and all Children for saving in DB
    /// </summary>
    /// <returns></returns>
    public List<List<string>> GenSaveStrings()
    {
        List<List<string>> savestrings = new List<List<string>>();
        savestrings.Add(GenSaveString());

        foreach (SubBuildingController child in Children)
        {
            savestrings.AddRange(child.GenSaveStrings());
        }

        return savestrings;
    }   

    /// <summary>
    /// Generate String for this SubBuildingController for saving in DB
    /// </summary>
    /// <returns></returns>
    public virtual List<string> GenSaveString()
    {
        List<string> strings = new List<string>();
        // Base Data
        strings.Add(Name);
        strings.Add((Rating == null) ? "null" : Rating.ToString());

        // Drawed Object (annotation Object) additional Data for Recreation
        if (IsAnnoObj)
        {
            strings.Add(GetComponent<AnnotationObject>().Type);

            strings.Add(transform.localPosition.x.ToString());
            strings.Add(transform.localPosition.y.ToString());
            strings.Add(transform.localPosition.z.ToString());

            strings.Add(transform.localScale.x.ToString());
            strings.Add(transform.localScale.y.ToString());
            strings.Add(transform.localScale.z.ToString());

            strings.Add(transform.localRotation.eulerAngles.x.ToString());
            strings.Add(transform.localRotation.eulerAngles.y.ToString());
            strings.Add(transform.localRotation.eulerAngles.z.ToString());
        }

        return strings;
    }
}