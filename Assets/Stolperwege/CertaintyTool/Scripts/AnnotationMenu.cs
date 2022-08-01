using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnnotationMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public TextMeshPro ObjectDisplay { get; private set; }
    public InteractiveButton ButtonConnect { get; private set; }
    public InteractiveButton ButtonDisconnect { get; private set; }
    public InteractiveButton ButtonAutoConnect { get; private set; }
    private string OriginalObjectDisplayText = "No Annotation Object selected";

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        ObjectDisplay = transform.Find("TextfieldDisplay/Tag").gameObject.GetComponent<TextMeshPro>();

        InitButtons();
        Show(false);
    }
    private void InitButtons()
    {
        ButtonConnect = transform.Find("ButtonConnect").gameObject.GetComponent<InteractiveButton>();
        ButtonConnect.OnClick = OnClickButtonConnect;
        ButtonDisconnect = transform.Find("ButtonDisconnect").gameObject.GetComponent<InteractiveButton>();
        ButtonDisconnect.OnClick = OnClickButtonDisconnect;
        ButtonAutoConnect = transform.Find("CheckBoxAutoConnect/ButtonCheck").gameObject.GetComponent<InteractiveButton>();
        ButtonAutoConnect.OnClick = OnClickButtonAutoConnect;
    }
    public void Show(bool b)
    {
        Menu.SetActive(b);
    }
    public void SetObjectDisplayText(string text)
    {
        ObjectDisplay.SetText(text);
    }

    public void ResetObjectDisplayText()
    {
        ObjectDisplay.SetText(OriginalObjectDisplayText);
    }

    private void OnClickButtonConnect()
    {
        Interface.AddAnnotationObject();
    }
    private void OnClickButtonDisconnect()
    {
        Interface.RemoveAnnotationObject();
    }
    private void OnClickButtonAutoConnect()
    {
        Interface.AutoConnectAnnos = !Interface.AutoConnectAnnos;
        ButtonAutoConnect.ButtonOn = !Interface.AutoConnectAnnos;
    }
}
