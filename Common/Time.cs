﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using QuantConnect.Logging;
using QuantConnect.Securities;

namespace QuantConnect 
{
    /// <summary>
    /// Time helper class collection for working with trading dates
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Provides a value far enough in the future the current computer hardware will have decayed :)
        /// </summary>
        public static readonly DateTime EndOfTime = new DateTime(2050, 12, 31);

        /// <summary>
        /// One Day TimeSpan Period Constant
        /// </summary>
        public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

        /// <summary>
        /// One Hour TimeSpan Period Constant
        /// </summary>
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
        
        /// <summary>
        /// One Minute TimeSpan Period Constant
        /// </summary>
        public static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);

        /// <summary>
        /// One Second TimeSpan Period Constant
        /// </summary>
        public static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        /// <summary>
        /// One Millisecond TimeSpan Period Constant
        /// </summary>
        public static readonly TimeSpan OneMillisecond = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Live charting is sensitive to timezone so need to convert the local system time to a UTC and display in browser as UTC.
        /// </summary>
        public struct DateTimeWithZone
        {
            private readonly DateTime utcDateTime;
            private readonly TimeZoneInfo timeZone;

            /// <summary>
            /// Initializes a new instance of the <see cref="QuantConnect.Time+DateTimeWithZone"/> struct.
            /// </summary>
            /// <param name="dateTime">Date time.</param>
            /// <param name="timeZone">Time zone.</param>
            public DateTimeWithZone(DateTime dateTime, TimeZoneInfo timeZone)
            {
                utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
                this.timeZone = timeZone;
            }

            /// <summary>
            /// Gets the universal time.
            /// </summary>
            /// <value>The universal time.</value>
            public DateTime UniversalTime { get { return utcDateTime; } }

            /// <summary>
            /// Gets the time zone.
            /// </summary>
            /// <value>The time zone.</value>
            public TimeZoneInfo TimeZone { get { return timeZone; } }

            /// <summary>
            /// Gets the local time.
            /// </summary>
            /// <value>The local time.</value>
            public DateTime LocalTime
            {
                get
                {
                    return TimeZoneInfo.ConvertTime(utcDateTime, timeZone);
                }
            }
        }

        /// <summary>
        /// Create a C# DateTime from a UnixTimestamp
        /// </summary>
        /// <param name="unixTimeStamp">Double unix timestamp (Time since Midnight Jan 1 1970)</param>
        /// <returns>C# date timeobject</returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp) 
        {
            var time = DateTime.Now;
            try 
            {
                // Unix timestamp is seconds past epoch
                time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                time = time.AddSeconds(unixTimeStamp);
            }
            catch (Exception err)
            {
                Log.Error("Time.UnixTimeStampToDateTime(): " + unixTimeStamp + err.Message);
            }
            return time;
        }

        /// <summary>
        /// Convert a Datetime to Unix Timestamp
        /// </summary>
        /// <param name="time">C# datetime object</param>
        /// <returns>Double unix timestamp</returns>
        public static double DateTimeToUnixTimeStamp(DateTime time) 
        {
            double timestamp = 0;
            try 
            {
                timestamp = (time - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            } 
            catch (Exception err) 
            {
                Log.Error("Time.DateTimeToUnixTimeStamp(): " + time.ToOADate() + err.Message);
            }
            return timestamp;
        }

        /// <summary>
        /// Get the current time as a unix timestamp
        /// </summary>
        /// <returns>Double value of the unix as UTC timestamp</returns>
        public static double TimeStamp() 
        {
            return DateTimeToUnixTimeStamp(DateTime.UtcNow);
        }

        /// <summary>
        /// Parse a standard YY MM DD date into a DateTime. Attempt common date formats 
        /// </summary>
        /// <param name="dateToParse">String date time to parse</param>
        /// <returns>Date time</returns>
        public static DateTime ParseDate(string dateToParse)
        {
            try
            {
                //First try the exact options:
                DateTime date;
                if (DateTime.TryParseExact(dateToParse, DateFormat.SixCharacter, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
                if (DateTime.TryParseExact(dateToParse, DateFormat.EightCharacter, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
                if (DateTime.TryParseExact(dateToParse.Substring(0, 19), DateFormat.JsonFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
                if (DateTime.TryParseExact(dateToParse, DateFormat.US, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
                if (DateTime.TryParse(dateToParse, out date))
                {
                    return date;
                }
            }
            catch (Exception err)
            {
                Log.Error("Time.ParseDate(): " + err.Message);
            }
            
            return DateTime.Now;
        }


        /// <summary>
        /// Define an enumerable date range and return each date as a datetime object in the date range
        /// </summary>
        /// <param name="from">DateTime start date</param>
        /// <param name="thru">DateTime end date</param>
        /// <returns>Enumerable date range</returns>
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru) 
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }


        /// <summary>
        /// Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
        /// </summary>
        /// <param name="securities">Securities we have in portfolio</param>
        /// <param name="from">Start date</param>
        /// <param name="thru">End date</param>
        /// <returns>Enumerable date range</returns>
        public static IEnumerable<DateTime> EachTradeableDay(ICollection<Security> securities, DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                if (TradableDate(securities, day))
                {
                    yield return day;
                }
            }
        }


        /// <summary>
        /// Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
        /// </summary>
        /// <param name="security">The security to get tradeable dates for</param>
        /// <param name="from">Start date</param>
        /// <param name="thru">End date</param>
        /// <returns>Enumerable date range</returns>
        public static IEnumerable<DateTime> EachTradeableDay(Security security, DateTime from, DateTime thru)
        {
            return EachTradeableDay(security.Exchange.Hours, from, thru);
        }


        /// <summary>
        /// Define an enumerable date range of tradeable dates - skip the holidays and weekends when securities in this algorithm don't trade.
        /// </summary>
        /// <param name="exchange">The security to get tradeable dates for</param>
        /// <param name="from">Start date</param>
        /// <param name="thru">End date</param>
        /// <returns>Enumerable date range</returns>
        public static IEnumerable<DateTime> EachTradeableDay(SecurityExchangeHours exchange, DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                if (exchange.IsDateOpen(day))
                {
                    yield return day;
                }
            }
        }

        /// <summary>
        /// Make sure this date is not a holiday, or weekend for the securities in this algorithm.
        /// </summary>
        /// <param name="securities">Security manager from the algorithm</param>
        /// <param name="day">DateTime to check if trade-able.</param>
        /// <returns>True if tradeable date</returns>
        public static bool TradableDate(IEnumerable<Security> securities, DateTime day)
        {
            try
            {
                foreach (var security in securities)
                {
                    if (security.Exchange.IsOpenDuringBar(day.Date, day.Date.AddDays(1), security.IsExtendedMarketHours)) return true;
                }
            }
            catch (Exception err)
            {
                Log.Error("Time.TradeableDate(): " + err.Message);
            }
            return false;
        }


        /// <summary>
        /// Could of the number of tradeable dates within this period.
        /// </summary>
        /// <param name="securities">Securities we're trading</param>
        /// <param name="start">Start of Date Loop</param>
        /// <param name="finish">End of Date Loop</param>
        /// <returns>Number of dates</returns>
        public static int TradeableDates(ICollection<Security> securities, DateTime start, DateTime finish)
        {
            var count = 0;
            Log.Trace("Time.TradeableDates(): Security Count: " + securities.Count);
            try 
            {
                foreach (var day in EachDay(start, finish)) 
                {
                    if (TradableDate(securities, day)) 
                    {
                        count++;
                    }
                }
            } 
            catch (Exception err) 
            {
                Log.Error("Time.TradeableDates(): " + err.Message);
            }
            return count;
        }

        /// <summary>
        /// Determines the start time required to produce the requested number of bars and the given size
        /// </summary>
        /// <param name="exchange">The exchange used to test for market open hours</param>
        /// <param name="end">The end time of the last bar over the requested period</param>
        /// <param name="barSize">The length of each bar</param>
        /// <param name="barCount">The number of bars requested</param>
        /// <param name="extendedMarketHours">True to allow extended market hours bars, otherwise false for only normal market hours</param>
        /// <returns>The start time that would provide the specified number of bars ending at the specified end time, rounded down by the requested bar size</returns>
        public static DateTime GetStartTimeForTradeBars(SecurityExchangeHours exchange, DateTime end, TimeSpan barSize, int barCount, bool extendedMarketHours)
        {
            if (barSize <= TimeSpan.Zero)
            {
                throw new ArgumentException("barSize must be greater than TimeSpan.Zero", "barSize");
            }

            var current = end.RoundDown(barSize);
            for (int i = 0; i < barCount;)
            {
                var previous = current;
                current = current - barSize;
                if (exchange.IsOpen(current, previous, extendedMarketHours))
                {
                    i++;
                }
            }
            return current;
        }
    }
}
