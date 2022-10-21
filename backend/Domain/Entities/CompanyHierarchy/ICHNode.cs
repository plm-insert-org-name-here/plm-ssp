using System.ComponentModel;
using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public interface ICHNode<TParent, TChild>
    : ICHNodeWithChildren<TChild>, ICHNodeWithParent<TParent>
    where TParent : class, ICHNode
    where TChild : class, ICHNode
{
}

public interface ICHNodeWithParent<TParent> : ICHNode
    where TParent : class, ICHNode
{
    public TParent Parent { get; set; }
    public int ParentId { get; set; }
}

public interface ICHNodeWithChildren<TChild> : ICHNode
    where TChild : class, ICHNode
{
    public List<TChild> Children { get; set; }

    public Result<TChild> AddChildNode<TParent>(string childNodeName,
        ICHNameUniquenessChecker<TParent, TChild> nameUniquenessChecker)
        where TParent : class, ICHNodeWithChildren<TChild>;
}

public interface ICHNode : IBaseEntity
{
    public string Name { get; set; }

}