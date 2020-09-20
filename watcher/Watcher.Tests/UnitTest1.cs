using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Watcher.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [DeploymentItem("../../../data/video.mp4")]
        [TestMethod]
        public void Test()
        {
            const string green = "rgb(0, 255, 1)";
            const string red = "rgb(254, 0, 0)";
            const string white = "rgb(255, 255, 255)";

            var expectedColors = new List<Colors>
            {
                new Colors(white, red, white),
                new Colors(white, green, white),
                new Colors(green, green, green),
                new Colors(white, green, white),
                new Colors(white, red, white),
            };

            var actualColors = Program.GetFramesFromVideo("video.mp4")
                .Select(Program.GetColorsFromFrame)
                .ToList();

            CollectionAssert.AreEqual(expectedColors, actualColors);
        }
    }
}