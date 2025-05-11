using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Data;

ACIDDbContextFactory factory = new ACIDDbContextFactory();
var context = factory.CreateDbContext(Array.Empty<string>());

// Readuncommitted'da işlem commit olmamasına rağmen saveasync'den geçince diğer client eklenmiş kabul etti.
// ReadCommitted'da işlem commit olmadan diğer client göremez. Commit olduktan sonra görebilir.
// RepeatableRead'da işlem commit olmadan diğer client select sorgusu yapamaz kitlenir. Bu sayede daha tutarlı veri gösterimi gerçekleşir ama tek başına yeterli olmaz.

using var transactions = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

try
{
    var persons = await context.Persons.ToListAsync();

    var personFirst = persons.FirstOrDefault(p => p.Id == 1);
    var personSecond = persons.FirstOrDefault(p => p.Id == 2);

    if (personFirst.Balance >= 100)
    {
        personFirst.Balance = personFirst.Balance - 100;
        personSecond.Balance = personSecond.Balance + 100;
    }

    await context.SaveChangesAsync();
    await transactions.CommitAsync();
}
catch (Exception)
{
    await transactions.RollbackAsync();    
}

public class ACIDDbContextFactory : IDesignTimeDbContextFactory<ACIDDbContext>
{
    public ACIDDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ACIDDbContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=ACIDDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new ACIDDbContext(optionsBuilder.Options);
    }
}

public class ACIDDbContext(DbContextOptions<ACIDDbContext> opt) : DbContext(opt)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Person> Persons { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
}
