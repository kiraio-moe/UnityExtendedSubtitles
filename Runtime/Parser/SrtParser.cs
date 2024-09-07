using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kiraio.UniXSub.Parser
{
    /// <summary>
    /// Provide a parser for .srt file.
    /// </summary>
    public static class SrtParser
    {
        /// <summary>
        /// Parse .srt <paramref name="subtitleTexts"/>.
        /// References:
        /// - https://en.wikipedia.org/wiki/SubRip#Format
        /// - https://wiki.videolan.org/SubRip/#Format
        /// - https://matroska.org/technical/subtitles.html
        /// - https://ale5000.altervista.org/subtitles.htm
        /// </summary>
        /// <param name="subtitleTexts"></param>
        /// <returns></returns>
        public static List<Subtitle> ParseSrt(string subtitleTexts)
        {
            List<Subtitle> subtitles = new List<Subtitle>();

            try
            {
                // Add empty subtitle at first index
                subtitles.Add(Subtitle.Empty);

                // Split the subtitle file into blocks by double new lines
                string[] blocks = subtitleTexts.Split(
                    new[] { "\r\n\r\n", "\n\n", "\r\r" },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (string block in blocks)
                {
                    // Regex pattern to match SRT block with optional position tags
                    Match match = Regex.Match(
                        block,
                        @"(\d+)\r?\n(\d{2}:\d{2}:\d{2},\d{3}) --> (\d{2}:\d{2}:\d{2},\d{3})(?:\sX1:(-?\d+)\sX2:(-?\d+)\sY1:(-?\d+)\sY2:(-?\d+))?\r?\n([\s\S]+)",
                        RegexOptions.Multiline
                    );

                    if (match.Success)
                    {
                        int index = int.Parse(match.Groups[1].Value); // Index
                        TimeSpan start = TimeSpan.ParseExact(
                            match.Groups[2].Value,
                            @"hh\:mm\:ss\,fff",
                            CultureInfo.InvariantCulture
                        ); // Start time
                        TimeSpan end = TimeSpan.ParseExact(
                            match.Groups[3].Value,
                            @"hh\:mm\:ss\,fff",
                            CultureInfo.InvariantCulture
                        ); // End time
                        string text = FixFormatting(match.Groups[8].Value.Trim()); // Text

                        // Parse the position if available, otherwise set to Vector4.zero
                        float x1 = !string.IsNullOrEmpty(match.Groups[4].Value)
                            ? float.Parse(match.Groups[4].Value)
                            : 0;
                        float x2 = !string.IsNullOrEmpty(match.Groups[5].Value)
                            ? float.Parse(match.Groups[5].Value)
                            : 0;
                        float y1 = !string.IsNullOrEmpty(match.Groups[6].Value)
                            ? float.Parse(match.Groups[6].Value)
                            : 0;
                        float y2 = !string.IsNullOrEmpty(match.Groups[7].Value)
                            ? float.Parse(match.Groups[7].Value)
                            : 0;

                        Vector4 position = new Vector4(x1, y1, x2, y2);

                        subtitles.Add(new Subtitle(index, start, end, position, text));
                    }
                    else
                    {
                        Debug.LogError($"Invalid subtitle block: {block}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse SRT file: {ex.Message}");
            }

            return subtitles;
        }

        /// <summary>
        /// Replace formatting tags with TMP-compatible tags
        /// References:
        /// - https://en.wikipedia.org/wiki/SubRip#Markup
        /// - https://wiki.videolan.org/SubRip/#Extensions
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static string FixFormatting(string text)
        {
            text = Regex.Replace(text, @"{(\w)}", "<$1>");
            text = Regex.Replace(text, @"{(/\w)}", "<$1>");
            text = Regex.Replace(
                text,
                @"(?:(?:{|<)font)\s?(?:color=\x22(.*?)\x22)\s?(?:face=\x22(.*?)\x22)?\s*(?:}|>)([\s\S]*?)(?:(?:{|<)\/font(?:}|>))",
                match =>
                {
                    string color = string.IsNullOrEmpty(match.Groups[1].Value)
                        ? "#ffffff"
                        : match.Groups[1].Value;
                    string face = string.IsNullOrEmpty(match.Groups[2].Value)
                        ? "default"
                        : match.Groups[2].Value;

                    return $"<font=\"{face}\"><color={color}>{match.Groups[3].Value}</color></font>";
                }
            );
            return text;
        }
    }
}
