using System.Collections;
using UnityEngine;
using AOT;

public class NexPlayerAndroid : NexPlayerCommon 
{

    private static int TEX_WIDTH = 1;
    private static int TEX_HEIGHT = 1;
    private static int videoWidth = 0;
    private static int videoHeight = 0;
    private static Texture2D tex;

    private static int totalPlaybackTime = 0;

    
    
    public override void Init(string URI)
    {
        Init(URI, true, true);
    }

    public override void Init(string URI, bool autoPlay, bool useExtendedLogs)
    {
        setupPlayer(URI , autoPlay , useExtendedLogs, null);
    }

    public override void Init(string URI, bool autoPlay, bool useExtendedLogs, string keyServerURI)
    {
        setupPlayer(URI , autoPlay , useExtendedLogs, keyServerURI);

    }

    private void setupPlayer (string URI, bool autoPlay, bool useExtendedLogs , string keyServerURI) {
        Log("Init setupPlayer");
        
        // for VideoAssetPlay Later it is going to may extract different class.
        bool useAssetPlay = false;
        if(URI.Trim().StartsWith("http") || URI.Trim().StartsWith("https://") )
            useAssetPlay = false;
         else
            useAssetPlay = true; 

        if(useAssetPlay) {
            NexPlayerCommon.useAssetPlay = useAssetPlay;
        }

        NexPlayerCommon.URI = URI ;
        NexPlayerCommon.keyServerURI = keyServerURI;
        NexPlayerCommon.autoPlay = autoPlay;
        NexPlayerCommon.useExtendedLogs = useExtendedLogs;
        NexPlayerCommon.OnEventStatic = this.OnEvent;
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;
        AndroidJNIHelper.debug = true;
        Log("Init NexPlayer Android");
        Log("Graphics Device : " + SystemInfo.graphicsDeviceVersion);
        // Create a texture
        Log("Init Start");
        tex = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.ARGB32, false);
        // Set point filtering just so we can see the pixels clearly
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        // Call Apply() so it's actually uploaded to the GPU
        tex.Apply();

        // Pass texture pointer to the plugin
        SetTextureFromUnity(tex.GetNativeTexturePtr(), tex.width, tex.height);
        if(SystemInfo.graphicsDeviceVersion.Contains("OpenGL ES 3")) 
        {
            NexPlayerUnity_SetOpenGLVersion(true);
        }
        GL.IssuePluginEvent(NexPlayerUnityRenderEvent(), (int)NexPlayerUnityEventType.Initialize);
    }



    public override void Update()
    {
        Log("Update");

        while(!NEXPLAYERUnity_QueueIsEmpty()) 
        {
            int type = NEXPLAYERUnity_AsyncCmdType();
            int command = NEXPLAYERUnity_AsyncCmdValue();            
            int result = NEXPLAYERUnity_AsyncCmdResult();
            int param1 = NEXPLAYERUnity_AsyncCmdParam1();
            nexPlayerListener(type, command, result, param1);
            Log("Update | Queue is not empty");
        }

        if (TEX_WIDTH != GetTexture().width || TEX_HEIGHT != GetTexture().height)
        {

            Log("Updating size of texture with width :" + TEX_WIDTH + " height : " + TEX_HEIGHT);

            Texture2D temporaryTexture;

            temporaryTexture = new Texture2D(TEX_WIDTH, TEX_HEIGHT, TextureFormat.ARGB32, false);
            // Set point filtering just so we can see the pixels clearly
            temporaryTexture.filterMode = FilterMode.Bilinear;
            temporaryTexture.wrapMode = TextureWrapMode.Repeat;
            // Call Apply() so it's actually uploaded to the GPU
            temporaryTexture.Apply();

            // Pass texture pointer to the plugin
            UpdateTextureFromUnity(temporaryTexture.GetNativeTexturePtr(), temporaryTexture.width, temporaryTexture.height);

            tex = temporaryTexture;
            Debug.Log("NexPlayerAndroid texture updated");

            CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, TEX_WIDTH, TEX_HEIGHT);
        }
    }

    public override void Pause()
    {
        NEXPLAYERUnity_Pause();
        isPausedBeforeSeek = true;
        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
    }

    public override void Resume()
    {
        if (currentPlaybackTime == 0 && isStarted == false)
        {
            StartPlayBack();
        }
        else
            NEXPLAYERUnity_Resume();

        isPausedBeforeSeek = false;

        statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
    }

    //from this method for changeTrackAPI.
    public override NexPlayerAudioStream GetCurrentAudioStream()
    { 
        NexPlayerAudioStream currentAudioStream = new NexPlayerAudioStream();

        int id =   NEXPLAYERUnity_GetCurrentAudioStreamID();        
        string name = NEXPLAYERUnity_GetCurrentAudioStreamName();
        string language = NEXPLAYERUnity_GetCurrentAudioStreamLanguage();
        int wholeCount = NEXPLAYERUnity_GetAudioStreamCount();

        Log("GetCurrentAudioStream id c# : " +id );
        Log("GetCurrentAudioStream name c# : " +name );
        Log("GetCurrentAudioStream language c# : " +language );
        Log("GetCurrentAudioStream count c# : " +wholeCount );
       
        currentAudioStream.id =         id;
        currentAudioStream.name =       name;
        currentAudioStream.language =   language;
        
        
        return currentAudioStream ;
    }

    public override NexPlayerAudioStream[] GetAudioStreams() 
    {
        int count = NEXPLAYERUnity_GetAudioStreamCount();

        Log(" NexPlayerAudioStream count : " +count );

        NexPlayerAudioStream[] audioStreams = null;

        if(count > 0 ) {
           audioStreams = new NexPlayerAudioStream[count];
        Log("NexPlayerAudioStream audioStreams : " +audioStreams.Length );

        for(int i = 0 ; i< audioStreams.Length ; i++) {
            Log("NexPlayerAudioStream call to index count : " +i);
            audioStreams[i].id =  NEXPLAYERUnity_GetAudioStreamID(i);
            audioStreams[i].name =  NEXPLAYERUnity_GetAudioStreamName(i);
            audioStreams[i].language =  NEXPLAYERUnity_GetAudioStreamLanguage(i);
        }

    }
        return audioStreams;   
    }

    public override NexPlayerTrack[] GetTracks() {

        int count = NEXPLAYERUnity_GetTrackCount();
        
        Log(" NexPlayerTrack count : " +count );

        NexPlayerTrack[] tracks = null;

        if(count > 0) {
            
            tracks = new NexPlayerTrack[count];

            for (int i = 0; i < count; i++) {

                Log("NexPlayerTrack call to index count : " +i);
                tracks[i].id =  NEXPLAYERUnity_GetTrackID(i);
                tracks[i].bitrate =  NEXPLAYERUnity_GetTrackBitrate(i);
                tracks[i].width =  NEXPLAYERUnity_GetTrackWidth(i);
                tracks[i].height =  NEXPLAYERUnity_GetTrackHeight(i);
            }

        }

        return tracks;

    }

    public override int GetAudioStreamCount()
    {
        int num = 0;
        return num;
    }

   
    public override IEnumerator CoroutineEndOfTheFrame()
    {
        Log("CallPluginAtEndOfFrames - start");
        yield return null;

        // GL.IssuePluginEvent(NexPlayerUnityRenderEvent(), (int)NexPlayerUnityEventType.Initialize);
        if (NexPlayerAndroid.useExtendedLogs)
            NEXPLAYERUnity_SetLogLevel((int)NexPlayer_LOG_LEVEL.NEXPLAYER_LOG_LEVEL_ALL);

        if(NexPlayerCommon.keyServerURI != null) {
            NEXPLAYERUnity_SetKeyServerUri(NexPlayerCommon.keyServerURI);
        }
        
        if(NexPlayerCommon.useAssetPlay)
            NEXPLAYERUnity_OpenFD(NexPlayerAndroid.URI);
        else
            NEXPLAYERUnity_Open(NexPlayerAndroid.URI);  
        
        // NEXPLAYERUnity_EnableABR(true);

        while (true)
        {
            // Issue a plugin event with update identifier.
            GL.IssuePluginEvent(NexPlayerUnityRenderEvent(), (int)NexPlayerUnityEventType.Update);
            GL.InvalidateState();
            Log("CallPluginAtEndOfFrames - end");
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();
        }
    }


    public override int GetBufferedEnd()
    {
        return NEXPLAYERUnity_GetBufferInfo();
    }

    public override int GetCurrentTime()
    {
        return currentPlaybackTime;
    }
    
    public override Texture GetTexture()
    {
        return tex;
    }

    public override int GetTotalTime()
    {

        return totalPlaybackTime;
    }

    public override int GetVideoHeight()
    {
        Log("GetVideoHeight: " + videoHeight);
        return videoHeight;
    }

    public override int GetVideoWidth()
    {
        Log("GetVideoWidth: " + videoWidth);
        return videoWidth;
    }

    static void processOnAsyncCompleteListener(int command, int param1, int param2, int param3, int param4)
    {
        Log("[processOnAsyncCompleteListener] command:" + command + " param1:" + param1 + " param2:" + param2 + " param3:" + param3 + " param4:" + param4);
        switch (command)
        {
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_OPEN_LOCAL:
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_OPEN_STREAMING:
                {
                    Log("Open Player command: " + param1);
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_OPENED;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_INIT_COMPLEATE, param1, param2);
                    if (NexPlayerAndroid.autoPlay)
                    {
                        
                        NEXPLAYERUnity_Start(0);
                    }
                }
                break;
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_START_LOCAL:    
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_START_STREAMING:
                {
                    Log("Start Player command: " + param1);
                    totalPlaybackTime = NEXPLAYERUnity_GetContentInfoInt(1);
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
                    isStarted = true;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_STARTED, param1, param2);
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, TEX_WIDTH, TEX_HEIGHT);
                }
                break;
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_PAUSE:
                {
                    Log("Pause Player command: " + param1);
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED, param1, param2);
                }
                break;
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_RESUME:
                {
                    Log("Pause Resume command: " + param1);
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_STARTED, param1, param2);
                }
                break;
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_STOP:
                {
                    Log("Stop Player command: " + param1);
                    statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
                    CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED, param1, param2);
                    isStarted = false;
                }
                break;
            case (int)NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_SEEK:
                {
                    Log("Seek Player command: " + param1);
                }
                break;
            default:
                break;
        }
   }

    static void nexPlayerListener(int type, int param1, int param2, int param3)
    {
       Log("[PlayerListener] type:" + type + " param1:" + param1 + " param2:" + param2 + " param3:" + param3);

        switch (type)
       {
           case (int)NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_ASYNC_COMPLETE:
               processOnAsyncCompleteListener(param1, param2, param3, 0, 0);
               break;
           case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_END_OF_CONTENT:
                CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_END_OF_CONTENT, param1, param2);
                processOnEndOfContentListener();
               break;
           case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_UPDATE_CONTENT_INFO:
               processOnUpdateContentInfoListener();
               break;
           case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_TIME:
               processOnTimeListener(param1);
               break;
           case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_BUFFERING:
               processOnBufferingListener(param1, param2);
               break;
           case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_ERROR:
               processOnErrorListener(param1);
               break;
           default:
               break;
       }
   }

    static void processOnTimeListener(int time)
    {
        Log("[processOnTimeListener] Time:" + time);
        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME, time, NEXPLAYERUnity_GetBufferInfo());
        currentPlaybackTime = time;
    }

    static void processOnUpdateContentInfoListener()
    {
        int duration = NEXPLAYERUnity_GetContentInfoInt(1); // Media Duration
        int nVCodec = NEXPLAYERUnity_GetContentInfoInt(2);  // Video Codec
        int nVWidth = NEXPLAYERUnity_GetContentInfoInt(3);  // Video Width
        int nVHeight = NEXPLAYERUnity_GetContentInfoInt(4); // Video Height
        updateVideoSize();
        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TRACK_CHANGED, nVWidth, nVHeight);
        Log("[processOnUpdateContentInfoListener] Video duration: "+duration+", Video Codec:" + nVCodec + " (" + nVWidth + "x" + nVHeight + ")");
    }

    private static void updateVideoSize()
    {
        videoWidth = NEXPLAYERUnity_GetContentInfoInt(3);
        videoHeight = NEXPLAYERUnity_GetContentInfoInt(4);

        Log("updatedVideoSize | videoWidth: " + videoWidth + " videoHeight :" + videoHeight);

        if (videoWidth != 0 && videoHeight != 0 && videoWidth != -1 && videoHeight != -1) 
        {
            Log("Updating size of texture");
            TEX_WIDTH = videoWidth;
            TEX_HEIGHT = videoHeight;

        }
    }

    static void processOnBufferingListener(int type, int progress)
    {
        Log("[processOnBufferingListener] type :" + type + " Buffering : " + progress);
        if (type == 0)
        { // The doc here is wrong about this
            statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_BUFFERING;
            CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_STARTED, type, progress);
        }
        else if (type == 2)
        {
            if (!isPausedBeforeSeek)
                statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
            else
                statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
            CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_ENDED, type, progress);
        }
    }

    static void processOnEndOfContentListener()
    {
        Log("[processOnEndOfContentListener] ");
        NEXPLAYERUnity_Stop(); // Carefull with static. That's why we can't call Stop()
        currentPlaybackTime = 0;
    }

    static void processOnErrorListener(int err)
    {
        Log("[processOnErrorListener] Error :" + err);
        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ERROR, 0, 0);
    }

    private static void Log(string str)
    {
        if (NexPlayerCommon.useExtendedLogs)
            Debug.Log("NexPlayer for Android: " + str);
    }

   /*
     * This is called when the object is set to null and is no longer referenced to anyone
     */
    ~NexPlayerAndroid()
    {
        Log("NativeShutdown");
    }

    public override void ClosePlayback()
    {
        Log("Close()");
        Stop();
        NEXPLAYERUnity_Close();
        GL.IssuePluginEvent(NexPlayerUnityRenderEvent(), (int)NexPlayerUnityEventType.Shutdown);
        Log("NEXPLAYER_EVENT_CLOSED will be called");

        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_CLOSED, 0, 0);
    }
}