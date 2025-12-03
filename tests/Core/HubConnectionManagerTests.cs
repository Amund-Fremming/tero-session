using Microsoft.Extensions.Logging;
using Moq;
using tero.session.src.Core;
using tero.session.src.Features.Platform;

namespace tero.session.tests.Core;

public class HubConnectionManagerTests
{
    private readonly CacheTTLOptions _options;
    private readonly HubConnectionManager<TestSession> _manager;

    public HubConnectionManagerTests()
    {
        _options = new CacheTTLOptions { SessionMinuttes = 10, ManagerMinuttes = 30 };
        _manager = new HubConnectionManager<TestSession>(
            Mock.Of<ILogger<HubConnectionManager<TestSession>>>(),
            _options,
            null! // PlatformClient is optional for testing
        );
    }

    [Fact]
    public void Insert_WithValidConnectionId_ShouldReturnTrue()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());

        // Act
        var result = _manager.Insert(connectionId, hubInfo);

        // Assert
        Assert.True(result.IsOk());
    }

    [Fact]
    public void Insert_WithDuplicateConnectionId_ShouldReturnFalse()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo1 = new HubInfo("game-key-1", Guid.NewGuid());
        var hubInfo2 = new HubInfo("game-key-2", Guid.NewGuid());

        // Act
        _manager.Insert(connectionId, hubInfo1);
        var result = _manager.Insert(connectionId, hubInfo2);

        // Assert
        Assert.True(result.IsErr());
    }

    [Fact]
    public void Insert_ShouldSetTtlOnHubInfo()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());
        var initialExpiry = hubInfo.ExpiresAt;

        // Act
        _manager.Insert(connectionId, hubInfo);

        // Assert
        Assert.True(hubInfo.ExpiresAt > initialExpiry);
    }

    [Fact]
    public void Get_WithExistingConnectionId_ShouldReturnSome()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());
        _manager.Insert(connectionId, hubInfo);

        // Act
        var result = _manager.Get(connectionId);

        // Assert
        Assert.True(result.IsOk());
        var option = result.Unwrap();
        Assert.True(option.IsSome());
        var retrievedInfo = option.Unwrap();
        Assert.Equal("game-key", retrievedInfo.GameKey);
    }

    [Fact]
    public void Get_WithNonExistingConnectionId_ShouldReturnNone()
    {
        // Arrange
        var connectionId = "non-existing-conn";

        // Act
        var result = _manager.Get(connectionId);

        // Assert
        Assert.True(result.IsOk());
        var option = result.Unwrap();
        Assert.True(option.IsNone());
    }

    [Fact]
    public void Remove_WithExistingConnectionId_ShouldReturnSome()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());
        _manager.Insert(connectionId, hubInfo);

        // Act
        var result = _manager.Remove(connectionId);

        // Assert
        Assert.True(result.IsOk());
        var option = result.Unwrap();
        Assert.True(option.IsSome());
        var removedInfo = option.Unwrap();
        Assert.Equal("game-key", removedInfo.GameKey);
    }

    [Fact]
    public void Remove_WithNonExistingConnectionId_ShouldReturnNone()
    {
        // Arrange
        var connectionId = "non-existing-conn";

        // Act
        var result = _manager.Remove(connectionId);

        // Assert
        Assert.True(result.IsOk());
        var option = result.Unwrap();
        Assert.True(option.IsNone());
    }

    [Fact]
    public void Remove_ShouldRemoveFromManager()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());
        _manager.Insert(connectionId, hubInfo);

        // Act
        _manager.Remove(connectionId);
        var getResult = _manager.Get(connectionId);

        // Assert
        Assert.True(getResult.IsOk());
        var option = getResult.Unwrap();
        Assert.True(option.IsNone());
    }

    [Fact]
    public void GetCopy_ShouldReturnCopyOfManager()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());
        _manager.Insert(connectionId, hubInfo);

        // Act
        var copy = _manager.GetCopy();

        // Assert
        Assert.Single(copy);
        Assert.True(copy.ContainsKey(connectionId));
    }

    [Fact]
    public void GetCopy_ModifyingCopy_ShouldNotAffectOriginal()
    {
        // Arrange
        var connectionId = "conn-123";
        var hubInfo = new HubInfo("game-key", Guid.NewGuid());
        _manager.Insert(connectionId, hubInfo);

        // Act
        var copy = _manager.GetCopy();
        copy.TryRemove(connectionId, out _);

        // Assert
        var originalGet = _manager.Get(connectionId);
        Assert.True(originalGet.IsOk());
        var option = originalGet.Unwrap();
        Assert.True(option.IsSome());
    }

    [Fact]
    public void Insert_MultipleDifferentConnections_ShouldSucceed()
    {
        // Arrange
        var conn1 = "conn-1";
        var conn2 = "conn-2";
        var hubInfo1 = new HubInfo("game-key-1", Guid.NewGuid());
        var hubInfo2 = new HubInfo("game-key-2", Guid.NewGuid());

        // Act
        var result1 = _manager.Insert(conn1, hubInfo1);
        var result2 = _manager.Insert(conn2, hubInfo2);

        // Assert
        Assert.True(result1.IsOk());
        Assert.True(result2.IsOk());
        var copy = _manager.GetCopy();
        Assert.Equal(2, copy.Count);
    }
}
