using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class UserNameUniquenessCheckerSpec : Specification<User>
{
    public UserNameUniquenessCheckerSpec(string Name)
    {
        Query.Where(u => u.UserName == Name);
    }
}