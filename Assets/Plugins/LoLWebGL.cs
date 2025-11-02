using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

namespace LoLSDK
{
#if UNITY_EDITOR
    public interface ILOLSDK_EDITOR : ILOLSDK_EXTENSION
    {
        void SpeakText(string text, Action<AudioClip> onDownloaded, MonoBehaviour owner, string currentLang = "en", string currentTTSLangKey = "en-US");
    }
#endif

    public interface ILOLSDK_EXTENSION : ILOLSDK
    {
        void CancelSpeakText();
    }

    public class WebGL : ILOLSDK_EXTENSION
    {
        public const string SDK_VERSION = "5.2";

        [DllImport("__Internal")]
        private static extern void _PostWindowMessage(string msgName, string jsonPayload);

        public void PostWindowMessage(string msgName, string jsonPayload)
        {
            Debug.Log("PostWindowMessage " + msgName + ": " + jsonPayload);
            _PostWindowMessage(msgName, jsonPayload);
        }

        public void LogMessage(string msg)
        {
            JSONObject payload = new JSONObject { ["msg"] = msg };
            PostWindowMessage("logMessage", payload.ToString());
        }

        [DllImport("__Internal")]
        private static extern string _GameIsReady(string gameName, string callbackGameObject, string aspectRatio, string resolution, string sdkVersion);

        public void GameIsReady(string gameName, string callbackGameObject, string aspectRatio, string resolution)
        {
            _GameIsReady(gameName, callbackGameObject, aspectRatio, resolution, SDK_VERSION);
            Debug.Log($"GameIsReady WebGL - {gameName}, callback: {callbackGameObject}");
        }

        public void CompleteGame()
        {
            PostWindowMessage("complete", "{}");
        }

        public void SubmitProgress(int score, int currentProgress, int maximumProgress = -1)
        {
            JSONObject payload = new JSONObject
            {
                ["score"] = score,
                ["currentProgress"] = currentProgress,
                ["maximumProgress"] = maximumProgress
            };
            PostWindowMessage("progress", payload.ToString());
        }

        public void SubmitAnswer(int questionId, int alternativeId)
        {
            JSONObject payload = new JSONObject
            {
                ["questionId"] = questionId,
                ["alternativeId"] = alternativeId
            };
            PostWindowMessage("answer", payload.ToString());
        }

        public void SpeakText(string key)
        {
            JSONObject payload = new JSONObject { ["key"] = key };
            PostWindowMessage("speakText", payload.ToString());
        }

        public void CancelSpeakText()
        {
            PostWindowMessage("speakTextCancel", "{}");
        }

        public void SpeakQuestion(int questionId)
        {
            JSONObject payload = new JSONObject { ["questionId"] = questionId };
            PostWindowMessage("speakQuestion", payload.ToString());
        }

        public void SpeakAlternative(int alternativeId)
        {
            JSONObject payload = new JSONObject { ["alternativeId"] = alternativeId };
            PostWindowMessage("speakAlternative", payload.ToString());
        }

        public void SpeakQuestionAndAlternatives(int questionId)
        {
            JSONObject payload = new JSONObject { ["questionId"] = questionId };
            PostWindowMessage("speakQuestionAndAlternatives", payload.ToString());
        }

        public void Error(string msg)
        {
            JSONObject payload = new JSONObject { ["msg"] = msg };
            PostWindowMessage("error", payload.ToString());
        }

        public void ShowQuestion()
        {
            PostWindowMessage("showQuestion", "{}");
        }

        public void PlaySound(string file, bool background = false, bool loop = false)
        {
            JSONObject payload = new JSONObject
            {
                ["file"] = file,
                ["background"] = background,
                ["loop"] = loop
            };
            PostWindowMessage("playSound", payload.ToString());
        }

        public void ConfigureSound(float foreground, float background, float fade)
        {
            JSONObject payload = new JSONObject
            {
                ["foreground"] = foreground,
                ["background"] = background,
                ["fade"] = fade
            };
            PostWindowMessage("configureSound", payload.ToString());
        }

        public void StopSound(string file)
        {
            JSONObject payload = new JSONObject { ["file"] = file };
            PostWindowMessage("stopSound", payload.ToString());
        }

        public void SaveState(string data)
        {
            PostWindowMessage("saveState", data);
        }

        public void LoadState()
        {
            PostWindowMessage("loadState", "{}");
        }

        public void GetPlayerActivityId()
        {
            PostWindowMessage("getPlayerActivityId", "{}");
        }
    }

#if UNITY_EDITOR
    public class MockWebGL : ILOLSDK_EDITOR
    {
        private string savedJson;

        public void PostWindowMessage(string msgName, string jsonPayload)
        {
            Debug.Log($"[MockWebGL] {msgName}: {jsonPayload}");
        }

        public void LogMessage(string msg)
        {
            Debug.Log("[MockWebGL Log] " + msg);
        }

        public void sError(string msg)
        {
            Debug.LogError("[MockWebGL Error] " + msg);
        }

        public void CompleteGame()
        {
            Debug.Log("[MockWebGL] CompleteGame called");
        }

        public void GameIsReady(string gameName, string callbackGameObject, string aspectRatio, string resolution)
        {
            Debug.Log($"[MockWebGL] GameIsReady Editor - {gameName}, callback: {callbackGameObject}");
        }

        public void SubmitProgress(int score, int currentProgress, int maximumProgress = -1)
        {
            Debug.Log($"[MockWebGL] SubmitProgress - Score: {score}, Progress: {currentProgress}/{maximumProgress}");
        }

        public void SubmitAnswer(int questionId, int alternativeId)
        {
            Debug.Log($"[MockWebGL] SubmitAnswer - QID: {questionId}, AltID: {alternativeId}");
        }

        public void ShowQuestion()
        {
            Debug.Log("[MockWebGL] ShowQuestion called");
        }

        public void SpeakText(string key)
        {
            Debug.Log("[MockWebGL] SpeakText: " + key);
        }

        public void CancelSpeakText()
        {
            Debug.Log("[MockWebGL] CancelSpeakText called");
        }

        public void SpeakText(string text, Action<AudioClip> onDownloaded, MonoBehaviour owner, string currentLang = "en", string currentTTSLangKey = "en-US")
        {
            Debug.Log("[MockWebGL] SpeakText with callback: " + text);
            onDownloaded?.Invoke(null); // Devuelve null en modo Editor
        }

        public void SpeakQuestion(int questionId)
        {
            Debug.Log("[MockWebGL] SpeakQuestion - QID: " + questionId);
        }

        public void SpeakAlternative(int alternativeId)
        {
            Debug.Log("[MockWebGL] SpeakAlternative - AltID: " + alternativeId);
        }

        public void SpeakQuestionAndAlternatives(int questionId)
        {
            Debug.Log("[MockWebGL] SpeakQuestionAndAlternatives - QID: " + questionId);
        }

        public void ConfigureSound(float a, float b, float c)
        {
            Debug.Log($"[MockWebGL] ConfigureSound - Foreground: {a}, Background: {b}, Fade: {c}");
        }

        public void PlaySound(string path, bool background, bool loop)
        {
            Debug.Log($"[MockWebGL] PlaySound - {path}, Background: {background}, Loop: {loop}");
        }

        public void StopSound(string path)
        {
            Debug.Log("[MockWebGL] StopSound - " + path);
        }

        public void SaveState(string data)
        {
            savedJson = data;
            Debug.Log("[MockWebGL] SaveState called: " + data);
        }

        public void LoadState()
        {
            Debug.Log("[MockWebGL] LoadState called, returning saved data");
            if (!string.IsNullOrEmpty(savedJson))
            {
                Debug.Log("[MockWebGL] Loaded data: " + savedJson);
            }
            else
            {
                Debug.Log("[MockWebGL] No saved data found");
            }
        }

        public void GetPlayerActivityId()
        {
            Debug.Log("[MockWebGL] GetPlayerActivityId called");
        }
    }
#endif
}
