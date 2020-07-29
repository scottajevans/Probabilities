using Microsoft.AspNetCore.Hosting;
using Probabilities.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Probabilities.Services
{
    //Service to interact with the JSON log file.
    public class JsonFileLogService
    {
        //Uses IWebHostEnvironment to resolve path for the file.
        public JsonFileLogService(IWebHostEnvironment webHostEnvironment)
        {
            WebHostEnvironment = webHostEnvironment;
        }

        public IWebHostEnvironment WebHostEnvironment { get; }

        //Declaring the path for the log file.
        private string JsonFileName
        {
            get { return Path.Combine(WebHostEnvironment.WebRootPath, "data", "log.json"); }
        }

        //Retrieve all logs from the file and return in the format of an IEnumerable of type Log (using the model).
        public IEnumerable<Log> GetLogs()
        {
            //Check JSON file exists, if not create.
            if (!File.Exists(JsonFileName))
            {
                File.Create(JsonFileName).Close();
            }

            //Get all of the JSON text from the file.
            string jsonText = File.ReadAllText(JsonFileName);
            //If the file is not empty deserialize into an array of logs.
            if (jsonText != "")
            {
                return JsonSerializer.Deserialize<Log[]>(jsonText);
            } else
            {
                //If the file is empty return a null value.
                return null;
            }
        }

        //Adding to the log file.
        public void AddToLog(Log newLogObject)
        {
            //Get current state of the log file.
            IEnumerable<Log> Logs = GetLogs();
            //If there are no current logs in the file, create a new list, add the new log and set that back to the variable of current log file state.
            if (Logs == null)
            {
                var list = new List<Log>();
                list.Add(newLogObject);
                Logs = list;
            }
            else
            //If the logs are not empty, append the new log to the list and set to the current log file state.
            {
                Logs = Logs.Append(newLogObject);

            }
            //Write the current state (updated in either above instance) back to the log file in a JSON serialized format.
            File.WriteAllText(JsonFileName, JsonSerializer.Serialize(Logs));
        }
    }
}
