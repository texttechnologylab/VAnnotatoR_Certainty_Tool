using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiIconScript : MonoBehaviour {

    private float timeElapsed = 0;
    private float flashSpeed = 0.5f;
    private bool flashOn = false;

    private void Start()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    private void Update()
    {
        if (flashOn) flash();
    }

    private void flash()
    {
        if (timeElapsed >= 1) timeElapsed = 0;
        if (timeElapsed < flashSpeed) gameObject.GetComponent<Renderer>().enabled = true;
        if (timeElapsed >= flashSpeed) gameObject.GetComponent<Renderer>().enabled = false;
        timeElapsed += Time.deltaTime;
    }

    public void setFlash(bool value)
    {
        if (flashOn != value)
        {
            flashOn = value;
            if (!flashOn)
            {
                timeElapsed = 0;
                gameObject.GetComponent<Renderer>().enabled = false;
            }
        }

    }

}
