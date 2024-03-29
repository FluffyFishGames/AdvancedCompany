﻿using AssetsTools.NET.Extra;
using System;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassDatabaseFileHeader
    {
        public string Magic { get; set; }
        public byte FileVersion { get; set; }
        public UnityVersion Version { get; set; }
        public ClassFileCompressionType CompressionType { get; set; }
        public int CompressedSize { get; set; }
        public int DecompressedSize { get; set; }

        /// <summary>
        /// Read the <see cref="ClassDatabaseFileHeader"/> with the provided reader.
        /// Note only new CLDB files are supported. Original UABE cldb files are no longer supported.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            Magic = reader.ReadStringLength(4);

            if (Magic != "CLDB")
            {
                if (Magic == "cldb")
                    throw new NotSupportedException("Old cldb style class databases are no longer supported.");
                else
                    throw new NotSupportedException("CLDB magic not found. Is this really a class database file?");
            }

            FileVersion = reader.ReadByte();
            if (FileVersion > 1)
                throw new Exception($"Unsupported or invalid file version {FileVersion}.");

            Version = UnityVersion.FromUInt64(reader.ReadUInt64());

            CompressionType = (ClassFileCompressionType)reader.ReadByte();
            CompressedSize = reader.ReadInt32();
            DecompressedSize = reader.ReadInt32();
        }

        /// <summary>
        /// Write the <see cref="ClassDatabaseFileHeader"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(Magic));
            writer.Write(FileVersion);
            writer.Write(Version.ToUInt64());
            writer.Write((byte)CompressionType);
            writer.Write(CompressedSize);
            writer.Write(DecompressedSize);
        }
    }
}
