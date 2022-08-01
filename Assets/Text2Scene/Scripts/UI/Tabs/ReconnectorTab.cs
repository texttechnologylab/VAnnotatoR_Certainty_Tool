using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[PrefabInterface(PrefabPath + "ReconnectorTab")]
public class ReconnectorTab : BuilderTab
{

    private InteractiveButton ReconnectButton;
    private TextMeshPro ReconnectButtonTag;
    public TextMeshPro ServerLog { get; private set; }

    public override void Initialize(SceneBuilder builder)
    {
        base.Initialize(builder);

        Name = "Reconnector";
        ShowOnToolbar = false;

        ReconnectButton = transform.Find("ReconnectButton").GetComponent<InteractiveButton>();
        ReconnectButton.AsyncClick = TryReconnect;
        ReconnectButtonTag = ReconnectButton.GetComponentInChildren<TextMeshPro>();

        ServerLog = transform.Find("ServerLog").GetComponent<TextMeshPro>();

        TextAnnotatorInterface textAnno = SceneController.GetInterface<TextAnnotatorInterface>();
        textAnno.Client.OnOpen += (s, e) => { SceneBuilderSceneScript.WaitingForResponse = false; };
        
        textAnno.Client.OnError += (s, e) => 
        {
            //Debug.Log("============");
            //Debug.Log(s.ToString());
            //Debug.Log(e.Message);
            //Debug.Log(e.Exception);
            //Debug.Log("============");
            if (Active) 
            {
                ServerLog.text = e.Message;
                SceneBuilderSceneScript.WaitingForResponse = false;
            } 
        };
        textAnno.Client.OnClose += (s, e) => 
        {
            if (Builder.GetTab<DocumentTab>().DocumentData != null)
                Builder.GetTab<DocumentTab>().DocumentData = null;
            Active = true;
            ServerLog.text = "Exit code: " + e.Code;
            if (e.Reason != null && e.Reason.Length > 0) ServerLog.text += " - " + e.Reason;
        };
    }

    private IEnumerator TryReconnect()
    {
        ReconnectButton.Active = false;
        SceneBuilderSceneScript.WaitingForResponse = true;
        Builder.GetTab<DocumentTab>().ExamplesLoaded = false;
        StartCoroutine(Builder.LoadingAnimation(ReconnectButtonTag));
        StartCoroutine(SceneController.GetInterface<TextAnnotatorInterface>().StartAuthorization());
        while (SceneBuilderSceneScript.WaitingForResponse)
            yield return null;

        if (SceneController.GetInterface<TextAnnotatorInterface>().Authorized)
        {
            Builder.GetTab<DocumentTab>().Active = true;
        }
        Builder.InterruptLoadingAnimation = true;
        ReconnectButton.Active = true;
    }

    public override void ResetTab()
    {
        
    }
}
