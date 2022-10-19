using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
{
    private readonly Context _context;

    public Repository(Context context) : base(context)
    {
        _context = context;
    }
}