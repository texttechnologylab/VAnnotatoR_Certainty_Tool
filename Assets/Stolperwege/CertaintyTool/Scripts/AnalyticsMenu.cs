using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnalyticsMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public TextMeshPro UnratedObjNumDisplay { get; private set; }
    public TextMeshPro DummyObjNumDisplay { get; private set; }
    public TextMeshPro UneditableObjNumDisplay { get; private set; }
    public InteractiveButton ButtonSelectUnrated { get; private set; }
    private string UnInitiatedNumText = "-";

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        UnratedObjNumDisplay = transform.Find("TextfieldObjNum/Tag").gameObject.GetComponent<TextMeshPro>();
        DummyObjNumDisplay = transform.Find("TextfieldDumNum/Tag").gameObject.GetComponent<TextMeshPro>();
        UneditableObjNumDisplay = transform.Find("TextfieldUneditNum/Tag").gameObject.GetComponent<TextMeshPro>();

        InitButtons();
        Show(false);
    }
    private void InitButtons()
    {
        ButtonSelectUnrated = transform.Find("ButtonSelectUnrated").gameObject.GetComponent<InteractiveButton>();
        ButtonSelectUnrated.OnClick = OnClickButtonSelectUnrated;
    }
    public void Show(bool b)
    {
        Menu.SetActive(b);
    }
    public void SetUnratedObjDisplay(int number)
    {
        string text = number.ToString();
        if (number < 0)
        {
            Debug.LogError("Unrated Object Number supplied was below 0");
            text = "E";
        }
        else if (number > 9999)
        {
            text = ">9999";
        }

        UnratedObjNumDisplay.SetText(text);
    }
    public void ResetTextfieldObjNum()
    {
        UnratedObjNumDisplay.SetText(UnInitiatedNumText);
    }
    public void SetDummyObjNumDisplay(int number)
    {
        string text = number.ToString();
        if (number < 0)
        {
            Debug.LogError("Dummy Object Number supplied was below 0");
            text = "E";
        }
        else if (number > 9999)
        {
            text = ">9999";
        }

        DummyObjNumDisplay.SetText(text);
    }
    public void ResetDummyObjNumDisplay()
    {
        DummyObjNumDisplay.SetText(UnInitiatedNumText);
    }
    public void SetUneditableObjNumDisplay(int number)
    {
        string text = number.ToString();
        if (number < 0)
        {
            Debug.LogError("Uneditable Object Number supplied was below 0");
            text = "E";
        }
        else if (number > 9999)
        {
            text = ">9999";
        }

        UneditableObjNumDisplay.SetText(text);
    }
    public void ResetUneditableObjNumDisplay()
    {
        UneditableObjNumDisplay.SetText(UnInitiatedNumText);
    }
    private void OnClickButtonSelectUnrated()
    {

    }
}
