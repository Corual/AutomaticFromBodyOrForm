using FromBodyOrForm.Attributes;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FromBodyOrForm
{
    public class FromBodyOrFormProvider : IModelBinderProvider
    {
        private readonly IList<IInputFormatter> _formatters;

        public FromBodyOrFormProvider(IList<IInputFormatter> formatters)
        {
            _formatters = formatters;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //避免与原系统的FromBody、FromForm特性冲突,不是自定义的绑定源都直接返回
            if (context.BindingInfo.BindingSource != BindingSource.Custom)
            {
                return null;
            }

            //没有打上AutoFromBodyOrForm标记的，都不处理，避免系统注册了多种自定义的绑定源
            bool hasAutoFrom = ((DefaultModelMetadata)context.Metadata)
                .Attributes
                .ParameterAttributes.Any(attr => typeof(AutoFromBodyOrFormAttribute)
                .IsAssignableFrom(attr.GetType())) || null != context.Metadata.ModelType.GetCustomAttribute<AutoFromBodyOrFormAttribute>(true);

            if (!hasAutoFrom)
            {
                return null;
            }

            var binders = new Dictionary<Type, (ModelMetadata, IModelBinder)>();
            var type = context.Metadata.ModelType;
            var modelMetadata = context.MetadataProvider.GetMetadataForType(type);
            binders[type] = (modelMetadata, context.CreateBinder(modelMetadata));

            return new FromBodyOrFormBinder(_formatters, context.Services.GetRequiredService<IHttpRequestStreamReaderFactory>(), binders);
        }
    }
}
