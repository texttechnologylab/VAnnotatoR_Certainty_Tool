using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeLine : MonoBehaviour {

    public GameObject MainBox;
    public GameObject SeperatorOuter;
    public GameObject Marker;
    public GameObject TimeSpanObject;

    public enum TimeLevel { DECADE, YEAR, MONTH, DAY, HOUR}
    public enum MONTHS { JANUARY,FEBRUARY, MARCH, APRIL, MAY, JUNE, JULY, AUGUST, SEPTEMBER, OCTOBER,NOVEMBER, DEZEMBER}

    private float OuterDiff;
    private float Length;

    public string Date
    {
        get; set;
    }

    public float Level
    {
        get; set;
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Length = MainBox.transform.localScale.z;
        OuterDiff = Length / 25;
        Generate();
        Spans = new HashSet<TimeSpan>();

        Color lbTrans = StolperwegeHelper.GUCOLOR.LICHTBLAU;
        lbTrans.a = 144f/255f;
        MainBox.GetComponent<Renderer>().material.color = lbTrans;
        MainBox.GetComponent<Renderer>().material.renderQueue = 2999;
        SeperatorOuter.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.LICHTBLAU;
        Marker.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.ORANGE;
        /*
        SetMarker(3, 3, 1980);
        SetMarker(3, 1, 1990);
        SetMarker(3, 8, 1975);
        SetMarker(3, 12, 2005);
        SetMarker(3, 1, 1999);

        

        SetTimeSpan(3, 1, 1999, 3, 12, 2005);
        SetTimeSpan(3, 1, 2000, 3, 12, 2025);
        SetTimeSpan(3, 1, 1980, 3, 12, 2001);
        SetTimeSpan(3, 1, 1970, 3, 12, 1975);
        SetTimeSpan(3, 1, 1960, 3, 12, 1980);*/

        SetMarker(8,10,1904);
        SetMarker(22,10,1922);
        SetMarker(18,3,1928);
        SetMarker(0,1,1936);

        SetTimeSpan(1, 1, 1937, 1, 1, 1939);
        SetMarker(1, 1, 1939);
    }

    int startYear = 1920;

    private void Generate()
    {
        float currZ = -Length/2;

        int i = 0;

        while(currZ <= Length/2)
        {
            GameObject sep = Instantiate(SeperatorOuter,transform);
            TextMeshPro text = sep.GetComponentInChildren<TextMeshPro>();
            text.gameObject.SetActive(false);

            Vector3 pos = SeperatorOuter.transform.localPosition;
            pos.z = currZ;
            pos.y = -0.75f;
            sep.transform.localPosition = pos;

            if (i % 10 == 0)
            {

                text.gameObject.SetActive(true);
                text.text = startYear + i + "";

                pos.y = 0;
                sep.transform.localPosition = pos;
                Vector3 scale = sep.transform.localScale;
                scale.x = MainBox.transform.localScale.y;
                sep.transform.localScale = scale;
            }
            else if(i % 5 == 0)
            {
                pos.y = -MainBox.transform.localScale.y/4;
                sep.transform.localPosition = pos;
                Vector3 scale = sep.transform.localScale;
                scale.x = MainBox.transform.localScale.y/2;
                sep.transform.localScale = scale;
            }

            sep.SetActive(true);

            currZ += OuterDiff;
            i++;
        }
    }

    private Vector3 GetPosForDate(int day, int month, int year)
    {
        int yearDiff = year - startYear;

        if (yearDiff < 0) return Vector3.negativeInfinity;

        Vector3 pos = Vector3.zero;

        pos -= Vector3.forward * Length/2;

        pos += (Vector3.forward * (OuterDiff * yearDiff));
        pos += (Vector3.forward * ((month-1) * OuterDiff/12));

        return pos;
    }

    private void SetMarker(int day, int month, int year)
    {
        GameObject marker = Instantiate(Marker, transform);

        marker.SetActive(true);

        Vector3 pos = GetPosForDate(day, month, year);
        pos.y = 0.5f;
        marker.transform.localPosition = pos;
    }

    private struct TimeSpan
    {
        public int dayStart;
        public int monthStart;
        public int yearStart;

        public int dayEnd;
        public int monthEnd;
        public int yearEnd;

        public bool Intersect(TimeSpan span)
        {
            return (span.yearEnd < yearEnd && span.yearEnd > yearStart ||
                span.yearStart < yearEnd && span.yearStart > yearStart );
        }
    }

    private HashSet<TimeSpan> Spans;

    private void SetTimeSpan(int dayStart, int monthStart, int yearStart, int dayEnd, int monthEnd, int yearEnd)
    {
        TimeSpan span = new TimeSpan
        {
            dayStart = dayStart,
            dayEnd = dayEnd,
            monthStart = monthStart,
            monthEnd = monthEnd,
            yearStart = yearStart,
            yearEnd = yearEnd
        };

        int i = 0;

        foreach (TimeSpan s in Spans)
            if (s.Intersect(span) || span.Intersect(s)) i++;

        Spans.Add(span);

        GameObject spanObject = Instantiate(TimeSpanObject, transform);

        spanObject.SetActive(true);

        float yPos = -1.5f - 0.6f * i;

        Vector3 start = GetPosForDate(dayStart, monthStart, yearStart);
        Vector3 end = GetPosForDate(dayEnd, monthEnd, yearEnd);

        Transform centerTrans = spanObject.transform.Find("Center");
        Transform leftTrans = spanObject.transform.Find("Left");
        Transform rightTrans = spanObject.transform.Find("Right");

        centerTrans.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.ORANGE;
        leftTrans.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.ORANGE;
        rightTrans.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.ORANGE;

        if (start.z == float.NegativeInfinity)
        {
            start = Vector3.forward * -Length / 2;
            leftTrans.gameObject.SetActive(false);
        }

        if (end.z > Length / 2)
        {
            end = Vector3.forward * Length / 2;
            rightTrans.gameObject.SetActive(false);
        }


        start.y = yPos;
        end.y = yPos;

        float diff = (end - start).magnitude;

        Vector3 center = start + (end - start) / 2;

        spanObject.GetComponentInChildren<TextMeshPro>().transform.localPosition = center + Vector3.up * 0.156f;
        spanObject.GetComponentInChildren<TextMeshPro>().text = yearStart + " - " + yearEnd;

        center.y = yPos;

        centerTrans.localPosition = center;
        Vector3 scale = centerTrans.localScale;
        scale.z = diff;
        centerTrans.localScale = scale;

        leftTrans.localPosition = start;
        rightTrans.localPosition = end;
    }
}
