namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }
    
    public (Response Response, int ItemId) Create(WorkItemCreateDTO workItem)
    {
        Response response;
        var entity = new WorkItem(workItem.Title);
        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == workItem.AssignedToId);

        if(assignedUser == null && workItem.AssignedToId != null) return (BadRequest, 0);

        ICollection<Tag> tags = new List<Tag>();
        
        entity.Title = workItem.Title;
        entity.Description = workItem.Description;
        entity.State = New;
        entity.AssignedTo = assignedUser;
        entity.Tags = tags;

        var taskExists = _context.Items.FirstOrDefault(t => t.Id == entity.Id) != null;
        
        if(taskExists)
        {
            return (Conflict, 0);
        }

        entity.Created = DateTime.UtcNow;
        entity.StateUpdated = DateTime.UtcNow;

        _context.Items.Add(entity);
        _context.SaveChanges();

        response = Created;
        

        return (response, entity.Id);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        return ReadByState(Removed);
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        var tagQuery = from t in Read()
            where t.Tags.Contains(tag)
            select t ;

        return tagQuery.Any() ? tagQuery.ToList() : null!;
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        var userQuery = from t in Read() where t.Id == userId select t;
        
        return userQuery.Any() ? userQuery.ToList() : null!;
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var stateQuery = from t in Read() where t.State == state select t;
        
        return stateQuery.Any() ? stateQuery.ToList() : null!;
    }
    
    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        if(!_context.Items.Any())
        {
            return null!;
        }


        var tasks = from t in _context.Items
            select new WorkItemDTO(t.Id, t.Title, t.AssignedTo.Name, t.Tags.Select(x => x.Name).ToList(), t.State);

        return tasks.ToList();
    }

    public Response Update(WorkItemUpdateDTO workitem)
    {
        
        var entity = _context.Items.Find(workitem.Id);
        
        if(entity == null) return NotFound;
        
        var assignedUser = _context.Users.FirstOrDefault(u => u.Id == workitem.AssignedToId);

        if(assignedUser == null) return BadRequest;

        entity.Title = workitem.Title;
        entity.Description = workitem.Description;
        entity.AssignedTo = assignedUser;
        entity.Tags = workitem.Tags.Select(x => new Tag(x))
            .ToList();
       
        if(entity.State != workitem.State) entity.StateUpdated = DateTime.UtcNow;
        
        entity.State = workitem.State;
        
        _context.SaveChanges();
        
        return Updated;
    }

    public Response Delete(int workItemId)
    {
        var workitem = _context.Items.FirstOrDefault(u => u.Id == workItemId);
        
        if(workitem == null)
        {
            return NotFound;
        }
        
        if(workitem.State == New) _context.Items.Remove(workitem);
        else if (workitem.State == Active) {
            workitem.State = Removed;
            workitem.StateUpdated = DateTime.UtcNow;
        }
        else return Conflict;
        
        return Deleted;
    }

    public WorkItemDetailsDTO? Find(int workItemId)
    {
        var taskNotExists = _context.Items.FirstOrDefault(t => t.Id == workItemId) == null;

        if (taskNotExists)
        {
            return null;
        }
        
        var workitem = from t in _context.Items where t.Id == workItemId 
        select new WorkItemDetailsDTO(t.Id, t.Title, t.Description, t.Created, t.AssignedTo.Name, t.Tags.Select(x => x.Name).ToList(), t.State, t.StateUpdated);
        return workitem.First();
    }
}
