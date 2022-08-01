using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DataSearchPanel : MonoBehaviour
{

    public string SearchPattern { get { return Inputfield.Text; } }

    public KeyboardEditText Inputfield { get; private set; }
    private InteractiveButton DeleteButton;
    private DataBrowser Browser;

    private Transform _parent;
    private Vector3 _visiblePos;
    private Quaternion _visibleRot;
    private Quaternion _hiddenRot;
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Vector3 _targetScale;

    public void Init()
    {
        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();
        Inputfield = transform.Find("Inputfield").GetComponent<KeyboardEditText>();
        Inputfield.OnCommit = (res, go) => 
        {
            DeleteButton.Active = (res != "");
            Browser.BrowserUpdater?.Invoke();
            //Browser.ActualizeData();
        };

        DeleteButton = transform.Find("DeleteButton").GetComponent<InteractiveButton>();
        DeleteButton.OnClick = () => 
        { 
            Inputfield.Text = "";
            DeleteButton.Active = false;
            Browser.BrowserUpdater?.Invoke();
            //Browser.ActualizeData();
        };
        DeleteButton.Active = false;

        _visiblePos = transform.localPosition;
        _visibleRot = transform.localRotation;
        _hiddenRot = Quaternion.AngleAxis(-180, Vector3.right);

        transform.localPosition = Vector3.forward * -0.01f;
        transform.localRotation = _hiddenRot;
        transform.localScale = Vector3.one * 0.01f;
    }

    public void Reset()
    {
        SetColliderStatus(true);
        transform.parent = _parent;
        transform.localPosition = _visiblePos;
        transform.localRotation = _visibleRot;
    }

    //public override bool onDrop()
    //{
    //    bool res = base.onDrop();
    //    transform.parent = _parent;
    //    return res;
    //}

    public void SetColliderStatus(bool status)
    {
        for (int i = 0; i < GetComponentsInChildren<BoxCollider>().Length; i++)
            GetComponentsInChildren<BoxCollider>()[i].enabled = status;
    }

    private float _lerp; public bool IsActive { get; private set; }
    public IEnumerator Activate(bool status)
    {
        _targetPos = (status) ? _visiblePos : Vector3.forward * -0.01f;
        _targetScale = (status) ? Vector3.one : Vector3.one * 0.01f;
        _targetRot = (status) ? _visibleRot : _hiddenRot;
        _lerp = 0;
        while (transform.localPosition != _targetPos)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPos, _lerp);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRot, _lerp);
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, _lerp);
            _lerp += Time.deltaTime / 10;
            yield return null;
        }
        IsActive = status;
    }

    // TODO
    //public bool MatchesPattern(DiscourseReferent element)
    //{
    //    if (SearchPattern == "") return true;
    //    return element.value.ToLower().Contains(SearchPattern.ToLower());
    //}

    //public bool MatchesPattern(ShapeNetModel snM)
    //{
    //    if (SearchPattern == "") return true;
    //    if (snM.Name.ToLower().Contains(SearchPattern)) return true;
    //    if (snM.Lemmas.Contains(SearchPattern)) return true;
    //    return false;
    //}

    //public bool MatchesPattern(ShapeNetTexture snT)
    //{
    //    if (SearchPattern == "") return true;
    //    if (snT.Name.ToLower().Contains(SearchPattern)) return true;
    //    return false;
    //}
}
