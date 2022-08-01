using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractiveSlider : MonoBehaviour
{
    private TextMeshPro Display;
    private KeyboardEditText DisplayValue;
    private InteractiveButton NextButton;
    private InteractiveButton PreviousButton;
    public float Precision { get; private set; }
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }
    private float _selectedValue;
    public float SelectedValue
    {
        get { return _selectedValue; }
        set
        {
            float param = value < MaxValue ? (value > MinValue ? value : MinValue) : MaxValue;
            OnSelectionChange.Invoke(_selectedValue, param);
            _selectedValue = param;
            Display.text = _selectedValue.ToString();
        }
    }

    public delegate void OnSelectionChangeEvent(float previous, float next);
    public OnSelectionChangeEvent OnSelectionChange;

    public void Initialize(float initialValue = 1.0f, float minValue = 0f, float maxValue = 1.0f, float precision = 0.1f)
    {
        _selectedValue = initialValue;
        Precision = precision;
        MinValue = minValue;
        MaxValue = maxValue;
        NextButton = transform.Find("NextButton").GetComponent<InteractiveButton>();
        PreviousButton = transform.Find("PreviousButton").GetComponent<InteractiveButton>();
        Display = transform.Find("Text").GetComponentInChildren<TextMeshPro>();
        DisplayValue = transform.Find("Text").GetComponent<KeyboardEditText>();

        Display.text = _selectedValue.ToString();
        DisplayValue.OnCommit = (text, go) => { SelectedValue = float.Parse(text); };
        NextButton.OnClick = () => { SelectedValue = (float) System.Math.Round(_selectedValue + Precision, 5) ; };
        PreviousButton.OnClick = () => { SelectedValue = (float) System.Math.Round(_selectedValue - Precision, 5) ; };
    }
}
