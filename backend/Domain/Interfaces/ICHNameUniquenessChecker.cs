using Domain.Entities.CompanyHierarchy;

namespace Domain.Interfaces;

public interface ICHNameUniquenessChecker<in T>
    where T: ICHNode
{
    public Task<bool> IsDuplicate(string name, T? existingNode);
}

public interface ICHNameUniquenessChecker<in TParent, in T>
    where TParent : class, ICHNodeWithChildren<T>
    where T: class, ICHNode
{
    public Task<bool> IsDuplicate(TParent parentNode, string name, T? existingNode);
}