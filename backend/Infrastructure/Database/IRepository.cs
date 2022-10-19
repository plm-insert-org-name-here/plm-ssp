using Ardalis.Specification;

namespace Infrastructure.Database;

public interface IRepository<T> : IRepositoryBase<T> where T : class
{

}