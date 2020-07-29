using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Probabilities.Models;
using Probabilities.Services;

namespace Probabilities.Controllers
{
    //Separate API endpoint for using the combined with calculation.
    [Route("api/combined")]
    [ApiController]
    public class CombinedCalulatorController : ControllerBase
    { 
        //This makes use of the log service so whena  valid calculation is make it is stored in the JSON file.
        public CombinedCalulatorController(JsonFileLogService logService)
        {
            this.LogService = logService;
        }

        public JsonFileLogService LogService { get; }

        //Currently one endpoint that performs the get for the result.
        [HttpGet]
        public IActionResult Get([Required]decimal inputOne, [Required]decimal inputTwo)
        {
            //If the 2 inputs are not provided with request it will return an error.
            if (ModelState.IsValid)
            {
                //Perform the given calculation for this formula P(A)P(B).
                decimal Result = inputOne * inputTwo;

                //Create a new log object, with current datetime stamp, the combinedWith function as calc type and the inputs and result.
                Log NewLog = new Log();
                NewLog.Date = DateTime.Now;
                NewLog.CalculationType = "CombinedWith";
                NewLog.InputOne = inputOne;
                NewLog.InputTwo = inputTwo;
                NewLog.Result = Result;

                //Now validate that the inputs are actually valid (this is mainly checking against the model that both InputOne and InputTwo
                //are within the range of 0 and 1. Store the result of that to pass back as errors if they fail.
                var ValidationResults = new List<ValidationResult>();
                if(Validator.TryValidateObject(NewLog, new ValidationContext(NewLog, null, null), ValidationResults, true))
                {
                    //If the validation has passed start an async task with the log service to add to the JSON log file. In the meantime pass
                    //a success back to the request, which currently only contains the result of the calculation.
                    new Task(() => { LogService.AddToLog(NewLog); }).Start();
                    return Ok(Result);
                }
                else
                {
                    //Passing model validation errors back to request.
                    return BadRequest(ValidationResults);
                }
                
            }
            return BadRequest("Invalid input");
        }
    }
}