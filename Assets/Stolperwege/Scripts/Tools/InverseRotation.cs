using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InverseRotation : InteractiveObject {

    public GameObject hover;

    public bool DestroyWhenHoverNull = true;

    public Vector3 normalScale = new Vector3(0.0085f, 0.0085f, 0.0085f);

    private Vector3 _orientation = Vector3.zero;
    public new Vector3 Orientation {
        get
        {
            return _orientation;
        }

        set
        {
            _orientation = value;
        }

    }

    
    public float Scale = 1;

    public float Textsize = 1;

    public string Text
    {
        set
        {
            GetComponent<TextMeshPro>().text = value;
        }
    }

    static LabelManager manager;

    public override void Awake()
    {
        base.Awake();
        if(manager == null)
        {
            manager = new GameObject().AddComponent<LabelManager>();
        }
        
        //blockOutline = true;
    }

    public override void Start()
    {
        base.Start();

        manager.AddLabel(this);
    }


    // Update is called once per frame

    /*
        public override void Update()
        {
            base.Update();

            if (player == null)
                player = StolperwegeHelper.centerEyeAnchor?.transform;
            else if (hover != null)
            {
                transform.position = hover.transform.position + Vector3.up * 0.15f * Scale;
                //transform.LookAt(2* transform.position - player.position);
                if (StolperwegeHelper.centerEyeAnchor != null)
                    transform.forward = StolperwegeHelper.centerEyeAnchor.transform.forward;
                transform.localScale = normalScale * Textsize;
                if (GetComponent<MeshRenderer>() != null)
                    GetComponent<MeshRenderer>().enabled = hover.gameObject.activeInHierarchy;
            }
            else if (hover == null && DestroyWhenHoverNull)
                Destroy(gameObject);

        }*/

}
