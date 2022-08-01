using System.Collections.Generic;
using Text2Scene.NeuralNetwork;
using TMPro;

namespace Text2Scene
{
    /// <summary>
    /// Configurates the TextObjectBuilder with its functionalities
    /// </summary>
    public class TextObjectBuilderWindow : AnimatedWindow
    {
        private InteractiveButton Create;
        private InteractiveButton Reset;
        private InteractiveButton CloseWindow;

        private KeyboardEditText TextInput;
        private KeyboardEditText TitleInput;
        private TextMeshPro StatusDisplay;
        private List<Interface> Interfaces;
        public DocumentTab DocumentTab { get; set; }

        public int InterfaceCount { get { return Interfaces.Count; } }

        private int _interfacePointer;
        public int InterfacePointer
        {
            get { return _interfacePointer; }
            set
            {
                _interfacePointer = (value < 0) ? Interfaces.Count - 1 : value % Interfaces.Count;
                TextInput.Text = "";
                TitleInput.Text = "";
                StatusDisplay.text = "";
            }
        }
        private TextAnnotatorInterface TextAnnotator
        {
            get { return SceneController.GetInterface<TextAnnotatorInterface>(); }
        }

        private bool _initialized;

        public override void Start()
        {
            base.Start();
            Init();
            Active = true;
        }

        private void BaseInit()
        {
            Interfaces = new List<Interface>();
            foreach (Interface iFace in SceneController.Interfaces)
                if (iFace.OnLogin != null) Interfaces.Add(iFace);

            Create = transform.Find("CreateObject").GetComponent<InteractiveButton>();
            Create.OnClick = () =>
            {
                NN_Helper.TextInputInteractionInScene.textInteraction.getXmi(TextInput.Text, TextAnnotator, TitleInput.Text);
                StatusDisplay.text = "Object created and it's now available in the SceneBuilder";
                TextInput.Text = "";
                TitleInput.Text = "";
            };

            Reset = transform.Find("Reset").GetComponent<InteractiveButton>();
            Reset.OnClick = () =>
            {
                TextInput.Text = "";
                TitleInput.Text = "";
                StatusDisplay.text = "";
            };

            CloseWindow = transform.Find("CloseWindow").GetComponent<InteractiveButton>();
            CloseWindow.OnClick = () =>
            {
                DocumentTab.toWindow = null;
                gameObject.Destroy();
            };

            TextInput = transform.Find("TextInput").GetComponent<KeyboardEditText>();
            TextInput.Text = TextInput.Description;
            TextInput.OnClick = () =>
            {
                StatusDisplay.text = "";
                TextInput.ActivateWriter();
            };


            TitleInput = transform.Find("TitleInput").GetComponent<KeyboardEditText>();
            TitleInput.Text = TitleInput.Description;
            TitleInput.OnClick = () =>
            {
                StatusDisplay.text = "";
                TitleInput.ActivateWriter();
            };

            StatusDisplay = transform.Find("StatusDisplay").GetComponent<TextMeshPro>();

            DestroyOnObjectRemover = false;
            OnRemove = () => { Active = false; };

            InterfacePointer = 0;
            _active = true;
            Active = false;

            _initialized = true;
        }

        public void Init(Interface iFace)
        {
            if (!_initialized) BaseInit();
            for (int i = 0; i < Interfaces.Count; i++)
            {
                if (Interfaces[i].GetType().Equals(iFace.GetType()))
                {
                    InterfacePointer = i;
                    return;
                }
            }
        }

        public void Init()
        {
            if (!_initialized) BaseInit();
            InterfacePointer = 0;
        }
        public override void Update()
        {
            base.Update();
            if (TextInput.Text.Length > 0 && TitleInput.Text.Length > 0) Create.Active = true;
            else Create.Active = false;
            if (TextInput.Text.Length == 0 && TitleInput.Text.Length == 0) Reset.Active = false;
            else Reset.Active = true;
        }
    }
}