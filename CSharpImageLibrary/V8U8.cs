﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulThings;
using static CSharpImageLibrary.DDSGeneral;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Provides V8U8 format functionality.
    /// </summary>
    internal static class V8U8
    {
        /// <summary>
        /// Loads useful information from V8U8 image.
        /// </summary>
        /// <param name="imagePath">Path to V8U8 image file.</param>
        /// <param name="Width">Image Width.</param>
        /// <param name="Height">Image Height.</param>
        /// <returns>RGBA Pixel data as stream.</returns>
        internal static MemoryTributary Load(string imagePath, out int Width, out int Height)
        {
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(fs, out Width, out Height);
        }


        /// <summary>
        /// Loads useful information from V8U8 image stream.
        /// </summary>
        /// <param name="stream">Stream containing entire V8U8 image. Not just Pixels.</param>
        /// <param name="Width">Image Width.</param>
        /// <param name="Height">Image Height.</param>
        /// <returns>RGBA Pixel data as stream.</returns>
        internal static MemoryTributary Load(Stream stream, out int Width, out int Height)
        {
            // KFreon: Read pixel data. Note: No blue channel. Only 2 colour channels.
            Func<Stream, int> PixelReader = fileData =>
            {
                sbyte red = (sbyte)fileData.ReadByte();
                sbyte green = (sbyte)fileData.ReadByte();
                byte blue = 0xFF;

                return blue | (0x7F + green) << 8 | (0x7F + red) << 16 | 0xFF << 24;
            };

            return DDSGeneral.LoadUncompressed(stream, out Width, out Height, PixelReader);
        }

        internal static bool Save(Stream pixelData, Stream destination, int Width, int Height, int Mips)
        {
            try
            {
                DDS_HEADER header = Build_DDS_Header(Mips, Height, Width, ImageEngineFormat.DDS_V8U8);


                pixelData.Seek(0, SeekOrigin.Begin);
                using (BinaryWriter writer = new BinaryWriter(destination, Encoding.Default, true))
                {
                    for (int m = 1; m < Mips; m++)
                    {
                        for (int h = 0; h < Height / Mips; h++)
                        {
                            for (int w = 0; w < Width / Mips; w++)
                            {
                                writer.Write(pixelData.ReadByte());  // Red
                                writer.Write(pixelData.ReadByte());  // Green
                                pixelData.Position += 2;    // No blue or alpha
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
    }
}
