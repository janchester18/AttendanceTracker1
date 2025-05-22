namespace DENR_IHRMIS.Data
{
    public static class DateTimeHelper
    {
        public static DateTime ConvertToPST(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila"));
        }
    }
}
