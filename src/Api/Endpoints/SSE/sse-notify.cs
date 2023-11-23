using System.Text;
using System.Threading.Channels;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;
using Newtonsoft.Json;

namespace Api;

public class sse_notify : Endpoint<sse_notify.Req, sse_notify.Res>
{
    public INotifyChannel NotifyChannel { get; set; } = default!;
    public Channel<int> _channel;
    public CancellationToken _ct;
    public class Req
    {
        public int Id { get; set; }
    }

    public class Res
    {
        public int Update { get; set;}
    }

    public override void Configure()
    {
        Get(Api.Routes.Locations.notify);
        AllowAnonymous();
        Options(x => x.WithTags("Locations"));
    }

    public void CheckLife()
    {
        while (true)
        {
            Thread.Sleep(1000000);
            if (_ct.IsCancellationRequested)
            {
                NotifyChannel.Unsubscribe(_channel);
                return;
            }
        }
        
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        _ct = ct;
        _channel = Channel.CreateUnbounded<int>();
        // Console.WriteLine(_channel);
        
        // var ts = new ThreadStart(CheckLife);
        // var backgroundThread = new Thread(ts);
        // backgroundThread.Start();
        
        //subscribe to get the notifications
        NotifyChannel.Subscribe(_channel);
        
        var response = HttpContext.Response;
        response.Headers.Add("connection", "keep-alive");
        response.Headers.Add("cach-control", "no-cache");
        response.Headers.Add("content-type", "text/event-stream");
        
        while (await _channel.Reader.WaitToReadAsync())
        {
            if (ct.IsCancellationRequested) break;
            //
            // if (HttpContext.RequestAborted.IsCancellationRequested)
            // {
            //     Console.WriteLine("lajos");
            // } 
            
            var id = await _channel.Reader.ReadAsync();

            //if the correct location id is in the channel, notify the client
            if (id == req.Id)
            {
                await HttpContext.Response.Body
                    .WriteAsync(Encoding.UTF8.GetBytes($"data: {id}\n\n"), ct);
            
                await HttpContext.Response.Body.FlushAsync(); 
            }

        }
    } 
}
