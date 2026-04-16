using Practice.Services.Background;
using System.Collections.Concurrent;


public class EmailQueue : IEmailQueue
{
    private readonly ConcurrentQueue<string> _queue = new();

    public void Enqueue(string email)
    {
        _queue.Enqueue(email);
    }

    public bool TryDequeue(out string email)
    {
        return _queue.TryDequeue(out email! );
    }
}