﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkWinter.Utils
{
    public class DateTimeUtils
    {
        /// <summary>
        /// Converts a given DateTime into a Unix timestamp
        /// </summary>
        /// <param name="value">Any DateTime</param>
        /// <returns>The given DateTime in Unix timestamp format</returns>
        public static int ToUnixTimestamp(DateTime value)
        {
            return (int) Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        /// <summary>
        /// Gets a Unix timestamp representing the current moment
        /// </summary>
        /// <returns>Now expressed as a Unix timestamp</returns>
        public static int UnixTimestamp()
        {
            return (int) Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }
    }
}
