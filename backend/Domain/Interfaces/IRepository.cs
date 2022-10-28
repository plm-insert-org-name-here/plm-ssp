using Ardalis.Specification;

namespace Domain.Interfaces;

public interface IRepository<T> : IRepositoryBase<T> where T : class
{

}