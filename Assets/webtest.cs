using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class webtest : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(Upload());
    }

    IEnumerator Upload()
    {
        WWWForm form = new WWWForm();

        form.AddField("latitude", "1");
        form.AddField("longitude", "1");


        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:4567/position", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.LogWarning(www.GetResponseHeader("message"));
            }
        }
    }
}
