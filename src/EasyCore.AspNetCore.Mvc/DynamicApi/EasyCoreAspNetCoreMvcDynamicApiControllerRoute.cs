using EasyCore.AspNetCore.Mvc.AppService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyCore.AspNetCore.Mvc.DynamicApi
{
    /// <summary>
    /// Rewrites action routes and HTTP verbs for dynamic APIs based on method-name prefixes
    /// (<c>Get*</c>, <c>Post*</c>, <c>Put*</c>, <c>Delete*</c>).
    /// Only applies to controllers that inherit <see cref="BaseAppService"/>.
    /// </summary>
    public class EasyCoreAspNetCoreMvcDynamicApiControllerRoute : IApplicationModelConvention
    {
        /// <summary>
        /// Applies the route and HTTP-verb convention to the application model.
        /// </summary>
        /// <param name="application">The MVC application model.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an action method name does not start with a supported HTTP verb prefix.
        /// </exception>
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (!typeof(BaseAppService).IsAssignableFrom(controller.ControllerType))
                    continue;

                foreach (var action in controller.Actions)
                {
                    var methodName = action.ActionMethod.Name;

                    string httpVerb = methodName switch
                    {
                        var n when n.StartsWith("Get", StringComparison.OrdinalIgnoreCase) => "GET",
                        var n when n.StartsWith("Post", StringComparison.OrdinalIgnoreCase) => "POST",
                        var n when n.StartsWith("Put", StringComparison.OrdinalIgnoreCase) => "PUT",
                        var n when n.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) => "DELETE",
                        _ => throw new InvalidOperationException(
                            $"Cannot infer HTTP verb from action '{controller.ControllerName}.{methodName}'. Use Get*/Post*/Put*/Delete* naming.")
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
}
