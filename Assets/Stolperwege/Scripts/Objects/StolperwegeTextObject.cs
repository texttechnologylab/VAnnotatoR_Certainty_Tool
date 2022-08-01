using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StolperwegeTextObject : StolperwegeObject {

    TextMeshPro textMesh;
    GameObject background;

    public bool copy = true;

	// Use this for initialization
	public override void Awake () {
        base.Awake();
        //blockOutline = true;
	}

    private List<StolperwegeWordObject> wordObjs;

    private List<StolperwegeSubTextObject> subTexts;

    public PredicateObject ActivatedPredicate { get; set; }

    public override StolperwegeElement Referent
    {
        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;

            CreateWordArray();
        }
    }

    private void CreateWordArray()
    {
        string[] words = Referent.Value.Replace("\n"," ").Split(' ');

        wordObjs = new List<StolperwegeWordObject>();

        Object prefab = Resources.Load("StolperwegeElements/StolperwegeWordObject");
        int i = 1;
        foreach (string word in words)
        {
            if (word.Length <= 0) continue;

            StolperwegeWordObject wordObj = ((GameObject)Instantiate(prefab)).GetComponent<StolperwegeWordObject>();
            wordObj.Text = word;
            wordObj.Referent = Referent;
            wordObj.Range = new Vector2(i,i++);
            wordObj.gameObject.SetActive(false);
            wordObjs.Add(wordObj);
        }

        subTexts = new List<StolperwegeSubTextObject>();


        prefab = Resources.Load("StolperwegeElements/StolperwegeSubTextObject");

        StolperwegeSubTextObject current = null;
        List<StolperwegeWordObject> sentence = null;
        foreach (StolperwegeWordObject word in wordObjs)
        {
            if (current == null)
            {
                current = ((GameObject)Instantiate(prefab)).GetComponent<StolperwegeSubTextObject>();
                current.Referent = Referent;
                current.MainTextObject = this;
                current.gameObject.SetActive(false);
                subTexts.Add(current);
                sentence = new List<StolperwegeWordObject>();
            }

            sentence.Add(word);
            word.SubText = current;
            int temp;

            if (word.Text.EndsWith(".") 
                && !int.TryParse(word.Text.Substring(word.Text.Length-2,1).Replace(".",""), out temp) 
                && !word.Text.Equals("[...]") 
                && !word.Text.Equals("...")
                && !word.Text.Equals("bzw.") 
                && !word.Text.Equals("geb.")
                && !word.Text.EndsWith("St.")
                && word.Text.Length > 2
                && !word.Text.Equals("Dr."))
            {
                current.Words = sentence;
                current = null;
                sentence = null;
            }
        }

        if(current != null)
            current.Words = sentence;

        Referent.StolperwegeObject = this;
    }

    public override GameObject OnGrab(Collider other)
    {
        base.OnGrab(other);

        foreach (StolperwegeSubTextObject subtext in subTexts)
            subtext.ResetObject();

        if (((DiscourseReferent)Referent).GetUnityPosition() != null && copy && nearTimer())
        {
            Hashtable keys = new Hashtable();

            keys.Add("value", Referent.Value);
            keys.Add("description", StolperwegeInterface.lastTimer);
            keys.Add("equivalent", Referent.ID);

            StartCoroutine(StolperwegeInterface.CreateElement("text", keys, (StolperwegeElement e) => {; e.StolperwegeObject.OnGrab(other);  }));

            Destroy(gameObject);
        }

        return gameObject;
    }

    private bool nearTimer()
    {
        Vector3 pos = ((DiscourseReferent)Referent).GetUnityPosition().Position;

        Collider[] colls = Physics.OverlapSphere(pos, 1f);

        foreach (Collider c in colls)
            if (c.GetComponent<EvaluationTimer>() != null)
                return true;

        return false;
    }

    float radius = 0;

    public override ExpandView OnExpand()
    {
        dragging = false;
        Grabbing.Clear();
        resetScale();
        transform.position += Vector3.up * -0.4f;

        float deltaRot = 270f / subTexts.Count;
/*
        radius = (1/Mathf.Tan((deltaRot*Mathf.Deg2Rad)/2))*3f;

        radius = Mathf.Max(radius, 2f);*/

        Vector3 startAxis =  Quaternion.AngleAxis(StolperwegeHelper.CenterEyeAnchor.transform.eulerAngles.y, Vector3.up) * Vector3.forward;

        float textLength = 0;

        foreach (StolperwegeSubTextObject obj in subTexts)
            textLength += obj.MaxLineLength + 0.75f;

        textLength += 0.25f * textLength;

        radius = textLength / (2 * Mathf.PI);

        float currRot = 0;
        foreach (StolperwegeSubTextObject obj in subTexts)
        {
            deltaRot = 360 * ((obj.MaxLineLength+.5f) / textLength);
            
            Quaternion q = Quaternion.AngleAxis(currRot+deltaRot/2 , Vector3.up);
            currRot += deltaRot;

            obj.transform.position = (transform.position+(Vector3.up * 0.75f)) + q * (radius*startAxis);
            transform.rotation = q;

            obj.gameObject.SetActive(true);
            ExpandView eView = obj.OnExpand();
            Debug.Log(eView.transform.localScale.x);

        }

        //ShowAllRelatedObjects();
        //ShowPredicates();
        ShowLinkedObjects();

        return null;
    }


    

    public StolperwegeWordObject GetWord(int index)
    {
        foreach(StolperwegeWordObject word in wordObjs)
        {
            if (index >= word.Range.x && index <= word.Range.y)
                return word;
        }

        return null;
    }

    public List<StolperwegeWordObject> GetWords(int begin, int end)
    {
        List<StolperwegeWordObject> words = new List<StolperwegeWordObject>();

        foreach (StolperwegeWordObject word in wordObjs)
        {
            if ((word.Range.x <= end && word.Range.x >= begin) || word.Range.y <= end && word.Range.y >= begin)
                words.Add(word);
        }
    

        return words;
    }

    private float lastCheck = 0;

    public void Update()
    {

        if(Time.time - lastCheck > 1)
        {
            lastCheck = Time.time;
            //CheckWordAnchors();
        }
    }


    //Alte Darstellung mit Knoten
    private void CheckWordAnchors()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            StolperwegeRelationAnchor anchor = transform.GetChild(i).GetComponent<StolperwegeRelationAnchor>();

            if(anchor != null)
            {
                StolperwegeObject obj = anchor.OtherEnd.GetComponentInParent<StolperwegeObject>();

                if(obj != null && obj.Referent != null && obj.Referent.Begin != 0)
                {
                    StolperwegeWordObject word = GetWord(obj.Referent.Begin);

                    if (word.gameObject.activeInHierarchy)
                    {
                        anchor.transform.parent = word.transform;
                        anchor.transform.localPosition = Vector3.forward * 0.1f;

                        if(obj.transform.parent == null)
                            word.SubText.addLinkedObject(obj,radius);

                        Vector3 offset = (transform.position - word.SubText.transform.position);
                        offset.y = 0;
                        offset.Normalize();
                        offset *= 0.6f*radius;

                        foreach(StolperwegeElement element in obj.Referent.RelatedElements)
                        {
                            if (element.StolperwegeObject!= null && element.StolperwegeObject.gameObject.activeInHierarchy) continue;
                            if (element.StolperwegeObject == null) element.Draw();
                            if (element == Referent || element.StolperwegeObject == null) continue;

                            element.StolperwegeObject.gameObject.SetActive(true);

                            //element.Object3D.CalculatePos(offset);
                            element.StolperwegeObject.transform.position = obj.transform.position + offset;

                            /*foreach(StolperwegeElement equi in element.GetRelatedElementsByType(StolperwegeInterface.EquivalentRelation))
                            {
                                equi.Object3D.gameObject.SetActive(true);
                                equi.Object3D.CalculatePos(offset);
                            }*/
                        }
                    }
                        
                    
                    if(obj.Referent.Begin != obj.Referent.End)
                    {
                        StolperwegeWordObject word2 = GetWord(obj.Referent.End);

                        if(word2 != word)
                        {
                            StolperwegeRelation relation = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"))).GetComponent<StolperwegeRelation>();

                            relation.StartAnchor.SetParent(word2.transform, false);
                            relation.EndAnchor.SetParent(anchor.OtherEnd.transform.parent, false);
                            relation.StartAnchor.localPosition = Vector3.forward * 0.1f;
                            relation.EndAnchor.localPosition = Vector3.zero;
                        }
                    }
                        

                }
            }
        }
    }

    private Color[] propositonColors = { StolperwegeHelper.GUCOLOR.EMOROT, StolperwegeHelper.GUCOLOR.ORANGE, StolperwegeHelper.GUCOLOR.GRUEN ,StolperwegeHelper.GUCOLOR.PURPLE};

    Hashtable wordCounter = new Hashtable();


    //Neue Darstellung mit Elementen über und Propositionen unter den Texten
    public void ShowLinkedObjects()
    {
        StolperwegeInterface.RelationType termArgumentRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "argument");
        StolperwegeInterface.RelationType termPredRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "predicate");
        foreach (StolperwegeElement referent in Referent.GetRelatedElementsByType(StolperwegeInterface.EquivalentRelation))
        {
            StolperwegeWordObject wordObj = GetWord(referent.Begin);

            if (wordObj == null || !(referent is DiscourseReferent)) continue;

            wordObj.ConnectedReferent = (DiscourseReferent) referent;

            foreach(StolperwegeElement rep in referent.GetRelatedElementsByType(StolperwegeInterface.EquivalentRelation))
            {
                if (rep == Referent || wordObj == null) continue;

                AddStolperwegeElement(wordObj, rep);
            }
        }

        foreach (StolperwegeElement term in Referent.GetRelatedElementsByType(termArgumentRel))
        {
            foreach (StolperwegeElement predicate in term.GetRelatedElementsByType(termPredRel))
            {
                AddPredicate((Predicate)predicate);
            }
        }
    }

    float deltaY = 0.25f;
    public void AddStolperwegeElement(StolperwegeWordObject wordObj, StolperwegeElement element)
    {
        if (!wordCounter.Contains(wordObj))
            wordCounter.Add(wordObj, new List<GameObject>());

        int counter = ((List<GameObject>)wordCounter[wordObj]).Count;
        counter++;

        GameObject go = element.Draw();

        go.transform.position = wordObj.transform.position + Vector3.up * deltaY * counter;
        go.transform.forward = wordObj.transform.forward;
        go.GetComponent<StolperwegeObject>().RelationsVisible = false;

        ((List<GameObject>)wordCounter[wordObj]).Add(go);

        go.transform.parent = wordObj.transform;
    }

    public void RmvStolperwegeElement(StolperwegeObject stObj, StolperwegeWordObject wordObj)
    {

        ((List<GameObject>)wordCounter[wordObj]).Remove(stObj.gameObject);

        Destroy(stObj.gameObject);

        int i = 1; 
        foreach(GameObject go in (List<GameObject>)wordCounter[wordObj])
        {
            go.transform.position = wordObj.transform.position + Vector3.up * deltaY * i;
        }
    }

    Hashtable predCounter = new Hashtable();

    public void AddPredicate(Predicate predicate)
    {
        StolperwegeInterface.RelationType eventProptRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Event", "propositions");
        StolperwegeInterface.RelationType propEventRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Proposition", "events");
        StolperwegeInterface.RelationType eventEnd = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Event", "endsWith");
        StolperwegeInterface.RelationType eventStart = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Event", "startsWith");
        StolperwegeWordObject wordObj = GetWord(predicate.Begin);

        if (!predCounter.Contains(wordObj.SubText))
            predCounter.Add(wordObj.SubText, 0);

        int counter = (int)predCounter[wordObj.SubText];
        counter++;

        GameObject go = predicate.Draw();

        go.transform.position = wordObj.transform.position + Vector3.up * -deltaY * counter + Vector3.up * 0.03f;
        go.transform.right = -wordObj.transform.forward;
        go.transform.parent = wordObj.transform;
        go.GetComponent<StolperwegeObject>().RelationsVisible = false;
        go.GetComponent<PredicateObject>().SetColor(propositonColors[counter - 1]);
        ExpandView textBackgound = wordObj.SubText.transform.parent.GetComponentInChildren<ExpandView>();
        GameObject background = Instantiate(textBackgound.gameObject);

        background.GetComponent<ExpandView>().enabled = false;
        Vector3 backgroundPos = textBackgound.transform.position;
        backgroundPos.y = go.transform.position.y - 0.03f;
        background.transform.position = backgroundPos;
        background.transform.rotation = textBackgound.transform.rotation;

        predCounter[wordObj.SubText] = counter;

        foreach (ArgumentRoleObject role in go.GetComponentsInChildren<ArgumentRoleObject>())
        {
            if (role.Referent == null)
            {
                role.gameObject.SetActive(false);
                continue;
            }

            StolperwegeWordObject argWord = GetWord(role.Referent.Begin);
            role.transform.position = argWord.transform.position + Vector3.up * -deltaY * counter + Vector3.up * 0.03f;
            role.transform.right = -argWord.transform.forward;
        }

        PropositionObject prop = go.GetComponentInChildren<PropositionObject>();
        Transform expandView = wordObj.SubText.transform.parent.GetComponentInChildren<ExpandView>().transform;

        Vector3 pos = wordObj.SubText.transform.position + expandView.right * (expandView.localScale.x / 2 + 0.1f);
        pos.y = go.transform.position.y;
        prop.transform.position = pos;
        prop.GetComponent<StolperwegeObject>().RelationsVisible = false;

        float i = 0.20f;

        HashSet<StolperwegeElement> events = prop.Referent.GetRelatedElementsByType(propEventRel);
        events.UnionWith(prop.Referent.GetRelatedElementsByType(eventProptRel));
        foreach (StolperwegeElement eventE in events)
        {
            GameObject eventObj = eventE.Draw();

            Vector3 posE = prop.transform.position + expandView.right * (i + counter * 0.1f);
            eventObj.transform.position = posE;
            eventObj.transform.rotation = go.transform.rotation;
            eventObj.GetComponent<StolperwegeObject>().RelationsVisible = false;

            foreach (StolperwegeRelationAnchor anchor in eventObj.GetComponentsInChildren<StolperwegeRelationAnchor>(true))
            {
                if (anchor.Relation.type.id.Equals(eventStart.id) || anchor.Relation.type.id.Equals(eventEnd.id))
                {
                    anchor.gameObject.SetActive(true);
                    anchor.Relation.DrawOnGround = true;
                }
            }

            i += 0.2f;
        }

        if(events.Count == 0)
        {
            GameObject eventDummy = StolperwegeInterface.CreateElementDummy("org.hucompute.publichistory.datastore.typesystem.Event");

            Vector3 posE = prop.transform.position + expandView.right * (i + counter * 0.1f);
            eventDummy.transform.position = posE;
            eventDummy.transform.rotation = go.transform.rotation;

            eventDummy.GetComponent<NewElement>().excecuteOnUpload = (StolperwegeElement e) =>
            {
                e.AddStolperwegeRelation(prop.Referent, eventProptRel, true);
                e.StolperwegeObject.transform.position = posE;
                e.StolperwegeObject.transform.parent = go.transform;
                e.StolperwegeObject.transform.rotation = go.transform.rotation;
                e.StolperwegeObject.GetComponent<StolperwegeObject>().RelationsVisible = false;
            };
        }

        foreach (StolperwegeElement setE in prop.Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation))
        {
            if (!(setE is StolperwegeSet)) continue;

            StolperwegeSetRelation.Init((StolperwegeSet)setE);
        }
    }

    private void ShowPredicates()
    {
        StolperwegeInterface.RelationType termArgumentRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "argument");
        StolperwegeInterface.RelationType termPredRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "predicate");
        StolperwegeInterface.RelationType propEventRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Proposition", "events");

        foreach (StolperwegeElement term in Referent.GetRelatedElementsByType(termArgumentRel))
        {
            foreach (StolperwegeElement predicate in term.GetRelatedElementsByType(termPredRel))
            {
                if (predicate.StolperwegeObject == null) predicate.Draw();
                predicate.StolperwegeObject.gameObject.SetActive(true);
            }
        }
    }

    public void AddArgumentRole(ArgumentRoleObject roleObject)
    {
        StolperwegeWordObject argWord = GetWord(roleObject.Referent.Begin);
        Vector3 pos = argWord.transform.position;
        pos.y = roleObject.GetComponentInParent<PredicateObject>().transform.position.y;
        roleObject.transform.position = pos;

        roleObject.transform.right = -argWord.transform.forward;
    }

}
