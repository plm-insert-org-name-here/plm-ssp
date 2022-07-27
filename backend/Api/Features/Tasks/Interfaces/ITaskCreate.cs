using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Tasks.Interfaces
{
    public interface ITaskCreate
    {
        Task<ActionResult<Create.Res>> _HandleAsync(Create.Req req);
    }
}