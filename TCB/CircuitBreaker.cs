using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCB
{
    public class CircuitBreakerTimerConfig
    {
        public int SecondsToExtendTimer { get; private set; }
        public Func<int,int> ExtendingTimeFunction { get; private set; }
        public int MaxExtendTime { get; set; }

        public CircuitBreakerTimerConfig(int secondsToExtendTimer = 10, int maxExtendTime = 60, Func<int,int> extendingTimeFunction = null)
        {
            SecondsToExtendTimer = secondsToExtendTimer;
            ExtendingTimeFunction = extendingTimeFunction;
            MaxExtendTime = maxExtendTime;
        }
    }
    public class CircuitBreakerConfig
    {
        public int MaxNumberOfActiveCalls { get; private set; }
        public int NumberOfSuccessCallsNeededToChangeStateToClosed { get; private set; }
        public int NumberOfFailedCallsNeededToChangeStateToOpen { get; private set; }


        /// <summary>
        /// Initializes a new instance of the CircuitBreakerConfig class.
        /// Used to configure Circuit Breaker.
        /// </summary>
        /// <param name="maxNumberOfActiveCalls">How many active calls to allow before returning failed responses.</param>
        /// <param name="numberOfFailedCallsNeededToChangeStateToOpen">The number of failed calls before opening circuit and blocking further calls.</param>
        /// <param name="numberOfSuccessCallsNeededToChangeStateToClosed">The number of successfull calls before closing circuit and allowing calls normally again.</param>

        public CircuitBreakerConfig(
            int maxNumberOfActiveCalls = 10,
            int numberOfFailedCallsNeededToChangeStateToOpen = 3,
            int numberOfSuccessCallsNeededToChangeStateToClosed = 3
            )
        {
            MaxNumberOfActiveCalls = maxNumberOfActiveCalls;
            NumberOfSuccessCallsNeededToChangeStateToClosed = numberOfSuccessCallsNeededToChangeStateToClosed;
            NumberOfFailedCallsNeededToChangeStateToOpen = numberOfFailedCallsNeededToChangeStateToOpen;
        }

    }


    public class CircuitBreaker
    {
        int _numberOfContinuesFailures;
        TimeToAllowCall _allowCallsAgainTime;
        int _numberOfActiveCalls;
        readonly CircuitBreakerState state;
        readonly int _numberOfSuccessNeededToChangeState;
        readonly int _numberOfFailuresNeededToChangeState;
        readonly int _maxNumberOfActiveCalls;

        int _numberOfContinuesSuccess;

        public CircuitBreaker(CircuitBreakerConfig config, CircuitBreakerTimerConfig timer)
        {
            _numberOfSuccessNeededToChangeState = config.NumberOfSuccessCallsNeededToChangeStateToClosed;
            _numberOfFailuresNeededToChangeState = config.NumberOfFailedCallsNeededToChangeStateToOpen;

            _maxNumberOfActiveCalls = config.MaxNumberOfActiveCalls;

            _allowCallsAgainTime = new TimeToAllowCall(timer);

            _numberOfContinuesFailures = 0;
            _numberOfContinuesSuccess = 0;
            _numberOfActiveCalls = 0;

            state = new CircuitBreakerState();
        }

        public CircuitBreaker()
        {
            CircuitBreakerConfig config = new CircuitBreakerConfig();
            CircuitBreakerTimerConfig timer = new CircuitBreakerTimerConfig();

            _numberOfSuccessNeededToChangeState = config.NumberOfSuccessCallsNeededToChangeStateToClosed;
            _numberOfFailuresNeededToChangeState = config.NumberOfFailedCallsNeededToChangeStateToOpen;

            _maxNumberOfActiveCalls = config.MaxNumberOfActiveCalls;

            _allowCallsAgainTime = new TimeToAllowCall(timer);

            _numberOfContinuesFailures = 0;
            _numberOfContinuesSuccess = 0;
            _numberOfActiveCalls = 0;

            state = new CircuitBreakerState();
        }


        public void ForceClosed()
        {
            lock (_allowCallsAgainTime)
            {
                _allowCallsAgainTime.ResetTimer();
            } 

            _numberOfContinuesFailures = 0;

            lock (state)
            {
                state.ChangeState(CircuitBreakerStateEnum.Closed);
            }
        }

        public void CallStart()
        {
            _numberOfActiveCalls++;
        }

        public void CallStop()
        {
            if(_numberOfActiveCalls > 0)
                _numberOfActiveCalls--;
        }

        public bool AllowCall()
        {
            lock (this)
            {

                if (_maxNumberOfActiveCalls != 0 && _numberOfActiveCalls > _maxNumberOfActiveCalls && state.StateEnum != CircuitBreakerStateEnum.Open)
                {
                    lock (this)
                    {
                        RegisterFailure();
                    }

                    return false;
                }

                if (state.StateEnum == CircuitBreakerStateEnum.Closed || state.StateEnum == CircuitBreakerStateEnum.HalfOpen)
                {
                    return true;
                }

                if (_allowCallsAgainTime.AllowCall())
                {
                    return true;
                }

                return false;
            }
        }

        public void RegisterFailure()
        {
            _numberOfContinuesSuccess = 0;
            _numberOfContinuesFailures++;

            if (_numberOfFailuresNeededToChangeState <= _numberOfContinuesFailures || state.StateEnum == CircuitBreakerStateEnum.HalfOpen || state.StateEnum == CircuitBreakerStateEnum.Open)
            {
               
                lock (_allowCallsAgainTime)
                {
                    _allowCallsAgainTime.ExtendTimer(state);
                }
                
                    
                lock (state)
                {
                    state.ChangeState(CircuitBreakerStateEnum.Open);
                }
                
            }
        }

        public void RegisterSuccess()
        {
            _numberOfContinuesFailures = 0;
            _numberOfContinuesSuccess++;

            if (state.StateEnum == CircuitBreakerStateEnum.Open)
            {
                lock (state)
                {
                    state.ChangeState(CircuitBreakerStateEnum.HalfOpen);
                }
                
            }
            if (_numberOfContinuesSuccess >= _numberOfSuccessNeededToChangeState)
            {
                lock (state)
                {
                    state.ChangeState(CircuitBreakerStateEnum.Closed);
                }
                
            }
        }


        public void CancelPendingRequests()
        {
            lock (this)
            {
                _numberOfActiveCalls = 0;
            }
        }
    }

    public class TimeToAllowCall
    {
        private DateTime Time { get; set; }
        public Func<int, int> ExtendTimeFunc { get; set; }
        private int LastExtend { get; set; }
        public int InitialExtend { get; set; }
        public int MaxExtend { get; set; }

        public TimeToAllowCall(CircuitBreakerTimerConfig config)
        {
            Time = DateTime.Now;
            InitialExtend = config.SecondsToExtendTimer;
            ExtendTimeFunc = config.ExtendingTimeFunction;
            MaxExtend = config.MaxExtendTime;
        }

        public bool AllowCall()
        {
            return DateTime.Now >= Time;
        }

        public void ResetTimer()
        {
            Time = DateTime.Now;
            LastExtend = InitialExtend;
        }

        public void ExtendTimer(CircuitBreakerState state)
        {

            DateTime newDateToAllow;

            if (state.StateEnum == CircuitBreakerStateEnum.HalfOpen && ExtendTimeFunc != null)
            {
                LastExtend = ExtendTimeFunc(LastExtend);
                if (LastExtend > MaxExtend)
                {
                    LastExtend = MaxExtend;
                }

                newDateToAllow = DateTime.Now.AddSeconds(LastExtend);
            }
            else
            {
                newDateToAllow = DateTime.Now.AddSeconds(InitialExtend);
            }

            if (newDateToAllow > Time)
            {
                Time = newDateToAllow;
            }
        }
    }

    public class CircuitBreakerState
    {
        public CircuitBreakerStateEnum StateEnum;
        public CircuitBreakerState()
        {
            StateEnum = CircuitBreakerStateEnum.Closed;
        }

        public void ChangeState(CircuitBreakerStateEnum change)
        {
            if(StateEnum != change)
                Console.WriteLine($"Circuit breaker state change from: {StateEnum} to: {change} ");
            
            StateEnum = change;
        } 
    }

    public enum CircuitBreakerStateEnum
    {
        Closed,
        Open,
        HalfOpen
    }
}