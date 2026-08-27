using Tips_Player.Infrastructure.Persistence;
using Tips_Player.Infrastructure.Validation;
using Tips_Player.Models;

namespace Tips_Player.Tests.Infrastructure;

public sealed class AtomicJsonFileStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"tips-player-tests-{Guid.NewGuid():N}");

    public AtomicJsonFileStoreTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public async Task RoundTrip_UsesProductionStore()
    {
        var path = Path.Combine(_directory, "data.json");
        var store = new AtomicJsonFileStore();
        var expected = new[] { "one", "two" };
        await store.WriteAsync(path, expected, 1024);

        var actual = await store.ReadAsync<string[]>(path, 1024);

        actual.Should().Equal(expected);
        File.Exists(path + ".tmp").Should().BeFalse();
    }

    [Fact]
    public async Task Read_RejectsOversizedData()
    {
        var path = Path.Combine(_directory, "large.json");
        await File.WriteAllTextAsync(path, "\"too large\"");
        var store = new AtomicJsonFileStore();
        var action = () => store.ReadAsync<string>(path, 4);
        await action.Should().ThrowAsync<InvalidDataException>();
    }

    [Fact]
    public void MediaValidator_RejectsNetworkSources()
    {
        var item = new MediaItem { Title = "Remote", FilePath = "https://example.com/audio.mp3" };
        MediaItemValidator.Validate(item).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MediaValidator_AcceptsContentUri()
    {
        var item = new MediaItem { Title = "Local", FilePath = "content://media/external/audio/42" };
        MediaItemValidator.Validate(item).IsSuccess.Should().BeTrue();
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
