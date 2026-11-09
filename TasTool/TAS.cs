using Blasphemous.Framework.UI;
using Blasphemous.ModdingAPI;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace TasTool
{
    public class TAS : BlasMod
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetAsyncKeyState(int vKey);
        const int PAUSE_KEY = 0xBD;
        const int ADVANCE_KEY = 0xBB;

        public TAS() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }
        
        private bool _timeFrozen;
        private long _lastAdvanceTime = System.DateTime.Now.Ticks;

        private Text _textDisplay;

        protected override void OnLevelLoaded(string oldLevel, string newLevel)
        {
            if (_textDisplay == null)
                CreateTextDisplay();
        }

        protected override void OnLateUpdate()
        {
            // Update text display
            _textDisplay?.text = $"Frame: {Time.frameCount}";

            // Check for freeze input
            if (!_timeFrozen && Input.GetKeyDown(KeyCode.Minus))
            {
                FreezeTime();
            }

            // Frozen time loop
            if (_timeFrozen)
            {
                while (true)
                {
                    Thread.Sleep(10);

                    Application.targetFrameRate = Application.targetFrameRate;

                    if ((GetAsyncKeyState(PAUSE_KEY) & 0x8000) > 0)
                    {
                        UnfreezeTime();
                        break;
                    }

                    if ((System.DateTime.Now.Ticks - _lastAdvanceTime > 2_000_000) && (GetAsyncKeyState(ADVANCE_KEY) & 0x8000) > 0)
                    {
                        AdvanceTime();
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

        private void AdvanceTime()
        {
            ModLog.Warn($"Advancing time");
        }

        private void CreateTextDisplay()
        {
            _textDisplay = UIModder.Create(new RectCreationOptions()
            {
                XRange = Vector2.zero,
                YRange = Vector2.zero,
                Name = "TAS display",
                Parent = UIModder.Parents.CanvasHighRes,
                Pivot = Vector2.zero,
                Position = new Vector2(10, 10),
            }).AddText(new TextCreationOptions()
            {
                Alignment = TextAnchor.LowerLeft,
                FontSize = 42,
            });

            ModLog.Info("Created text display");
        }
    }
}
