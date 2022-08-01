using System.Collections;
using System.Collections.Generic;

//TODO: Export messageTypeDict to json
public static class Logger
{
    //Message Types:
    public const string LogString = "LOG";
    public const string WarningString = "WARNING";
    public const string ErrorString = "ERROR";

    //Sender Contexts:
    public const string NoSenderText = "No Sender";

    private static readonly Dictionary<string, Dictionary<string, List<string>>> messageTypeDict = new Dictionary<string, Dictionary<string, List<string>>>();

    public delegate void MessageDelegate(string message, string messageType, string senderContext);
    public static event MessageDelegate OnNewMessageLogged;


    public static void AppendMessage(string message, string messageType, string senderContext)
    {
        if (!messageTypeDict.ContainsKey(messageType))
            messageTypeDict[messageType] = new Dictionary<string, List<string>>();
        
        var messageDict = messageTypeDict[messageType];
        if(!messageDict.ContainsKey(senderContext))
            messageDict[senderContext] = new List<string>();
        
        messageDict[senderContext].Add(message);
        OnNewMessageLogged?.Invoke(message, messageType, senderContext);
    }

    public static void Log(string message)
    {
        AppendMessage(message, LogString, NoSenderText);
    }
    
    public static void LogWithContext(string message, string senderContext)
    {
        AppendMessage(message, LogString, senderContext);
    }

    public static void LogWarning(string message)
    {
        AppendMessage(message, WarningString, NoSenderText);
    }

    public static void LogWarningWithContext(string message, string senderContext)
    {
        AppendMessage(message, WarningString, senderContext);
    }

    public static void LogError(string message)
    {
        AppendMessage(message, ErrorString, NoSenderText);
    }

    public static void LogErrorWithContext(string message, string senderContext)
    {
        AppendMessage(message, ErrorString, senderContext);
    }
}
