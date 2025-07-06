using System;
using System.Collections.Generic;

namespace PMEditor.Util
{
    public class EaseFunctions
    {
        private EaseFunctions() { }

        public readonly static Func<double, double> Linear = (t) 
            => t;
        public readonly static Func<double, double> SineIn = (t) 
            => (float)Math.Sin(t * Math.PI / 2);
        public readonly static Func<double, double> SineOut = (t) 
            => (float)Math.Sin(t * Math.PI / 2 + Math.PI / 2);
        public readonly static Func<double, double> SineInOut = (t) 
            => (float)(Math.Sin(t * Math.PI - Math.PI / 2) / 2 + 0.5);
        public readonly static Func<double, double> QuadIn = (t) 
            => t * t;
        public readonly static Func<double, double> QuadOut = (t) 
            => 1 - (1 - t) * (1 - t);
        public readonly static Func<double, double> QuadInOut = (t) 
            => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
        public readonly static Func<double, double> CubicIn = (t) 
            => t * t * t;
        public readonly static Func<double, double> CubicOut = (t) 
            => 1 - Math.Pow(1 - t, 3);
        public readonly static Func<double, double> CubicInOut = (t) 
            => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        public readonly static Func<double, double> QuartIn = (t) 
            => t * t * t * t;
        public readonly static Func<double, double> QuartOut = (t) 
            => 1 - Math.Pow(1 - t, 4);
        public readonly static Func<double, double> QuartInOut = (t) 
            => t < 0.5 ? 8 * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 4) / 2;
        public readonly static Func<double, double> QuintIn = (t) 
            => t * t * t * t * t;
        public readonly static Func<double, double> QuintOut = (t) 
            => 1 - Math.Pow(1 - t, 5);
        public readonly static Func<double, double> QuintInOut = (t) 
            => t < 0.5 ? 16 * t * t * t * t * t : 1 - Math.Pow(-2 * t + 2, 5) / 2;
        public readonly static Func<double, double> ExpoIn = (t) 
            => (float)Math.Pow(2, 10 * (t - 1));
        public readonly static Func<double, double> ExpoOut = (t) 
            => (float)(1 - Math.Pow(2, -10 * t));
        public readonly static Func<double, double> ExpoInOut = (t) 
            => t < 0.5 ? (float)(Math.Pow(2, 10 * (2 * t - 1)) / 2) : (float)(2 - Math.Pow(2, -10 * (2 * t - 1)) / 2);
        public readonly static Func<double, double> CircIn = (t) 
            => (float)(1 - Math.Sqrt(1 - t * t));
        public readonly static Func<double, double> CircOut = (t) 
            => (float)Math.Sqrt(1 - Math.Pow(t - 1, 2));
        public readonly static Func<double, double> CircInOut = (t) 
            => t < 0.5 ? (float)(0.5 * (1 - Math.Sqrt(1 - 4 * t * t))) : (float)(0.5 * (Math.Sqrt(-((2 * t - 3) * (2 * t - 1))) + 1));
        public readonly static Func<double, double> BackIn = (t) 
            => (float)(t * t * (2.70158 * t - 1.70158));
        public readonly static Func<double, double> BackOut = (t) 
            => (float)(1 - Math.Pow(1 - t, 2) * (-2.70158 * t + 1));
        public readonly static Func<double, double> BackInOut = (t) 
            => t < 0.5 ? (float)(0.5 * (Math.Pow(2 * t, 2) * (((1.70158 * 1.525) + 1) * 2 * t - (1.70158 * 1.525)))) : (float)(0.5 * (Math.Pow(2 * t - 2, 2) * (((1.70158 * 1.525) + 1) * (t * 2 - 2) + (1.70158 * 1.525)) + 2));
        public readonly static Func<double, double> ElasticIn = (t) 
            => (float)(Math.Sin(13 * Math.PI / 2 * t) * Math.Pow(2, 10 * (t - 1)));
        public readonly static Func<double, double> ElasticOut = (t) 
            => (float)(Math.Sin(-13 * Math.PI / 2 * (t + 1)) * Math.Pow(2, -10 * t) + 1);
        public readonly static Func<double, double> ElasticInOut = (t) 
            => t < 0.5 ? (float)(0.5 * (Math.Sin(13 * Math.PI * t) * Math.Pow(2, 20 * t - 10) + 1)) : (float)(0.5 * (Math.Sin(-13 * Math.PI * t) * Math.Pow(2, -20 * t + 10) - 1));
        public readonly static Func<double, double> BounceOut = 
            _BounceOut;
        public readonly static Func<double, double> BounceIn = (t) 
            => 1 - _BounceOut(1 - t);
        public readonly static Func<double, double> BounceInOut = (t) 
            => t < 0.5 ? 0.5f * (1 - _BounceOut(1 - 2 * t)) : 0.5f * _BounceOut(2 * t - 1) + 0.5f;

        #region 函数名字
        public const string LinearName = "Linear";
        public const string SineInName = "SineIn";
        public const string SineOutName = "SineOut";
        public const string SineInOutName = "SineInOut";
        public const string QuadInName = "QuadIn";
        public const string QuadOutName = "QuadOut";
        public const string QuadInOutName = "QuadInOut";
        public const string CubicInName = "CubicIn";
        public const string CubicOutName = "CubicOut";
        public const string CubicInOutName = "CubicInOut";
        public const string QuartInName = "QuartIn";
        public const string QuartOutName = "QuartOut";
        public const string QuartInOutName = "QuartInOut";
        public const string QuintInName = "QuintIn";
        public const string QuintOutName = "QuintOut";
        public const string QuintInOutName = "QuintInOut";
        public const string ExpoInName = "ExpoIn";
        public const string ExpoOutName = "ExpoOut";
        public const string ExpoInOutName = "ExpoInOut";
        public const string CircInName = "CircIn";
        public const string CircOutName = "CircOut";
        public const string CircInOutName = "CircInOut";
        public const string BackInName = "BackIn";
        public const string BackOutName = "BackOut";
        public const string BackInOutName = "BackInOut";
        public const string ElasticInName = "ElasticIn";
        public const string ElasticOutName = "ElasticOut";
        public const string ElasticInOutName = "ElasticInOut";
        public const string BounceOutName = "BounceOut";
        public const string BounceInName = "BounceIn";
        public const string BounceInOutName = "BounceInOut";
        #endregion
        
        public static readonly Dictionary<string, Func<double, double>> Functions = new()
        {
            {LinearName, Linear},

            {SineInName, SineIn},
            {SineOutName, SineOut},
            {SineInOutName, SineInOut},
            
            {QuadInName, QuadIn},
            {QuadOutName, QuadOut},
            {QuadInOutName, QuadInOut},
            
            {CubicInName, CubicIn},
            {CubicOutName, CubicOut},
            {CubicInOutName, CubicInOut},
            
            {QuartInName, QuartIn},
            {QuartOutName, QuartOut},
            {QuartInOutName, QuartInOut},
            
            {QuintInName, QuintIn},
            {QuintOutName, QuintOut},
            {QuintInOutName, QuintInOut},
            
            {ExpoInName, ExpoIn},
            {ExpoOutName, ExpoOut},
            {ExpoInOutName, ExpoInOut},
            
            {CircInName, CircIn},
            {CircOutName, CircOut},
            {CircInOutName, CircInOut},
            
            {BackInName, BackIn},
            {BackOutName, BackOut},
            {BackInOutName, BackInOut},
            
            {ElasticInName, ElasticIn},
            {ElasticOutName, ElasticOut},
            {ElasticInOutName, ElasticInOut},
            
            {BounceInName, BounceIn},
            {BounceOutName, BounceOut},
            {BounceInOutName, BounceInOut}
        };

        public static readonly Dictionary<string, (double, double)> FunctionMaxValues = new()
        {
            { LinearName, (0, 1) },
            { SineInName, (0, 1) }, { SineOutName, (0, 1) }, { SineInOutName, (0, 1) },
            { QuadInName, (0, 1) }, { QuadOutName, (0, 1) }, { QuadInOutName, (0, 1) },
            { CubicInName, (0, 1) }, { CubicOutName, (0, 1) }, { CubicInOutName, (0, 1) },
            { QuartInName, (0, 1) }, { QuartOutName, (0, 1) }, { QuartInOutName, (0, 1) },
            { QuintInName, (0, 1) }, { QuintOutName, (0, 1) }, { QuintInOutName, (0, 1) },
            { ExpoInName, (0, 1) }, { ExpoOutName, (0, 1) }, { ExpoInOutName, (0, 1) },
            { CircInName, (0, 1) }, { CircOutName, (0, 1) }, { CircInOutName, (0, 1) },
            { BackInName, (-0.1, 1) }, { BackOutName, (0, 1.1) }, { BackInOutName, (-0.05, 1.05) },
            { ElasticInName, (-0.36428, 1) }, { ElasticOutName, (0, 1.36428) }, { ElasticInOutName, (-0.18214, 1.18214) },
            { BounceInName, (0, 1) }, { BounceOutName, (0, 1) }, { BounceInOutName, (0, 1) }
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

        private static double _BounceOut(double t)
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
