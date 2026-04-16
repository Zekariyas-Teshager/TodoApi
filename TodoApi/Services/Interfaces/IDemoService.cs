namespace TodoApi.Services.Interfaces
{
    public interface IDemoService
    {
        string GetInstanceId();
        DateTime GetCreatedAt();
        Dictionary<string, object> GetInfo();
    }
}
