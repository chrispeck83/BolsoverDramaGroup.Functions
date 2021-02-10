using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BolsoverDramaGroup.Functions.Factory.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToRfc3339String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
