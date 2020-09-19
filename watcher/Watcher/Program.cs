using System;
using System.Collections.Generic;

namespace Watcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        public static List<Frame> ProcessVideo(string path)
        {
            return new List<Frame>
            {
                new Frame(localColor: Colors.Red, remoteColor: Colors.Blue),
                new Frame(localColor: Colors.Green, remoteColor: Colors.Green),
                new Frame(localColor: Colors.Blue, remoteColor: Colors.Red),
            };
        }
    }

    public enum Colors
    {
        Blue,
        Green,
        Red,
    }

    public struct Frame
    {
        public Colors LocalColor { get; }
        public Colors RemoteColor { get; }

        public Frame(Colors localColor, Colors remoteColor)
        {
            LocalColor = localColor;
            RemoteColor = remoteColor;
        }
    }
}