using Ardalis.Specification.EntityFrameworkCore;
using Domain.Interfaces;

namespace Infrastructure.Database;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : class
{
    private readonly Context _context;

    public Repository(Context context) : base(context)
    {
        _context = context;
    }
}