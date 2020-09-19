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

        private static Colors GetColor(int red, int green, int blue)
        {
            // Even though Mashee and Masher use pure colors (rgb(255, 0, 0) for red, rgb(0, 255, 0) for green,
            // etcetera) it's unlikely that users will be feeding Watcher lossless videos and so we return red when the
            // color is almost pure red, green when the color is almost pure green, etcetera.
            var lowerLimit = Math.Ceiling(255 * 0.1);
            var upperLimit = Math.Floor(255 * 0.9);

            if (red >= upperLimit && green <= lowerLimit && blue <= lowerLimit)
            {
                return Colors.Red;
            }
            else if (red <= lowerLimit && green >= upperLimit && blue <= lowerLimit)
            {
                return Colors.Green;
            }
            else if (red <= lowerLimit && green <= lowerLimit && blue >= upperLimit)
            {
                return Colors.Blue;
            }
            else
            {
                throw new ArgumentException($"Invalid color: rgb({red}, {green}, {blue})");
            }
        }

        public static List<Frame> ProcessVideo(string path)
        {
            var red = GetColor(229, 26, 26);
            var green = GetColor(26, 229, 26);
            var blue = GetColor(26, 26, 229);

            var frames = new List<Frame>();
            frames.Add(new Frame(localColor: red, remoteColor: blue));
            frames.Add(new Frame(localColor: green, remoteColor: green));
            frames.Add(new Frame(localColor: blue, remoteColor: red));

            return frames;
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