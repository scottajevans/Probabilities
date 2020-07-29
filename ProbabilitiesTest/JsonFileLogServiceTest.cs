using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probabilities.Services;
using Probabilities.Models;
using Moq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ProbabilitiesTest
{
    [TestClass]
    public class JsonFileLogServiceTest
    {
        //Set up a service variable, refreshable for each use. Sets Web Root Path to C drive.
        //Using Mock to create a fake IWebHostEnvironment for testing purposes.
        private JsonFileLogService LogServiceSetup()
        {
            var Mock = new Mock<IWebHostEnvironment>();
            Mock.SetupAllProperties();
            Mock.Object.WebRootPath = "C:/";
            var Service = new JsonFileLogService(Mock.Object);
            return Service;
        }

        //Return the full file path (returns "C:\Data\log.json").
        private string FileName(IWebHostEnvironment Environment)
        {
            return Path.Combine(Environment.WebRootPath, "data", "log.json");
        }

        //Test data, 2 log objects.
        private IEnumerable<Log> Logs()
        {
            var Log1 = new Log();
            Log1.Date = DateTime.Now;
            Log1.CalculationType = "Either";
            Log1.InputOne = 0.5m;
            Log1.InputTwo = 0.5m;
            Log1.Result = 0.75m;

            var Log2 = new Log();
            Log2.Date = DateTime.Now;
            Log2.CalculationType = "CombinedWith";
            Log2.InputOne = 0.5m;
            Log2.InputTwo = 0.5m;
            Log2.Result = 0.25m;

            var list = new List<Log>();
            list.Add(Log1);
            list.Add(Log2);
            IEnumerable<Log> Logs = list;
            return Logs;
        }

        //Test data, 1 Log object.
        private Log NewLog()
        {
            Log Log = new Log();
            Log.Date = DateTime.Now;
            Log.CalculationType = "CombinedWith";
            Log.InputOne = 0.7m;
            Log.InputTwo = 0.3m;
            Log.Result = 0.21m;

            return Log;
        }

        //Remove the log file entirely. Results in F:\data\ being empty.
        private void RemoveLogFile()
        {
            var Service = LogServiceSetup();
            File.Delete(FileName(Service.WebHostEnvironment));
        }

        //Creates a fresh log file if it doesn't exist. Uses above remove function first to be sure.
        private void CreateNewLogFile()
        {
            RemoveLogFile();
            var Service = LogServiceSetup();
            File.Create(FileName(Service.WebHostEnvironment)).Close();
        }

        //Clears the file so contents are null.
        private void ClearLogFile()
        {
            var Service = LogServiceSetup();
            File.WriteAllText(FileName(Service.WebHostEnvironment), null);
        }

        //Gives the file some mock data to test with.
        private void FillLogFile(IEnumerable<Log> logs)
        {
            var Service = LogServiceSetup();
            File.WriteAllText(FileName(Service.WebHostEnvironment), JsonSerializer.Serialize(logs));
        }

        //Create a new file using the get method, this should then be null and the function should be null. The new file should exist.
        [TestMethod]
        public void GetLogsNoFile_CreatesNewFileReturnsNull()
        {
            //Before testing the get method, remove the file.
            RemoveLogFile();
            var Service = LogServiceSetup();

            //Verify the file does not exist.
            var existsInitially = File.Exists(FileName(Service.WebHostEnvironment));
            Assert.IsFalse(existsInitially);

            //Launch get method, check result is null.
            var Results = Service.GetLogs();
            Assert.IsNull(Results);

            //Now check that the file does exist.
            var exists = File.Exists(FileName(Service.WebHostEnvironment));
            Assert.IsTrue(exists);
        }

        //Clear the log file and check that the get results just return absolute null.
        [TestMethod]
        public void GetLogsEmptyFile_ReturnsNull()
        {
            //Clearing the file
            ClearLogFile();
            var Service = LogServiceSetup();

            //Verify result is null
            var Results = Service.GetLogs();
            Assert.IsNull(Results);

            //Now check that the file does exist, it is not just failing to find file.
            var exists = File.Exists(FileName(Service.WebHostEnvironment));
            Assert.IsTrue(exists);
        }

        //Populate the file with test data, check that the results are a list of 2 log objects with expected values.
        [TestMethod]
        public void GetLogsFilledFile_ReturnsSameList()
        {
            //Create a brand new log file, fill with test data.
            IEnumerable<Log> LogsList = Logs();
            CreateNewLogFile();
            FillLogFile(LogsList);
            var Service = LogServiceSetup();

            //Verify that results do exist, then that they are a IEnumerable list.
            var Results = Service.GetLogs();
            Assert.IsNotNull(Results);
            Assert.IsInstanceOfType(Results, typeof(IEnumerable<Log>));

            //Further testing that each object in the file is correct compared to the original list used above.
            foreach (var CurrentLog in Results.Select((x, i) => new { Value = x, Index = i }))
            {
                //Asserting against each property as the objects are different instances.
                Assert.AreEqual(CurrentLog.Value.Date, LogsList.ElementAt(CurrentLog.Index).Date);
                Assert.AreEqual(CurrentLog.Value.CalculationType, LogsList.ElementAt(CurrentLog.Index).CalculationType);
                Assert.AreEqual(CurrentLog.Value.InputOne, LogsList.ElementAt(CurrentLog.Index).InputOne);
                Assert.AreEqual(CurrentLog.Value.InputTwo, LogsList.ElementAt(CurrentLog.Index).InputTwo);
                Assert.AreEqual(CurrentLog.Value.Result, LogsList.ElementAt(CurrentLog.Index).Result);
            }
        }

        //Testing the add method with no file.
        [TestMethod]
        public void AddLogsNoFile_CreatesNewAdds()
        {
            //First remove the log file.
            RemoveLogFile();
            var Service = LogServiceSetup();

            //Verify the file does not exist.
            var existsInitially = File.Exists(FileName(Service.WebHostEnvironment));
            Assert.IsFalse(existsInitially);

            //Now add a new log using passing a created variable. Then check this is created in a new file as the sole object.
            Log log = NewLog();
            Service.AddToLog(log);

            //First does file exist?
            var existsNow = File.Exists(FileName(Service.WebHostEnvironment));
            Assert.IsTrue(existsNow);

            //Now use the get to check that the file is not null, and has 1 object of correct type.
            var FileContents = Service.GetLogs();
            Assert.IsNotNull(FileContents);
            Assert.IsInstanceOfType(FileContents, typeof(IEnumerable<Log>));

            //Check values, using a foreach in case more than 1 value in FileContents.
            foreach (var CurrentLog in FileContents)
            {
                Assert.AreEqual(log.Date, CurrentLog.Date);
                Assert.AreEqual(log.CalculationType, CurrentLog.CalculationType);
                Assert.AreEqual(log.InputOne, CurrentLog.InputOne);
                Assert.AreEqual(log.InputTwo, CurrentLog.InputTwo);
                Assert.AreEqual(log.Result, CurrentLog.Result);
            }
            //Assuring there was only 1 record in the list.
            Assert.AreEqual(1, FileContents.Count());
        }

        //Testing the add method with an empty file.
        [TestMethod]
        public void AddLogsEmptyFile_AddsToFile()
        {
            //First remove the log file.
            ClearLogFile();
            var Service = LogServiceSetup();

            //Now add a new log using passing a created variable. Then check this is created in the file as the sole object.
            Log log = NewLog();
            Service.AddToLog(log);

            //Now use the get to check that the file is not null, and has 1 object of correct type.
            var FileContents = Service.GetLogs();
            Assert.IsNotNull(FileContents);
            Assert.IsInstanceOfType(FileContents, typeof(IEnumerable<Log>));

            //Check values, using a foreach in case more than 1 value in FileContents.
            foreach (var CurrentLog in FileContents)
            {
                Assert.AreEqual(log.Date, CurrentLog.Date);
                Assert.AreEqual(log.CalculationType, CurrentLog.CalculationType);
                Assert.AreEqual(log.InputOne, CurrentLog.InputOne);
                Assert.AreEqual(log.InputTwo, CurrentLog.InputTwo);
                Assert.AreEqual(log.Result, CurrentLog.Result);
            }
            //Assuring there was only 1 record in the list.
            Assert.AreEqual(1, FileContents.Count());
        }

        //Testing that add method with a file with contents works, creating a list of size 3.
        [TestMethod]
        public void AddLogsFilledFile_AppendsToExisting()
        {
            //Create a brand new log file, fill with test data.
            IEnumerable<Log> LogsList = Logs();
            CreateNewLogFile();
            FillLogFile(LogsList);
            var Service = LogServiceSetup();

            //Now add a new log using passing a created variable. Then check this is created in a new file as the sole object.
            Log log = NewLog();
            Service.AddToLog(log);

            //Now that the new log has been added to file, add to the list that was created at start of method.
            LogsList = LogsList.Append(log);

            //Now use the get to check that the file is not null, and has 3 objects of correct type.
            var FileContents = Service.GetLogs();
            Assert.IsNotNull(FileContents);
            Assert.IsInstanceOfType(FileContents, typeof(IEnumerable<Log>));

            //Check values, using a foreach so able to verify all data the same
            foreach (var CurrentLog in FileContents.Select((x, i) => new { Value = x, Index = i }))
            {
                //Asserting against each property as the objects are different instances.
                Assert.AreEqual(CurrentLog.Value.Date, LogsList.ElementAt(CurrentLog.Index).Date);
                Assert.AreEqual(CurrentLog.Value.CalculationType, LogsList.ElementAt(CurrentLog.Index).CalculationType);
                Assert.AreEqual(CurrentLog.Value.InputOne, LogsList.ElementAt(CurrentLog.Index).InputOne);
                Assert.AreEqual(CurrentLog.Value.InputTwo, LogsList.ElementAt(CurrentLog.Index).InputTwo);
                Assert.AreEqual(CurrentLog.Value.Result, LogsList.ElementAt(CurrentLog.Index).Result);
            }
            //Assuring there was only 1 record in the list.
            Assert.AreEqual(3, FileContents.Count());
        }
    }
}
