using System.Reflection;
using System.Xml;

namespace PresalesApp.Database.Helpers;

public static class BusinessTimeCalculator
{
    private static readonly Dictionary<int, byte[,]> _DaysByType = [];

    public static double CalculateBusinessMinutesLOCAL(this DateTime start, DateTime end)
    {
        var time_zone_offset = TimeSpan.FromHours(5);
        start = start.TimeOfDay == new DateTime().TimeOfDay ? start : start + time_zone_offset;
        end = end.TimeOfDay == new DateTime().TimeOfDay ? end : end + time_zone_offset;

        if (start > end) return 0;
        if (start.Date == end.Date) return start.GetBusinessMinutesUTC(end.TimeOfDay);

        if (end.Date - start.Date > TimeSpan.FromDays(1))
        {
            double r = 0;

            foreach (var dt in _EachDay(start.AddDays(1), end.AddDays(-1)))
                r += dt.GetBusinessMinutesUTC();

            return r + start.GetBusinessMinutesUTC() + GetBusinessMinutesUTC(end.Date, end.TimeOfDay);
        }

        return start.GetBusinessMinutesUTC() + GetBusinessMinutesUTC(end.Date, end.TimeOfDay);
    }

    private static void _LoadCalendar(int year)
    {
        try
        {
            if (!_DaysByType.ContainsKey(year))
            {
                var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
                    Directory.GetCurrentDirectory();
                path += "/Calendars/";

                Directory.CreateDirectory(path);
                path = Path.Combine(path, $"{year}.xml");

                if (!File.Exists(path)) _DownloadXmlCalendar(year, path);

                _ParseXmlCalendar(path);
            }
        }
        catch { }
    }

    public static bool IsBusinessDayUTC(this DateTime dt) => dt.Date.GetBusinessMinutesUTC() != 0;

    public static double GetBusinessMinutesUTC(this DateTime dt, TimeSpan? end_of_day = null)
    {
        _LoadCalendar(dt.Year);

        var day_type = _DaysByType.TryGetValue(dt.Year, out var value) ? value?[dt.Month - 1, dt.Day - 1] : 0;

        var start_of_period = dt.TimeOfDay;
        var start_of_day = TimeSpan.FromHours(9);

        end_of_day = end_of_day is null
                     || end_of_day > TimeSpan.FromHours(day_type == 2 ? 17 : 18)
                        ? TimeSpan.FromHours(day_type == 2 ? 17 : 18)
                        : end_of_day;
        start_of_period = start_of_period > start_of_day ? start_of_period : start_of_day;

        return start_of_period > end_of_day
            ? 0
            : day_type switch
        {
            // 1 - выходной день, 2 - рабочий и сокращенный, 3 - рабочий день (суббота/воскресенье)
            0 => dt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                ? 0
                : ((TimeSpan)end_of_day - start_of_period).TotalMinutes,
            1 => 0,
            2 => ((TimeSpan)end_of_day - start_of_period).TotalMinutes,
            3 => ((TimeSpan)end_of_day - start_of_period).TotalMinutes,
            _ => throw new NotImplementedException()
        };
    }

    private static void _ParseXmlCalendar(string path)
    {
        #region Documentation
        /*
         <!--
                year    - год на который сформирован календарь
                lang    - двухбуквенный код языка на котором представлены названия праздников
                date    - дата формирования xml-календаря в формате ГГГГ.ММ.ДД
                country - двухбуквенный код страны
                -->
                <calendar year="2014" lang="ru" date="2014.01.01" country="ru">
	                    <!--
		                    holidays - Список праздников
		                    id - идентификатор праздника
		                    title - название праздника
	                    -->
	                    <holidays>
		                    <holiday id="1" title="Новогодние каникулы" />
		                    <holiday id="2" title="Рождество Христово" />                        
		                    <holiday id="3" title="День защитника Отечества" />
		                    <holiday id="4" title="Международный женский день" />
		                    <holiday id="5" title="Праздник Весны и Труда" />
		                    <holiday id="6" title="День Победы" />
		                    <holiday id="7" title="День России" />
		                    <holiday id="8" title="День народного единства" />
	                    </holidays>
	                    <!--
		                    days - праздники/короткие дни/рабочие дни (суббота либо воскресенье)
		                    d (day) - день (формат ММ.ДД)
		                    t (type) - тип дня: 1 - выходной день, 2 - рабочий и сокращенный
                                (может быть использован для любого дня недели), 3 - рабочий день (суббота/воскресенье)
		                    h (holiday) - номер праздника (ссылка на атрибут id тэга holiday)
		                    f (from) - дата с которой был перенесен выходной день
		                    суббота и воскресенье считаются выходными, если нет тегов day с атрибутом t=2 и t=3 за этот день
	                    -->
	                    <days>
		                    <day d="01.01" t="1" h="1" />
		                    <day d="01.02" t="1" h="1" />
		                    <day d="01.03" t="1" h="1" />
		                    <day d="02.22" t="1" f="01.03" />
		                    ...
	                    </days>
                </calendar>
         */
        #endregion

        var xml_document = new XmlDocument();
        xml_document.Load(path);

        var root = xml_document.DocumentElement;
        if (root is null) return;

        if (!int.TryParse(root.Attributes?.GetNamedItem("year")?.Value, out var year)) return;

        _DaysByType.Add(year, new byte[12, 31]);

        var days = root.GetElementsByTagName("days")[0];
        if (days is null) return;

        foreach (XmlNode node in days)
        {
            var d = node?.Attributes?.GetNamedItem("d");
            if (d is null) continue;

            var day_of_month = d.Value?.Split(".");
            if (day_of_month is null) continue;

            if (!int.TryParse(day_of_month[0], out var month)) continue;
            if (!int.TryParse(day_of_month[1], out var day)) continue;

            var t = node?.Attributes?.GetNamedItem("t");
            if (t is null) continue;

            var type = t?.Value;
            if (type is null) continue;

            if (!byte.TryParse(type, out var value)) continue;

            _DaysByType[year][month - 1, day - 1] = value;
        }
    }

    private static void _DownloadXmlCalendar(int year, string path)
    {
        using var client = new HttpClient();

        var uri = new Uri($"https://xmlcalendar.ru/data/ru/{year}/calendar.xml");
        var response = client.GetAsync(uri).Result;
        if (!response.IsSuccessStatusCode) return;

        using var stream = new FileStream(path, FileMode.CreateNew);
        response.Content.CopyToAsync(stream).Wait();
    }

    private static IEnumerable<DateTime> _EachDay(DateTime from, DateTime to)
    {
        for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
        {
            yield return day;
        }
    }
}
