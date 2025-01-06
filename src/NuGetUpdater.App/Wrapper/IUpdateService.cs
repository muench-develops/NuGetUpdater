namespace NuGetUpdater.App.Wrapper;

public interface IUpdateService
{
    Task<bool> UpdateDependenciesAsync(string projectPath, string updateType);
}
