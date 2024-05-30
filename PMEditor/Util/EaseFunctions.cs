using System;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace PMEditor.Util
{
    public class EaseFunctions
    {
        private EaseFunctions() { }

        public readonly static Func<double, double> linear = (t) 
            => t;
        public readonly static Func<double, double> sineIn = (t) 
            => (float)Math.Sin(t * Math.PI / 2);
        public readonly static Func<double, double> sineOut = (t) 
            => (float)Math.Sin(t * Math.PI / 2 + Math.PI / 2);
        public readonly static Func<double, double> sineInOut = (t) 
            => (float)(Math.Sin(t * Math.PI - Math.PI / 2) / 2 + 0.5);
        public readonly static Func<double, double> quadIn = (t) 
            => t * t;
        public readonly static Func<double, double> quadOut = (t) 
            => 1 - (1 - t) * (1 - t);
        public readonly static Func<double, double> quadInOut = (t) 
            => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
        public readonly static Func<double, double> cubicIn = (t) 
            => t * t * t;
        public readonly static Func<double, double> cubicOut = (t) 
            => 1 - Math.Pow(1 - t, 3);
        public readonly static Func<double, double> cubicInOut = (t) 
            => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        public readonly static Func<double, double> quartIn = (t) 
            => t * t * t * t;
        public readonly static Func<double, double> quartOut = (t) 
            => 1 - Math.Pow(1 - t, 4);
        public readonly static Func<double, double> quartInOut = (t) 
            => t < 0.5 ? 8 * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 4) / 2;
        public readonly static Func<double, double> quintIn = (t) 
            => t * t * t * t * t;
        public readonly static Func<double, double> quintOut = (t) 
            => 1 - Math.Pow(1 - t, 5);
        public readonly static Func<double, double> quintInOut = (t) 
            => t < 0.5 ? 16 * t * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 5) / 2;
        public readonly static Func<double, double> expoIn = (t) 
            => (float)Math.Pow(2, 10 * (t - 1));
        public readonly static Func<double, double> expoOut = (t) 
            => (float)(1 - Math.Pow(2, -10 * t));
        public readonly static Func<double, double> expoInOut = (t) 
            => t < 0.5 ? (float)(Math.Pow(2, 10 * (2 * t - 1)) / 2) : (float)(2 - Math.Pow(2, -10 * (2 * t - 1)) / 2);
        public readonly static Func<double, double> circIn = (t) 
            => (float)(1 - Math.Sqrt(1 - t * t));
        public readonly static Func<double, double> circOut = (t) 
            => (float)Math.Sqrt(1 - Math.Pow(t - 1, 2));
        public readonly static Func<double, double> circInOut = (t) 
            => t < 0.5 ? (float)(0.5 * (1 - Math.Sqrt(1 - 4 * t * t))) : (float)(0.5 * (Math.Sqrt(-((2 * t - 3) * (2 * t - 1))) + 1));
        public readonly static Func<double, double> backIn = (t) 
            => (float)(t * t * ((1.70158 + 1) * t - 1.70158));
        public readonly static Func<double, double> backOut = (t) 
            => (float)(1 - Math.Pow(1 - t, 2) * ((1.70158 + 1) * (1 - t) - 1.70158));
        public readonly static Func<double, double> backInOut = (t) 
            => t < 0.5 ? (float)(0.5 * (Math.Pow(2 * t, 2) * (((1.70158 * 1.525) + 1) * 2 * t - (1.70158 * 1.525)))) : (float)(0.5 * (Math.Pow(2 * t - 2, 2) * (((1.70158 * 1.525) + 1) * (t * 2 - 2) + (1.70158 * 1.525)) + 2));
        public readonly static Func<double, double> elasticIn = (t) 
            => (float)(Math.Sin(13 * Math.PI / 2 * t) * Math.Pow(2, 10 * (t - 1)));
        public readonly static Func<double, double> elasticOut = (t) 
            => (float)(Math.Sin(-13 * Math.PI / 2 * (t + 1)) * Math.Pow(2, -10 * t) + 1);
        public readonly static Func<double, double> elasticInOut = (t) 
            => t < 0.5 ? (float)(0.5 * (Math.Sin(13 * Math.PI / 2 * (2 * t)) * Math.Pow(2, 10 * ((2 * t) - 1)) - 1)) : (float)(0.5 * (Math.Sin(-13 * Math.PI / 2 * ((2 * t - 1) + 1)) * Math.Pow(2, -10 * (2 * t - 1)) + 2));
        public readonly static Func<double, double> bounceOut = 
            BounceOut;
        public readonly static Func<double, double> bounceIn = (t) 
            => 1 - BounceOut(1 - t);
        public readonly static Func<double, double> bounceInOut = (t) 
            => t < 0.5 ? 0.5f * (1 - BounceOut(1 - 2 * t)) : 0.5f * BounceOut(2 * t - 1) + 0.5f;

        public const string linearName = "Linear";
        public const string sineInName = "SineIn";
        public const string sineOutName = "SineOut";
        public const string sineInOutName = "SineInOut";
        public const string quadInName = "QuadIn";
        public const string quadOutName = "QuadOut";
        public const string quadInOutName = "QuadInOut";
        public const string cubicInName = "CubicIn";
        public const string cubicOutName = "CubicOut";
        public const string cubicInOutName = "CubicInOut";
        public const string quartInName = "QuartIn";
        public const string quartOutName = "QuartOut";
        public const string quartInOutName = "QuartInOut";
        public const string quintInName = "QuintIn";
        public const string quintOutName = "QuintOut";
        public const string quintInOutName = "QuintInOut";
        public const string expoInName = "ExpoIn";
        public const string expoOutName = "ExpoOut";
        public const string expoInOutName = "ExpoInOut";
        public const string circInName = "CircIn";
        public const string circOutName = "CircOut";
        public const string circInOutName = "CircInOut";
        public const string backInName = "BackIn";
        public const string backOutName = "BackOut";
        public const string backInOutName = "BackInOut";
        public const string elasticInName = "ElasticIn";
        public const string elasticOutName = "ElasticOut";
        public const string elasticInOutName = "ElasticInOut";
        public const string bounceOutName = "BounceOut";
        public const string bounceInName = "BounceIn";
        public const string bounceInOutName = "BounceInOut";

        public static Dictionary<string, Func<double, double>> functions = new()
        {
            {linearName, linear},

            {sineInName, sineIn},
            {sineOutName, sineOut},
            {sineInOutName, sineInOut},
            
            {quadInName, quadIn},
            {quadOutName, quadOut},
            {quadInOutName, quadInOut},
            
            {cubicInName, cubicIn},
            {cubicOutName, cubicOut},
            {cubicInOutName, cubicInOut},
            
            {quartInName, quartIn},
            {quartOutName, quartOut},
            {quartInOutName, quartInOut},
            
            {quintInName, quintIn},
            {quintOutName, quintOut},
            {quintInOutName, quintInOut},
            
            {expoInName, expoIn},
            {expoOutName, expoOut},
            {expoInOutName, expoInOut},
            
            {circInName, circIn},
            {circOutName, circOut},
            {circInOutName, circInOut},
            
            {backInName, backIn},
            {backOutName, backOut},
            {backInOutName, backInOut},
            
            {elasticInName, elasticIn},
            {elasticOutName, elasticOut},
            {elasticInOutName, elasticInOut},
            
            {bounceInName, bounceIn},
            {bounceOutName, bounceOut},
            {bounceInOutName, bounceInOut}
        };

        /*
        public static double MaxOf(string funcName)
        {
            switch (funcName)
            {
                case linearName:
                    return 1;
            }

        }
        */

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
