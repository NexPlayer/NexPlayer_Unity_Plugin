using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Possible status of NexPlayer
/// </summary>
public enum NexPlayerStatus
{
	/// <summary>
	/// The player is closed and not initialized
	/// </summary>
    NEXPLAYER_STATUS_CLOSED = 1,

	/// <summary>
	/// The player is opened, but not playing
	/// </summary>
    NEXPLAYER_STATUS_OPENED = 2,

	/// <summary>
	/// The player is playing the video
	/// </summary>
    NEXPLAYER_STATUS_PLAYING = 3,

	/// <summary>
	/// The player is paused
	/// </summary>
    NEXPLAYER_STATUS_PAUSED = 4,

	/// <summary>
	/// The player is buffering new content
	/// </summary>
    NEXPLAYER_STATUS_BUFFERING = 5
}

/// <summary>
/// Possible events that NexPlayer sends to every Action subscribed to the EventNotify OnEvent
/// </summary>
public enum NexPlayerEvent
{
	/// <summary>
	/// The Player has been initialized, but it's not playing 
	/// </summary>
    NEXPLAYER_EVENT_INIT_COMPLEATE = 1,

	/// <summary>
	/// The player has stated playing the video
	/// </summary>
    NEXPLAYER_EVENT_PLAYBACK_STARTED = 2,

	/// <summary>
	/// The player has reached the end of the video playback
	/// </summary>
    NEXPLAYER_EVENT_END_OF_CONTENT = 3,

	/// <summary>
	/// This will be called at regular intervals, and can be useful to update the UI
	/// </summary>
    NEXPLAYER_EVENT_ON_TIME = 4,

	/// <summary>
	/// The player has stated buffering
	/// </summary>
    NEXPLAYER_EVENT_BUFFERING_STARTED = 5,

	/// <summary>
	/// The player has buffered enough content and has resume the playback
	/// </summary>
    NEXPLAYER_EVENT_BUFFERING_ENDED = 6,

	/// <summary>
	/// The internal texture has changed, and NexPlayerBase.GetTexture() should be called to retrieve the texture 
	/// that should be set in all the objects that will display the video
	/// </summary>
    NEXPLAYER_EVENT_TEXTURE_CHANGED = 7,

	/// <summary>
	/// The track of the playback has changed. This is especially useful for protocols with several resolution tracks https://en.wikipedia.org/wiki/Adaptive_bitrate_streaming
	/// </summary>
    NEXPLAYER_EVENT_TRACK_CHANGED = 8,

	/// <summary>
	/// The playback has been paused
	/// </summary>
    NEXPLAYER_EVENT_PLAYBACK_PAUSED = 9,

    /// <summary>
    /// The player has been closed. The Close coroutine can now be stopped and the App can exit gracefully 
    /// </summary>
    NEXPLAYER_EVENT_CLOSED = 10,

    /// <summary>
    /// There was an error in the playback
    /// </summary>
    NEXPLAYER_EVENT_ERROR = 11
}


/// <summary>
/// Possible values of param1 when an error event is generated
/// </summary>
public enum NexPlayerError
{
    /// <summary>
    /// General unknown error 
    /// </summary>
    NEXPLAYER_ERROR_GENERAL = 1,

    /// <summary>
    /// The URI is not supported or not found. Make sure the URI actually exists and check that necessary permissions are set 
    /// (eg. if you are playing a video on Android platform set the Player Setting "Internet Access" to "Required" (streaming videos) and the "Write Permission" to "External" (local videos)
    /// </summary>
    NEXPLAYER_ERROR_SRC_NOT_FOUND = 666
}

/// <summary>
/// Structure of an audio stream
/// </summary>
public struct NexPlayerAudioStream
{
    /// <summary>
    /// id of the audio stream
    /// </summary>
    public int id;

    /// <summary>
    /// Name of the audio stream if available
    /// </summary>
    public string name;

    /// <summary>
    /// Language of the audio stream if available
    /// </summary>
    public string language;
}

/// <summary>
/// Structure of a video track
/// </summary>
public struct NexPlayerTrack
{
    /// <summary>
    /// id of the track
    /// </summary>
    public int id;

    /// <summary>
    /// Bit rate of the track in bits per second
    /// </summary>
    public int bitrate;

    /// <summary>
    /// Width of the track if available
    /// </summary>
    public int width;

    /// <summary>
    /// Height of the track if available
    /// </summary>
    public int height;
}

/// <summary>
/// Use this delegate to receive events from NexPlayer
/// </summary>
public delegate void EventNotify(NexPlayerEvent paramEvent, int param1, int param2);

/// <summary>
/// NexPlayerBase abstract class that determines the available methods for all the supported platforms. Use this with the NexPlayerFactory.GetNexPlayer();
/// </summary>
public abstract class NexPlayerBase
{
	/// <summary>
	/// The event where registered actions will be notified when there is a player event. Register to this after getting the instance from NexPlayerFactory.GetNexPlayer() and before initializing the player
	/// </summary>
	public EventNotify OnEvent;

	/// <summary>
	/// Initializes the player with an URI, and auto-plays it without the extended logs enabled
	/// </summary>
	/// <param name="URI">URI of the video to play</param>
    abstract public void Init(string URI);

	/// <summary>
	/// Initializes the player with an URI, it set the autoPlay and useExtendedLogs
	/// </summary>
	/// <param name="URI">URI of the video to play</param>
	/// <param name="autoPlay">If set to <c>true</c> auto play.</param>
	/// <param name="useExtendedLogs">If set to <c>true</c> use extended logs.</param>
    abstract public void Init(string URI, bool autoPlay, bool useExtendedLogs);

    /// <summary>
    /// Initializes the player with an URI, it set the autoPlay, useExtendedLogs and a Widevine server Key URI for the DASH content. This method is not available for all the platforms
    /// </summary>
    /// <param name="URI">URI of the video to play</param>
    /// <param name="autoPlay">If set to <c>true</c> auto play.</param>
    /// <param name="useExtendedLogs">If set to <c>true</c> use extended logs.</param>
    /// <param name="KeyServerURI">Widevine Key server URI.</param>
    public virtual void Init(string URI, bool autoPlay, bool useExtendedLogs, string KeyServerURI){ Debug.Log("Unsupported platform"); Init(URI, autoPlay, useExtendedLogs);   }

    /// <summary>
    /// Starts the video playback. Call this after the event NEXPLAYER_EVENT_INIT_COMPLEATE if auto play is disabled
    /// </summary>
    abstract public void StartPlayBack();

	/// <summary>
	/// Stop the playback
	/// </summary>
    abstract public void Stop();

	/// <summary>
	/// Resumes the playback
	/// </summary>
    abstract public void Resume();

	/// <summary>
	/// Pauses the playback
	/// </summary>
    abstract public void Pause();

	/// <summary>
	/// Seeks in the playback, moving the playback to the specified millisecond
	/// </summary>
	/// <param name="milliseconds">Milliseconds where the playback will seek</param>
    abstract public void Seek(int milliseconds);

    [System.Obsolete("CoroutineEndOfTheFame is deprecated because it has a typo, please use CoroutineEndOfTheFrame instead.")]
    public IEnumerator CoroutineEndOfTheFame()
    {
        return CoroutineEndOfTheFrame();
    }

    /// <summary>
    /// Coroutines that needs to be started after player is initialized, and stopped before setting the player to null. Using this in this way it's mandatory and extremely important
    /// </summary>
    /// <returns>The IEnumerator that allows the player properly play the video</returns>
    abstract public IEnumerator CoroutineEndOfTheFrame();

    /// <summary>
    /// This Update method should be called on the Update callback of a Unity MonoBehaviour. Using this in this way it's mandatory and extremely important
    /// </summary>
    public virtual void Update()
	{
		// Not used on all the platforms
	}

	/// <summary>
	/// Returns a Unity Texture that is updated with the video frames. Update this on your Unity Objects when the event NEXPLAYER_EVENT_TEXTURE_CHANGED is triggered 
	/// </summary>
	/// <returns>The texture updated with the video frames</returns>
    abstract public Texture GetTexture();

	/// <summary>
	/// Returns the status player
	/// </summary>
	/// <returns>The status player</returns>
    abstract public NexPlayerStatus GetStatusPlayer();

	/// <summary>
	/// Returns the current time of the playback
	/// </summary>
	/// <returns>The current time of the playback in millisecond</returns>
    abstract public int GetCurrentTime();

	/// <summary>
	/// Returns the total time of the of the playback
	/// </summary>
	/// <returns>The total time of the playback in millisecond</returns>
    abstract public int GetTotalTime();


	/// <summary>
	/// Returns the current height of the video. This changes when the event NEXPLAYER_EVENT_TRACK_CHANGED is triggered
	/// </summary>
	/// <returns>The video height in pixels</returns>
    abstract public int GetVideoHeight();

	/// <summary>
	/// Returns the current width of the video. This changes when the event NEXPLAYER_EVENT_TRACK_CHANGED is triggered
	/// </summary>
	/// <returns>The video width.</returns>
    abstract public int GetVideoWidth();


	/// <summary>
	/// Returns the last millisecond buffered on the player. This is useful to display a seek bar with a secondary progress, indicating the buffered content
	/// </summary>
	/// <returns>The last millisecond buffered.</returns>
    abstract public int GetBufferedEnd();

    /// <summary>
    /// Coroutine that needs to be started to close the player
    /// </summary>
    /// <returns>The IEnumerator that allows the player properly close. The event will NEXPLAYER_EVENT_CLOSED be launched after closing the player</returns>
    [System.Obsolete("Close is deprecated because ClosePlayer is more efficient and it's not a coroutine. This method is only here for backwards compatibility")]
    public virtual IEnumerator Close()
    {
        yield return null;

        ClosePlayback();
    }

    /// <summary>
    /// Close the player playback. The NEXPLAYER_EVENT_CLOSED event will be launched after closing the player
    /// </summary>
    abstract public void ClosePlayback();

    /// <summary>
    /// Returns the current audio stream. This method is not available for all the platforms
    /// </summary>
    /// <returns>The current audio stream. The id will be -1 in case the platform doesn't support it</returns>
    public virtual NexPlayerAudioStream GetCurrentAudioStream() {   Debug.Log("Unsupported platform"); return new NexPlayerAudioStream { id = 0, name = "basic", language = "base" };    }

    /// <summary>
    /// Returns an array of all the available audio streams. This method is not available for all the platforms
    /// </summary>
    /// <returns>An array of all the possible audio streams or null if the platform doesn't support it</returns>
    public virtual NexPlayerAudioStream[] GetAudioStreams() {   Debug.Log("Unsupported platform"); return new NexPlayerAudioStream[0]; }

    /// <summary>
    /// Returns the number of streams
    /// </summary>
    /// <returns>The number of streams</returns>
    public virtual int GetAudioStreamCount() {  Debug.Log("Unsupported platform"); return 0;   }

    /// <summary>
    /// Set a stream to be used. The possible audio streams can be obtained from the method GetAudioStreams. This method is not available for all the platforms
    /// </summary>
    public virtual void SetAudioStream(NexPlayerAudioStream audioStream) {  Debug.Log("Unsupported platform"); }

    /// <summary>
    /// Returns information about the current track. This method is not available for all the platforms
    /// </summary>
    /// <returns>Information about the current track. The id will be -1 in case the platform doesn't support it</returns>
    public virtual NexPlayerTrack GetCurrentTrack() {   Debug.Log("Unsupported platform"); return new NexPlayerTrack {   id = 0, bitrate = 0, width = 0, height = 0 };   }

    /// <summary>
    /// Returns information about all the possible tracks. This method is not available for all the platforms
    /// </summary>
    /// <returns>Information about all the possible tracks or null if the platform doesn't support it</returns>
    public virtual NexPlayerTrack[] GetTracks() {   Debug.Log("Unsupported platform"); return new NexPlayerTrack[0]; }

    /// <summary>
    /// Returns the number of tracks
    /// </summary>
    /// <returns>The number of tracks</returns>
    public virtual int GetTracksCount() {   Debug.Log("Unsupported platform");    return 0;   }

    /// <summary>
    /// Set a tack to be used. Using this disables ABR. The possible tracks can be obtained from the method GetTracks. This method is not available for all the platforms
    /// </summary>
    public virtual void SetTrack(NexPlayerTrack trackToUse) {   Debug.Log("Unsupported platform");  }
}
