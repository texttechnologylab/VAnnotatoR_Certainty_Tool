using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMAModel
{
    private UMAModel(string value, bool male)
    {
        Value = value;
        IsMale = male;
    }

    public string Value { get; private set; }
    public bool IsMale { get; private set; }

    public static UMAModel HumanMale { get { return new UMAModel("HumanMaleDCS", true); } }
    public static UMAModel HumanFemale { get { return new UMAModel("HumanFemaleDCS", false); } }

    public static List<UMAModel> ModelList { get { return new List<UMAModel> { HumanMale, HumanFemale }; } }

    public static UMAModel GetModel(string name)
    {
        if(!ModelList.Exists(model => model.Value == name))
        {
            Debug.LogWarning("Could not find UMAModel for name: " + name);
            return null;
        }
        return ModelList.Find(model => model.Value == name);
    }

    public static string ToString(UMAModel model)
    {
        return model.Value;
    }


    public static List<string> StringModelList { get { return ModelList.ConvertAll(new System.Converter<UMAModel, string>(ToString)); } }
}