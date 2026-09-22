// ZipStorer, by Jaime Olivares
// Website: zipstorer.codeplex.com
// Version: 2.35 (March 14, 2010)
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

/// <summary>
/// Unique class for compression/decompression file. Represents a Zip file.
/// </summary>
public class ZipFile : IDisposable
{
    /// <summary>
    /// Compression method enumeration
    /// </summary>
    public enum CompressionMethod : ushort
    {
        /// <summary>Uncompressed storage</summary> 
        Store = 0,
        /// <summary>Deflate compression method</summary>
        Deflate = 8
    }

    /// <summary>
    /// Represents an entry in Zip file directory
    /// </summary>
    public struct Entry
    {
        /// <summary>Compression method</summary>
        public CompressionMethod Method;
        /// <summary>Full path and filename as stored in Zip</summary>
        public string FileName;
        /// <summary>Original file size</summary>
        public uint FileSize;
        /// <summary>Compressed file size</summary>
        public uint CompressedSize;
        /// <summary>Offset of header information inside Zip storage</summary>
        public uint HeaderOffset;
        /// <summary>Offset of file inside Zip storage</summary>
        public uint FileOffset;
        /// <summary>Size of header information</summary>
        public uint HeaderSize;
        /// <summary>32-bit checksum of entire file</summary>
        public uint Crc32;
        /// <summary>Last modification time of file</summary>
        public DateTime ModifiedTime;
        /// <summary>User comment for file</summary>
        public string Comment;
        /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
        public bool EncodeUTF8;

        /// <summary>Overriden method</summary>
        /// <returns>File name in Zip</returns>
        public override string ToString()
        {
            return this.FileName;
        }
    }

    #region Public fields
    /// <summary>True if UTF8 encoding for filename and comments, true by default (ignore CP 437)</summary>
    public bool EncodeUTF8 = true;
    /// <summary>Force deflate algotithm even if it inflates the stored file. Off by default.</summary>
    public bool ForceDeflating = false;
    #endregion

    #region Private fields
    // List of files to store
    private List<Entry> Files = new List<Entry>();
    // Filename of storage file
    private string FileName;
    // Stream object of storage file
    private Stream ZipFileStream;
    // General comment
    private string Comment = "";
    // Central dir image
    private byte[] CentralDirectoryImage = null;
    // Existing files in zip
    private ushort ExistingFiles = 0;
    // File access for Open method
    private FileAccess Access;
    // Static CRC32 Table
    private static UInt32[] CrcTable = null;
    // Default filename encoder
    private static Encoding DefaultEncoding = Encoding.GetEncoding(437);
    #endregion

    #region Public methods
    // Static constructor. Just invoked once in order to create the CRC32 lookup table.
    static ZipFile()
    {
        // Generate CRC32 table
        CrcTable = new UInt32[256];
        for (int i = 0; i < CrcTable.Length; i++)
        {
            UInt32 c = (UInt32)i;
            for (int j = 0; j < 8; j++)
            {
                if ((c & 1) != 0)
                    c = 3988292384 ^ (c >> 1);
                else
                    c >>= 1;
            }
            CrcTable[i] = c;
        }
    }
    /// <summary>
    /// Method to create a new storage file
    /// </summary>
    /// <param name="_filename">Full path of Zip file to create</param>
    /// <param name="_comment">General comment for Zip file</param>
    /// <returns>A valid ZipStorer object</returns>
    public static ZipFile Create(string filename, string comment)
    {
        Stream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);

        ZipFile zip = Create(stream, comment);
        zip.Comment = comment;
        zip.FileName = filename;

        return zip;
    }
    /// <summary>
    /// Method to create a new zip storage in a stream
    /// </summary>
    /// <param name="_stream"></param>
    /// <param name="_comment"></param>
    /// <returns>A valid ZipStorer object</returns>
    public static ZipFile Create(Stream stream, string comment)
    {
        ZipFile zip = new ZipFile();
        zip.Comment = comment;
        zip.ZipFileStream = stream;
        zip.Access = FileAccess.Write;

        return zip;
    }
    /// <summary>
    /// Method to open an existing storage file
    /// </summary>
    /// <param name="_filename">Full path of Zip file to open</param>
    /// <param name="_access">File access mode as used in FileStream constructor</param>
    /// <returns>A valid ZipStorer object</returns>
    public static ZipFile Open(string filename, FileAccess file_access)
    {
        Stream stream = (Stream)new FileStream(filename, FileMode.Open, file_access == FileAccess.Read ? FileAccess.Read : FileAccess.ReadWrite);

        ZipFile zip = Open(stream, file_access);
        zip.FileName = filename;

        return zip;
    }
    /// <summary>
    /// Method to open an existing storage from stream
    /// </summary>
    /// <param name="_stream">Already opened stream with zip contents</param>
    /// <param name="_access">File access mode for stream operations</param>
    /// <returns>A valid ZipStorer object</returns>
    public static ZipFile Open(Stream stream, FileAccess file_access)
    {
        if (!stream.CanSeek && file_access != FileAccess.Read)
            throw new InvalidOperationException("Stream cannot seek");

        ZipFile zip = new ZipFile();
        //zip.FileName = _filename;
        zip.ZipFileStream = stream;
        zip.Access = file_access;

        if (zip.ReadFileInfo())
            return zip;

        throw new System.IO.InvalidDataException();
    }
    /// <summary>
    /// Add full contents of a file into the Zip storage
    /// </summary>
    /// <param name="_method">Compression method</param>
    /// <param name="_pathname">Full path of file to add to Zip storage</param>
    /// <param name="_fileName">Filename and path as desired in Zip directory</param>
    /// <param name="_comment">Comment for stored file</param>        
    public void AddFile(CompressionMethod compression_method, string path, string filename, string comment)
    {
        if (Access == FileAccess.Read)
            throw new InvalidOperationException("Writing is not alowed");

        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        AddStream(compression_method, filename, stream, File.GetLastWriteTime(path), comment);
        stream.Close();
    }
    /// <summary>
    /// Add full contents of a stream into the Zip storage
    /// </summary>
    /// <param name="compression_method">Compression method</param>
    /// <param name="fileName">Filename and path as desired in Zip directory</param>
    /// <param name="source">Stream object containing the data to store in Zip</param>
    /// <param name="modified_time">Modification time of the data to store</param>
    /// <param name="comment">Comment for stored file</param>
    public void AddStream(CompressionMethod compression_method, string filename, Stream source, DateTime modified_time, string comment)
    {
        if (Access == FileAccess.Read)
            throw new InvalidOperationException("Writing is not alowed");

        long offset;
        if (this.Files.Count == 0)
            offset = 0;
        else
        {
            Entry last = this.Files[this.Files.Count - 1];
            offset = last.HeaderOffset + last.HeaderSize;
        }

        // Prepare the fileinfo
        Entry zfe = new Entry();
        zfe.Method = compression_method;
        zfe.EncodeUTF8 = this.EncodeUTF8;
        zfe.FileName = NormalizedFilename(filename);
        zfe.Comment = (comment == null ? "" : comment);

        // Even though we write the header now, it will have to be rewritten, since we don't know compressed size or crc.
        zfe.Crc32 = 0;  // to be updated later
        zfe.HeaderOffset = (uint)this.ZipFileStream.Position;  // offset within file of the start of this local record
        zfe.ModifiedTime = modified_time;

        // Write local header
        WriteLocalHeader(ref zfe);
        zfe.FileOffset = (uint)this.ZipFileStream.Position;

        // Write file to zip (store)
        Store(ref zfe, source);
        source.Close();

        this.UpdateCrcAndSizes(ref zfe);

        Files.Add(zfe);
    }
    /// <summary>
    /// Updates central directory (if pertinent) and close the Zip storage
    /// </summary>
    /// <remarks>This is a required step, unless automatic dispose is used</remarks>
    public void Close()
    {
        if (this.Access != FileAccess.Read)
        {
            uint centralOffset = (uint)this.ZipFileStream.Position;
            uint centralSize = 0;

            if (this.CentralDirectoryImage != null)
                this.ZipFileStream.Write(CentralDirectoryImage, 0, CentralDirectoryImage.Length);

            for (int i = 0; i < Files.Count; i++)
            {
                long pos = this.ZipFileStream.Position;
                this.WriteCentralDirectoryRecord(Files[i]);
                centralSize += (uint)(this.ZipFileStream.Position - pos);
            }

            if (this.CentralDirectoryImage != null)
                this.WriteEndRecord(centralSize + (uint)CentralDirectoryImage.Length, centralOffset);
            else
                this.WriteEndRecord(centralSize, centralOffset);
        }

        if (this.ZipFileStream != null)
        {
            this.ZipFileStream.Flush();
            this.ZipFileStream.Dispose();
            this.ZipFileStream = null;
        }
    }
    /// <summary>
    /// Read all the file records in the central directory 
    /// </summary>
    /// <returns>List of all entries in directory</returns>
    public List<Entry> GetEntries()
    {
        if (this.CentralDirectoryImage == null)
            throw new InvalidOperationException("Central directory currently does not exist");

        List<Entry> result = new List<Entry>();

        for (int pointer = 0; pointer < this.CentralDirectoryImage.Length; )
        {
            uint signature = BitConverter.ToUInt32(CentralDirectoryImage, pointer);
            if (signature != 0x02014b50)
                break;

            bool encodeUTF8 = (BitConverter.ToUInt16(CentralDirectoryImage, pointer + 8) & 0x0800) != 0;
            ushort method = BitConverter.ToUInt16(CentralDirectoryImage, pointer + 10);
            uint modifiedTime = BitConverter.ToUInt32(CentralDirectoryImage, pointer + 12);
            uint crc32 = BitConverter.ToUInt32(CentralDirectoryImage, pointer + 16);
            uint comprSize = BitConverter.ToUInt32(CentralDirectoryImage, pointer + 20);
            uint fileSize = BitConverter.ToUInt32(CentralDirectoryImage, pointer + 24);
            ushort filenameSize = BitConverter.ToUInt16(CentralDirectoryImage, pointer + 28);
            ushort extraSize = BitConverter.ToUInt16(CentralDirectoryImage, pointer + 30);
            ushort commentSize = BitConverter.ToUInt16(CentralDirectoryImage, pointer + 32);
            uint headerOffset = BitConverter.ToUInt32(CentralDirectoryImage, pointer + 42);
            uint headerSize = (uint)(46 + filenameSize + extraSize + commentSize);

            Encoding encoding = encodeUTF8 ? Encoding.UTF8 : DefaultEncoding;

            Entry entry = new Entry();
            entry.Method = (CompressionMethod)method;
            entry.FileName = encoding.GetString(CentralDirectoryImage, pointer + 46, filenameSize);
            entry.FileOffset = GetFileOffset(headerOffset);
            entry.FileSize = fileSize;
            entry.CompressedSize = comprSize;
            entry.HeaderOffset = headerOffset;
            entry.HeaderSize = headerSize;
            entry.Crc32 = crc32;
            entry.ModifiedTime = DosTimeToDateTime(modifiedTime);
            if (commentSize > 0)
                entry.Comment = encoding.GetString(CentralDirectoryImage, pointer + 46 + filenameSize + extraSize, commentSize);

            result.Add(entry);
            pointer += (46 + filenameSize + extraSize + commentSize);
        }

        return result;
    }
    /// <summary>
    /// Copy the contents of a stored file into a physical file
    /// </summary>
    /// <param name="entry">Entry information of file to extract</param>
    /// <param name="filename">Name of file to store uncompressed data</param>
    /// <returns>True if success, false if not.</returns>
    /// <remarks>Unique compression methods are Store and Deflate</remarks>
    public bool ExtractFile(Entry entry, string filename)
    {
        // Make sure the parent directory exist
        string path = System.IO.Path.GetDirectoryName(filename);
        if (String.IsNullOrEmpty(path))
            path = ".";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        // Check if entry already exist is directory. If so, do nothing
        if (Directory.Exists(filename))
            return true;

        Stream output = new FileStream(filename, FileMode.Create, FileAccess.Write);
        bool result = ExtractFile(entry, output);
        if (result)
            output.Close();

        File.SetCreationTime(filename, entry.ModifiedTime);
        File.SetLastWriteTime(filename, entry.ModifiedTime);

        return result;
    }
    /// <summary>
    /// Copy the contents of a stored file into an opened stream
    /// </summary>
    /// <param name="entry">Entry information of file to extract</param>
    /// <param name="stream">Stream to store the uncompressed data</param>
    /// <returns>True if success, false if not.</returns>
    /// <remarks>Unique compression methods are Store and Deflate</remarks>
    public bool ExtractFile(Entry entry, Stream stream)
    {
        if (!stream.CanWrite)
            throw new InvalidOperationException("Stream cannot be written");

        // check signature
        byte[] signature = new byte[4];
        this.ZipFileStream.Seek(entry.HeaderOffset, SeekOrigin.Begin);
        this.ZipFileStream.Read(signature, 0, 4);
        if (BitConverter.ToUInt32(signature, 0) != 0x04034b50)
            return false;

        // Select input stream for inflating or just reading
        Stream inStream;
        if (entry.Method == CompressionMethod.Store)
            inStream = this.ZipFileStream;
        else if (entry.Method == CompressionMethod.Deflate)
            inStream = new DeflateStream(this.ZipFileStream, CompressionMode.Decompress, true);
        else
            return false;

        // Buffered copy
        byte[] buffer = new byte[16384];
        this.ZipFileStream.Seek(entry.FileOffset, SeekOrigin.Begin);
        uint bytesPending = entry.FileSize;
        while (bytesPending > 0)
        {
            int bytesRead = inStream.Read(buffer, 0, (int)Math.Min(bytesPending, buffer.Length));
            stream.Write(buffer, 0, bytesRead);
            bytesPending -= (uint)bytesRead;
        }
        stream.Flush();

        if (entry.Method == CompressionMethod.Deflate)
            inStream.Dispose();
        return true;
    }
    /// <summary>
    /// Removes one of many files in storage. It creates a new Zip file.
    /// </summary>
    /// <param name="zip_file">Reference to the current Zip object</param>
    /// <param name="entries">List of Entries to remove from storage</param>
    /// <returns>True if success, false if not</returns>
    /// <remarks>This method only works for storage of type FileStream</remarks>
    public static bool RemoveEntries(ref ZipFile zip_file, List<Entry> entries)
    {
        if (!(zip_file.ZipFileStream is FileStream))
            throw new InvalidOperationException("RemoveEntries is allowed just over streams of type FileStream");


        //Get full list of entries
        List<Entry> fullList = zip_file.GetEntries();

        //In order to delete we need to create a copy of the zip file excluding the selected items
        string tempZipName = Path.GetTempFileName();
        string tempEntryName = Path.GetTempFileName();

        try
        {
            ZipFile tempZip = ZipFile.Create(tempZipName, "");

            foreach (Entry zfe in fullList)
            {
                if (!entries.Contains(zfe))
                {
                    if (zip_file.ExtractFile(zfe, tempEntryName))
                    {
                        tempZip.AddFile(zfe.Method, tempEntryName, zfe.FileName, zfe.Comment);
                    }
                }
            }
            zip_file.Close();
            tempZip.Close();

            File.Delete(zip_file.FileName);
            File.Move(tempZipName, zip_file.FileName);

            zip_file = ZipFile.Open(zip_file.FileName, zip_file.Access);
        }
        catch
        {
            return false;
        }
        finally
        {
            if (File.Exists(tempZipName))
                File.Delete(tempZipName);
            if (File.Exists(tempEntryName))
                File.Delete(tempEntryName);
        }
        return true;
    }
    #endregion

    #region Private methods
    // Calculate the file offset by reading the corresponding local header
    private uint GetFileOffset(uint header_offset)
    {
        byte[] buffer = new byte[2];

        this.ZipFileStream.Seek(header_offset + 26, SeekOrigin.Begin);
        this.ZipFileStream.Read(buffer, 0, 2);
        ushort filenameSize = BitConverter.ToUInt16(buffer, 0);
        this.ZipFileStream.Read(buffer, 0, 2);
        ushort extraSize = BitConverter.ToUInt16(buffer, 0);

        return (uint)(30 + filenameSize + extraSize + header_offset);
    }
    /* Local file header:
        local file header signature     4 bytes  (0x04034b50)
        version needed to extract       2 bytes
        general purpose bit flag        2 bytes
        compression method              2 bytes
        last mod file time              2 bytes
        last mod file date              2 bytes
        crc-32                          4 bytes
        compressed size                 4 bytes
        uncompressed size               4 bytes
        filename length                 2 bytes
        extra field length              2 bytes

        filename (variable size)
        extra field (variable size)
    */
    private void WriteLocalHeader(ref Entry entry)
    {
        long position = this.ZipFileStream.Position;
        Encoding encoding = entry.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
        byte[] encodedFilename = encoding.GetBytes(entry.FileName);

        this.ZipFileStream.Write(new byte[] { 80, 75, 3, 4, 20, 0 }, 0, 6); // No extra header
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)(entry.EncodeUTF8 ? 0x0800 : 0)), 0, 2); // filename and comment encoding 
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)entry.Method), 0, 2);  // zipping method
        this.ZipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(entry.ModifiedTime)), 0, 4); // zipping date and time
        this.ZipFileStream.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12); // unused CRC, un/compressed size, updated later
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedFilename.Length), 0, 2); // filename length
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length

        this.ZipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
        entry.HeaderSize = (uint)(this.ZipFileStream.Position - position);
    }
    /* Central directory's File header:
        central file header signature   4 bytes  (0x02014b50)
        version made by                 2 bytes
        version needed to extract       2 bytes
        general purpose bit flag        2 bytes
        compression method              2 bytes
        last mod file time              2 bytes
        last mod file date              2 bytes
        crc-32                          4 bytes
        compressed size                 4 bytes
        uncompressed size               4 bytes
        filename length                 2 bytes
        extra field length              2 bytes
        file comment length             2 bytes
        disk number start               2 bytes
        internal file attributes        2 bytes
        external file attributes        4 bytes
        relative offset of local header 4 bytes

        filename (variable size)
        extra field (variable size)
        file comment (variable size)
    */
    private void WriteCentralDirectoryRecord(Entry entry)
    {
        Encoding encoder = entry.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
        byte[] encodedFilename = encoder.GetBytes(entry.FileName);
        byte[] encodedComment = encoder.GetBytes(entry.Comment);

        this.ZipFileStream.Write(new byte[] { 80, 75, 1, 2, 23, 0xB, 20, 0 }, 0, 8);
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)(entry.EncodeUTF8 ? 0x0800 : 0)), 0, 2); // filename and comment encoding 
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)entry.Method), 0, 2);  // zipping method
        this.ZipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(entry.ModifiedTime)), 0, 4);  // zipping date and time
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.Crc32), 0, 4); // file CRC
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.CompressedSize), 0, 4); // compressed file size
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.FileSize), 0, 4); // uncompressed file size
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedFilename.Length), 0, 2); // Filename in zip
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedComment.Length), 0, 2);

        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // disk=0
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // file type: binary
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // Internal file attributes
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0x8100), 0, 2); // External file attributes (normal/readable)
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.HeaderOffset), 0, 4);  // Offset of header

        this.ZipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
        this.ZipFileStream.Write(encodedComment, 0, encodedComment.Length);
    }
    /* End of central dir record:
        end of central dir signature    4 bytes  (0x06054b50)
        number of this disk             2 bytes
        number of the disk with the
        start of the central directory  2 bytes
        total number of entries in
        the central dir on this disk    2 bytes
        total number of entries in
        the central dir                 2 bytes
        size of the central directory   4 bytes
        offset of start of central
        directory with respect to
        the starting disk number        4 bytes
        zipfile comment length          2 bytes
        zipfile comment (variable size)
    */
    private void WriteEndRecord(uint size, uint offset)
    {
        Encoding encoder = this.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
        byte[] encodedComment = encoder.GetBytes(this.Comment);

        this.ZipFileStream.Write(new byte[] { 80, 75, 5, 6, 0, 0, 0, 0 }, 0, 8);
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + ExistingFiles), 0, 2);
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + ExistingFiles), 0, 2);
        this.ZipFileStream.Write(BitConverter.GetBytes(size), 0, 4);
        this.ZipFileStream.Write(BitConverter.GetBytes(offset), 0, 4);
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedComment.Length), 0, 2);
        this.ZipFileStream.Write(encodedComment, 0, encodedComment.Length);
    }
    // Copies all source file into storage file
    private void Store(ref Entry entry, Stream source)
    {
        byte[] buffer = new byte[16384];
        int bytesRead;
        uint totalRead = 0;
        Stream outStream;

        long posStart = this.ZipFileStream.Position;
        long sourceStart = source.Position;

        if (entry.Method == CompressionMethod.Store)
            outStream = this.ZipFileStream;
        else
            outStream = new DeflateStream(this.ZipFileStream, CompressionMode.Compress, true);

        entry.Crc32 = 0 ^ 0xffffffff;

        do
        {
            bytesRead = source.Read(buffer, 0, buffer.Length);
            totalRead += (uint)bytesRead;
            if (bytesRead > 0)
            {
                outStream.Write(buffer, 0, bytesRead);

                for (uint i = 0; i < bytesRead; i++)
                {
                    entry.Crc32 = ZipFile.CrcTable[(entry.Crc32 ^ buffer[i]) & 0xFF] ^ (entry.Crc32 >> 8);
                }
            }
        } while (bytesRead == buffer.Length);
        outStream.Flush();

        if (entry.Method == CompressionMethod.Deflate)
            outStream.Dispose();

        entry.Crc32 ^= 0xffffffff;
        entry.FileSize = totalRead;
        entry.CompressedSize = (uint)(this.ZipFileStream.Position - posStart);

        // Verify for real compression
        if (entry.Method == CompressionMethod.Deflate && !this.ForceDeflating && source.CanSeek && entry.CompressedSize > entry.FileSize)
        {
            // Start operation again with Store algorithm
            entry.Method = CompressionMethod.Store;
            this.ZipFileStream.Position = posStart;
            this.ZipFileStream.SetLength(posStart);
            source.Position = sourceStart;
            this.Store(ref entry, source);
        }
    }
    /* DOS Date and time:
        MS-DOS date. The date is a packed value with the following format. Bits Description 
            0-4 Day of the month (1-31) 
            5-8 Month (1 = January, 2 = February, and so on) 
            9-15 Year offset from 1980 (add 1980 to get actual year) 
        MS-DOS time. The time is a packed value with the following format. Bits Description 
            0-4 Second divided by 2 
            5-10 Minute (0-59) 
            11-15 Hour (0-23 on a 24-hour clock) 
    */
    private uint DateTimeToDosTime(DateTime datetime)
    {
        return (uint)(
            (datetime.Second / 2) | (datetime.Minute << 5) | (datetime.Hour << 11) |
            (datetime.Day << 16) | (datetime.Month << 21) | ((datetime.Year - 1980) << 25));
    }
    private DateTime DosTimeToDateTime(uint dos_datetime)
    {
        return new DateTime(
            (int)(dos_datetime >> 25) + 1980,
            (int)(dos_datetime >> 21) & 15,
            (int)(dos_datetime >> 16) & 31,
            (int)(dos_datetime >> 11) & 31,
            (int)(dos_datetime >> 5) & 63,
            (int)(dos_datetime & 31) * 2);
    }

    /* CRC32 algorithm
      The 'magic number' for the CRC is 0xdebb20e3.  
      The proper CRC pre and post conditioning is used,
      meaning that the CRC register is pre-conditioned
      with all ones (a starting value of 0xffffffff)
      and the value is post-conditioned by taking the
      one's complement of the CRC residual. If bit 3 of the
      general purpose flag is set, this field is set to zero
      in the local header and the correct value is put
      in the data descriptor and in the central directory.
    */
    private void UpdateCrcAndSizes(ref Entry entry)
    {
        long lastPos = this.ZipFileStream.Position;  // remember position

        this.ZipFileStream.Position = entry.HeaderOffset + 8;
        this.ZipFileStream.Write(BitConverter.GetBytes((ushort)entry.Method), 0, 2);  // zipping method

        this.ZipFileStream.Position = entry.HeaderOffset + 14;
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.Crc32), 0, 4);  // Update CRC
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.CompressedSize), 0, 4);  // Compressed size
        this.ZipFileStream.Write(BitConverter.GetBytes(entry.FileSize), 0, 4);  // Uncompressed size

        this.ZipFileStream.Position = lastPos;  // restore position
    }
    // Replaces backslashes with slashes to store in zip header
    private string NormalizedFilename(string filename)
    {
        filename = filename.Replace('\\', '/');

        int pos = filename.IndexOf(':');
        if (pos > -1)
            filename = filename.Remove(0, pos + 1);

        return filename.Trim('/');
    }
    // Reads the end-of-central-directory record
    private bool ReadFileInfo()
    {
        if (this.ZipFileStream.Length < 22)
            return false;

        try
        {
            this.ZipFileStream.Seek(-17, SeekOrigin.End);
            BinaryReader br = new BinaryReader(this.ZipFileStream);
            do
            {
                this.ZipFileStream.Seek(-5, SeekOrigin.Current);
                UInt32 sig = br.ReadUInt32();
                if (sig == 0x06054b50)
                {
                    this.ZipFileStream.Seek(6, SeekOrigin.Current);

                    UInt16 entries = br.ReadUInt16();
                    Int32 centralSize = br.ReadInt32();
                    UInt32 centralDirOffset = br.ReadUInt32();
                    UInt16 commentSize = br.ReadUInt16();

                    // check if comment field is the very last data in file
                    if (this.ZipFileStream.Position + commentSize != this.ZipFileStream.Length)
                        return false;

                    // Copy entire central directory to a memory buffer
                    this.ExistingFiles = entries;
                    this.CentralDirectoryImage = new byte[centralSize];
                    this.ZipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                    this.ZipFileStream.Read(this.CentralDirectoryImage, 0, centralSize);

                    // Leave the pointer at the begining of central dir, to append new files
                    this.ZipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                    return true;
                }
            } while (this.ZipFileStream.Position > 0);
        }
        catch { }

        return false;
    }
    #endregion

    #region IDisposable Members
    /// <summary>
    /// Closes the Zip file stream
    /// </summary>
    public void Dispose()
    {
        this.Close();
    }
    #endregion
}
