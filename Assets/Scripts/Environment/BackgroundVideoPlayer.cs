using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Optional looped background video support for menu or gameplay scenes.
/// If no VideoClip is assigned, the scene continues normally.
/// </summary>
public class BackgroundVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage targetRawImage;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool muteAudio = true;
    [SerializeField] private bool hideIfNoVideo = true;

    private void Awake()
    {
        ConfigureVideoPlayer();
    }

    private void Start()
    {
        ApplyVideoAssignments();

        if (videoClip == null)
        {
            if (hideIfNoVideo && targetRawImage != null)
            {
                targetRawImage.gameObject.SetActive(false);
            }

            return;
        }

        if (targetRawImage != null)
        {
            targetRawImage.gameObject.SetActive(true);
        }

        if (playOnStart)
        {
            videoPlayer.Play();
        }
    }

    public void Play()
    {
        ConfigureVideoPlayer();
        ApplyVideoAssignments();

        if (videoPlayer != null && videoClip != null)
        {
            if (targetRawImage != null)
            {
                targetRawImage.gameObject.SetActive(true);
            }

            videoPlayer.Play();
        }
    }

    public void Stop()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }

    public void SetVideoClip(VideoClip clip)
    {
        videoClip = clip;
        ApplyVideoAssignments();

        if (videoClip == null && hideIfNoVideo && targetRawImage != null)
        {
            targetRawImage.gameObject.SetActive(false);
        }
    }

    private void ConfigureVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = loop;

        if (muteAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
    }

    private void ApplyVideoAssignments()
    {
        ConfigureVideoPlayer();

        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }

        if (renderTexture != null)
        {
            videoPlayer.targetTexture = renderTexture;
        }

        if (targetRawImage != null && renderTexture != null)
        {
            targetRawImage.texture = renderTexture;
        }
    }
}
