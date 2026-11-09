using BepInEx;

namespace TasTool
{
    [BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    [BepInDependency("Blasphemous.ModdingAPI", "3.0.1")]
    [BepInDependency("Blasphemous.Framework.UI", "0.2.0")]
    public class Main : BaseUnityPlugin
    {
        public static TAS TAS { get; private set; }

        private void Start()
        {
            TAS = new TAS();
        }
    }
}
