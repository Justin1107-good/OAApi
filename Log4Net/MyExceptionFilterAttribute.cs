using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OASystemSynergy.Log4Net
{
    public class MyExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<MyExceptionFilterAttribute> _logger;
        /// <summary>
        /// 通过构造函数的方式，依赖注入日志对象
        /// </summary>
        /// <param name="logger"></param>
        public MyExceptionFilterAttribute(ILogger<MyExceptionFilterAttribute> logger)
        {
            this._logger = logger;
        }
        public override void OnException(ExceptionContext context)
        {
            // 判断是否被处理过
            if (!context.ExceptionHandled)
            {
                context.ExceptionHandled = true;
                var str = $"异常：{context.HttpContext.Request.Path}{context.Exception.Message}";
                // 输出到控制台
                Console.WriteLine(str);
                // 写入文本日志（或者是记录到数据库等....）
                _logger.LogWarning(str);
                if (context.HttpContext.Request.Method == "GET")
                {
                    // 如果是 get请求，则跳转页面
                }
                else
                {
                    // 如果是post 则都是ajax请求，则返回json数据格式,输出自定义或者约定好的格式
                    context.Result = new JsonResult(new { Result = false, Message = "请求出现错误，请联系管理员" });
                }
            }
        }
    }
}
