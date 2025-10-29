using System.Globalization;

ManualResetEvent canceled = new(false);

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    canceled.Set();
};

string path = @"/var/tmp/logoutput";

string randomStr = Guid.NewGuid().ToString();

DateTimeFormatInfo dtFormat = CultureInfo.InvariantCulture.DateTimeFormat;
string dtFormatStr = dtFormat.UniversalSortableDateTimePattern;

System.Timers.Timer timer = new(5000);

timer.Elapsed += (sender, e) =>
{
    string logLine = $"{DateTime.Now.ToString(dtFormatStr)}: {randomStr}";
    using StreamWriter writer = new(path, append: true);
    writer.WriteLine(logLine);
};

timer.Start();
canceled.WaitOne();
timer.Stop();
canceled.Dispose();
