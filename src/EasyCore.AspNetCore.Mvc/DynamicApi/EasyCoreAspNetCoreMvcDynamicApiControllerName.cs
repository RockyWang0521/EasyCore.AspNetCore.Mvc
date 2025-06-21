using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class EasyCoreAspNetCoreMvcDynamicApiControllerName : IApplicationModelConvention
{
    private readonly string _suffixToTrim;

    public EasyCoreAspNetCoreMvcDynamicApiControllerName(string suffixToTrim = "AppService") => _suffixToTrim = suffixToTrim;

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (controller.ControllerName.EndsWith(_suffixToTrim))
            {
                var newName = controller.ControllerName[..^_suffixToTrim.Length];

                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel != null)
                    {
                        selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template!.Replace("[controller]", newName);
                    }
                }
            }
        }
    }
}
