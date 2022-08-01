using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmenuScrollbox : InteractiveObject {

    private int StartIndex = 0;
    private bool colliderLocked = false;
    private Vector3 StartScrollPosition;
    private bool scrolling = false;

    protected const float X_PADDING = 0.15f;
    protected const float Y_POS = 0.15f;
    protected const float Z_POS = 0f;
    public int MaxVisibleItems = 8;

    private bool _scrollable;
    public bool Scrollable
    {
        get
        {
            return transform.childCount > 10;
        }
    }

    public bool ActivateScroll(int direction)
    {
        if (StartIndex + direction < 0 || StartIndex + MaxVisibleItems + direction > transform.childCount) return false;
        StartIndex += direction;
        CalculateSubmenuLayout();
        ShowItems();
        return true;
    }

    public void CalculateSubmenuLayout()
    {
        int visibleIcons = Mathf.Min(transform.childCount, MaxVisibleItems);
        float xPos = (visibleIcons / 2) * X_PADDING;
        if (visibleIcons % 2 == 0) xPos -= X_PADDING / 2;
        xPos += StartIndex * X_PADDING;
        GameObject icon;
        for (int i = 0; i < transform.childCount; i++)
        {
            icon = transform.GetChild(i).gameObject;
            icon.GetComponent<MenuScript>().MenuTargetPosition = new Vector3(xPos, Y_POS, Z_POS);
            xPos -= X_PADDING;
        }
    }
    private void Update()
    {
        transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0f, 0f);
    }

    public void ShowItems()
    {
        MenuScript actualScript;
        for (int i = 0; i < transform.childCount; i++)
        {
            actualScript = transform.GetChild(i).GetComponent<MenuScript>();
            if (i >= StartIndex && i < StartIndex + MaxVisibleItems)
            {
                actualScript.MenuTargetScale = actualScript.MenuOriginalScale;
                actualScript.Visible = true;
            } else
            {
                actualScript.MenuStartPosition = actualScript.MenuTargetPosition;
                if (actualScript.Visible)
                {
                    actualScript.Visible = false;
                }
                else actualScript.gameObject.transform.localPosition = actualScript.MenuTargetPosition;
            }            
        }
        scrolling = false;
    }

    public void HideItems()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            MenuScript actualScript = transform.GetChild(i).GetComponent<MenuScript>();
            actualScript.MenuStartPosition = Vector3.zero;
            actualScript.MenuStartPosition = Vector3.zero;
            if (actualScript.Visible)
            {
                actualScript.Visible = false;
            } else
            {
                actualScript.gameObject.transform.localPosition = actualScript.MenuStartPosition;
                actualScript.gameObject.transform.localScale = actualScript.MenuStartScale;
            }
        }
    }
}
