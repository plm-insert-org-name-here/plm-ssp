namespace Domain.Entities.CompanyHierarchy;

public interface ICompanyHierarchyNode<TParent, TChild>
    : ICompanyHierarchyNodeWithChildren<TChild>, ICompanyHierarchyNodeWithParent<TParent>
    where TParent : ICompanyHierarchyNode
    where TChild : ICompanyHierarchyNode
{
}

public interface ICompanyHierarchyNodeWithParent<TParent> : ICompanyHierarchyNode
    where TParent : ICompanyHierarchyNode
{
    public TParent Parent { get; set; }
    public int ParentId { get; set; }
}

public interface ICompanyHierarchyNodeWithChildren<TChild> : ICompanyHierarchyNode
    where TChild : ICompanyHierarchyNode
{
    public List<TChild> Children { get; set; }
}

public interface ICompanyHierarchyNode : IBaseEntity
{
    public string Name { get; set; }
}