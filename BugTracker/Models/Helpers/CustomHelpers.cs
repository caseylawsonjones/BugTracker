using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomHelpers {  //We originally named the class CustomHelpers, when we created it, then changed the namespace and class name
    public static class CustomHTMLHelpers {

        public static IHtmlString ToUserTime(this HtmlHelper helper, DateTimeOffset ModelTime, string timezone) {
            var timezoneId = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var newTime = TimeZoneInfo.ConvertTime(ModelTime, timezoneId);
            string htmlString = newTime.ToString();
            return new HtmlString(htmlString);
        }

        public static IHtmlString ToUserTime(this HtmlHelper helper, DateTimeOffset ModelTime, string timezone, string ToStringFormat) {
            var timezoneId = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var newTime = TimeZoneInfo.ConvertTime(ModelTime, timezoneId);
            string htmlString = newTime.ToString(ToStringFormat);
            return new HtmlString(htmlString);
        }
    }
}