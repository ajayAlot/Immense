

namespace processJobAndSmsApi.Utils
{
    public static class DateUtils
    {
        public static int DateToJulian(int day, int month, int year)
        {
            if (month > 2)
            {
                month -= 3;
            }
            else
            {
                month += 9;
                year -= 1;
            }

            int c = year / 100;
            int ya = year - 100 * c;
            int j = (146097 * c) / 4;
            j += (1461 * ya) / 4;
            j += (153 * month + 2) / 5;
            j += day + 1721119;

            return j;
        }
    }

}