using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class HierarchieViewer : MonoBehaviour
{
    public CertaintyToolInterface Interface;
    public TextMeshPro ObjectDisplayText { get; private set; }
    public InteractiveButton ButtonReturn { get; private set; }
    public InteractiveButton[] Entries { get; private set; }
    private string OriginalDisplayText = "No Object Selected";

    public delegate void ActionButtonAction();
    public ActionButtonAction _ActionButtonAction;

    private int DisplayStartAtChildNum = 0;
    private SubBuildingController[] CurentObjChildren;
    private IEnumerable<SubBuildingController> CurentObjChildrenSlice;

    private SubBuildingController LastSelectedObject;

    private void Init(CertaintyToolInterface CTInterface)
    {
        Interface = CTInterface;

        ObjectDisplayText = transform.Find("ObjectNameDisplay/Tag").gameObject.GetComponent<TextMeshPro>();
        ButtonReturn = transform.Find("ButtonReturn").gameObject.GetComponent<InteractiveButton>();

        Entries = new InteractiveButton[6];

        for (int i = 0; i >= 6; i++)
        {
            Entries[i] = transform.Find("ButtonOption" + i.ToString()).gameObject.GetComponent<InteractiveButton>();
            Entries[i].gameObject.SetActive(false);
        }

        BindButtons();
    }

    public void BindButtons()
    {
        ButtonReturn.OnClick = Return;

        for (int i = 0; i >= Entries.Length; i++)
        {
            Entries[i].ButtonValue = i;
            Entries[i].OnClick = () => {
                
            };
        }
    }

    public void Close()
    {
        gameObject.Destroy();
    }

    public void Return()
    {
        
    }

    public void DisplayChildCount(int i)
    {
        ObjectDisplayText.SetText("Current Object Child Count: " + i.ToString());
    }

    public void ResetDisplay()
    {
        ObjectDisplayText.SetText(OriginalDisplayText);
    }

    public void UpdateViewer()
    {
        UpdateObjList();
    }

    public void UpdateObjList()
    {
        if (Interface.SelectedSubBuilding != null && Interface.SelectedSubBuilding != LastSelectedObject)
        {
            CurentObjChildren = Interface.SelectedSubBuilding.Children;
            CurentObjChildrenSlice = CurentObjChildren.Take(6);

        }
        else if (Interface.SelectedSubBuilding == null)
        {
            LastSelectedObject = null;
        }
    }
}
