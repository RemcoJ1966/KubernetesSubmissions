using System.Globalization;

ManualResetEvent canceled = new(false);

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    canceled.Set();
};

string randomStr = Guid.NewGuid().ToString();

System.Timers.Timer timer = new(5000);

DateTimeFormatInfo dtFormat = CultureInfo.InvariantCulture.DateTimeFormat;
string dtFormatStr = dtFormat.UniversalSortableDateTimePattern;

timer.Elapsed += (sender, e) =>
    Console.WriteLine($"{DateTime.Now.ToString(dtFormatStr)}: {randomStr}");

timer.Start();
canceled.WaitOne();
timer.Stop();
canceled.Dispose();
