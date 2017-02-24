namespace Firefly.Extensions
{
    public static class LongHumanReadableByte
    {
        public static string ToHumanReadableBytes(this long number)
        {
            string[] sizes = {"B", "KB", "MB", "GB"};
            var len = (double) number;
            var order = 0;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1024;
            }

            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }
}