using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VivoxUnity;
using System.ComponentModel;

public class VivoxVoiceManager : MonoBehaviour
{
    #region Enums

    /// <summary>
    /// Defines properties that can change.  Used by the functions that subscribe to the OnAfterTYPEValueUpdated functions.
    /// </summary>
    public enum ChangedProperty
    {
        None,
        Speaking,
        Typing,
        Muted
    }

    public enum ChatCapability
    {
        TextOnly,
        AudioOnly,
        TextAndAudio
    };

    public enum MatchStatus
    {
        Open,
        Closed
    }

    #endregion

    #region Delegates/Events

    public delegate void ParticipantValueChangedHandler(string username, ChannelId channel, bool value);
    public event ParticipantValueChangedHandler OnSpeechDetectedEvent;
    //public event ParticipantValueChangedHandler OnAudioEnergyChangedEvent;


    public delegate void ParticipantStatusChangedHandler(string username, ChannelId channel, IParticipant participant);
    public event ParticipantStatusChangedHandler OnParticipantAddedEvent;
    public event ParticipantStatusChangedHandler OnParticipantRemovedEvent;

    public delegate void ChannelTextMessageChangedHandler(string sender, IChannelTextMessage channelTextMessage);
    public event ChannelTextMessageChangedHandler OnTextMessageLogReceivedEvent;

    public delegate void SessionArchiveMessageChangedHandler(string sender, ISessionArchiveMessage channelTextMessage);
    public event SessionArchiveMessageChangedHandler OnSessionArchiveMessageReceivedEvent;

    public delegate void LoginStatusChangedHandler();
    public event LoginStatusChangedHandler OnUserLoggedInEvent;
    public event LoginStatusChangedHandler OnUserLoggedOutEvent;

    #endregion

    #region Member Variables
    private Uri _serverUri
    {
        get => new Uri(_server);

        set
        {
            _server = value.ToString();
        }
    }
    private string _server;
    private string _domain;
    private string _tokenIssuer;
    private string _tokenKey;
    private TimeSpan _tokenExpiration = TimeSpan.FromSeconds(90);

    private Client _client = new Client();
    private AccountId _accountId;
    private static VivoxVoiceManager _instance;
    private static object _lock = new object();
    public static VivoxVoiceManager Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (VivoxVoiceManager)FindObjectOfType(typeof(VivoxVoiceManager));
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(VivoxVoiceManager).ToString());
                        _instance = go.AddComponent<VivoxVoiceManager>();
                    }
                }
            }
            DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }

    public LoginState LoginState { get; private set; }
    public ILoginSession LoginSession;
    public VivoxUnity.IReadOnlyDictionary<ChannelId, IChannelSession> ActiveChannels => LoginSession?.ChannelSessions;
    public IAudioDevices AudioInputDevices => _client.AudioInputDevices;
    public IAudioDevices AudioOutputDevices => _client.AudioOutputDevices;

    #endregion

    #region Properties

    /// <summary>
    /// Retrieves the first instance of a session that is transmitting. 
    /// </summary>
    [Obsolete]
    public IChannelSession TransmittingSession
    {
        get
        {
            if (_client == null)
                throw new NullReferenceException("client");

            return _client.GetLoginSession(_accountId).ChannelSessions.FirstOrDefault(x => x.IsTransmitting);
        }
        set
        {
            if (value != null)
            {
                value.IsTransmittingSession = true;
            }
        }
    }
    #endregion

    private void Start()
    {
        // TODO build it as option into the network window
        // TODO create inputfields for server, domain, issuer and key
        // TODO cache login data
        _server = "https://vdx5.www.vivox.com/api2";
        _domain = "vdx5.vivox.com";
        _tokenIssuer = "attila4351-vr60-dev";
        _tokenKey = "kick472";
        _client.Uninitialize();

        _client.Initialize();
    }

    private void OnApplicationQuit()
    {
        // Needed to add this to prevent some unsuccessful uninit, we can revisit to do better -carlo
        Client.Cleanup();
        if (_client != null)
        {
            VivoxLog("Uninitializing client.");
            _client.Uninitialize();
            _client = null;
        }
    }

    public void Login(string displayName = null)
    {
        string uniqueId = Guid.NewGuid().ToString();
        //for proto purposes only, need to get a real token from server eventually
        _accountId = new AccountId(_tokenIssuer, uniqueId, _domain, displayName);
        LoginSession = _client.GetLoginSession(_accountId);
        LoginSession.PropertyChanged += OnLoginSessionPropertyChanged;
        LoginSession.BeginLogin(_serverUri, LoginSession.GetLoginToken(_tokenKey, _tokenExpiration), SubscriptionMode.Accept, null, null, null, ar =>
        {
            try
            {
                LoginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                // Handle error 
                VivoxLogError(nameof(e));
                // Unbind if we failed to login.
                LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                return;
            }
        });
    }

    public void Logout()
    {
        if (LoginSession != null && LoginState != LoginState.LoggedOut && LoginState != LoginState.LoggingOut)
        {
            OnUserLoggedOutEvent?.Invoke();            
            LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
            LoginSession.Logout();
            LoginState = LoginState.LoggedOut;
            LoginSession = null;
        }
    }

    [Obsolete]
    public void JoinChannel(string channelName, ChannelType channelType, ChatCapability chatCapability,
        TransmitPolicy transmitPolicy = TransmitPolicy.Yes, Channel3DProperties properties = null)
    {
        if (LoginState == LoginState.LoggedIn)
        {

            ChannelId channelId = new ChannelId(_tokenIssuer, channelName, _domain, channelType, properties);
            IChannelSession channelSession = LoginSession.GetChannelSession(channelId);
            channelSession.PropertyChanged += OnChannelPropertyChanged;
            channelSession.Participants.AfterKeyAdded += OnParticipantAdded;
            channelSession.Participants.BeforeKeyRemoved += OnParticipantRemoved;
            channelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
            channelSession.MessageLog.AfterItemAdded += OnMessageLogRecieved;
            channelSession.SessionArchive.AfterItemAdded += OnSessionArchiveAdded;
            channelSession.BeginConnect(chatCapability != ChatCapability.TextOnly, chatCapability != ChatCapability.AudioOnly, transmitPolicy, channelSession.GetConnectToken(_tokenKey, _tokenExpiration), ar =>
            {
                try
                {
                    channelSession.EndConnect(ar);
                }
                catch (Exception e)
                {
                    // Handle error 
                    VivoxLogError($"Could not connect to voice channel: {e.Message}");
                    return;
                }
            });
        }
        else
        {
            VivoxLogError("Cannot join a channel when not logged in.");
        }
    }

    public void SendTextMessage(string messageToSend, ChannelId channel)
    {
        if (string.IsNullOrEmpty(messageToSend))
        {
            VivoxLogError("Enter a valid message to send");
            return;
        }
        var channelSession = LoginSession.GetChannelSession(channel);
        channelSession.BeginSendText(messageToSend, ar =>
        {
            try
            {
                channelSession.EndSendText(ar);
            }
            catch (Exception e)
            {
                VivoxLog($"SendTextMessage failed with exception {e.Message}");
            }
        });
    }

    public void SendMatchStatusMessage(MatchStatus status, string playerIp, ChannelId channelId)
    {
        if (ChannelId.IsNullOrEmpty(channelId))
        {
            throw new ArgumentException(string.Format("{0} is null", channelId));
        }

        if (string.IsNullOrEmpty(playerIp))
        {
            throw new ArgumentException(string.Format("{0} cannot be empty", playerIp));
        }

        SendTextMessage(string.Format($"<{status.ToString()}>{_accountId.DisplayName}:{playerIp}"), channelId);
    }

    public void RunArchiveQueryInChannel(ChannelId channelId, DateTime? timeStart, DateTime? timeEnd,
        string searchText = null, uint max = 50, string afterId = null, string beforeId = null, int firstMessageIndex = -1)
    {
        IChannelSession channelSession = LoginSession.GetChannelSession(channelId);
        var channelName = channelId.Name;
        var senderName = _accountId.Name;

        channelSession.BeginSessionArchiveQuery(timeStart, timeEnd, searchText, _accountId, max, afterId, beforeId, firstMessageIndex, ar =>
        {
            try
            {
                channelSession.EndSessionArchiveQuery(ar);
            }
            catch (Exception e)
            {
                // Handle error 
                VivoxLogError($"Failed to get archive query for channel: {e.Message}");
                return;
            }
            Debug.Log(channelName + ": " + senderName + ": ");

        });
    }

    public void DisconnectAllChannels()
    {
        if (ActiveChannels?.Count > 0)
        {
            foreach (var channelSession in ActiveChannels)
            {
                channelSession?.Disconnect();
            }
        }
    }

    #region Vivox Callbacks

    private void OnMessageLogRecieved(object sender, QueueItemAddedEventArgs<IChannelTextMessage> textMessage)
    {
        ValidateArgs(new object[] { sender, textMessage });

        IChannelTextMessage channelTextMessage = textMessage.Value;
        VivoxLog(channelTextMessage.Message);
        OnTextMessageLogReceivedEvent?.Invoke(channelTextMessage.Sender.DisplayName, channelTextMessage);
    }


    private void OnSessionArchiveAdded(object sender, QueueItemAddedEventArgs<ISessionArchiveMessage> archiveMessage)
    {
        Debug.Log("OnSessionArchiveAdded");
        ValidateArgs(new object[] { sender, archiveMessage });

        ISessionArchiveMessage sessionArchiveMessage = archiveMessage.Value;
        OnSessionArchiveMessageReceivedEvent?.Invoke(sessionArchiveMessage.Sender.DisplayName, sessionArchiveMessage);
    }


    private void OnLoginSessionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (propertyChangedEventArgs.PropertyName != "State")
        {
            return;
        }
        var loginSession = (ILoginSession)sender;
        LoginState = loginSession.State;
        VivoxLog("Detecting login session change");
        switch (LoginState)
        {
            case LoginState.LoggingIn:
                {
                    VivoxLog("Logging in");
                    break;
                }
            case LoginState.LoggedIn:
                {
                    VivoxLog("Connected to voice server and logged in.");
                    OnUserLoggedInEvent?.Invoke();
                    break;
                }
            case LoginState.LoggingOut:
                {
                    VivoxLog("Logging out");
                    break;
                }
            case LoginState.LoggedOut:
                {
                    VivoxLog("Logged out");
                    LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
                    break;
                }
            default:
                break;
        }
    }

    private void OnParticipantAdded(object sender, KeyEventArg<string> keyEventArg)
    {
        ValidateArgs(new object[] { sender, keyEventArg });

        // INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
        // Look up the participant via the key.
        var participant = source[keyEventArg.Key];
        var username = participant.Account.Name;
        var channel = participant.ParentChannelSession.Key;
        var channelSession = participant.ParentChannelSession;

        // Trigger callback
        OnParticipantAddedEvent?.Invoke(username, channel, participant);
    }

    private void OnParticipantRemoved(object sender, KeyEventArg<string> keyEventArg)
    {
        ValidateArgs(new object[] { sender, keyEventArg });

        // INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
        // Look up the participant via the key.
        var participant = source[keyEventArg.Key];
        var username = participant.Account.Name;
        var channel = participant.ParentChannelSession.Key;
        var channelSession = participant.ParentChannelSession;

        if (participant.IsSelf)
        {
            VivoxLog($"Unsubscribing from: {channelSession.Key.Name}");
            // Now that we are disconnected, unsubscribe.
            channelSession.PropertyChanged -= OnChannelPropertyChanged;
            channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
            channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
            channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
            channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;
            channelSession.SessionArchive.AfterItemAdded -= OnSessionArchiveAdded;

            // Remove session.
            var user = _client.GetLoginSession(_accountId);
            user.DeleteChannelSession(channelSession.Channel);
        }

        // Trigger callback
        OnParticipantRemovedEvent?.Invoke(username, channel, participant);
    }

    private static void ValidateArgs(object[] objs)
    {
        foreach (var obj in objs)
        {
            if (obj == null)
                throw new ArgumentNullException(obj.GetType().ToString(), "Specify a non-null/non-empty argument.");
        }
    }

    private void OnParticipantValueUpdated(object sender, ValueEventArg<string, IParticipant> valueEventArg)
    {
        ValidateArgs(new object[] { sender, valueEventArg });

        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
        // Look up the participant via the key.
        var participant = source[valueEventArg.Key];

        string username = valueEventArg.Value.Account.Name;
        ChannelId channel = valueEventArg.Value.ParentChannelSession.Key;
        string property = valueEventArg.PropertyName;

        switch (property)
        {
            case "SpeechDetected":
                {
                    VivoxLog($"OnSpeechDetectedEvent: {username} in {channel}.");
                    OnSpeechDetectedEvent?.Invoke(username, channel, valueEventArg.Value.SpeechDetected);
                    break;
                }
            case "AudioEnergy":
                {
                    break;
                }
            default:
                break;
        }
    }

    private void OnChannelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        ValidateArgs(new object[] { sender, propertyChangedEventArgs });

        //if (_client == null)
        //    throw new InvalidClient("Invalid client.");
        var channelSession = (IChannelSession)sender;

        // IF the channel has removed audio, make sure all the VAD indicators aren't showing speaking.
        if (propertyChangedEventArgs.PropertyName == "AudioState" && channelSession.AudioState == ConnectionState.Disconnected)
        {
            VivoxLog($"Audio disconnected from: {channelSession.Key.Name}");

            foreach (var participant in channelSession.Participants)
            {
                OnSpeechDetectedEvent?.Invoke(participant.Account.Name, channelSession.Channel, false);
            }
        }

        // IF the channel has fully disconnected, unsubscribe and remove.
        if ((propertyChangedEventArgs.PropertyName == "AudioState" || propertyChangedEventArgs.PropertyName == "TextState") &&
            channelSession.AudioState == ConnectionState.Disconnected &&
            channelSession.TextState == ConnectionState.Disconnected)
        {
            VivoxLog($"Unsubscribing from: {channelSession.Key.Name}");
            // Now that we are disconnected, unsubscribe.
            channelSession.PropertyChanged -= OnChannelPropertyChanged;
            channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
            channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
            channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
            channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;
            channelSession.SessionArchive.AfterItemAdded -= OnSessionArchiveAdded;

            // Remove session.
            var user = _client.GetLoginSession(_accountId);
            user.DeleteChannelSession(channelSession.Channel);

        }


        if (propertyChangedEventArgs.PropertyName == "SessionArchiveResult")
        {
            // TODO Let's flesh out the expected behaviour when this occurs. Adding this as a placeholder for now.
            Debug.Log("Session archive result change detected!");
        }
    }

    #endregion

    private void VivoxLog(string msg)
    {
        Debug.Log("<color=green>VivoxVoice: </color>: " + msg);
    }

    private void VivoxLogError(string msg)
    {
        Debug.LogError("<color=green>VivoxVoice: </color>: " + msg);
    }
}
