namespace Assignment.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly KanbanContext _context;

    public UserRepository(KanbanContext context)
    {
        _context = context;
    }
    
    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        Response response;
        var entity = new User(user.Email, user.Name);

        var userExists = _context.Users.FirstOrDefault(u => u.Email == user.Email) != null;
        if(userExists) 
        {
            return (Conflict, 0);
        }
        
        _context.Users.Add(entity);
        _context.SaveChanges();

        response = Created;


        return (response, entity.Id); 
    }

    public Response Delete(int userId, bool force = false)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if(user == null)
        {
            return NotFound;
        } 
        
        bool AssignedToTask = false;
        foreach(var task in user.Items) {
            if (task.State == Active) {
                AssignedToTask = true;
                break;
            }
        }

        if(AssignedToTask && !force) 
        {
            return Conflict;
        }
        
        _context.Users.Remove(user!);
        return Deleted;
    }
    

    public UserDTO? Find(int userId)
    {
        var ReadUser = _context.Users.FirstOrDefault(u => u.Id == userId);
        return ReadUser == null ? null : new UserDTO(ReadUser.Id, ReadUser.Name, ReadUser.Email);
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        if (!_context.Users.Any())
        {
            return null!;
        }
        
        var users = from u in _context.Users
                    select new UserDTO(u.Id, u.Name, u.Email);

        return users.ToArray();
    }

    public Response Update(UserUpdateDTO user)
    {
        var entity = _context.Users.Find(user.Id);
        if(entity == null) return NotFound;
        entity.Name = user.Name;
        entity.Email = user.Email;
        _context.SaveChanges();

        return Updated;
    }
}
