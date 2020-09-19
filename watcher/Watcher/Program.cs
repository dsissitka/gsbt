using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Watcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private static IEnumerable<MemoryStream> ExtractPngs(Stream stream)
        {
            using var binaryReader = new BinaryReader(stream);

            while (true)
            {
                var memoryStream = new MemoryStream();

                // Signature
                var signatureBytes = binaryReader.ReadBytes(8);

                if (signatureBytes.Length == 0)
                {
                    yield break;
                }

                memoryStream.Write(signatureBytes);

                while (true)
                {
                    // Length
                    var lengthBytes = binaryReader.ReadBytes(4);
                    var lengthInt = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);
                    memoryStream.Write(lengthBytes);

                    // Type
                    var typeBytes = binaryReader.ReadBytes(4);
                    var typeString = Encoding.UTF8.GetString(typeBytes);
                    memoryStream.Write(typeBytes);

                    // Data
                    var dataBytes = binaryReader.ReadBytes(lengthInt);
                    memoryStream.Write(dataBytes);

                    // CRC
                    var crcBytes = binaryReader.ReadBytes(4);
                    memoryStream.Write(crcBytes);

                    if (typeString == "IEND")
                    {
                        break;
                    }
                }

                memoryStream.Position = 0;

                yield return memoryStream;
            }
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
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,

                    ArgumentList =
                    {
                        "-i", path,
                        "-codec:v", "png",
                        "-f", "image2pipe",
                        "-vsync", "passthrough",
                        "-"
                    }
                }
            };

            process.Start();

            //
            return ExtractPngs(process.StandardOutput.BaseStream)
                .AsParallel()
                .AsOrdered()
                .Select(png =>
                {
                    var image = Image.Load<Rgb24>(png);

                    var localX = image.Width / 4;
                    var localY = image.Height / 2;
                    var localPixel = image[localX, localY];
                    var localColor = GetColor(localPixel.R, localPixel.G, localPixel.B);

                    var remoteX = image.Width * 3 / 4;
                    var remoteY = image.Height / 2;
                    var remotePixel = image[remoteX, remoteY];
                    var remoteColor = GetColor(remotePixel.R, remotePixel.G, remotePixel.B);

                    return new Frame(localColor, remoteColor);
                })
                .ToList();
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