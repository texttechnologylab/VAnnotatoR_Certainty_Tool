using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SelectionMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public InteractiveButton ButtonHide { get; private set; }
    public InteractiveButton ButtonColor { get; private set; }
    public InteractiveButton ButtonEdit { get; private set; }
    public InteractiveButton ButtonSave { get; private set; }
    public InteractiveButton ButtonLoad { get; private set; }
    public KeyboardEditText Textfield { get; private set; }

    public LoadBrowser LoadBrowser { get; private set; }


    public void Init(CertaintyToolInterface CTInterface)
    {
        Interface = CTInterface;
        Menu = transform.gameObject;

        InitButtons();

    }

    public void InitButtons()
    {
        ButtonHide = Menu.transform.Find("ButtonHide").gameObject.GetComponent<InteractiveButton>();
        ButtonHide.OnClick = ButtonHideOnClick;

        ButtonColor = Menu.transform.Find("ButtonColor").gameObject.GetComponent<InteractiveButton>();
        ButtonColor.OnClick = ButtonColorOnClick;

        ButtonEdit = Menu.transform.Find("ButtonEdit").gameObject.GetComponent<InteractiveButton>();
        ButtonEdit.OnClick = ButtonEditOnClick;

        ButtonSave = Menu.transform.Find("ButtonSave").gameObject.GetComponent<InteractiveButton>();
        ButtonSave.OnClick = ButtonSaveOnClick;

        ButtonLoad = Menu.transform.Find("ButtonLoad").gameObject.GetComponent<InteractiveButton>();
        ButtonLoad.OnClick = ButtonLoadOnClick;


        Textfield = transform.Find("TextfieldInput").gameObject.GetComponent<KeyboardEditText>();
    }

    private void ButtonHideOnClick()
    {
        ButtonHide.ButtonOn = !ButtonHide.ButtonOn;
        if (Interface.BuildingSelected) Interface.SelectedBuilding.MakeTransparent(!ButtonHide.ButtonOn);
    }

    private void ButtonColorOnClick()
    {
        ButtonColor.ButtonOn = !ButtonColor.ButtonOn;
        if (Interface.BuildingSelected) Interface.SelectedBuilding.ShowColor(ButtonColor.ButtonOn);
    }
    private void ButtonEditOnClick()
    {
        ButtonEdit.ButtonOn = !ButtonEdit.ButtonOn;
        Interface.MainMenu.EditMenu.Show(ButtonEdit.ButtonOn);
    }
    private void ButtonSaveOnClick()
    {
        string name = Textfield.Text;
        if (Interface.BuildingSelected)
        {
            Interface.SelectedBuilding.Save(name);
        }

        //StartCoroutine(Upload());
    }
    private void ButtonLoadOnClick()
    {


        //if (Interface.BuildingSelected)
        //{
        //    Interface.SelectedBuilding.LoadFromTestFile();
        //}

        GameObject go;

        if (LoadBrowser == null)
        {
            go = Instantiate(Resources.Load<GameObject>("Prefabs/Browser"));
            LoadBrowser = go.AddComponent<LoadBrowser>();
            LoadBrowser.Init(Interface);
        }
        else go = LoadBrowser.gameObject;

        go.transform.position = StolperwegeHelper.CenterEyeAnchor.transform.position + StolperwegeHelper.CenterEyeAnchor.transform.forward * 0.3f + Vector3.down * 0.2f;
        go.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
    }

    IEnumerator Upload()
    {
        WWWForm form = new WWWForm();
        //form.AddField("building", "thisBuilding");
        //form.AddField("position", "thisBuilding");
        //form.AddField("description", "thisBuilding");
        //form.AddField("orientation", "thisBuilding");
        //form.AddField("scale", "thisBuilding");
        //form.AddField("ratingType", "thisBuilding");
        //form.AddField("rating", "thisBuilding");

        form.AddField("position", "thisBuilding");
        form.AddField("description", "thisBuilding");
        form.AddField("title", "thisTitle");

        using (UnityWebRequest www = UnityWebRequest.Post("https://localhost:4567/building", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}
