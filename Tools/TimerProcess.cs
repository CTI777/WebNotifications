using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebNotifications.Tools
{
    public class TimerProcess
    {
        private long TimerInterval = 10000;
        Action<Object> CallbackFunc =null;

        private static object _locker = new object();
        private static Timer _timer;

        public TimerProcess(long TimerInterval, Action<Object> CallbackFunc) {
            this.TimerInterval = TimerInterval;
            this.CallbackFunc= CallbackFunc;
        }

        public void Start()
        {
            _timer = new Timer(Callback, null, 0, TimerInterval);
        }

        public void Stop()
        {
            _timer.Dispose();
        }

        public void Callback(object state)
        {
            var hasLock = false;

            try
            {
                Monitor.TryEnter(_locker, ref hasLock);
                if (!hasLock)
                {
                    return;
                }
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                //Call external function
                CallbackFunc(state);
            }
            finally
            {
                if (hasLock)
                {
                    Monitor.Exit(_locker);
                    _timer.Change(TimerInterval, TimerInterval);
                }
            }
        }
    }
}