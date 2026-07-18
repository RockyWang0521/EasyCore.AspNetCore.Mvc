using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.AspNetCore.Mvc.RemoteServices.DaprOptions;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class DaprInvokeTests
{
    [Fact]
    public void BuildDaprInvokeRelativeUri_Rewrites_Path_To_Invoke_Method()
    {
        var uri = RemoteApiDaprClientExtensions.BuildDaprInvokeRelativeUri(
            "provider",
            "api/Foo/GetDto");

        Assert.Equal("v1.0/invoke/provider/method/api/Foo/GetDto", uri.ToString());
    }

    [Fact]
    public void BuildDaprInvokeRelativeUri_Preserves_Query_String()
    {
        var uri = RemoteApiDaprClientExtensions.BuildDaprInvokeRelativeUri(
            "provider",
            "/api/Foo/DeleteDto?id=1");

        Assert.Equal("v1.0/invoke/provider/method/api/Foo/DeleteDto?id=1", uri.ToString());
    }

    [Fact]
    public void RewriteRequestUri_Keeps_Absolute_Authority()
    {
        var rewritten = DaprResolvingHandler.RewriteRequestUri(
            new Uri("http://127.0.0.1:3500/api/Foo/GetDto"),
            "provider");

        Assert.Equal(
            "http://127.0.0.1:3500/v1.0/invoke/provider/method/api/Foo/GetDto",
            rewritten.ToString());
    }

    [Fact]
    public void ResolveHttpEndpoint_Defaults_To_Local_3500()
    {
        var option = new DaprOption { HttpEndpoint = string.Empty };
        Assert.Equal("http://127.0.0.1:3500/", option.ResolveHttpEndpoint().ToString());
    }

    [Fact]
    public void ResolveHttpEndpoint_Appends_Trailing_Slash()
    {
        var option = new DaprOption { HttpEndpoint = "http://127.0.0.1:3501" };
        Assert.Equal("http://127.0.0.1:3501/", option.ResolveHttpEndpoint().ToString());
    }
}
