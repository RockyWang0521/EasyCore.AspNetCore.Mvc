using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class EasyCoreAspNetCoreMvcDynamicApiControllerRoute : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                var methodName = action.ActionMethod.Name;

                string httpVerb = methodName switch
                {
                    var n when n.StartsWith("Get") => "GET",

                    var n when n.StartsWith("Post") => "POST",

                    var n when n.StartsWith("Put") => "PUT",

                    var n when n.StartsWith("Delete") => "DELETE",

                    _ => "GET"
                };

                action.Selectors.Clear();

                action.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(methodName)),

                    ActionConstraints = { new HttpMethodActionConstraint(new[] { httpVerb }) }
                });
            }
        }
    }
}
