using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_IOS || UNITY_ANDROID
using UnityEngine.Advertisements;
#endif

public class UnityAdsHelper : MonoBehaviour
{
	public bool enableTestMode = true;
	public bool showInfoLogs;
	public bool showDebugLogs;
	public bool showWarningLogs = true;
	public bool showErrorLogs = true;

	private static Action _handleFinished;
	private static Action _handleSkipped;
	private static Action _handleFailed;
	private static Action _onContinue;

	//--- Unity Ads Setup and Initialization

	Text textMessage;
	void Start()
	{
		textMessage = GameObject.Find("Canvas/Text").GetComponent<Text>();
	}


	public void OnButtonInit()
	{
		InputField field = GameObject.Find("InputField").GetComponent<InputField>();

		string gameId = field.text;

		if (string.IsNullOrEmpty(gameId))
			return;

		InitWithGameID(gameId);

		Destroy(GameObject.Find("ButtonInit"));

	}

	public void OnButtonShow()
	{
		ShowAd();
	}

	private void InitWithGameID(string gameID)
	{
		if (!Advertisement.isSupported)
		{
			Debug.LogWarning("Unity Ads is not supported on the current runtime platform.");
		}
		else if (Advertisement.isInitialized)
		{
			Debug.LogWarning("Unity Ads is already initialized.");
		}
		else if (string.IsNullOrEmpty(gameID))
		{
			Debug.LogError("The game ID value is not set. A valid game ID is required to initialize Unity Ads.");
		}
		else
		{
			Advertisement.debugLevel = Advertisement.DebugLevel.None;
			if (showInfoLogs) Advertisement.debugLevel |= Advertisement.DebugLevel.Info;
			if (showDebugLogs) Advertisement.debugLevel |= Advertisement.DebugLevel.Debug;
			if (showWarningLogs) Advertisement.debugLevel |= Advertisement.DebugLevel.Warning;
			if (showErrorLogs) Advertisement.debugLevel |= Advertisement.DebugLevel.Error;

			if (enableTestMode && !Debug.isDebugBuild)
			{
				Debug.LogWarning("Development Build must be enabled in Build Settings to enable test mode for Unity Ads.");
			}

			bool isTestModeEnabled = Debug.isDebugBuild && enableTestMode;
			Debug.Log(string.Format("Precheck done. Initializing Unity Ads for game ID {0} with test mode {1}...",
									gameID, isTestModeEnabled ? "enabled" : "disabled"));

			Advertisement.Initialize(gameID, isTestModeEnabled);

			StartCoroutine(LogWhenUnityAdsIsInitialized());
		}
	}

#if UNITY_IOS || UNITY_ANDROID

	private IEnumerator LogWhenUnityAdsIsInitialized ()
	{
		float initStartTime = Time.time;

		do
		{
			yield return new WaitForSeconds(0.1f);
			
			textMessage.text = string.Format("Initializing {0:F1} seconds.", Time.time - initStartTime);
			Debug.Log(textMessage.text);

		} while (!Advertisement.isInitialized);

		textMessage.text = string.Format("Initialized in {0:F1} seconds.", Time.time - initStartTime);
		Debug.Log(textMessage.text);

		yield break;
	}
	
	//--- Static Helper Methods

	public static bool isShowing { get { return Advertisement.isShowing; }}
	public static bool isSupported { get { return Advertisement.isSupported; }}
	public static bool isInitialized { get { return Advertisement.isInitialized; }}
	
	public static bool IsReady () 
	{ 
		return IsReady(null); 
	}
	public static bool IsReady (string zoneID) 
	{
		if (string.IsNullOrEmpty(zoneID)) zoneID = null;
		
		return Advertisement.IsReady(zoneID);
	}

	public static void ShowAd () 
	{
		ShowAd(null,null,null,null,null);
	}
	public static void ShowAd (string zoneID) 
	{
		ShowAd(zoneID,null,null,null,null);
	}
	public static void ShowAd (string zoneID, Action handleFinished) 
	{
		ShowAd(zoneID,handleFinished,null,null,null);
	}
	public static void ShowAd (string zoneID, Action handleFinished, Action handleSkipped) 
	{
		ShowAd(zoneID,handleFinished,handleSkipped,null,null);
	}
	public static void ShowAd (string zoneID, Action handleFinished, Action handleSkipped, Action handleFailed) 
	{
		ShowAd(zoneID,handleFinished,handleSkipped,handleFailed,null);
	}
	public static void ShowAd (string zoneID, Action handleFinished, Action handleSkipped, Action handleFailed, Action onContinue)
	{
		if (string.IsNullOrEmpty(zoneID)) zoneID = null;

		_handleFinished = handleFinished;
		_handleSkipped = handleSkipped;
		_handleFailed = handleFailed;
		_onContinue = onContinue;

		if (Advertisement.IsReady(zoneID))
		{
			Debug.Log("Showing ad now...");
			
			ShowOptions options = new ShowOptions();
			options.resultCallback = HandleShowResult;
			//options.pause = true;

			Advertisement.Show(zoneID,options);
		}
		else 
		{
			Debug.LogWarning(string.Format("Unable to show ad. The ad placement zone {0} is not ready.",
			                               object.ReferenceEquals(zoneID,null) ? "default" : zoneID));
		}
	}

	private static void HandleShowResult (ShowResult result)
	{
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log("The ad was successfully shown.");
			if (!object.ReferenceEquals(_handleFinished,null)) _handleFinished();
			break;
		case ShowResult.Skipped:
			Debug.LogWarning("The ad was skipped before reaching the end.");
			if (!object.ReferenceEquals(_handleSkipped,null)) _handleSkipped();
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			if (!object.ReferenceEquals(_handleFailed,null)) _handleFailed();
			break;
		}

		if (!object.ReferenceEquals(_onContinue,null)) _onContinue();
	}

	public void ShowTestAds()
	{
		ShowAd();
	}

#else

	void Start()
	{
		Debug.LogWarning("Unity Ads is not supported under the current build platform.");
	}

	public static bool isShowing { get { return false; } }
	public static bool isSupported { get { return false; } }
	public static bool isInitialized { get { return false; } }

	public static bool IsReady() { return false; }
	public static bool IsReady(string zoneID) { return false; }

	public static void ShowAd()
	{
		Debug.LogError("Failed to show ad. Unity Ads is not supported under the current build platform.");
	}
	public static void ShowAd(string zoneID) { ShowAd(); }
	public static void ShowAd(string zoneID, Action handleFinished) { ShowAd(); }
	public static void ShowAd(string zoneID, Action handleFinished, Action handleSkipped) { ShowAd(); }
	public static void ShowAd(string zoneID, Action handleFinished, Action handleSkipped, Action handleFailed) { ShowAd(); }
	public static void ShowAd(string zoneID, Action handleFinished, Action handleSkipped, Action handleFailed, Action onContinue) { ShowAd(); }

#endif
}