using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerFade : InteractiveObject {

    private IEnumerator enumerator;

    private Material m;

	private IEnumerator StartFading()
    {
        yield return new WaitForSeconds(3);

        Expanded = false;

        m = transform.Find("label").GetComponent<Renderer>().material;

        transform.Find("label").GetComponent<InteractiveObject>().ChangeBlendMode(m, "Fade");

        float alpha = 1;
        Color color = m.color; 
        while(alpha > 0)
        {
            color.a = alpha;
            m.color = color;

            alpha -= 0.05f;

            yield return new WaitForSeconds(0.1f);
        }

        Destroy(gameObject);
    }

    public override bool OnPointerClick()
    {
        base.OnPointerClick();

        if(enumerator != null)
        {

            Expanded = true;
            StopCoroutine(enumerator);

            m = GetComponent<Renderer>().material;
            Color color = m.color;
            color.a = 1;
            m.color = color;

            ChangeBlendMode(m, "Opaque");

            enumerator = null;
        }

        return true;
    }

    public override void OnPointerExit()
    {
        base.OnPointerExit();
        return;
        // code below unreachable, commented out because of project-cleanup
        //enumerator = StartFading();

        //StartCoroutine(enumerator);
    }

    private bool _expanded;
    private bool Expanded
    {
        get
        {
            return _expanded;
        }

        set
        {

            if (value == _expanded) return;

            _expanded = value;

            for(int i=0; i<transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains("Label")) continue;

                transform.GetChild(i).gameObject.SetActive(_expanded);
            }
        }
    }
}
