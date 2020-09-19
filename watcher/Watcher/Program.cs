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

            return ExtractPngs(process.StandardOutput.BaseStream)
                .AsParallel()
                .AsOrdered()
                .Select(png =>
                {
                    var image = Image.Load<Rgb24>(png);

                    var localMasheeX = image.Width / 4;
                    var localMasheeY = image.Height / 2;
                    var localMasheePixel = image[localMasheeX, localMasheeY];
                    var localMasheeColor = $"rgb({localMasheePixel.R}, {localMasheePixel.G}, {localMasheePixel.B})";

                    var masherX = image.Width / 2;
                    var masherY = image.Height / 2;
                    var masherPixel = image[masherX, masherY];
                    var masherColor = $"rgb({masherPixel.R}, {masherPixel.G}, {masherPixel.B})";

                    var remoteMasheeX = image.Width * 3 / 4;
                    var remoteMasheeY = image.Height / 2;
                    var remoteMasheePixel = image[remoteMasheeX, remoteMasheeY];
                    var remoteMasheeColor = $"rgb({remoteMasheePixel.R}, {remoteMasheePixel.G}, {remoteMasheePixel.B})";

                    return new Frame(localMasheeColor, masherColor, remoteMasheeColor);
                })
                .ToList();
        }
    }

    public struct Frame
    {
        public string LocalMasheeColor { get; }
        public string MasherColor { get; }
        public string RemoteMasheeColor { get; }

        public Frame(string localMasheeColor, string masherColor, string remoteMasheeColor)
        {
            LocalMasheeColor = localMasheeColor;
            MasherColor = masherColor;
            RemoteMasheeColor = remoteMasheeColor;
        }
    }
}