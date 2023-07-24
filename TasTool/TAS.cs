using Framework.Managers;
using Gameplay.UI.Others.UIGameLogic;
using ModdingAPI;
using Newtonsoft.Json;
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

        private List<FrameState> _frameStates = new();


        protected override void Initialize()
        {
            DisableFileLogging = true;

            if (UnityEngine.Input.GetKey(KeyCode.LeftBracket))
            {
                LogWarning("Starting TAS playback");
                _currentMode = TasMode.Playing;
                LoadTasFromFile();
            }
            else if (UnityEngine.Input.GetKey(KeyCode.RightBracket))
            {
                LogWarning("Starting TAS recording");
                _currentMode = TasMode.Recording;
            }
            else
            {
                _currentMode = TasMode.Nothing;
            }
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            if (_frameCountText == null)
                CreateFrameCountText();
        }

        protected override void LateUpdate()
        {
            SpecialInput = true;
            Player input = ReInput.players.GetPlayer(0);
            _lastState = new FrameState()
            {
                //rng = Random.state,
                aKey = input.GetButton(6),
                bKey = input.GetButton(57),
            };
            SpecialInput = false;

            // If recording, store the current state for this frame
            if (_currentMode == TasMode.Recording)
            {
                if (_frameStates.Count <= _currentFrame)
                    _frameStates.Add(_lastState);
                else
                    _frameStates[_currentFrame] = _lastState;
            }
            // If playing, display the input for this frame
            else if (_currentMode == TasMode.Playing)
            {
                if (_frameStates.Count > _currentFrame)
                {
                    _lastState = _frameStates[_currentFrame];
                    if (_lastState.aKey)
                        LogError("Pressed A on this frame");
                    if (_lastState.bKey)
                        LogError("Pressed B on this frame");
                }
                else
                {
                    _currentMode = TasMode.Nothing;
                }
            }


            // Update frame text
            if (_frameCountText != null)
                _frameCountText.text = $"Frame: {_currentFrame}\t\tStatus: {_currentMode}";

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
            if (File.Exists(tasPath))
            {
                string json = File.ReadAllText(tasPath);
                _frameStates = JsonConvert.DeserializeObject<List<FrameState>>(json);
            }
        }

        private void SaveTasToFile()
        {
            string json = JsonConvert.SerializeObject(_frameStates);
            File.WriteAllText(tasPath, json);
        }

        #endregion File I/O

        #region UI Display

        private Text _frameCountText;

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

            GameObject newText = Object.Instantiate(textObject, parent);
            newText.name = "FrameCountText";
            newText.SetActive(true);

            RectTransform rect = newText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, 20f);
            rect.sizeDelta = new Vector2(250f, 18f);

            Text text = newText.GetComponent<Text>();
            text.color = Color.white;
            text.text = string.Empty;
            text.alignment = TextAnchor.MiddleLeft;

            Log("Created frame count text");
            _frameCountText = text;
        }

        #endregion UI Display
    }
}
