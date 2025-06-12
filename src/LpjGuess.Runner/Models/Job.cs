namespace LpjGuess.Runner.Models;

public class Job
{
    public string Name { get; private init; }
    public string InsFile { get; private init; }

    public Job(string name, string insFile)
    {
        Name = name;
        InsFile = insFile;
    }
}
