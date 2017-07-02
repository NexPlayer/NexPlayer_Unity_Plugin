using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NexPlayerWebGL : NexPlayerBase
{
#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern int WebGLNexPlayerCreate(string url, bool autoPlay, bool extendedLogs);

    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerUpdate(int video, int texture);

    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerPlay(int video);

    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerPause(int video);

    // These methods are in seconds
    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerSeek(int video, float time);

    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerLoop(int video, bool loop);

    [DllImport("__Internal")]
    private static extern int WebGLNexPlayerWidth(int video);

    [DllImport("__Internal")]
    private static extern int WebGLNexPlayerHeight(int video);

    [DllImport("__Internal")]
    private static extern bool WebGLNexPlayerIsReady(int video);

    [DllImport("__Internal")]
    private static extern float WebGLNexPlayerTime(int video);

    [DllImport("__Internal")]
    private static extern float WebGLNexPlayerDuration(int video);

    [DllImport("__Internal")]
    private static extern float WebGLNexPlayerBufferInfo(int video);

    [DllImport("__Internal")]
    private static extern bool WebGLNexPlayerQueueIsEmpty(int video);

    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerQueuePop(int video);

    [DllImport("__Internal")]
    private static extern int WebGLNexPlayerFrontParamEvent(int video);

    [DllImport("__Internal")]
    private static extern int WebGLNexPlayerQueueFrontParam1(int video);

    [DllImport("__Internal")]
    private static extern int WebGLNexPlayerQueueFrontParam2(int video);

    [DllImport("__Internal")]
    private static extern bool WebGLNexPlayerIsBrowserSafari();

    [DllImport("__Internal")]
    private static extern bool WebGLNexPlayerIsVideoDASH(int video);

    [DllImport("__Internal")]
    private static extern void WebGLNexPlayerNativeShutdown(int video);

    [DllImport("__Internal")]
    private static extern bool WebGLNexPlayerVideoInstanceNull(int video);

#else
    private static int WebGLNexPlayerCreate(string url, bool autoPlay, bool extendedLogs)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerUpdate(int video, int texture)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerPlay(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerPause(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerSeek(int video, float time)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerLoop(int video, bool loop)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static int WebGLNexPlayerWidth(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static int WebGLNexPlayerHeight(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static bool WebGLNexPlayerIsReady(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static float WebGLNexPlayerTime(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static float WebGLNexPlayerDuration(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static float WebGLNexPlayerBufferInfo(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static bool WebGLNexPlayerQueueIsEmpty(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerQueuePop(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static int WebGLNexPlayerFrontParamEvent(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static int WebGLNexPlayerQueueFrontParam1(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static int WebGLNexPlayerQueueFrontParam2(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static bool WebGLNexPlayerIsBrowserSafari()
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static bool WebGLNexPlayerIsVideoDASH(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static void WebGLNexPlayerNativeShutdown(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
    private static bool WebGLNexPlayerVideoInstanceNull(int video)
    {
        throw new PlatformNotSupportedException("This class is not supported on this platform.");
    }
#endif
    // This are very similar to the nexplayerbase events, but since that could change in the future, this are placed here
    private const int NEXPLAYER_EVENT_INIT_COMPLEATE = 1,
    NEXPLAYER_EVENT_PLAYBACK_STARTED = 2,
    NEXPLAYER_EVENT_END_OF_CONTENT = 3,
    NEXPLAYER_EVENT_ON_TIME = 4,
    NEXPLAYER_EVENT_BUFFERING_STARTED = 5,
    NEXPLAYER_EVENT_BUFFERING_ENDED = 6,
    NEXPLAYER_EVENT_TEXTURE_CHANGED = 7,
    NEXPLAYER_EVENT_TRACK_CHANGED = 8,
    NEXPLAYER_EVENT_PLAYBACK_PAUSED = 9,
    NEXPLAYER_EVENT_ERROR = 10;

    private Texture2D textureToUpdate = null;
    private string URI = null;
    private bool useExtendedLogs;
    private NexPlayerStatus statusPlayer;
    private int m_Instance;

    private bool pausedBeforeSeeking = false;

    public override void Init(string URI)
    {
        Init(URI, true, false);
    }

    public override void Init(string URI, bool autoPlay, bool useExtendedLogs)
    {
        this.URI = URI;
        this.useExtendedLogs = useExtendedLogs;
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;

        m_Instance = WebGLNexPlayerCreate(this.URI, autoPlay, useExtendedLogs);

        textureToUpdate = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        textureToUpdate.wrapMode = TextureWrapMode.Repeat;
    }

    public override IEnumerator CoroutineEndOfTheFrame()
    {
        yield return null;
    }

    public override void Update()
    {
        // Only process if the video instance actually exist
        if (!WebGLNexPlayerVideoInstanceNull(m_Instance))
        {
            while (!WebGLNexPlayerQueueIsEmpty(m_Instance) && !WebGLNexPlayerVideoInstanceNull(m_Instance))
            {
                ProcessCallBack(WebGLNexPlayerFrontParamEvent(m_Instance), WebGLNexPlayerQueueFrontParam1(m_Instance), WebGLNexPlayerQueueFrontParam2(m_Instance));
                WebGLNexPlayerQueuePop(m_Instance);
            }

            // In these states the ontime is not called
            if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_BUFFERING || statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_PAUSED)
                ProcessCallBack(NEXPLAYER_EVENT_ON_TIME, 0, 0);

            if (!WebGLNexPlayerVideoInstanceNull(m_Instance) && WebGLNexPlayerWidth(m_Instance) != 0 && WebGLNexPlayerHeight(m_Instance) != 0 // The video has started
                && (textureToUpdate.width != WebGLNexPlayerWidth(m_Instance) || textureToUpdate.height != WebGLNexPlayerHeight(m_Instance)) // The video size is different from the texture 
                && !WebGLNexPlayerIsVideoDASH(m_Instance))
            { // It's not DASH. DASH will generate the proper events
                ChangeSizeTexture();
            }

            if (textureToUpdate.width != 0 && !WebGLNexPlayerVideoInstanceNull(m_Instance))
            { // The texture has been initialized
                WebGLNexPlayerUpdate(m_Instance, (int)textureToUpdate.GetNativeTexturePtr());
            }
        }
    }

    private void ChangeSizeTexture()
    {
        int width = WebGLNexPlayerWidth(m_Instance);
        int height = WebGLNexPlayerHeight(m_Instance);

        // The texture may need to change size
        Log("Video width: " + width + ", Video height: " + height);
        if (width != textureToUpdate.width || height != textureToUpdate.height)
        {
            textureToUpdate.Resize(width, height);
            OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, 0, 0);
            OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TRACK_CHANGED, 0, 0);
        }
    }

    private void ProcessCallBack(int paramEvent, int param1, int param2)
    {
        Log("paramEvent: " + paramEvent + ", param1: " + param1 + ", param2: " + param2);

        switch (paramEvent)
        {
            // Only used for DASH
            case NEXPLAYER_EVENT_TRACK_CHANGED: ChangeSizeTexture(); break;
            case NEXPLAYER_EVENT_INIT_COMPLEATE:
                {
                    if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_CLOSED) // It's the first time it's closed
                    {
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_OPENED;
                        OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_INIT_COMPLEATE, param1, param2);
                    }
                }
                break;
            case NEXPLAYER_EVENT_END_OF_CONTENT: statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED; OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_END_OF_CONTENT, param1, param2); break;
            case NEXPLAYER_EVENT_ON_TIME: OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME, param1, 0); break;
            case NEXPLAYER_EVENT_PLAYBACK_STARTED:
                {
                    if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_CLOSED)
                    {
                        // This is being called before the init callback
                        ProcessCallBack(NEXPLAYER_EVENT_INIT_COMPLEATE, 0 ,0);
                    }

                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
                    OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_STARTED, param1, param2);
                }
                break;
            case NEXPLAYER_EVENT_BUFFERING_STARTED: statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_BUFFERING; OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_STARTED, param1, param2); break;
            case NEXPLAYER_EVENT_BUFFERING_ENDED: {
                    if (pausedBeforeSeeking)
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
                    else
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;

                    OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_ENDED, param1, param2);
                } break;
            case NEXPLAYER_EVENT_TEXTURE_CHANGED: OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, param1, param2); break;
            case NEXPLAYER_EVENT_PLAYBACK_PAUSED: statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED; OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED, param1, param2); break;
            case NEXPLAYER_EVENT_ERROR:
                {
                    Log("NEXPLAYER_EVENT_ERROR internal error. param1: " + param1 + ", param2: " + param2);

                    int errorCode = (int)NexPlayerError.NEXPLAYER_ERROR_GENERAL;
                    if (param1 == 4) errorCode = (int)NexPlayerError.NEXPLAYER_ERROR_SRC_NOT_FOUND;

                    OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ERROR, errorCode, param2);
                }
                break;

            default: break;
        }
    }

    public override int GetBufferedEnd()
    {
        return (int) (WebGLNexPlayerBufferInfo(m_Instance) * 1000);
    }

    public override int GetCurrentTime()
    {
        return (int) (WebGLNexPlayerTime(m_Instance) * 1000);
    }

    public override NexPlayerStatus GetStatusPlayer()
    {
        return statusPlayer;
    }

    public override Texture GetTexture()
    {
        return textureToUpdate;
    }

    public override int GetTotalTime()
    {
        return (int) (WebGLNexPlayerDuration(m_Instance) * 1000);
    }

    public override int GetVideoHeight()
    {
        return WebGLNexPlayerHeight(m_Instance);
    }

    public override int GetVideoWidth()
    {
        return WebGLNexPlayerWidth(m_Instance);
    }

    public override void Pause()
    {
        pausedBeforeSeeking = true;

        // Other wise it won't be updated until the next Update()
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
        WebGLNexPlayerPause(m_Instance);
    }

    public override void Resume()
    {
        StartPlayBack();
    }

    public override void Seek(int milliseconds)
    {
        WebGLNexPlayerSeek(m_Instance, ((float)milliseconds) / 1000.0f);
    }

    public override void StartPlayBack()
    {
        pausedBeforeSeeking = false;
        WebGLNexPlayerPlay(m_Instance);
    }

    public override void Stop()
    {
        Seek(0);
        Pause();
    }

    private void Log(string str)
    {
        if (useExtendedLogs)
            Debug.Log("NexPlayer for WebGL - Unity: " + str);
    }

    ~NexPlayerWebGL()
    {
        Log("NativeShutdown");
        //statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;
        //WebGLNexPlayerNativeShutdown(m_Instance);
    }

    public override void ClosePlayback()
    {
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;
        WebGLNexPlayerNativeShutdown(m_Instance);
        Log("NEXPLAYER_EVENT_CLOSED will be called");

        OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_CLOSED, 0, 0);
    }

}
