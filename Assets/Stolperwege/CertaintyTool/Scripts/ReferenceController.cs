using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceController : SubBuildingController
{
    // public bool IsRefObj = true;
    public bool Reflexive { get; protected set; }

    private List<GameObject> MeshComponents;

    private Material MaterialDefault;

    private Color ColorBase;
    private Color ColorEmissionBase = new Color(.25f, .25f, .25f);
    private Color ColorEmissionHighlight = new Color(.5f, .5f, .5f);

    private ReferenceLabel Label;

    public SubBuildingController Origin { get; protected set; }
    public SubBuildingController Target { get; protected set; }

    public bool ShowLabel = true;


    public void Init(BuildingController main, bool reflexive, SubBuildingController origin, SubBuildingController target)
    {
        Main = main;

        Origin = origin;
        Target = target;

        Refernceable = false;

        Children = new SubBuildingController[0];

        MeshComponents = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<Renderer>() != null) MeshComponents.Add(child.gameObject);
            // Debug.LogWarning("Child " + child.name + " added");
        }
        
        ColorBase = reflexive ? new Color(0, .55f, .7f, .7f) : new Color(0, .7f, .55f, .7f);
        //ColorEmissionBase = reflexive ? new Color(0, .55f, .7f) : new Color(0, .7f, .55f);
        //ColorEmissionHighlight = reflexive ? new Color(0, .7f, 1f) : new Color(0, 1f, .7f);
        ColorEmissionBase = new Color(.15f, .15f, .15f);
        ColorEmissionHighlight = new Color(.55f, .55f, .55f);

        MaterialDefault = Resources.Load("Materials/Reference", typeof(Material)) as Material;

        foreach (GameObject child in MeshComponents)
        {
            MaterialDefault.SetColor("_BaseColor", ColorBase);
            MaterialDefault.SetColor("_EmissionColor", ColorEmissionBase);
            MaterialDefault.EnableKeyword("_EMISSION");

            child.GetComponent<Renderer>().material = MaterialDefault;
            bool found = child.GetComponent<Renderer>() != null;
        }

        
        CreateTag();
        UpdateRefTag("<Undefined>");
        UpdateVisualization();
        
    }

    // Deselect
    public override void Deselect()
    {
        IsSelected = false;
        IsSelectedAsChild = false;

        Origin.IsReferenceSelected = false;
        Target.IsReferenceSelected = false;
    }

    // Select
    public override void Select(bool deselect = true)
    {
        if (deselect) Main.Deselect();
        _Select();
        Main.UpdateVisualization();
    }

    // Helper Function for Select
    protected override void _Select()
    {
        IsSelected = true;
        Origin.IsReferenceSelected = true;
        Target.IsReferenceSelected = true;
    }

    public override void UpdateVisualization()
    {
        if (Main.ReferenceMode)
        {
            bool IsInShowBoundaries = Main.IsInShowBoundaries(Rating);
            bool IsInRatingBoundaries = Main.IsInRatingBoundaries(Rating);

            bool hide = !IsInShowBoundaries && IsInRatingBoundaries;
            bool hideInEditMode = !IsSelected && Main.EditMode;

            //Debug.LogWarning("IsInShowBoundaries: " + IsInShowBoundaries.ToString());
            //Debug.LogWarning("IsInRatingBoundaries: " + IsInRatingBoundaries.ToString());

            if (Main.DisplayMode == BuildingController.DisplayType.Hidden && hide)
            {
                gameObject.SetActive(false);
                Label.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                Label.gameObject.SetActive(ShowLabel);

                foreach (GameObject child in MeshComponents)
                {
                    Material material = child.GetComponent<Renderer>().material;
                    material.EnableKeyword("_EMISSION");

                    Color newColor = IsColored ? CertaintyToolInterface.CalcColor(Rating, Main.RatingMin, Main.RatingMax) : ColorBase;

                    if (IsSelected)
                    {
                        material.SetColor("_EmissionColor", ColorEmissionHighlight);
                    }
                    else
                    {
                        material.SetColor("_EmissionColor", ColorEmissionBase);
                    }


                    if (hide || hideInEditMode) newColor.a = .25f;

                    material.SetColor("_BaseColor", newColor);
                    // material.SetColor("_EmissionColor", newColor);

                }
            }
        }
        else
        {
            gameObject.SetActive(false);
            Label.gameObject.SetActive(false);
        }
        
    }
    public override void ShowColor(bool b)
    {
        IsColored = b;

        UpdateVisualization();
    }

    public void CreateTag()
    {
        GameObject labelObject = Instantiate(Resources.Load<GameObject>("Prefabs/Label"));
        Label = labelObject.GetComponentInChildren<ReferenceLabel>();
        Label.RootObject = gameObject;
    }

    public void UpdateRefTag(string text)
    {
        Label.SetText(text);

    }

    public override List<string> GenSaveString()
    {
        List<string> strings = new List<string>();
        // Base Data
        strings.Add(name);
        strings.Add((Rating == null) ? "null" : Rating.ToString());


        strings.Add(Origin.name);
        strings.Add(Target.name);

        strings.Add(Reflexive.ToString());


        return strings;
    }
}
