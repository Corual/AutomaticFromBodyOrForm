
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using FromBodyOrForm.Attributes;
using System.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FromBodyOrForm
{
    public class FromBodyOrFormBinder : IModelBinder
    {
        /// <summary>
        /// json内容
        /// </summary>
        private const string JSON_CONTENT_TYPE = "application/json";

        /// <summary>
        /// 表单内容,POST
        /// </summary>
        private const string FORM_DATA_POST_CONTENT_TYPE = "multipart/form-data";

        /// <summary>
        /// 表单内容,GET
        /// </summary>
        private const string FORM_DATA_GET_CONTENT_TYPE = "application/x-www-form-urlencoded";


        private readonly IList<IInputFormatter> _formatters;
        private readonly IHttpRequestStreamReaderFactory _readerFactory;
        private readonly IDictionary<Type, (ModelMetadata, IModelBinder)> _binders;

        public FromBodyOrFormBinder(IList<IInputFormatter> formatters, IHttpRequestStreamReaderFactory readerFactory, IDictionary<Type, (ModelMetadata, IModelBinder)> binders)
        {
            _formatters = formatters;
            _readerFactory = readerFactory;
            _binders = binders;
        }


        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            #region 该代码已移动到FromBodyOrFormProvider.cs
            ////避免与原系统的FromBody、FromForm特性冲突,不是自定义的绑定源都直接返回
            //if (bindingContext.BindingSource != BindingSource.Custom)
            //{
            //    return;
            //}

            ////没有打上AutoFromBodyOrForm标记的，都不处理，避免系统注册了多种自定义的绑定源
            //bool hasAutoFrom = ((DefaultModelMetadata)bindingContext.ModelMetadata)
            //    .Attributes
            //    .ParameterAttributes.Any(attr => typeof(AutoFromBodyOrFormAttribute)
            //    .IsAssignableFrom(attr.GetType())) || null != bindingContext.ModelType.GetCustomAttribute<AutoFromBodyOrFormAttribute>(true);

            //if (!hasAutoFrom)
            //{
            //    return;
            //}

            #endregion

            //获取请求类容的Type
            string contentType = bindingContext.HttpContext.Request.ContentType;
            IModelBinder modelBinder = null;
            ModelBindingContext modelBindingContext = bindingContext;

            //FromBody
            if (contentType.Contains(JSON_CONTENT_TYPE))
            {
                modelBinder = new BodyModelBinder(_formatters, _readerFactory);
                await modelBinder.BindModelAsync(modelBindingContext);
            }
            //FromForm
            else if (contentType.Contains(FORM_DATA_POST_CONTENT_TYPE) || contentType.Contains(FORM_DATA_GET_CONTENT_TYPE))
            {

                ModelMetadata modelMetadata = null;

                (modelMetadata, modelBinder) = _binders[bindingContext.ModelType];
                modelBindingContext = DefaultModelBindingContext.CreateBindingContext(bindingContext.ActionContext,
                    bindingContext.ValueProvider,
                    modelMetadata,
                    bindingInfo: null,
                    bindingContext.ModelName);

                await modelBinder.BindModelAsync(modelBindingContext);

                bindingContext.Result = modelBindingContext.Result;
            }


            return;
        }
    }
}
