using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR;

public class MessageBoxScript : MonoBehaviour {

    private const float MAX_WAIT = 0.5f;

    private TextMeshPro TextBox;
    private GameObject Background;
    private GameObject Outliner;
    private readonly Vector3 Hidden = new Vector3(0, 0.5f, 0.3f);
    private readonly Vector3 Shown = new Vector3(0, 0.1f, 0.3f);

    private bool show = false;
    private bool hide = false;
    private bool messageDisplayed = false;
    private bool messageChanged = false;
    private bool infoDisplayed = false;
    private bool infoMessageSetted = false;
    private float actualTimer = 0;
    private float waitingTimer = 0;
    private bool waitingForNewMsg = false;
    private Vector3 actualLocalPos;
    private float lerp = 0;
    private float lerpMultiplier = 3;
    private float InfoMessageTimer = 0;
    private string currentSelection = null;

    private List<Message> MessageQueue = new List<Message>();
    private Message InfoMessage;

    public Message ActualMessage;

    public enum PriorityType { Normal, High, Immediate, None }

    public enum MessageType { INFO, SELECTION, NONE}

    public GameObject VRFile;

    public struct Message
    {

        public string Text;
        public List<string> Selection;
        public int Index;
        public float DisplayTime { get; private set; }
        public MessageType MsgType { get; private set; }
        public PriorityType Priority { get; private set; }
        public OnSelect OnSelection;

        public delegate void OnSelect(int index);
        public Message(string text, List<string> selection, float displayTime, MessageType msgType, PriorityType priority, OnSelect onSelect)
        {
            Text = text;
            Selection = selection;
            Index = 0;
            DisplayTime = displayTime;
            MsgType = msgType;
            Priority = priority;
            OnSelection = onSelect;
        }

        public Message(string text, float displayTime, PriorityType priority)
        {
            Text = text;
            Selection = null;
            Index = 0;
            DisplayTime = displayTime;
            MsgType = MessageType.INFO;
            Priority = priority;
            OnSelection = null;
        }

        public Message(string text)
        {
            Text = text;
            Selection = null;
            Index = 0;
            DisplayTime = 0;
            MsgType = MessageType.NONE;
            Priority = PriorityType.None;
            OnSelection = null;
        }
    }

    // Use this for initialization
    void Start()
    {
        TextBox = transform.GetComponentInChildren<TextMeshPro>();
        actualLocalPos = Hidden;
        gameObject.transform.localPosition = Hidden;
        Background = transform.Find("Background").gameObject;
        Outliner = transform.Find("Outliner").gameObject;
        SetComponentStatus(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (InfoMessage.Text == null && !messageDisplayed &&
            MessageQueue.Count > 0 && actualTimer == 0)
        {
            Message nextMsg = MessageQueue[0];
            ActualMessage = nextMsg;
            TextBox.text = nextMsg.Text;
            //SetBackgroundSize();
            actualTimer = nextMsg.DisplayTime;
            SetComponentStatus(true);
            show = true;            
        }

        if (InfoMessage.Text != null && !infoDisplayed)
        {
            ActualMessage = InfoMessage;
            TextBox.text = InfoMessage.Text;
            //SetBackgroundSize();
            SetComponentStatus(true);
            show = true;
        }

        if (show) ShowMessageBox();

        if (InfoMessage.Text == null && messageDisplayed)
        {
            actualTimer -= Time.deltaTime;
            if (actualTimer <= 0) hide = true;
        }

        if (hide) HideMessageBox();

        if (waitingForNewMsg)
        {
            waitingTimer += Time.deltaTime;
            if (messageChanged)
            {
                waitingForNewMsg = false;
                ChangeMessageBoxText();
                waitingTimer = 0;
                messageChanged = false;
            }
            if (waitingTimer >= MAX_WAIT)
            {
                waitingForNewMsg = false;
                hide = true;
                waitingTimer = 0;
            }
        }

        if (InfoMessage.Text == "")
        {
            waitingForNewMsg = false;
            hide = true;
            waitingTimer = 0;
        }

        if (ActualMessage.MsgType == MessageType.SELECTION)
        {
            if(SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                hide = true;
                ActualMessage.OnSelection(ActualMessage.Index);
            }
            string combinedSelection = "";
            for (int i = 0; i < ActualMessage.Selection.Count; i++)
            {
                combinedSelection += (i == ActualMessage.Index) ? "<b>" + ActualMessage.Selection[i] + "</b>" + ", " : ActualMessage.Selection[i] + ", ";
            }
            combinedSelection = combinedSelection.Remove(combinedSelection.Length - 2);
            TextBox.text =  ActualMessage.Text + "\n<align=center>" + combinedSelection + "</align=center>";

            if (ActualMessage.Index < 0) ActualMessage.Index = 0;
            else if (ActualMessage.Index >= ActualMessage.Selection.Count) ActualMessage.Index = ActualMessage.Selection.Count - 1;
        }
    }


    public void AddMessage(Message message)
    {
        //switch (message.Priority)
        //{
        //    case PriorityType.Immediate:
        //        int insertAt = messageDisplayed ? 1 : 0;
        //        MessageQueue.Insert(insertAt, message);
        //        break;
        //    case PriorityType.High:
        //        if (MessageQueue.Count == 0)
        //        {
        //            MessageQueue.Add(message);
        //        }
        //        else
        //        {
        //            for (int i = 0; i < MessageQueue.Count; i++)
        //            {
        //                if (MessageQueue[i].Priority == PriorityType.Normal)
        //                {
        //                    MessageQueue.Insert(i, message);
        //                }
        //            }
        //        }
        //        break;
        //    case PriorityType.Normal:
        //        MessageQueue.Add(message);
        //        break;
        //    default:
        //        InfoMessage = message;
        //        if (messageDisplayed || infoDisplayed) ChangeMessageBoxText();
        //        InfoMessageTimer = 0;
        //        break;
        //}
    }

    public void RemoveActualInfoMessage()
    {
        if (InfoMessage.Text == null) return;
        waitingForNewMsg = true;
    }

    private void ChangeMessageBoxText()
    {
        ActualMessage = InfoMessage;
        TextBox.text = InfoMessage.Text;
        messageChanged = true;
        //SetBackgroundSize();
    }

    public void ShowMessageBox()
    {
        lerp += Time.deltaTime * lerpMultiplier;
        gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, Shown, lerp);
        if (gameObject.transform.localPosition == Shown)
        {
            infoDisplayed = InfoMessage.Text != null;
            messageDisplayed = InfoMessage.Text == null;
            show = false;
            lerp = 0;
        }
    }

    public void HideMessageBox()
    {
        lerp += Time.deltaTime * lerpMultiplier;
        gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, Hidden, lerp);
        if (gameObject.transform.localPosition == Hidden)
        {
            SetComponentStatus(false);
            infoDisplayed = false;
            messageDisplayed = false;
            hide = false;
            actualTimer = 0;
            if (!infoMessageSetted)
            {
                if (InfoMessage.Text != null) InfoMessage = new Message();
                else if (MessageQueue.Count > 0) MessageQueue.RemoveAt(0);
            }
            infoMessageSetted = false;
            lerp = 0;
        }
    }

    private void SetBackgroundSize()
    {
        if (TextBox.text == null) return;
        string[] rows = TextBox.text.Split('\n');
        int maxRowLength = 0;
        int maxRows = 0;
        foreach (string row in rows)
        {
            if (row.Length > maxRowLength) maxRowLength = row.Length;
            maxRows += 1;
        }
        float xSize = (maxRowLength / 26f) * 2 + 0.02f;
        float zSize = maxRows * 0.14f + 0.04f;
        Background.transform.localScale = new Vector3(xSize, 1, zSize);
    }

    private void SetComponentStatus(bool status)
    {
        Background.SetActive(status);
        Outliner.SetActive(status);
        TextBox.enabled = status;
    }

}
