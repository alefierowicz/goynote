namespace goynote.Helpers
{
    public static class ViewHelpers
    {
        public static string FormatSize(int bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1048576) return $"{bytes / 1024} KB";
            return $"{bytes / 1048576.0:0.#} MB";
        }

        public static string TruncateWords(string input, int wordLimit)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= wordLimit) return input;

            return string.Join(" ", words.Take(wordLimit)) + "...";
        }
    }
}