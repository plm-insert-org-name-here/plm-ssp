using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Tasks
{
    public interface ITaskGetAll
    {
        Task<List<GetAll.Res>> HandleAsync(CancellationToken ct = new());
    }
}