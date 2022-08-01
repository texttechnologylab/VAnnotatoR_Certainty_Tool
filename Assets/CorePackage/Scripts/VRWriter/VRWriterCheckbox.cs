using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VRWriterCheckbox : InteractiveButton
{
    
    private bool _checked = false;
    public bool Checked
    {
        get { return _checked; }
        set
        {
            if (_checked == value) return;
            _checked = value;
            Textbox.text = (_checked) ? "\xf00c" : "";
            
        }
    }

    public TextMeshPro Textbox;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();
        Textbox = transform.Find("Tag").GetComponent<TextMeshPro>();
    }

}
