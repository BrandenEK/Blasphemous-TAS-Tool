using BepInEx;

namespace TasTool
{
    [BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
    [BepInDependency("Blasphemous.ModdingAPI", "2.1.0")]
    public class Main : BaseUnityPlugin
    {
        public static TAS TAS { get; private set; }

        private void Start()
        {
            TAS = new TAS();
        }
    }
}
