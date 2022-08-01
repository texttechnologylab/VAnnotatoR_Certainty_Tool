using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>Class <c>Spinbox</c> Spinbox for VR GUI
/// </summary>
///
public class Spinbox : MonoBehaviour
{
    public InteractiveButton ButtonIncrease { get; private set; }
    public InteractiveButton ButtonDecrease { get; private set; }

    public delegate void ActionOnValueChange();
    /// <summary>Called when Spinbox Value is changed</summary>
    public ActionOnValueChange OnValueChange;

    public delegate void ActionOnBoundaryChange();
    /// <summary>Called when Boundaries are changed</summary>
    public ActionOnBoundaryChange OnBoundaryChange;

    public delegate void ActionOnIncrease();
    /// <summary>Called when Increase Button is pressed</summary>
    public ActionOnIncrease OnIncrease;

    public delegate void ActionOnDecrease();
    /// <summary>Called when Decrease Button is pressed</summary>
    public ActionOnIncrease OnDecrease;


    private GameObject Outliner;
    public Color OriginalOutlinerColor { get; private set; }
    public TextMeshPro Tag { get; private set; }
    private GameObject GameObject;

    public Color InactiveButtonColor = new Color(0.4f, 0.4f, 0.4f);
    public int Value { get; private set; }

    public int UpperBoundary { get; private set; }
    public int LowerBoundary { get; private set; }

    /// <summary>Init Spinbox</summary>
    public void Init(int startAt, int lowerBoundary, int upperBoundary)
    {
        GameObject = transform.gameObject;

        Tag = GameObject.transform.Find("Tag").gameObject.GetComponent<TextMeshPro>();
        Outliner = transform.Find("Outliner").gameObject;
        OriginalOutlinerColor = Outliner.GetComponent<Renderer>().material.GetColor("_Color");
        InitButtons();



        SetBoundaries(lowerBoundary, upperBoundary);
        SetValue(startAt);
    }

    /// <summary>Init Buttons</summary>
    private void InitButtons()
    {
        ButtonIncrease = GameObject.transform.Find("ButtonIncrease").gameObject.GetComponent<InteractiveButton>();
        ButtonIncrease.OnClick = OnClickButtonIncrease;
        ButtonIncrease.InactiveButtonOffColor = InactiveButtonColor;

        ButtonDecrease = GameObject.transform.Find("ButtonDecrease").gameObject.GetComponent<InteractiveButton>();
        ButtonDecrease.OnClick = OnClickButtonDecrease;
        ButtonDecrease.InactiveButtonOffColor = InactiveButtonColor;

    }

    /// <summary>Set Boundaries which cannot be exceeded for Spinbox Value</summary>
    public void SetBoundaries(int lowerBoundary, int upperBoundary, bool triggerOnChange = true)
    {
        if (lowerBoundary > upperBoundary)
        {
            Debug.LogError("Spinbox cannot work properly because lower Boundary is bigger than upper Boundary.");
        }
        LowerBoundary = lowerBoundary;
        UpperBoundary = upperBoundary;

        UpdateButtons();
        if (triggerOnChange) OnBoundaryChange?.Invoke();
    }

    /// <summary>Set Spinbox Value</summary>
    public void SetValue(int value, bool triggerOnchange = true)
    {
        //DebugHelper(value);
        Value = value;
        Tag.text = value.ToString();
        UpdateButtons();
        if (triggerOnchange) OnValueChange?.Invoke();
    }

    /// <summary>Set Outliner Color</summary>
    public void SetOutlinerColor(Color color)
    {
        Outliner.GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    /// <summary>Button Update</summary>
    private void UpdateButtons()
    {
        bool a = (Value < UpperBoundary);
        bool b = (Value > LowerBoundary);
        ButtonIncrease.Active = (Value < UpperBoundary);
        ButtonDecrease.Active = (Value > LowerBoundary);
    }

    private void OnClickButtonIncrease()
    {
        int newValue = Value + 1;
        if (newValue <= UpperBoundary)
        {
            SetValue(newValue);
            OnIncrease?.Invoke();
        }
    }

    private void OnClickButtonDecrease()
    {
        int newValue = Value - 1;
        if (newValue >= LowerBoundary)
        {
            SetValue(newValue);
            OnDecrease?.Invoke();
        }
    }

    private void DebugHelper(int value)
    {
        Debug.LogWarning("Spinbox: " + transform.name + ", Value: " + value);
    }
}
