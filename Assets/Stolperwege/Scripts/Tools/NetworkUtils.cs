using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.Xml.Linq;
using System.Text;
using System.Linq;

public class NetworkUtils : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	
    

    /// <summary>
    /// Converts a dictionary to json
    /// </summary>
    public static string DictToJson(Dictionary<string, string> inputDict)
    {
        JsonData jsonData = new JsonData();
        foreach (string key in inputDict.Keys)
            jsonData[key] = inputDict[key];
        return jsonData.ToJson();
    }

    /// <summary>
    /// Converts string to Vector3
    /// </summary>
    public static Vector3 StringToVector3(string aData)
    {
        string[] values = aData.Split(',');
        Vector3 result = new Vector3();

        result = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        Debug.Log(result);
        return result;
    }

    /// <summary>
    /// Converts a json to Dictionary
    /// </summary>
    public static Dictionary<string, string> JsonToDict(string json)
    {
        JsonData jsonData = JsonMapper.ToJson(json);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (string key in jsonData.Keys)
            dict.Add(key, jsonData[key].ToString());
        return dict;
    }
    
    public static Dictionary<string,string> parseRoomData(string jsonDataString)
    {


        Dictionary<string, string> roomDataDict = new Dictionary<string, string>();

        roomDataDict = JsonToDict(jsonDataString);

        return roomDataDict;
    }

    /// <summary>
    /// Converts XML to Dictionary
    /// </summary>
    public static Dictionary<string, string> XmlToDictionary
                                        (string key, string value, XElement baseElm)
    {
        Dictionary<string, string> roomObjectsOnServer = new Dictionary<string, string>();

        foreach (XElement elm in baseElm.Elements())
        {
            string dictKey = elm.Attribute(key).Value;
            string dictVal = elm.Attribute(value).Value;

            roomObjectsOnServer.Add(dictKey, dictVal);

        }

        Debug.Log("objects in dict: " + roomObjectsOnServer);
        return roomObjectsOnServer;
    }


    /// <summary>
    /// Converts Dictionary to XML
    /// </summary>
    public static XElement DictToXml
                  (Dictionary<string, Vector3> inputDict, string elmName, string valuesName)
    {

        XElement outElm = new XElement(elmName);

        Dictionary<string, Vector3>.KeyCollection keys = inputDict.Keys;

        foreach (string key in keys)
        {


            XElement inner = new XElement(valuesName);
            Debug.Log(key);
            inner.Add(new XAttribute("key", key));
            inner.Add(new XAttribute("value", inputDict[key]));

            outElm.Add(inner);
        }


        return outElm;
    }

    /// <summary>
    /// Converts string to Vector3
    /// </summary>
    public static Vector3[] DeserializeVector3Array(string aData)
    {
        string[] vectors = aData.Split('|');
        Vector3[] result = new Vector3[vectors.Length];
        for (int i = 0; i < vectors.Length; i++)
        {
            string[] values = vectors[i].Split(' ');
            if (values.Length != 3)
                throw new System.FormatException("component count mismatch. Expected 3 components but got " + values.Length);
            result[i] = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }
        return result;
    }

    /// <summary>
    /// Converts Vector3 to string
    /// </summary>
    public static string SerializeVector3Array(Vector3[] aVectors)
    {
        StringBuilder sb = new StringBuilder();
        foreach (Vector3 v in aVectors)
        {
            sb.Append(v.x).Append(" ").Append(v.y).Append(" ").Append(v.z).Append("|");
        }
        if (sb.Length > 0) // remove last "|"
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }


    /// <summary>
    /// changes object scale to fit an excact size
    /// </summary>
    public void newScale(GameObject go, float newSize)
    {
        //float size;
        float x = go.GetComponent<Renderer>().bounds.size.x;
        float y = go.GetComponent<Renderer>().bounds.size.y;
        float z = go.GetComponent<Renderer>().bounds.size.z;
        float[] sizes = new float[] {x , y, z };
        float biggestSize = sizes.Max();


        Vector3 rescale = go.transform.localScale;

        if(biggestSize == x)
        {
            rescale.x = newSize * rescale.y / biggestSize;
        }
        else if(biggestSize == y)
        {
            rescale.y = newSize * rescale.y / biggestSize;
        }
        else if(biggestSize == z)
        {
            rescale.z = newSize * rescale.y / biggestSize;
        }
        else
        {
            rescale.x = newSize * rescale.y / biggestSize;
        }
        
        go.transform.localScale = rescale;

    }

    public string[] SplitTransformData(string positionAndRotation)
    {
        string[] splitPositionData = positionAndRotation.Split(':');
        return splitPositionData;
    }

    public void CoroutineHelper(IEnumerator routine)
    {
        StartCoroutine(routine);
    }


}
