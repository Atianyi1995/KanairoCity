using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Newtonsoft.Json.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Kanairo.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace player2_sdk
{
    [Serializable]
    public class Function
    {
        [Tooltip(
            "The name of the function, used by the LLM to call this function, so try to keep it short and to the point")]
        public string name;

        [Tooltip("A short description of the function, used for explaining to the LLM what this function does")]
        public string description;

        public List<FunctionArgument> functionArguments;

        [Tooltip("If true, this function will never respond with a message when called")]
        public bool neverRespondWithMessage;

        public SerializableFunction ToSerializableFunction()
        {
            var props = new Dictionary<string, SerializedArguments>();

            for (var i = 0; i < functionArguments.Count; i++)
            {
                var arg = functionArguments[i];
                props[arg.argumentName] = new SerializedArguments
                {
                    type = arg.argumentType,
                    description = arg.argumentDescription
                };
            }

            NpcManager.Log(props);
            return new SerializableFunction
            {
                name = name,
                description = description,
                parameters = new Parameters
                {
                    Properties = props,
                    required = functionArguments.FindAll(arg => arg.required).ConvertAll(arg => arg.argumentName)
                },
                neverRespondWithMessage = neverRespondWithMessage
            };
        }
    }


    [Serializable]
    public class FunctionArgument
    {
        public string argumentName;
        public string argumentType;
        public string argumentDescription;
        public bool required;
    }


    [Serializable]
    public class NpcApiChatResponse
    {
        public string npc_id;
        public string message;
        public AudioData audio;
        public List<FunctionCallData> command;
    }

    [Serializable]
    public class AudioData
    {
        public string data;
    }

    [Serializable]
    public class FunctionCallData
    {
        public string name;
        public JObject arguments;

        public FunctionCall ToFunctionCall(GameObject npc)
        {
            return new FunctionCall
            {
                name = name,
                arguments = arguments,
                npc = npc
            };
        }
    }

    [Serializable]
    public class FunctionCall
    {
        public string name;
        public JObject arguments;
        public GameObject npc;

        // Compatibility for older scripts
        public GameObject aiObject => npc;

        public T GetArgument<T>(string key)
        {
            if (arguments != null && arguments.TryGetValue(key, out var val))
            {
                return val.ToObject<T>();
            }

            return default;
        }
    }

    public class NpcManager : MonoBehaviour
    {
        public static NpcManager Instance { get; private set; }

        public static void Log(object message)
        {
            if (Instance != null && Instance.showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if (Instance != null && Instance.showDebugLogs)
            {
                Debug.LogWarning(message);
            }
        }

        public static void LogError(object message)
        {
            if (Instance != null && Instance.showDebugLogs)
            {
                Debug.LogError(message);
            }
        }

        private const string BaseUrl = "https://api.player2.game/v1";

        [Header("Config")]
        [SerializeField]
        [Tooltip(
            "The Client ID is used to identify your game. It can be acquired from the Player2 Developer Dashboard")]
        public string clientId;

        [SerializeField]
        [Tooltip(
            "If true, the NPCs will use Text-to-Speech (TTS) to speak their responses. Requires a valid voice_id in the tts.voice_ids configuration.")]
        public bool TTS;

        [SerializeField]
        [Tooltip("If true, the NPCs will keep track of game state information in the conversation history.")]
        public bool keepGameState;

        [SerializeField]
        [Tooltip("If true, debug logs will be shown in the console.")]
        public bool showDebugLogs = true;

        [Header("Functions")] [SerializeField] public List<Function> functions;


        [SerializeField]
        [Tooltip(
            "This event is triggered when a function call is received from the NPC. See the `ExampleFunctionHandler` script for how to handle these calls.")]
        public UnityEvent<FunctionCall> functionHandler;

        [SerializeField]
        [Tooltip("This event is triggered when the NPC Manager is fully initialized and authenticated.")]
        public UnityEvent onInitialized;

        public readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        private Player2NpcResponseListener _responseListener;

        [NonSerialized] public UnityEvent apiTokenReady = new();

        [NonSerialized] public UnityEvent<string> NewApiKey = new();

        [NonSerialized] public UnityEvent spawnNpcs = new();

        public string ApiKey { get; private set; }


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                LogWarning("Multiple NpcManager instances found in the scene.");
            }

            Log("=== NpcManager.Awake: Starting initialization ===");
            Log($"NpcManager.Awake: Platform: {Application.platform}");

#if UNITY_EDITOR
            Log("NpcManager.Awake: Running in Unity Editor");
            // Set insecure HTTP option in editor only if needed
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
            Log("NpcManager.Awake: Running in WebGL build (not editor)");
            // For WebGL builds, we'll handle certificate validation differently
            // This is set at runtime, not in PlayerSettings
#endif

            // Log domain detection status early
            var isOnPlayer2Game = IsWebGLAndOnPlayer2GameDomain();
            Log($"NpcManager.Awake: On player2.game domain: {isOnPlayer2Game}");
            Log($"NpcManager.Awake: Base URL will be: {GetBaseUrl()}");
            Log($"NpcManager.Awake: Will skip authentication: {ShouldSkipAuthentication()}");

            if (string.IsNullOrEmpty(clientId))
            {
                LogError("NpcManager requires a Client ID to be set.");
                return;
            }

            _responseListener = gameObject.GetComponent<Player2NpcResponseListener>();
            if (_responseListener == null)
            {
                LogError(
                    "Player2NpcResponseListener component not found on NPC Manager GameObject. Please attach it in the editor.");
                return;
            }

            _responseListener.JsonSerializerSettings = JsonSerializerSettings;
            _responseListener._baseUrl = GetBaseUrl();

            _responseListener.SetReconnectionSettings(5, 2.5f);

            NewApiKey.AddListener(async apiKey =>
            {
                Log("NpcManager.NewApiKey listener: Received API key");
                ApiKey = apiKey;
                Log("NpcManager.NewApiKey listener: API key set");

                // For WebGL on player2.game domain, pass empty API key to skip auth headers
                var skipAuth = ShouldSkipAuthentication();
                var apiKeyForListener = skipAuth ? "" : apiKey;
                Log($"NpcManager.NewApiKey listener: Skip authentication: {skipAuth}");
                Log($"NpcManager.NewApiKey listener: Base URL: {GetBaseUrl()}");
                Log(
                    $"NpcManager.NewApiKey listener: Passing to response listener: {(string.IsNullOrEmpty(apiKeyForListener) ? "empty (skipping auth)" : "API key")}");

                // Set the API key on the response listener
                _responseListener.newApiKey.Invoke(apiKeyForListener);

                // Wait for the response listener to actually be connected before signaling ready
                await WaitForResponseListenerReady();

                // Skip health check if authentication was bypassed (hosted scenario)
                if (skipAuth && string.IsNullOrEmpty(apiKey))
                {
                    Log(
                        "NpcManager.NewApiKey listener: Authentication bypassed for hosted scenario, skipping health check");
                    apiTokenReady.Invoke();
                }
                else
                {
                    // Verify token works with health check before signaling ready
                    Log("NpcManager.NewApiKey listener: Response listener connected, performing health check...");
                    var healthCheckPassed = await TokenValidator.ValidateTokenAsync(apiKey, this);

                    if (healthCheckPassed)
                    {
                        Log("NpcManager.NewApiKey listener: Health check passed, signaling API token ready");
                        apiTokenReady.Invoke();
                    }
                    else
                    {
                        LogError(
                            "NpcManager.NewApiKey listener: Health check failed, token is not working properly. Not signaling ready.");
                    }
                }
            });

            // Listen for when the authentication system signals it's fully ready
            apiTokenReady.AddListener(() =>
            {
                Log("NpcManager.apiTokenReady listener: Authentication fully complete, spawning NPCs");
                spawnNpcs.Invoke();
                Log("NpcManager.apiTokenReady listener: spawnNpcs invoked");
                
                // Signal that initialization is complete
                onInitialized?.Invoke();
                Log("NpcManager.apiTokenReady listener: onInitialized invoked");
            });

            Log($"NpcManager initialized with clientId: {clientId}");

            // Automatically start authentication if not already started
            StartCoroutine(AutoStartAuthentication());
        }

        private void OnDestroy()
        {
            if (_responseListener != null) _responseListener.StopListening();
        }


        private void OnValidate()
        {
            if (string.IsNullOrEmpty(clientId))
            {
                LogError("NpcManager requires a Game ID to be set.");
                
            }
        }

        public List<SerializableFunction> GetSerializableFunctions()
        {
            var serializableFunctions = new List<SerializableFunction>();
            foreach (var function in functions) serializableFunctions.Add(function.ToSerializableFunction());
            if (serializableFunctions.Count > 0) return serializableFunctions;

            return null;
        }

        private string _cachedBaseUrl;

        public string GetBaseUrl()
        {
            if (!string.IsNullOrEmpty(_cachedBaseUrl))
            {
                return _cachedBaseUrl;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                var overrideUrl = GetLocalStorageItem("player2_api_base_url");
                if (!string.IsNullOrEmpty(overrideUrl))
                {
                    Log($"NpcManager.GetBaseUrl: Using override: {overrideUrl}");
                    _cachedBaseUrl = overrideUrl;
                    return _cachedBaseUrl;
                }
            }
            catch (Exception ex)
            {
                LogWarning($"NpcManager.GetBaseUrl: Failed to read localStorage: {ex.Message}");
            }
#endif

            Log($"NpcManager.GetBaseUrl: Using standard API URL: {BaseUrl}");
            _cachedBaseUrl = BaseUrl;
            return _cachedBaseUrl;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetLocalStorageItem(string key);
#endif

        /// <summary>
        ///     Check if authentication should be skipped (WebGL on player2.game domain)
        /// </summary>
        public bool ShouldSkipAuthentication()
        {
            var shouldSkip = IsWebGLAndOnPlayer2GameDomain();
            Log($"NpcManager.ShouldSkipAuthentication: {shouldSkip}");
            return shouldSkip;
        }

        /// <summary>
        ///     Check if we're running in WebGL and on player2.game domain
        /// </summary>
        private bool IsWebGLAndOnPlayer2GameDomain()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Log("IsWebGLAndOnPlayer2GameDomain: Running in WebGL build (not editor)");
            try
            {
                // Use Unity's built-in Application.absoluteURL for reliable URL detection
                string absoluteUrl = Application.absoluteURL;
                Log($"IsWebGLAndOnPlayer2GameDomain: Retrieved absolute URL: '{absoluteUrl}'");
                
                if (string.IsNullOrEmpty(absoluteUrl))
                {
                    LogWarning("IsWebGLAndOnPlayer2GameDomain: Application.absoluteURL is null or empty");
                    return false;
                }
                
                // Parse the URL to get the host
                System.Uri uri = new System.Uri(absoluteUrl);
                string host = uri.Host;
                Log($"IsWebGLAndOnPlayer2GameDomain: Parsed host: '{host}'");
                
                bool isPlayer2Game = host.Equals("player2.game", StringComparison.OrdinalIgnoreCase) || 
                                     host.EndsWith(".player2.game", StringComparison.OrdinalIgnoreCase);
                Log($"IsWebGLAndOnPlayer2GameDomain: Is legitimate player2.game domain: {isPlayer2Game}");
                Log($"IsWebGLAndOnPlayer2GameDomain: Final result: {isPlayer2Game}");
                
                return isPlayer2Game;
            }
            catch (Exception ex)
            {
                LogWarning($"IsWebGLAndOnPlayer2GameDomain: Failed to detect WebGL domain: {ex.Message}");
                LogWarning($"IsWebGLAndOnPlayer2GameDomain: Stack trace: {ex.StackTrace}");
                return false;
            }
#else
            Log(
                "IsWebGLAndOnPlayer2GameDomain: Not running in WebGL build (editor or other platform), returning false");
            return false;
#endif
        }

        private async Awaitable WaitForResponseListenerReady()
        {
            if (_responseListener == null) return;

            // Wait for the response listener to actually establish its connection
            var attempts = 0;
            const int maxAttempts = 50; // 5 seconds max (50 * 100ms)

            while (!_responseListener.IsListening && attempts < maxAttempts)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (!_responseListener.IsListening)
                LogWarning("Response listener failed to connect within timeout, proceeding anyway");
            else
                Log($"Response listener connected after {attempts * 100}ms");
        }

        private IEnumerator AutoStartAuthentication()
        {
            // Wait a few frames for other components to initialize
            yield return new WaitForSecondsRealtime(0.1f);

            // Check if AuthenticationUI already exists
            var existingAuth = FindObjectOfType<AuthenticationUI>();
            if (existingAuth != null)
            {
                Debug.Log("NpcManager.AutoStartAuthentication: AuthenticationUI already exists, not auto-creating");
                yield break;
            }

            // Auto-setup authentication
           // Debug.Log("NpcManager.AutoStartAuthentication: No AuthenticationUI found, auto-creating one");
            AuthenticationUI.Setup(this);
        }


        public void RegisterNpc(string id, TextMeshProUGUI onNpcResponse, GameObject npcObject)
        {
            if (_responseListener == null)
            {
                LogError("Response listener is null! Cannot register NPC.");
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                LogError("Cannot register NPC with empty ID");
                return;
            }

            var uiAttached = onNpcResponse != null;
            if (!uiAttached)
                LogWarning(
                    $"Registering NPC {id} without a TextMeshProUGUI target; responses will not display in UI.");

            Log($"Registering NPC with ID: {id}");

            var onNpcApiResponse = new UnityEvent<NpcApiChatResponse>();
            onNpcApiResponse.AddListener(response =>
                HandleNpcApiResponse(id, response, uiAttached, onNpcResponse, npcObject));

            _responseListener.RegisterNpc(id, onNpcApiResponse);

            // Ensure listener is running after registering
            if (!_responseListener.IsListening)
            {
                if (showDebugLogs) Debug.Log("Listener was not running, starting it now");
                _responseListener.StartListening();
            }
        }

        private void HandleNpcApiResponse(string id, NpcApiChatResponse response, bool uiAttached,
            TextMeshProUGUI onNpcResponse, GameObject npcObject)
        {
            try
            {
                if (response == null)
                {
                    Debug.LogWarning($"Received null response object for NPC {id}");
                    return;
                }

                if (npcObject == null)
                {
                    Debug.LogWarning($"NPC object is null for NPC {id}");
                    return;
                }

                if (!string.IsNullOrEmpty(response.message))
                {
                    if (uiAttached && onNpcResponse != null)
                    {
                        // Stop thinking animation before displaying message
                        var player2Npc = npcObject.GetComponent<Player2Npc>();
                        if (player2Npc != null)
                        {
                            player2Npc.StopThinkingAnimation();
                        }

                        Debug.Log($"Updating UI for NPC {id}: {response.message}");
                        onNpcResponse.text = response.message;
                    }
                    else
                    {
                        Debug.Log($"(No UI) NPC {id} message: {response.message}");
                    }

                    // --- PERSUASION LOGIC START ---
                    // Apply trust changes based on the AI's response to the player's message
                    if (npcObject != null)
                    {
                        var votingProfile = npcObject.GetComponent<NPCVotingProfile>();
                        if (votingProfile != null)
                        {
                            float trustChange = VotingLogic.CalculateTrustFromAIResponse(response.message);
                            if (Mathf.Abs(trustChange) > 0.01f)
                            {
                                votingProfile.AddPlayerTrust(trustChange);
                                // Update campaign totals
                                if (CampaignManager.Instance != null)
                                    CampaignManager.Instance.RefreshApprovalTotals();
                                
                                Debug.Log($"[Reaction Persuasion] AI response was {(trustChange > 0 ? "positive" : "negative")}! Trust changed by {trustChange}. New Trust: {votingProfile.trustInPlayer}");
                            }
                        }
                    }
                    // --- PERSUASION LOGIC END ---
                }

                if (response.audio != null && !string.IsNullOrEmpty(response.audio.data))
                {
                    // Log detailed audio data information for troubleshooting
                    var audioDataPreview = response.audio.data.Length > 100
                        ? response.audio.data.Substring(0, 100) + "..."
                        : response.audio.data;
                    NpcManager.Log(
                        $"NPC {id} - Audio data received: Length={response.audio.data.Length}, Preview={audioDataPreview}");

                    // Check if NPC GameObject has AudioSource, add if needed
                    var audioSource = npcObject.GetComponent<AudioSource>();
                    if (audioSource == null) audioSource = npcObject.AddComponent<AudioSource>();

                    // IMPORTANT: Stop any currently playing clip before starting the new one
                    // This prevents multiple audio clips from overlapping if the server sends them rapidly
                    if (audioSource.isPlaying)
                    {
                        audioSource.Stop();
                        NpcManager.Log($"NPC {id} - Stopped previous audio playback to start new clip");
                    }

                    // Start coroutine to decode and play audio using platform-specific implementation
                    var audioPlayer = AudioPlayerFactory.GetAudioPlayer();
                    StartCoroutine(audioPlayer.PlayAudioFromDataUrl(response.audio.data, audioSource, id));
                }

                if (response.command == null || response.command.Count == 0) return;

                foreach (var functionCall in response.command)
                    try
                    {
                        var call = functionCall.ToFunctionCall(npcObject);
                        functionHandler?.Invoke(call);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"Error invoking function call '{functionCall?.name}' for NPC {id}: {ex.Message}");
                    }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unhandled exception processing response for NPC {id}: {ex.Message}");
            }
        }


        public void UnregisterNpc(string id)
        {
            if (_responseListener != null) _responseListener.UnregisterNpc(id);
        }

        public bool IsListenerActive()
        {
            return _responseListener != null && _responseListener.IsListening;
        }

        public void StartListener()
        {
            if (_responseListener != null) _responseListener.StartListening();
        }

        public void StopListener()
        {
            if (_responseListener != null) _responseListener.StopListening();
        }

        // Add this method for debugging
        [ContextMenu("Debug Listener Status")]
        public void DebugListenerStatus()
        {
            if (_responseListener == null)
                Debug.Log("Response listener is NULL");
            else
                Debug.Log(
                    $"Response listener status: IsListening={_responseListener.IsListening}");
        }
    }

    [Serializable]
    public class SerializableFunction
    {
        public string name;
        public string description;
        public Parameters parameters;
        public bool neverRespondWithMessage;
    }

    [Serializable]
    public class Parameters
    {
        public List<string> required;
        public string type = "object";
        public Dictionary<string, SerializedArguments> Properties { get; set; }
    }

    [Serializable]
    public class SerializedArguments
    {
        public string type;
        public string description;
    }
}