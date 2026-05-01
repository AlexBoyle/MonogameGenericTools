namespace MonoTools.Core.ThreadUtilities
{
    using System.Threading;

    public class TimerThread
    {


        private Thread timerThread;
        private double targetTPS = 1d;
        private Queue<double> tickTimes = new Queue<double>();
        private double targetDeltaMs = 1d;
        private bool runTimerThread = false;
        private Func<bool> timedFunction = null;


        private volatile bool stepMode = false;
        private volatile bool isPaused = false;

        private readonly AutoResetEvent stepSignal = new AutoResetEvent(false);
        private readonly ManualResetEventSlim pauseSignal = new ManualResetEventSlim(true);


        public TimerThread(Func<bool> func, double initalTps)
        {
            if (func == null)
                throw new Exception("func in TimerThread must not be null");
            this.timedFunction = func;
            updateTPS(initalTps);
        }

        public bool isRunning()
        {
            return runTimerThread;
        }

        public void enableStepMode(bool enabled)
        {
            stepMode = enabled;
        }

        public bool isInStepMode => stepMode;

        public bool IsPaused => isPaused;

        public void step()
        {
            // Triggers the loop to run one tick
            if (stepMode)
                stepSignal.Set();
        }

        public double getAverageTickTime()
        {
            return tickTimes.Average();
        }

        public double getApproximateTicksPerSecond()
        {
            double avrTickTime = tickTimes.Average();
            double tps = 1000d / avrTickTime;
            tps = tps > targetTPS ? targetTPS : tps;
            return tps;
        }
        public void updateTPS(double newTps)
        {
            if (newTps > 0)
            {
                targetTPS = newTps;
                targetDeltaMs = 1000d / targetTPS;

                // Clear the tick time tracking and fill it with 0s
                // To be updated if the overhead is too much
                tickTimes.Clear();
                for (int i = 0; i < targetTPS; i++)
                {
                    tickTimes.Enqueue(0);
                }
            }
        }

        public bool start()
        {
            if (timedFunction != null)
            {
                runTimerThread = true;
                timerThread = new Thread(runner);
                timerThread.IsBackground = true;
                timerThread.Start();
                return true;
            }
            return false;
        }
        public void pause()
        {
            isPaused = true;
            pauseSignal.Reset();
        }

        public void resume()
        {
            isPaused = false;
            pauseSignal.Set();
        }

        public void stop()
        {
            runTimerThread = false;
            stepSignal.Set();
            pauseSignal.Set();
            timerThread.Join();
        }

        private void runner()
        {
            Stopwatch timer = Stopwatch.StartNew();
            double nextTickStartTime = timer.Elapsed.TotalMilliseconds;
            try
            {
                while (runTimerThread)
                {
                    pauseSignal.Wait();
                    if (stepMode)
                    {
                        // Wait until we are signaled to step
                        stepSignal.WaitOne();
                        if (!runTimerThread) break; // if stopped while waiting
                    }

                    timedFunction();

                    // Time control overhead 
                    nextTickStartTime += targetDeltaMs;
                    double delayTillNextTickMs = nextTickStartTime - timer.Elapsed.TotalMilliseconds;

                    tickTimes.Dequeue(); tickTimes.Enqueue(targetDeltaMs - delayTillNextTickMs);
                    // If the loop finished with extra time, sleep; else start next loop
                    if (delayTillNextTickMs > 0)
                    {
                        Thread.Sleep((int)Math.Round(delayTillNextTickMs));
                    }
                    else
                    {
                        nextTickStartTime = timer.Elapsed.TotalMilliseconds;
                    }
                }
            }
            catch (Exception e)
            {
                LoggingUtil.err("Timer Thread Failed On: " + e.Message);
                LoggingUtil.err(e.StackTrace);
                LoggingUtil.err("Timer Thread Exited");
                // We should attempt to save game on this crash
                //make a "saved when crash" save
                //throw e;
                // Add a callback for thread failure?
            }
        }
    }
}
