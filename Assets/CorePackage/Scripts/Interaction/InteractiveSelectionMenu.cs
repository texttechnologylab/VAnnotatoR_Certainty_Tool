using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractiveSelectionMenu : MonoBehaviour
{
    private TextMeshPro Display;
    private InteractiveButton NextButton;
    private InteractiveButton PreviousButton;
    private List<string> SelectionItems;
    private int _selectedItemIndex;
    public string SelectedItem
    {
        get { return SelectionItems[_selectedItemIndex]; }
        set
        {
            if(OnSelectionChange != null) OnSelectionChange.Invoke(SelectionItems[_selectedItemIndex], value);
            _selectedItemIndex = SelectionItems.IndexOf(value);
            Display.text = SelectionItems[_selectedItemIndex];
            PreviousButton.Active = _selectedItemIndex > 0;
            NextButton.Active = _selectedItemIndex < SelectionItems.Count - 1;
        }
    }

    public delegate void OnSelectionChangeEvent(string previous, string next);
    public OnSelectionChangeEvent OnSelectionChange;

    public void Initialize(List<string> selectionItems)
    {
        Initialize(selectionItems, selectionItems[0]);
    }

    public void Initialize(List<string> selectionItems, string selectedItem)
    {
        if (selectionItems == null || selectionItems.Count == 0)
            return;

        NextButton = transform.Find("NextButton").GetComponent<InteractiveButton>();
        PreviousButton = transform.Find("PreviousButton").GetComponent<InteractiveButton>();
        Display = transform.Find("Text/Tag").GetComponent<TextMeshPro>();

        SelectionItems = selectionItems;
        SelectedItem = selectedItem;

        Display.text = SelectedItem;
        NextButton.OnClick = () => { SelectedItem = SelectionItems[_selectedItemIndex < SelectionItems.Count - 1 ? _selectedItemIndex + 1 : 0]; };
        PreviousButton.OnClick = () => { SelectedItem = SelectionItems[_selectedItemIndex > 0 ? _selectedItemIndex - 1 : 0]; };
    }
}
