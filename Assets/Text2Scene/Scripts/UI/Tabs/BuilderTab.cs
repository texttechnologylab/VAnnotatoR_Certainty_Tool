using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuilderTab : MonoBehaviour
{
    public bool UsableWithoutDocument { get; protected set; }
    protected const string PrefabPath = "Prefabs/Tabs/";
    public string Name { get; protected set; }
    public bool ShowOnToolbar { get; protected set; }    
    public SceneBuilder Builder { get; private set; }

    public InteractiveButton ControlButton;

    private bool _active;
    public bool Active
    {
        get { return _active; }
        set
        {
            if (value == _active) return;
            _active = value;
            if (_active) UpdateTab();
            ActualizeControlButtonStatus();            
        }
    }

    protected DragFinger LeftHand, RightHand;
    protected DataBrowserResource ObjectInLeftHand, ObjectInRightHand;

    public virtual void Initialize(SceneBuilder builder)
    {
        Builder = builder;
    }

    protected virtual void UpdateTab()
    {
        foreach (Type type in Builder.BuilderTabMap.Keys)
            Builder.BuilderTabMap[type].gameObject.SetActive(type.Equals(GetType()));
    }

    protected Vector3 localPos;
    protected virtual void CheckHands()
    {
        if (StolperwegeHelper.LeftHand == null || StolperwegeHelper.RightHand == null) return;
        if (LeftHand == null)
            LeftHand = StolperwegeHelper.LeftHand.GetComponent<DragFinger>();
        if (RightHand == null)
            RightHand = StolperwegeHelper.RightHand.GetComponent<DragFinger>();

        if (LeftHand.GrabedObject == null || !(LeftHand.GrabedObject is DataBrowserResource))
            ObjectInLeftHand = null;
        else ObjectInLeftHand = (DataBrowserResource)LeftHand.GrabedObject;

        if (RightHand.GrabedObject == null || !(RightHand.GrabedObject is DataBrowserResource))
            ObjectInRightHand = null;
        else ObjectInRightHand = (DataBrowserResource)RightHand.GrabedObject;
    }

    public abstract void ResetTab();

    public void ActualizeControlButtonStatus()
    {
        if (ControlButton != null && ControlButton.gameObject.activeInHierarchy)
        {
            ControlButton.Active = (Builder.GetTab<DocumentTab>().Document != null || UsableWithoutDocument) && !Active;
            ControlButton.ButtonOn = Active;
        }
    }
}
