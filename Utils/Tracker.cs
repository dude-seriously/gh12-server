using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Server {
    public class Tracker {
        private static long frequency = Stopwatch.Frequency;

        private Stopwatch watch;
        private Dictionary<string, long> points;

        public Tracker() {
            this.points = new Dictionary<string, long>();
            this.watch = Stopwatch.StartNew();
        }

        public void Track(string pointName) {
            if (this.watch != null) {
                this.points.Add(pointName, this.watch.ElapsedTicks);
            }
        }

        public void Stop() {
            if (this.watch != null) {
                this.points.Add("end", this.watch.ElapsedTicks);
                this.watch.Stop();
                this.watch = null;
            }
        }

        public int Count {
            get { return this.points.Count; }
        }

        public double this[string pointName] {
            get { return 1000000000.0 * (this.points[pointName] / (double)frequency); }
        }

        public double ElapsedNanoseconds {
            get {
                if (this.watch != null) {
                    return 1000000000.0 * (this.watch.ElapsedTicks / (double)frequency);
                } else {
                    return 1000000000.0 * (this.points.Last().Value / (double)frequency);
                }
            }
        }

        public double ElapsedMilliseconds {
            get {
                if (this.watch != null) {
                    return 1000.0 * (this.watch.ElapsedTicks / (double)frequency);
                } else {
                    return 1000.0 * (this.points.Last().Value / (double)frequency);
                }
            }
        }

        public double ElapsedSeconds {
            get {
                if (this.watch != null) {
                    return this.watch.ElapsedTicks / (double)frequency;
                } else {
                    return this.points.Last().Value / (double)frequency;
                }
            }
        }
    }
}