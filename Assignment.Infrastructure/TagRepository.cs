using System.Collections.Immutable;

namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;
    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        Response response;
        
        var entity = new Tag(tag.Name);

        var userExists = _context.Users.FirstOrDefault(t => t.Name == tag.Name) != null;
        
        if(userExists) 
        {
            return (Response.Conflict, 0);
        }
        
        _context.Tags.Add(entity);
        _context.SaveChanges();

        response = Response.Created;
        
        return (response, entity.Id); 
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        if (!_context.Tags.Any())
        {
            return null!;
        }
        
        var tags = from u in _context.Tags
            select new TagDTO(u.Id, u.Name);

        return tags.ToList();
    }

    public TagDTO? Find(int tagId)
    {
        var readTag = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        return readTag == null ? null : new TagDTO(readTag.Id, readTag.Name);
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.Find(tag.Id);
        if(entity == null) return Response.NotFound;
        entity.Name = tag.Name;
        _context.SaveChanges();

        return Response.Updated;
    }

    public Response Delete(int tagId, bool force = false)
    {
        var tag = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        if(tag == null)
        {
            return Response.NotFound;
        } 
        
        if(!force) 
        {
            return Response.Conflict;
        }
        
        _context.Tags.Remove(tag);
        return Response.Deleted;
    }
}
