using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleText : InteractiveObject {

    public string Text { get; private set; }
    private GameObject textMesh;

    public override void Awake()
    {
        base.Awake();
        Grabable = true;
        ManipulatedByGravity = false;
        isThrowable = true;
        _normalScale = Vector3.one * 0.1f;
        Removable = true;
        DestroyOnObjectRemover = true;
        OnClick = () =>
        {

            if (StolperwegeHelper.GetDistanceToPlayer(gameObject) > 1.5f)
            {
                transform.parent = StolperwegeHelper.CenterEyeAnchor.transform;
                transform.localPosition = CalculateRandomRespawnPosition();
                transform.parent = null;
            }
        };
    }

    public void Init(string text)
    {
        Text = text;
        Material mat = gameObject.GetComponent<Renderer>().material;
        mat.SetColor("_Color", StolperwegeHelper.GUCOLOR.GOETHEBLAU);
        mat.SetColor("_EmissionColor", StolperwegeHelper.GUCOLOR.GOETHEBLAU);
        mat.EnableKeyword("_EMISSION");
        textMesh = new GameObject();
        transform.localScale = Vector3.one * 0.1f;
        textMesh.transform.parent = gameObject.transform;
        textMesh.transform.localEulerAngles = Vector3.up * 180;
        textMesh.transform.localPosition = Vector3.forward * 0.51f + Vector3.up * 0.01f;
        TextMeshPro tm = textMesh.AddComponent<TextMeshPro>();
        tm.font = Resources.Load<TMP_FontAsset>("FileExplorer/Font/FontAwesomeSolid");
        tm.text = Text;
        tm.fontSize = 0.1f;
        tm.alignment = TextAlignmentOptions.Center;
        GetComponent<Collider>().isTrigger = true;
    }
}
