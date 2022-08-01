using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class DataBrowserResource : InteractiveObject
{

    public List<TextMeshPro> IconTextBoxes { get; private set; }
    public MeshRenderer Renderer { get; private set; }
    public TextMeshPro InfoTextBox { get; private set; }

    public VRData Data;
    
    private void Init()
    {
        Grabable = true;
        DestroyOnObjectRemover = true;
        Removable = true;
        //SnapHand = false;
        UseHighlighting = false;
        OnClick = () =>
        {
            if (StolperwegeHelper.GetDistanceToPlayer(gameObject) >= 5)
                transform.position = CalculateRandomRespawnPosition();
        };

        IconTextBoxes = new List<TextMeshPro>();
        TextMeshPro[] textMeshes = GetComponentsInChildren<TextMeshPro>();
        for (int i = 0; i < textMeshes.Length; i++)
        {
            if (textMeshes[i].name.Equals("InfoTextBox"))
                InfoTextBox = textMeshes[i];
            else IconTextBoxes.Add(textMeshes[i]);

        }

        Renderer = transform.Find("Cube").GetComponent<MeshRenderer>();
        Renderer.material = StolperwegeHelper.ThumbnailMaterial;
        InfoTextBox = transform.Find("InfoTextBox").GetComponent<TextMeshPro>();
    }

    public void ChangeIcon(string icon)
    {
        foreach (TextMeshPro iconText in IconTextBoxes)
            iconText.text = icon;
    }

    public void SetIconStatus(bool status)
    {
        foreach (TextMeshPro iconText in IconTextBoxes)
            iconText.gameObject.SetActive(status);
    }


    public void Init(VRData data)
    {
        Init();
        Data = data;
    }

    private Vector3 eyePos;
    public void Update()
    {
        eyePos = StolperwegeHelper.CenterEyeAnchor.transform.position;
        InfoTextBox.transform.LookAt(new Vector3(eyePos.x, InfoTextBox.transform.position.y, eyePos.z));
        InfoTextBox.transform.Rotate(Vector3.up, 180);
    }
}
