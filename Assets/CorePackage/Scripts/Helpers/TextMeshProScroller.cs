using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Unity.Jobs;

public class TextMeshProScroller : MonoBehaviour
{

    public bool Active = true;
    public int MaxSites { get { return Sites.Count; } }
    public bool IsAtEnd;
    public bool IsAtStart;
    public TextMeshPro TextContent { get; private set; }

    private Site ActualSite { get { return Sites[ActualSiteIndex]; } }
    private int _actualSiteIndex;
    public bool SitesDetermined = false;
    public int ActualSiteIndex
    {
        get { return _actualSiteIndex; }
        set
        {
            
            if (value < 0 || value == Sites.Count) return;
            _actualSiteIndex = value;                

            TextContent.text = Sites[_actualSiteIndex].Text;
            TextContent.ForceMeshUpdate();
            IsAtStart = _actualSiteIndex == 0;
            IsAtEnd = _actualSiteIndex == Sites.Count - 1;
            if (PatternMatcher != null) MarkPatternMatches();
            
        }
    }

    List<Site> Sites;
    Dictionary<int, List<Site>> TextMap;
    private string _text; MonoBehaviour mb;
    public string Text
    {
        get { return _text; }
        set
        {
            _text = value;
            if (_text == "") return;
            DetermineSites();
        }
    }

    public SearchPatternMatcher PatternMatcher;
    public TextMeshProMarker Marker;

    public class Site
    {
        public string Text { get; private set; }
        public int Index { get; private set; }        
        
        public Site(string text, int index)
        {
            Text = text;
            Index = index;
        }
    }


    public void Init(TextMeshPro textContent)
    {
        TextContent = textContent;
        TextContent.overflowMode = TextOverflowModes.Truncate;
        TextContent.alignment = TextAlignmentOptions.TopLeft;
        Sites = new List<Site>();
    }

    public void ScrollText(int dir)
    {
        ActualSiteIndex += dir;
    }

    public void JumpToEnd()
    {
        ActualSiteIndex = Sites.Count - 1;
    }

    public void JumpToBegin()
    {
        ActualSiteIndex = 0;
    }

    
    private void DetermineSites()
    {
        if (TextMap == null) TextMap = new Dictionary<int, List<Site>>();
        if (!TextMap.ContainsKey(_text.GetHashCode()))
        {
            TextMap.Add(_text.GetHashCode(), new List<Site>());
            Sites = TextMap[_text.GetHashCode()];
            SitesDetermined = false;
            while (!SitesDetermined)
                GetNextSite();
        }
        else
            Sites = TextMap[_text.GetHashCode()];

        ActualSiteIndex = 0;

    }

    private int actualIndex = 0; private int lastIndex, relativeIndex; string siteText;
    private void GetNextSite()
    {
        TextContent.text = Text.Substring(actualIndex, Mathf.Min(Text.Length - actualIndex, 1000));
        TextContent.ForceMeshUpdate();
        lastIndex = actualIndex;
        if (TextContent.textInfo.wordCount > 2 && TextContent.textInfo.characterCount + lastIndex < Text.Length)
            actualIndex = lastIndex + TextContent.textInfo.wordInfo[TextContent.textInfo.wordCount - 2].lastCharacterIndex + 1;
        else
            actualIndex = lastIndex + TextContent.textInfo.characterCount;

        siteText = Text.Substring(lastIndex, actualIndex - lastIndex);;
        relativeIndex = lastIndex;
        Sites.Add(new Site(siteText, relativeIndex));
        IsAtEnd = lastIndex + siteText.Length >= Text.Length;        
        SitesDetermined = IsAtEnd;
    }


    List<TextMeshProMarker.MarkerInfo> markers; int start, end;
    public void MarkPatternMatches()
    {

        start = ActualSite.Index;
        end = start + ActualSite.Text.Length;


        markers = new List<TextMeshProMarker.MarkerInfo>();
        for (int i = 0; i < PatternMatcher.Matches.Count; i++)
            if (PatternMatcher.Matches[i] >= start && PatternMatcher.Matches[i] < end)
                markers.Add(new TextMeshProMarker.MarkerInfo(PatternMatcher.Matches[i], PatternMatcher.Pattern.Length));

        if (Marker == null)
        {
            Marker = gameObject.AddComponent<TextMeshProMarker>();
            Marker.Init(TextContent);
        }
            
        Marker.MarkText(markers, new Color32(255, 255, 255, 255), new Color32(255, 128, 0, 255), start);
    }

}
