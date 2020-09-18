using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace ImageDPI
{
    internal class PngChunk
    {
        private const string TerminatingChunkTag = "IEND";

        /// <summary>
        /// The identifying tag of the chunk.
        /// </summary>
        public string Tag { get; }
        /// <summary>
        /// The raw data of the chunk, including length, tag, data and checksum.
        /// </summary>
        public byte[] Data { get; }


        private PngChunk(byte[] data, int tagOffset = 4)
        {
            Data = data;
            Tag = Encoding.ASCII.GetString(data, tagOffset, 4);
        }

        // pngs start with this signature:
        private static readonly byte[] _signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        /// <summary>
        /// Loads all chunks from the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The list of chunks.</returns>
        [ItemNotNull]
        public static ICollection<PngChunk> Load(string fileName)
        {
            return InternalLoad(fileName).ToList().AsReadOnly();
        }

        private static IEnumerable<PngChunk> InternalLoad(string fileName)
        {
            using var reader = new BinaryReader(File.OpenRead(fileName));
            var chunkData = reader.ReadBytes(8);

            if (!chunkData.SequenceEqual(_signature))
                yield break;

            var lastChunk = new PngChunk(chunkData, 0);

            yield return lastChunk;

            // chunk is <length[4]><tagname[4]><data[length]><checksum[4]>
            // so we must be able to read at least 12 bytes for an empty chunk.
            while (((chunkData = reader.ReadBytes(12)).Length == 12))
            {
                var len = ReadBigEndianInt(chunkData);
                if (len > 0)
                {
                    // if there is data simply seek back and read the whole chunk including data.
                    reader.BaseStream.Seek(-12, SeekOrigin.Current);
                    chunkData = reader.ReadBytes(len + 12);
                }
                lastChunk = new PngChunk(chunkData);
                yield return lastChunk;

                if (lastChunk.Tag == TerminatingChunkTag)
                    yield break;
            }

            if (lastChunk.Tag != TerminatingChunkTag)
            {
                throw new InvalidDataException($"last chunk is not {TerminatingChunkTag}: {fileName}");
            }
        }

        /// <summary>
        /// Saves all chunks to the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="chunks">The chunks.</param>
        public static void Save(string fileName, [ItemNotNull] ICollection<PngChunk> chunks)
        {
            if (!chunks.Any())
                return;

            using var writer = new BinaryWriter(File.Open(fileName, FileMode.Create));
            
            foreach (var chunk in chunks)
            {
                writer.Write(chunk.Data);
            }
        }

        private static int ReadBigEndianInt(byte[] data)
        {
            uint result = 0;
            for (var i = 0; i < 4; i++)
            {
                result = (result << 8) + data[i];
            }

            return (int)result;
        }
    }
}
