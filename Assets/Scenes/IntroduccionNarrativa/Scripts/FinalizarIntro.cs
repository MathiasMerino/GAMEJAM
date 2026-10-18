using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class FinalizarIntro : MonoBehaviour
{
    private VideoPlayer video;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
        video.loopPointReached += TerminarVideo;
    }

    void TerminarVideo(VideoPlayer vp)
    {
        SceneManager.LoadScene("EscenaInicial");
    }

    void OnDestroy()
    {
        if (video != null)
            video.loopPointReached -= TerminarVideo;
    }
}