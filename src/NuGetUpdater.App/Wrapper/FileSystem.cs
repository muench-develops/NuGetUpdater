namespace NuGetUpdater.App.Wrapper;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    public IEnumerable<string> ReadAllLines(string path) => File.ReadAllLines(path);
    public void SaveFile(string path, string content) => File.WriteAllText(path, content);
    public string GetDirectoryName(string path) => Path.GetDirectoryName(path) ?? string.Empty;
}
