using Ccs.Registration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    public sealed class RPBinderAttribute : Attribute, IModelBinder, IBinderTypeProviderMetadata, IBindingSourceMetadata
    {
        public Type BinderType => typeof(RPBinderAttribute);

        public BindingSource BindingSource => BindingSource.Path;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrWhiteSpace(value))
                return Task.CompletedTask;

            value = value.Trim();
            var providers = Get(bindingContext.HttpContext);

            for (int i = 0; i < providers.Count; i++)
            {
                if (providers[i].Item1 == value)
                {
                    bindingContext.Result = ModelBindingResult.Success(providers[i].Item2);
                }
            }

            return Task.CompletedTask;
        }

        public static IReadOnlyList<(string, IRegisterProvider)> Get(HttpContext context)
        {
            return context.RequestServices.GetRequiredService<IOptions<ContestRegistrationOptions>>().Value.Providers;
        }
    }
}
