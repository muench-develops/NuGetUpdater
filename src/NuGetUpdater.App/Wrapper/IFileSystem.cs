namespace NuGetUpdater.App.Wrapper;

public interface IFileSystem
{
    bool FileExists(string path);
    IEnumerable<string> ReadAllLines(string path);
    void SaveFile(string path, string content);
    string GetDirectoryName(string path);
}
