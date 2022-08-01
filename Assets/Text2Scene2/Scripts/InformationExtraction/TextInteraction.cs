using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Text2Scene.NeuralNetwork;
using UnityEngine;

namespace Text2Scene
{
    /// <summary>
    /// Handles all TI related request. Configurates the request
    /// </summary>
    public class TextInteraction : MonoBehaviour
    {
        const string sampleText = "The bed is next to the wall. On the left side of the headboard is a bedside table with a telephone. " +
            "Right next to it is a desk with a lamp and in front of it a chair. On the opposite side of the room is a sofa and a cupboard.";
        const string defaultLanguage = "en";
        List<string> pipeline = new List<string> { "CoreNlpPosTagger" }; //'Tokenizer', Lemmatizer MateLemmatizer, POS, 'Parser', NER CoreNlpNamedEntityRecognizer, Coreference //CoreNlpCoreferenceResolver
        const string outputFormat = "XMI";
        private XNamespace xmiNamespace = "http://www.omg.org/XMI";
        public string text = "";
        public bool outputReady = false;
        public bool DocumentCreated = false;

        private int counter = 1;

        public void InformationExtraction(TextAsset text)
        {
            //DetectLanguage(text.text);
            ProcessText(text.text, pipeline, defaultLanguage, outputFormat);
        }
        public void InformationExtraction(string text)
        {
            //DetectLanguage(text);
            ProcessText(text, pipeline, defaultLanguage, outputFormat);
        }

        private void DetectLanguage(string text)
        {
            StartCoroutine(TICommunicator.TIRequest(TICommunicator.baseURL + TICommunicator.languageURL + "?document=" + text, onLanguageResponse, onLanguageError));
        }

        /// <summary>
        /// collects the data for the TI request
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pipeline"></param>
        /// <param name="language"></param>
        /// <param name="outputFormat"></param>
        private void ProcessText(string input, List<string> pipeline, string language, string outputFormat)
        {
            text = input;
            string tools = "";
            foreach (string tool in pipeline)
            {
                if (pipeline[pipeline.Count - 1].Equals(tool)) tools += tool;
                else tools += tool + ",";
            }

            string url = TICommunicator.baseURL + TICommunicator.processURL + "?document=" + text + "&pipeline=" + tools + "&language=" + language + "&outputFormat=" + outputFormat;
            Debug.Log("URL is " + url);

            //TODO: Abändern des statischen Imports zu einem Import über das Netzwerk
            //StartCoroutine(TICommunicator.TIRequest(url, onProcessComplete, onProcessError));

            //Sobald Bert verfügbar ist, kann der Abschnitt weg
            string t = counter.ToString();
            StreamReader sR = new StreamReader("B:/Vincent/Downloads/KafkaHenlein.xml");
            counter++;
            string result = sR.ReadToEnd();
            Debug.Log("XML read.");
            sR.Close();
            XDocument xmi = XDocument.Parse(result);
            NN_Helper.Document = CreateDocument(xmi, text);
            Debug.Log("Parsing done");
            //

            outputReady = true;
        }

        /// <summary>
        /// sends a request to the TI and saves it in the Resource Mangager
        /// </summary>
        /// <param name="input"></param>
        /// <param name="TextAnnotator"></param>
        /// <param name="Title"></param>
        public void getXmi(string input, TextAnnotatorInterface TextAnnotator, string Title)
        {
            text = input;
            string tools = "";
            foreach (string tool in pipeline)
            {
                if (pipeline[pipeline.Count - 1].Equals(tool)) tools += tool;
                else tools += tool + ",";
            }

            string url = TICommunicator.baseURL + TICommunicator.processURL + "?document=" + text + "&pipeline=" + tools + "&language=" + defaultLanguage + "&outputFormat=" + outputFormat;
            //StartCoroutine(TICommunicator.TIRequest(url, (response) =>
            //{
            //    TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.create_db_cas, "" + response, fileName: Title + ".xmi", parent: "" + DocumentTab.EXAMPLE_REPOSITORY);
            //    ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().LoadExamples();
            //}, onProcessError));

            //Sobald Bert verfügbar ist, kann der Abschnitt weg
            StreamReader sR = new StreamReader("B:/Vincent/Downloads/KafkaHenlein.xml");
            string result = sR.ReadToEnd();
            Debug.Log("XML read.");
            sR.Close();
            TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.create_db_cas, "" + result, fileName: Title, parent: "" + DocumentTab.EXAMPLE_REPOSITORY, text: input);
            //((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().LoadExamples();
            VRResourceData root = new VRResourceData("root", ResourceManagerInterface.ROOT, null, "root", DateTime.MaxValue, DateTime.MaxValue, VRData.SourceType.Remote);
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<DocumentTab>().LoadFolder(root);
            //
        }

        private void onProcessComplete(string response)
        {
            response = response.Replace("[\"", string.Empty).Replace("\"]", string.Empty).Replace("\\n", string.Empty).Replace("\\\"", "\"").Replace("<\\/", "</");
            XDocument xmi = XDocument.Parse(response);
            NN_Helper.Document = CreateDocument(xmi, text);
            Debug.Log("Parsing done");
        }

        private void onProcessError(string error)
        {
            Debug.LogError(error);
        }

        private void onLanguageResponse(string response)
        {
            Debug.Log(response);
            response = response.Replace("[\"", string.Empty).Replace("\"]", string.Empty);
            string parsedLanguage = (response.Equals("x-unspecified")) ? defaultLanguage : response;
            ProcessText(text, pipeline, parsedLanguage, outputFormat);
        }

        private void onLanguageError(string error)
        {
            Debug.LogError(error);
            Debug.Break();
        }

        private int getId(AnnotationDocument Doc)
        {
            System.Random rand = new System.Random();
            int num = rand.Next(1000000, 999999999);
            while (Doc.Text_ID_Map.ContainsKey(num)) num = rand.Next(1000000, 999999999);
            return num;
        }

        /// <summary>
        /// Creates Annnotation Document. Smaller version of the TextAnnotatorDataContainer function, supports only XML
        /// </summary>
        /// <param name="data">The XMI document</param>
        /// <param name="Text">original text</param>
        /// <returns></returns>
        public AnnotationDocument CreateDocument(XDocument data, string Text)
        {
            Debug.Log("Create document");

            int id; int begin; int end;

            XElement doc = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("DocumentAnnotation") || a.Name.LocalName.Contains("DocumentMetaData")).First();
            
            id = int.Parse(doc.Attribute(xmiNamespace + "id").Value);
            begin = int.Parse(doc.Attribute("begin").Value);
            end = int.Parse(doc.Attribute("end").Value);
            List<XElement> objList; AnnotationBase element;

            AnnotationDocument Document = new AnnotationDocument(id, Text);
            /************************************************************************************************************
            get chapters
            ************************************************************************************************************/
            List<Chapter> chapters = new List<Chapter>(); Chapter chapter;
            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Chapter")).ToList();
            if (objList.Count > 0)
            {
                foreach (XElement cId in objList)
                {
                    id = int.Parse(cId.Attribute(xmiNamespace + "id").Value);
                    begin = int.Parse(cId.Attribute("begin").Value);
                    end = int.Parse(cId.Attribute("end").Value);
                    chapter = new Chapter(id, begin, end, Document);
                    chapters.Add(chapter);
                }
            }
            else
            {
                id = getId(Document);
                chapter = new Chapter(id, begin, end, Document);
                chapters.Add(chapter);
            }
            // set&sort chapters
            Document.SetChildElements(chapters);

            /************************************************************************************************************
            get paragraphs
            ************************************************************************************************************/
            Dictionary<Chapter, List<Paragraph>> paragraphs = new Dictionary<Chapter, List<Paragraph>>(); Paragraph paragraph;
            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Paragraph")).ToList();
            if (objList.Count > 0)
            {
                foreach (XElement pID in objList)
                {
                    id = int.Parse(pID.Attribute(xmiNamespace + "id").Value);
                    begin = int.Parse(pID.Attribute("begin").Value);
                    end = int.Parse(pID.Attribute("end").Value);

                    chapter = Document.GetElementOfTypeInRangeGreaterEqual<Chapter>(begin, end);
                    paragraph = new Paragraph(id, begin, end, chapter);
                    if (!paragraphs.ContainsKey(chapter)) paragraphs.Add(chapter, new List<Paragraph>());
                    paragraphs[chapter].Add(paragraph);
                }
            }
            else
            {
                id = getId(Document);
                chapter = Document.GetElementOfTypeInRangeGreaterEqual<Chapter>(begin, end);
                paragraph = new Paragraph(id, begin, end, chapter);
                if (!paragraphs.ContainsKey(chapter)) paragraphs.Add(chapter, new List<Paragraph>());
                paragraphs[chapter].Add(paragraph);
            }

            // set&sort paragraphs in each chapter
            foreach (Chapter c in paragraphs.Keys)
            {
                c.SetChildElements(paragraphs[c]);
                //c.BuildParagraphGroups();
            }


            /************************************************************************************************************
            get sentences
            ************************************************************************************************************/

            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Sentence")).ToList();
            Dictionary<Paragraph, List<Sentence>> sentences = new Dictionary<Paragraph, List<Sentence>>(); Sentence sentence;
            foreach (XElement sID in objList)
            {

                id = int.Parse(sID.Attribute(xmiNamespace + "id").Value);
                begin = int.Parse(sID.Attribute("begin").Value);
                end = int.Parse(sID.Attribute("end").Value);

                paragraph = Document.GetElementOfTypeInRangeGreaterEqual<Paragraph>(begin, end);
                sentence = new Sentence(id, begin, end, paragraph);
                if (!sentences.ContainsKey(paragraph)) sentences.Add(paragraph, new List<Sentence>());
                sentences[paragraph].Add(sentence);
            }

            // set&sort sentences in each paragraph
            foreach (Paragraph p in sentences.Keys)
                p.SetChildElements(sentences[p]);

            /************************************************************************************************************
            get tokens
            ************************************************************************************************************/
            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Token")).ToList();
            Dictionary<Sentence, List<AnnotationToken>> tokens = new Dictionary<Sentence, List<AnnotationToken>>();
            AnnotationToken token;
            foreach (XElement tID in objList)
            {
                id = int.Parse(tID.Attribute(xmiNamespace + "id").Value);
                begin = int.Parse(tID.Attribute("begin").Value);
                end = int.Parse(tID.Attribute("end").Value);

                sentence = Document.GetElementOfTypeInRangeGreaterEqual<Sentence>(begin, end);
                token = new AnnotationToken(id, begin, end, sentence);
                if (!tokens.ContainsKey(sentence)) tokens.Add(sentence, new List<AnnotationToken>());
                tokens[sentence].Add(token);
            }

            // sort sentences in each paragraph
            foreach (Sentence s in tokens.Keys)
                s.SetChildElements(tokens[s]);

            /************************************************************************************************************
           get part-of-speeches
           ************************************************************************************************************/

            PartOfSpeech pos;
            Dictionary<int, string> posType = new Dictionary<int, string>();
            Dictionary<int, int> posID = new Dictionary<int, int>();
            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("POS")).ToList();
            //objList = data.DocumentElement.ChildNodes;
            foreach (XElement node in objList)
            {
                posType.Add(int.Parse(node.Attribute("begin").Value), "de.tudarmstadt.ukp.dkpro.core.api.lexmorph.type.pos." + node.Attribute("PosValue").Value);
                posID.Add(int.Parse(node.Attribute("begin").Value), int.Parse(node.Attribute(xmiNamespace + "id").Value));
            }
            foreach (XElement pID in objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("Token")).ToList())
            {
                if (posType.ContainsKey(int.Parse(pID.Attribute("begin").Value)))
                {
                    id = int.Parse(pID.Attribute(xmiNamespace + "id").Value);
                    begin = int.Parse(pID.Attribute("begin").Value);
                    end = int.Parse(pID.Attribute("end").Value);

                    element = Document.GetElementOfTypeInRangeGreaterEqual<AnnotationToken>(begin, end);
                    pos = new PartOfSpeech(element, posID[begin], element.Begin, element.End, posType[begin]);
                }
            }

            /************************************************************************************************************
            get spatial entities
            ************************************************************************************************************/
            ClassifiedObject cO;
            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("SpatialEntity")).ToList();
            if (objList.Count > 0)
            {
                foreach (XElement oId in objList)
                {
                    id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                    begin = int.Parse(oId.Attribute("begin").Value);
                    end = int.Parse(oId.Attribute("end").Value);
                    string word = oId.Attribute("comment").Value;
                    cO = new ClassifiedObject(Document, id, begin, end, word, word, word, word);
                    NN_Helper.isoSpatialEntities.Add(cO);
                }
            }

            List<string> brokenLinks = new List<string>();
            IsoLink link;
            /************************************************************************************************************
            get iso links
            ************************************************************************************************************/
            objList = data.Root.Descendants().Where(a => a.Name.LocalName.Contains("MetaLink")).ToList();
            if (objList.Count > 0)
            {
                foreach (XElement oId in objList)
                {
                    id = int.Parse(oId.Attribute(xmiNamespace + "id").Value);
                    if (oId.Attribute("rel_type").Value == "MASK")
                    {
                        int reference = int.Parse(oId.Attribute("figure").Value);
                        string word = oId.Attribute("comment").Value.Length > 0 ? oId.Attribute("comment").Value + " " : oId.Attribute("comment").Value;
                        ClassifiedObject classifiedObject = Document.GetElementByID<ClassifiedObject>(reference, false);
                        classifiedObject.Praefix = word + classifiedObject.Praefix;
                    }
                }
            }

            foreach (System.Type type in Document.Type_Map.Keys)
                Document.Type_Map[type].Sort((x, y) => x.Begin.CompareTo(y.Begin));
            DocumentCreated = true;

            Debug.Log("Document created");
            return Document;
        }
    }
}