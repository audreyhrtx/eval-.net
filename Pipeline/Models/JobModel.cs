namespace Pipeline.Models;

public sealed class JobModel
{
    public string Name { get; set; } = string.Empty;

    public List<string> Dependencies { get; set; } = [];
}
