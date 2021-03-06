using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

            // Ensure FFmpeg and FFprobe are installed. Check FFmpeg first because FFprobe should be included with
            // FFmpeg.
            if (!CheckIfProgramIsInstalled("ffmpeg", "-version"))
            {
                Console.Error.WriteLine("FFmpeg must be installed.");
                Environment.Exit(1);
            }

            if (!CheckIfProgramIsInstalled("ffprobe", "-version"))
            {
                Console.Error.WriteLine("FFprobe must be installed.");
                Environment.Exit(1);
            }

            // Ensure the video is an MP4.
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = "ffprobe",
                    RedirectStandardOutput = true,

                    ArgumentList =
                    {
                        "-loglevel", "warning",
                        "-print_format", "csv=print_section=0",
                        "-show_entries", "format=format_long_name",
                        args[0]
                    }
                }
            };

            process.Start();
            var standardOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (standardOutput != "QuickTime / MOV\n")
            {
                Console.Error.WriteLine("The video must be an MP4.");
                Environment.Exit(1);
            }

            //
            var frames = GetFramesFromVideo(args[0]);

            foreach (var frame in frames)
            {
                var colors = GetColorsFromFrame(frame);

                frame.Dispose();

                Console.WriteLine(colors);
            }
        }

        private static bool CheckIfProgramIsInstalled(string fileName, string arguments)
        {
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();

                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        public static Colors GetColorsFromFrame(MemoryStream frame)
        {
            using var image = Image.Load<Rgb24>(frame);

            var localMasheeX = image.Width * 1 / 4;
            var localMasheeY = image.Height * 1 / 2;
            var localMasheePixel = image[localMasheeX, localMasheeY];

            var masherX = image.Width * 1 / 2;
            var masherY = image.Height * 1 / 2;
            var masherPixel = image[masherX, masherY];

            var remoteMasheeX = image.Width * 3 / 4;
            var remoteMasheeY = image.Height * 1 / 2;
            var remoteMasheePixel = image[remoteMasheeX, remoteMasheeY];

            return new Colors(localMasheePixel, masherPixel, remoteMasheePixel);
        }

        private static MemoryStream GetFrameFromFrames(BinaryReader frames)
        {
            var frame = new MemoryStream();

            // Signature
            var signatureBytes = frames.ReadBytes(8);

            if (signatureBytes.Length == 0)
            {
                return null;
            }

            frame.Write(signatureBytes);

            while (true)
            {
                // Length
                var lengthBytes = frames.ReadBytes(4);
                var lengthInt = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);
                frame.Write(lengthBytes);

                // Type
                var typeBytes = frames.ReadBytes(4);
                var typeString = Encoding.UTF8.GetString(typeBytes);
                frame.Write(typeBytes);

                // Data
                var dataBytes = frames.ReadBytes(lengthInt);
                frame.Write(dataBytes);

                // CRC
                var crcBytes = frames.ReadBytes(4);
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
            using var process = new Process
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

            using var frames = new BinaryReader(process.StandardOutput.BaseStream);

            //
            while (true)
            {
                var frame = GetFrameFromFrames(frames);

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
        private string LocalMasheeColor { get; }
        private string MasherColor { get; }
        private string RemoteMasheeColor { get; }

        public Colors(Rgb24 localMasheePixel, Rgb24 masherPixel, Rgb24 remoteMasheePixel)
        {
            LocalMasheeColor = $"rgb({localMasheePixel.R}, {localMasheePixel.G}, {localMasheePixel.B})";
            MasherColor = $"rgb({masherPixel.R}, {masherPixel.G}, {masherPixel.B})";
            RemoteMasheeColor = $"rgb({remoteMasheePixel.R}, {remoteMasheePixel.G}, {remoteMasheePixel.B})";
        }

        public override string ToString()
        {
            return $"{LocalMasheeColor} {MasherColor} {RemoteMasheeColor}";
        }
    }
}