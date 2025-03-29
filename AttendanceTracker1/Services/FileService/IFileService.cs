namespace AttendanceTracker1.Services.FileService
{
    public interface IFileService
    {
        Task<byte[]> GetFileAsync(string fileName);
        string GetContentType(string fileName);
    }
}
