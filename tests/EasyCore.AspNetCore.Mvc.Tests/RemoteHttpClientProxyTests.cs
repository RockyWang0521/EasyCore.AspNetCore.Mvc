using System.Reflection;
using EasyCore.AspNetCore.Mvc.RemoteServices;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class RemoteHttpClientProxyTests
{
    [Fact]
    public void AppendQuery_Uses_Real_Parameter_Names()
    {
        var method = typeof(ISample).GetMethod(nameof(ISample.DeleteDto))!;
        var route = RemoteHttpClientProxy.AppendQuery("api/Sample/DeleteDto", method, new object?[] { 42 });

        Assert.Equal("api/Sample/DeleteDto?id=42", route);
    }

    [Fact]
    public void GetHttpMethodFromName_Throws_For_Unknown_Verb()
    {
        Assert.Throws<InvalidOperationException>(() => RemoteHttpClientProxy.GetHttpMethodFromName("ExecuteSomething"));
    }

    [Theory]
    [InlineData("GetDto", "GET")]
    [InlineData("PostDto", "POST")]
    [InlineData("PutDto", "PUT")]
    [InlineData("DeleteDto", "DELETE")]
    public void GetHttpMethodFromName_Maps_Known_Prefixes(string name, string verb)
    {
        Assert.Equal(verb, RemoteHttpClientProxy.GetHttpMethodFromName(name));
    }

    private interface ISample
    {
        Task DeleteDto(int id);
    }
}
