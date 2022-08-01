using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.SceneManagement;


public class WordRecognizer : MonoBehaviour {

    public string[] keywords = new string[] { "weiter" , "annotation", "aufnehmen", "karte", "menü", "schließen", "teleportieren", "test", "zeichnen ein", "zeichnen aus",
                                              "zeichne", "zeichnen", "zeichnen beenden", "kreisförmig", "kreis", "quadrat", "frei", "form", "objekt", "zeigend", "finger",
                                              "mit finger", "mit dem finger", "linie", "rechteck"};
    public ConfidenceLevel confidence = ConfidenceLevel.High;
    public float speed = 1;

    public GameObject menu;

    protected DictationRecognizer dictationRecognizer;
    //protected KeywordRecognizer keywordRecognizer;
    protected string word = "right";

    public string RecordedText;

    public SaveasWav savetowav;

    private void Start()
    {
        StolperwegeHelper.WordRecognizer = this;
        dictationRecognizer =  new DictationRecognizer(confidence);
        StolperwegeHelper.DictationRecognizer = dictationRecognizer;
        //StartListening();
    }

    IEnumerator aufnehmen()
    {
        AudioSource aud = gameObject.AddComponent<AudioSource>();
        aud.clip = Microphone.Start(Microphone.devices[0], true, 20, 44100);
        yield return new WaitForSeconds(25);
        savetowav.Save("annotation", aud.clip);
        
        Microphone.End(Microphone.devices[0]);
        Destroy(aud);

    }

    public void StartDictation()
    {
        try
        {
            dictationRecognizer.Start();
            dictationRecognizer.DictationResult -= Listener;
            dictationRecognizer.DictationResult += TextRecorder;
        } catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }

    public void StartListening()
    {
        try
        {
            dictationRecognizer.Start();
            dictationRecognizer.DictationResult -= TextRecorder;
            dictationRecognizer.DictationResult += Listener;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void TextRecorder(string text, ConfidenceLevel level)
    {
        StartCoroutine(StolperwegeHelper.VRWriter.Interface.Keyboard.AutoWriteText(text));
    }

    private void Listener(string text, ConfidenceLevel level)
    {
        text = text.ToLower();
        if  (text.Equals("aufnehmen") && Microphone.devices != null) StartCoroutine("aufnehmen");
        
    }

    private void OnApplicationQuit()
    {
        //if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        //{
        //    keywordRecognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
        //    keywordRecognizer.Stop();
        //}

        //if (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
        //{
        //    dictationRecognizer.Stop();
        //}
    }
}
