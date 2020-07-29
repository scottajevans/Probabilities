using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probabilities.Controllers;
using Probabilities.Services;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace ProbabilitiesTest
{
    [TestClass]
    public class EitherCalculatorControllerTests
    {
        //Setup reusable controller instance.
        private EitherCalculatorController EitherControllerSetup()
        {
            var Mock = new Mock<IWebHostEnvironment>();
            var Service = new JsonFileLogService(Mock.Object);
            var Controller = new EitherCalculatorController(Service);
            return Controller;
        }

        //Check for fail with invalid inputs. Using variables over of range.
        [TestMethod]
        public void EitherCalculation_FailsWithInvalidInputsOverOne()
        {
            var Controller = EitherControllerSetup();
            var Result = Controller.Get(1.1m, 1.5m) as BadRequestObjectResult;
            Assert.AreEqual(400, Result.StatusCode);
        }

        //Check for fail with invalid inputs. Using variabled below range.
        [TestMethod]
        public void EitherCalculation_FailsWithInvalidInputsUnderZero()
        {
            var Controller = EitherControllerSetup();
            var Result = Controller.Get(-0.5m, -1.5m) as BadRequestObjectResult;
            Assert.AreEqual(400, Result.StatusCode);
        }

        //Check for fail with invalid inputs. Either side of range.
        [TestMethod]
        public void EitherCalculation_FailsWithInvalidInputsOutsideRange()
        {
            var Controller = EitherControllerSetup();
            var Result = Controller.Get(1.1m, -1.5m) as BadRequestObjectResult;
            Assert.AreEqual(400, Result.StatusCode);
        }

        //Check for pass using valid inputs.
        [TestMethod]
        public void EitherCalculation_PassesWithValidInputs()
        {
            var Controller = EitherControllerSetup();
            var Result = Controller.Get(0.5m, 0.5m) as OkObjectResult;
            Assert.IsNotNull(Result);
            Assert.AreEqual(200, Result.StatusCode);
            Assert.AreEqual(0.75m, Result.Value);
        }

        //Check for pass using valid inputs at edge of range.
        [TestMethod]
        public void EitherCalculation_PassesWithValidInputsEdgeOfRange()
        {
            var Controller = EitherControllerSetup();
            var Result = Controller.Get(0m, 1m) as OkObjectResult;
            Assert.IsNotNull(Result);
            Assert.AreEqual(200, Result.StatusCode);
            Assert.AreEqual(1m, Result.Value);
        }
    }
}
