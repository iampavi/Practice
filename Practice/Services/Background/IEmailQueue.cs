namespace Practice.Services.Background
{
    public interface IEmailQueue
    {
        void Enqueue(string email);
        bool TryDequeue(out string email);
    }
}
