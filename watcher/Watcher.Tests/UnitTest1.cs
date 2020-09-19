using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Watcher.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [DeploymentItem("../../../data/video.mp4")]
        [TestMethod]
        public void TestProcessVideo()
        {
            const string green = "rgb(0, 255, 1)";
            const string red = "rgb(254, 0, 0)";
            const string white = "rgb(255, 255, 255)";

            CollectionAssert.AreEqual(
                new List<Frame>
                {
                    new Frame(white, red, white),
                    new Frame(white, green, white),
                    new Frame(green, green, green),
                    new Frame(white, green, white),
                    new Frame(white, red, white),
                },
                Program.ProcessVideo("video.mp4")
            );
        }
    }
}