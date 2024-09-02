using System;
using UnityEngine;

namespace Kiraio.UniXSub.Parser
{
    public class Subtitle
    {
        static Subtitle _empty;
        public static Subtitle Empty =>
            _empty ??= new Subtitle(0, TimeSpan.Zero, TimeSpan.Zero, Vector2.zero, Vector2.zero, string.Empty);
        public int Index { get; }
        public TimeSpan Start { get; }
        public TimeSpan End { get; }
        public TimeSpan Duration { get; }
        public Vector2 XY1 { get; } // position on screen
        public Vector2 XY2 { get; }
        public string Text { get; }

        public Subtitle(
            int index,
            TimeSpan start,
            TimeSpan end,
            Vector2 xy1,
            Vector2 xy2,
            string text
        )
        {
            Index = index;
            Start = start;
            End = end;
            Duration = end - start;
            XY1 = xy1;
            XY2 = xy2;
            Text = text;
        }

        public override string ToString()
        {
            return $"Index: {Index}\nText: {Text}\nStart: {Start}\nEnd: {End}\nDuration: {Duration}\nPosition: {XY1} {XY2}";
        }
    }
}
