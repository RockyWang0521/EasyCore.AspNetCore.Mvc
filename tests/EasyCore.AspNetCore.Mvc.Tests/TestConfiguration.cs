using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace EasyCore.AspNetCore.Mvc.Tests;

internal sealed class TestConfiguration : IConfiguration
{
    private readonly Dictionary<string, string?> _values;

    public TestConfiguration(IDictionary<string, string?> values)
        => _values = new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase);

    public string? this[string key]
    {
        get => _values.TryGetValue(key, out var value) ? value : null;
        set => _values[key] = value;
    }

    public IEnumerable<IConfigurationSection> GetChildren() => Array.Empty<IConfigurationSection>();

    public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);

    public IConfigurationSection GetSection(string key) => new TestConfigurationSection(this, key);

    private sealed class TestConfigurationSection : IConfigurationSection
    {
        private readonly TestConfiguration _root;
        private readonly string _key;

        public TestConfigurationSection(TestConfiguration root, string key)
        {
            _root = root;
            _key = key;
        }

        public string? this[string key]
        {
            get => _root[$"{_key}:{key}"];
            set => _root[$"{_key}:{key}"] = value;
        }

        public string Key => _key;
        public string Path => _key;
        public string? Value
        {
            get => _root[_key];
            set => _root[_key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => Array.Empty<IConfigurationSection>();
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new TestConfigurationSection(_root, $"{_key}:{key}");
    }
}
