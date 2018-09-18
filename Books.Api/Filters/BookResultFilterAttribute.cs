using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Books.Api.Filters
{
    public class BookResultFilterAttribute : ResultFilterAttribute
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            //context parameter gives us access to HttpContext etc. Request/Response
            //context.HttpContext

            //access to ModelState etc
            //context.ActionDescriptor
            //context.ModelState

            var resultFromAction = context.Result as ObjectResult;

            if (resultFromAction?.Value == null
                || resultFromAction.StatusCode < 200
                || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            resultFromAction.Value = Mapper.Map<Models.Book>(resultFromAction.Value);

            await next();

            //return base.OnResultExecutionAsync(context, next);
        }
    }
}
