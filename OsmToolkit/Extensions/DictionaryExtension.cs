namespace OsmToolkit.Extensions
{
    internal static class DictionaryExtension
    {
        public static string ToTagString(this IDictionary<string, string>? tags, bool multiline = false)
        {

            if (tags == null || tags.Count == 0)
            {
                return string.Empty;
            }

            var seperator = multiline ? Environment.NewLine : ", ";
            return string.Join(seperator, tags.Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
