using Framework.Managers;
using Gameplay.UI.Others.UIGameLogic;
using Blasphemous.ModdingAPI;
using Rewired;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Runtime.InteropServices;

namespace TasTool
{
    public class TAS : BlasMod
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetAsyncKeyState(int vKey);

        private const int PAUSE_KEY = 0x21;
        private const int ADVANCE_KEY = 0x22;

        public TAS() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }
        
        // A flag used to determine whether rewired should actually read input
        public bool SpecialInput { get; private set; }

        private bool _timeFrozen;
        public float PreviousTimeScale { get; private set; } = 1;

        private int _currentFrame;

        private TasMode _currentMode;

        // These control the input for the most recent two frames
        public FrameState CurrentFrameInput => Time.timeScale > 0 ? _currentState : _emptyState;
        public FrameState PreviousFrameInput => Time.timeScale > 0 ? _previousState : _emptyState;
        private FrameState _currentState;
        private FrameState _previousState;
        private FrameState _emptyState;

        // These are only initialized when recording/playing a tas
        private List<FrameState> _frameStates;
        private Random.State _initialRng;

        private Config _config = new();
        public int FrameRate => _config.Fps;

        protected override void OnInitialize()
        {
            _previousState = new FrameState();
            _currentState = new FrameState();
            _emptyState = new FrameState();

            _currentMode = TasMode.Nothing;

            _config = ConfigHandler.Load<Config>();
            ConfigHandler.Save(_config);
            Application.targetFrameRate = _config.Fps;
        }

        protected override void OnLevelLoaded(string oldLevel, string newLevel)
        {
            if (_textObjects == null)
                CreateFrameCountText();
        }

        protected override void OnLateUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftBracket))
            {
                LogWarning("Starting TAS playback");
                _currentMode = TasMode.Playing;

                // Load framestates list and set rng
                LoadTasFromFile();
                Random.InitState(_config.Rng);
                _currentFrame = 0;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightBracket))
            {
                LogWarning("Starting TAS recording");
                _currentMode = TasMode.Recording;

                // Create framestates list and store rng
                _frameStates = new List<FrameState>();
                Random.InitState(_config.Rng);
                _currentFrame = 0;
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Backslash))
            {
                _currentMode = TasMode.Nothing;
            }

            // Save current and previous frame input
            SpecialInput = true;
            Player input = ReInput.players.GetPlayer(0);
            if (Time.timeScale > 0)
                _previousState = _currentState;
            _currentState = new FrameState();
            for (int i = 5; i < 66; i++)
            {
                _currentState.SetInput(i, input.GetButton(i));
            }
            _currentState.SetAxis(0, input.GetAxisRaw(0));
            _currentState.SetAxis(4, input.GetAxisRaw(4));
            SpecialInput = false;

            // If recording, store the current state for this frame
            if (_currentMode == TasMode.Recording)
            {
                if (_frameStates.Count <= _currentFrame)
                {
                    _frameStates.Add(_currentState);
                }
                else
                {
                    _frameStates[_currentFrame] = _currentState;
                }
            }
            // If playing, display the input for this frame
            else if (_currentMode == TasMode.Playing)
            {
                if (_frameStates.Count > _currentFrame)
                {
                    _currentState = _frameStates[_currentFrame];
                }
                else
                {
                    _currentMode = TasMode.Nothing;
                }
            }

            //LogError($"FPS: {Application.targetFrameRate}, DeltaTime: {Time.deltaTime}, TimeScale: {Time.timeScale}");

            // Update text display
            if (_textObjects != null)
            {
                _textObjects[0].text = $"Frame: {_currentFrame}";
                _textObjects[1].text = $"Status: {_currentMode}";
                _textObjects[2].text = $"Input: {_currentState.Input.ToHex()}";
                _textObjects[3].text = $"       {Random.state.s0.ToHex()}:{Random.state.s1.ToHex()}\nRNG: {Random.state.s2.ToHex()}:{Random.state.s3.ToHex()}";
            }

            // If time is not frozen, increase the frame count
            if (Time.timeScale > 0 && !Core.LevelManager.InsideChangeLevel && _currentMode != TasMode.Nothing)
            {
                _currentFrame++;
                if (_timeFrozen)
                    FreezeTime();
            }

            // Button to toggle frozen time
            if (UnityEngine.Input.GetKeyDown(KeyCode.Minus))
            {
                FreezeTime();
                //if (_timeFrozen) UnfreezeTime();
                //else FreezeTime();
            }

            if (_timeFrozen)
            {
                while (true)
                {
                    Thread.Sleep(100);

                    //_textObjects[0].text = $"Frame: {_currentFrame} (Paused)";

                    //LogWarning("Checking input: " + GetAsyncKeyState(PAUSE_KEY));
                    if ((GetAsyncKeyState(PAUSE_KEY) & 0x8000) > 0)
                    {
                        UnfreezeTime();
                        break;
                    }

                    if ((System.DateTime.Now.Ticks - _lastAdvanceTime > 5_000_000) && (GetAsyncKeyState(ADVANCE_KEY) & 0x8000) > 0)
                    {
                        FrameAdvance();
                        _lastAdvanceTime = System.DateTime.Now.Ticks;
                        break;
                    }
                }

                //if (UnityEngine.Input.GetKey(KeyCode.Equals))
                //    UnfreezeTime();

                //Thread.Sleep(1000);
            }

            // Button to frame advance
            //else if (UnityEngine.Input.GetKeyDown(KeyCode.Equals))
            //{
            //    FrameAdvance();
            //}
            // Button to end and save the tas
            else if (UnityEngine.Input.GetKeyDown(KeyCode.F11))
            {
                SaveTasToFile();
                _currentMode = TasMode.Nothing;
            }

            PreviousTimeScale = Time.timeScale;
        }

        private long _lastAdvanceTime = System.DateTime.Now.Ticks;

        private void FreezeTime()
        {
            LogWarning("Freezing time");
            //Core.Logic.CurrentLevelConfig.TimeScale = 0;
            //Time.timeScale = 0;
            _timeFrozen = true;
        }

        private void UnfreezeTime()
        {
            LogWarning("Unfreezing time");
            //Core.Logic.CurrentLevelConfig.TimeScale = 1;
            //Time.timeScale = 1;
            _timeFrozen = false;
        }

        private void FrameAdvance()
        {
            UnfreezeTime();
            _timeFrozen = true;
        }

        #region File I/O

        private readonly string tasPath = System.Environment.CurrentDirectory + "\\blasphemous.tas";

        private void LoadTasFromFile()
        {
            if (!File.Exists(tasPath))
                return;

            byte[] bytes = File.ReadAllBytes(tasPath);
            int totalFrames = (bytes.Length - 16) / 12;
            _frameStates = new List<FrameState>(totalFrames);

            // Get initial rng value
            _initialRng = new Random.State
            {
                s0 = System.BitConverter.ToInt32(bytes, 0),
                s1 = System.BitConverter.ToInt32(bytes, 4),
                s2 = System.BitConverter.ToInt32(bytes, 8),
                s3 = System.BitConverter.ToInt32(bytes, 12)
            };

            // Get input for each frame
            for (int i = 0; i < totalFrames; i++)
            {
                int input = System.BitConverter.ToInt32(bytes, 12 * i + 16);
                var state = new FrameState(input);
                state.SetAxis(0, System.BitConverter.ToSingle(bytes, 12 * i + 20));
                state.SetAxis(4, System.BitConverter.ToSingle(bytes, 12 * i + 24));

                _frameStates.Add(state);
            }

            LogWarning($"Loaded {_frameStates.Count} frames from {tasPath}");
        }

        private void SaveTasToFile()
        {
            var bytes = new byte[12 * _frameStates.Count + 16];

            // Add initial rng value
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s0), 0);
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s1), 4);
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s2), 8);
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s3), 12);

            // Add input for each frame
            for (int i = 0; i < _frameStates.Count; i++)
            {
                byte[] input = System.BitConverter.GetBytes(_frameStates[i].Input);
                bytes.SetBytes(input, 12 * i + 16);

                bytes.SetBytes(System.BitConverter.GetBytes(_frameStates[i].GetAxis(0)), 12 * i + 20);
                bytes.SetBytes(System.BitConverter.GetBytes(_frameStates[i].GetAxis(4)), 12 * i + 24);
            }

            File.WriteAllBytes(tasPath, bytes);
            LogWarning($"Saved {_frameStates.Count} frames to {tasPath}");
        }

        #endregion File I/O

        #region UI Display

        private Text[] _textObjects;

        private void CreateFrameCountText()
        {
            GameObject textObject = null;
            foreach (PlayerPurgePoints obj in Object.FindObjectsOfType<PlayerPurgePoints>())
            {
                if (obj.name == "PurgePoints")
                {
                    textObject = obj.transform.GetChild(1).gameObject;
                    break;
                }
            }

            Transform parent = null;
            foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
            {
                if (canvas.name == "Game UI")
                {
                    parent = canvas.transform;
                    break;
                }
            }

            if (textObject == null || parent == null)
                return;

            _textObjects = new Text[4];
            for (int i = 0; i < _textObjects.Length; i++)
            {
                GameObject newText = Object.Instantiate(textObject, parent);
                newText.name = "TasText";
                newText.SetActive(true);

                RectTransform rect = newText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f + 150f * i, 90f);
                rect.sizeDelta = new Vector2(250f, 50f);

                Text text = newText.GetComponent<Text>();
                text.color = Color.white;
                text.text = string.Empty;
                text.alignment = TextAnchor.LowerLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;

                _textObjects[i] = text;
            }

            Log("Created text objects");
        }

        #endregion UI Display
    }
}
