using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Tasks.Interfaces
{
    public interface ITaskGetAll
    {
        Task<List<GetAll.Res>> HandleAsync(CancellationToken ct = new());
    }
}