using Common;
using UnityEngine;

public class Sound_AmbienceForest : CMSEntity
{
    public Sound_AmbienceForest()
    {
        Define<AmbientTag>().clip = "audio/gameplay_theme1".Load<AudioClip>();
        Define<AmbientTag>().volume = 0.5f;
    }
}