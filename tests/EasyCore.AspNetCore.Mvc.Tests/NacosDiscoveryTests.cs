using EasyCore.AspNetCore.Mvc.RemoteServices;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class NacosDiscoveryTests
{
    [Fact]
    public void PickServerAddress_Takes_First_From_Comma_Separated_List()
    {
        var server = NacosServiceDiscovery.PickServerAddress(" http://nacos1:8848 , http://nacos2:8848 ");
        Assert.Equal("http://nacos1:8848", server);
    }

    [Fact]
    public void PickServerAddress_Returns_Null_When_Empty()
    {
        Assert.Null(NacosServiceDiscovery.PickServerAddress(null));
        Assert.Null(NacosServiceDiscovery.PickServerAddress("  "));
    }

    [Fact]
    public void ParseHealthyInstanceUris_Filters_Unhealthy_And_Disabled()
    {
        const string json = """
            {
              "hosts": [
                { "ip": "10.0.0.1", "port": 8080, "healthy": true, "enabled": true },
                { "ip": "10.0.0.2", "port": 8080, "healthy": false, "enabled": true },
                { "ip": "10.0.0.3", "port": 8080, "healthy": true, "enabled": false },
                { "ip": "", "port": 8080, "healthy": true, "enabled": true }
              ]
            }
            """;

        var uris = NacosServiceDiscovery.ParseHealthyInstanceUris(json);

        Assert.Single(uris);
        Assert.Equal("http://10.0.0.1:8080/", uris[0].ToString());
    }

    [Fact]
    public void ParseHealthyInstanceUris_Builds_IPv6_Uri()
    {
        const string json = """
            {
              "hosts": [
                { "ip": "2001:db8::1", "port": 8080, "healthy": true, "enabled": true }
              ]
            }
            """;

        var uris = NacosServiceDiscovery.ParseHealthyInstanceUris(json);
        Assert.Equal("http://[2001:db8::1]:8080/", uris[0].ToString());
    }
}
