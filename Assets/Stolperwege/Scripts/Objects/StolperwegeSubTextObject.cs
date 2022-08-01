using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StolperwegeSubTextObject : StolperwegeObject {

    public StolperwegeTextObject MainTextObject { get; set; }

    List<List<StolperwegeWordObject>> Lines = new List<List<StolperwegeWordObject>>();

    private List<StolperwegeWordObject> _words;
    public List<StolperwegeWordObject> Words {

        get
        {
            return _words;
        }
        set
        {
            _words = value;


            Referent.StolperwegeInterface.StartCoroutine(CalculateLines(_words));
        }
    }

    public float MaxLineLength { get; private set; }
    // never used
    // private float maxLineCount = 0;

    private IEnumerator CalculateLines(List<StolperwegeWordObject> wordObjs)
    {
        MaxLineLength = 0.00001f;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        List<StolperwegeWordObject> line = new List<StolperwegeWordObject>();
        Lines.Add(line);
        float length = 0;

        foreach (StolperwegeWordObject word in wordObjs)
        {
            float textSizeX = word.GetComponentInChildren<TextMeshPro>().textBounds.size.x * transform.localScale.x;
            //Debug.Log(word.Text + " " + textSizeX);
            if ((length + textSizeX) > 20f)
            {
                if (length > MaxLineLength) MaxLineLength = length;
                line = new List<StolperwegeWordObject>();
                Lines.Add(line);
                length = 0;
            }

            line.Add(word);
            length += textSizeX + 0.05f;
        }


        if (length > MaxLineLength) MaxLineLength = length;
    }

    public override void ResetObject()
    {
        transform.parent = null;
        gameObject.SetActive(false);
    }

    public override ExpandView OnExpand()
    {

        ExpandView expandView = base.OnExpand();
        expandView.dragable = false;
        expandView.clickable = false;

        expandView.SetColor(StolperwegeHelper.GUCOLOR.ORANGE);

        float xPos = 0;
        float yPos = (Lines.Count - 1) / 2f;
        for (int i = 0; i < Lines.Count; i++)
        {
            xPos = -MaxLineLength / 2;
            for (int j = 0; j < Lines[i].Count; j++)
            {
                StolperwegeWordObject word = Lines[i][j];
                word.transform.parent = expandView.transform.parent;
                word.gameObject.SetActive(true);
                float textSizeX = word.GetComponentInChildren<TextMeshPro>().textBounds.extents.x * transform.localScale.x;
                word.transform.localEulerAngles = Vector3.zero;
                word.transform.localPosition = new Vector3(xPos + textSizeX, (yPos - i) * 0.1f, 0);

                xPos += textSizeX * 2 + 0.05f;
            }
        }

        expandView.transform.localScale = new Vector3(MaxLineLength + 0.1f, 0.005f, (Lines.Count + 1) * 0.1f);
        expandView.transform.localPosition = Vector3.forward * 0.05f;

        Vector3 lookPos = Referent.StolperwegeObject.transform.position;
        lookPos.y = transform.position.y;
        expandView.transform.parent.forward = transform.position - lookPos;

        return expandView;
    }

    private int objectsCounter = 0;
    private int predicateCounter = 0;

    public void addLinkedObject(StolperwegeObject obj, float radius)
    {

        if(obj is PredicateObject)
        {
            obj.transform.position += Vector3.up * 0.3f + transform.position + Words[0].transform.forward * -0.3f*radius + Words[0].transform.right * predicateCounter * -0.6f;
            obj.transform.forward = Words[0].transform.forward;
            predicateCounter = (predicateCounter <= 0) ? (predicateCounter - 1) * -1 : predicateCounter * -1;

            StolperwegeInterface.RelationType propEventRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Proposition", "events");

            Predicate predicate = (Predicate)obj.Referent;

            Vector3 offset = (transform.position - predicate.StolperwegeObject.transform.position);
            offset.y = 0;
            offset.Normalize();
            offset *= -0.15f * radius;

            foreach (StolperwegeElement stEvent in ((Predicate)predicate).proposition.GetRelatedElementsByType(propEventRel))
            {
                if (stEvent.StolperwegeObject != null && stEvent.StolperwegeObject.gameObject.activeInHierarchy) continue;
                if (stEvent.StolperwegeObject == null) stEvent.Draw();

                stEvent.StolperwegeObject.gameObject.SetActive(true);
                stEvent.StolperwegeObject.transform.position = predicate.StolperwegeObject.transform.position + offset;
            }
            return;
        }

        obj.transform.position = transform.position + Words[0].transform.forward * -0.1f*radius + Words[0].transform.right * objectsCounter * -0.2f;

        obj.transform.forward = Words[0].transform.forward;

        objectsCounter = (objectsCounter <= 0) ? (objectsCounter - 1) * -1 : objectsCounter * -1;
    }
}
