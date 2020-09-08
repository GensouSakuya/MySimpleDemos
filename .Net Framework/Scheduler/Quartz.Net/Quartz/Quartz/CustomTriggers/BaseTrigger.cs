using Quartz.Impl.Triggers;
using Quartz.Util;
using System;

namespace Quartz.CustomTriggers
{
    public abstract class BaseTrigger: AbstractTrigger
    {
        protected TimeZoneInfo _timeZoneInfo;

        public BaseTrigger(TimeZoneInfo timeZoneInfo)
        {
            _timeZoneInfo = timeZoneInfo;
        }

        protected abstract DateTimeOffset? GetNext(DateTimeOffset dateTimeOffset);

        public override DateTimeOffset? FinalFireTimeUtc => _finalFireTime?.ToUniversalTime();
        public override bool HasMillisecondPrecision => false;

        protected DateTimeOffset? _lastFireTime;
        protected DateTimeOffset? LastFireTimeUtc => _lastFireTime?.ToUniversalTime();

        protected DateTimeOffset? _finalFireTime;
        protected DateTimeOffset? _calculatedNextTime;
        protected DateTimeOffset? _manualNextTime;
        protected DateTimeOffset? CalculatedNextTimeUtc => _calculatedNextTime?.ToUniversalTime();
        protected DateTimeOffset StartTimeLocal => GetLocalTime(StartTimeUtc);
        protected DateTimeOffset? EndTimeLocal => GetLocalTime(EndTimeUtc);

        public override DateTimeOffset? GetNextFireTimeUtc()
        {
            DateTimeOffset? next;
            if (_manualNextTime.HasValue)
            {
                if (!_calculatedNextTime.HasValue)
                {
                    next = _manualNextTime;
                }
                else
                {
                    next = _calculatedNextTime < _manualNextTime ? _calculatedNextTime : _manualNextTime;
                }
            }
            else
            {
                next = _calculatedNextTime;
            }
            return next?.ToUniversalTime();
        }

        public override DateTimeOffset? GetPreviousFireTimeUtc()
        {
            return LastFireTimeUtc;
        }

        protected abstract void Init();

        public override DateTimeOffset? ComputeFirstFireTimeUtc(ICalendar cal)
        {
            Init();
            InitNextTime();
            InitEndTime();
            return CalculatedNextTimeUtc;
        }

        public override void Triggered(ICalendar cal)
        {
            var localNow = GetLocalTime(DateTime.UtcNow);
            _lastFireTime = localNow;
            if (_manualNextTime.HasValue && Math.Abs((_manualNextTime.Value - localNow).TotalSeconds) < 1)
            {
                _manualNextTime = null;
                return;
            }

            _calculatedNextTime = GetNext(_calculatedNextTime.Value);
        }

        private void InitEndTime()
        {
            if (EndTimeLocal.HasValue)
            {
                DateTimeOffset? startTime = StartTimeLocal;
                DateTimeOffset? finalTime = null;
                while (startTime.HasValue && startTime < EndTimeLocal)
                {
                    finalTime = startTime;
                    startTime = GetNext(startTime.Value);
                }

                _finalFireTime = finalTime;
            }
        }

        private void InitNextTime()
        {
            var now = GetLocalTime(DateTime.UtcNow);
            _calculatedNextTime = StartTimeLocal;

            ////如果相差时间小于1秒，当作立刻执行
            //if (Math.Abs((StartTimeLocal - now).TotalSeconds) < 1)
            //{
            //    //do nothing
            //}
            //else
            {
                //计算现在之后的下一次执行时间
                while (_calculatedNextTime < now)
                {
                    _calculatedNextTime = GetNext(_calculatedNextTime.Value);
                }
            }
        }

        public override DateTimeOffset? GetFireTimeAfter(DateTimeOffset? afterTime)
        {
            if (!afterTime.HasValue)
            {
                return CalculatedNextTimeUtc;
            }

            var afterTimeLocal = GetLocalTime(afterTime);
            var startTime = _calculatedNextTime;
            while (startTime.HasValue && startTime.Value <= afterTimeLocal)
            {
                startTime = GetNext(startTime.Value);
            }

            if (_manualNextTime.HasValue && _manualNextTime > afterTimeLocal && _manualNextTime < startTime)
                startTime = _manualNextTime;

            return startTime?.ToUniversalTime();
        }

        public override void UpdateWithNewCalendar(ICalendar cal, TimeSpan misfireThreshold)
        {
            throw new NotImplementedException();
        }

        protected override bool ValidateMisfireInstruction(int misfireInstruction)
        {
            throw new NotImplementedException();
        }

        public override void UpdateAfterMisfire(ICalendar cal)
        {
            //ignore
        }

        public override bool GetMayFireAgain()
        {
            if (!_finalFireTime.HasValue)
                return true;
            return _calculatedNextTime <= _finalFireTime;
        }

        public override IScheduleBuilder GetScheduleBuilder()
        {
            throw new NotImplementedException();
        }

        public override void SetNextFireTimeUtc(DateTimeOffset? nextFireTime)
        {
            _manualNextTime = nextFireTime?.ToUniversalTime();
        }

        public override void SetPreviousFireTimeUtc(DateTimeOffset? previousFireTime)
        {
            _lastFireTime = previousFireTime?.ToUniversalTime();
        }

        protected DateTimeOffset? GetLocalTime(DateTimeOffset? dateTimeOffset)
        {
            if (!dateTimeOffset.HasValue)
                return null;
            return GetLocalTime(dateTimeOffset.Value);
        }

        protected DateTimeOffset GetLocalTime(DateTimeOffset dateTimeOffset)
        {
            return TimeZoneUtil.ConvertTime(dateTimeOffset, _timeZoneInfo);
        }
    }
}
