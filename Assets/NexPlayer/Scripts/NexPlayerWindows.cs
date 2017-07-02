using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// NexPlayer for Windows
/// </summary>
public class NexPlayerWindows : NexPlayerBase
{
    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeSetTextureFromUnity(System.IntPtr texture, int w, int h);

    [DllImport("WindowsNexPlayerSDK")]
    private static extern IntPtr NativeGetRenderEventFunc();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeOpenURI(string URI);

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeShutdown();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeStartPlayBack();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativePause();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeSeek(int miliseconds);

    [DllImport("WindowsNexPlayerSDK")]
    private static extern bool NativeQueueIsEmpty();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeQueuePop();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern bool NativeQueueFrontIsString();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeQueueFrontString(StringBuilder buffer, int buffLen);

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeQueueFrontParamEvent();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeQueueFrontParam1();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeQueueFrontParam2();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern double NativeGetCurrentTime();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern double NativeGetTotalTime();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeGetVideoHeight();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeGetVideoWidth();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern void NativeInit();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern bool NativeCheckValidity();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeGetMilisecondsBuffered();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeGetMilisecondsEndBuffered();

    [DllImport("WindowsNexPlayerSDK")]
    private static extern int NativeGetMilisecondsStartBuffered();

    const int NEXPLAYER_TEXTURE_CHANGED = 0,
    NEXPLAYER_EVENT_LOADSTART = 1,
    NEXPLAYER_EVENT_PROGRESS = 2,
    NEXPLAYER_EVENT_SUSPEND = 3,
    NEXPLAYER_EVENT_ABORT = 4,
    NEXPLAYER_EVENT_ERROR = 5,
    NEXPLAYER_EVENT_EMPTIED = 6,
    NEXPLAYER_EVENT_STALLED = 7,
    NEXPLAYER_EVENT_PLAY = 8,
    NEXPLAYER_EVENT_PAUSE = 9,
    NEXPLAYER_EVENT_LOADEDMETADATA = 10,
    NEXPLAYER_EVENT_LOADEDDATA = 11,
    NEXPLAYER_EVENT_WAITING = 12,
    NEXPLAYER_EVENT_PLAYING = 13,
    NEXPLAYER_EVENT_CANPLAY = 14,
    NEXPLAYER_EVENT_CANPLAYTHROUGH = 15,
    NEXPLAYER_EVENT_SEEKING = 16,
    NEXPLAYER_EVENT_SEEKED = 17,
    NEXPLAYER_EVENT_TIMEUPDATE = 18,
    NEXPLAYER_EVENT_ENDED = 19,
    NEXPLAYER_EVENT_RATECHANGE = 20,
    NEXPLAYER_EVENT_DURATIONCHANGE = 21,
    NEXPLAYER_EVENT_VOLUMECHANGE = 22,
    NEXPLAYER_EVENT_FORMATCHANGE = 1000,
    NEXPLAYER_EVENT_PURGEQUEUEDEVENTS = 1001,
    NEXPLAYER_EVENT_TIMELINE_MARKER = 1002,
    NEXPLAYER_EVENT_BALANCECHANGE = 1003,
    NEXPLAYER_EVENT_DOWNLOADCOMPLETE = 1004,
    NEXPLAYER_EVENT_BUFFERINGSTARTED = 1005,
    NEXPLAYER_EVENT_BUFFERINGENDED = 1006,
    NEXPLAYER_EVENT_FRAMESTEPCOMPLETED = 1007,
    NEXPLAYER_EVENT_NOTIFYSTABLESTATE = 1008,
    NEXPLAYER_EVENT_FIRSTFRAMEREADY = 1009,
    NEXPLAYER_EVENT_TRACKSCHANGE = 1010,
    NEXPLAYER_EVENT_OPMINFO = 1011,
    NEXPLAYER_EVENT_STREAMRENDERINGERROR = 1014;

    private Texture2D textureToUpdate = null;

    private string URI = null;
    private bool autoPlay = true;
    private bool useExtendedLogs;

    private int heightTexture = 0;
    private int widthTexture = 0;

    private NexPlayerStatus statusPlayer;

    private static bool isPausedBeforeSeek = false;

    public override void Init(string URI)
    {
        Init(URI, true, false);
    }

    public override void Init(string URI, bool autoPlay, bool useExtendedLogs)
    {
        this.URI = URI;
        this.autoPlay = autoPlay;
        this.useExtendedLogs = useExtendedLogs;
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;

        if (NativeCheckValidity())
            NativeInit();
        else throw new System.Exception("Invalid Time Lock!");
    }

    public override void Pause()
    {
        NativePause();
        isPausedBeforeSeek = true;
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
        GetStatusPlayer();
    }

    public override void Resume()
    {
        NativeStartPlayBack();
        isPausedBeforeSeek = false;
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
        GetStatusPlayer();
    }

    public override void Seek(int milliseconds)
    {
        if (GetTotalTime() <= milliseconds)
            milliseconds = GetTotalTime();
        NativeSeek(milliseconds);
    }

    public override void StartPlayBack()
    {
        NativeStartPlayBack();
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
        GetStatusPlayer();
    }

    public override void Stop()
    {
        NativeShutdown();
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;
        GetStatusPlayer();
    }

    public override IEnumerator CoroutineEndOfTheFrame()
    {
        while (true)
        {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();

            // Issue a plugin event with arbitrary integer identifier.
            // The plugin can distinguish between different
            // things it needs to do based on this ID.
            Log("Before IssuePluginEvent");

            GL.IssuePluginEvent(NativeGetRenderEventFunc(), 1);

            Log("After IssuePluginEvent");

            while (!NativeQueueIsEmpty())
            {
                if (NativeQueueFrontIsString())
                {
                    if (useExtendedLogs)
                    {
                        StringBuilder str = new StringBuilder(256);
                        NativeQueueFrontString(str, str.Capacity);
                        Log(str.ToString());
                    }
                }
                else
                {
                    CallBack(NativeQueueFrontParamEvent(), NativeQueueFrontParam1(), NativeQueueFrontParam2());
                }

                NativeQueuePop();
            }
        }
    }

    public override Texture GetTexture()
    {
        return textureToUpdate;
    }

    public override NexPlayerStatus GetStatusPlayer()
    {
        return statusPlayer;
    }

    public override int GetCurrentTime()
    {
        return (int)(NativeGetCurrentTime()*1000);
    }

    public override int GetTotalTime()
    {
        return (int)(NativeGetTotalTime()*1000);
    }

    public override int GetVideoWidth()
    {
        return NativeGetVideoWidth();
    }

    public override int GetVideoHeight()
    {
        return NativeGetVideoHeight();
    }

    public override int GetBufferedEnd()
    {
        return NativeGetMilisecondsEndBuffered();
    }

    private void CallBackInMainThread(int paramEvent, int param1, int param2)
    {
        lock (this) //This can be null the second time it's called. Why? ... I don't know
        {
            Log("CallBack paramEvent: " + paramEvent + ", param1: " + param1 + ", param2: " + param2);

            switch (paramEvent)
            {
                case NEXPLAYER_TEXTURE_CHANGED:
                    {
                        if (param1 == 0)
                        { // First texture created. Subsequent calls will have this set to 0
                            NativeOpenURI(URI);
							statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_OPENED;

							CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_INIT_COMPLEATE, 0, 0);

                            if (autoPlay)
                                NativeStartPlayBack();
                        }
                        else
                        {
                            widthTexture = param1;
                            heightTexture = param2;

                            Log("Texture2D heightTexture: " + heightTexture + ", widthTexture: " + widthTexture);
               
                            textureToUpdate = new Texture2D(widthTexture, heightTexture, TextureFormat.BGRA32, false); //TextureFormat.RGBA32 TextureFormat.BGRA32

                            Log("new Texture2D done");

                            textureToUpdate.filterMode = FilterMode.Bilinear;
                            textureToUpdate.anisoLevel = 1;
                            textureToUpdate.wrapMode = TextureWrapMode.Repeat;
                    
                            Log("Texture in Unity created");

                            NativeSetTextureFromUnity(textureToUpdate.GetNativeTexturePtr(), textureToUpdate.width, textureToUpdate.height);

                            Log("NativeSetTextureFromUnity already called");

                            CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, widthTexture, heightTexture);

                            Log("CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED done");

                        }
                    }
                    break;
                case NEXPLAYER_EVENT_ENDED:
                    {
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
                        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_END_OF_CONTENT, param1, param2);
                    }
                    break;
                case NEXPLAYER_EVENT_PLAYING:
                    {
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
                        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_STARTED, param1, param2);
                    }
                    break;
                case NEXPLAYER_EVENT_PAUSE:
                    {
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
                        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED, param1, param2);
                    }
                    break;
                case NEXPLAYER_EVENT_TIMEUPDATE:
                {
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME, param1, param2);
                }
                    break;
                case NEXPLAYER_EVENT_BUFFERINGSTARTED:
                {
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_BUFFERING;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_STARTED, param1, param2);
                }
                break;
                case NEXPLAYER_EVENT_BUFFERINGENDED:
                {
                    if (!isPausedBeforeSeek)
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
                    else
                        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_ENDED, param1, param2);
                }
                break;
                case NEXPLAYER_EVENT_FORMATCHANGE: CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TRACK_CHANGED, param1, param2); break;
                // Otherwise it won't be called in some .mp4 online videos
                case NEXPLAYER_EVENT_FIRSTFRAMEREADY: CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TRACK_CHANGED, param1, param2); break; 
                case NEXPLAYER_EVENT_ERROR:
                {
                    Log("NEXPLAYER_EVENT_ERROR internal error. param1: " + param1 + ", param2: " + param2);
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;

                    int errorCode = (int) NexPlayerError.NEXPLAYER_ERROR_GENERAL;
                    if (param1 == 4) errorCode = (int)NexPlayerError.NEXPLAYER_ERROR_SRC_NOT_FOUND;

                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ERROR, errorCode, param2);
                    NativeShutdown();
                }
                break;

                default: break;
            }
        }
    }

    private void CallOnEvent(NexPlayerEvent eventType, int param1, int param2)
    {
        if (OnEvent != null)
            OnEvent(eventType, param1, param2);
    }

    private void CallBack(int paramEvent, int param1, int param2)
    {
        Log("CallBack paramEvent: " + paramEvent + ", param1: " + param1 + ", param2: " + param2);

        // In case we need it in the future
        // https://github.com/nickgravelyn/UnityToolbag/tree/master/Dispatcher
        //Dispatcher.InvokeAsync(() =>
        //{ // We do everything in the main thread. Unity functions can not be called outside of the main thread.
            CallBackInMainThread(paramEvent, param1, param2);
        //}); 
    }

    private void Log(string str)
    {
        if (useExtendedLogs)
            Debug.Log("NexPlayer for Windows: " + str);
    }

    ~NexPlayerWindows()
    {
        Log("NativeShutdown");
        NativeShutdown();
    }

    public override void ClosePlayback()
    {
        NativeShutdown();
        Log("NEXPLAYER_EVENT_CLOSED will be called");

        OnEvent(NexPlayerEvent.NEXPLAYER_EVENT_CLOSED, 0, 0);
    }
}
