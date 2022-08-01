using UnityEngine;
using System.Collections;
using System.IO;

public class EvaluationTimer : InteractiveObject
{

    // never used
    //private Collider[] finger = new Collider[2];

    private float timer;

    private bool running = false;

    private InverseRotation label;

    private static string filename = "";

    public string TimerTag = "";

    public override void Start()
    {
        base.Start();

        label = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeLabel"))).GetComponent<InverseRotation>();
        label.Text = "0";
        label.hover = gameObject;



        if (filename.Length <= 0)
        {

            string id = System.DateTime.Now.Day + "_" + System.DateTime.Now.Month +
                "_" + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute + "_" + System.DateTime.Now.Second;

            filename = Application.persistentDataPath + "\\VREvaluation\\" + id + ".txt";

            File.Create(filename);

            StolperwegeInterface.lastTimer = id;
        }

        Debug.Log("started");
    }

    public override bool OnPointerClick()
    {
        base.OnPointerClick();

        if (running)
            stopTimer();
        else
            startTimer();

        return true;
    }

    private float startedTime;

    private void stopTimer()
    {
        if ((Time.time - startedTime) < 1) return;
        File.AppendAllText(filename, TimerTag+":"+timer + ";");
        timer = 0;
        label.Text = "" + 0;
        running = false;
    }

    private void startTimer()
    {
        timer = 0;
        running = true;

        startedTime = Time.time;
    }

    public void Update()
    {

        if (running)
        {
            timer += Time.deltaTime;
            label.Text = "" + Mathf.Round(timer);
        }
    }
}
