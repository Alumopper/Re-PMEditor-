using System;
using System.Collections.Generic;

namespace PMEditor.Util
{
    public class EaseFunctions
    {
        private EaseFunctions() { }

        public readonly static Func<double, double> linear = (t) => t;
        public readonly static Func<double, double> sineIn = (t) => (float)Math.Sin(t * Math.PI / 2);
        public readonly static Func<double, double> sineOut = (t) => (float)Math.Sin(t * Math.PI / 2 + Math.PI / 2);
        public readonly static Func<double, double> sineInOut = (t) => (float)(Math.Sin(t * Math.PI - Math.PI / 2) / 2 + 0.5);
        public readonly static Func<double, double> quadIn = (t) => t * t;
        public readonly static Func<double, double> quadOut = (t) => 1 - (1 - t) * (1 - t);
        public readonly static Func<double, double> quadInOut = (t) => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
        public readonly static Func<double, double> cubicIn = (t) => t * t * t;
        public readonly static Func<double, double> cubicOut = (t) => 1 - Math.Pow(1 - t, 3);
        public readonly static Func<double, double> cubicInOut = (t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        public readonly static Func<double, double> quartIn = (t) => t * t * t * t;
        public readonly static Func<double, double> quartOut = (t) => 1 - Math.Pow(1 - t, 4);
        public readonly static Func<double, double> quartInOut = (t) => t < 0.5 ? 8 * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 4) / 2;
        public readonly static Func<double, double> quintIn = (t) => t * t * t * t * t;
        public readonly static Func<double, double> quintOut = (t) => 1 - Math.Pow(1 - t, 5);
        public readonly static Func<double, double> quintInOut = (t) => t < 0.5 ? 16 * t * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 5) / 2;
        public readonly static Func<double, double> expoIn = (t) => (float)Math.Pow(2, 10 * (t - 1));
        public readonly static Func<double, double> expoOut = (t) => (float)(1 - Math.Pow(2, -10 * t));
        public readonly static Func<double, double> expoInOut = (t) => t < 0.5 ? (float)(Math.Pow(2, 10 * (2 * t - 1)) / 2) : (float)(2 - Math.Pow(2, -10 * (2 * t - 1)) / 2);
        public readonly static Func<double, double> circIn = (t) => (float)(1 - Math.Sqrt(1 - t * t));
        public readonly static Func<double, double> circOut = (t) => (float)Math.Sqrt(1 - Math.Pow(t - 1, 2));
        public readonly static Func<double, double> circInOut = (t) => t < 0.5 ? (float)(0.5 * (1 - Math.Sqrt(1 - 4 * t * t))) : (float)(0.5 * (Math.Sqrt(-((2 * t - 3) * (2 * t - 1))) + 1));
        public readonly static Func<double, double> backIn = (t) => (float)(t * t * ((1.70158 + 1) * t - 1.70158));
        public readonly static Func<double, double> backOut = (t) => (float)(1 - Math.Pow(1 - t, 2) * ((1.70158 + 1) * (1 - t) - 1.70158));
        public readonly static Func<double, double> backInOut = (t) => t < 0.5 ? (float)(0.5 * (Math.Pow(2 * t, 2) * (((1.70158 * 1.525) + 1) * 2 * t - (1.70158 * 1.525)))) : (float)(0.5 * (Math.Pow(2 * t - 2, 2) * (((1.70158 * 1.525) + 1) * (t * 2 - 2) + (1.70158 * 1.525)) + 2));
        public readonly static Func<double, double> elasticIn = (t) => (float)(Math.Sin(13 * Math.PI / 2 * t) * Math.Pow(2, 10 * (t - 1)));
        public readonly static Func<double, double> elasticOut = (t) => (float)(Math.Sin(-13 * Math.PI / 2 * (t + 1)) * Math.Pow(2, -10 * t) + 1);
        public readonly static Func<double, double> elasticInOut = (t) => t < 0.5 ? (float)(0.5 * (Math.Sin(13 * Math.PI / 2 * (2 * t)) * Math.Pow(2, 10 * ((2 * t) - 1)) - 1)) : (float)(0.5 * (Math.Sin(-13 * Math.PI / 2 * ((2 * t - 1) + 1)) * Math.Pow(2, -10 * (2 * t - 1)) + 2));
        public readonly static Func<double, double> bounceOut = BounceOut;
        public readonly static Func<double, double> bounceIn = (t) => 1 - BounceOut(1 - t);
        public readonly static Func<double, double> bounceInOut = (t) => t < 0.5 ? 0.5f * (1 - BounceOut(1 - 2 * t)) : 0.5f * BounceOut(2 * t - 1) + 0.5f;

        public static Dictionary<string, Func<double, double>> functions = new()
        {
            {"Linear", linear},
            {"SineIn", sineIn},
            {"SineOut", sineOut},
            {"SineInOut", sineInOut},
            {"QuadIn", quadIn},
            {"QuadOut", quadOut},
            {"QuadInOut", quadInOut},
            {"CubicIn", cubicIn},
            {"CubicOut", cubicOut},
            {"CubicInOut", cubicInOut},
            {"QuartIn", quartIn},
            {"QuartOut", quadOut},
            {"QuartInOut", quartInOut},
            {"QuintIn", quintIn},
            {"QuintOut", quintOut},
            {"QuintInOut", quintInOut},
            {"ExpoIn", expoIn},
            {"ExpoOut", expoOut},
            {"ExpoInOut", expoInOut},
            {"CircIn", cubicIn},
            {"CircOut", circOut},
            {"CircInOut", circInOut},
            {"BackIn", backIn},
            {"BackOut", backOut},
            {"BackInOut", backInOut},
            {"ElasticIn", elasticIn},
            {"ElasticOut", elasticOut},
            {"ElasticInOut", elasticInOut},
            {"BounceOut", bounceOut},
            {"BounceIn", bounceIn},
            {"BounceInOut", bounceInOut}
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

        public static double Interpolate(double startValue, double endValue, double t, Func<double, double> function)
        {
            double v = function(t);
            return startValue + (endValue - startValue) * v;
        }

        public static double Interpolate(double start, double end, double startValue, double endValue, double t, Func<double, double> function)
        {
            double v = function((t - start) / (end - start));
            return startValue + (endValue - startValue) * v;
        }
    }
}
