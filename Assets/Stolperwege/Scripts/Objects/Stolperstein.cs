using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Stolperstein : StolperwegeObject {

    private StolperwegeImage _image;

    public override void Start()
    {
        base.Start();
        Grabable = true;
        
    }

    public StolperwegeImage image {

        get {
            return _image;
        }

        set
        {
            _image = value;
            if (_image == null) return;
            
            image.LoadImgToRenderer(GetComponent<MeshRenderer>());
        }
    }

	public override StolperwegeElement Referent { 
	
		get
		{
			return base.Referent;
		}

		set
		{
            base.Referent = value;
        
            foreach (StolperwegeImage img in ((DiscourseReferent)Referent).getImages())
                if (img.Value.Contains("justin")) image = img;

            if(image == null)
                foreach (StolperwegeImage img in ((DiscourseReferent)Referent).getImages())
                {
                    if (img.Value.ToLower().Contains("stolperstein"))
                    {
                        image = img;
                        break;
                    }
                }


            if (image == null)
                foreach (StolperwegeImage img in ((DiscourseReferent)Referent).getImages())
                {
                    image = img;
                    break;
                }

        }
	}

    public override bool OnPointerClick() {

       // GetComponent<Rigidbody>().useGravity = false;
        //GetComponent<Rigidbody>().isKinematic = false;
        //transform.position = StartPos;


        if (!base.OnPointerClick()) return false;
        //Inflated = !Inflated;
        return true;
    }

    public override GameObject OnGrab(Collider other)
    {
        float oldy = 0;
        Vector3 pos = Vector2.zero;
        if (Inflated)
        {
            Inflated = false;
            pos = transform.position;
            oldy = pos.y;
            pos.y = other.transform.position.y;
            transform.position = pos;
        }
        
        GameObject result = base.OnGrab(other);
        if (result.GetComponent<Stolperstein>().Clone)
        {
            for(int i=0; i<result.transform.childCount; i++)
                Destroy(result.transform.GetChild(i).gameObject);
        }

        if (oldy != 0)
        {
            pos.y = oldy;
            //transform.position = pos;
        }

        return result;
    }

    private void ShowSideImages(bool toggle)
    {
        for (int i = 0; i < transform.childCount; i++)
            if(transform.GetChild(i).name.Contains("Picture"))
                transform.GetChild(i).gameObject.SetActive(!toggle);
    }

    public override ExpandView OnExpand()
    {
        ExpandView expandView = base.OnExpand();

        if (expandView == null) return null;

        addView(expandView);

        return expandView;
    }

    public override ExpandView OnEnbed()
    {
        ExpandView expandView = base.OnEnbed();

        if (expandView == null) return null;

        addView(expandView);

        return expandView;
    }


    private void addView (ExpandView expandView)
    {
        GameObject stolpersteinImage = (GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperewegeExpandView"));
        stolpersteinImage.GetComponent<ExpandView>().StObject = this;

        stolpersteinImage.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;

        expandView.drawComponent(stolpersteinImage.transform, ExpandView.LAYOUT.TOPLEFT);

        Person stPerson = ((DiscourseReferent)Referent).getPersons()[0];

        if (stPerson != null)
        {
            ExpandView personExpandView = stPerson.StolperwegeObject.OnEnbed();
            expandView.drawComponent(personExpandView.transform, ExpandView.LAYOUT.TOPRIGHT);
        }

        GameObject pictureBox = (GameObject)Instantiate(Resources.Load("StolperwegeElements/ImageBox"));

        pictureBox.GetComponent<ImageBox>().Images = ((DiscourseReferent)Referent).getImages();

        expandView.drawComponent(pictureBox.GetComponent<StolperwegeObject>().OnEnbed().transform, ExpandView.LAYOUT.BOTTOM);
    } 

    private bool _inflated = false;
    public bool Inflated {
        get
        {
            return _inflated;
        }

        set
        {
            if (_inflated == value) return;

            _inflated = value;

            if (_inflated)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                stand.transform.rotation = transform.rotation;
                stand.transform.localScale = transform.localScale;
                stand.name = "stand";
                stand.GetComponent<MeshRenderer>().material = Resources.Load("materials/StolperwegeMaterials/StolpersteinStandMaterial") as Material;
                stand.GetComponent<Collider>().enabled = false;
                GetComponent<BoxCollider>().center = new Vector3(0, -4f, 0);
                GetComponent<BoxCollider>().size = new Vector3(1, 10f, 1);
                stand.transform.parent = transform;
                stand.transform.localPosition = new Vector3(0, -1, 0);

                Mesh mesh = stand.GetComponent<MeshFilter>().mesh;

                Vector3[] verts = mesh.vertices;
                Vector3[] newverts = new Vector3[verts.Length];

                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 v = verts[i];
                    if (v.y == -0.5) newverts[i] = new Vector3(v.x, v.y - 10, v.z);
                    else if (v.y == -10.5) newverts[i] = new Vector3(v.x, v.y + 10, v.z);
                    else newverts[i] = v;
                }

                mesh.vertices = newverts;
                mesh.RecalculateBounds();

                ShowSideImages(true);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
                GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
                GetComponent<BoxCollider>().size = new Vector3(1, 1, 1);
                Destroy(transform.Find("stand").gameObject);
                ShowSideImages(false);
            }

           
        }

    }

    private bool infClick = false;

    public override void OnPointerEnter(Collider other)
    {
        base.OnPointerEnter(other);

        infClick = false;
    }

    public override void CheckClick()
    {
        base.CheckClick();
        if (!infClick && SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            Inflated = !Inflated;
            infClick = true;
        }
    }
}
