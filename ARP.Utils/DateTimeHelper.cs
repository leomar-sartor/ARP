namespace ARP.Utils
{
    public static class DateTimeHelper
    {
        public static DateTime CreateUtcDateTime(int days = 0)
        {
            DateTime data;

            if (days != 0)
            {
                data = DateTime.SpecifyKind((DateTime.Now.AddDays(days)), DateTimeKind.Utc);
                return data;
            }

            data = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            return data;
        }

        public static DateTime ToUtcDateTime(DateTime date)
        {
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond), DateTimeKind.Utc);
        }


        public static DateTime ToUtcDateTime(int Year, int Month, int Day, int Hour, int Minute, int Second, int Millisecond)
        {
            return DateTime.SpecifyKind(new DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond), DateTimeKind.Utc);
        }

        public static DateTime ToUtcDateTime000000(DateTime date)
        {
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0), DateTimeKind.Utc);
        }

        public static DateTime ToUtcDateTime235959(DateTime date)
        {
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999), DateTimeKind.Utc);
        }

        public static DateTime ToUtcDateNow(DateTime date)
        {
            var agora = DateTime.Now;
            return DateTime.SpecifyKind(new DateTime(date.Year, date.Month, date.Day, agora.Hour, agora.Minute, agora.Second, agora.Millisecond), DateTimeKind.Utc);
        }
    }
}
