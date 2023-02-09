using Ardalis.Specification;
using Domain.Common;

namespace Domain.Specifications;

public sealed class FileByNameSpec : Specification<PLM_File>
{
    //give back the newest version of a file
    public FileByNameSpec(string name)
    {
        Query.Where(f => f.Name == name).Where(f => f.IsNewest);
    }
}