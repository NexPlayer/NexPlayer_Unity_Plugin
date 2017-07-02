using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// NexPlayer factory that creates NexPlayerBase instances.
/// </summary>
public class NexPlayerFactory {
	
	/// <summary>
	/// Creates an instance of NexPlayerBase for the currently selected platform. This allows to use one API for all the supported platforms. 
	/// It's recommended to use this and not instantiate directly a class that extends NexPlayerBase.
	/// </summary>
	/// <returns>A NexPlayerBase instance.</returns>
    public static NexPlayerBase GetNexPlayer () {
        NexPlayerBase player = null;

        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            player = new NexPlayerWindows();
        #endif

        #if !UNITY_EDITOR && UNITY_IOS
	        player = new NexPlayeriOS();
        #endif

        #if !UNITY_EDITOR && UNITY_ANDROID
            player = new NexPlayerAndroid();
        #endif

        #if !UNITY_EDITOR && UNITY_WEBGL
            player = new NexPlayerWebGL();
        #endif

        return player;
    }

#if UNITY_EDITOR

	private static GraphicsDeviceType [] SupportedAPIsWindows = new GraphicsDeviceType[] { GraphicsDeviceType.Direct3D11 };
	private static GraphicsDeviceType [] SupportedAPIsAndroid = new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLES2 };
	private static GraphicsDeviceType [] SupportedAPIsiOS = new GraphicsDeviceType[] { GraphicsDeviceType.Metal, GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.OpenGLES2 };
    private static GraphicsDeviceType[] SupportedAPIsWebGL = new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES2, GraphicsDeviceType.OpenGLES3 };
    private static GraphicsDeviceType[] SupportedAPIsWebGLBelow_5_4 = new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES2 };

    /// <summary>
    /// Determines if the currently selected platform is supported.
    /// </summary>
    /// <returns><c>true</c> if the platform is supported; otherwise, <c>false</c>.</returns>
    public static bool IsPlatformSupported () {

		BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

		return target == BuildTarget.iOS || target == BuildTarget.Android || target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64 || target == BuildTarget.WebGL;
	}

	/// <summary>
	/// Are the graphics APIs used supported.
	/// </summary>
	/// <returns><c>true</c>, if the graphics APIs are supported, <c>false</c> otherwise.</returns>
	public static bool AreGraphicsAPIsSupported () {
		bool APIsSupported = IsPlatformSupported ();
        
        if (APIsSupported) {
			BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

			switch (target) {
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					APIsSupported = AreAPIsSelected (SupportedAPIsWindows, target);
				break;
				case BuildTarget.Android: 
					APIsSupported = AreAPIsSelected (SupportedAPIsAndroid, target);
				break;
				case BuildTarget.iOS: 
					APIsSupported = AreAPIsSelected (SupportedAPIsiOS, target);
                break;
                case BuildTarget.WebGL:
                    if (IsUnityVersionEqualsOrAbove_5_4())
                        APIsSupported = AreAPIsSelected(SupportedAPIsWebGL, target);
                    else
                        APIsSupported = AreAPIsSelected(SupportedAPIsWebGLBelow_5_4, target);
                break;
			}
		}

		return APIsSupported;
	}

    public static bool IsUnityVersionEqualsOrAbove_5_4()
    {
        #if UNITY_5_4_OR_NEWER
            return true;
        #else
            return false;
        #endif
    }

    private static bool AreAPIsSelected(GraphicsDeviceType[] supported, BuildTarget target){
		bool supportedAPI = true;
		GraphicsDeviceType[] selected = PlayerSettings.GetGraphicsAPIs (target);
		if (selected.Length == supported.Length) {
			int i = 0;
			while (supportedAPI && i < selected.Length) {
				// Check this API support
				bool IsInividualAPISupported = false;
				int j = 0;
				GraphicsDeviceType current = selected[i];

				while (!IsInividualAPISupported && j < supported.Length){
					IsInividualAPISupported = (current == supported[j]);
                    j++;
				}

                supportedAPI = IsInividualAPISupported;
                i++;
			}
		} else
			supportedAPI = false;

		return supportedAPI;
	}


	/// <summary>
	/// Fixes the graphics APIs, to select only the supported ones.
	/// </summary>
	public static void FixGraphicsAPIsSupported(){
		if (!AreGraphicsAPIsSupported ()) {
			BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);

            switch (target) {
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					PlayerSettings.SetGraphicsAPIs (target, SupportedAPIsWindows);
					break;
				case BuildTarget.Android: 
					PlayerSettings.SetGraphicsAPIs (target, SupportedAPIsAndroid);
					break;
				case BuildTarget.iOS: 
					PlayerSettings.SetGraphicsAPIs (target, SupportedAPIsiOS);
					break;
                case BuildTarget.WebGL:
                    if (IsUnityVersionEqualsOrAbove_5_4())
                        PlayerSettings.SetGraphicsAPIs(target, SupportedAPIsWebGL);
                    else
                        PlayerSettings.SetGraphicsAPIs(target, SupportedAPIsWebGLBelow_5_4);
                    break;
            }
		}
	}

#endif
    }
