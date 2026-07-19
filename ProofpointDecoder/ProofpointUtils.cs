using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ProofpointDecoder
{
    /// <summary>
    /// Rewrite of Proofpoint URL Defense decoder (v1, v2, v3).
    /// </summary>
    public static partial class ProofpointUtils
    {
        private static readonly Regex UdPattern = UdRegex();
        private static readonly Regex V1Pattern = V1Regex();
        private static readonly Regex V2Pattern = V2Regex();
        private static readonly Regex V3Pattern = V3Regex();
        private static readonly Regex V3TokenPattern = V3TokenRegex();
        private static readonly Regex V3SingleSlash = V3SingleSlashRegex();

        private static readonly Dictionary<char, int> V3RunMapping = BuildV3RunMapping();

        internal static bool IsProofpointHost(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl)) return false;
            return UdPattern.IsMatch(originalUrl);
        }

        public static string GetProofpointUrl(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl)) return originalUrl;

            var m = UdPattern.Match(originalUrl);
            if (!m.Success) return originalUrl;

            string version = m.Groups[1].Value.ToLowerInvariant();
            return version switch
            {
                "v1" => DecodeV1(originalUrl),
                "v2" => DecodeV2(originalUrl),
                "v3" => DecodeV3(originalUrl),
                _ => originalUrl
            };
        }

        private static string DecodeV1(string originalUrl)
        {
            var m = V1Pattern.Match(originalUrl);
            if (!m.Success) return originalUrl;

            try
            {
                string payload = m.Groups["url"].Value;
                string urlDecoded = HttpUtility.UrlDecode(payload);
                return HttpUtility.HtmlDecode(urlDecoded);
            }
            catch
            {
                return originalUrl;
            }
        }

        private static string DecodeV2(string originalUrl)
        {
            var m = V2Pattern.Match(originalUrl);
            if (!m.Success) return originalUrl;

            try
            {
                string payload = m.Groups["url"].Value;
                string translated = payload.Replace('-', '%').Replace('_', '/');
                string urlDecoded = HttpUtility.UrlDecode(translated);
                return HttpUtility.HtmlDecode(urlDecoded);
            }
            catch
            {
                return originalUrl;
            }
        }

        private static string DecodeV3(string originalUrl)
        {
            var m = V3Pattern.Match(originalUrl);
            if (!m.Success) return originalUrl;

            try
            {
                string embedded = m.Groups["url"].Value;
                string encBytesRaw = m.Groups["enc_bytes"].Value ?? string.Empty;

                var singleSlashMatch = V3SingleSlash.Match(embedded);
                if (singleSlashMatch.Success && singleSlashMatch.Groups.Count >= 3)
                {
                    embedded = singleSlashMatch.Groups[1].Value + "/" + singleSlashMatch.Groups[2].Value;
                }

                string decodedEmbedded = HttpUtility.UrlDecode(embedded);

                if (string.IsNullOrEmpty(encBytesRaw))
                {
                    return decodedEmbedded;
                }

                string base64 = encBytesRaw.Replace('-', '+').Replace('_', '/');
                int pad = (4 - (base64.Length % 4)) % 4;
                if (pad > 0) base64 += new string('=', pad);

                byte[] bytes = Convert.FromBase64String(base64);
                string decBytes = Encoding.UTF8.GetString(bytes);

                var sb = new StringBuilder();
                int lastPos = 0;
                int cursor = 0;
                var tokenMatches = V3TokenPattern.Matches(decodedEmbedded);

                foreach (Match tokenMatch in tokenMatches)
                {
                    sb.Append(decodedEmbedded, lastPos, tokenMatch.Index - lastPos);

                    string token = tokenMatch.Value;
                    if (token == "*")
                    {
                        if (cursor < decBytes.Length)
                        {
                            sb.Append(decBytes[cursor]);
                            cursor++;
                        }
                    }
                    else if (token.StartsWith("**", StringComparison.Ordinal))
                    {
                        char key = token[^1];
                        if (!V3RunMapping.TryGetValue(key, out int runLength))
                        {
                            sb.Append(token);
                        }
                        else
                        {
                            int take = Math.Min(runLength, decBytes.Length - cursor);
                            if (take > 0)
                            {
                                sb.Append(decBytes, cursor, take);
                                cursor += take;
                            }
                        }
                    }
                    else
                    {
                        sb.Append(token);
                    }

                    lastPos = tokenMatch.Index + tokenMatch.Length;
                }

                if (lastPos < decodedEmbedded.Length)
                {
                    sb.Append(decodedEmbedded, lastPos, decodedEmbedded.Length - lastPos);
                }

                return sb.ToString();
            }
            catch
            {
                return originalUrl;
            }
        }

        private static Dictionary<char, int> BuildV3RunMapping()
        {
            var map = new Dictionary<char, int>(128);
            string runValues = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            int runLength = 2;
            foreach (char c in runValues)
            {
                map[c] = runLength;
                runLength++;
            }

            return map;
        }

        [GeneratedRegex(@"https://urldefense(?:\.proofpoint)?\.com/(v[0-9])/", RegexOptions.Compiled)]
        private static partial Regex UdRegex();

        [GeneratedRegex(@"u=(?<url>.+?)&k=", RegexOptions.Compiled)]
        private static partial Regex V1Regex();

        [GeneratedRegex(@"u=(?<url>.+?)&[dc]=", RegexOptions.Compiled)]
        private static partial Regex V2Regex();

        [GeneratedRegex(@"v3/__(?<url>.+?)__;(?<enc_bytes>.*?)!", RegexOptions.Compiled)]
        private static partial Regex V3Regex();

        [GeneratedRegex(@"\*(\*.)?", RegexOptions.Compiled)]
        private static partial Regex V3TokenRegex();

        [GeneratedRegex(@"^([a-z0-9+.-]+:/)([^/].+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex V3SingleSlashRegex();
    }
}
