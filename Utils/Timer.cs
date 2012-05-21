using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Server {
    public class Timer {
        private static long frequency = Stopwatch.Frequency;
        
        private long elapsed;
        private Stopwatch watch;
        
        private int average;
        private long start;
        private List<long> elapsedCollection;
        
        public Timer() {
            this.watch = new Stopwatch();
            this.elapsedCollection = new List<long>();
        }
        
        public void Start() {
            this.Stop();
            this.watch.Restart();
        }
        
        public void Stop() {
            if (this.watch.IsRunning) {
                this.watch.Stop();
                if (this.average == 0) {
                    this.elapsed = this.watch.ElapsedTicks;
                } else {
                    this.elapsedCollection.Add(this.watch.ElapsedTicks);
                    if (Time.Now - this.start >= this.average) {
                        this.CalculateAverage();
                        this.elapsedCollection.Clear();
                        this.start = Time.Now;
                    }
                }
            }
        }

        private void CalculateAverage() {
            this.elapsed = 0;
            for (int i = 0; i < this.elapsedCollection.Count; ++i) {
                this.elapsed += this.elapsedCollection[i] / this.elapsedCollection.Count;
            }
        }
        
        public int Average {
            set {
                if (this.average != value) {
                    if (this.average > value) {
                        this.CalculateAverage();
                        this.elapsedCollection.Clear();
                        this.start = Time.Now;
                    }
                    this.average = value;
                }
            }
            get { return this.average; }
        }
        
        public double ElapsedNanoseconds {
            get {
                if (this.watch.IsRunning && this.average == 0) {
                    return 1000000000.0 * (this.watch.ElapsedTicks / (double)frequency);
                } else {
                    return 1000000000.0 * (this.elapsed / (double)frequency);
                }
            }
        }
        
        public double ElapsedMilliseconds {
            get {
                if (this.watch.IsRunning && this.average == 0) {
                    return 1000.0 * (this.watch.ElapsedTicks / (double)frequency);
                } else {
                    return 1000.0 * (this.elapsed / (double)frequency);
                }
            }
        }
        
        public double ElapsedSeconds {
            get {
                if (this.watch.IsRunning && this.average == 0) {
                    return this.watch.ElapsedTicks / (double)frequency;
                } else {
                    return this.elapsed / (double)frequency;
                }
            }
        }
    }
}