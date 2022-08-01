using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

/// <summary>
/// https://youtu.be/CPyaWkjo6Ss
/// https://github.com/C-Through/VR-RadialMenu
/// </summary>
public class RadialMenuController : MonoBehaviour
{

    private const int SEGMENTS = 8;

    [Header("Scene")]
    public Transform SelectionTransform = null;
    public Transform CurserTransform = null;

    private Vector2 _touchPosition = Vector2.zero;

    private List<RadialSection> _radialSections = null;
    private int _sectionPointer;
    private Boolean returnisclose = false;

    private List<bool> _activeSections = null;

    private int SectionPointer
    {
        get { return _sectionPointer; }
        set
        {
            _sectionPointer = (value >= _radialSections.Count) ? 0 : value;
            UpdateSectionLabels();
            _highlightedSection = null;
            SelectionTransform.gameObject.SetActive(false);
            UpdateCenter();            
        }
    }

    private List<TextMeshPro> _sectionLabels = null;

    private RadialSection _highlightedSection = null;
    private int _highlightedSectionIndex = -1;

    private readonly float _zeroPosition = 0.1f;
    private readonly float _deadZone = 0.4f;
    private readonly float _degreeIncrement = 45.0f;

    private TextMeshPro _centerTitleLabel = null;
    private TextMeshPro _centerDescriptionLabel = null;

    public bool Visible { get; private set; }
    public delegate void InterruptionHandler();
    public InterruptionHandler OnInterrupt;

    /// <summary>
    /// </summary>
    /// <param name="sections"></param>
    public void UpdateSection(List<RadialSection> sections, int beginHighlight = -1, Boolean _returnisclose=false)
    {
        _radialSections = new List<RadialSection>(sections);
        returnisclose = _returnisclose;
        if (_activeSections == null) _activeSections = new List<bool>();
        else _activeSections.Clear();
        CheckSectionCount();
        SectionPointer = 0;
        
        //UpdateSectionLabels();
        _highlightedSectionIndex = beginHighlight;
        if (beginHighlight < 0)
        {
            _highlightedSection = null;
            SelectionTransform.gameObject.SetActive(false);
        }
        else
        {
            _highlightedSection = sections[beginHighlight];
            SelectionTransform.gameObject.SetActive(true);
        }

        UpdateCenter();
    }

    public List<RadialSection> GetCurrentSection()
    {
        return _radialSections;
    }

    /*
     * Important: Does not work with pages!!!!! Init is okay, but no later updates!!!!!
    **/
    public void UpdateSectionLock(List<bool> activeSections)
    {
        _activeSections = activeSections;
        if (_activeSections == null)
        {
            _activeSections = new List<bool>();
            for (int i = 0; i < _radialSections.Count; i++)
            {
                _activeSections.Add(_radialSections[i] != null);
            }
        }
        
        for (int i = SectionPointer; i < _activeSections.Count; i++)
        {
            if (i - SectionPointer == _activeSections.Count) break;
            if (!_activeSections[i] || _radialSections[i] == null)
            {
                _sectionLabels[i - SectionPointer].color = Color.grey;
            }
            else
            {
                if (_radialSections[i].ChildSections != null)
                {
                    _sectionLabels[i - SectionPointer].color = StolperwegeHelper.GUCOLOR.LICHTBLAU;
                }
                else
                {
                    _sectionLabels[i - SectionPointer].color = Color.white;
                }
            }
        }

        //Cleanup, if current selected Section gets deactivated.
        if (_highlightedSectionIndex > -1 && !_activeSections[_highlightedSectionIndex])
        {
            _highlightedSectionIndex = -1;
            _highlightedSection = null;
            SelectionTransform.gameObject.SetActive(false);
            UpdateCenter();
        }
    }


    public RadialSection GetSelectedSection()
    {
        if (!Visible || !InDeadZone())
            return null;

        return _highlightedSection;
    }

    public void Show(bool value)
    {
        if (Visible && value)
        {
            OnInterrupt?.Invoke();
            OnInterrupt = null;
        }
        Visible = value;
        gameObject.SetActive(value);
    }


    private void Awake()
    {
        CreateAndSetupSections();        
    }

    private void Start()
    {
        StolperwegeHelper.RadialMenu = this;
        Show(false);
    }

    private void Update()
    {
        LookAtUser();
        Vector2 direction = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand);
        direction = new Vector2(-direction.x, direction.y);
        SetTouchPosition(direction);
        float rotation = GetDegree(direction);

        if (!InDeadZone())
        {
            SetSelectedEvent(rotation);
            //SetSelectionRotation(rotation);
            UpdateCenter();
        }

        if (InZeroZone())
        {
            if (_highlightedSection != null)
            {
                if (_highlightedSection.Title.Equals("..."))
                    SectionPointer += SEGMENTS;
                else if (_highlightedSection.ChildSections != null)
                    if (returnisclose)
                    {
                        Show(false);
                    }
                    else
                    {
                        UpdateSection(_highlightedSection.ChildSections);
                    }
                    
            }
        }
    }

    private void LookAtUser()
    {
        Vector3 lookTarget = StolperwegeHelper.CenterEyeAnchor.transform.position;
        transform.LookAt(lookTarget);
    }


    private void CreateAndSetupSections() 
    {
        _centerTitleLabel = gameObject.transform.Find("InnerCircle/TopTag").GetComponent<TextMeshPro>();
        _centerDescriptionLabel = gameObject.transform.Find("InnerCircle/BottomTag").GetComponent<TextMeshPro>();

        GameObject sections = gameObject.transform.Find("Sections").gameObject;
        TextMeshPro[] sectionMeshs = sections.transform.GetComponentsInChildren<TextMeshPro>();
        _sectionLabels = new List<TextMeshPro>(sectionMeshs);


        //UpdateSection(RadialLinkMenuData.GetMainSection());

        UpdateSection(new List<RadialSection>());
    }

    
    private void UpdateSectionLabels()
    {
        for (int i=SectionPointer; i<_radialSections.Count; i++)
        {
            if (i - SectionPointer == _sectionLabels.Count) return;
            if (_radialSections[i] != null)
            {
                _sectionLabels[i - SectionPointer].text = _radialSections[i].Title;
                if (!_activeSections[i])
                {
                    _sectionLabels[i - SectionPointer].color = Color.grey;
                }
                else if (_radialSections[i].ChildSections != null)
                {
                    _sectionLabels[i - SectionPointer].color = StolperwegeHelper.GUCOLOR.LICHTBLAU;
                }
                else
                {
                    _sectionLabels[i - SectionPointer].color = Color.white;
                }
            }
            else
                _sectionLabels[i - SectionPointer].text = "";
        }

    }

    private void CheckSectionCount()
    {
        while(_radialSections.Count < SEGMENTS)
            _radialSections.Add(null);

        for (int i=0; i<_radialSections.Count; i++)
        {
            _activeSections.Add(_radialSections[i] != null);
        }
        //while (_activeSections.Count < _radialSections.Count)
        //    _activeSections.Add(true);

        if (_radialSections.Count == SEGMENTS) return;
        int pages = Mathf.CeilToInt(_radialSections.Count / (float)(SEGMENTS - 1));

        for (int i=0; i<pages; i++)
        {
            RadialSection extraSection = new RadialSection("...", ((i + 1) == pages) ? "to page 1" : "to page " + (i + 2), null);
            if ((i + 1) < pages) {
                _radialSections.Insert(i * SEGMENTS + (SEGMENTS - 1), extraSection);
                _activeSections.Insert(i * SEGMENTS + (SEGMENTS - 1), true);
            }
            else
            {
                _radialSections.Add(extraSection);
                _activeSections.Add(true);
                while (_radialSections.Count % SEGMENTS != 0)
                {
                    _radialSections.Add(null);
                    _activeSections.Add(false);
                }
            }
        }
    }



    private void UpdateCenter()
    {
        if(_highlightedSection != null)
        {
            _centerTitleLabel.text = _highlightedSection.Title;
            _centerDescriptionLabel.text = _highlightedSection.Description;
        }
        else
        {
            _centerTitleLabel.text = "";
            _centerDescriptionLabel.text = "";
        }
    }

    private float GetDegree(Vector2 direction)
    {
        float value = Mathf.Atan2(direction.x, direction.y);
        value *= Mathf.Rad2Deg;
        if(value < 0)
        {
            value += 360.0f;
        }
        return 360 - value;
    }

    private int GetNearestIncrement(float rotation)
    {
        return Mathf.RoundToInt(rotation / _degreeIncrement);
    }

    private void SetSelectedEvent(float currentRotation)
    {
        int index = GetNearestIncrement(currentRotation) % SEGMENTS;
        SetSelectedEvent(index);
    }

    private void SetSelectedEvent(int currentindex)
    {
        if (!InDeadZone())
        {
            if (_radialSections[SectionPointer + currentindex] != null && _activeSections[SectionPointer + currentindex])
            {
                _highlightedSection = _radialSections[SectionPointer + currentindex];
                _highlightedSectionIndex = SectionPointer + currentindex;
                SelectionTransform.localEulerAngles = new Vector3(0, 0, currentindex * _degreeIncrement);
                SelectionTransform.gameObject.SetActive(true);
            }
        }
    }

    private Boolean InDeadZone()
    {
        if (Vector2.Distance(_touchPosition, Vector2.zero) > _deadZone)
            return false;
        else
            return true;
    }

    public Boolean InZeroZone()
    {
        if (Vector2.Distance(_touchPosition, Vector2.zero) > _zeroPosition)
            return false;
        else
            return true;
    }

    private void SetTouchPosition(Vector2 newValue)
    {
        _touchPosition = newValue;
    }

}
