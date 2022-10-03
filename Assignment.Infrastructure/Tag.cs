namespace Assignment.Infrastructure;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<WorkItem> Items { get; set; }

    public Tag(string name)
    {
        Name = name;
        Items = new HashSet<WorkItem>();
    }
}
