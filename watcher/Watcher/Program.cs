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
    public static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("usage: watcher VIDEO_PATH");
                Environment.Exit(1);
            }
            
            var frames = GetFramesFromVideo(args[0])
                .AsParallel()
                .AsOrdered();

            foreach (var frame in frames)
            {
                var colors = GetColorsFromFrame(frame);

                frame.Dispose();

                Console.WriteLine($"{colors.LocalMasheeColor} {colors.MasherColor} {colors.RemoteMasheeColor}");
            }
        }

        public static Colors GetColorsFromFrame(MemoryStream frame)
        {
            using var image = Image.Load<Rgb24>(frame);

            var localMasheeX = image.Width * 1 / 4;
            var localMasheeY = image.Height * 1 / 2;
            var localMasheePixel = image[localMasheeX, localMasheeY];
            var localMasheeColor = $"rgb({localMasheePixel.R}, {localMasheePixel.G}, {localMasheePixel.B})";

            var masherX = image.Width * 1 / 2;
            var masherY = image.Height * 1 / 2;
            var masherPixel = image[masherX, masherY];
            var masherColor = $"rgb({masherPixel.R}, {masherPixel.G}, {masherPixel.B})";

            var remoteMasheeX = image.Width * 3 / 4;
            var remoteMasheeY = image.Height * 1 / 2;
            var remoteMasheePixel = image[remoteMasheeX, remoteMasheeY];
            var remoteMasheeColor = $"rgb({remoteMasheePixel.R}, {remoteMasheePixel.G}, {remoteMasheePixel.B})";

            return new Colors(localMasheeColor, masherColor, remoteMasheeColor);
        }

        private static MemoryStream GetFrameFromVideo(BinaryReader video)
        {
            var frame = new MemoryStream();

            // Signature
            var signatureBytes = video.ReadBytes(8);

            if (signatureBytes.Length == 0)
            {
                return null;
            }

            frame.Write(signatureBytes);

            while (true)
            {
                // Length
                var lengthBytes = video.ReadBytes(4);
                var lengthInt = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);
                frame.Write(lengthBytes);

                // Type
                var typeBytes = video.ReadBytes(4);
                var typeString = Encoding.UTF8.GetString(typeBytes);
                frame.Write(typeBytes);

                // Data
                var dataBytes = video.ReadBytes(lengthInt);
                frame.Write(dataBytes);

                // CRC
                var crcBytes = video.ReadBytes(4);
                frame.Write(crcBytes);

                if (typeString == "IEND")
                {
                    break;
                }
            }

            frame.Position = 0;

            return frame;
        }

        public static IEnumerable<MemoryStream> GetFramesFromVideo(string path)
        {
            //
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg",
                    RedirectStandardOutput = true,

                    ArgumentList =
                    {
                        "-i", path,
                        "-codec:v", "png",
                        "-f", "image2pipe",
                        "-loglevel", "warning",
                        "-vsync", "passthrough",
                        "-"
                    }
                }
            };

            process.Start();

            //
            using var video = new BinaryReader(process.StandardOutput.BaseStream);

            while (true)
            {
                var frame = GetFrameFromVideo(video);

                if (frame == null)
                {
                    yield break;
                }

                yield return frame;
            }
        }
    }

    public readonly struct Colors
    {
        public string LocalMasheeColor { get; }
        public string MasherColor { get; }
        public string RemoteMasheeColor { get; }

        public Colors(string localMasheeColor, string masherColor, string remoteMasheeColor)
        {
            LocalMasheeColor = localMasheeColor;
            MasherColor = masherColor;
            RemoteMasheeColor = remoteMasheeColor;
        }
    }
}