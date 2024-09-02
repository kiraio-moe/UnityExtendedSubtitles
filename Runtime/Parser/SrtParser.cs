using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kiraio.UniXSub.Parser
{
    public static class SrtParser
    {
        public static List<Subtitle> ParseSrt(string subtitleFile)
        {
            var subtitles = new List<Subtitle>();

            try
            {
                // Split the subtitle file into blocks by double new lines
                string[] blocks = subtitleFile.Split(
                    new[] { "\r\n\r\n", "\n\n", "\r\r" },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (string block in blocks)
                {
                    // Regex pattern to match SRT block
                    Match match = Regex.Match(
                        block,
                        @"(\d+)\r?\n(\d{2}:\d{2}:\d{2},\d{3}) --> (\d{2}:\d{2}:\d{2},\d{3})\r?\n([\s\S]+)",
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
                        string text = FixFormatting(match.Groups[4].Value.Trim()); // Text

                        // Set position to Vector2.zero for SRT
                        Vector2 xy1 = Vector2.zero;
                        Vector2 xy2 = Vector2.zero;

                        subtitles.Add(new Subtitle(index, start, end, xy1, xy2, text));
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
        /// Fixes the formatting of the text
        /// </summary>
        private static string FixFormatting(string text)
        {
            // replacing the { and } with < and > so that the text can be parsed by the TextMeshPro component
            text = Regex.Replace(text, @"{(\w)}", "<$1>");
            text = Regex.Replace(text, @"{(/\w)}", "<$1>");
            text = Regex.Replace(
                text,
                @"<font color=\x22(.*)\x22>([\S\s]+)</font>",
                "<color=$1>$2</color>"
            );
            return text;
        }
    }
}
