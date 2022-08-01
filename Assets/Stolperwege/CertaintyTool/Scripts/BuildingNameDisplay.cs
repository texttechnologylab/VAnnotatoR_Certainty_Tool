using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingNameDisplay : MonoBehaviour

{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public TextMeshPro Tag { get; private set; }

    private InteractiveButton ButtonClear;

    public string Text { get; private set; }
    private readonly string DefaultText = "Nothing Selected";

    public void Init(CertaintyToolInterface CTInterface)
    {
        Interface = CTInterface;
        Menu = transform.gameObject;
        Tag = Menu.transform.Find("Tag").GetComponent<TextMeshPro>();
        InitButtons();
    }

    private void InitButtons()
    {
        ButtonClear = Menu.transform.Find("ButtonClear").GetComponent<InteractiveButton>();
        ButtonClear.OnClick = () => {
            Interface.ClearSelection();
            ClearText();
        };
    }

    public void SetText (string text)
    {
        Text = text;
        Tag.SetText(Text);
    }

    public void ClearText()
    {
        Text = DefaultText;
        Tag.SetText(DefaultText);
    }
}
