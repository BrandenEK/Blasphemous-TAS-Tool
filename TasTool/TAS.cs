using Framework.Managers;
using Gameplay.UI.Others.UIGameLogic;
using ModdingAPI;
using Rewired;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TasTool
{
    public class TAS : Mod
    {
        public TAS(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }
        
        public bool SpecialInput { get; private set; }

        public FrameState CurrentFrameInput => _lastState;

        private bool _timeFrozen;

        private int _currentFrame;

        private TasMode _currentMode;

        private FrameState _lastState;

        // These are only initialized when recording/playing a tas
        private List<FrameState> _frameStates;
        private Random.State _initialRng;


        protected override void Initialize()
        {
            DisableFileLogging = true;

            if (UnityEngine.Input.GetKey(KeyCode.LeftBracket))
            {
                LogWarning("Starting TAS playback");
                _currentMode = TasMode.Playing;

                // Load framestates list and set rng
                LoadTasFromFile();
                Random.state = _initialRng;

            }
            else if (UnityEngine.Input.GetKey(KeyCode.RightBracket))
            {
                LogWarning("Starting TAS recording");
                _currentMode = TasMode.Recording;

                // Create framestates list and store rng
                _frameStates = new List<FrameState>();
                _initialRng = Random.state;
            }
            else
            {
                _currentMode = TasMode.Nothing;
            }
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            if (_textObjects == null)
                CreateFrameCountText();
        }

        protected override void LateUpdate()
        {
            // Save current input to this framestate
            SpecialInput = true;
            Player input = ReInput.players.GetPlayer(0);
            _lastState = new FrameState();
            for (int i = 5; i < 66; i++)
            {
                _lastState.SetInput(i, input.GetButton(i));
            }
            SpecialInput = false;

            // If recording, store the current state for this frame
            if (_currentMode == TasMode.Recording)
            {
                if (_frameStates.Count <= _currentFrame)
                {
                    _frameStates.Add(_lastState);
                }
                else
                {
                    _frameStates[_currentFrame] = _lastState;
                }
            }
            // If playing, display the input for this frame
            else if (_currentMode == TasMode.Playing)
            {
                if (_frameStates.Count > _currentFrame)
                {
                    _lastState = _frameStates[_currentFrame];
                }
                else
                {
                    _currentMode = TasMode.Nothing;
                }
            }


            // Update text display
            if (_textObjects != null)
            {
                _textObjects[0].text = $"Frame: {_currentFrame}";
                _textObjects[1].text = $"Status: {_currentMode}";
                _textObjects[2].text = $"Input: {_lastState.Input.ToHex()}";
                _textObjects[3].text = $"       {Random.state.s0.ToHex()}:{Random.state.s1.ToHex()}\nRNG: {Random.state.s2.ToHex()}:{Random.state.s3.ToHex()}";
            }

            // If time is not frozen, increase the frame count
            if (Time.timeScale > 0 && !Core.LevelManager.InsideChangeLevel)
            {
                _currentFrame++;
                if (_timeFrozen)
                    FreezeTime();
            }

            // Button to toggle frozen time
            if (UnityEngine.Input.GetKeyDown(KeyCode.Minus))
            {
                if (_timeFrozen) UnfreezeTime();
                else FreezeTime();
            }
            // Button to frame advance
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Equals))
            {
                FrameAdvance();
            }
            // Button to end and save the tas
            else if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                SaveTasToFile();
                _currentMode = TasMode.Nothing;
            }
        }

        private void FreezeTime()
        {
            LogWarning("Freezing time");
            Core.Logic.CurrentLevelConfig.TimeScale = 0;
            Time.timeScale = 0;
            _timeFrozen = true;
        }

        private void UnfreezeTime()
        {
            LogWarning("Unfreezing time");
            Core.Logic.CurrentLevelConfig.TimeScale = 1;
            Time.timeScale = 1;
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
            int totalFrames = (bytes.Length - 16) / 4;
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
                int input = System.BitConverter.ToInt32(bytes, 4 * i + 16);
                _frameStates.Add(new FrameState(input));
            }

            LogWarning($"Loaded {_frameStates.Count} frames from {tasPath}");
        }

        private void SaveTasToFile()
        {
            var bytes = new byte[4 * _frameStates.Count + 16];

            // Add initial rng value
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s0), 0);
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s1), 4);
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s2), 8);
            bytes.SetBytes(System.BitConverter.GetBytes(_initialRng.s3), 12);

            // Add input for each frame
            for (int i = 0; i < _frameStates.Count; i++)
            {
                byte[] input = System.BitConverter.GetBytes(_frameStates[i].Input);
                bytes.SetBytes(input, 4 * i + 16);
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
                rect.anchoredPosition = new Vector2(10f + 150f * i, 57f);
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
