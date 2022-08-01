using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PersonObject : StolperwegeObject {

    GameObject iconPlane = null;

    public override StolperwegeElement Referent
    {

        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;

            

            if (Referent.Value == null || Referent.Value.Length == 0)
                Label.Text = ((Person)Referent).FirstName + " " + ((Person)Referent).LastName;

            //if (!LoadImage())
            //{
            //    Texture2D texture = Text2SceneDBInterface.GetTexture("Mann");

            //    if (texture != null)
            //    {
            //        iconPlane = GetIcon(texture);
            //    }
            //}
        }
    }

    private void Update()
    {
        if (iconPlane != null && Label != null)
        {
            iconPlane.transform.rotation = Quaternion.LookRotation(Vector3.down, -Label.transform.forward);
        }
    }

    private bool LoadImage()
    {
        if (((Person)Referent).getImages().Count > 0)
        {
            StartCoroutine(((Person)Referent).getImages()[0].LoadImgToRenderer(GetComponent<MeshRenderer>()));
            return true;
        }
        return false;
    }

    public override ExpandView OnExpand()
    {
        ExpandView expandView = base.OnExpand();

        if (expandView == null) return null;

        expandView.SetColor(StolperwegeHelper.GUCOLOR.LICHTBLAU);

        addPersonView(expandView);

        return expandView;
    }

    public override ExpandView OnEnbed()
    {
        ExpandView expandView = base.OnEnbed();

        if (expandView == null) return null;

        addPersonView(expandView);

        return expandView;
    }

    private void addPersonView(ExpandView expandView)
    {
        GameObject personBox = ExpandView.createContentBox();
        Person p = (Person)Referent;

        GameObject personName = ExpandView.createText(p.FirstName + " " + p.LastName);
        GameObject personPicture = ExpandView.createContentBox();

        StolperwegeImage image = (((Person)Referent).getImages().Count > 0) ? ((Person)Referent).getImages()[0] : null;

        Person pReferent = (Person)Referent;
        if (image != null)
        {
            StartCoroutine(image.LoadImgToRenderer(personPicture.GetComponent<MeshRenderer>()));
        }

        if (pReferent.BirthDate != null)
        {
            GameObject personBirth = ExpandView.createText(pReferent.BirthDate.ToString());
            personBirth.transform.parent = personBox.transform;
            personBirth.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.2f + Vector3.forward * 0.25f;
            personBirth.transform.localScale = Vector3.up * 0.03f + Vector3.right * 0.025f + Vector3.forward * 0.025f;

            GameObject personBirthIcon = ExpandView.createContentBox();
            personBirthIcon.GetComponent<MeshRenderer>().material = Resources.Load("materials/StolperwegeMaterials/ExpandViewPersonBirth") as Material;
            personBirthIcon.transform.parent = personBox.transform;
            personBirthIcon.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.36f + Vector3.forward * 0.25f;
            personBirthIcon.transform.localScale = Vector3.up * 0.03f + Vector3.right * 0.047f + Vector3.forward * 0.047f;
        }

        if (pReferent.DeathDate != null)
        {
            GameObject personDeath = ExpandView.createText(pReferent.DeathDate.ToString());
            personDeath.transform.parent = personBox.transform;
            personDeath.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.2f + Vector3.forward * 0.15f;
            personDeath.transform.localScale = Vector3.up * 0.03f + Vector3.right * 0.025f + Vector3.forward * 0.025f;

            GameObject personBirthDeath = ExpandView.createContentBox();
            personBirthDeath.GetComponent<MeshRenderer>().material = Resources.Load("materials/StolperwegeMaterials/ExpandViewPersonDeath") as Material;
            personBirthDeath.transform.parent = personBox.transform;
            personBirthDeath.transform.localPosition = Vector3.up * -1.15f + Vector3.right * 0.36f + Vector3.forward * 0.15f;
            personBirthDeath.transform.localScale = Vector3.up * 0.03f + Vector3.right * 0.047f + Vector3.forward * 0.047f;
        }

        personPicture.transform.parent = personBox.transform;
        personPicture.transform.localPosition = Vector3.up * -1.1f - Vector3.right * 0.2f + Vector3.forward * 0.05f;
        personPicture.transform.localScale = Vector3.up + Vector3.right * 0.4f + Vector3.forward * 0.5f;

        personName.transform.parent = personBox.transform;
        personName.transform.localPosition = Vector3.up * -1.15f + Vector3.forward * 0.4f;
        personName.transform.localEulerAngles = new Vector3(270, 90, 90);
        personName.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
        personName.GetComponent<TextMeshPro>().enableAutoSizing = true;

        expandView.drawComponent(personBox.transform, ExpandView.LAYOUT.FULL);
    }

    public override void UpdateElement()
    {
        base.UpdateElement();

        LoadImage();
    }
}
