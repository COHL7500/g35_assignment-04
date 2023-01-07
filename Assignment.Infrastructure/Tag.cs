namespace Assignment.Infrastructure;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<WorkItem> Items { get; set; }

    // Don't use constructor when having object initializer. 
    
    public Tag(string name)
    {
        Name = name;
        Items = new HashSet<WorkItem>();
    }
}
