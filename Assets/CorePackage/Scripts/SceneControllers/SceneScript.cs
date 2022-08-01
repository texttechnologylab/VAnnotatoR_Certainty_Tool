using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public abstract class SceneScript : MonoBehaviour {
    
    protected bool unreadedMessage = false;
    protected string speechInput = "";


    public abstract void Initialize();

    public void newMessageReceived()
    {
        unreadedMessage = true;
    }

    public void userSpeechInput(string text)
    {
        speechInput = text;
    }  
    

}
