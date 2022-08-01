using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class BuildingController : MonoBehaviour
{
    public string Name { get; protected set; }
    public GameObject GameObject { get; protected set; }

    public List<SubBuildingController> Children { get; protected set; }

    public bool IsColored { get; protected set; } = false;
    public bool IsSelected { get; protected set; } = false;
    public bool IsTransparent { get; protected set; } = false;
    public bool HideOutsideRatingBoundaries { get; protected set; } = false;
    public int RatingMin { get; protected set; }
    public int RatingMax { get; protected set; }

    public int ShowMin { get; protected set; }
    public int ShowMax { get; protected set; }
    public bool EditMode { get; private set; } = false;
    public bool ReferenceMode { get; private set; } = false;

    public List<(SubBuildingController, SubBuildingController)> References;

    public enum SelectionType { House, Object }
    public SelectionType SelectionMode;

    public enum DisplayType { Transparent, Hidden }
    public DisplayType DisplayMode = DisplayType.Transparent;

    private void Awake()
    {
        GameObject = transform.gameObject;
        Name = GameObject.name;
        References = new List<(SubBuildingController, SubBuildingController)>();
        ParseObjects();
    }

    protected void ParseObjects()
    {
        Children = new List<SubBuildingController>();
        foreach (Transform child in transform)
        {
            Children.Add(child.gameObject.AddComponent<SubBuildingController>());
            child.gameObject.GetComponent<SubBuildingController>().Init(this);
        }

        gameObject.GetComponent<RatingFaker>()?.Init();
    }

    public void SetActive(bool b)
    {
        transform.gameObject.SetActive(b);
    }

    public void SetEditMode(bool b)
    {
        EditMode = b;
        UpdateVisualization();
    }

    public void SetReferenceMode(bool b)
    {
        ReferenceMode = b;
        UpdateVisualization();
    }

    public virtual void ShowColor(bool b)
    {
        IsColored = b;
        foreach (SubBuildingController child in Children)
        {
            child.ShowColor(b);
        }
    }

    public void SetDisplayMode(DisplayType type)
    {
        DisplayMode = type;
        foreach (SubBuildingController child in Children)
        {
            child.UpdateVisualization();
        }

        ShowColor(IsColored);
    }

    public virtual void MakeTransparent(bool b)
    {
        if (b) DisplayMode = DisplayType.Transparent;
        else DisplayMode = DisplayType.Hidden;
        UpdateVisualization();
        //foreach (SubBuildingController child in Children)
        //{
        //    child.MakeTransparent(b);
        //}

        ShowColor(IsColored);
    }
    public void UpdateVisualization()
    {

        foreach (SubBuildingController child in Children)
        {
            child.UpdateVisualization();
        }
    }
    public void SetShowBoundaries(int minRating, int maxRating)
    {
        ShowMin = minRating;
        ShowMax = maxRating;

        UpdateVisualization();
    }

    public void SetRatingBoundaries(int minRating, int maxRating)
    {
        RatingMin = minRating;
        RatingMax = maxRating;

        if (ShowMin < RatingMin) ShowMin = RatingMin;
        if (ShowMax > RatingMax) ShowMax = RatingMin;

        foreach (SubBuildingController child in Children)
        {
            child._SetRatingBoundaries();
        }

        ShowColor(IsColored);
    }

    public virtual void SelectAll()
    {
        foreach (SubBuildingController child in Children)
        {
            child.Select(false);
        }
    }

    public virtual void Deselect()
    {
        foreach (SubBuildingController child in Children)
        {
            child.Deselect();
        }

        UpdateVisualization();
    }

    public bool IsInShowBoundaries(int? num)
    {
        if (num >= ShowMin && num <= ShowMax) return true;
        else return false;
    }

    public bool IsInRatingBoundaries(int? num)
    {
        if (num >= RatingMin && num <= RatingMax) return true;
        else return false;
    }

    public void OnBuildingChanged()
    {
        ShowColor(false);
        EditMode = false;
        Deselect();
        SetDisplayMode(DisplayType.Transparent);
        SetShowBoundaries(RatingMin, RatingMax);
    }

    public int[] CalcNumbers()
    {
        int[] numbers = new int[3];
        numbers[0] = 0; // Unrated Objects Num
        numbers[1] = 0; // Dummy Objects Num
        numbers[2] = 0; // Uneditable Objects Num
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

    public void CreateReferences(SubBuildingController origin, List<SubBuildingController> targets, bool reflexive)
    {
        foreach (SubBuildingController target in targets)
        {
            if (!References.Contains((origin, target)) && origin != target
                && origin.Refernceable && target.Refernceable)
            {
                BuildReference(origin, target, reflexive);
            }
        }

        
    }

    public void BuildReference(SubBuildingController origin, SubBuildingController target, bool reflexive)
    {
        References.Add((origin, target));

        Vector3 PosOrigin = origin.transform.position;
        Vector3 PosTarget = target.transform.position;

        Vector3 PosNew = PosOrigin / 2f + PosTarget / 2f;

        float distance = Vector3.Distance(PosOrigin, PosTarget);

        Vector3 rotation = PosTarget - PosOrigin;



        PrimitiveType pType = PrimitiveType.Cube;


        // GameObject go = GameObject.CreatePrimitive(pType);
        //lowpoly_cylinder
        GameObject go = new GameObject();
        GameObject cylinder = Instantiate(Resources.Load<GameObject>("Prefabs/lowpoly_cylinder"));

        // go.SetActive(false);

        go.name = "Connection " + origin.Name + " " + target.Name;
        //Debug.LogWarning(go.name);

        //Debug.LogWarning(PosNew.ToString());


        // Position
        go.transform.position = PosNew;
        cylinder.transform.position = PosNew;
        // Scale
        cylinder.transform.localScale = new Vector3(.05f, .05f, distance/2f);
        // Rotation
        cylinder.transform.localRotation = Quaternion.LookRotation(rotation);

        if (!reflexive)
        {
            GameObject arrow = Instantiate(Resources.Load<GameObject>("Prefabs/lowpoly_cone"));
            // arrow.transform.position = PosNew;
            arrow.transform.localRotation = Quaternion.LookRotation(rotation);
            arrow.transform.position = PosTarget;
            arrow.transform.position -= arrow.transform.forward * .15f;
            arrow.transform.localScale = new Vector3(.15f, .15f, .15f);

            cylinder.transform.position -= arrow.transform.forward * .15f;
            cylinder.transform.localScale -= new Vector3(0, 0, .15f);

            arrow.transform.SetParent(go.transform, true);
        }

        cylinder.transform.SetParent(go.transform, true);

        go.transform.SetParent(gameObject.transform, true);

        cylinder.AddComponent<MeshCollider>();

        ReferenceController refco = go.AddComponent<ReferenceController>();
        refco.Init(this, reflexive, origin, target);
        // subco.DefineAsRefObj(reflexive);

        Children.Add(refco);
    }

    public void UpdateReferenceTags(string text)
    {
        foreach(SubBuildingController child in Children)
        {
            if (child is ReferenceController)
            {
                ((ReferenceController)child).UpdateRefTag(text);
            }
        }
    }

    public List<(string, SubBuildingController)> ReturnNameObjTuples()
    {
        List<(string, SubBuildingController)> nameObjList = new List<(string, SubBuildingController)>();
        foreach (SubBuildingController child in Children)
        {
            nameObjList.AddRange(child.ReturnNameObjTuples());
        }
        return nameObjList;
    }

    public void MakeNamesUnique()
    {
        List<(string, SubBuildingController)> nameObjList = ReturnNameObjTuples();

        List<string> nameList = new List<string>();

        foreach ((string name, SubBuildingController subco) in nameObjList)
        {
            if (nameList.Contains(name)) subco.name += "1";


        }
    }

    /// <summary>
    /// Saves Building Data to DB
    /// </summary>
    public void Save(string name)
    {
        string path = "Assets/Stolperwege/CertaintyTool/saves/" + name  + ".txt";

        List<List<string>> savestrings = new List<List<string>>();

        foreach (SubBuildingController child in Children)
        {
            savestrings.AddRange(child.GenSaveStrings());
        }

        File.WriteAllText(path, "");

        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine(Name + " || " + RatingMin.ToString() + " || " + RatingMax.ToString());
        

        foreach (List<string> strings in savestrings)
        {
            string lineString = "";

            bool first = true;
            foreach (string el in strings)
            {
                if (!first) lineString += " || ";
                lineString += el;
                first = false;
            }

            writer.WriteLine(lineString);
        }

        writer.Close();
    }

    public void LoadFromTestFile(string filename)
    {
        string path = "Assets/Stolperwege/CertaintyTool/saves/" + filename + ".txt";
        //string path = filename;
        StreamReader reader = new StreamReader(path);

        string line;
        int AnnoNum = 0;
        while ((line = reader.ReadLine()) != null)
        {
            // Debug.Log(line);
            string[] subs = line.Split(new[] { " || " }, StringSplitOptions.None);

            if (subs.Length == 3)
            {
                string name = subs[0];
                int minRating = int.Parse(subs[1]);
                int maxRating = int.Parse(subs[2]);

                SetRatingBoundaries(minRating, maxRating);
                SetShowBoundaries(minRating, maxRating);
            }
            else if (subs.Length == 2)
            {
                string name = subs[0];
                int? rating = null;
                if (subs[1] != "null") rating = int.Parse(subs[1]);

                FindChild(name).GetComponent<SubBuildingController>().SetRating(rating, false, true);
            }
            else if (subs.Length == 12)
            {
                Debug.LogWarning("Loading Annotation...");
                string name = subs[0];
                int? rating = null;
                if (subs[1] != "null") rating = int.Parse(subs[1]);

                string type = subs[2];

                Vector3[] vectors = new Vector3[3];

                for (int i = 0; i < 3; i++)
                {
                    float x = float.Parse(subs[3 + 3 * i]);
                    float y = float.Parse(subs[4 + 3 * i]);
                    float z = float.Parse(subs[5 + 3 * i]);

                    vectors[i] = new Vector3(x, y, z);
                }

                // Recreate Annotation Object
                PrimitiveType pType = PrimitiveType.Cube;
                if (type == "Rect")
                {
                    pType = PrimitiveType.Cube;
                }
                else if (type == "Circle")
                {
                    pType = PrimitiveType.Cylinder;
                }

                GameObject go = GameObject.CreatePrimitive(pType);
                if (type == "Circle")
                {
                    Destroy(go.GetComponent<CapsuleCollider>());
                    go.AddComponent<MeshCollider>();
                }
                go.name = "Annotation " + AnnoNum.ToString();
                Debug.LogWarning("Primitive created: " + go.name);
                go.transform.SetParent(gameObject.transform);
                // Position
                go.transform.localPosition = vectors[0];
                // Scale
                go.transform.localScale = vectors[1];
                // Rotation
                go.transform.localRotation = Quaternion.Euler(vectors[2]);

                SubBuildingController subco = go.AddComponent<SubBuildingController>();
                subco.Init(this);

                Children.Add(subco);

                go.AddComponent<AnnotationObject>().Type = type;
                subco.IsAnnoObj = true;
                subco.SetRating(rating, false, true);
                AnnoNum++;
            }
            else if (subs.Length == 5)
            {
                string name = subs[0];
                int? rating = null;
                if (subs[1] != "null") rating = int.Parse(subs[1]);

                SubBuildingController origin = FindChild(subs[2])?.GetComponent<SubBuildingController>();
                SubBuildingController target = FindChild(subs[3])?.GetComponent<SubBuildingController>();
                bool reflexive = bool.Parse(subs[4]);

                Debug.LogWarning("LoadReference: " + origin.Name + " and " + target.Name);

                if (origin != null && target != null)
                {
                    CreateReferences(origin, new List<SubBuildingController>() { target }, reflexive);
                }

                if (rating != null)
                {
                    FindChild(name)?.GetComponent<SubBuildingController>().SetRating(rating, false, true);
                }
            }
        }
        UpdateVisualization();
        reader.Close();
    }

}
