using EasyCore.AspNetCore.Mvc.AppService;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyCore.AspNetCore.Mvc.DynamicApi
{
    /// <summary>
    /// Trims a configured suffix (default: <c>AppService</c>) from dynamic API controller names in route templates.
    /// Only applies to controllers that inherit <see cref="BaseAppService"/>.
    /// </summary>
    public class EasyCoreAspNetCoreMvcDynamicApiControllerName : IApplicationModelConvention
    {
        /// <summary>
        /// The controller-name suffix to remove when rewriting route templates.
        /// </summary>
        private readonly string _suffixToTrim;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyCoreAspNetCoreMvcDynamicApiControllerName"/> class.
        /// </summary>
        /// <param name="suffixToTrim">The suffix to remove from controller names. Defaults to <c>AppService</c>.</param>
        public EasyCoreAspNetCoreMvcDynamicApiControllerName(string suffixToTrim = "AppService")
            => _suffixToTrim = suffixToTrim;

        /// <summary>
        /// Applies the naming convention to the application model.
        /// </summary>
        /// <param name="application">The MVC application model.</param>
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (!typeof(BaseAppService).IsAssignableFrom(controller.ControllerType))
                    continue;

                if (!controller.ControllerName.EndsWith(_suffixToTrim, StringComparison.Ordinal))
                    continue;

                var newName = controller.ControllerName[..^_suffixToTrim.Length];

                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel?.Template != null)
                    {
                        selector.AttributeRouteModel.Template =
                            selector.AttributeRouteModel.Template.Replace("[controller]", newName);
                    }
                }
            }
        }
    }
}
