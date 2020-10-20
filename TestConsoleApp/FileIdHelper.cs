using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TestConsoleApp
{
    public static class FileIdHelper
    {
        const FileAttributes _FILE_FLAG_BACKUP_SEMANTICS = (FileAttributes)0x02000000;

        /// <inheritdoc />
        public static string GetFilePath(long fileSystemId)
        {
            using var handle = _CreateSafeFileHandle(".");

            if (handle == null || handle.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            var size = Marshal.SizeOf(typeof(FILE_ID_DESCRIPTOR));
            var descriptor = new FILE_ID_DESCRIPTOR { Type = FILE_ID_TYPE.FileIdType, FileId = fileSystemId, dwSize = size };

            try
            {
                using var handle2 = _OpenFileById(handle, ref descriptor);

                if (handle2 == null || handle2.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }

                const int length = 128;
                var builder = new StringBuilder(length);
                _GetFinalPathNameByHandleW(handle2, builder, length, 0);
                return builder.ToString();
            }
            catch
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return null;
        }

        public static long GetFileSystemId(string path)
        {
            if (File.Exists(path))
            {
                var fileInfo = _Get_File_Information(path);
                return ((long) fileInfo.FileIndexHigh << 32) + (long) fileInfo.FileIndexLow;
            }

            var folderInfo = _Get_Folder_Information(path);
            return ((long)folderInfo.FileIndexHigh << 32) + (long)folderInfo.FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW")]
        static extern SafeFileHandle _CreateFileW(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            [In, MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            [In] IntPtr lpSecurityAttributes,
            [In, MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [In, MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            [In] IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "OpenFileById")]
        static extern SafeFileHandle _OpenFileById(
            [In] SafeFileHandle hVolumeHint,
            [In, Out] ref FILE_ID_DESCRIPTOR lpFileId,
            [In, MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            [In] IntPtr lpSecurityAttributes,
            [In, MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes
        );

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetFileInformationByHandle")]
        static extern bool _GetFileInformationByHandle(
            SafeFileHandle hFile,
            out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetFinalPathNameByHandleW")]
        static extern int _GetFinalPathNameByHandleW(
            SafeFileHandle hFile,
            StringBuilder lpszFilePath,
            int cchFilePath,
            int dwFlags);

        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path" /> was not found.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"><paramref name="path" /> was not found.</exception>
        static BY_HANDLE_FILE_INFORMATION _Get_File_Information(string path)
        {
            using var handle = _CreateSafeFileHandle(path);

            if (handle == null || handle.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (_GetFileInformationByHandle(handle, out var hInfo) == false)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return hInfo;
        }

        /// <exception cref="System.IO.FileNotFoundException"><paramref name="path" /> was not found.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException"><paramref name="path" /> was not found.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        static BY_HANDLE_FILE_INFORMATION _Get_Folder_Information(string path)
        {
            using var handle = _CreateSafeFileHandle(path);

            if (handle == null || handle.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (_GetFileInformationByHandle(handle, out var hInfo) == false)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return hInfo;
        }

        static SafeFileHandle _CreateSafeFileHandle(string path) =>
            _CreateFileW(
                path,
                FileAccess.Read,
                FileShare.ReadWrite,
                IntPtr.Zero,
                FileMode.Open,
                _FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);

        static SafeFileHandle _OpenFileById(SafeFileHandle hint, ref FILE_ID_DESCRIPTOR fileId) =>
        _OpenFileById(
            hint,
            ref fileId,
            FileAccess.ReadWrite,
            FileShare.ReadWrite,
            IntPtr.Zero,
            _FILE_FLAG_BACKUP_SEMANTICS);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FILE_ID_DESCRIPTOR
    {
        [FieldOffset(0)]
        public int dwSize;
        [FieldOffset(4)]
        public FILE_ID_TYPE Type;
        [FieldOffset(8)]
        public long FileId;
        [FieldOffset(8)]
        public Guid ObjectId;
        [FieldOffset(8)]
        public Guid ExtendedFileId; //Use for ReFS; need to use v3 structures or later instead of v2 as done in this sample
    }


    struct BY_HANDLE_FILE_INFORMATION
    {
        public uint FileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    public enum FILE_ID_TYPE
    {
        FileIdType = 0,
        ObjectIdType = 1,
        ExtendedFileIdType = 2,
        MaximumFileIdType
    };
}