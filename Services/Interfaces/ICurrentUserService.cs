namespace StudentRecordManager.Services.Interfaces
{
    public interface ICurrentUserService
    {
        string Department { get; }
        bool IsAdmin { get; }
    }
}