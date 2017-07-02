using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public abstract class NexPlayerCommon: NexPlayerBase {

	private const int NEXPLAYER_UNITY_SDK_VERSION_MAJOR = 1;
	private const int NEXPLAYER_UNITY_SDK_VERSION_MINOR = 1;
	private const int NEXPLAYER_UNITY_SDK_VERSION_PATCH = 0;
	private const int NEXPLAYER_UNITY_SDK_VERSION_BUILD = 2;

	/**
	 * \brief prototype of NexPlayer listener callback
	 *
	 * C# delegate method (Similar to C/C++ function pointer)
	 *
	 * @param type		event type. This value can be one onf <i>NexPlayer_EVENT_TYPE</i>.
	 * @param param1	In case of ASYNC_COMPLETE type NexPlayer
	 *					Command can be one of the following param1 <i>NexPlayerASYNC_EVENT_TYPE</i>
	 *					In Case of NEXPLAYER_EVENT_TIME. It will be the current Time in Millisec.
	 * @param param2	A value specific to the command that has completed.
	 * @param param3	Reserved for future use.
	 * @param param4	Reserved for future use
	 * @param param5	Reserved for future use
	 *
	 * @author NexStreaming
	 *
	 */
	protected delegate void nexPlayerDelegate( int type, int param1, int param2, int param3, int param4, int param5 );

	/**
	 * \brief Pass Unity Texture to NexPlayer
	 *
	 * This function provides Texture2D Id to NexPlayer along with width and
	 * height parameters.
	 * NexPlayer decode video frame and display this decoded frame to this Texture.
	 *
	 * @param texture	Texture2D Id (Created in Unity)
	 * @param w			Width of the Texture
	 * @param h			Height of the Texture
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

/**
	 * \brief Pass Updated Texture to NexPlayer
	 *
	 * This function provides new  Texture2D Id to NexPlayer along with width and
	 * height parameters.
	 * NexPlayer decode video frame and display this decoded frame to this Texture.
	 *
	 * @param texture	Texture2D Id (Created in Unity)
	 * @param w			Width of the Texture
	 * @param h			Height of the Texture
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void UpdateTextureFromUnity(System.IntPtr texture, int w, int h);
	/**
	 * \brief Return render event handling function pointer
	 *
	 * In order to do any rendering from the Unity plugin, we call
	 * GL.IssuePluginEvent from Unity c# script. This will cause the
	 * NexPlayer native function to be called from render thread. We use
	 * NexPlayerUnityRenderEvent for this purpose with following three
	 * states defined by <i>NexPlayerUnityEventType</i> 
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern IntPtr NexPlayerUnityRenderEvent();

	/**
	 * \brief Register NexPlayerListener.
	 *
	 * This method registers nexPlayerListener for receiving callbacks from
	 * NexPlayer to Unity. We should call the following method during
	 * Initialization of NexPlayer
	 *
	 * @param  l	nexPlayer listener callback function
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif    
	protected static extern void setPlayerListener(nexPlayerDelegate l);	

	/**
	 * \brief Open content 
	 *
	 * This method begins opening the media at the specified URL. This
	 * supports streaming contents. This is asynchronous operation that will
	 * run in background. When this operations completes, nexPlayerDelegate is
	 * called with one of the following constants (param1)
	 * <ul>
	 * <li> NEXPLAYER_ASYNC_CMD_OPEN_LOCAL </li>
	 * <li> NEXPLAYER_ASYNC_CMD_OPEN_STREAMING </li>
	 * </ul>
	 *
	 * @param url	The URL for remote content or local content path
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Open (String url);

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_OpenFD (String fileName);	

	/**
	 * \bried Close content
	 *
	 * This method ends all the work on the content currently open and closes
	 * content data. The content must be stopped before calling this method.
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Close ();	

	/**
	 * \brief Start playback
	 *
	 * Starts playing media. The media must have already been successfully
	 * opened with NEXPLAYERUnity_Open. When this operation completes,
	 * nexPlayerDelegate is called with one of the following command
	 * constants (param1).
	 * <ul>
	 * <li>NEXPLAYER_ASYNC_CMD_START_LOCAL</li>
	 * <li>NEXPLAYER_ASYNC_CMD_START_STREAMING</li>
	 * </ul>
	 *
	 * @param msec	Playing start time in Millisec
	 *
	 * @author NexStreaming
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Start (int msec);

	/**
	 * \brief Stop playback
	 *
	 * This function stops the current playback.
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Stop();

	/**
	 * \brief Seek playback
	 *
	 * This function seeks the playback position exactly to the specific time.
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Seek(int msec);	


	/**
	 * \brief Pause playback
	 * 
	 * This method pauses the current playback. During the pause state, we
	 * should stop rendering Frames too and hence we stop calling
	 * GL.IssuePluginEvent
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Pause();

	/**
	 * \brief Resume playback
	 *
	 * This method resumes playback beginning at the point at which player
	 * was last paused. It also starts rendering the NexPlayer frames again
	 * there by calling GL.IssuePluginEvent
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_Resume();

	/**
	 * \brief Get current content information
	 *
	 * It is same API as getContentInfoInt of NexPlayer. It retrieves the specific
	 * content information item.
	 *
	 * @param info_index 	Content info indices (Refer to NexPlayer official documentation)
	 *
	 * @Return  retrieved content information value.
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern int NEXPLAYERUnity_GetContentInfoInt (int info_index);

	/**
	 * \brief Get the buffer duration
	 *
	 * It is same API as getBufferInfo of NexPlayer. It retrieves the specified
	 * buffer information item (NEXPLAYER_BUFINFO_INDEX_LASTCTS)
	 *
	 *
	 * @Return  retrieved buffer information value.
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern int NEXPLAYERUnity_GetBufferInfo();

	/**
	 * \brief Set Log level
	 *
	 * It sets the log level for NexPlayer.
	 *
	 * @param log_level  This value can be one onf <i>NexPlayer_LOG_LEVEL</i>.
	 * 
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern int NEXPLAYERUnity_SetLogLevel (int log_level);

	/**
	 * \brief Set property
	 *
	 * It sets the log level for NexPlayer.
	 *
	 * @param property  This value can be one onf <i>NexPlayer_Property</i>.
	 * 
	 * @param value 
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern int NEXPLAYERUnity_SetProperty (int property, int value);
	
	/**
	 * \brief Set KeyServerUri
	 *
	 * It sets the Media DRM KeyServerUri
	 *
	 * @param uri 
	 * 
	 *
	 * @author NexStreaming
	 *
	 */
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
	[DllImport("nexplayer_unity")]
#endif
	protected static extern void NEXPLAYERUnity_SetKeyServerUri (String uri);

	//stream API.

	[DllImport("nexplayer_unity")]
	protected static extern void NEXPLAYERUnity_SetAudioStream(int id);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetAudioStreamID(int index);

	[DllImport("nexplayer_unity")]
	protected static extern String NEXPLAYERUnity_GetAudioStreamName(int index);

	[DllImport("nexplayer_unity")]
	protected static extern String NEXPLAYERUnity_GetAudioStreamLanguage(int index);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetAudioStreamCount ();

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetCurrentAudioStreamID();

	[DllImport("nexplayer_unity")]
	protected static extern string NEXPLAYERUnity_GetCurrentAudioStreamName();

	[DllImport("nexplayer_unity")]
	protected static extern String NEXPLAYERUnity_GetCurrentAudioStreamLanguage();

	//track API.

	[DllImport("nexplayer_unity")]
	protected static extern void NEXPLAYERUnity_SetTrack(int bandwidth);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetTrackID(int index);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetTrackBitrate(int index);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetTrackWidth(int index);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetTrackHeight(int index);

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetTrackCount ();

// 	#if UNITY_IPHONE && !UNITY_EDITOR
// 	[DllImport ("__Internal")]
// #else
// 	[DllImport("nexplayer_unity")]
// #endif
// 	protected static extern int NEXPLAYERUnity_GetCurrentTrackID();
	
// 	#if UNITY_IPHONE && !UNITY_EDITOR
// 	[DllImport ("__Internal")]
// #else
// 	[DllImport("nexplayer_unity")]
// #endif
// 	protected static extern int NEXPLAYERUnity_GetCurrentTrackBitrate();

// 	#if UNITY_IPHONE && !UNITY_EDITOR
// 	[DllImport ("__Internal")]
// #else
// 	[DllImport("nexplayer_unity")]
// #endif
// 	protected static extern int NEXPLAYERUnity_GetCurrentTrackWidth();

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_GetCurrentTrackHeight();
	

   	[DllImport("nexplayer_unity")]
	protected static extern bool NEXPLAYERUnity_QueueIsEmpty();

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_AsyncCmdResult();

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_AsyncCmdType();

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_AsyncCmdParam1();

	[DllImport("nexplayer_unity")]
	protected static extern int NEXPLAYERUnity_AsyncCmdValue();

	[DllImport("nexplayer_unity")]
	protected static extern void NEXPLAYERUnity_EnableABR(bool enable);

	[DllImport("nexplayer_unity")]
	protected static extern void NexPlayerUnity_SetOpenGLVersion(bool isOpenGL3);

    protected enum NexPlayer_LOG_LEVEL
    {
        NEXPLAYER_LOG_LEVEL_NONE = 0,
        NEXPLAYER_LOG_LEVEL_DEBUG = 1,
        NEXPLAYER_LOG_LEVEL_RTP = 2,
        NEXPLAYER_LOG_LEVEL_RTCP = 4,
        NEXPLAYER_LOG_LEVEL_FRAME = 8,
        NEXPLAYER_LOG_LEVEL_ALL = 15,
    }

    protected enum NexPlayer_Property
    {
        MAXIMUM_BW = 0,
        MINIMUM_BW = 1,
        AV_SYNC_OFFSET = 2,
        PREFER_LANGUAGE_AUDIO = 3,
        PREFER_BANDWIDTH = 4,
        INITIAL_BUFFERING_DURATION = 5,
        RE_BUFFERING_DURATION = 6,
        ENABLE_TRACKDOWN = 7,
        TRACKDOWN_VIDEO_RATIO = 8,
        TIMESTAMP_DIFFERENCE_VDISP_WAIT = 9,
        TIMESTAMP_DIFFERENCE_VDISP_SKIP = 10,
        START_NEARESTBW = 11
    }

    protected enum NexPlayerUnityEventType
    {
        /** Initialize NexPlayer Instance **/
        Initialize = 0,

        /** Close player and destroy NexPlayer instances **/
        Shutdown = 1,

        /** Render Frame on Render Thread **/
        Update = 2,

        /** Maximum value of event type **/
        Max_EventType
    }

    protected enum NexPlayer_EVENT_TYPE
    {
        /** Complete Asy. NexPlayer API 
		 * param1 : NexPlayerASYNC_EVENT_TYPE
		 * param2 : result
		 */
        NEXPLAYER_EVENT_ASYNC_COMPLETE = 1,
        /** Reached the end of content **/
        NEXPLAYER_EVENT_END_OF_CONTENT = 2,
        /** Content information is updated **/
        NEXPLAYER_EVENT_UPDATE_CONTENT_INFO = 3,
        /** Current playing time is updated 
		 * param1 : current playing time (milisecond)
		 */
        NEXPLAYER_EVENT_TIME = 4,
        /** Buffer state is chaged 
		 * param1 : 1 (buffering begin), 2 (buffering progress), 3(buffering end)
		 * param2 : buffering progress (percent)
		 */
        NEXPLAYER_EVENT_BUFFERING = 5,
        /** Error happen at NexPlayer 
		 * param1 : error code
		 */
        NEXPLAYER_EVENT_ERROR = 6

    }

    public enum NexPlayerASYNC_EVENT_TYPE
    {
        NEXPLAYER_ASYNC_CMD_OPEN_LOCAL = 1,
        NEXPLAYER_ASYNC_CMD_OPEN_STREAMING = 2,
        NEXPLAYER_ASYNC_CMD_START_LOCAL = 5,
        NEXPLAYER_ASYNC_CMD_START_STREAMING = 6,
        NEXPLAYER_ASYNC_CMD_STOP = 8,
        NEXPLAYER_ASYNC_CMD_PAUSE = 9,
        NEXPLAYER_ASYNC_CMD_RESUME = 10,
        NEXPLAYER_ASYNC_CMD_SEEK = 11
    }

    public static string URI = null;
	public static string keyServerURI = null;
    public static bool autoPlay = true;
    public static bool useExtendedLogs;
	public static bool useAssetPlay;

    public static NexPlayerStatus statusPlayer;
    public static EventNotify OnEventStatic;

    public static int currentPlaybackTime = 0;

    public static bool isPausedBeforeSeek = false;
    public static bool isStarted = false;

    public static void CallOnEvent(NexPlayerEvent eventType, int param1, int param2)
    {
        if (OnEventStatic != null)
            OnEventStatic(eventType, param1, param2);
    }

	public override void StartPlayBack()
    {
        NEXPLAYERUnity_Start(currentPlaybackTime);
    }

	
	public override void SetAudioStream(NexPlayerAudioStream audioStream) 
    {
        int id = audioStream.id;
        
        NEXPLAYERUnity_SetAudioStream(id);
    }

	public override void SetTrack(NexPlayerTrack trackToUse) {

		int bitrate = trackToUse.bitrate;
		 NEXPLAYERUnity_SetTrack(bitrate);
		
	}

	public override void Stop()
    {
        NEXPLAYERUnity_Stop();
    }

	public override void Seek(int milliseconds)
    {

        if (0 > milliseconds)
            milliseconds = 0;
        else if (milliseconds > GetTotalTime())
            milliseconds = GetTotalTime();
        currentPlaybackTime = milliseconds; // This will allow seek while the player is stopeed

        NEXPLAYERUnity_Seek(milliseconds);

        CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME, 0, 0); // So it updates the UI when seeking with the player paused
    }

	public override NexPlayerStatus GetStatusPlayer()
	{
		return statusPlayer;
	}


}