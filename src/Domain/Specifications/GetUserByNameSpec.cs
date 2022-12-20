using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class GetUserByNameSpec : Specification<User>, ISingleResultSpecification
{
    public GetUserByNameSpec(string name)
    {
        Query.Where(u => u.UserName == name);
    }
}