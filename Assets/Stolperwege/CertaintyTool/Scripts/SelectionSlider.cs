using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSlider : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public int SliderMaximum {get; private set;}

    public bool SelectLower { get; private set; }

    private int SliderCurrentLower;
    private int SliderCurrentUpper;

    public GameObject Menu { get; private set; }
    public GameObject Slider { get; private set; }
    public InteractiveButton[] ButtonList {get; private set;}

    public InteractiveButton ButtonUpper { get; private set; }
    public InteractiveButton ButtonLower { get; private set; }

    public GameObject TagUpper { get; private set; }
    public GameObject TagLower { get; private set; }

    private int[] numList;

    public void Init(CertaintyToolInterface CTInterface)
    {
        Interface = CTInterface;
        Menu = transform.gameObject;
        InitButtons();
    }
    void InitButtons () 
    {
        Slider = Menu.transform.Find("Slider").gameObject;
        GameObject TopRow = Menu.transform.Find("TopRow").gameObject;
        ButtonList = Slider.GetComponentsInChildren<InteractiveButton>();
        SliderMaximum = ButtonList.Length - 1;

        SliderCurrentLower = 0;
        SliderCurrentUpper = SliderMaximum;

        SelectLower = true;

        ButtonLower = TopRow.transform.Find("ButtonLower").gameObject.GetComponent<InteractiveButton>();
        ButtonUpper = TopRow.transform.Find("ButtonUpper").gameObject.GetComponent<InteractiveButton>();

        TagUpper = Menu.transform.Find("TagUpper").gameObject;
        TagLower = Menu.transform.Find("TagLower").gameObject;

        ButtonLower.ButtonOn = true;
        ButtonUpper.ButtonOn = false;

        ButtonLower.OnClick = () => 
        { 
            SelectLower = true;
            ButtonLower.ButtonOn = true;
            ButtonUpper.ButtonOn = false;
        };
        ButtonUpper.OnClick = () => 
        { 
            SelectLower = false;
            ButtonLower.ButtonOn = false;
            ButtonUpper.ButtonOn = true;
        };

        for (int i = 0; i <= SliderMaximum; i++)
        {
            string buttonName = "SliderButton" + i.ToString();
            InteractiveButton button = Slider.transform.Find(buttonName).GetComponent<InteractiveButton>();
            button.ButtonValue = i;
            button.OnClick = () => { SliderButtonPressed((int)button.ButtonValue); };
        }


    }

    void SliderButtonPressed(int i)
    {
        if (i < SliderCurrentLower)
        {
            Debug.Log("Lower: Old: " + SliderCurrentLower + ", New: " + i);
            int delta = i - SliderCurrentLower;
            SliderCurrentLower = i;
            moveTagLower(delta);
        }
        else if (i > SliderCurrentUpper)
        {
            Debug.Log("Upper: Old: " + SliderCurrentUpper + ", New: " + i);
            int delta = i - SliderCurrentUpper;
            SliderCurrentUpper = i;
            moveTagUpper(delta);
        }
        else if (i >= SliderCurrentLower && i <= SliderCurrentUpper)
        {
            if (SelectLower)
            {
                Debug.Log("Lower: Old: " + SliderCurrentLower + ", New: " + i);
                int delta = i - SliderCurrentLower;
                SliderCurrentLower = i;
                moveTagLower(delta);
            }
            else
            {
                Debug.Log("Upper: Old: " + SliderCurrentUpper + ", New: " + i);
                int delta = i - SliderCurrentUpper;
                SliderCurrentUpper = i;
                moveTagUpper(delta);
            }
        }
        if (Interface.BuildingSelected) Interface.SelectedBuilding.SetRatingBoundaries(SliderCurrentLower, SliderCurrentUpper);
    }

    void moveTagUpper(int delta)
    {
        TagUpper.transform.localPosition += Vector3.left * 0.06f * delta;
    }

    void moveTagLower(int delta)
    {
        TagLower.transform.localPosition += Vector3.left * 0.06f * delta;
    }
}
