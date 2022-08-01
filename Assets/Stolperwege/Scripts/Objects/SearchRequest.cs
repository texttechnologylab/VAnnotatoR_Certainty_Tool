using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchRequest : KeyboardEditText {


    public override void Commit()
    {
        StolperwegeInterface.OnPostExcecute excecute = (HashSet<StolperwegeElement> result) =>
        {

            if (result.Count == 0)
            {
                Text = "Keine Ergebnisse";
                return;
            }
                

            IEnumerator enumerator = result.GetEnumerator();
            for (int i = 0; i < Mathf.CeilToInt(result.Count / 8f); i++)
            {
                for (int j = -Mathf.Min(8, result.Count) / 2; j < Mathf.Min(8, result.Count) / 2; j++)
                {
                    if (!enumerator.MoveNext()) goto endofloop;

                    StolperwegeObject obj = ((StolperwegeElement)enumerator.Current).StolperwegeObject;
                    if (obj == null) continue;
                    obj.gameObject.SetActive(true);
                    obj.transform.position = transform.position;
                    obj.transform.parent = transform;
                    obj.transform.localEulerAngles = new Vector3(90, 180, 0);
                    obj.transform.localPosition = new Vector3(j * 1.75f, 0, 2f+i*3f);
                    obj.RelationsVisible = false;
                }
            }



            endofloop: { };
        };



        StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().SearchReferents(StolperwegeInterface.DISCOURSEREFERENTS, Text, "StolperwegeElement", true, excecute));

        Text = "";
    }

    public override string Text
    {
        get
        {
            return base.Text;
        }

        set
        {
            if (Text.Equals("") && !value.Equals(""))
                ResetSearch();

            base.Text = value;
        }
    }

    private void ResetSearch()
    {
        foreach (StolperwegeObject obj in GetComponentsInChildren<StolperwegeObject>())
            obj.ResetObject();
    }

    public override void Deselect()
    {
        base.Deselect();

        ResetSearch();
    }
}
