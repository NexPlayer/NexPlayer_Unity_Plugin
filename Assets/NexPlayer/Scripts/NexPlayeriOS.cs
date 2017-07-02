using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using AOT;
using UnityEngine;

class NexPlayeriOS: NexPlayerCommon 
{

    /**
	 * \brief Get current video size
	 *
	 * This API is only for iOS. It retrieves the current video width and height of the
	 * content which is used in Texture2D.CreateExternalTexture method
	 *
	 * @param w width of the video.
	 *
	 * @param h height of the video.
	 *
	 * @author NexStreaming
	 *
	 */
	[DllImport ("__Internal")]
	protected static extern void NEXPLAYERUnity_VideoSize(ref int w, ref int h);

	/**
	 * \brief Get texture for current video frame
	 *
	 * This API is only for iOS. It retrieves the current texture ID for video 
	 * frame which is used in Texture2D.CreateExternalTexture method
	 *
	 * @Return retrieved current texture id
	 *
	 *
	 * @author NexStreaming
	 *
	 */
	[DllImport ("__Internal")]
	protected static extern System.IntPtr NEXPLAYERUnity_CurFrameTexture();

	/**
	 * 
	 * It provides the last millisecond buffered (current time + buffered time).
	 *
	 * @Return total duration of the buffer
	 * 
	 * @author NexStreaming
	 */
	[DllImport ("__Internal")]
	protected static extern int NEXPLAYERUnity_GetBufferedEndTime();

    private static bool videoMatTexAssigned;
    private Texture2D _videoTexture = null;

    private static int currentTimeVideo;
	private static int totalPlaybackTime;

	
    public override void Init(string URI)
	{
		Init(URI, true, false);
	}

	public override void Init(string URI, bool autoPlay, bool useExtendedLogs)
	{
		setupPlayer(URI, autoPlay, useExtendedLogs, null);

	}

	public override void Init(string URI, bool autoPlay, bool useExtendedLogs, string  keyServerURI)
    {
        setupPlayer(URI, autoPlay, useExtendedLogs, keyServerURI);

    }

	private void setupPlayer(string URI, bool autoPlay, bool useExtendedLogs , string  keyServerURI) 
	{
		bool useAssetPlay = false;
        if(URI.Trim().StartsWith("http") || URI.Trim().StartsWith("https://") )
            useAssetPlay = false;
         else
            useAssetPlay = true; 
		NexPlayerCommon.URI = URI;
		NexPlayerCommon.autoPlay = autoPlay;
		NexPlayerCommon.useExtendedLogs = useExtendedLogs;
		currentTimeVideo = 0;
		totalPlaybackTime = 0;
		videoMatTexAssigned = false;
		NexPlayerCommon.OnEventStatic = this.OnEvent;
		statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;

		if (NexPlayeriOS.useExtendedLogs)
			NEXPLAYERUnity_SetLogLevel ((int) NexPlayer_LOG_LEVEL.NEXPLAYER_LOG_LEVEL_ALL);

		SetTextureFromUnity (IntPtr.Zero, 1, 1);

		setPlayerListener(new nexPlayerDelegate( nexPlayerListener ));

		if (keyServerURI != null)
			NEXPLAYERUnity_SetKeyServerUri(keyServerURI);

		if(useAssetPlay)
			NEXPLAYERUnity_OpenFD(NexPlayerCommon.URI);
		else
			NEXPLAYERUnity_Open (NexPlayerCommon.URI);
		
	}

    public override void Update()
	{
		
		if (!videoMatTexAssigned && videoTexture)
		{
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, 0, 0);

			videoMatTexAssigned = true;
		}
		else if (videoMatTexAssigned && !videoTexture)
		{
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_TEXTURE_CHANGED, 0, 0);

			videoMatTexAssigned = false;
		}

	}

	public override void Resume()
	{
		Log ("Resume called");

		// This represents properly that it's buffering
		statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_BUFFERING;
		CallOnEvent (NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_STARTED, 0, 0);

		StartPlayBack ();
	}

	public override void Pause()
	{
		Log ("Pause called");
		// In iOS when you go te App goes to the background the playback needs to STOP and release all resouces of the hardware decoder, otherwise it will crash. 
		// This is a retriction controlled by Apple
		Stop();
	}

    public override Texture GetTexture()
	{
		return _videoTexture;
	}

	public override int GetCurrentTime()
	{
		return currentTimeVideo;
	}

	public override int GetTotalTime()
	{
		return totalPlaybackTime;
	}

	public override int GetVideoHeight()
	{
		return videoHeight;
	}

	public override int GetVideoWidth()
	{
		return videoWidth;
	}

    [MonoPInvokeCallback(typeof(nexPlayerDelegate))]
	static void nexPlayerListener(int type, int param1, int param2, int param3, int param4, int param5) {

		switch (type) {
		case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_ASYNC_COMPLETE:
			processOnAsyncCompleteListener(param1, param2, param3, param4, param5);
			break;
		case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_END_OF_CONTENT:
			processOnEndOfContentListener ();
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_END_OF_CONTENT, param1, param2);
			break;
		case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_UPDATE_CONTENT_INFO:
			processOnUpdateContentInfoListener();
			CallOnEvent (NexPlayerEvent.NEXPLAYER_EVENT_TRACK_CHANGED, param1, param2);

			break;

		case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_TIME:
			processOnTimeListener(param1);
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ON_TIME, param1, param2);

			break;
		case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_BUFFERING:
			if (param1 == 0) { // The doc here is wrong about this
				statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_BUFFERING;
				CallOnEvent (NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_STARTED, param1, param2);
			} else if (param1 == 2) {
				statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;
				CallOnEvent (NexPlayerEvent.NEXPLAYER_EVENT_BUFFERING_ENDED, param1, param2);
			}

			break;
		case (int) NexPlayer_EVENT_TYPE.NEXPLAYER_EVENT_ERROR:
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_ERROR, param1, param2);

			break;
		default:
			break;
		}
	}

    static void processOnAsyncCompleteListener(int command, int param1, int param2, int param3, int param4) {

		switch (command) 
		{
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_OPEN_LOCAL:
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_OPEN_STREAMING:

			statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_OPENED;

			if (autoPlay)
				NEXPLAYERUnity_Start (currentTimeVideo);
			
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_INIT_COMPLEATE, param1, param2);

			break;
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_STOP:

			statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED, param1, param2);

			break;
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_SEEK:
			break;
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_PAUSE:
    
			statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PAUSED;
			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_PAUSED, param1, param2);

			break;
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_START_LOCAL:
		case (int) NexPlayerASYNC_EVENT_TYPE.NEXPLAYER_ASYNC_CMD_START_STREAMING:

			processOnUpdateContentInfoListener ();
			statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_PLAYING;

			CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_PLAYBACK_STARTED, param1, param2);

			break;
		default:
			break;

		}
	}

    static void processOnTimeListener(int time) {
		currentTimeVideo = time;
	}

	static void processOnEndOfContentListener() {

		NEXPLAYERUnity_Stop (); // Carefull with static. That's why we can't call Stop()
		currentTimeVideo = 0;
	}

	static void processOnUpdateContentInfoListener() {

		int duration = NEXPLAYERUnity_GetContentInfoInt (1); // Media Duration
		int nVCodec = NEXPLAYERUnity_GetContentInfoInt (2);	// Video Codec
		int nVWidth = NEXPLAYERUnity_GetContentInfoInt (3);	// Video Width
		int nVHeight = NEXPLAYERUnity_GetContentInfoInt (4);	// Video Height

		totalPlaybackTime = duration;
		videoMatTexAssigned = false;

		Log("[processOnUpdateContentInfoListener] Video Codec:"+ nVCodec + " (" + nVWidth + "x" + nVHeight + "). Duration: "+ duration);
	}

    private int videoWidth
	{
		get
		{
			int w = 0, h = 0;
			NEXPLAYERUnity_VideoSize(ref w, ref h);
			return w;
		}
	}

	private int videoHeight
	{
		get
		{
			int w = 0, h = 0;
			NEXPLAYERUnity_VideoSize(ref w, ref h);
			return h;
		}
	}
    private Texture2D videoTexture
	{
		get
		{
			System.IntPtr nativeTex = NEXPLAYERUnity_CurFrameTexture();
			if (nativeTex != System.IntPtr.Zero)
			{
				if (_videoTexture == null || videoWidth != _videoTexture.width || videoHeight != _videoTexture.height)
				{
					_videoTexture = Texture2D.CreateExternalTexture(videoWidth, videoHeight, TextureFormat.BGRA32, false, false, System.IntPtr.Zero);
					_videoTexture.filterMode = FilterMode.Point;
					_videoTexture.Apply ();
				}
				if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal) 
				{
					if(nativeTex != System.IntPtr.Zero) 
					{
						_videoTexture.UpdateExternalTexture(nativeTex);
					} 
				}
				else 
				{
					_videoTexture.UpdateExternalTexture(nativeTex);
				}

			} 
			else 
			{
				_videoTexture = null;	
			}	

			return _videoTexture;
		}
	}

    private static void Log(string str)
    {
        if (NexPlayerCommon.useExtendedLogs)
            Debug.Log("NexPlayer for iOS: " + str);
    }

	~NexPlayeriOS()
	{
		if (statusPlayer == NexPlayerStatus.NEXPLAYER_STATUS_CLOSED) 
		{
			ClosePlayback ();
		}
	}

	public override void ClosePlayback()
	{
		Stop();
		NEXPLAYERUnity_Close();
		statusPlayer = NexPlayerStatus.NEXPLAYER_STATUS_CLOSED;
		Log("NEXPLAYER_EVENT_CLOSED will be called");

		CallOnEvent(NexPlayerEvent.NEXPLAYER_EVENT_CLOSED, 0, 0);
	}

	public override IEnumerator CoroutineEndOfTheFrame()
	{
		yield return null;
	}

	public override int GetBufferedEnd()
	{
		return NEXPLAYERUnity_GetBufferedEndTime();
	}
}