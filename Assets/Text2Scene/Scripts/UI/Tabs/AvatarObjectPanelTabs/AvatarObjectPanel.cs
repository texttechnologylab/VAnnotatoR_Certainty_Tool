using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static SceneBuilder;

public class AvatarObjectPanel : AvatarObjectPanelTab
{
    private readonly string REMINDERMESSAGE = "The avatar has unsaved changes.\nDo you want to save them before closing?";
    private InteractiveButton SwitchButton;
    private InteractiveButton SaveButton;
    private UMAController AvatarController;

    private GameObject _reminder;
    private TextMeshPro _textField;
    private InteractiveButton _acceptButton;
    private InteractiveButton _declineButton;
    private InteractiveButton _closeButton;
    public GameObject Reminder
    {
        get
        {
            if (_reminder == null)
            {
                _reminder = ((GameObject)Instantiate(Resources.Load("Prefabs/Reminder")));
                _reminder.transform.SetParent(transform);
                _reminder.transform.localRotation = Quaternion.identity;
                _reminder.transform.localPosition = Vector3.forward * 0.05f;
                _textField = _reminder.transform.Find("Text").GetComponent<TextMeshPro>();
                _acceptButton = _reminder.transform.Find("AcceptButton").GetComponent<InteractiveButton>();
                _declineButton = _reminder.transform.Find("DeclineButton").GetComponent<InteractiveButton>();
                _closeButton = _reminder.transform.Find("CloseButton").GetComponent<InteractiveButton>();
            }
            return _reminder;
        }
    }

    private int _buttonIndex;
    private readonly float _switchBtnPosition = -0.165f;
    public int ButtonIndex
    {
        get { return _buttonIndex; }
        set
        {
            _buttonIndex = value;
            if(_buttonIndex == 0)
            {
                SwitchButton.transform.localPosition = new Vector3(_switchBtnPosition, SwitchButton.transform.localPosition.y, SwitchButton.transform.localPosition.z);
                SwitchButton.ChangeText(">");
            } else
            {
                SwitchButton.transform.localPosition = new Vector3(_switchBtnPosition * -1, SwitchButton.transform.localPosition.y, SwitchButton.transform.localPosition.z);
                SwitchButton.ChangeText("<");
            }
            SubTabs[0].TabButton.gameObject.SetActive(_buttonIndex == 0);
            SubTabs[1].TabButton.gameObject.SetActive(_buttonIndex == 0);
            SubTabs[2].TabButton.gameObject.SetActive(_buttonIndex == 0);
            SubTabs[3].TabButton.gameObject.SetActive(_buttonIndex == 1);
            SubTabs[4].TabButton.gameObject.SetActive(_buttonIndex == 1);
        }
    }

    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        throw new System.NotImplementedException();
    }

    private TextAnnotatorInterface TextAnnotator
    {
        get { return SceneController.GetInterface<TextAnnotatorInterface>(); }
    }

    private UMAISOObject _umaIsoObject;
    public void Initialize(GameObject parent, InteractiveShapeNetObject iShapeObject)
    {
        this.iShapeObject = iShapeObject;
        AvatarController = this.iShapeObject.GetComponentInChildren<UMAController>();

        SubTabs.Add(transform.Find("GeneralTab").GetComponent<GeneralTab>().Initialize(this));
        SubTabs.Add(transform.Find("EditHeadTab").GetComponent<EditHeadTab>().Initialize(this));
        SubTabs.Add(transform.Find("EditUpperBodyTab").GetComponent<EditUpperBodyTab>().Initialize(this));
        SubTabs.Add(transform.Find("EditLowerBodyTab").GetComponent<EditLowerBodyTab>().Initialize(this));
        SubTabs.Add(transform.Find("WardrobeTab").GetComponent<WardrobeTab>().Initialize(this));

        SwitchButton = transform.Find("SwitchButton").GetComponent<InteractiveButton>();
        SwitchButton.OnClick = () => { ButtonIndex = (ButtonIndex - 1) * -1; };

        SaveButton = transform.Find("SaveButton").GetComponent<InteractiveButton>();
        SaveButton.OnClick = () => {
            _umaIsoObject = iShapeObject.GetComponentInChildren<UMAISOObject>();
            Save(_umaIsoObject);
            TextAnnotator.FireWorkBatchCommand(null, null, _umaIsoObject.GetChanges(), null);
            TextAnnotator.ActualDocument.Document.HasChanges = true;
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().SaveDocumentBtn.Active = true;
        };

        ButtonIndex = 0;
        ActiveTab = SubTabs[0];
    }

    public delegate void OnDestroy();
    public void SafeDestroy(OnDestroy onDestroy)
    {
        if (ChildrenHaveChanges())
        {
            InitReminder(REMINDERMESSAGE, () =>
            {
                Save(iShapeObject.GetComponentInChildren<UMAISOObject>());
                onDestroy();
            }, () => onDestroy(), () => onDestroy());
        }
        else onDestroy();
    }

    /// <summary>
    /// Initializes a popup window for a reminder message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="acceptText"></param>
    /// <param name="accept"></param>
    /// <param name="closeText"></param>
    /// <param name="close"></param>
    public void InitReminder(string msg, ButtonEvent accept, ButtonEvent decline = null, ButtonEvent close = null)
    {
        Reminder.SetActive(true);
        _textField.text = msg;

        _acceptButton.gameObject.SetActive(true);
        _acceptButton.OnClick = () => { accept?.Invoke(); };

        if (decline != null)
        {
            _declineButton.gameObject.SetActive(true);
            _declineButton.OnClick = decline.Invoke;
            Vector3 localPos = _acceptButton.transform.localPosition;
            localPos.x = 0.1f;
            _acceptButton.transform.localPosition = localPos;
            localPos = _declineButton.transform.localPosition;
            localPos.x = -0.1f;
            _declineButton.transform.localPosition = localPos;
        }
        else
        {
            _declineButton.gameObject.SetActive(false);
            Vector3 localPos = _acceptButton.transform.localPosition;
            localPos.x = 0;
            _acceptButton.transform.localPosition = localPos;
        }

        _closeButton.gameObject.SetActive(close != null);
        if (close != null) _closeButton.OnClick = close.Invoke;
    }
}
