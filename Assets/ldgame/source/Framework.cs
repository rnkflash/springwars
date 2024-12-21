using UnityEngine;
using UnityEngine.Events;

public static class G
{
    public static AudioSystem audio;
    public static Main main;
    public static HUD hud;
    public static UI ui;
    public static Savefile save;
    public static CameraHandle camera;
    public static Feel feel;
    public static ScreenFader fader;

    public static RunState run;

    public static InteractiveObject drag_card;
    public static InteractiveObject hover_card;
    
    public static bool IsPaused;
}

public class ManagedBehaviour : MonoBehaviour
{
    void Update()
    {
        if (!G.IsPaused)
            PausableUpdate();
    }

    protected virtual void PausableUpdate()
    {
    }

    void FixedUpdate()
    {
        if (!G.IsPaused)
            PausableFixedUpdate();
    }

    protected virtual void PausableFixedUpdate()
    {
    }
}