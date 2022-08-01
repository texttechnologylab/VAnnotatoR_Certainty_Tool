using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LegendMenu : MonoBehaviour
{
    private CertaintyToolInterface Interface;
    public int Step { get; private set; }

    public KeyboardEditText Textfield { get; private set; }

    public TextMeshPro DsiplayStep { get; private set; }

    // Start is called before the first frame update
    void Init(CertaintyToolInterface ITInterface, int step)
    {
        Interface = ITInterface;
        Step = step;

        Textfield = transform.Find("Textfield").GetComponent<KeyboardEditText>();
        DsiplayStep = transform.Find("DisplayRating/Tag").GetComponent<TextMeshPro >();

        DsiplayStep.SetText(Step.ToString());
    }
}
