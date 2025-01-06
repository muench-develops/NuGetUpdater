# NuGet Updater

NuGet Updater is a command-line tool designed to simplify the process of updating NuGet package dependencies in `.csproj` or `packages.config` files. Whether you're working on .NET Framework, .NET Core, or .NET 5+, this tool ensures your projects stay up-to-date with minimal effort.

### 🚀 Features
- **Automatic Dependency Updates**:
  - Update dependencies to the latest **Major**, **Minor**, or **Patch** versions.
- **Project & Solution Support**:
  - Works with individual `.csproj` files or `.sln` solutions.
- **Restore After Update**:
  - Automatically restores the project after updating packages using `dotnet restore` or `msbuild /restore`.
- **Extensive Logging**:
  - Provides detailed logs of operations, including detected versions and updates applied.
- **Beautiful Console Output**:
  - Utilizes Spectre.Console for visually appealing output.

---

## 🛠️ Requirements
1. **.NET SDK**:
   - Ensure `.NET 6+` or `.NET 9+` is installed to run this tool.
2. **MSBuild**:
   - MSBuild must be installed and added to your system's PATH environment variable. 
   - Example path for Visual Studio 2022:
     ```
     C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin
     ```
3. **NuGet Feed**:
   - The tool uses NuGet's public feed by default. Update the `appsettings.json` file if you need to use a custom feed.

---

## 📦 Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/NuGetUpdater.git
   cd NuGetUpdater
    ```

2. Build the project:

   ```bash
   dotnet build
    ```

3. Run the application:
   ```bash
   dotnet run --path "path/to/your/project.csproj" --major --log "path/to/logfile.log"
    ```

4. Publish the application 
To prepare the application for use, publish it as a standalone executable:

   ```bash
    dotnet publish -c Release -r win-x64 --self-contained true -o [TargetFolder]
    ```

- Replace [TargetFolder] with the directory where youd like the published files to be saved. For example, `C:\Tools\NuGetUpdater`.
- The `--self-contained true` flag ensures the executable includes the .NET runtime, so it can run on systems without .NET installed.

Once published, navigate to the [TargetFolder] to run the application or configure it for global use.

 ## 🖥️ Usage
 ### Basic Commands
```bash
NuGetUpdater --path "path/to/your/project.csproj" --major
```

- `--path`: Path to the .csproj or .sln file.
- `--major`: | --minor | --patch: Specify the type of updates to apply.
- `--log`: (Optional) Specify a file path to save logs.

## Examples
1. Update to the latest minor versions:
```bash
NuGetUpdater --path "path/to/your/project.csproj" --minor
```
2. Update all projects in a solution:
```bash
NuGetUpdater --path "path/to/your/solution.sln" --patch --log "nuget-updater.log"
```

## 📖 Contributing
Contributions are more than welcome! While the tool works as intended, it can be improved in many areas:

- Unit Testing:
  - Test coverage is minimal, and robust testing is needed to ensure reliability.
- Code Quality:
  - Refactoring and cleanup are always appreciated!
- New Features:
  - Suggestions and implementations for additional functionality are encouraged.

## How to Contribute
1. Fork the repository.
2. Create a new branch for your feature or fix
```bash
git checkout -b feature/your-feature-name
```
3. Commit your changes and push:
```bash
git push origin feature/your-feature-name
```
4. Open a pull request and describe your changes.

## ✅ To-Do Features
- [ ] Basic Dependency Updates:
  - Support for --major, --minor, and --patch.
- [ ] List Potential Updates:
  - Use the --list flag to preview changes before applying updates.
- [ ] Update All Dependencies:
  - Use the --all flag to update all dependencies to the latest version.
- [ ] Interactive Mode:
  - Add an option to confirm updates interactively.
- [ ]  Dependency Exclusions:
  - Add support for excluding specific dependencies from updates.
- [ ]  Batch Processing:
  - Improve performance by batching operations for large solutions.
- [ ]  Unit Test Integration:
  - Enhance test coverage and introduce regression tests.

## 🪪 License

This project is licensed under the [Apache License 2.0](LICENSE).

You may obtain a copy of the license at:

[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0)