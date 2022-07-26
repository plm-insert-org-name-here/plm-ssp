using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Tasks
{
    public interface ITaskCreate
    {
        Task<ActionResult<Create.Res>> _HandleAsync(Create.Req req);
    }
}