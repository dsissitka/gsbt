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
            CollectionAssert.AreEqual(
                new List<Frame>
                {
                    new Frame(localColor: Colors.Red, remoteColor: Colors.Blue),
                    new Frame(localColor: Colors.Green, remoteColor: Colors.Green),
                    new Frame(localColor: Colors.Blue, remoteColor: Colors.Red),
                },
                Program.ProcessVideo("video.mp4")
            );
        }
    }
}