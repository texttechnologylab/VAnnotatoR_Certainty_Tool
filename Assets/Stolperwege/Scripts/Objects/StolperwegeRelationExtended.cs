using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolperwegeRelationExtended : MonoBehaviour {

    private const float OFFSET_Y = -0.3f;

    public enum NominalExpressionType { CN_DEFINITE, CN_DEFINITE_METONYMIC, CN_GENERIC, CN_GENERIC_METONYMIC, PN_SINGLETON, PN_SINGLETON_METONYMIC, PN_SET, PN_SET_METONYMIC }

    public NominalExpressionType NominalExpression_Type { get; private set; }

    public StolperwegeElement StartElement { get; private set; }
    public List<HashSet<StolperwegeElement>> EndElements { get; private set; }

    public Vector3 StartCenter { get; private set; }
    public Vector3[] EndCenters { get; private set; }

    public bool Directed { get; set; }

    public Color RelColor { get; set; }

    private Transform PathContainer;
    private GameObject PathObject;
    private Transform ArrowContainer;
    private GameObject ArrowObject;

    private Hashtable ConnectedRelations;

    private int PathUpdateCounter;
    private int ArrowUpdateCounter;

    private void Init()
    {
        PathContainer = new GameObject("PathContainer").transform;
        PathContainer.parent = transform;

        PathObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        PathObject.transform.parent = transform;
        PathObject.SetActive(false);


        ConnectedRelations = new Hashtable();

        ArrowContainer = new GameObject("ArrowContainer").transform;
        ArrowContainer.parent = transform;

        ArrowObject = (GameObject)GameObject.Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelationArrow"));

        ArrowObject.transform.localScale = Vector3.one * 8f;
        ArrowObject.GetComponent<Renderer>().material.color = RelColor;
        ArrowObject.transform.parent = transform;
        ArrowObject.SetActive(false);
        ArrowObject.transform.localPosition = Vector3.zero;
        ArrowObject.transform.up = transform.up;

        if (RelColor != null) relCol1 = RelColor;

        EndCenters = new Vector3[2];
    }

    private void AddConnectedRelation(StolperwegeRelationExtended rel)
    {
        ConnectedRelations.Add(rel, true);
        rel.ConnectedRelations.Add(this, false);
    }

    private Color relCol1 = Color.green;
    private Color relCol2 = Color.red;
    private Color relColBase = StolperwegeHelper.GUCOLOR.DUNKELGRAU;


    private void Draw()
    {
        PathUpdateCounter = 0;
        ArrowUpdateCounter = 0;

        if(StartElement != null)
            StartCenter = StartElement.StolperwegeObject.transform.position + Vector3.up * OFFSET_Y;

        int c = 0;
        foreach(HashSet<StolperwegeElement> elements in EndElements)
        {
            EndCenters[c++] = GetCenterPosition(elements);
        }


        if (EndElements.Count == 1)
        {
            foreach (StolperwegeElement element in EndElements[0])
                if (element.StolperwegeObject != null && element.StolperwegeObject.gameObject.activeInHierarchy)
                    DrawPath(EndCenters[0], element.StolperwegeObject.transform, Directed, relCol1);

            if (StartElement != null)
            {
                DrawPath(StartElement.StolperwegeObject.transform.position + Vector3.up * -0.1f, StartCenter, false, relCol1);
                DrawPath(StartCenter, EndCenters[0], false, relCol1);
            }

            foreach (GameObject go in attachedObjects)
                go.transform.position = EndCenters[0];
                
        }
        else if (EndElements.Count == 2)
        {
            Vector3 diff = EndCenters[0] - EndCenters[1];

            Vector3 startPosA = StartCenter + (diff / 2);
            Vector3 startPosB = StartCenter - (diff / 2);

            EndCenters[0] -= (EndCenters[0] - startPosA).normalized * 0.5f;
            EndCenters[1] -= (EndCenters[1] - startPosB).normalized * 0.5f;

            DrawPath(StartElement.StolperwegeObject.transform.position + Vector3.up * -0.1f, StartCenter, false, relColBase);
            DrawPath(StartCenter, startPosA, false, relCol1);
            DrawPath(StartCenter, startPosB, false, relCol2);

            DrawPath(startPosA, EndCenters[0], false, relCol1);
            DrawPath(startPosB, EndCenters[1], false, relCol2);

            c = 0;
            Color[] colors = { relCol1, relCol2 };
            foreach (HashSet<StolperwegeElement> elements in EndElements)
            {
                foreach (StolperwegeElement element in elements)
                {
                    if (element.StolperwegeObject != null && element.StolperwegeObject.gameObject.activeInHierarchy)
                        DrawPath(EndCenters[c], element.StolperwegeObject.transform, Directed, colors[c]);
                }
                c++;
            }

            Vector3 center = startPosA + (EndCenters[0] - startPosA) / 2;
            Vector3 offset = (EndCenters[0] - startPosA).normalized * 0.1f;

            Vector3 centerRel = startPosB + (EndCenters[1] - startPosB) / 2;
            Vector3 offsetRel = (EndCenters[1] - startPosB).normalized * 0.1f;

            DrawPathCylinder(center - offset, centerRel - offsetRel, StolperwegeHelper.GUCOLOR.DUNKELGRAU);
            DrawPathCylinder(center + offset, centerRel + offsetRel, StolperwegeHelper.GUCOLOR.DUNKELGRAU);

        }

        /*
        if(!(float.IsNaN(StartCenter.x)|| float.IsNaN(EndCenters.x)))
        {
            foreach (StolperwegeElement element in StartElements)
                if (element.Object3D != null && element.Object3D.gameObject.activeInHierarchy)
                    DrawPath(StartCenter, element.Object3D.transform, false);

            foreach (StolperwegeElement element in EndElements)
                if (element.Object3D != null && element.Object3D.gameObject.activeInHierarchy)
                    DrawPath(EndCenter, element.Object3D.transform, Directed);

            DrawPath(StartElement.Object3D.transform.position, StartCenter + Vector3.up * -0.1f, false);
            DrawPath(StartCenter, EndCenter, false);

            foreach (Object rel in ConnectedRelations.Keys)
            {
                if (!(bool)ConnectedRelations[rel]) continue;

                StolperwegeRelationExtended stRel = (StolperwegeRelationExtended)rel;

                Vector3 center = StartCenter + (EndCenter - StartCenter) / 2;
                Vector3 offset = (EndCenter - StartCenter).normalized * 0.1f;

                Vector3 centerRel = stRel.StartCenter + (stRel.EndCenter - stRel.StartCenter) / 2;
                Vector3 offsetRel = (stRel.EndCenter - stRel.StartCenter).normalized * 0.1f;

                if (Vector3.Angle((center - offset) - (centerRel - offsetRel), (center + offset) - (centerRel + offsetRel)) < 3f)
                {
                    DrawPathCylinder(center - offset, centerRel - offsetRel, StolperwegeHelper.GUCOLOR.DUNKELGRAU);
                    DrawPathCylinder(center + offset, centerRel + offsetRel, StolperwegeHelper.GUCOLOR.DUNKELGRAU);
                }
                else
                {
                    DrawPathCylinder(center - offset, centerRel + offsetRel, StolperwegeHelper.GUCOLOR.DUNKELGRAU);
                    DrawPathCylinder(center + offset, centerRel - offsetRel, StolperwegeHelper.GUCOLOR.DUNKELGRAU);
                }

            }
        }*/

        for (int i = PathUpdateCounter; i < PathContainer.childCount; i++)
            Destroy(PathContainer.GetChild(i).gameObject);

        for (int i = ArrowUpdateCounter; i < ArrowContainer.childCount; i++)
            Destroy(ArrowContainer.GetChild(i).gameObject);
    }

    private Vector3 GetCenterPosition(HashSet<StolperwegeElement> elements)
    {
        int c = 0;
        Vector3 center = Vector3.zero;

        foreach (StolperwegeElement element in elements)
            if (element.StolperwegeObject != null && element.StolperwegeObject.gameObject.activeInHierarchy)
            {
                center += element.StolperwegeObject.transform.position;
                c++;
            }

        center /= c;

        return center;
    }

    HashSet<GameObject> attachedObjects = new HashSet<GameObject>();

    public void attachObject(GameObject go)
    {
        attachedObjects.Add(go);
    }

    private void DrawPath(Vector3 vertex, Transform trans, bool drawArrow, Color c)
    {
        DrawPath(vertex, trans.position + (trans.position-vertex).normalized * OFFSET_Y, drawArrow, c);

        //DrawPathCylinder(trans.position + Vector3.up * OFFSET_Y, trans.position + Vector3.up * -0.1f, c);
    }


    private void DrawPath(Vector3 start, Vector3 end, bool drawArrow, Color c)
    {
        end = (drawArrow) ? end - (end - start).normalized * 0.075f : end;

        if (NominalExpression_Type == NominalExpressionType.CN_GENERIC ||
            NominalExpression_Type == NominalExpressionType.CN_GENERIC_METONYMIC)
        {
            Vector3 deltaVec = end - start;
            Vector3 offset = deltaVec.normalized * 0.2f;
            Vector3 currVec = Vector3.zero;

            while ((currVec + offset).magnitude < deltaVec.magnitude)
            {
                DrawPathCylinder(start + currVec, start + currVec + offset, c);

                currVec += 2 * offset;
            }

            //DrawPathCylinder(start + currVec, end);
        }
        else
        {
            DrawPathCylinder(start, end, c);
        }

        if (drawArrow)
        {
            DrawPathArrow(end, (end - start), c);
        }
    }

    private void DrawPathCylinder(Vector3 start, Vector3 end, Color c)
    {
        Transform tempPath = (PathUpdateCounter < PathContainer.childCount) ? PathContainer.GetChild(PathUpdateCounter) : Instantiate(PathObject, PathContainer).transform;
        tempPath.position = end + (start - end) * 0.5f;
        tempPath.up = (start - end);
        tempPath.localScale = new Vector3(0.03f, (start - end).magnitude / 2, 0.03f);
        tempPath.gameObject.SetActive(true);
        tempPath.GetComponent<Renderer>().material.color = c;

        PathUpdateCounter++;
    }

    private void DrawPathArrow(Vector3 pos, Vector3 dir, Color c)
    {
        Transform arrow = (ArrowUpdateCounter < ArrowContainer.childCount) ? ArrowContainer.GetChild(ArrowUpdateCounter) : Instantiate(ArrowObject, ArrowContainer).transform;
        arrow.gameObject.SetActive(true);
        arrow.forward = dir;
        arrow.transform.position = pos;
        arrow.GetComponent<Renderer>().material.color = c;

        ArrowUpdateCounter++;
    }

    public void Update()
    {
        Draw();
    }

    /*public static StolperwegeRelationExtended CreateRelation(HashSet<StolperwegeElement> startElements, HashSet<StolperwegeElement> endElements, NominalExpressionType type)
    {
        StolperwegeRelationExtended rel = new GameObject(type.ToString()).AddComponent<StolperwegeRelationExtended>();
        rel.StartElements = startElements;
        rel.EndElements = endElements;
        rel.NominalExpression_Type = type;
        rel.RelColor = StolperwegeHelper.GUCOLOR.LICHTBLAU;
        rel.Directed = true;

        rel.Init();

        return rel;

    }*/

    public static StolperwegeRelationExtended CreateRelation(StolperwegeElement startElement, List<HashSet<StolperwegeElement>> endElements, NominalExpressionType type, bool directed, Color c)
    {

        if (endElements.Count > 1) return null;

        StolperwegeRelationExtended rel = new GameObject(type.ToString()).AddComponent<StolperwegeRelationExtended>();
        rel.StartElement = startElement;
        rel.EndElements = endElements;
        rel.NominalExpression_Type = type;
        rel.RelColor = c;
        rel.Directed = directed;

        rel.Init();

        return rel;

    }

    /*
    public static void CreateRelation(List<HashSet<StolperwegeElement>> startElements, List<HashSet<StolperwegeElement>> endElements, NominalExpressionType type)
    {
        if(startElements.Count != endElements.Count)
        {
            Debug.Log("List Elements count doesnt match!");
            return;
        }


        StolperwegeRelationExtended lastRel = null;
        for(int i=0; i<startElements.Count; i++)
        {
            StolperwegeRelationExtended rel = CreateRelation(startElements[i], endElements[i], type);

            if (lastRel != null) lastRel.AddConnectedRelation(rel);

            Color c = StolperwegeHelper.GUCOLOR.LICHTBLAU;

            switch (i)
            {
                case 1:
                    c = StolperwegeHelper.GUCOLOR.ORANGE;
                    break;
                case 2:
                    c = StolperwegeHelper.GUCOLOR.EMOROT;
                    break;
            }

            rel.RelColor = c;

            lastRel = rel;
        }
    }*/

    public static void Test()
    {
        /*
        StolperwegeElement se1 = new StolperwegeElement("1", "name1");
        StolperwegeElement se2 = new StolperwegeElement("2", "name2");
        StolperwegeElement se3 = new StolperwegeElement("3", "name3");
        StolperwegeElement se4 = new StolperwegeElement("4", "name4");

        se1.draw();
        se2.draw();
        se3.draw();
        se4.draw();

        se1.Object3D.gameObject.SetActive(true);
        se2.Object3D.gameObject.SetActive(true);
        se3.Object3D.gameObject.SetActive(true);
        se4.Object3D.gameObject.SetActive(true);

        DiscourseReferent se5 = new DiscourseReferent("5", "Mann");
        DiscourseReferent se7 = new DiscourseReferent("7", "Frau");
        DiscourseReferent se6 = new DiscourseReferent("6", "Universität");

        se5.draw();
        se6.draw();
        se7.draw();

        se5.Object3D.gameObject.SetActive(true);
        se6.Object3D.gameObject.SetActive(true);
        se7.Object3D.gameObject.SetActive(true);

        //CreateRelation(new HashSet<StolperwegeElement> { se1 }, new HashSet<StolperwegeElement> { se2, se3, se4 }, NominalExpressionType.CN_GENERIC);
        //CreateRelation(new List<HashSet<StolperwegeElement>> { new HashSet<StolperwegeElement> { se1 }, new HashSet<StolperwegeElement> { se5 } }, new List<HashSet<StolperwegeElement>> { new HashSet<StolperwegeElement> { se2, se3, se4 }, new HashSet<StolperwegeElement> { se6 } }, NominalExpressionType.CN_GENERIC_METONYMIC);
        CreateRelation(new HashSet<StolperwegeElement> { se5,se7 }, new HashSet<StolperwegeElement> {se6 }, NominalExpressionType.PN_SET);

        DiscourseReferent se10 = new DiscourseReferent("10", "Haus");
        DiscourseReferent se11 = new DiscourseReferent("11", "Universität");
        DiscourseReferent se12 = new DiscourseReferent("12", "Schule");
        DiscourseReferent se13 = new DiscourseReferent("13", "Gebäude");

        se10.draw();
        se11.draw();
        se12.draw();
        se13.draw();

        se10.Object3D.gameObject.SetActive(true);
        se11.Object3D.gameObject.SetActive(true);
        se12.Object3D.gameObject.SetActive(true);
        se13.Object3D.gameObject.SetActive(true);

        CreateRelation(new HashSet<StolperwegeElement> { se10 }, new HashSet<StolperwegeElement> { se11,se12,se13 }, NominalExpressionType.CN_GENERIC);
     
        StolperwegeText ericIsenburger = new StolperwegeText("1", "Eric Isenburger attended the Musterschule (Secondary School) in Frankfurt am Main between 1912 and 1920.");

        ericIsenburger.draw();

        ericIsenburger.Object3D.gameObject.SetActive(true);
        ericIsenburger.Object3D.transform.position += Vector3.up * -1;   */

        StolperwegeText familieMay = new StolperwegeText("21", "Im Frühjahr 1936 zog die aus Schlüchtern stammende Familie May in die ehmalige Neuhaus-Wohnung in den 3. Stock ein.");

        familieMay.Draw();

        familieMay.StolperwegeObject.gameObject.SetActive(true);
        familieMay.StolperwegeObject.transform.position += Vector3.up * -1;

        Position schlüchtern = new Position("22", "50.348,9.5271");

        schlüchtern.Draw();

        schlüchtern.StolperwegeObject.gameObject.SetActive(true);
        schlüchtern.StolperwegeObject.transform.position += Vector3.up * -1;

        StolperwegeImage ernstImage = new StolperwegeImage("23", "https://www.frankfurt.de/sixcms/media.php/674/thumbnails/may_jacob%20.jpg.510778.jpg");
        StolperwegeImage ernaImage = new StolperwegeImage("23", "https://www.frankfurt.de/sixcms/media.php/674/thumbnails/may_erna%20.jpg.510772.jpg");

        ernstImage.Draw();

        ernstImage.StolperwegeObject.gameObject.SetActive(true);
        ernstImage.StolperwegeObject.transform.position += Vector3.up * -1;

        ernaImage.Draw();

        ernaImage.StolperwegeObject.gameObject.SetActive(true);
        ernaImage.StolperwegeObject.transform.position += Vector3.up * -1;

        /*
        StolperwegeText se20 = new StolperwegeText("20", "The White House says that ...");
        DiscourseReferent se21 = new DiscourseReferent("21", "Universität");
        DiscourseReferent se23 = new DiscourseReferent("23", "Treffen");
        DiscourseReferent se25 = new DiscourseReferent("25", "Treffen");
        DiscourseReferent se26 = new DiscourseReferent("26", "Treffen");

        se20.draw();
        se21.draw();
        se23.draw();
        se25.draw();
        se26.draw();

        foreach (MeshRenderer c in se23.Object3D.gameObject.GetComponentsInChildren<MeshRenderer>())
            c.enabled = false;
        foreach (MeshRenderer c in se25.Object3D.gameObject.GetComponentsInChildren<MeshRenderer>())
            c.enabled = false;
        foreach (MeshRenderer c in se26.Object3D.gameObject.GetComponentsInChildren<MeshRenderer>())
            c.enabled = false;

        Transform man = GameObject.Find("Man").transform;
        man.parent = se23.Object3D.transform;
        man.localPosition = Vector3.down;
        se23.Object3D.gameObject.SetActive(true);

        Transform woman = GameObject.Find("Woman").transform;
        woman.parent = se26.Object3D.transform;
        woman.localPosition = Vector3.down;
        se26.Object3D.gameObject.SetActive(true);

        Transform whiteHouse = GameObject.Find("WhiteHouse").transform;
        whiteHouse.parent = se25.Object3D.transform;
        whiteHouse.localPosition = Vector3.forward;
        se25.Object3D.gameObject.SetActive(true);

        CreateRelation(se20, new List<HashSet<StolperwegeElement>> { new HashSet<StolperwegeElement> { se23,se26 }, new HashSet<StolperwegeElement> { se25 } }, NominalExpressionType.PN_SINGLETON_METONYMIC);*/

        /*
        DiscourseReferent se30 = new DiscourseReferent("30", "Universität");
        DiscourseReferent se31 = new DiscourseReferent("31", "Universität");
        DiscourseReferent se32 = new DiscourseReferent("32", "Gesetz");
        DiscourseReferent se33 = new DiscourseReferent("33", "Gesetz");
        DiscourseReferent se34 = new DiscourseReferent("34", "Gesetz");
        DiscourseReferent se35 = new DiscourseReferent("35", "Gesetz");

        se30.draw();
        se31.draw();
        se32.draw();
        se33.draw();
        se34.draw();
        se35.draw();

        CreateRelation(new List<HashSet<StolperwegeElement>> { new HashSet<StolperwegeElement> { se30 }, new HashSet<StolperwegeElement> { se31 } }, new List<HashSet<StolperwegeElement>> { new HashSet<StolperwegeElement> { se32, se33 }, new HashSet<StolperwegeElement> { se34, se35 } }, NominalExpressionType.PN_SET_METONYMIC);
    */
    }
}
