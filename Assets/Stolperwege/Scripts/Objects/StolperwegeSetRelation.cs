using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolperwegeSetRelation : MonoBehaviour
{
    private static HashSet<StolperwegeSet> initiated = new HashSet<StolperwegeSet>();

    private static Color[] relationColors = { StolperwegeHelper.GUCOLOR.EMOROT, StolperwegeHelper.GUCOLOR.ORANGE, StolperwegeHelper.GUCOLOR.GRUEN, StolperwegeHelper.GUCOLOR.PURPLE, StolperwegeHelper.GUCOLOR.LICHTBLAU, StolperwegeHelper.GUCOLOR.SENFGELB };

    public static StolperwegeRelationExtended Init(StolperwegeSet set)
    {
        if (initiated.Contains(set)) return null;

        initiated.Add(set);

        List<HashSet<StolperwegeElement>> ends = new List<HashSet<StolperwegeElement>>();
        HashSet<StolperwegeElement> endsSet = new HashSet<StolperwegeElement>();

        foreach(StolperwegeElement stElement in set.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation)){
            endsSet.Add(stElement);
        }

        ends.Add(endsSet);

        Color c = relationColors[Random.Range(0, relationColors.Length - 1)];

        StolperwegeRelationExtended relation = StolperwegeRelationExtended.CreateRelation(null,ends,StolperwegeRelationExtended.NominalExpressionType.CN_DEFINITE,false, c);

        GameObject setObj = set.Draw();
        setObj.GetComponent<StolperwegeObject>().RelationsVisible = false;

        relation.attachObject(setObj);

        return relation;
    }
}
