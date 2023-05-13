using IdGen;

try
{
    var hashset = new HashSet<long>();
    for (var i = 0; i < 50000; i++)
    {
        var id = IdGen<Snowflake, long>.Gen();
        if (hashset.Contains(id))
            throw new Exception("xxx");
        hashset.Add(id);
    }
}
catch (Exception e) 
{
    Console.WriteLine(e.ToString());
}
