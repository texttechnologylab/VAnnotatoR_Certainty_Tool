using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDMenuStatusBox : InteractiveObject {
    

    public TextMeshPro Focus { get; private set; }
    public TextMeshPro Status1 { get; private set; }
    public TextMeshPro Status2 { get; private set; }

    public void Init()
    {
        Focus = transform.Find("Focus").GetComponent<TextMeshPro>();
        Status1 = transform.Find("Status1").GetComponent<TextMeshPro>();
        Status2 = transform.Find("Status2").GetComponent<TextMeshPro>();
    }

    public void SetFocus(string text)
    {
        Focus.text = text;
    }

    public void SetStatus1(string text)
    {
        Status1.gameObject.SetActive(true);
        Status1.text = text;
    }

    public void SetStatus2(string text)
    {
        Status2.gameObject.SetActive(true);
        Status2.text = text;
    }

    public void TurnOffStatus()
    {
        Status1.gameObject.SetActive(false);
        Status2.gameObject.SetActive(false);
    }
}
