using Domain.Entities.CompanyHierarchy;

namespace Domain.Interfaces;

public interface ICHNameUniquenessChecker<in T>
    where T: ICompanyHierarchyNode
{
    public Task<bool> Check(string name, T? existingNode);
}

public interface ICHNameUniquenessChecker<in TParent, in T>
    where TParent : ICompanyHierarchyNode
    where T: ICompanyHierarchyNode
{
    public Task<bool> Check(TParent parentNode, string name, T? existingNode);
}