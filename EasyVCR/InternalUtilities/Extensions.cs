namespace EasyVCR.InternalUtilities
{
    public static class Extensions
    {
        public static bool IsEmptyStringOrNull(this string? str) => str == null || string.IsNullOrWhiteSpace(str);
    }
}