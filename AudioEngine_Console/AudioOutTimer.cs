// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Timers;

using Incendonet.Utilities.LogClient;


namespace AudioMgr
{
    public enum TimerArmMode
    {
        OnStart,
        Manual
    }

    public delegate void AudioOutTimerElapsedHandler();

    public sealed class AudioOutTimer
    {
        private static TimeSpan sMinimumDuration = new TimeSpan(0, 0, 0, 0, 100);               // According to MSDN the minimum duration allowed for a Timer is 100 ms.

        private readonly string m_sTimerName;
        private readonly AudioOutTimerElapsedHandler m_TimerElapsedHandler;
        private readonly TimerArmMode m_eTimerArmMode;
        private readonly int m_iThreadIndex;
        private readonly ILegacyLogger m_Logger = null;

        private readonly Object m_padlock = new Object();
        private bool m_bArmed = false;

        private Timer m_timer;
        private DateTime m_timerStarted;


        public AudioOutTimer(string i_sTimerName, AudioOutTimerElapsedHandler i_TimerElapsedHandler, int i_iThreadIndex, ILegacyLogger i_Logger)
            : this(i_sTimerName, i_TimerElapsedHandler, TimerArmMode.OnStart, i_iThreadIndex, i_Logger)
        {
        }

        public AudioOutTimer(string i_sTimerName, AudioOutTimerElapsedHandler i_TimerElapsedHandler, TimerArmMode i_eTimerArmMode, int i_iThreadIndex, ILegacyLogger i_Logger)
        {
            m_sTimerName = i_sTimerName;
            m_TimerElapsedHandler = i_TimerElapsedHandler;
            m_eTimerArmMode = i_eTimerArmMode;
            m_iThreadIndex = i_iThreadIndex;
            m_Logger = i_Logger;

            m_timer = new Timer();
            m_timer.Enabled = false;
            m_timer.AutoReset = false;
            m_timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
        }

        public void Start(TimeSpan i_Duration)
        {
            try
            {
                lock (m_padlock)
                {
                    StartTimer(i_Duration);
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("Timer Start (tid {0}) caught: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, exc.ToString()));
            }
        }

        public void Add(TimeSpan i_Duration)
        {
            try
            {
                lock (m_padlock)
                {
                    TimeSpan newDuration;

                    if (m_timer.Enabled)
                    {
                        newDuration = new TimeSpan(0, 0, 0, 0, (int)m_timer.Interval) - (DateTime.Now - m_timerStarted) + i_Duration;
                    }
                    else
                    {
                        newDuration = i_Duration;
                    }

                    StartTimer(newDuration);
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("Timer Add (tid {0}) caught: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, exc.ToString()));
            }
        }

        public void Stop()
        {
            try
            {
                lock (m_padlock)
                {
                    m_timer.Enabled = false;
                    m_bArmed = false;
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("Timer Stop (tid {0}) caught: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, exc.ToString()));
            }
        }

        public void Arm()
        {
            try
            {
                lock (m_padlock)
                {
                    m_bArmed = true;
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("Timer Arm (tid {0}) caught: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, exc.ToString()));
            }
        }

        private void StartTimer(TimeSpan i_Duration)
        {
            if (i_Duration < sMinimumDuration)
            {
                i_Duration = sMinimumDuration;
                Log(Level.Warning, String.Format("Duration specified ({0}) is too short, using default duration ({1}).", i_Duration, sMinimumDuration));
            }

            if (m_eTimerArmMode == TimerArmMode.OnStart)
            {
                m_bArmed = true;
            }

            m_timer.Interval = i_Duration.TotalMilliseconds;
            m_timer.Enabled = true;

            m_timerStarted = DateTime.Now;
        }

        private void OnTimerElapsed(Object sender, ElapsedEventArgs e)
        {
            try
            {
                if (m_bArmed)
                {
                    m_bArmed = false;
                    m_TimerElapsedHandler();
                }
                else
                {
                    Log(Level.Verbose, "Timer elapsed without being armed.");
                }
            }
            catch (Exception exc)
            {
                Log(Level.Exception, String.Format("OnTimerElapsed (tid {0}) caught: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, exc.ToString()));
            }
        }

        private void Log(Level i_Level, string i_sMessage)
        {
            if (m_Logger != null)
            {
                m_Logger.Log(i_Level, String.Format("[{0}]{1} - {2}", m_iThreadIndex, m_sTimerName, i_sMessage));
            }
            else
            {
                Console.Error.WriteLine(String.Format("{0} {1} [{2}]{3} - {4}", DateTime.Now, i_Level, m_iThreadIndex, m_sTimerName, i_sMessage));
            }
        }
    }
}
