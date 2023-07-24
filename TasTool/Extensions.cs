
namespace TasTool
{
    public static class Extensions
    {
        public static string ToHex(this int num)
        {
            return num.ToString("X").PadLeft(8, '0');
        }

        public static void SetBytes(this byte[] arr, byte[] bytes, int startIdx)
        {
            if (startIdx + bytes.Length > arr.Length)
                throw new System.ArgumentException("The bytes to add would exceed this array's length");

            for (int i = 0; i < bytes.Length; i++)
            {
                arr[startIdx + i] = bytes[i];
            }
        }
    }
}
