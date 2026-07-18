using EasyCore.AspNetCore.Mvc.RemoteServices;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class K8sDnsTests
{
    [Fact]
    public void BuildK8sBaseAddress_Uses_Standard_Service_Dns_With_Trailing_Slash()
    {
        var uri = RemoteApiK8sClientExtensions.BuildK8sBaseAddress(
            "provider",
            "default",
            "cluster.local",
            port: null);

        Assert.Equal("http://provider.default.svc.cluster.local/", uri.ToString());
    }

    [Fact]
    public void BuildK8sBaseAddress_Includes_Non_Default_Port()
    {
        var uri = RemoteApiK8sClientExtensions.BuildK8sBaseAddress(
            "provider",
            "default",
            "cluster.local",
            port: 8080);

        Assert.Equal("http://provider.default.svc.cluster.local:8080/", uri.ToString());
    }

    [Fact]
    public void BuildK8sBaseAddress_Omits_Port_80()
    {
        var uri = RemoteApiK8sClientExtensions.BuildK8sBaseAddress(
            "provider",
            "default",
            "cluster.local",
            port: 80);

        Assert.Equal("http://provider.default.svc.cluster.local/", uri.ToString());
    }
}
