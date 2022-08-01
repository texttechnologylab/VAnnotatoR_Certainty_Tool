using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ImageBox : StolperwegeObject {


    private List<StolperwegeImage> _images;

    public List<StolperwegeImage> Images
    {
        get
        {
            return _images;
        }

        set
        {
            _images = value;

            if (_images.Count <= 0) return;

            for(int i=0; i<transform.childCount; i++)
            {
                StartCoroutine(_images[i%_images.Count].LoadImgToRenderer(transform.GetChild(i).GetComponent<MeshRenderer>()));
            }
        }
    }

    public override ExpandView OnExpand()
    {
        ExpandView expandView = base.OnExpand();

        int i = -Images.Count/2;
        foreach(StolperwegeImage image in Images)
        {
            image.StolperwegeObject.transform.position = transform.position;
            ExpandView eView = image.StolperwegeObject.OnEnbed();
            eView.transform.parent = expandView.transform.parent;
            eView.transform.localPosition = Vector3.right * i++ * 0.75f;

            eView.transform.localEulerAngles = new Vector3(270, 90, 90);
        }

        expandView.GetComponent<BoxCollider>().enabled = false;
        expandView.GetComponent<MeshRenderer>().enabled = false;

        /*
        ExpandView result = base.onExpand();

        int count = (int)Mathf.Sqrt(Images.Count);
        print(Images.Count);
        int c = 0;
        for (int i = 0; i < Mathf.CeilToInt(Images.Count / 4f); i++)
        {
            for (int j = -Mathf.Min(4, Images.Count) / 2; j < Mathf.Min(4, Images.Count) / 2; j++)
            {
                if (c == Images.Count - 1) goto endofloop;
                StolperwegeImage image = Images[c++];
                image.Object3D.transform.position = transform.position;
                ExpandView eView = image.Object3D.onExpand();
                eView.transform.position += eView.transform.right * i * 0.75f + eView.transform.forward * j * 0.75f + eView.transform.forward * 0.75f * (count / 2);
                eView.transform.parent = result.transform;
            }
        }

        endofloop: { };

        */

        return null;
    }

    public override ExpandView OnEnbed()
    {
        ExpandView result = base.OnEnbed();

        GameObject image1 = ExpandView.createContentBox();
        GameObject image2 = ExpandView.createContentBox();
        if (Images.Count >= 2)
        {
            StartCoroutine(Images[0].LoadImgToRenderer(image1.GetComponent<MeshRenderer>()));
            StartCoroutine(Images[1].LoadImgToRenderer(image2.GetComponent<MeshRenderer>()));

            if (Images.Count > 2)
            {
                GameObject imageCount = ExpandView.createContentBox();

                imageCount.GetComponent<MeshRenderer>().material.color = new UnityEngine.Color(1, 1, 1, 0.3f);

                GameObject count = ExpandView.createText("+" + (Images.Count - 1));
                count.transform.parent = imageCount.transform;
                count.transform.localPosition = Vector3.zero + Vector3.up * -1;
                count.transform.localScale = Vector3.one * 0.1f;
                count.transform.localEulerAngles = new Vector3(270, 90, 90);
                count.GetComponent<TextMeshPro>().enableAutoSizing = true;
                count.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                Material m = new Material(count.GetComponent<MeshRenderer>().material);
                m.color = new UnityEngine.Color(1, 1, 1, 0.3f);
                count.GetComponent<MeshRenderer>().material = m;

                result.drawComponent(imageCount.transform, ExpandView.LAYOUT.RIGHT);
            }
        }


        result.drawComponent(image1.transform, ExpandView.LAYOUT.LEFT);
        result.drawComponent(image2.transform, ExpandView.LAYOUT.RIGHT);
            

        result.ScaleMultiplier = new Vector2(1, 0.5f);
        result.StartScale = result.transform.localScale;
        return result;

    }

    public override GameObject OnGrab(Collider other)
    {
        GameObject result = base.OnGrab(other);

        if (result.GetComponent<ImageBox>().Clone)
            result.GetComponent<ImageBox>().Images = Images;

        return result;
    }

    public override void Awake()
    {
        base.Awake();
        Clone = true;
    }
}
