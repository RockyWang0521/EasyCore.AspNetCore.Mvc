using Castle.DynamicProxy;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class RemoteInterceptorComposerTests
{
    [Fact]
    public void Wrap_Returns_Original_When_No_Interceptors()
    {
        var services = new ServiceCollection();
        RemoteInterceptorComposer.RegisterCore(services);
        var sp = services.BuildServiceProvider();

        var target = new StubRemoteService();
        var wrapped = RemoteInterceptorComposer.Wrap(typeof(IStubRemoteService), target, sp);

        Assert.Same(target, wrapped);
    }

    [Fact]
    public void Wrap_Stacks_Castle_Proxy_When_Interceptors_Registered()
    {
        var services = new ServiceCollection();
        RemoteInterceptorComposer.RegisterCore(services);
        services.AddSingleton<IAsyncInterceptor, CountingInterceptor>();
        var sp = services.BuildServiceProvider();

        var target = new StubRemoteService();
        var wrapped = RemoteInterceptorComposer.Wrap(typeof(IStubRemoteService), target, sp);

        Assert.NotSame(target, wrapped);
        Assert.IsAssignableFrom<IStubRemoteService>(wrapped);

        var typed = (IStubRemoteService)wrapped;
        Assert.Equal("ok", typed.GetValue());

        var interceptor = sp.GetRequiredService<IAsyncInterceptor>();
        Assert.IsType<CountingInterceptor>(interceptor);
        Assert.Equal(1, ((CountingInterceptor)interceptor).Calls);
    }

    [Fact]
    public void OrderInterceptors_Sorts_By_Order_Then_TypeName()
    {
        var mid = new OrderedInterceptor(50);
        var outer = new OrderedInterceptor(0);
        var inner = new OrderedInterceptor(100);

        var ordered = RemoteInterceptorComposer.OrderInterceptors([mid, inner, outer]);

        Assert.Equal([outer, mid, inner], ordered);
    }

    public interface IStubRemoteService
    {
        string GetValue();
    }

    private sealed class StubRemoteService : IStubRemoteService
    {
        public string GetValue() => "ok";
    }

    private sealed class CountingInterceptor : IAsyncInterceptor
    {
        public int Calls { get; private set; }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            Calls++;
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue!;
            invocation.ReturnValue = task;
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            Calls++;
            invocation.Proceed();
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            Calls++;
            invocation.Proceed();
        }
    }

    private sealed class OrderedInterceptor : IAsyncInterceptor
    {
        public OrderedInterceptor(int order) => Order = order;

        public int Order { get; }

        public void InterceptAsynchronous(IInvocation invocation) => invocation.Proceed();

        public void InterceptAsynchronous<TResult>(IInvocation invocation) => invocation.Proceed();

        public void InterceptSynchronous(IInvocation invocation) => invocation.Proceed();
    }
}
