
namespace TasTool
{
    public static class Extensions
    {
        public static string ToHex(this int num)
        {
            return num.ToString("X").PadLeft(8, '0');
        }
    }
}
