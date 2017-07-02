using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Helper class that provides information about the supported graphic APIs, and helps select the compatible ones.
/// </summary>
[CustomEditor(typeof(NexEditorHelper))]
[CanEditMultipleObjects]
public class NexCustomEditor : Editor {

	private bool isAPIsValid;
	private bool isPlatformSupported;

	void OnEnable()
	{
		isPlatformSupported = NexPlayerFactory.IsPlatformSupported();
		isAPIsValid = NexPlayerFactory.AreGraphicsAPIsSupported ();
	}

    private bool IsInternetAccessEnabled()
    {

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        bool usedInternetPermission = true;
        switch (target)
        {
            case BuildTarget.Android: usedInternetPermission = PlayerSettings.Android.forceInternetPermission; break;
        }

        return usedInternetPermission;
    }

    private bool IsHTTPAllowed()
    {

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        bool allowHTTP = true;
        switch (target)
        {
            case BuildTarget.iOS: allowHTTP = PlayerSettings.iOS.allowHTTPDownload; break;
        }

        return allowHTTP;
    }

	private bool IsiOS8OrAbove()
	{

		BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

		bool IsiOS8OrAbove = true;
		switch (target)
		{
			case BuildTarget.iOS:
			{
				#if UNITY_5_5_OR_NEWER
					string str = PlayerSettings.iOS.targetOSVersionString.Split ('.')[0];
					int value = int.Parse (str);
					IsiOS8OrAbove = value >= 8;
				#else
					IsiOS8OrAbove = PlayerSettings.iOS.targetOSVersion >= iOSTargetOSVersion.iOS_8_0;
				#endif
			}
			break;
		}

		return IsiOS8OrAbove;
	}

    public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (!isPlatformSupported) {
			EditorGUILayout.HelpBox("Platform not supported !", MessageType.Error);
		}
		
		else {
			if (isAPIsValid)
				EditorGUILayout.HelpBox ("All the Graphics APIs are properly set", MessageType.Info);
			else {
				EditorGUILayout.HelpBox ("There are invalid Graphics APIs. Changed them to use NexPlayer", MessageType.Warning);

				if (GUILayout.Button ("Select valid Graphics APIs")) {
					
					Debug.Log ("Selecting only the valid Graphics APIs");

					NexPlayerFactory.FixGraphicsAPIsSupported ();
					isAPIsValid = NexPlayerFactory.AreGraphicsAPIsSupported ();
				}
			}

            if (!IsInternetAccessEnabled())
            {
                if (GUILayout.Button("Set Internet access to required (needed for HTTP request)"))
                {
                    PlayerSettings.Android.forceInternetPermission = true;
                }
            }

            if (!IsHTTPAllowed())
            {
                if (GUILayout.Button("Enable HTTP connections (in addition to HTTPS)"))
                {
                    PlayerSettings.iOS.allowHTTPDownload = true;
                }
            }

			if (!IsiOS8OrAbove())
			{
				if (GUILayout.Button("Set the minimum target version to iOS 8.0"))
				{
					#if UNITY_5_5_OR_NEWER
						PlayerSettings.iOS.targetOSVersionString = "8.0";
					#else
						PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.iOS_8_0;
					#endif
				}
			}
        }
	}
}
