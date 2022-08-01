using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StolperwegePositionObject : StolperwegeObject {

    private const int ZOOM = 14;

    private Vector2 SlippyXY;

    Texture MapTexture;

    Vector2 posToUnityCoords;

    public override void Start()
    {
        base.Start();

        Position stPos = (Position)Referent;
        SlippyXY = OSMManager.LatLonToXY(stPos.Latitude, stPos.Longitude,ZOOM);

        StartCoroutine(LoadMapTexture());

        posToUnityCoords = OSMManager.LatLonToUnityCoords(stPos.Latitude, stPos.Longitude, SlippyXY,SlippyXY, 1, ZOOM);
    }

    public IEnumerator LoadMapTexture()
    {
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(LoadMap.GetTileUri((int)SlippyXY.x, (int)SlippyXY.y, ZOOM));
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        MapTexture = DownloadHandlerTexture.GetContent(webRequest);

        GetComponent<MeshRenderer>().material.mainTexture = MapTexture;
    }

    public override ExpandView OnExpand()
    {
        ExpandView expandView = base.OnExpand();

        expandView.SetColor(StolperwegeHelper.GUCOLOR.EMOROT);

        if (expandView == null) return null;

        GameObject imageBox = ExpandView.createContentBox();

        imageBox.GetComponent<MeshRenderer>().material.mainTexture = MapTexture;

        expandView.drawComponent(imageBox.transform, ExpandView.LAYOUT.FULL);

        imageBox.AddComponent<StolperwegeImageExtended>();
        imageBox.GetComponent<Collider>().enabled = true;
        imageBox.GetComponent<Collider>().isTrigger = true;

        expandView.ScaleMultiplier = new Vector2(0.5f, 0.5f);
        expandView.StartScale = expandView.transform.localScale;

        expandView.dragable = false;

        transform.localPosition -= Vector3.up * 0.1f;

        expandView.ScaleMultiplier = new Vector2(2, 2);

        GameObject posMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        posMarker.transform.parent = imageBox.transform;
        posMarker.transform.localScale = Vector3.one * 0.1f;
        posMarker.transform.localPosition = new Vector3(posToUnityCoords.x, -1, posToUnityCoords.y);
        posMarker.GetComponent<MeshRenderer>().material.color = StolperwegeHelper.GUCOLOR.EMOROT;


        return expandView;
    }
}
