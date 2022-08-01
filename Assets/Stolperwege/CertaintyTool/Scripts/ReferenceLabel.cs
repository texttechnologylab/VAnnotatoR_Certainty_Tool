using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReferenceLabel : MonoBehaviour
{
    public GameObject RootObject;
    public string Text { get; private set; }

    public InteractiveButton Button;
    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (RootObject != null && gameObject.activeInHierarchy)
        {
            // StolperwegeHelper.CenterEyeAnchor.transform.up
            transform.position = RootObject.transform.position + transform.up * 0.3f;
            // transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
            // transform.rotation = StolperwegeHelper.CenterEyeAnchor.transform.rotation;
            Vector3 lookPosition = StolperwegeHelper.CenterEyeAnchor.transform.position;
            lookPosition.y = transform.position.y;
            transform.LookAt(lookPosition);
            
        }
        else if (RootObject == null)
        {
            gameObject.Destroy();
        }
    }

    public void SetText(string text)
    {
        Text = text;
        gameObject.GetComponentInChildren<TextMeshPro>().SetText(text);
    }
}
