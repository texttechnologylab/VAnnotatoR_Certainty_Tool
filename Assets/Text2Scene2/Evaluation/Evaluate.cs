using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using Text2Scene.NeuralNetwork;

namespace Text2Scene
{
    /// <summary>
    /// Organizes all Methods for evaluation
    /// </summary>
    public static class Evaluate
    {
        public static Dictionary<string, HashSet<EvalutationWords>> texts = new Dictionary<string, HashSet<EvalutationWords>>();
        // Start is called before the first frame update
        public static void Init()
        {
            string s = Application.dataPath;
            Debug.Log(s);
            DirectoryInfo dir = new DirectoryInfo("B:/Vincent/Dokumente/Uni/Master/VAnnotatoRv2/Assets/Text2Scene2/Evaluation/DataImport/");
            FileInfo[] info = dir.GetFiles("*.txt");
            
            foreach (FileInfo f in info)
            {
                HashSet<EvalutationWords> allNouns = new HashSet<EvalutationWords>();
                StreamReader sr = new StreamReader(f.FullName);
                string text = sr.ReadLine();
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    if (line.Length == 0) continue;
                    string[] words = line.Split('|');
                    if (words.Length != 3) throw new Exception("File enthält zu wenig Kategorien");
                    allNouns.Add(new EvalutationWords(words[0], words[1].Split(',').ToList(), words[2].Split(',').ToList()));
                    
                }
                texts.Add(text, allNouns);
            }
        }

        public static void Eval(string pathName, string text)
        {
            StreamWriter file = new StreamWriter("B:/Vincent/Dokumente/Uni/Master/VAnnotatoRv2/Assets/Text2Scene2/Evaluation/Results/" + pathName + ".txt");
            string s = "Word|PartNet|PartNetFail|Disambiguation|DisambiguationFail";
            file.WriteLine(s);
            foreach (ClassifiedObject obj in NN_Helper.isoSpatialEntities)
            {
                string PartNetCorrect = "";
                string DisambigCorrext = "";
                foreach (EvalutationWords ew in texts[text])
                {
                    if (obj.Comment.Contains(ew.word)) //ew.word == 
                    {
                        PartNetCorrect = ew.partNetWords.Contains(obj.Holonym) ? "1" : "0";
                        DisambigCorrext = ew.disambiguationWords.Contains(obj.DisambiguationWord) ? "1" : "0";
                        break;
                    }
                }
                s = obj.Comment + "|" + obj.Holonym + "|" + PartNetCorrect + "|" + obj.DisambiguationWord + "|" + DisambigCorrext;
                file.WriteLine(s);
            }
            file.Close();
            Debug.Log("File written");
        }
    }

    [CustomEditor(typeof(Text2SceneInterface))]
    public class WorkflowEditor : Editor
    {
        private int counter = 0;
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            DrawDefaultInspector();
            Text2SceneInterface tO = (Text2SceneInterface)target;
            if (GUILayout.Button("Test Workflow - Once"))
            {
                tO.Evaluation = true;
                tO.InitButton(Evaluate.texts.Keys.ToList()[counter], "Wohnzimmer" + counter, true);
                Debug.Log(Evaluate.texts.Keys.ToList()[counter]);
                counter++;
                if (counter % Evaluate.texts.Count == 0) counter = 0;
            }
        }
    }

    public class EvalutationWords
    {
        public string word;
        public HashSet<string> partNetWords;
        public List<string> disambiguationWords;

        public EvalutationWords(string word, List<string> partNetWords, List<string> disambiguationWords)
        {
            this.word = word ?? throw new ArgumentNullException(nameof(word));
            this.partNetWords = new HashSet<string>();
            partNetWords.ForEach(x => this.partNetWords.Add(x));
            this.disambiguationWords = disambiguationWords ?? throw new ArgumentNullException(nameof(disambiguationWords));
        }
    }
}