using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.IO;
using System;
using System.Linq;

public class CertaintyToolInterface : Interface
{
    public GameObject CurrentHouse { get; private set; }

    public GameObject CurrentCategory { get; private set; }

    public GameObject SelectedObj { get; private set; }
    public BuildingController SelectedBuilding { get; private set; } = null;
    public SubBuildingController SelectedSubBuilding { get; private set; }
    public bool MultiSelect = false;
    public List<SubBuildingController> SelectedSubBuildings { get; private set; } = new List<SubBuildingController>();

    public GameObject UI { get; private set; }

    public CertaintyToolMainMenu MainMenu { get; private set; }

    public DrawedObject SelectedDO { get; private set; }
    public AnnotationObject SelectedAnno { get; private set; }

    public bool BuildingSelected { get; private set; }

    private bool _active;

    public const int LowestPossibleRating = -99;
    public const int HighestPossibleRating = 99;

    public static Color OutsideBoundaryColor = new Color(0f, 0f, 1f);
    public bool AutoConnectAnnos = false;

    public int MinRating
    {
        get
        {
            if (BuildingSelected) return SelectedBuilding.RatingMin;
            else return 0;
        }
    }
    public int MaxRating
    {
        get
        {
            if (BuildingSelected) return SelectedBuilding.RatingMax;
            else return 0;
        }
    }
    public bool HideUI = false;
    public bool Active
    {
        get { return _active; }
        set
        {
            if (value == _active) return;
            _active = value;
            if (_active && !HideUI) UI.transform.position = StolperwegeHelper.CenterEyeAnchor.transform.position + StolperwegeHelper.CenterEyeAnchor.transform.forward * 0.4f + Vector3.down * 0.2f;
        }
    }

    /// <summary>
    /// Interface Init
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator InitializeInternal()
    {
        name = "CertaintyTool";

        UI = Instantiate(Resources.Load<GameObject>("Prefabs/MainMenu")); //.GetComponent<SurfaceHUDMenu>();
        Active = false;

        MainMenu = UI.GetComponent<CertaintyToolMainMenu>();
        MainMenu.Init(this);

        BuildingSelected = false;


        //Instantiate(Resources.Load<GameObject>("Prefabs/ScreenshotCam"));

        yield return new WaitUntil(() => { return StolperwegeHelper.CenterEyeAnchor != null; });

        // StolperwegeHelper.CenterEyeAnchor.AddComponent<screenshotCam>();

        yield break;
    }

    /// <summary>
    /// Clear Selection
    /// </summary>
    public void ClearSelection() 
    {
        if (BuildingSelected) SelectedBuilding.OnBuildingChanged();
        BuildingSelected = false;
        SelectedObj = null;
        SelectedBuilding = null;
        SelectedSubBuilding = null;
        SelectedSubBuildings.Clear();
    }

    /// <summary>
    /// Clears Selected Object
    /// </summary>
    public void ClearSelectedObject()
    {
        SelectedBuilding.Deselect();
        SelectedObj = null;
        SelectedSubBuilding = null;
        SelectedSubBuildings.Clear();
    }

    /// <summary>
    /// Required for Selection
    /// </summary>
    void Update()
    {
        HideUI = StolperwegeHelper.VRWriter.Active;


        UI.SetActive(_active && !HideUI);

        if (Active && !HideUI) UI.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);

        if (SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            GameObject hitObj = StolperwegeHelper.RightFinger.HittedObject;
            // Debug.LogWarning("hitObj: " + hitObj.name);
            if (hitObj != null && hitObj.GetComponent<SubBuildingController>() != null)
            {
                Selection(hitObj);
            }
            else if (hitObj != null && (hitObj.GetComponent<DrawedObject>() != null || hitObj.GetComponent<AnnotationObject>() != null))
            {
                SelectionAnno(hitObj);
            }

            else if (hitObj != null && hitObj.transform.parent?.GetComponent<ReferenceController>() != null)
            {
                Selection(hitObj.transform.parent.gameObject);
            }

        }
        //SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.LeftHand))
        if (SteamVR_Actions.default_right_action1.GetStateDown(SteamVR_Input_Sources.RightHand) && !HideUI)
        {
            Active = !Active;
        }
    }

    /// <summary>
    /// Selection Method for objects of a building (SubBuildingControllers)
    /// </summary>
    /// <param name="hitObj"></param>
    private void Selection(GameObject hitObj)
    {
        BuildingController lastBuilding = SelectedBuilding;

        BuildingSelected = true;
        SelectedObj = hitObj;
        SelectedSubBuilding = hitObj.GetComponent<SubBuildingController>();
        SelectedBuilding = hitObj.GetComponent<SubBuildingController>().Main;

        bool buildingChanged = false;

        if (lastBuilding == null || lastBuilding != SelectedBuilding)
        {
            buildingChanged = true;
            lastBuilding?.OnBuildingChanged();
        }

        if (MultiSelect && !buildingChanged)
        {
            if (!SelectedSubBuildings.Contains(SelectedSubBuilding)) SelectedSubBuildings.Add(SelectedSubBuilding);

            // In case it was already one of the selected subcontrollers deselect
            else
            {
                SelectedSubBuildings.Remove(SelectedSubBuilding);
                SelectedSubBuilding.Deselect();
                if (SelectedSubBuildings.Count > 0)
                {
                    SelectedSubBuilding = SelectedSubBuildings.Last();
                    SelectedObj = SelectedSubBuildings.Last().gameObject;
                }
                else
                {
                    SelectedSubBuilding = null;
                    SelectedObj = null;

                    SelectedBuilding.UpdateVisualization();
                }
            }
        }
        else
        {
            SelectedSubBuildings.Clear();
            SelectedSubBuildings.Add(SelectedSubBuilding);
        }

        if (SelectedSubBuilding != null) SelectedSubBuilding.Select(!MultiSelect);


        UpdateDisplayMenu(buildingChanged);
        UpdateRatingSettingsMenu();
        UpdateSetRatingPanel();
        UpdateAnalyticPanel();

    }

    /// <summary>
    /// Selection Method for Drawn Objects
    /// </summary>
    /// <param name="hitObj"></param>
    private void SelectionAnno(GameObject hitObj)
    {
        //Debug.LogWarning("SelectionAnno: " + hitObj.name);
        SelectedDO = hitObj.GetComponent<DrawedObject>();
        SelectedAnno = hitObj.GetComponent<AnnotationObject>();

        string displayOutput = hitObj.name;

        if (SelectedAnno != null && SelectedAnno.IsConnected)
            displayOutput += "\nConnected to:\n" + SelectedAnno.ConnectedBuilding.Name;

        else displayOutput += "\nnot Connected";

        MainMenu.AnnotationMenu.SetObjectDisplayText(displayOutput);
    }

    /// <summary>
    /// Selects Every Object of the Building
    /// </summary>
    public void SelectBuildingController()
    {
        SelectedSubBuildings.Clear();
        if (BuildingSelected)
        {
            SelectedBuilding.SelectAll();


            foreach (SubBuildingController sub in SelectedBuilding.Children)
            {
                SelectedSubBuildings.Add(sub);
            }
        }
    }
    /// <summary>
    /// Selects Parent (in Hierarchy) of selected Object
    /// </summary>
    public void SelectParent()
    {
        // TODO: Multi Select Support ?
        SelectedSubBuildings.Clear();

        SubBuildingController parent = SelectedSubBuilding.transform.parent.GetComponent<SubBuildingController>();
        if (parent != null)
        {
            parent.Select();
            SelectedSubBuilding = parent;
            SelectedSubBuildings.Add(parent);
        }
        // TODO: BuildingController Handling

        UpdateSetRatingPanel();
    }

    /// <summary>
    /// Sets rating of selected Object (SubBuildingController)
    /// </summary>
    /// <param name="rating"></param>
    /// <param name="overrideLower"></param>
    /// <returns></returns>
    public void SetRatingObj(int rating, bool overrideLower)
    {
        foreach (SubBuildingController selectedBuilding in SelectedSubBuildings)
        {
            selectedBuilding.SetRating(rating, overrideLower);
        }
        UpdateAnalyticPanel();
    }

    /// <summary>
    /// Sets rating of selected Object (SubBuildingController)
    /// </summary>
    /// <param name="rating"></param>
    /// <param name="overrideLower"></param>
    /// <returns></returns>
    public void ClearRatingObj(bool overrideLower)
    {
        foreach (SubBuildingController selectedBuilding in SelectedSubBuildings)
        {
            selectedBuilding.SetRating(null, overrideLower);
        }
        UpdateAnalyticPanel();
    }

    /// <summary>
    /// Calculate Color corresponding to Rating for given Parameters
    /// </summary>
    /// <param name="rating"></param>
    /// <param name="minRating"></param>
    /// <param name="maxRating"></param>
    /// <param name="forTransparent"></param>
    /// <returns></returns>
    static public Color CalcColor(int? rating, int minRating, int maxRating, bool forTransparent = false)
    {
        if (rating != null && minRating <= rating && rating <= maxRating)
        {
            float steps = (float)maxRating - (float)minRating;

            if ((maxRating - minRating) == 0) return OutsideBoundaryColor;

            rating -= minRating;

            float r = 1f;
            float g = 1f;
            float b = 0f;

            float middle = steps / 2;

            if (rating < middle)
            {
                g = (2 * (int)rating) * (1f / steps);
            }
            else if (rating > middle)
            {
                r = (2 * (steps - (int)rating)) * (1f / steps);
            }

            return new Color(r, g, b);
        }
        else 
        {
            return OutsideBoundaryColor;
        }
    }

    /// <summary>
    /// Update UI Display Menu Panel
    /// </summary>
    private void UpdateDisplayMenu(bool buildingChanged)
    {
        if (SelectedBuilding != null && buildingChanged)
        {
            MainMenu.DisplayMenu.SetBuildingDisplayTag(SelectedBuilding.Name);
            MainMenu.DisplayMenu.SpinboxMin.SetValue(MinRating);
            MainMenu.DisplayMenu.SpinboxMax.SetValue(MaxRating);

            MainMenu.DisplayMenu.SelectionMenu.ButtonHide.ButtonOn = SelectedBuilding.IsTransparent;
            MainMenu.DisplayMenu.SelectionMenu.ButtonColor.ButtonOn = SelectedBuilding.IsColored;
        }
    }

    /// <summary>
    /// Update UI Rating Settings Panel
    /// </summary>
    private void UpdateRatingSettingsMenu()
    {
        if (SelectedBuilding != null)
        {
            MainMenu.RatingSettingsMenu.SpinboxMin.SetBoundaries(LowestPossibleRating, SelectedBuilding.RatingMax);
            MainMenu.RatingSettingsMenu.SpinboxMax.SetBoundaries(SelectedBuilding.RatingMin, HighestPossibleRating);

            MainMenu.RatingSettingsMenu.SpinboxMin.SetValue(SelectedBuilding.RatingMin);
            MainMenu.RatingSettingsMenu.SpinboxMax.SetValue(SelectedBuilding.RatingMax);
        }
    }

    /// <summary>
    /// Update UI Set Rating Panel
    /// </summary>
    private void UpdateSetRatingPanel()
    {

        if (SelectedBuilding != null)
        {
            MainMenu.SetRatingMenu.Spinbox.SetBoundaries(SelectedBuilding.RatingMin, SelectedBuilding.RatingMax);

            if (SelectedBuilding != null && SelectedSubBuilding != null)
            {
                MainMenu.SetRatingMenu.SetObjectDisplayTag(SelectedSubBuilding.name);

                int spinVal = (SelectedSubBuilding.Rating != null) ? (int)SelectedSubBuilding.Rating : MinRating;

                MainMenu.SetRatingMenu.Spinbox.SetValue(spinVal);
            }
            else
            {
                MainMenu.SetRatingMenu.ClearObjectDisplayTagText();

                MainMenu.SetRatingMenu.Spinbox.SetValue(MinRating);
            }
        }
    }

    /// <summary>
    /// Update UI Analytics Panel
    /// </summary>
    private void UpdateAnalyticPanel()
    {
        if (SelectedBuilding != null && SelectedSubBuilding != null)
        {
            int[] numbers = SelectedBuilding.CalcNumbers();
            MainMenu.AnalyticsMenu.SetUnratedObjDisplay(numbers[0]);
            MainMenu.AnalyticsMenu.SetDummyObjNumDisplay(numbers[1]);
            MainMenu.AnalyticsMenu.SetUneditableObjNumDisplay(numbers[2]);
        }
    }

    /// <summary>
    /// Adds selected Annotation Object to the selected Building as SubBuildingController
    /// </summary>
    public void AddAnnotationObject()
    {
        if (BuildingSelected && SelectedAnno != null)
        {
            if (!SelectedAnno.IsConnected) SelectedAnno.transform.parent?.gameObject.Destroy();

            SelectedAnno.Connect(SelectedBuilding);
            SelectedAnno.gameObject.transform.SetParent(SelectedBuilding.transform);
            SubBuildingController subco = SelectedAnno.gameObject.AddComponent<SubBuildingController>();
            subco.Init(SelectedBuilding);
            subco.IsAnnoObj = true;
            SelectedBuilding.Children.Add(subco);

        }
    }

    /// <summary>
    /// Receives AnnotationObject and connects it to the selected Building
    /// </summary>
    public void AutoConnectAnno(AnnotationObject obj)
    {
        SelectedAnno = obj;
        Debug.LogWarning(obj.name);
        AddAnnotationObject();
    }

    /// <summary>
    /// Removes Selected Annotation Object
    /// </summary>
    public void RemoveAnnotationObject()
    {
        if (SelectedAnno != null)
        {
            SubBuildingController subco = SelectedAnno.gameObject.GetComponent<SubBuildingController>();
            if (subco != null)
            {
                if (subco.Main.Children.Contains(subco)) subco.Main.Children.Remove(subco);
            }

            SelectedAnno.gameObject.Destroy();
        }
    }

    public void DeleteSelectedReferences()
    {
        foreach (SubBuildingController subco in SelectedSubBuildings)
        {
            if (subco is ReferenceController)
            {
                ReferenceController reference = (ReferenceController) subco;
                if (SelectedBuilding.References.Contains((reference.Origin, reference.Target))) 
                    SelectedBuilding.References.Remove((reference.Origin, reference.Target));
                if (subco.Main.Children.Contains(subco)) subco.Main.Children.Remove(subco);
                Debug.LogWarning("Destroyed " + subco.name);
                subco.gameObject.Destroy();
                
            }
        }
    }

    //public void SaveBuildingData()
    //{
        

    //    if (BuildingSelected)
    //    {
    //        SelectedBuilding.Save();
    //    }
    //}

    /// <summary>
    /// Load Building Data
    /// </summary>
    //public void LoadBuildingData()
    //{
        
    //}

    public void LoadFromFile()
    {

    }

    /// <summary>
    /// Loads Saved Drawed Objects
    /// </summary>
    /// <param name="type"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    /// <param name="depth"></param>
    public void LoadDrawedObj(string type, int x, int y, int z, int height, int width, int depth)
    {
        GameObject dObject = null;

        if (type == "Rect")
        {
            dObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        else if (type == "Circle")
        {
            dObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        }
        // Add more types if necessary here
        else
        {
            return;
        }

        dObject.transform.localPosition = new Vector3(x, y, z);
        dObject.transform.localScale = new Vector3(height, width, depth);
    }
}