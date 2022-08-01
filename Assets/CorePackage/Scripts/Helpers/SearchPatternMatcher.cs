using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Text.RegularExpressions;

public class SearchPatternMatcher
{

    public string Text { get; private set; }
    public string Pattern { get; private set; }
    public List<int> Matches { get; private set; }
    public bool MatchesDetermined { get; private set; }


    public SearchPatternMatcher(string text, string pattern)
    {
        Text = text;
        Pattern = pattern;
        Thread t = new Thread(() => DetermineMatches());
        t.Start();
    }

    
    private void DetermineMatches()
    {
        Matches = new List<int>();
        Regex rgx = new Regex(Pattern);
        MatchCollection mc = rgx.Matches(Text);
        foreach (Match match in mc)
            Matches.Add(match.Index);
        MatchesDetermined = true;
    }
}
