namespace Assignment.Infrastructure.Tests;
using Assignment.Core;
using Assignment.Infrastructure;

public class TagRepositoryTests
{

    private readonly KanbanContext _context;
    private readonly TagRepository _repository;
    public TagRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

        context.Add(new Tag("Low priority"));
        context.Add(new Tag("High priority"));
        context.SaveChanges();

        _context = context;
        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Create_tag_returns_created()
    {
        //Arrange
        var tag = new TagCreateDTO("Program");
        var expected = (Created, 3);
        
        //Act
        var actual = _repository.Create(tag);
        
        //Assert
        actual.Should().Be(expected);
    }

   [Fact]
    public void Create_returns_conflict_response_if_tag_already_exists()
    {
        //Arrange
        var tag = new TagCreateDTO("High priority");

        //act
        var actual = _repository.Create(tag);
        
        //assert
        actual.Should().Be((Conflict, 0));
    }

    [Fact]
    public void Delete_returns_deleted_response_given_tag()
    {
        //Arrange
        var actual = _repository.Delete(1, true);
    
        //Assert
        actual.Should().Be(Deleted);
    }

     [Fact]
    public void Delete_tag_in_use_without_force_returns_Conflict()
    {
        //Arrange
        var actual = _repository.Delete(1);
    
        //Assert
        actual.Should().Be(Conflict);
    }

    [Fact]
    public void Delete_returns_NotFound_given_non_existing_Id()
    {
        _repository.Delete(50).Should().Be(NotFound);
    }

    [Fact]
    public void Update_returns_notfound_given_non_existing_Id()
    {
        _repository.Update(new TagUpdateDTO(50, "Program")).Should().Be(NotFound);
    }

    [Fact]
    public void Find_returns_null_given_non_existing_Id()
    {
        _context.Tags.Find(50).Should().Be(null);
    }

    [Fact]
    public void Read_returns_all_tags()
    {
        var actual = _repository.Read();
        actual.Should().BeEquivalentTo(new[] {
            new TagDTO(1,"Low priority"), new TagDTO(2, "High priority")
        });
    }


    public void Dispose()
    {
        _context.Dispose();
    }

}
