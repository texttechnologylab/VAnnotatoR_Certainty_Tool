using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class BrowserMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshPro Header { get; private set; }
    public InteractiveButton ButtonClose { get; private set; }
    public InteractiveButton ButtonAction { get; private set; }
    private InteractiveButton ButtonScrollUp;
    private InteractiveButton ButtonScrollDown;
    public int Page { get; private set; }
    public int MaxPage => (int)Math.Ceiling((NumItems / 5d)) - 1;

    private List<string> _ItemLabels = new List<string>();
    public List<string> ItemLabels
    {
        get
        {
            return _ItemLabels;
        }

        set
        {
            _ItemLabels = value;
            PageCorrection();
        }
    }
    public int NumItems => ItemLabels.Count;
    public InteractiveButton[] Entries { get; private set; }
    public delegate void EntryAction(int? i);
    public EntryAction _EntryAction;

    public delegate void ActionButtonAction(int? selectedItem);
    public ActionButtonAction _ActionButtonAction;

    public bool CloseOnAction = false;
    public bool CloseOnEntry = false;
    public int? SelectedItem { get; private set; }

    public string HeaderText
    {
        get
        {
            return Header?.text;
        }
        set
        {
            Header?.SetText(value);
        }
    }

    public string ButtonActionText
    {
        get
        {
            return ButtonAction?.ButtonText?.text;
        }
        set
        {
            ButtonAction?.ButtonText?.SetText(value);
        }
    }

    private void Awake()
    {
        Page = 0;

        Header = transform.Find("Textfield/Tag").gameObject.GetComponent<TextMeshPro>();

        ButtonClose = transform.Find("ButtonClose").gameObject.GetComponent<InteractiveButton>();
        ButtonAction = transform.Find("ButtonAction").gameObject.GetComponent<InteractiveButton>();

        ButtonScrollUp = transform.Find("ButtonScrollUp").gameObject.GetComponent<InteractiveButton>();
        ButtonScrollDown = transform.Find("ButtonScrollDown").gameObject.GetComponent<InteractiveButton>();

        Entries = new InteractiveButton[5];

        //for (int i = 0; i >= 5; i++)
        //{
        //    Entries[i] = transform.Find("ButtonEntry" + i.ToString()).gameObject.GetComponent<InteractiveButton>();
        //    // Entries[i].gameObject.SetActive(false);
        //}

        int i = 0;
        foreach (Transform child in transform.Find("EntryButtons"))
        {
            Entries[i] = child.GetComponent<InteractiveButton>();
            i++;
        }

        ButtonAction.Active = false;

        BindButtons();
        PageChangeUpdate();
    }

    public void BindButtons()
    {
        ButtonClose.OnClick = Close;
        ButtonAction.OnClick = Action;

        ButtonScrollUp.OnClick = ScrollUp;
        ButtonScrollDown.OnClick = ScrollDown;

        for (int i = 0; i < Entries.Length; i++)
        {
            BindHelp(i);
        }


    }

    private void BindHelp(int i)
    {
        Entries[i].ButtonValue = i;
        Entries[i].OnClick = () => {
            SelectedItem = (int)Entries[i].ButtonValue + Page * Entries.Length;
            _EntryAction(SelectedItem);
            if (CloseOnEntry) Close();

            ButtonAction.Active = true;
        };
    }

    public void Close()
    {
        gameObject.Destroy();
    }

    public void Action()
    {
        _ActionButtonAction(SelectedItem);
        if (CloseOnAction) Close();
    }

    private void ScrollUp()
    {
        Page += -1;
        PageChangeUpdate();
    }

    private void ScrollDown()
    {
        Page += 1;
        PageChangeUpdate();
    }

    private void PageChangeUpdate()
    {
        Debug.LogWarning("PageChangeUpdate:");
        Debug.LogWarning("Page " + Page.ToString() + " of " + MaxPage.ToString());
        //foreach (string label in ItemLabels)
        //{
        //    Debug.LogWarning(label);
        //}
        ButtonScrollUp.Active = Page > 0;
        ButtonScrollDown.Active = Page < MaxPage;

        for (int i = 0; i < Entries.Length; i++)
        {
            if (i + Page * Entries.Length <= NumItems - 1)
            {
                // Debug.LogWarning(ItemLabels[i]);
                Entries[i].ButtonText.SetText(ItemLabels[i + Page * Entries.Length]);
                Entries[i].gameObject.SetActive(true);
            }
            else Entries[i].gameObject.SetActive(false);
        }
    }

    private void PageCorrection()
    {
        if (Page < 0) ScrollUp();
        else if (Page > MaxPage) ScrollDown();
        else PageChangeUpdate();
    }
}
