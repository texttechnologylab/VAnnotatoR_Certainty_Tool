using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoObject : StolperwegeObject {

    private string VideoURL = "";

    public override StolperwegeElement Referent
    {
        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;

            StolperwegeUri uri = null;
            foreach (StolperwegeElement e in Referent.GetRelatedElementsByType(StolperwegeInterface.EquivalentRelation))
                if (e is StolperwegeUri) uri = (StolperwegeUri)e;

            if (uri != null) VideoURL = uri.Value;
        }
    }

    public override ExpandView OnExpand()
    {
        ExpandView expandView =  base.OnExpand();

        expandView.SetColor(StolperwegeHelper.GUCOLOR.PURPLE);

        GameObject videoBox = ExpandView.createContentBox();
        videoBox.GetComponent<MeshRenderer>().material.color = Color.white;

        expandView.drawComponent(videoBox.transform, ExpandView.LAYOUT.FULL);

        VideoPlayer player = videoBox.AddComponent<VideoPlayer>();
        player.url = VideoURL;
        player.isLooping = true;
        player.aspectRatio = VideoAspectRatio.FitHorizontally;

        player.Prepare();
        player.Play();

        expandView.OnLongClick += () =>
        {
            if (player.isPlaying)
                player.Pause();
            else player.Play();
        };

        expandView.ScaleMultiplier = new Vector2(0.5f, 0.5f);
        expandView.StartScale = expandView.transform.localScale;


        expandView.dragable = false;

        return expandView;
    }
}
