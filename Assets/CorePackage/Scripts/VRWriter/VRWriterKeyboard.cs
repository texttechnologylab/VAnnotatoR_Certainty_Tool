using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRWriterKeyboard : MonoBehaviour {

    public VRWriterInterface Interface { get; private set; }

    private KeyboardLayouts.Layout _activeKeyboardLayoutType;
    public KeyboardLayouts.Layout ActiveKeyboardLayoutType
    {
        get { return _activeKeyboardLayoutType; }
        set
        {
            _activeKeyboardLayoutType = value;
            if (Layouts == null) Layouts = new Dictionary<Keyboard, Dictionary<char, KeyWithMode>>();
            foreach (Keyboard kb in Layouts.Keys)
                if (kb.Layout == _activeKeyboardLayoutType)
                {
                    ActiveKeyboard = kb;
                    return;
                }
            ActiveKeyboard = new Keyboard(_activeKeyboardLayoutType);
        }
    }
    
    private Keyboard activeKeyboard;
    public Keyboard ActiveKeyboard
    {
        get { return activeKeyboard; }
        set
        {
            if (activeKeyboard != null) SetLayoutStatus(activeKeyboard, false);
            activeKeyboard = value;
            if (Layouts == null || !Layouts.ContainsKey(activeKeyboard))
                CreateLayout();
            else
                SetLayoutStatus(activeKeyboard, true);
            AltGr.Active = Space.Active = Shift.Active = Linebreak.Active = activeKeyboard.Layout != KeyboardLayouts.Layout.Numerical;
        }
    }

    public enum KeyboardMode { Normal, Shift, AltGr }
    private KeyboardMode _mode = KeyboardMode.Normal;
    public KeyboardMode Mode
    {
        get { return _mode; }
        set
        {
            _mode = value;
            SetKeyboardMode();
        }
    }

    public bool EnableFloatingNumber = true;
    private bool _floatingPointOn;
    public bool FloatingPointOn
    {
        get { return _floatingPointOn; }
        set
        {
            if (ActiveKeyboard.Layout != KeyboardLayouts.Layout.Numerical) return;
            _floatingPointOn = EnableFloatingNumber && value;
            FloatingPoint.Active = _floatingPointOn;
        }
    }

    public bool EnableNegativeNumber = true;
    private bool _negativeOn;
    public bool NegativeOn
    {
        get { return _negativeOn; }
        set
        {
            if (ActiveKeyboard.Layout != KeyboardLayouts.Layout.Numerical) return;
            _negativeOn = EnableNegativeNumber && value;
            Negative.Active = _negativeOn;
        }
    }

    private Dictionary<Keyboard, Dictionary<char, KeyWithMode>> Layouts;

    public struct KeyWithMode
    {
        public KeyboardMode Mode;
        public VRWriterInterfaceKey Key;

        public KeyWithMode(KeyboardMode mode, VRWriterInterfaceKey key)
        {
            Mode = mode;
            Key = key;
        }

        public void SetKeyStatus(bool status)
        {
            Key.gameObject.SetActive(status);
        }
    }

    // special keys
    public VRWriterInterfaceKey Linebreak { get; private set; }
    public InteractiveButton Backspace { get; private set; }
    public InteractiveButton Delete { get; private set; }
    public VRWriterInterfaceKey Space { get; private set; }
    public InteractiveButton AltGr { get; private set; }
    public InteractiveButton Shift { get; private set; }
    public InteractiveButton ToEnd { get; private set; }
    public InteractiveButton FloatingPoint { get; private set; }
    public InteractiveButton Negative { get; private set; }

    public string InputString { get; set; }

    public void Awake()
    {
        Interface = transform.parent.GetComponent<VRWriterInterface>();

        Linebreak = transform.Find("Linebreak").GetComponent<VRWriterInterfaceKey>();
        Linebreak.Init(new Key('\n', '\0', '\0', 0, 0, "\xf3be"));
        Linebreak.InfoText = "Linebreak";

        Backspace = transform.Find("Backspace").GetComponent<InteractiveButton>();
        Backspace.OnClick = () => { Interface.Editor.Delete(); };
        Backspace.InfoText = "Backspace - delete backwards";

        Delete = transform.Find("Delete").GetComponent<InteractiveButton>();
        Delete.OnClick = () => { Interface.Editor.Delete(true); };
        Delete.InfoText = "Delete - delete forwards";

        Space = transform.Find("Space").GetComponent<VRWriterInterfaceKey>();
        Space.Init(new Key(' ', '\0', '\0', 0, 0));
        Space.InfoText = "Whitespace";

        Shift = transform.Find("Shift").GetComponent<InteractiveButton>();
        Shift.OnClick = () =>
        {
            AltGr.ButtonOn = false;
            Mode = (Mode != KeyboardMode.Shift) ? KeyboardMode.Shift : KeyboardMode.Normal;
            Shift.ButtonOn = !Shift.ButtonOn;
        };
        Shift.ButtonOn = false;
        Shift.InfoText = "Shift";

        AltGr = transform.Find("AltGr").GetComponent<InteractiveButton>();
        AltGr.OnClick = () => 
        {
            Shift.ButtonOn = false;
            Mode = (Mode != KeyboardMode.AltGr) ? KeyboardMode.AltGr : KeyboardMode.Normal;
            AltGr.ButtonOn = !AltGr.ButtonOn;
        };
        AltGr.ButtonOn = false;
        AltGr.InfoText = "AltGr";

        ToEnd = transform.Find("ToEnd").GetComponent<InteractiveButton>();
        ToEnd.OnClick = () =>
        {
            Interface.Editor.CursorToEnd();
            ToEnd.Highlight = false;
            ToEnd.Active = false;
        };
        ToEnd.InfoText = "Sets the cursor to the end of the text.";
        ToEnd.Active = false;
    }

    GameObject key; Dictionary<char, KeyWithMode> keys;
    private void CreateLayout()
    {

        Layouts.Add(ActiveKeyboard, new Dictionary<char, KeyWithMode>());

        keys = Layouts[ActiveKeyboard];

        keys.Add(' ', new KeyWithMode(KeyboardMode.Normal, Space));
        keys.Add('\n', new KeyWithMode(KeyboardMode.Normal, Linebreak));

        GameObject keyBlueprint = (GameObject)Instantiate(Resources.Load("Prefabs/VRWriter/KeyBlueprint"));

        for (int i = 0; i < ActiveKeyboard.Keys.Length; i++)
        {
            if (ActiveKeyboard.Keys[i] == null) continue;
            key = Instantiate(keyBlueprint);
            key.transform.SetParent(transform);
            key.transform.localPosition = new Vector3(ActiveKeyboard.Keys[i].Position.x, ActiveKeyboard.Keys[i].Position.y - 0.03f, 0);
            key.transform.localRotation = Quaternion.identity;
            key.GetComponent<VRWriterInterfaceKey>().Init(ActiveKeyboard.Keys[i]);
            key.name = "" + ActiveKeyboard.Keys[i].Value + ActiveKeyboard.Keys[i].ShiftValue + ActiveKeyboard.Keys[i].AltGrValue;
            keys.Add(ActiveKeyboard.Keys[i].Value, new KeyWithMode(KeyboardMode.Normal, key.GetComponent<VRWriterInterfaceKey>()));

            if (ActiveKeyboard.Keys[i].ShiftValue != '\0')
                keys.Add(ActiveKeyboard.Keys[i].ShiftValue, new KeyWithMode(KeyboardMode.Shift, key.GetComponent<VRWriterInterfaceKey>()));

            if (ActiveKeyboard.Keys[i].AltGrValue != '\0')
                keys.Add(ActiveKeyboard.Keys[i].AltGrValue, new KeyWithMode(KeyboardMode.AltGr, key.GetComponent<VRWriterInterfaceKey>()));

            if (ActiveKeyboard.Layout == KeyboardLayouts.Layout.Numerical)
            {
                if (ActiveKeyboard.Keys[i].Value == '.') FloatingPoint = key.GetComponent<VRWriterInterfaceKey>();
                if (ActiveKeyboard.Keys[i].Value == '-') Negative = key.GetComponent<VRWriterInterfaceKey>();
            }
                
        }
        transform.localPosition = new Vector3(0, -0.8f, 0.2f);
        transform.localEulerAngles = Vector3.left * 30;
        Destroy(keyBlueprint);
    }


    private void SetKeyboardMode()
    {
        foreach (char letter in Layouts[ActiveKeyboard].Keys)
            Layouts[ActiveKeyboard][letter].Key.GetComponent<VRWriterInterfaceKey>().SetKeyMode();
    }

    private WaitForSeconds _wait = new WaitForSeconds(0.1f);
    public IEnumerator AutoWriteText(string text)
    {
        if (text == null || text == "")
        {
            Debug.Log("No text to write");
            yield break;
        }
        for (int i = 0; i<text.Length; i++)
        {
            if (Interface.Inputfield != null && Interface.Inputfield.MaxChars == Interface.Editor.InputText.Length) yield break;
            if (!Layouts[ActiveKeyboard].ContainsKey(text[i]))
            {
                Debug.Log("Failed on: " + text[i] + ".");
                continue;
            }
            Layouts[ActiveKeyboard][text[i]].Key.WriteWithMode(Layouts[ActiveKeyboard][text[i]].Mode);
            yield return _wait;
        }

    }

    private void SetLayoutStatus(Keyboard keyboard, bool status)
    {
        foreach (KeyWithMode key in Layouts[keyboard].Values)
            key.SetKeyStatus(status);
    }

}
