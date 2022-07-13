using System;

namespace EasyVCR
{
    /// <summary>
    ///     TimeFrame used to store a number of days, hours, minutes and seconds.
    /// </summary>
    public class TimeFrame
    {
        /// <summary>
        ///     Enums for common time frames.
        /// </summary>
        private enum CommonTimeFrames
        {
            Never,
            Forever,
        }

        /// <summary>
        ///     Number of days for this time frame.
        /// </summary>
        public int Days { get; set; } = 0;

        /// <summary>
        ///     Number of hours for this time frame.
        /// </summary>
        public int Hours { get; set; } = 0;

        /// <summary>
        ///     Number of minutes for this time frame.
        /// </summary>
        public int Minutes { get; set; } = 0;

        /// <summary>
        ///     Number of seconds for this time frame.
        /// </summary>
        public int Seconds { get; set; } = 0;

        /// <summary>
        ///     If this time frame is a common time frame.
        /// </summary>
        private CommonTimeFrames? CommonTimeFrame { get; set; }

        /// <summary>
        ///     Get a TimeFrame that represents "forever".
        /// </summary>
        public static TimeFrame Forever => new TimeFrame
        {
            CommonTimeFrame = CommonTimeFrames.Forever
        };

        /// <summary>
        ///     Get a TimeFrame that represents 1 month.
        /// </summary>
        public static TimeFrame Months1 => new TimeFrame
        {
            Days = 30
        };

        /// <summary>
        ///     Get a TimeFrame that represents 12 months.
        /// </summary>
        public static TimeFrame Months12 => new TimeFrame
        {
            Days = 365
        };

        /// <summary>
        ///     Get a TimeFrame that represents 2 months.
        /// </summary>
        public static TimeFrame Months2 => new TimeFrame
        {
            Days = 61
        };

        /// <summary>
        ///     Get a TimeFrame that represents 3 months.
        /// </summary>
        public static TimeFrame Months3 => new TimeFrame
        {
            Days = 91
        };

        /// <summary>
        ///     Get a TimeFrame that represents 6 months.
        /// </summary>
        public static TimeFrame Months6 => new TimeFrame
        {
            Days = 182
        };

        /// <summary>
        ///     Get a TimeFrame that represents "never".
        /// </summary>
        public static TimeFrame Never => new TimeFrame
        {
            CommonTimeFrame = CommonTimeFrames.Never
        };

        /// <summary>
        ///     Constructor for a TimeFrame object.
        /// </summary>
        public TimeFrame()
        {
        }

        /// <summary>
        ///     Check if this time frame has lapsed from the given time.
        /// </summary>
        /// <param name="fromTime">Time to add time frame to.</param>
        /// <returns>Whether this time frame has lapsed.</returns>
        public bool HasLapsed(DateTimeOffset fromTime)
        {
            var startTimePlusFrame = TimePlusFrame(fromTime.DateTime);
            return startTimePlusFrame < DateTime.Now;
        }

        /// <summary>
        ///     Get the provided time plus the time frame.
        /// </summary>
        /// <param name="fromTime">Starting time.</param>
        /// <returns>Starting time plus this time frame.</returns>
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
