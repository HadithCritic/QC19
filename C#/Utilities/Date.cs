using System;
using System.Collections.Generic;

public class Date
{
    public const int CALENDAR_CHANGE_YEAR = 1582;
    public static string MonthName(int month)
    {
        switch (month)
        {
            case 1: return "January";
            case 2: return "February";
            case 3: return "March";
            case 4: return "April";
            case 5: return "May";
            case 6: return "June";
            case 7: return "July";
            case 8: return "August";
            case 9: return "September";
            case 10: return "October";
            case 11: return "November";
            case 12: return "Decemeber";
            default: return "---------";
        }
    }
    public static string ShortMonthName(int month)
    {
        switch (month)
        {
            case 1: return "Jan";
            case 2: return "Feb";
            case 3: return "Mar";
            case 4: return "Apr";
            case 5: return "May";
            case 6: return "Jun";
            case 7: return "Jul";
            case 8: return "Aug";
            case 9: return "Sep";
            case 10: return "Oct";
            case 11: return "Nov";
            case 12: return "Dec";
            default: return "---";
        }
    }
    public static string WeekdayName(int weekday)
    {
        switch (weekday)
        {
            case 1: return "Sunday";
            case 2: return "Monday";
            case 3: return "Tuesday";
            case 4: return "Wednesday";
            case 5: return "Thursday";
            case 6: return "Friday";
            case 7: return "Saturday";
            default: return "---------";
        }
    }
    public static string ShortWeekdayName(int weekday)
    {
        switch (weekday)
        {
            case 1: return "Sun";
            case 2: return "Mon";
            case 3: return "Tue";
            case 4: return "Wed";
            case 5: return "Thu";
            case 6: return "Fri";
            case 7: return "Sat";
            default: return "---";
        }
    }

    private readonly int m_year = 0;
    private readonly int m_month = 0;
    private readonly int m_day = 0;
    public int Year
    {
        get { return m_year; }
    }
    public int Month
    {
        get { return m_month; }
    }
    public int Day
    {
        get { return m_day; }
    }
    public int Weekday
    {
        get
        {
            int year = this.Year;
            int month = this.Month;
            int day = this.Day;

            int days = FirstDayOfYear(year) - 1;
            for (int i = 1; i < month; i++)
            {
                days += DaysInMonth(year, i);
            }
            days += day - 1;                   // so not count day 1 twice;
            return (days % 7) + 1;             // 1-based index of WeekdayName()
        }
    }
    public Date(int year, int month, int day)
    {
        this.m_year = year;
        this.m_month = month;
        this.m_day = day;
    }
    public override string ToString()
    {
        return
            Year.ToString("0000") + "-" +
            Month.ToString("00") + "-" +
            Day.ToString("00");
    }
    public string ToLongString()
    {
        return
            Year.ToString("0000") + " " +
            MonthName(Month) + " " +
            Day.ToString("00") + " " +
            WeekdayName(Weekday);
    }
    public string ToMediumString()
    {
        return
            this.ToString() + " " +
            WeekdayName(Weekday);
    }
    public string ToShortString()
    {
        return
            Year.ToString("0000") + " " +
            ShortMonthName(Month) + " " +
            Day.ToString("00") + " " +
            ShortWeekdayName(Weekday);
    }

    public static int FirstDayOf0001 = 6; // 01/01/0001 was Saturday
    public static int FirstDayOfYear(int year)
    {
        int days = FirstDayOf0001;
        days += DaysBetweenYears(1, year);
        int weekday = (days % 7) + 1; // 1-based index of WeekdayName()
        return weekday;
    }
    public static int DaysInMonth(int year, int month)
    {
        switch (month)
        {
            case 1:
            case 3:
            case 5:
            case 7:
            case 8:
            case 10:
            case 12:
                {
                    return 31;
                }
            case 4:
            case 6:
            case 9:
            case 11:
                {
                    return 30;
                }
            case 2:
                {
                    if (IsLeapYear(year))
                    {
                        return 29;
                    }
                    else
                    {
                        return 28;
                    }
                }
            default:
                {
                    return -1;
                }
        }
    }

    public static Date AddDays(Date date, int days)
    {
        if (date == null) return new Date(1, 1, 1);
        if (days < 0) return SubtractDays(date, -days);

        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        // guard
        if ((year < 1) || (year > 9999)) return new Date(1, 1, 1);

        // guard
        if ((month < 1) || (month > 12)) return new Date(1, 1, 1);

        // guard
        int month_days = DaysInMonth(year, month);
        if ((day < 1) || (day > month_days)) return new Date(1, 1, 1);

        int years_sum = date.Year;
        int months_sum = date.Month;
        int days_sum = date.Day + days;

        // adjust overflow days_sum
        int days_in_month = DaysInMonth(date.Year, date.Month);
        if (days_sum > days_in_month)
        {
            months_sum++;
        }

        // adjust overflow months_sum
        if (months_sum > 12)
        {
            years_sum++;
        }

        return new Date(years_sum, months_sum, days_sum);
    }
    public static Date AddDates(Date date1, Date date2)
    {
        if (date1 == null) return new Date(1, 1, 1);
        if (date2 == null) return new Date(1, 1, 1);

        int year1 = date1.Year;
        int month1 = date1.Month;
        int day1 = date1.Day;
        int year2 = date2.Year;
        int month2 = date2.Month;
        int day2 = date2.Day;

        // guard
        if ((year1 < 1) || (year1 > 9999)) return new Date(1, 1, 1);
        if ((year2 < 1) || (year2 > 9999)) return new Date(1, 1, 1);

        // guard
        if ((month1 < 1) || (month1 > 12)) return new Date(1, 1, 1);
        if ((month2 < 1) || (month2 > 12)) return new Date(1, 1, 1);

        // guard
        int month1_days = DaysInMonth(year1, month1);
        if ((day1 < 1) || (day1 > month1_days)) return new Date(1, 1, 1);
        int month2_days = DaysInMonth(year2, month1);
        if ((day2 < 1) || (day2 > month2_days)) return new Date(1, 1, 1);

        int years_sum = date1.Year + date2.Year;
        int months_sum = date1.Month + date2.Month;
        int days_sum = date1.Day + date2.Day;

        // adjust overflow days_sum
        int days_in_month = DaysInMonth(date1.Year, date1.Month);
        if (days_sum > days_in_month)
        {
            months_sum++;
        }

        // adjust overflow months_sum
        if (months_sum > 12)
        {
            years_sum++;
        }

        return new Date(years_sum, months_sum, days_sum);
    }
    public static Date SubtractDays(Date date, int days)
    {
        if (date == null) return new Date(1, 1, 1);
        if (days < 0) return AddDays(date, -days);

        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        // guard
        if ((year < 1) || (year > 9999)) return new Date(1, 1, 1);

        // guard
        if ((month < 1) || (month > 12)) return new Date(1, 1, 1);

        // guard
        int month_days = DaysInMonth(year, month);
        if ((day < 1) || (day > month_days)) return new Date(1, 1, 1);

        int years_diff = date.Year;
        int months_diff = date.Month;
        int days_diff = date.Day - days;

        // adjust negative days_diff
        if (days_diff < 0)
        {
            months_diff--;
            days_diff += DaysInMonth(date.Year, date.Month);
        }

        // adjust negative months_diff
        if (months_diff < 0)
        {
            years_diff--;
            months_diff += 12;
        }

        return new Date(years_diff, months_diff, days_diff);
    }
    public static Date SubtractDates(Date date1, Date date2)
    {
        if (date1 == null) return new Date(1, 1, 1);
        if (date2 == null) return new Date(1, 1, 1);

        int year1 = date1.Year;
        int month1 = date1.Month;
        int day1 = date1.Day;
        int year2 = date2.Year;
        int month2 = date2.Month;
        int day2 = date2.Day;

        // guard
        if ((year1 < 1) || (year1 > 9999)) return new Date(1, 1, 1);
        if ((year2 < 1) || (year2 > 9999)) return new Date(1, 1, 1);
        if (year1 > year2) return new Date(1, 1, 1);

        // guard
        if ((month1 < 1) || (month1 > 12)) return new Date(1, 1, 1);
        if ((month2 < 1) || (month2 > 12)) return new Date(1, 1, 1);
        if (year1 == year2)
            if (month1 > month2) return new Date(1, 1, 1);

        // guard
        int month1_days = DaysInMonth(year1, month1);
        if ((day1 < 1) || (day1 > month1_days)) return new Date(1, 1, 1);
        int month2_days = DaysInMonth(year2, month1);
        if ((day2 < 1) || (day2 > month2_days)) return new Date(1, 1, 1);
        if ((year1 == year2) && (month1 == month2))
            if (day1 > day2) return new Date(1, 1, 1);

        int years_diff = date2.Year - date1.Year;
        int months_diff = date2.Month - date1.Month;
        int days_diff = date2.Day - date1.Day;

        // adjust negative days_diff
        if (days_diff < 0)
        {
            months_diff--;
            days_diff += DaysInMonth(date1.Year, date1.Month);
        }

        // adjust negative months_diff
        if (months_diff < 0)
        {
            years_diff--;
            months_diff += 12;
        }

        return new Date(years_diff, months_diff, days_diff);
    }
    public static Date NextDay(Date date)
    {
        return AddDays(date, 1);
    }
    public static Date PreviousDay(Date date)
    {
        return SubtractDays(date, 1);
    }

    public static int DaysBetweenDates(Date date1, Date date2)
    {
        if (date1 == null) return 0;
        if (date2 == null) return 0;

        int days = 0;

        int year1 = date1.Year;
        int month1 = date1.Month;
        int day1 = date1.Day;
        int year2 = date2.Year;
        int month2 = date2.Month;
        int day2 = date2.Day;

        // guard
        if ((year1 < 1) || (year1 > 9999)) return 0;
        if ((year2 < 1) || (year2 > 9999)) return 0;
        if (year1 > year2) return 0;

        // guard
        if ((month1 < 1) || (month1 > 12)) return 0;
        if ((month2 < 1) || (month2 > 12)) return 0;
        if (year1 == year2)
            if (month1 > month2) return 0;

        // guard
        int month1_days = DaysInMonth(year1, month1);
        if ((day1 < 1) || (day1 > month1_days)) return 0;
        int month2_days = DaysInMonth(year2, month1);
        if ((day2 < 1) || (day2 > month2_days)) return 0;
        if ((year1 == year2) && (month1 == month2))
            if (day1 > day2) return 0;

        // add days of full months
        for (int i = year1; i <= year2; i++)
        {
            for (int j = 1; j <= 12; j++)
            {
                if ((i == year1) && (j <= month1))
                    continue;

                if ((i == year2) && (j >= month2))
                    continue;

                days += DaysInMonth(i, j);
            }
        }

        // add days in first and last months
        if ((year1 == year2) && (month1 == month2))
        {
            days += day2 - day1;
        }
        else
        {
            // add days before full months
            days += DaysInMonth(year1, month1) - day1;
            // add days after full months
            days += day2;
        }

        return days;
    }
    public static int WeeksBetweenDates(Date date1, Date date2)
    {
        if (date1 == null) return 0;
        if (date2 == null) return 0;

        int weeks = DaysBetweenDates(date1, date2) / 7;
        return weeks;
    }
    public static int WeekdaysBetweenDates(Date date1, Date date2, int weekday)
    {
        if (date1 == null) return 0;
        if (date2 == null) return 0;

        int weekdays = 0;

        int year1 = date1.Year;
        int month1 = date1.Month;
        int day1 = date1.Day;
        int year2 = date2.Year;
        int month2 = date2.Month;
        int day2 = date2.Day;

        // guard
        if ((year1 < 1) || (year1 > 9999)) return 0;
        if ((year2 < 1) || (year2 > 9999)) return 0;
        if (year1 > year2) return 0;

        // guard
        if ((month1 < 1) || (month1 > 12)) return 0;
        if ((month2 < 1) || (month2 > 12)) return 0;
        if (year1 == year2)
            if (month1 > month2) return 0;

        // guard
        int month1_days = DaysInMonth(year1, month1);
        if ((day1 < 1) || (day1 > month1_days)) return 0;
        int month2_days = DaysInMonth(year2, month1);
        if ((day2 < 1) || (day2 > month2_days)) return 0;
        if ((year1 == year2) && (month1 == month2))
            if (day1 > day2) return 0;

        int remainder_days = DaysBetweenDates(date1, date2);
        if (remainder_days >= 0) // same dates and matching our weekday
        {
            int max = remainder_days;
            for (int i = 0; i < max; i++)
            {
                remainder_days--;

                Date date = Date.AddDays(date1, i);
                if (date != null)
                {
                    if (date.Weekday == weekday)
                    {
                        weekdays++; // add first matching weekday
                        break;
                    }
                }
            }

            weekdays += remainder_days / 7; // full weeks' weekday
            remainder_days = (remainder_days % 7);

            max = remainder_days + 1;
            for (int i = 0; i < max; i++)
            {
                Date date = Date.SubtractDays(date2, i);
                remainder_days--;

                if (date != null)
                {
                    if (date.Weekday == weekday)
                    {
                        weekdays++; // add last matching weekday
                        break;
                    }
                }
            }
        }

        return weekdays;
    }

    public static int DaysBetweenYears(int year1, int year2)
    {
        // guard
        if ((year1 < 1) || (year1 > 9999)) return 0;
        if ((year2 < 1) || (year2 > 9999)) return 0;
        if (year1 > year2) return 0;

        int days = 0;
        for (int i = year1; i < year2; i++)
        {
            if (IsLeapYear(i))
            {
                days += 366;
            }
            else // normal year
            {
                days += 365;
                if (i == CALENDAR_CHANGE_YEAR)
                {
                    days -= 10; // CALENDAR_CHANGE_YEAR jumped 10 days from Monday 4 Oct to Friday 15 Oct
                }
            }
        }
        return days;
    }
    public static int LeapYearsBetweenDates(Date date1, Date date2)
    {
        if (date1 == null) return 0;
        if (date2 == null) return 0;

        int count = 0;

        int year1 = date1.Year;
        int month1 = date1.Month;
        int day1 = date1.Day;
        int year2 = date2.Year;
        int month2 = date2.Month;
        int day2 = date2.Day;

        // guard
        if ((year1 < 1) || (year1 > 9999)) return 0;
        if ((year2 < 1) || (year2 > 9999)) return 0;
        if (year1 > year2) return 0;

        // guard
        if ((month1 < 1) || (month1 > 12)) return 0;
        if ((month2 < 1) || (month2 > 12)) return 0;
        if (year1 == year2)
            if (month1 > month2) return 0;

        // guard
        int month1_days = DaysInMonth(year1, month1);
        if ((day1 < 1) || (day1 > month1_days)) return 0;
        int month2_days = DaysInMonth(year2, month1);
        if ((day2 < 1) || (day2 > month2_days)) return 0;
        if ((year1 == year2) && (month1 == month2))
            if (day1 > day2) return 0;

        for (int i = year1; i <= year2; i++)
        {
            if ((i == year1) && (month1 > 2))
                continue; // after leap day

            if ((i == year2) && (month2 <= 2))
                continue; // before leap day

            if (IsLeapYear(i))
            {
                count++;
            }
        }

        return count;
    }
    public static List<int> YearsWith53WeekdaysList(int year1, int year2, int weekday)
    {
        List<int> result = new List<int>();
        for (int i = year1; i <= year2; i++)
        {
            if (DoesYearHave53Weekdays(i, weekday))
            {
                result.Add(i);
            }
        }
        return result;
    }
    public static int YearsWith53Weekdays(int year1, int year2, int weekday)
    {
        int count = 0;
        for (int i = year1; i <= year2; i++)
        {
            if (DoesYearHave53Weekdays(i, weekday))
            {
                count++;
            }
        }
        return count;
    }
    public static bool IsLeapYear(int year)
    {
        // guard
        if ((year < 1) || (year > 9999)) return false;

        // Julian Calendar
        if (year < CALENDAR_CHANGE_YEAR) // every multiple of 4 years
        {
            return ((year % 4) == 0);
        }
        else if (year == CALENDAR_CHANGE_YEAR)
        {
            return false; // 10 days short and isn't multiple of 4 anyway
        }
        // Gregorian Calendar
        else if (year > CALENDAR_CHANGE_YEAR) // every multiple of 4 years except multiples of 100 but not 400
        {
            if ((year % 4) == 0)
            {
                if ((year % 100) == 0)
                {
                    if ((year % 400) == 0)
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        return false;
    }
    public static bool IsValidDate(Date date)
    {
        if (date == null) return false;

        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if ((year < 1) || (year > 9999)) return false;
        if ((month < 1) || (month > 12)) return false;
        int month_days = DaysInMonth(year, month);
        if ((day < 1) || (day > month_days)) return false;
        return true;
    }
    public static bool DoesYearHave53Weekdays(int year, int weekday)
    {
        if (IsLeapYear(year))
        {
            return
            (
              (FirstDayOfYear(year) == weekday)
              ||
              (FirstDayOfYear(year) == weekday - 1)
            );
        }
        else
        {
            return (FirstDayOfYear(year) == weekday);
        }
    }
}
