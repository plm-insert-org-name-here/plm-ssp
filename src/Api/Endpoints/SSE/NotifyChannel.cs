using System.Threading.Channels;

namespace Api;

public interface INotifyChannel
{
    public void AddNotify(int id);
    public void Subscribe(Channel<int> channel);
}

public class NotifyChannel : INotifyChannel
{
    private List<Channel<int>> Subscribers;
    
    public NotifyChannel()
    {
        Subscribers = new List<Channel<int>>(); 
    }

    //if a location need to be refreshed at client side, notify every attached observer about it
    public async void AddNotify(int id)
    {
        Console.WriteLine("------");
        Console.WriteLine(Subscribers.Count);
        Console.WriteLine("------");
        foreach (var channel in Subscribers)
        {
            await channel.Writer.WriteAsync(id);
        }
    }
    
    //add the new to the list
    public void Subscribe(Channel<int> newChannel)
    {
        Subscribers.Add(newChannel);
    }
}