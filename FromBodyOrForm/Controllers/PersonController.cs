using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FromBodyOrForm.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FromBodyOrForm.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class PersonController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "hello world!";
        }

        [HttpPost]
        public IActionResult Post([AutoFromBodyOrForm][ModelBinder(BinderType = typeof(FromBodyOrFormBinder))] Person person)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            return new JsonResult(person);
        }
    }
}