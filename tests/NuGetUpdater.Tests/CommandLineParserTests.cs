using NuGetUpdater.App;

namespace NuGetUpdater.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void GetFlagValueReturnsValueWhenFlagExists()
    {
        // Arrange
        string?[] args = ["--path", "test"];

        // Act
        string? result = CommandLineParser.GetFlagValue(args, "--path");

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void GetFlagValueReturnsNullWhenFlagDoesNotExist()
    {
        // Arrange
        string?[] args = ["--path", "test"];

        // Act
        string? result = CommandLineParser.GetFlagValue(args, "--invalid");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFlagReturnsFlagWhenFlagExists()
    {
        // Arrange
        string?[] args = ["--major"];

        // Act
        string? result = CommandLineParser.GetFlag(args, ["--major"]);

        // Assert
        Assert.Equal("--major", result);
    }

    [Fact]
    public void GetFlagReturnsNullWhenFlagDoesNotExist()
    {
        // Arrange
        string?[] args = ["--major"];

        // Act
        string? result = CommandLineParser.GetFlag(args, ["--minor"]);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateArgumentsReturnsInvalidWhenPathIsMissing()
    {
        // Arrange
        string?[] args = ["--major"];

        // Act
        (bool isValid, string errorMessage) = CommandLineParser.ValidateArguments(args);

        // Assert
        Assert.False(isValid);
        Assert.Equal("The --path flag is required and must specify a valid project or solution path.", errorMessage);
    }

    [Fact]
    public void ValidateArgumentsReturnsInvalidWhenUpdateTypeIsMissing()
    {
        // Arrange
        string?[] args = ["--path", "test"];

        // Act
        (bool isValid, string errorMessage) = CommandLineParser.ValidateArguments(args);

        // Assert
        Assert.False(isValid);
        Assert.Equal("One of --major, --minor, or --patch flags must be specified.", errorMessage);
    }
}
