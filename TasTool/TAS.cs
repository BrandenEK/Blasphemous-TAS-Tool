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
        private long _lastPauseTime = System.DateTime.Now.Ticks;
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

            if (_timeFrozen)
            {
                while (true)
                {
                    Thread.Sleep(10);

                    // Check for unfreeze input
                    if ((System.DateTime.Now.Ticks - _lastPauseTime > 2_000_000) && (GetAsyncKeyState(PAUSE_KEY) & 0x8000) > 0)
                    {
                        UnfreezeTime();
                        break;
                    }

                    // Check for advance input
                    if ((System.DateTime.Now.Ticks - _lastAdvanceTime > 2_000_000) && (GetAsyncKeyState(ADVANCE_KEY) & 0x8000) > 0)
                    {
                        AdvanceTime();
                        break;
                    }
                }
            }
            else
            {
                // Check for freeze input
                if (Input.GetKeyDown(KeyCode.Minus))
                    FreezeTime();
            }
        }

        private void FreezeTime()
        {
            ModLog.Warn("Freezing time");
            _timeFrozen = true;
            _lastPauseTime = System.DateTime.Now.Ticks;
        }

        private void UnfreezeTime()
        {
            ModLog.Warn("Unfreezing time");
            _timeFrozen = false;
            _lastPauseTime = System.DateTime.Now.Ticks;
        }

        private void AdvanceTime()
        {
            ModLog.Warn($"Advancing time");
            _lastAdvanceTime = System.DateTime.Now.Ticks;
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
