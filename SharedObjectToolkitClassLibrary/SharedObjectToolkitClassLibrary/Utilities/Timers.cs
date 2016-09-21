using System;
using System.Diagnostics;
using System.Threading;

namespace SharedObjectToolkitClassLibrary.Utilities {
    public class HighPrecisionTimer {
        Stopwatch _sw = new Stopwatch();

        public void Reset(bool _start) {
            _sw.Reset();
            if (_start)
                _sw.Start();
        }

        public void Start() {
            _sw.Start();
        }

        public void Stop() {
            _sw.Stop();
        }

        public long Milliseconds {
            get {
                return _sw.ElapsedMilliseconds;
            }
        }

        public long Microseconds {
            get {
                return _sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            }
        }

        public long Nanoseconds {
            get {
                return _sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L * 1000L));
            }
        }

        public void Wait(int microsec) {
            Reset(true);
            do {
                Thread.Yield();
            } while (Microseconds < microsec);
        }
    }

    public class HighPrecisionTimerAverage {
        [ThreadStatic]
        HighPrecisionTimer timer = null;
        private long total_count = 0;
        private long total_timer = 0;

        public void Start() {
            if (timer == null)
                timer = new HighPrecisionTimer();
            timer.Reset(true);
        }

        public void Stop() {
            if (timer == null)
                timer = new HighPrecisionTimer();
            timer.Stop();
            Interlocked.Increment(ref total_count);
            Interlocked.Add(ref total_timer, timer.Microseconds);
        }

        public long Average {
            get {
                if (total_count > 0) {
                    if (total_count > 100000) {
                        Interlocked.Exchange(ref total_count, total_count /= 1000);
                        Interlocked.Exchange(ref total_timer, total_timer /= 1000);
                    }
                    return (total_timer / total_count);
                }
                return 0;
            }
        }
    }
}