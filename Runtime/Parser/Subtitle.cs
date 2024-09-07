using System;
using UnityEngine;

namespace Kiraio.UniXSub.Parser
{
    public class Subtitle
    {
        static Subtitle _empty;
        public static Subtitle Empty =>
            _empty ??= new Subtitle(0, TimeSpan.Zero, TimeSpan.Zero, Vector4.zero, string.Empty);
        public int Index { get; }
        public TimeSpan Start { get; }
        public TimeSpan End { get; }
        public TimeSpan Duration { get; }
        public Vector4 Position { get; }
        public string Text { get; }

        public Subtitle(int index, TimeSpan start, TimeSpan end, Vector4 position, string text)
        {
            Index = index;
            Start = start;
            End = end;
            Duration = end - start;
            Position = position;
            Text = text;
        }

        public override string ToString()
        {
            return $"Index: {Index}\nText: {Text}\nStart: {Start}\nEnd: {End}\nDuration: {Duration}\nPosition: {Position}";
        }
    }
}
