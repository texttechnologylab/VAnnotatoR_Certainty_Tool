using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioObject : StolperwegeObject {

    public override StolperwegeElement Referent
    {
        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;

            ((Audio)Referent).loadAudioToSource(GetComponent<AudioSource>(), this);
            GetComponent<AudioSource>().loop = true;
        }
    }

    public override ExpandView OnExpand()
    {
        OnDrop();

        AudioSource source = GetComponent<AudioSource>();

        if (source.isPlaying) source.Stop();
        else source.Play();

        return null;
    }
}
