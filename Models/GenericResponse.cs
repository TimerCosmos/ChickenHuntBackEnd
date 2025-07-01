using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class GenericResponse
    {
        public Boolean Status { get; set; }
        public Response? Response { get; set; }
        public Error? Error { get; set; }

    }
    public class Error
    {
        public string? Description { get; set; }
    }
    public class Response
    {
        public Boolean Success { get; set; }
        public Object? Data { get; set; }

    }
}
