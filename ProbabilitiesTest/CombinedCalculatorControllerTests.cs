using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probabilities.Controllers;
using Probabilities.Services;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace ProbabilitiesTest
{
    [TestClass]
    public class CombinedCalculatorControllerTests
    {
        //Setup reusable controller instance.
        private CombinedCalulatorController CombinedControllerSetup()
        {
            var Mock = new Mock<IWebHostEnvironment>();
            var Service = new JsonFileLogService(Mock.Object);
            var Controller = new CombinedCalulatorController(Service);
            return Controller;
        }

        //Check for fail with invalid inputs. Using variables over the range.
        [TestMethod]
        public void CombinedCalcuation_FailsWithInvalidInputsOverOne()
        {
            var Controller = CombinedControllerSetup();
            var Result = Controller.Get(1.1m, 1.5m) as BadRequestObjectResult;
            Assert.AreEqual(400, Result.StatusCode);
        }

        //Check for fail with invalid inputs. Using variables under the range.
        [TestMethod]
        public void CombinedCalcuation_FailsWithInvalidInputsUnderZero()
        {
            var Controller = CombinedControllerSetup();
            var Result = Controller.Get(-0.5m, -1.5m) as BadRequestObjectResult;
            Assert.AreEqual(400, Result.StatusCode);
        }

        //Check for fail with invalid inputs. Using variables either side of range.
        [TestMethod]
        public void CombinedCalcuation_FailsWithInvalidInputsOutsideRange()
        {
            var Controller = CombinedControllerSetup();
            var Result = Controller.Get(-0.5m, 1.5m) as BadRequestObjectResult;
            Assert.AreEqual(400, Result.StatusCode);
        }

        //Check for pass using valid inputs.
        [TestMethod]
        public void CombinedCalcuation_PassesWithValidInputs()
        {
            var Controller = CombinedControllerSetup();
            var Result = Controller.Get(0.5m, 0.5m) as OkObjectResult;
            Assert.IsNotNull(Result);
            Assert.AreEqual(200, Result.StatusCode);
            Assert.AreEqual(0.25m, Result.Value);
        }

        //Check for pass using valid inputs at edge of range.
        [TestMethod]
        public void CombinedCalcuation_PassesWithValidInputsEdgeOfRange()
        {
            var Controller = CombinedControllerSetup();
            var Result = Controller.Get(0m, 1m) as OkObjectResult;
            Assert.IsNotNull(Result);
            Assert.AreEqual(200, Result.StatusCode);
            Assert.AreEqual(0m, Result.Value);
        }
    }
}
