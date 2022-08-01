using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusBoxScript : MonoBehaviour 
{

    private bool _active;
    private bool _animOn;
    private readonly Vector3 Hidden = new Vector3(0, 0.25f, 0.1f);
    private readonly Vector3 Shown = new Vector3(0, -0.05f, 0.35f);
    private float _animLerp;
    public bool Active
    {
        get { return _active; }
        set
        {
            if (value == _active) return;
            _active = value;
            if (_active) Components.SetActive(_active);
            _animOn = true;
            _animLerp = 0;
        }
    }

    public struct Compontents
    {
        public GameObject Outliner;
        public GameObject Background;
        public TextMeshPro Description;
        public LoadingBar LoadingBar;

        public Compontents(GameObject outliner, GameObject background, TextMeshPro description, LoadingBar loadingBar)
        {
            Outliner = outliner;
            Background = background;
            Description = description;
            LoadingBar = loadingBar;
        }

        public void SetActive(bool status)
        {
            Outliner.SetActive(status);
            Background.SetActive(status);
            Description.gameObject.SetActive(status);
            LoadingBar.SetActive(status);
        }

        public void SetInfoText(string text)
        {
            Description.text = text;
        }
    }

    public struct LoadingBar
    {
        public GameObject LeftSide;
        public GameObject RightSide;
        public TextMeshPro StatusBar;
        public bool Active;

        public LoadingBar(GameObject lS, GameObject rS, TextMeshPro sB)
        {
            LeftSide = lS;
            RightSide = rS;
            StatusBar = sB;
            Active = false;
        }

        public void SetActive(bool status)
        {
            Active = status;
            LeftSide.SetActive(status);
            RightSide.SetActive(status);
            StatusBar.gameObject.SetActive(status);
            ResetBar();
        }

        public void SetStatus(int percent)
        {
            string text = "";
            for (int i=1; i<=percent; i++)
            {
                text += "\xf04d";
                if (i < percent) text += " ";
            }
            StatusBar.text = text;
        }



        public void ResetBar()
        {
            StatusBar.text = "";
        }
    }

    public Compontents Components;

    public void Start()
    {
        LoadingBar bar = new LoadingBar(transform.Find("LeftSide").gameObject, transform.Find("RightSide").gameObject, transform.Find("LoadingBar").GetComponent<TextMeshPro>());
        Components = new Compontents(transform.Find("Outliner").gameObject, transform.Find("Background").gameObject, transform.Find("Description").GetComponent<TextMeshPro>(), bar);
        StolperwegeHelper.StatusBox = this;
        Components.SetActive(false);
        transform.localPosition = Hidden;
    }

    public void Update()
    {
        if (_animOn)
        {
            _animLerp += Time.deltaTime;
            if (Active) Show();
            else Hide();
        }
    }

    private void Show()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, Shown, _animLerp);
        if (transform.localPosition == Shown)
        {
            _animLerp = 0;
            _animOn = false;
        }
    }

    private void Hide()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, Hidden, _animLerp);
        if (transform.localPosition == Hidden)
        {
            Components.SetActive(false);
            _animLerp = 0;
            _animOn = false;
        }
    }

    public IEnumerator SetInfoText(string text, bool autoClose, float closeAfterSeconds=0)
    {
        Components.SetInfoText(text);
        if (autoClose)
        {
            yield return new WaitForSeconds(closeAfterSeconds);
            Active = false;
        }
    }

    public void SetLoadingStatus(float actual, float max)
    {
        if (!Components.LoadingBar.Active) Components.LoadingBar.SetActive(true);
        Components.LoadingBar.SetStatus((int)(actual / max * 100));
    }

    public void Reset()
    {
        StartCoroutine(SetInfoText("", false));
        Active = false;
        Components.LoadingBar.ResetBar();
    }
}
