using System;
using System.Linq;
using Text2Scene.NeuralNetwork;
using UnityEngine;

namespace Text2Scene
{
    /// <summary>
    /// Main interface, which handles the whole pipeline
    /// </summary>
    public class Text2SceneInterface : MonoBehaviour
    {
        public bool ObjectInserted;
        public string Title { get; private set; }
        public bool UseAutomatic { get; private set; }
        public bool objectsPlaced { get; set; }
        public TextInteraction textInteraction { get; private set; }
        private bool startedNeuralNetwork = false;
        private bool NNOutputReady = false;
        private bool disambiguationNotSet = true;
        private ObjectPlacer objectPlacer;
        private PartNetAgent partNetAgent;
        private DisamAgent disamAgent;
        public InteractiveButton Text2SceneButton = null;
        public bool Evaluation = false;

        private bool NNReady = false;

        //Testing
        private string pathName = "";

        private void Start()
        {
            StartCoroutine(NN_Helper.SetUp());
            textInteraction = GetComponent<TextInteraction>();
            objectPlacer = GetComponent<ObjectPlacer>();
            UseAutomatic = false;
            objectsPlaced = false;
            partNetAgent = transform.Find("PartNetNetwork").GetComponent<PartNetAgent>();
            disamAgent = transform.Find("DisamNetwork").GetComponent<DisamAgent>();
            partNetAgent.gameObject.SetActive(false);
            disamAgent.gameObject.SetActive(false);
        }

        /// <summary>
        /// Checks if a document is open
        /// </summary>
        public void checkDocuments()
        {
            try
            {
                AnnotationDocument anDoc = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().Document;
                if (anDoc != null)
                {
                    if (NN_Helper.isoSpatialEntities.Count == 0) InitButton(NN_Helper.Document.TextContent, NN_Helper.Document.Name, false);
                    else InitWithDocument(anDoc);
                    textInteraction.outputReady = true;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("DocumentTab not found\n" + e);
            }
        }

        private void OnEnable()
        {
            NN_Helper.TextInputInteractionInScene = this;
        }

        private void OnDisable()
        {
            NN_Helper.TextInputInteractionInScene = null;
        }

        void FixedUpdate()
        {
            if (!NN_Helper.Word2VecInitialized) return;
            if (Text2SceneButton != null) UseAutomatic = !(bool)Text2SceneButton.ButtonValue;
            NNOutputReady = DisamAgent.ExecutionFinished && PartNetAgent.ExecutionFinished && startedNeuralNetwork;
            if (textInteraction.outputReady)
            {
                partNetAgent.gameObject.SetActive(true);
                disamAgent.gameObject.SetActive(true);
                DisamAgent.Use = true;
                PartNetAgent.Use = true;

                Debug.Log("Starting Neural Network");
                Debug.Log("Counter " + NN_Helper.isoSpatialEntities.Count);
                textInteraction.outputReady = false;
                startedNeuralNetwork = true;
                disambiguationNotSet = true;
                NNReady = false;
            }

            if (NNOutputReady)
            {
                startedNeuralNetwork = false;
                partNetAgent.gameObject.SetActive(false);
                disamAgent.gameObject.SetActive(false);

                if (disambiguationNotSet) SetDisambiguation();
                Debug.Log("Evaluation " + Evaluation);
                if (!Evaluation)
                {
                    bool useAutomatic = true;
                    if (Text2SceneButton != null) useAutomatic = !(bool)Text2SceneButton.ButtonValue;
                    if (useAutomatic)
                    {
                        objectPlacer.LoadModels(0, NN_Helper.isoSpatialEntities.Count);
                        UseAutomatic = true;
                    }
                    NNReady = true;
                }
                else
                {
                    Debug.Log("Finished prediction" + NN_Helper.isoSpatialEntities.Count);
                    //Excel Format:
                    Evaluate.Eval(pathName, textInteraction.text);

                    ObjectInserted = false;
                    Evaluation = false;
                    Debug.Break();
                }
            }
        }

        /// <summary>
        /// sets the disambiguation for all objects
        /// </summary>
        private void SetDisambiguation()
        {
            int highestCount = 0;
            string highestDisambiguation = NN_Helper.isoSpatialEntities.GroupBy(word => word.DisambiguationWord).OrderByDescending(s =>
            {
                if (s.Count() > highestCount) highestCount = s.Count();
                return s.Count();
            }).First().Key;
            if (highestCount > 1)
            {
                NN_Helper.isoSpatialEntities.ForEach(word =>
                {
                    word.DisambiguationWord = highestDisambiguation;
                });
            }
            disambiguationNotSet = false;
        }

        public void InitButton(string text, string path, bool isPath)
        {
            NN_Helper.isoSpatialEntities.Clear();
            ObjectInserted = true;
            if (isPath) pathName = path;
            else Title = path;
            textInteraction.InformationExtraction(text);
        }

        public void InitWithDocument(AnnotationDocument document)
        {
            NN_Helper.Document = document;
            ObjectInserted = true;
            Title = document.Name;
        }


        public void Exit()
        {
            ObjectInserted = false;
            Title = "";
            NN_Helper.isoSpatialEntities.Clear();
            NN_Helper.Document = null;
            objectPlacer.DestroyObjects();
            objectsPlaced = false;
        }

        /// <summary>
        /// Loads models
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool PlaceModels(int start, int end)
        {
            if (NNReady)
            {
                objectPlacer.LoadModels(start, end);
                return true;
            }
            return false;
        }
    }
}