using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeTab : AvatarObjectPanelTab
{
    //private InteractiveSelectionMenu PhysiqueMenu;
    //private InteractiveSelectionMenu FullBodyOutfitMenu;
    //private InteractiveSelectionMenu AlternativeHeadMenu;
    //private InteractiveSelectionMenu AlternativeHandsMenu;
    //private InteractiveSelectionMenu CapeMenu;
    //private InteractiveSelectionMenu HelmetMenu;
    //private InteractiveSelectionMenu EarsMenu;
    //private InteractiveSelectionMenu BodyMenu;
    //private InteractiveSelectionMenu SkincolorMenu;
    //private InteractiveSelectionMenu TattooMenu;
    //private InteractiveSelectionMenu ShoulderMenu;
    //private InteractiveSelectionMenu ArmsMenu;
    //private InteractiveSelectionMenu HipsMenu;
    //private InteractiveSelectionMenu FeetMenu;

    private InteractiveSelectionMenu HairMenu;
    private InteractiveSelectionMenu ChestMenu;
    private InteractiveSelectionMenu LegsMenu;
    private InteractiveSelectionMenu UnderwearMenu;
    private InteractiveSelectionMenu BeardMenu;
    private InteractiveSelectionMenu EyebrowsMenu;
    private InteractiveSelectionMenu EyesMenu;
    private InteractiveSelectionMenu HandsMenu;
    private InteractiveSelectionMenu FaceMenu;
    private GameObject PageOne;
    private GameObject PageTwo;
    private InteractiveButton PreviousPage;
    private InteractiveButton NextPage;

    private UMAController AvatarController;

    private readonly int _maxIndex = 1;
    private int _pageIndex;
    private int PageIndex
    {
        get { return _pageIndex; }
        set
        {
            _pageIndex = value;
            PageOne.SetActive(_pageIndex == 0);
            PageTwo.SetActive(_pageIndex == 1);
            PreviousPage.Active = _pageIndex > 0;
            NextPage.Active = _pageIndex < _maxIndex;
        }
    }

    private UMAWardrobe _femaleHairStyles = new UMAWardrobe("female hair", UMAModel.HumanFemale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Style 1", "FemaleHair1"),
        new UMAWardrobeItem("Style 2", "FemaleHair2"),
        new UMAWardrobeItem("Style 3", "FemaleHair3")
    });
    private UMAWardrobe _maleHairStyles = new UMAWardrobe("male hair", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Style 1", "MaleHair1"),
        new UMAWardrobeItem("Style 2", "MaleHair2"),
        new UMAWardrobeItem("Style 3", "MaleHair3")
    });

    private UMAWardrobe _femaleShirtStyles = new UMAWardrobe("female shirts", UMAModel.HumanFemale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Style 1", "FemaleShirt1"),
        new UMAWardrobeItem("Style 2", "FemaleShirt2"),
        new UMAWardrobeItem("Style 3", "FemaleShirt3"),
        new UMAWardrobeItem("Style 4", "FemaleShirt4")
    });
    private UMAWardrobe _maleShirtStyles = new UMAWardrobe("male shirts", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Style 1", "MaleShirt1"),
        new UMAWardrobeItem("Style 2", "MaleShirt2"),
        new UMAWardrobeItem("Style 3", "MaleShirt3")
    });

    private UMAWardrobe _malePantsStyles = new UMAWardrobe("male pants", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Jeans 1", "MaleJeans"),
        new UMAWardrobeItem("Jeans 2", "MaleJeans1"),
        new UMAWardrobeItem("Hose", "MalePants"),
        new UMAWardrobeItem("Shorts 1", "MaleShorts1"),
        new UMAWardrobeItem("Shorts 2", "MaleShorts2")
    });
    private UMAWardrobe _femalePantsStyles = new UMAWardrobe("female pants", UMAModel.HumanFemale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Hose 1", "FemalePants1"),
        new UMAWardrobeItem("Hose 2", "FemalePants2")
    });

    private UMAWardrobe _femaleUnderwearStyles = new UMAWardrobe("female underwear", UMAModel.HumanFemale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Unterwäsche 1", "FemaleUndies1"),
        new UMAWardrobeItem("Unterwäsche 2", "FemaleUndies2")
    });
    private UMAWardrobe _maleUnderwearStyles = new UMAWardrobe("male underwear", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Unterwäsche", "MaleUnderwear")
    });

    private UMAWardrobe _maleBeardStyles = new UMAWardrobe("male bears", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Bart 1", "MaleBeard1"),
        new UMAWardrobeItem("Bart 2", "MaleBeard2"),
        new UMAWardrobeItem("Bart 3", "MaleBeard3")
    });

    private UMAWardrobe _maleBrowStyles = new UMAWardrobe("male brows", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Augenbraue 1", "MaleBrow01"),
        new UMAWardrobeItem("Augenbraue 2", "MaleBrow02"),
        new UMAWardrobeItem("Augenbraue 3", "MaleBrowHR")
    });

    private UMAWardrobe _maleEyesStyles = new UMAWardrobe("male eyes", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Augen 1", "MaleEyes1"),
        new UMAWardrobeItem("Augen 2", "MaleEyes2"),
        new UMAWardrobeItem("Augen 3", "MaleEyes3"),
        new UMAWardrobeItem("Augen 4", "MaleEyes4"),
        new UMAWardrobeItem("Augen 5", "MaleEyes5")
    });

    private UMAWardrobe _maleFaceStyles = new UMAWardrobe("male faces", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Gesicht alt", "MaleFaceOld")
    });

    private UMAWardrobe _maleGloveStyles = new UMAWardrobe("male gloves", UMAModel.HumanMale, new List<UMAWardrobeItem>
    {
        new UMAWardrobeItem("Handschuhe", "MaleGloves")
    });

    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("WardrobeButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'Wardrobe' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }

        // The following Wardrobe slots don't currently have any wardrobe recipes
        //PhysiqueMenu = transform.Find("PhysiqueMenu").GetComponent<InteractiveSelectionMenu>();
        //FullBodyOutfitMenu = transform.Find("FullBodyOutfitMenu").GetComponent<InteractiveSelectionMenu>();
        //AlternativeHeadMenu = transform.Find("AlternativeHeadMenu").GetComponent<InteractiveSelectionMenu>();
        //AlternativeHandsMenu = transform.Find("AlternativeHandsMenu").GetComponent<InteractiveSelectionMenu>();
        //CapeMenu = transform.Find("CapeMenu").GetComponent<InteractiveSelectionMenu>();
        //HelmetMenu = transform.Find("HelmetMenu").GetComponent<InteractiveSelectionMenu>();
        //EarsMenu = transform.Find("EarsMenu").GetComponent<InteractiveSelectionMenu>();
        //BodyMenu = transform.Find("BodyMenu").GetComponent<InteractiveSelectionMenu>();
        //SkincolorMenu = transform.Find("SkincolorMenu").GetComponent<InteractiveSelectionMenu>();
        //TattooMenu = transform.Find("TattooMenu").GetComponent<InteractiveSelectionMenu>();
        //ShoulderMenu = transform.Find("ShoulderMenu").GetComponent<InteractiveSelectionMenu>();
        //ArmsMenu = transform.Find("ArmsMenu").GetComponent<InteractiveSelectionMenu>();
        //HipsMenu = transform.Find("HipsMenu").GetComponent<InteractiveSelectionMenu>();
        //FeetMenu = transform.Find("FeetMenu").GetComponent<InteractiveSelectionMenu>();
        //
        //PhysiqueMenu.Initialize(new List<string> { });
        //FullBodyOutfitMenu.Initialize(new List<string> { });
        //AlternativeHeadMenu.Initialize(new List<string> { });
        //AlternativeHandsMenu.Initialize(new List<string> { });
        //CapeMenu.Initialize(new List<string> { });
        //HelmetMenu.Initialize(new List<string> { });
        //EarsMenu.Initialize(new List<string> { });
        //BodyMenu.Initialize(new List<string> { });
        //SkincolorMenu.Initialize(new List<string> { });
        //TattooMenu.Initialize(new List<string> { });
        //ShoulderMenu.Initialize(new List<string> { });
        //ArmsMenu.Initialize(new List<string> { });
        //HipsMenu.Initialize(new List<string> { });
        //FeetMenu.Initialize(new List<string> { });

        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        PageOne = transform.Find("Page1").gameObject;
        PageTwo = transform.Find("Page2").gameObject;
        PreviousPage = transform.Find("PreviousPageButton").GetComponent<InteractiveButton>();
        NextPage = transform.Find("NextPageButton").GetComponent<InteractiveButton>();

        HairMenu = PageOne.transform.Find("SelectHair").GetComponentInChildren<InteractiveSelectionMenu>();
        ChestMenu = PageOne.transform.Find("SelectChest").GetComponentInChildren<InteractiveSelectionMenu>();
        UnderwearMenu = PageOne.transform.Find("SelectUnderwear").GetComponentInChildren<InteractiveSelectionMenu>();
        LegsMenu = PageOne.transform.Find("SelectPants").GetComponentInChildren<InteractiveSelectionMenu>();
        FaceMenu = PageTwo.transform.Find("SelectFace").GetComponentInChildren<InteractiveSelectionMenu>();
        EyesMenu = PageTwo.transform.Find("SelectEyes").GetComponentInChildren<InteractiveSelectionMenu>();
        EyebrowsMenu = PageTwo.transform.Find("SelectEyebrows").GetComponentInChildren<InteractiveSelectionMenu>();
        BeardMenu = PageTwo.transform.Find("SelectBeard").GetComponentInChildren<InteractiveSelectionMenu>();
        HandsMenu = PageTwo.transform.Find("SelectHands").GetComponentInChildren<InteractiveSelectionMenu>();
        
        PreviousPage.OnClick = () => { if (_pageIndex > 0) PageIndex = _pageIndex - 1; };
        NextPage.OnClick = () => { if (_pageIndex < _maxIndex) PageIndex = _pageIndex + 1; };

        SetActive(false);
        return this;
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        if (active)
        {
            InitMenues();
            PageIndex = 0;
        }
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_HAIR, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_HAIR).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_CHEST, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_CHEST).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_UNDERWEAR, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_UNDERWEAR).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_LEGS, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_LEGS).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_FACE, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_FACE).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_EYES, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_EYES).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_EYEBROW, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_EYEBROW).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_BEARD, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_BEARD).ToString());
        umaISOObject.Wardrobe.SetProperty(UMAProperty.WARDROBE_HANDS, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_HANDS).ToString());
        base.Save(umaISOObject);
    }

    private void InitMenues()
    {
        if (AvatarController.Model.IsMale)
        {
            HairMenu.Initialize(_maleHairStyles.GetItemNames(), _maleHairStyles.GetFromIdentifier(AvatarController.GetHair()).Name);
            ChestMenu.Initialize(_maleShirtStyles.GetItemNames(), _maleShirtStyles.GetFromIdentifier(AvatarController.GetChest()).Name);
            UnderwearMenu.Initialize(_maleUnderwearStyles.GetItemNames(), _maleUnderwearStyles.GetFromIdentifier(AvatarController.GetUnderwear()).Name);
            LegsMenu.Initialize(_malePantsStyles.GetItemNames(), _malePantsStyles.GetFromIdentifier(AvatarController.GetLegs()).Name);
            FaceMenu.Initialize(_maleFaceStyles.GetItemNames(), _maleFaceStyles.GetFromIdentifier(AvatarController.GetFace()).Name);
            EyebrowsMenu.Initialize(_maleBrowStyles.GetItemNames(), _maleBrowStyles.GetFromIdentifier(AvatarController.GetEyebrow()).Name);
            EyesMenu.Initialize(_maleEyesStyles.GetItemNames(), _maleEyesStyles.GetFromIdentifier(AvatarController.GetEyes()).Name);
            BeardMenu.Initialize(_maleBeardStyles.GetItemNames(), _maleBeardStyles.GetFromIdentifier(AvatarController.GetBeard()).Name);
            HandsMenu.Initialize(_maleGloveStyles.GetItemNames(), _maleGloveStyles.GetFromIdentifier(AvatarController.GetHands()).Name);

            FaceMenu.OnSelectionChange = (p, n) => AvatarController.ChangeFace(_maleFaceStyles.GetItem(n).Identifier);
            EyebrowsMenu.OnSelectionChange = (p, n) => AvatarController.ChangeBrow(_maleBrowStyles.GetItem(n).Identifier);
            EyesMenu.OnSelectionChange = (p, n) => AvatarController.ChangeEyes(_maleEyesStyles.GetItem(n).Identifier);
            BeardMenu.OnSelectionChange = (p, n) => AvatarController.ChangeBeard(_maleBeardStyles.GetItem(n).Identifier);
            HandsMenu.OnSelectionChange = (p, n) => AvatarController.ChangeHands(_maleGloveStyles.GetItem(n).Identifier);
            HairMenu.OnSelectionChange = (p, n) => AvatarController.ChangeHair(_maleHairStyles.GetItem(n).Identifier);
            ChestMenu.OnSelectionChange = (p, n) => AvatarController.ChangeChest(_maleShirtStyles.GetItem(n).Identifier);
            UnderwearMenu.OnSelectionChange = (p, n) => AvatarController.ChangeUnderwear(_maleUnderwearStyles.GetItem(n).Identifier);
            LegsMenu.OnSelectionChange = (p, n) => AvatarController.ChangeLegs(_malePantsStyles.GetItem(n).Identifier);
        }
        else
        {
            HairMenu.Initialize(_femaleHairStyles.GetItemNames(), _femaleHairStyles.GetFromIdentifier(AvatarController.GetHair()).Name);
            ChestMenu.Initialize(_femaleShirtStyles.GetItemNames(), _femaleShirtStyles.GetFromIdentifier(AvatarController.GetChest()).Name);
            UnderwearMenu.Initialize(_femaleUnderwearStyles.GetItemNames(), _femaleUnderwearStyles.GetFromIdentifier(AvatarController.GetUnderwear()).Name);
            LegsMenu.Initialize(_femalePantsStyles.GetItemNames(), _femalePantsStyles.GetFromIdentifier(AvatarController.GetLegs()).Name);

            HairMenu.OnSelectionChange = (p, n) => AvatarController.ChangeHair(_femaleHairStyles.GetItem(n).Identifier);
            ChestMenu.OnSelectionChange = (p, n) => AvatarController.ChangeChest(_femaleShirtStyles.GetItem(n).Identifier);
            UnderwearMenu.OnSelectionChange = (p, n) => AvatarController.ChangeUnderwear(_femaleUnderwearStyles.GetItem(n).Identifier);
            LegsMenu.OnSelectionChange = (p, n) => AvatarController.ChangeLegs(_femalePantsStyles.GetItem(n).Identifier);
        }
    }
}