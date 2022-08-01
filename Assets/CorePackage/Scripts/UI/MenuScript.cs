using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public abstract class MenuScript : InteractiveObject 
{
    protected const string PrefabPath = "Prefabs/UI/Menus/";

    private GameObject _label;
    protected SubmenuScrollbox _subMenuControl;
    private MenuScript _parentMenuScript;
    private MenuScript _activeSubMenu;    
    private float _animLerp;
    private TextMeshPro _icon;

    public Vector3 MenuTargetPosition;
    public Vector3 MenuStartPosition;
    public Vector3 MenuOriginalScale;
    public Vector3 MenuStartScale;
    public Vector3 MenuTargetScale;
    public Collider IconCollider;
    public Renderer[] RenderedComponents;

    public delegate void IsActive();
    protected IsActive _proofStatus;

    public delegate void PostActivationEvent();
    public PostActivationEvent OnActivated;

    public bool OnTheMenu;
    public bool IsSubMenuActive;
    private bool _componentRendererActivated = false;
    private bool _visible;
    private bool _animOn = false;
    public bool MainMenuComponent { get; private set; }
    public bool Visible
    {
        get
        {
            return _visible;
        }
        set
        {
            _visible = value;
            _animOn = true;
            if (IconCollider != null)
            {
                IconCollider.enabled = false;
                OnPointerExit();
            }
            if (_visible) _proofStatus();
        }
    }

    private bool _active = true;
    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
            IconCollider.enabled = _active;
            _icon.color = _active ? Color.white : Color.gray;
        }
    }




    // Use this for initialization
    public override void Start () {
        SearchForParts = false;       
        if (transform.Find("Outliner") != null)
            PartsToHighlight.Add(transform.Find("Outliner").GetComponent<MeshRenderer>());
        if (transform.Find("Icon") != null)
            _icon = transform.Find("Icon").GetComponent<TextMeshPro>();
        base.Start();
        RenderedComponents = GetComponentsInChildren<Renderer>(true);
        IconCollider = GetComponent<BoxCollider>();
        if (IconCollider != null) IconCollider.enabled = false;
        Visible = false;
        MenuOriginalScale = transform.localScale;
        _proofStatus = () => { Active = true; };
        _parentMenuScript = transform.parent.GetComponentInParent<MenuScript>();
        _subMenuControl = transform.GetComponentInChildren<SubmenuScrollbox>();
        if (_subMenuControl != null) _subMenuControl.CalculateSubmenuLayout();
        
    }
	
	// Update is called once per frame
	public void Update () {
        if (_animOn)
        {
            if (Visible) ShowItem();
            else HideItem();
        }
	}

    private void HideItem()
    {
        _animLerp += Time.deltaTime;
        transform.localPosition = Vector3.Lerp(transform.localPosition, MenuStartPosition, _animLerp);
        transform.localScale = Vector3.Lerp(transform.localScale, MenuStartScale, _animLerp);
        if (transform.localPosition == MenuStartPosition)
        {
            _animLerp = 0;
            _animOn = false;
            SetComponentRendererStatus(false);
        }
    }

    private void ShowItem()
    {
        _animLerp += Time.deltaTime;
        if (!_componentRendererActivated) SetComponentRendererStatus(true);
        transform.localPosition = Vector3.Lerp(transform.localPosition, MenuTargetPosition, _animLerp);
        transform.localScale = Vector3.Lerp(transform.localScale, MenuTargetScale, _animLerp);
        if (transform.localPosition == MenuTargetPosition)
        {
            _animLerp = 0;
            _animOn = false;

            if (IconCollider != null && Active) IconCollider.enabled = true;

            OnActivated?.Invoke();
        }
    }

    public virtual IEnumerator ActivateSubMenu()
    {

        if (_subMenuControl == null) yield break;
        if (_parentMenuScript != null && _parentMenuScript._activeSubMenu != null &&
            _parentMenuScript._activeSubMenu != this)
            yield return StartCoroutine(_parentMenuScript._activeSubMenu.HideSubMenu());
        _subMenuControl.CalculateSubmenuLayout();
        _subMenuControl.ShowItems();
        if (_parentMenuScript != null)
            _parentMenuScript._activeSubMenu = this;
        IsSubMenuActive = true;
    }

    public virtual IEnumerator HideSubMenu()
    {
        if (_activeSubMenu != null)
        {
            Debug.Log(_activeSubMenu.GetType());
            yield return StartCoroutine(_activeSubMenu.HideSubMenu());
        }
            
        _subMenuControl.CalculateSubmenuLayout();
        _subMenuControl.HideItems();
        IsSubMenuActive = false;
    }

    public void SetComponentRendererStatus(bool value)
    {
        foreach (Renderer component in RenderedComponents)
            component.GetComponent<Renderer>().enabled = value;
        if (_label != null) _label.GetComponent<Renderer>().enabled = value;
        _componentRendererActivated = value;
    }

    public void SubmenuController() { StartCoroutine(ActivateSubMenu()); }

    public void MainMenuCallback() { StartCoroutine(_parentMenuScript.HideSubMenu()); }
    
    /// <summary>
    /// Sets the menu label and icon. Use this only for the standard menu item prefab.
    /// </summary>
    protected void SetNameAndIcon(string name, string icon)
    {
        TextMeshPro textComponent = transform.Find("Label").GetComponent<TextMeshPro>();
        if (textComponent != null) textComponent.text = name;
        if (_icon != null) _icon.text = icon;
    }

}
