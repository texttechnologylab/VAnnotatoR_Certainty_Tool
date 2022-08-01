using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

public class UMAController : MonoBehaviour
{
    // DNA Keys
    // armLength, armWidth, belly,  breastSize, cheekPosition, cheekSize, chinPosition, chinPronounced, chinSize, earsPosition,
    // earsRotation earsSize, eyeRotation, eyeSpacing, eyeSize, feetSize, forearmLength, forearmWidth, foreheadPosition, foreheadSize,
    // gluteusSize, handsSize, headSize, headWidth, height, jawsPosition, jawsSize, legSeparation, legsSize, lipsSize, lowCheekPosition,
    // lowCheekPronounced, lowerMuscle, lowerWeight, mandibleSize, mouthSize, neckThickness, noseCurve, noseFlatten, noseInclination,
    // nosePosition, nosePronounced, noseSize, noseWidth, skinBlueness, skinGreenness, skinRedness, upperMuscle, upperWeight, waist,

    public bool Initialized { get { return _dna != null; } }
    private DynamicCharacterAvatar _dynamicCharacterAvatar;
    private Dictionary<string, DnaSetter> _dna;

    private Dictionary<string, float> _dnaValues = new Dictionary<string, float>();
    private Dictionary<string, string> _recipes = new Dictionary<string, string>();

    public UMAModel Model { get; private set; }

    void Start()
    {
        // Do nothing
        _dnaValues.Add("height", 1F);
        _dnaValues.Add("handsSize", 0.6F);
        _dnaValues.Add("noseSize", 0.4F);
        _dnaValues.Add("mouthSize", 0.3F);
        _dnaValues.Add("belly", 0.6F);
        _dnaValues.Add("armWidth", 0.6F);
        _dnaValues.Add("armLength", 0.6F);

        _recipes.Add("Hair", "MaleHair1");
        _recipes.Add("Chest", "MaleShirt1");
        _recipes.Add("Legs", "MalePants");
        if (Model == null) Model = UMAModel.GetModel(_dynamicCharacterAvatar.activeRace.name);
    }

    void Update()
    {
        // Do nothing
    }

    void OnEnable()
    {
        _dynamicCharacterAvatar = GetComponent<DynamicCharacterAvatar>();
        _dynamicCharacterAvatar.CharacterUpdated.AddListener(Updated);
        if (Model == null) Model = UMAModel.GetModel(_dynamicCharacterAvatar.activeRace.name);
    }

    void OnDisable()
    {
        _dynamicCharacterAvatar.CharacterUpdated.RemoveListener(Updated);
    }

    void Updated(UMAData data)
    {
        _dna = _dynamicCharacterAvatar.GetDNA();
    }

    /// <summary>
    /// Changes the race of this UMA to the given race
    /// </summary>
    /// <param name="raceName">the race to change to</param>
    public void ChangeRace(string raceName)
    {
        if (_dynamicCharacterAvatar.activeRace.name != raceName)
        {
            _dynamicCharacterAvatar.ChangeRace(raceName);
        }
    }

    /// <summary>
    /// Loads an example UMA recipe
    /// </summary>
    public void LoadBaseModel()
    {
        BuildAvatar(UMAModel.HumanMale, new Color(1, 0.8F, 0.5F), _dnaValues, _recipes);
    }

    /// <summary>
    /// Builds an UMA with the given parameters
    /// </summary>
    /// <param name="raceName">the race to set</param>
    /// <param name="skinColor">the skin color to set</param>
    /// <param name="dnaValues">a dictionary of dna values for the uma</param>
    /// <param name="wardrobeRecipes">a dictionary of wardrobe recipes</param>
    public void BuildAvatar(UMAModel model, Color skinColor, Dictionary<string, float> dnaValues, Dictionary<string, string> wardrobeRecipes)
    {
        ChangeGender(model);
        ChangeSkinColor(skinColor);
        foreach (string dnaKey in dnaValues.Keys)
        {
            _dna[dnaKey].Set(dnaValues[dnaKey]);
        }
        _dynamicCharacterAvatar.BuildCharacter();
        foreach (string recipeKey in wardrobeRecipes.Keys)
        {
            if (wardrobeRecipes[recipeKey] == "None")
            {
                _dynamicCharacterAvatar.ClearSlot(recipeKey);
                _dynamicCharacterAvatar.BuildCharacter();
            }
            else
            {
                _dynamicCharacterAvatar.SetSlot(recipeKey, wardrobeRecipes[recipeKey]);
                _dynamicCharacterAvatar.BuildCharacter();
            }
        }
    }

    public void BuildAvatar(string model, string skinColorHex, Dictionary<UMAProperty, string> properties)
    {
        ChangeGender(UMAModel.GetModel(model));
        if(ColorUtility.TryParseHtmlString(skinColorHex, out Color newCol1)) ChangeSkinColor(newCol1);
        foreach (UMAProperty property in properties.Keys)
        {
            if (property.IsDNA) _dna[property.Value].Set(NumericHelper.ParseFloat(properties[property]));
            else if(!property.IsSpecial)
            {
                if (properties[property].Equals("None")) _dynamicCharacterAvatar.ClearSlot(property.Value);
                else _dynamicCharacterAvatar.SetSlot(property.Value, properties[property]);
                _dynamicCharacterAvatar.BuildCharacter();
            }
        }
        _dynamicCharacterAvatar.BuildCharacter();
    }

    /// <summary>
    /// Builds an UMA with the given paramters
    /// </summary>
    /// <param name="dnaValues">a dictionary of dna values for the uma</param>
    public void BuildAvatar(Dictionary<string, float> dnaValues)
    {
        foreach (string key in dnaValues.Keys)
        {
            _dna[key].Set(dnaValues[key]);
        }
        _dynamicCharacterAvatar.BuildCharacter();
    }

    /// <summary>
    /// Builds an UMA with the given parameters
    /// </summary>
    /// <param name="wardrobeRecipes">a dictionary of wardrobe recipes</param>
    public void BuildAvatar(Dictionary<string, string> wardrobeRecipes)
    {
        foreach (string key in wardrobeRecipes.Keys)
        {
            if (wardrobeRecipes[key] == "None")
            {
                _dynamicCharacterAvatar.ClearSlot(key);
            }
            else
            {
                _dynamicCharacterAvatar.SetSlot(key, wardrobeRecipes[key]);
            }
        }
        _dynamicCharacterAvatar.BuildCharacter();
    }

    /// <summary>
    /// Parses the race name.
    /// </summary>
    /// <param name="raceName">the race to set</param>
    /// <return>a list of strings, representing the parsed race name</return>
    private List<string> ParseRace(string raceName)
    {
        List<string> retVal = new List<string>();
        retVal.Add("Human");
        raceName = raceName.Substring(5);
        if (raceName.StartsWith("Female"))
        {
            retVal.Add("Female");
            raceName = raceName.Substring(6);
        }
        else if (raceName.StartsWith("Male"))
        {
            retVal.Add("Male");
            raceName = raceName.Substring(4);
        }
        else
        {
            retVal.Add("Girl");
            return retVal;
        }
        if (raceName.Length > 0)
        {
            retVal.Add(raceName);
        }
        return retVal;
    }

    /// <summary>
    /// Switches the gender of this uma between male and female
    /// </summary>
    public void ChangeGender()
    {
        List<string> raceValues = ParseRace(_dynamicCharacterAvatar.activeRace.name);
        if (raceValues[1] != "Girl")
        {
            if (raceValues[1] == "Male")
            {
                raceValues[1] = "Female";
            }
            else
            {
                raceValues[1] = "Male";
            }
            ChangeRace(string.Join("", raceValues));
        }
    }

    public float GetValue(UMAProperty property)
    {
        if (!property.IsDNA) return 0;
        if (_dna == null) _dna = _dynamicCharacterAvatar.GetDNA();
        return _dna.ContainsKey(property.Value) ? _dna[property.Value].Value : 0;
    }

    public string GetWardrobeValue(UMAProperty property)
    {
        if (property.IsDNA || property.IsSpecial) return null;
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey(property.Value) ? _dynamicCharacterAvatar.WardrobeRecipes[property.Value].name : "None";
    }

    public void ChangeValue(UMAProperty property, float value)
    {
        if(property.IsDNA || !property.IsSpecial)
        {
            _dna[property.Value].Set(value);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public void ChangeValue(UMAProperty property, string value)
    {
        if (value == "None") _dynamicCharacterAvatar.ClearSlot(property.Value);
        else  _dynamicCharacterAvatar.SetSlot(property.Value, value);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public Color GetSkinColor()
    {
        return _dynamicCharacterAvatar.GetColor("Skin").color;
    }

    public UMAModel GetModel()
    {
        if (Model == null) Model = UMAModel.GetModel(_dynamicCharacterAvatar.activeRace.name);
        return Model;
    }

    /// <summary>
    /// Switches the gender of this uma between male and female
    /// </summary>
    public void ChangeGender(UMAModel model)
    {
        ChangeRace(model.Value);
        Model = model;
    }

    public void ChangeArmLength(float armLength)
    {
        _dna["armLength"].Set(armLength);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeArmWidth(float armWidth)
    {
        _dna["armWidth"].Set(armWidth);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeBelly(float belly)
    {
        _dna["belly"].Set(belly);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeBreastSize(float breastSize)
    {
        _dna["breastSize"].Set(breastSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeCheekPositon(float cheekPosition)
    {
        _dna["cheekPosition"].Set(cheekPosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeCheekSize(float cheekSize)
    {
        _dna["cheekSize"].Set(cheekSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeChinPosition(float chinPosition)
    {
        _dna["chinPosition"].Set(chinPosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeChinPronounced(float chinPronounced)
    {
        _dna["chinPronounced"].Set(chinPronounced);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeChinSize(float chinSize)
    {
        _dna["chinSize"].Set(chinSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeEarsPosition(float earsPosition)
    {
        _dna["earsPosition"].Set(earsPosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeEarsRotation(float earsRotation)
    {
        _dna["earsRotation"].Set(earsRotation);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeEarsSize(float earsSize)
    {
        _dna["earsSize"].Set(earsSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeEyeRotation(float eyeRotation)
    {
        _dna["eyeRotation"].Set(eyeRotation);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeEyeSpacing(float eyeSpacing)
    {
        _dna["eyeSpacing"].Set(eyeSpacing);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeEyeSize(float eyeSize)
    {
        _dna["eyeSize"].Set(eyeSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeFeetSize(float feetSize)
    {
        _dna["feetSize"].Set(feetSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeForearmLength(float forearmLength)
    {
        _dna["forearmLength"].Set(forearmLength);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeForearmWidth(float forearmWidth)
    {
        _dna["forearmWidth"].Set(forearmWidth);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeForeheadPosition(float foreheadPosition)
    {
        _dna["foreheadPosition"].Set(foreheadPosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeForeheadSize(float foreheadSize)
    {
        _dna["foreheadSize"].Set(foreheadSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeGluteusSize(float gluteusSize)
    {
        _dna["gluteusSize"].Set(gluteusSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeHandsSize(float handsSize)
    {
        _dna["handsSize"].Set(handsSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeHeadSize(float headSize)
    {
        _dna["headSize"].Set(headSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeHeadWidth(float headWidth)
    {
        _dna["headWidth"].Set(headWidth);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeHeight(float height)
    {
        _dna["height"].Set(height);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeJawsPosition(float jawsPosition)
    {
        _dna["jawsPosition"].Set(jawsPosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeJawsSize(float jawsSize)
    {
        _dna["jawsSize"].Set(jawsSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeLegSeparation(float legSeparation)
    {
        _dna["legSeparation"].Set(legSeparation);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeLegsSize(float legsSize)
    {
        _dna["legsSize"].Set(legsSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeLipsSize(float lipsSize)
    {
        _dna["lipsSize"].Set(lipsSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeLowCheekPosition(float lowCheekPosition)
    {
        _dna["lowCheekPosition"].Set(lowCheekPosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeLowCheekPronounced(float lowCheekPronounced)
    {
        _dna["lowCheekPronounced"].Set(lowCheekPronounced);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeLowerMuscle(float lowerMuscle)
    {
        _dna["lowerMuscle"].Set(lowerMuscle);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeMandibleSize(float mandibleSize)
    {
        _dna["mandibleSize"].Set(mandibleSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeMouthSize(float mouthSize)
    {
        _dna["mouthSize"].Set(mouthSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNeckThickness(float neckThickness)
    {
        _dna["neckThickness"].Set(neckThickness);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNoseCurve(float noseCurve)
    {
        _dna["noseCurve"].Set(noseCurve);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNoseFlatten(float noseFlatten)
    {
        _dna["noseFlatten"].Set(noseFlatten);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNoseInclination(float noseInclination)
    {
        _dna["noseInclination"].Set(noseInclination);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNosePosition(float nosePosition)
    {
        _dna["nosePosition"].Set(nosePosition);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNosePronounced(float nosePronounced)
    {
        _dna["nosePronounced"].Set(nosePronounced);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNoseSize(float noseSize)
    {
        _dna["noseSize"].Set(noseSize);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeNoseWidth(float noseWidth)
    {
        _dna["noseWidth"].Set(noseWidth);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeSkinBlueness(float skinBlueness)
    {
        _dna["skinBlueness"].Set(skinBlueness);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeSkinColor(Color skinColor)
    {
        _dynamicCharacterAvatar.SetColor("Skin", skinColor);
        _dynamicCharacterAvatar.UpdateColors(true);
    }

    public void ChangeSkinGreenness(float skinGreenness)
    {
        _dna["skinGreenness"].Set(skinGreenness);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeSkinRedness(float skinRedness)
    {
        _dna["skinRedness"].Set(skinRedness);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeUpperMuscle(float upperMuscle)
    {
        _dna["upperMuscle"].Set(upperMuscle);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeUpperWeight(float upperWeight)
    {
        _dna["upperWeight"].Set(upperWeight);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    public void ChangeWaist(float waist)
    {
        _dna["waist"].Set(waist);
        _dynamicCharacterAvatar.BuildCharacter();
    }

    private List<string> _femaleHairStyles = new List<string>() { "FemaleHair1", "FemaleHair2", "FemaleHair3" };
    private List<string> _maleHairStyles = new List<string>() { "MaleHair1", "MaleHair2", "MaleHair3" };

    public void ChangeHair(string hairStyle)
    {
        if (hairStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Hair");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Female") && _femaleHairStyles.Contains(hairStyle) || _dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleHairStyles.Contains(hairStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Hair", hairStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetHair()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Hair") ? _dynamicCharacterAvatar.WardrobeRecipes["Hair"].name : "None";
    }

    private List<string> _femaleShirtStyles = new List<string>() { "FemaleShirt1", "FemaleShirt2", "FemaleShirt3", "FemaleShirt4" };
    private List<string> _maleShirtStyles = new List<string>() { "MaleShirt1", "MaleShirt2", "MaleShirt3" };

    public void ChangeChest(string shirtStyle)
    {
        if (shirtStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Chest");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Female") && _femaleShirtStyles.Contains(shirtStyle) || _dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleShirtStyles.Contains(shirtStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Chest", shirtStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetChest()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Chest") ? _dynamicCharacterAvatar.WardrobeRecipes["Chest"].name : "None";
    }

    private List<string> _malePantsStyles = new List<string>() { "MaleJeans", "MaleJeans1", "MalePants", "MaleShorts1", "MaleShorts2" };
    private List<string> _femalePantsStyles = new List<string>() { "FemalePants1", "FemalePants2" };

    public void ChangeLegs(string pantsStyle)
    {
        if (pantsStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Legs");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Female") && _femalePantsStyles.Contains(pantsStyle) || _dynamicCharacterAvatar.activeRace.name.Contains("Male") && _malePantsStyles.Contains(pantsStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Legs", pantsStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetLegs()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Legs") ? _dynamicCharacterAvatar.WardrobeRecipes["Legs"].name : "None";
    }

    private List<string> _femaleUnderwearStyles = new List<string>() { "FemaleUndies1", "FemaleUndies2" };
    private List<string> _maleUnderwearStyles = new List<string>() { "MaleUnderwear" };

    public void ChangeUnderwear(string underwearStyle)
    {
        if (underwearStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Underwear");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Female") && _femaleUnderwearStyles.Contains(underwearStyle) || _dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleUnderwearStyles.Contains(underwearStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Underwear", underwearStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }

    }

    public string GetUnderwear()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Underwear") ? _dynamicCharacterAvatar.WardrobeRecipes["Underwear"].name : "None";
    }

    private List<string> _maleBeardStyles = new List<string>() { "MaleBeard1", "MaleBeard2", "MaleBeard3" };

    public void ChangeBeard(string beardStyle)
    {
        if (beardStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Beard");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleBeardStyles.Contains(beardStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Beard", beardStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetBeard()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Beard") ? _dynamicCharacterAvatar.WardrobeRecipes["Beard"].name : "None";
    }

    private List<string> _maleBrowStyles = new List<string>() { "MaleBrow01", "MaleBrow02", "MaleBrowHR" };

    public void ChangeBrow(string browStyle)
    {
        if (browStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Eyebrow");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleBrowStyles.Contains(browStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Eyebrow", browStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetEyebrow()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Eyebrow") ? _dynamicCharacterAvatar.WardrobeRecipes["Eyebrow"].name : "None";
    }

    private List<string> _maleEyesStyles = new List<string>() { "MaleEyes1", "MaleEyes2", "MaleEyes3", "MaleEyes4", "MaleEyes5" };

    public void ChangeEyes(string eyesStyle)
    {
        if (eyesStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Eyes");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleEyesStyles.Contains(eyesStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Eyes", eyesStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetEyes()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Eyes") ? _dynamicCharacterAvatar.WardrobeRecipes["Eyes"].name : "None";
    }

    private List<string> _maleFaceStyles = new List<string>() { "MaleFaceOld" };

    public void ChangeFace(string faceStyle)
    {
        if (faceStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Face");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleFaceStyles.Contains(faceStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Face", faceStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetFace()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Face") ? _dynamicCharacterAvatar.WardrobeRecipes["Face"].name : "None";
    }

    private List<string> _maleGloveStyles = new List<string>() { "MaleGloves" };

    public void ChangeHands(string eyesStyle)
    {
        if (eyesStyle == "None")
        {
            _dynamicCharacterAvatar.ClearSlot("Hands");
            _dynamicCharacterAvatar.BuildCharacter();
        }
        else if (_dynamicCharacterAvatar.activeRace.name.Contains("Male") && _maleGloveStyles.Contains(eyesStyle))
        {
            _dynamicCharacterAvatar.SetSlot("Hands", eyesStyle);
            _dynamicCharacterAvatar.BuildCharacter();
        }
    }

    public string GetHands()
    {
        return _dynamicCharacterAvatar.WardrobeRecipes.ContainsKey("Hands") ? _dynamicCharacterAvatar.WardrobeRecipes["Hands"].name : "None";
    }

}