using System.Diagnostics;
using System.Text;

try
{
    var stopWatch = Stopwatch.StartNew();
    var sb = new StringBuilder();
    for (int i = 0; i < 100; i++)
    {
        var s = Convert.ToString(stopWatch.ElapsedTicks << 32, 2);
        if (s.Length < 64)
        {
            for (int j = 0; j < 64 - s.Length; j++)
                sb.Append('0');
            sb.Append(s);
            s = sb.ToString();
            sb.Clear();
        }
        Console.WriteLine(s);
    }

    for (int i = 0; i < 32; i++)
    {
        sb.Append('x');
    }    
    for (int i = 0; i < 32; i++)
    {
        sb.Append('y');
    }
    Console.WriteLine(sb.ToString());
    
}
catch (Exception e) 
{
    Console.WriteLine(e.ToString());
}
