using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoadBrowser : MonoBehaviour
{
    public BrowserMenu Browser { get; private set; }
    public CertaintyToolInterface Interface;

    private string BrowserHeaderText = "Load Rating";
    private string BrowserActionText = "Load";

    List<(int, int)> DBKeyLocalKeyList;

    bool LoadFromFile = true;
    string SavePath = "Assets/Stolperwege/CertaintyTool/saves";

    public void Init(CertaintyToolInterface CTInterface)
    {
        Interface = CTInterface;
        DBKeyLocalKeyList = new List<(int, int)>();

        Browser = gameObject.GetComponent<BrowserMenu>();

        Browser.HeaderText = BrowserHeaderText;
        Browser.ButtonActionText = BrowserActionText;

        Browser._ActionButtonAction = Load;

        Browser._EntryAction = SelectEntry;

        AddDummyItems();
    }

    private void AddDummyItems()
    {
        List<string> dummies = new List<string>();
        //dummies.Add("A1-0");
        //dummies.Add("A2-1");
        //dummies.Add("A3-2");
        //dummies.Add("A4-3");
        //dummies.Add("A5-4");
        //dummies.Add("B1-5");
        //dummies.Add("B2-6");
        //dummies.Add("B3-7");
        //dummies.Add("B4-8");
        //dummies.Add("B5-9");
        //dummies.Add("C1-10");
        //dummies.Add("C2-11");

        if (LoadFromFile)
        {
            string[] files = Directory.GetFiles(SavePath);
            foreach (string filepath in files)
            {
                if (filepath.EndsWith(".txt"))
                {
                    string filename = Path.GetFileNameWithoutExtension(filepath);
                    dummies.Add(filename);
                }
            }
        }

        Browser.ItemLabels = dummies;
    }

    private void Load(int? selectedItem)
    {
        Debug.LogWarning(selectedItem.ToString() + " loaded");
        if (selectedItem != null)
        {
            Interface.SelectedBuilding?.LoadFromTestFile(Browser.ItemLabels[(int)selectedItem]);
        }
    }

    private void SelectEntry(int? item)
    {
        Debug.LogWarning("Button " + item.ToString() + " pressed");
        if (item != null)
        {

        }
    }
}
