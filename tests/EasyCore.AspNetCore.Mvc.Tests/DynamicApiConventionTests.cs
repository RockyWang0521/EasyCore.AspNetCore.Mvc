using System.Reflection;
using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class DynamicApiConventionTests
{
    [Fact]
    public void Route_Convention_Does_Not_Rewrite_Ordinary_Controllers()
    {
        var convention = new EasyCoreAspNetCoreMvcDynamicApiControllerRoute();
        var controller = CreateControllerModel(typeof(OrdinaryController), nameof(OrdinaryController.GetValue));
        var originalTemplate = controller.Actions[0].Selectors[0].AttributeRouteModel!.Template;

        convention.Apply(new ApplicationModel { Controllers = { controller } });

        Assert.Equal(originalTemplate, controller.Actions[0].Selectors[0].AttributeRouteModel!.Template);
    }

    [Fact]
    public void Route_Convention_Rewrites_AppService_Controllers()
    {
        var convention = new EasyCoreAspNetCoreMvcDynamicApiControllerRoute();
        var controller = CreateControllerModel(typeof(SampleAppService), nameof(SampleAppService.GetDto));

        convention.Apply(new ApplicationModel { Controllers = { controller } });

        Assert.Equal("GetDto", controller.Actions[0].Selectors[0].AttributeRouteModel!.Template);
        Assert.Contains(controller.Actions[0].Selectors[0].ActionConstraints!,
            c => c is Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint);
    }

    private static ControllerModel CreateControllerModel(Type controllerType, string actionName)
    {
        var method = controllerType.GetMethod(actionName)!;
        var action = new ActionModel(method, Array.Empty<object>())
        {
            ActionName = actionName
        };
        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("original"))
        });

        var controller = new ControllerModel(controllerType.GetTypeInfo(), Array.Empty<object>())
        {
            ControllerName = controllerType.Name
        };
        controller.Actions.Add(action);
        action.Controller = controller;
        return controller;
    }

    private sealed class OrdinaryController : ControllerBase
    {
        public string GetValue() => "ok";
    }

    private sealed class SampleAppService : EasyCoreAppService
    {
        public Task<string> GetDto() => Task.FromResult("ok");
    }
}
