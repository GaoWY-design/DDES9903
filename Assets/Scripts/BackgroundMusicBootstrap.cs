using UnityEngine;

public class BackgroundMusicBootstrap : MonoBehaviour
{
    const string MusicObjectName = "GameAudio_Music3";
    const string ResourceName = "音乐3";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureMusic()
    {
        var existing = GameObject.Find(MusicObjectName);
        if (existing != null)
        {
            var src = existing.GetComponent<AudioSource>();
            if (src != null && src.clip != null && !src.isPlaying)
                src.Play();
            return;
        }

        var clip = Resources.Load<AudioClip>(ResourceName);
        var go = new GameObject(MusicObjectName);
        var audio = go.AddComponent<AudioSource>();
        audio.playOnAwake = true;
        audio.loop = true;
        audio.spatialBlend = 0f;
        audio.volume = 0.55f;
        audio.clip = clip;
        if (clip != null)
        {
            audio.Play();
            Debug.Log("[BackgroundMusicBootstrap] Playing BGM from Resources/" + ResourceName);
        }
        else
        {
            Debug.LogWarning("[BackgroundMusicBootstrap] Missing audio Resources/" + ResourceName +
                             " (wav/ogg). Mount point created; add file to Assets/Resources/音乐3.");
        }
    }
}
