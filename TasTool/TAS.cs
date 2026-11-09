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
using Blasphemous.Framework.UI;

namespace TasTool
{
    public class TAS : BlasMod
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetAsyncKeyState(int vKey);

        private const int PAUSE_KEY = 0x21;
        private const int ADVANCE_KEY = 0x22;

        public TAS() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }
        
        private bool _timeFrozen;
        private long _lastAdvanceTime = System.DateTime.Now.Ticks;

        private TasMode _currentMode;

        protected override void OnInitialize()
        {
            _currentMode = TasMode.Nothing;
        }

        protected override void OnLateUpdate()
        {
            // Update text display
            if (_textDisplay == null)
                CreateTextDisplay();

            _textDisplay.text = $"Frame: {Time.frameCount}";

            // If time is not frozen, increase the frame count
            if (Time.timeScale > 0 && !Core.LevelManager.InsideChangeLevel && _currentMode != TasMode.Nothing)
            {
                //_currentFrame++;
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
            }
        }

        private void FreezeTime()
        {
            ModLog.Warn("Freezing time");
            _timeFrozen = true;
        }

        private void UnfreezeTime()
        {
            ModLog.Warn("Unfreezing time");
            _timeFrozen = false;
        }

        private void FrameAdvance()
        {
            UnfreezeTime();
            _timeFrozen = true;
        }

        #region UI Display

        private Text[] _textObjects;

        private Text _textDisplay;

        private void CreateTextDisplay()
        {
            //GameObject textObject = null;
            //foreach (PlayerPurgePoints obj in Object.FindObjectsOfType<PlayerPurgePoints>())
            //{
            //    if (obj.name == "PurgePoints")
            //    {
            //        textObject = obj.transform.GetChild(1).gameObject;
            //        break;
            //    }
            //}

            //Transform parent = null;
            //foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
            //{
            //    if (canvas.name == "Game UI")
            //    {
            //        parent = canvas.transform;
            //        break;
            //    }
            //}

            //if (textObject == null || parent == null)
            //    return;

            _textDisplay = UIModder.Create(new RectCreationOptions()
            {
                XRange = Vector2.zero,
                YRange = Vector2.zero,
                Name = "TAS display",
                Parent = UIModder.Parents.CanvasHighRes,
                Pivot = Vector2.zero,
                Position = new Vector2(-15, -15),
                Size = new Vector2(300, 60),
            }).AddText(new TextCreationOptions()
            {
                Alignment = TextAnchor.LowerLeft,
                Contents = "Test txt"
            });

            //_textObjects = new Text[4];
            //for (int i = 0; i < _textObjects.Length; i++)
            //{
            //    GameObject newText = Object.Instantiate(textObject, parent);
            //    newText.name = "TasText";
            //    newText.SetActive(true);

            //    RectTransform rect = newText.GetComponent<RectTransform>();
            //    rect.anchorMin = new Vector2(0f, 0f);
            //    rect.anchorMax = new Vector2(0f, 0f);
            //    rect.pivot = new Vector2(0f, 1f);
            //    rect.anchoredPosition = new Vector2(10f + 150f * i, 90f);
            //    rect.sizeDelta = new Vector2(250f, 50f);

            //    Text text = newText.GetComponent<Text>();
            //    text.color = Color.white;
            //    text.text = string.Empty;
            //    text.alignment = TextAnchor.LowerLeft;
            //    text.horizontalOverflow = HorizontalWrapMode.Wrap;

            //    _textObjects[i] = text;
            //}

            ModLog.Info("Created text display");
        }

        #endregion UI Display
    }
}
