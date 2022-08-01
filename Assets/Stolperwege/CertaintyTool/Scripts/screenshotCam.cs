using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class screenshotCam : MonoBehaviour
{
    public int resWidth = 1920;
    public int resHeight = 1080;
    private Camera screencamera;
    private bool takeShot = false;
    // Start is called before the first frame update
    public void Awake()
    {
        screencamera = gameObject.GetComponent<Camera>();
    }

    public static string ScreenShotName(int width, int height)
    {
        return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png",
                             Application.dataPath,
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void takeScreenshot()
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        screencamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        screencamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screencamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidth, resHeight);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    private void LateUpdate()
    {
        gameObject.transform.position = StolperwegeHelper.CenterEyeAnchor.transform.position;
        gameObject.transform.rotation = StolperwegeHelper.CenterEyeAnchor.transform.rotation;
        if (SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            takeScreenshot();
        }
    }
}
