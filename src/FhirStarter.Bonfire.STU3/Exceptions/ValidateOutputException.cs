using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FhirStarter.Bonfire.STU3.Exceptions
{
    public class ValidateOutputException: ArgumentException
    {

        public ValidateOutputException(string message) : base(message)
        {
        }

        public ValidateOutputException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
