using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace NovaUI
{
    public static class Animator
    {
        #region Easing Functions

        public static float Linear(float t) { return t; }

        public static float EaseInOutCubic(float t)
        {
            if (t < 0.5f) return 4f * t * t * t;
            float f = (2f * t) - 2f;
            return 0.5f * f * f * f + 1f;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float f = t - 1f;
            return 1f + c3 * f * f * f + c1 * f * f;
        }

        public static float EaseInOutSine(float t)
        {
            return -(float)(Math.Cos(Math.PI * t) - 1) / 2f;
        }

        public static float EaseOutCubic(float t)
        {
            float f = t - 1f;
            return f * f * f + 1f;
        }

        public static float EaseInCubic(float t)
        {
            return t * t * t;
        }

        public static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        public static float EaseOutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            const float c4 = (float)(2 * Math.PI / 3);
            return (float)(Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1);
        }

        #endregion

        #region Animation Engine

        private sealed class Animation
        {
            public Control Control;
            public float From;
            public float To;
            public int DurationMs;
            public Action<float> OnTick;
            public Action OnComplete;
            public Func<float, float> Easing;
            public Stopwatch Stopwatch;
        }

        private static readonly List<Animation> _animations = new List<Animation>();
        private static readonly object _lock = new object();
        private static System.Windows.Forms.Timer _timer;

        /// <summary>
        /// يبدأ رسم متحرك.
        /// </summary>
        /// <param name="control">الكنترول الذي يُحرّك (يُستخدم كمعرّف).</param>
        /// <param name="from">القيمة الابتدائية.</param>
        /// <param name="to">القيمة النهائية.</param>
        /// <param name="durationMs">المدة بالمللي ثانية.</param>
        /// <param name="onTick">دالة تُستدعى كل إطار.</param>
        /// <param name="onComplete">دالة تُستدعى عند الانتهاء.</param>
        /// <param name="easing">دالة التنعيم (افتراضي: خطي).</param>
        public static void Animate(
            Control control,
            float from,
            float to,
            int durationMs,
            Action<float> onTick,
            Action onComplete = null,
            Func<float, float> easing = null)
        {
            if (control == null || control.IsDisposed) return;
            if (onTick == null) return;
            if (durationMs <= 0) durationMs = 1;
            if (easing == null) easing = Linear;

            lock (_lock)
            {
                for (int i = _animations.Count - 1; i >= 0; i--)
                {
                    if (_animations[i].Control == control)
                        _animations.RemoveAt(i);
                }

                var anim = new Animation
                {
                    Control = control,
                    From = from,
                    To = to,
                    DurationMs = durationMs,
                    OnTick = onTick,
                    OnComplete = onComplete,
                    Easing = easing,
                    Stopwatch = Stopwatch.StartNew()
                };

                _animations.Add(anim);
                EnsureTimer();
            }
        }

        public static void Stop(Control control)
        {
            if (control == null) return;
            lock (_lock)
            {
                for (int i = _animations.Count - 1; i >= 0; i--)
                {
                    if (_animations[i].Control == control)
                        _animations.RemoveAt(i);
                }
                if (_animations.Count == 0) StopTimer();
            }
        }

        public static void StopAll()
        {
            lock (_lock)
            {
                _animations.Clear();
                StopTimer();
            }
        }

        private static void EnsureTimer()
        {
            if (_timer == null)
            {
                _timer = new System.Windows.Forms.Timer { Interval = 15 }; // ~60 FPS
                _timer.Tick += Timer_Tick;
            }
            if (!_timer.Enabled) _timer.Start();
        }

        private static void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            Animation[] snapshot;
            lock (_lock)
            {
                snapshot = _animations.ToArray();
            }

            var completed = new List<Animation>();

            foreach (var anim in snapshot)
            {
                if (anim.Control == null || anim.Control.IsDisposed)
                {
                    completed.Add(anim);
                    continue;
                }

                float rawT = anim.Stopwatch.ElapsedMilliseconds / (float)anim.DurationMs;
                if (rawT >= 1f) rawT = 1f;

                float easedT = anim.Easing(rawT);
                float value = anim.From + (anim.To - anim.From) * easedT;

                try
                {
                    anim.OnTick(value);
                }
                catch {  }

                if (rawT >= 1f)
                    completed.Add(anim);
            }

            if (completed.Count > 0)
            {
                lock (_lock)
                {
                    foreach (var anim in completed)
                    {
                        _animations.Remove(anim);
                    }
                    if (_animations.Count == 0) StopTimer();
                }

                foreach (var anim in completed)
                {
                    if (anim.OnComplete != null && anim.Control != null && !anim.Control.IsDisposed)
                    {
                        try { anim.OnComplete(); }
                        catch { }
                    }
                }
            }
        }

        #endregion
    }
}