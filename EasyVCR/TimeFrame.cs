using System;

namespace EasyVCR
{
    internal enum CommonTimeFrames
    {
        Never,
        Forever,
    }

    public class TimeFrame
    {
        public int Days { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }

        public int Seconds { get; set; }

        internal CommonTimeFrames? CommonTimeFrame { get; set; }

        public static TimeFrame Forever => new TimeFrame
        {
            CommonTimeFrame = CommonTimeFrames.Forever
        };

        public static TimeFrame Never => new TimeFrame
        {
            CommonTimeFrame = CommonTimeFrames.Never
        };

        public TimeFrame()
        {
        }

        public bool HasLapsed(DateTimeOffset fromTime)
        {
            var startTimePlusFrame = TimePlusFrame(fromTime.DateTime);
            return startTimePlusFrame < DateTime.Now;
        }

        private DateTime TimePlusFrame(DateTime fromTime)
        {
            return CommonTimeFrame switch
            {
                CommonTimeFrames.Forever => DateTime.MaxValue, // will always be in the future
                CommonTimeFrames.Never => DateTime.MinValue, // will always been in the past
                var _ => fromTime.AddDays(Days).AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds)
            };
        }
    }
}
