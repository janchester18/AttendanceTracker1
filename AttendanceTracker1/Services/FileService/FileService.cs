namespace AttendanceTracker1.Services.FileService
{
    public class FileService : IFileService
    {
        private readonly string _uploadsFolder;

        public FileService(IWebHostEnvironment env)
        {
            // Get the absolute path to the uploads folder.
            _uploadsFolder = Path.Combine(env.ContentRootPath, "uploads");
        }

        public async Task<byte[]> GetFileAsync(string fileName)
        {
            var filePath = Path.Combine(_uploadsFolder, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", fileName);
            }
            return await File.ReadAllBytesAsync(filePath);
        }

        public string GetContentType(string fileName)
        {
            // A basic mapping from file extension to MIME type.
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }
}
