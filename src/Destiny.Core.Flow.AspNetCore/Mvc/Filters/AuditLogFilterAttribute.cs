﻿using Destiny.Core.Flow.AspNetCore.Ui;
using Destiny.Core.Flow.Audit;
using Destiny.Core.Flow.Dependency;
using Destiny.Core.Flow.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Destiny.Core.Flow.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// c# 计算程序运行时间三种方法
    /// https://www.cnblogs.com/dearzhoubi/p/9842452.html 
    /// </summary>
    public class AuditLogFilterAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuted(ActionExecutedContext context)
        {

            if (context.Result is ObjectResult result)
            {
                if (result.Value is AjaxResult ajax)
                {
                    var type = ajax.Type;
                    IServiceProvider provider = context.HttpContext.RequestServices;

                    DictionaryScoped dict = provider.GetService<DictionaryScoped>();
                    if (!ajax.Success)
                    {
                        dict.AuditChange.Message = ajax.Message;
                    }

                    dict.AuditChange.ResultType = type;
                }


            }

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            IServiceProvider provider = context.HttpContext.RequestServices;
            var controllerAction = context.ActionDescriptor as ControllerActionDescriptor;
            var isAuditEnabled = provider.GetAppSettings().AuditEnabled;
            if (isAuditEnabled)
            {
                DictionaryScoped dict = provider.GetService<DictionaryScoped>();
                AuditChange auditChange = new AuditChange();
                auditChange.FunctionName = $"{context.Controller.GetType().ToDescription()}-{controllerAction.MethodInfo.ToDescription()}";
                auditChange.Action = context.HttpContext.Request.Path;
                auditChange.Ip = context.HttpContext.GetClientIP();
                auditChange.BrowserInformation = context.HttpContext.Request.Headers["User-Agent"].ToString();
                auditChange.StartTime = DateTime.Now;
                dict.AuditChange = auditChange;
            }
          
        }
        public override void OnResultExecuted(ResultExecutedContext context)
        {

            IServiceProvider provider = context.HttpContext.RequestServices;
            var action = context.ActionDescriptor as ControllerActionDescriptor;
            var isAuditEnabled = provider.GetAppSettings().AuditEnabled;
            if (isAuditEnabled)
            {

                var dic = provider.GetService<DictionaryScoped>();

                dic.AuditChange.ExecutionDuration = DateTime.Now.Subtract(dic.AuditChange.StartTime).TotalMilliseconds;
                provider.GetService<IAuditStore>()?.SaveAsync(dic.AuditChange).GetAwaiter().GetResult();

            }
        }

    }
}
