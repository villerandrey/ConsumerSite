using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BE24Services.Model
{
    public class RememberPasswordModel
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
