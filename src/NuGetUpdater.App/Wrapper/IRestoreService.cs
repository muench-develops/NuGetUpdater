namespace NuGetUpdater.App.Wrapper;

public interface IRestoreService
{
    void Restore(string projectFilePath);
}
