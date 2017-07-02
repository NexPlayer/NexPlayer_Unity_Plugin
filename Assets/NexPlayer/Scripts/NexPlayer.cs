using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NexPlayer : MonoBehaviour
{
    [Tooltip("URI used to play the video")]
    public string URL = "http://d7wce5shv28x4.cloudfront.net/sample_streams/sintel_hls_/master.m3u8";
    [Tooltip("If selected the video will auto start playing. Otherwise the video will only be initialized but the playback will not start automatically")]
    public bool autoPlay = true;
    [Tooltip("If selected a lot of extra logs will be generated. This is useful for debugging or for reporting an issue")]
    public bool extendedLogs = false;

    [Tooltip("Text element that will be updated with the total time of the video")]
    public Text totTime;
    [Tooltip("Text element that will be updated with the current time of the playback")]
    public Text currentTime;
    [Tooltip("Text element that will be updated with the current video resolution of the playback")]
    public Text videoSize;
    [Tooltip("Text element that will be updated with the current status of the playback")]
    public Text status;
    [Tooltip("Seek bar used to display the current time and the last buffered content in the secondary progress")]
    public NexSeekBar seekBar;
    [Tooltip("Image used int the play / pause button")]
    public Image playPauseImage;
    [Tooltip("Sprite used to represent the ability to pause the video")]
    public Sprite pauseSprite;
    [Tooltip("Sprite used to represent the ability to play the video")]
    public Sprite playSprite;
    [Tooltip("Array of renderer component which texture will be updated")]
    public Renderer[] rendererToUpdate;

    /// <summary>
    /// URL used to get data from other scenes
    /// </summary>
    public static string sharedURL = null;

    private NexPlayerBase player;

    void Awake()
    {
#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
#endif

        if (sharedURL != null)
            URL = sharedURL;
        sharedURL = null;
    }

    void Start()
    {
        try
        {
            // Creation of the NexPlayer instance
            player = NexPlayerFactory.GetNexPlayer();
            // Register to the events of NexPlayer
            player.OnEvent += EventNotify;
            // Initialize NexPlayer with an URI
            player.Init(URL, autoPlay, extendedLogs);

            // Change the text informing the status of the player
            status.text = "Opening...";
            // The coroutine needs to be started after the player is created an initialized
            StartCoroutine(player.CoroutineEndOfTheFrame());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error while initializing the player. Please check that your platform is supported.");
            Debug.LogError("Exception: " + e);

            status.text = "Error";
            player = null;
        }
    }

#if UNITY_EDITOR
    void HandleOnPlayModeChanged()
    {
        if (player != null)
        {
            if (EditorApplication.isPaused && player.GetStatusPlayer() == NexPlayerStatus.NEXPLAYER_STATUS_PLAYING)
            {
                // Pause the player
                player.Pause();
            }
            else if (!EditorApplication.isPaused && player.GetStatusPlayer() == NexPlayerStatus.NEXPLAYER_STATUS_PAUSED)
            {
                // Resume the player
                player.Resume();
            }
        }
    }
#endif

    void Update()
    {
        if (player != null)
        {
            player.Update();
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            StopCoroutine(player.CoroutineEndOfTheFrame());
            player = null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (player != null)
        {
            if (pauseStatus && player.GetStatusPlayer() == NexPlayerStatus.NEXPLAYER_STATUS_PLAYING)
            {
                // Pause the player
                player.Pause();
            }
            else if (!pauseStatus && (player.GetStatusPlayer() == NexPlayerStatus.NEXPLAYER_STATUS_PAUSED || player.GetStatusPlayer() == NexPlayerStatus.NEXPLAYER_STATUS_OPENED))
            {
                // Resume the player
                player.Resume();
            }
        }
    }

    void EventNotify(NexPlayerEvent paramEvent, int param1, int param2)
    {
        if (paramEvent != NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME)
            Debug.Log("EventNotify: " + paramEvent + ", param1: " + param1 + ", param2: " + param2);

        switch (paramEvent)
        {
            case NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED:
                // It's important to change the texture of every Unity object that should display the video frame when this callback is called
                if (GetComponent<Renderer>())
                    GetComponent<Renderer>().material.mainTexture = player.GetTexture();
                else GetComponent<RawImage>().texture = player.GetTexture();

                if (rendererToUpdate != null)
                {
                    foreach (Renderer renderer in rendererToUpdate)
                    {
                        renderer.material.mainTexture = player.GetTexture();
                    }
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME:
                SetCurrentTime(); break;

            case NexPlayerEvent.NEXPLAYER_EVENT_TRACK_CHANGED:
                SetVideoSize(); break;

            case NexPlayerEvent.NEXPLAYER_EVENT_INIT_COMPLEATE:
                {
                    status.text = "Opened";
                    playPauseImage.sprite = playSprite;
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_STARTED:
                {
                    SetTotalTime();
                    status.text = "Playing";
                    playPauseImage.sprite = pauseSprite;
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED:
                {
                    status.text = "Pause";
                    playPauseImage.sprite = playSprite;
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_END_OF_CONTENT:
                {
                    status.text = "Pause";
                    playPauseImage.sprite = playSprite;
                    ToogleQuit();
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_STARTED:
                status.text = "Buffering..."; break;

            case NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_ENDED:
                {
                    NexPlayerStatus statusPlayer = player.GetStatusPlayer();
                    if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_PLAYING)
                    {
                        status.text = "Playing";
                    }
                    else if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_PAUSED)
                    {
                        status.text = "Pause";
                    }
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_ERROR:
                {
                    status.text = "Error";
                    player = null;
                    switch (param1)
                    {
                        case (int)NexPlayerError.NEXPLAYER_ERROR_GENERAL: Debug.LogError("NEXPLAYER_ERROR_GENERAL"); break;
                        case (int)NexPlayerError.NEXPLAYER_ERROR_SRC_NOT_FOUND: Debug.LogError("NEXPLAYER_ERROR_SRC_NOT_FOUND"); break;
                    }
                }
                break;

            case NexPlayerEvent.NEXPLAYER_EVENT_CLOSED:
                {
                    player = null;

                    GoBack();
                }
                break;
        }
    }

    public void TooglePlayPause()
    {
        Debug.Log("Click TooglePlayPause");
        NexPlayerStatus statusPlayer = player.GetStatusPlayer();

        if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_PLAYING)
        {
            OnApplicationPause(true);
        }
        else if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_PAUSED || statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_OPENED)
        {
            OnApplicationPause(false);
        }
    }

    public void ToogleQuit()
    {
        Debug.Log("Click ToogleQuit");
        if (player != null)
        {
            player.ClosePlayback();
        }
        else
        {
            GoBack();
        }
    }

    private void GoBack()
    {
        ChooseMain.ChooseMenu();
    }

    private string GetTimeString(int milliSeconds)
    {
        string str_currentTimeSeconds = "";
        string str_currentTimeMinutes = "";
        string str_currentTimeHours = "";
        int currentTimeSeconds = milliSeconds / 1000;
        int currentTimeMinutes = 0;

        int hours = currentTimeSeconds / 3600;
        int remainder = currentTimeSeconds % 3600;
        currentTimeMinutes = remainder / 60;
        currentTimeSeconds = remainder % 60;

        str_currentTimeSeconds = currentTimeSeconds.ToString();
        str_currentTimeMinutes = currentTimeMinutes.ToString();
        str_currentTimeHours = hours.ToString();
        if (currentTimeSeconds < 10)
            str_currentTimeSeconds = "0" + str_currentTimeSeconds;
        if (currentTimeMinutes < 10)
            str_currentTimeMinutes = "0" + str_currentTimeMinutes;
        if (hours < 10)
            str_currentTimeHours = "0" + str_currentTimeHours;

        return str_currentTimeHours + ":" + str_currentTimeMinutes + ":" + str_currentTimeSeconds;
    }

    private void SetVideoSize()
    {
        int height = player.GetVideoHeight();
        int width = player.GetVideoWidth();
        videoSize.text = width.ToString() + "x" + height.ToString();
    }

    private void SetCurrentTime()
    {
        currentTime.text = GetTimeString(player.GetCurrentTime());
        seekBar.SetValue((float)player.GetCurrentTime() / (float)player.GetTotalTime());
        seekBar.SetSecondaryValue((float)player.GetBufferedEnd() / (float)player.GetTotalTime());
    }

    private void SetTotalTime()
    {
        totTime.text = GetTimeString(player.GetTotalTime());
    }

    public void Seek()
    {
        int threshold = 1000;

        int valueTemp = (int)(seekBar.GetValue() * (float)player.GetTotalTime());
        // Prevents over seeking
        bool isUpdateBigEnough = Math.Abs(valueTemp - player.GetCurrentTime()) > threshold;

        if (isUpdateBigEnough)
        {
            player.Seek(valueTemp);
        }
    }
}
