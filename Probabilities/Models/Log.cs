using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Probabilities.Models
{
    //Model class for the logs that will be saved into a file. Keeps date, type, inputs and result.
    public class Log
    {
        //Used a datetime format for the date. Date information is still stored but can be more accurate in future by looking at timestamp also.
        public DateTime Date { get; set; }

        //The calculation type could also be an enum instead of a string. Used string for readability in current format.
        public string CalculationType { get; set; }

        //The inputs can be validated here for backend using a range.
        //Both inputs and result have used decimal format for avoid binary multiplication complications of floats or doubles.
        [Range(0, 1, ErrorMessage = "Value must be between 0 and 1.")]
        public decimal InputOne { get; set; }

        [Range(0, 1, ErrorMessage = "Value must be between 0 and 1.")]
        public decimal InputTwo { get; set; }

        public decimal Result { get; set; }

        //To string for easier understanding of the object if needed in future.
        public override string ToString() => ("Requested on " + this.Date + " with a result of " + this.Result);
    }
}
