﻿using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassPackageType
    {
        public int ClassId { get; set; }
        public ushort Name { get; set; }
        public ushort BaseName { get; set; }
        public ClassFileTypeFlags Flags { get; set; }
        public ushort EditorRootNode { get; set; }
        public ushort ReleaseRootNode { get; set; }

        /// <summary>
        /// Read the <see cref="ClassPackageType"/> with the provided reader and class ID.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="classId">The class ID to assign.</param>
        public void Read(AssetsFileReader reader, int classId)
        {
            ClassId = classId;

            Name = reader.ReadUInt16();
            BaseName = reader.ReadUInt16();

            Flags = (ClassFileTypeFlags)reader.ReadByte();

            EditorRootNode = ushort.MaxValue;
            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasEditorRootNode))
                EditorRootNode = reader.ReadUInt16();

            ReleaseRootNode = ushort.MaxValue;
            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasReleaseRootNode))
                ReleaseRootNode = reader.ReadUInt16();
        }

        /// <summary>
        /// Write the <see cref="ClassPackageType"/> with the provided writer.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(AssetsFileWriter writer)
        {
            writer.Write(Name);
            writer.Write(BaseName);
            writer.Write((byte)Flags);

            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasEditorRootNode))
                writer.Write(EditorRootNode);

            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasReleaseRootNode))
                writer.Write(ReleaseRootNode);
        }
    }
}
