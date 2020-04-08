using FromBodyOrForm.Attributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromBodyOrForm
{
    //[AutoFromBodyOrForm] //如果不在参数中使用该特性，可以定义在类上
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
