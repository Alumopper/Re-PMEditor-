using System;
using System.Collections.Generic;

namespace PMEditor.Util
{
    public class EaseFunctions
    {
        private EaseFunctions() { }

        public static Dictionary<string, Func<double, double>> functions = new()
        {
            {"Linear", (t) => t},
            {"SineIn", (t) => (float)Math.Sin(t * Math.PI / 2)},
            {"SineOut", (t) => (float)Math.Sin(t * Math.PI / 2 + Math.PI / 2)},
            {"SineInOut", (t) => (float)(Math.Sin(t * Math.PI - Math.PI / 2) / 2 + 0.5)},
            {"QuadIn", (t) => t * t},
            {"QuadOut", (t) => 1 - (1 - t) * (1 - t)},
            {"QuadInOut", (t) => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2},
            {"CubicIn", (t) => t * t * t},
            {"CubicOut", (t) => 1 - Math.Pow(1 - t, 3)},
            {"CubicInOut", (t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2},
            {"QuartIn", (t) => t * t * t * t},
            {"QuartOut", (t) => 1 - Math.Pow(1 - t, 4)},
            {"QuartInOut", (t) => t < 0.5 ? 8 * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 4) / 2},
            {"QuintIn", (t) => t * t * t * t * t},
            {"QuintOut", (t) => 1 - Math.Pow(1 - t, 5)},
            {"QuintInOut", (t) => t < 0.5 ? 16 * t * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 5) / 2},
            {"ExpoIn", (t) => (float)Math.Pow(2, 10 * (t - 1))},
            {"ExpoOut", (t) => (float)(1 - Math.Pow(2, -10 * t))},
            {"ExpoInOut", (t) => t < 0.5 ? (float)(Math.Pow(2, 10 * (2 * t - 1)) / 2) : (float)(2 - Math.Pow(2, -10 * (2 * t - 1)) / 2)},
            {"CircIn", (t) => (float)(1 - Math.Sqrt(1 - t * t))},
            {"CircOut", (t) => (float)Math.Sqrt(1 - Math.Pow(t - 1, 2))},
            {"CircInOut", (t) => t < 0.5 ? (float)(0.5 * (1 - Math.Sqrt(1 - 4 * t * t))) : (float)(0.5 * (Math.Sqrt(-((2 * t - 3) * (2 * t - 1))) + 1))},
            {"BackIn", (t) => (float)(t * t * ((1.70158 + 1) * t - 1.70158))},
            {"BackOut", (t) => (float)(1 - Math.Pow(1 - t, 2) * ((1.70158 + 1) * (1 - t) - 1.70158))},
            {"BackInOut", (t) => t < 0.5 ? (float)(0.5 * (Math.Pow(2 * t, 2) * (((1.70158 * 1.525) + 1) * 2 * t - (1.70158 * 1.525)))) : (float)(0.5 * (Math.Pow(2 * t - 2, 2) * (((1.70158 * 1.525) + 1) * (t * 2 - 2) + (1.70158 * 1.525)) + 2))},
            {"ElasticIn", (t) => (float)(Math.Sin(13 * Math.PI / 2 * t) * Math.Pow(2, 10 * (t - 1)))},
            {"ElasticOut", (t) => (float)(Math.Sin(-13 * Math.PI / 2 * (t + 1)) * Math.Pow(2, -10 * t) + 1)},
            {"ElasticInOut", (t) => t < 0.5 ? (float)(0.5 * (Math.Sin(13 * Math.PI / 2 * (2 * t)) * Math.Pow(2, 10 * ((2 * t) - 1)) - 1)) : (float)(0.5 * (Math.Sin(-13 * Math.PI / 2 * ((2 * t - 1) + 1)) * Math.Pow(2, -10 * (2 * t - 1)) + 2))},
            {"BounceOut", BounceOut},
            {"BounceIn", (t) => 1 - BounceOut(1 - t)},
            {"BounceInOut", (t) => t < 0.5 ? 0.5f * (1 - BounceOut(1 - 2 * t)) : 0.5f * BounceOut(2 * t - 1) + 0.5f}
        };

        private static double BounceOut(double t)
        {
            if (t < 1 / 2.75)
                return (float)(7.5625 * t * t);
            else if (t < 2 / 2.75)
                return (float)(7.5625 * (t -= 1.5 / 2.75) * t + 0.75);
            else if (t < 2.5 / 2.75)
                return (float)(7.5625 * (t -= 2.25 / 2.75) * t + 0.9375);
            else
                return (float)(7.5625 * (t -= 2.625 / 2.75) * t + 0.984375);
        }

    }
}
