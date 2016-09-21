using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Tools
{
#if !WINDOWS_APP && !WINDOWS_UWP
    public class ShellFileOperation
    {
        public static int DeleteFiles(bool showDialog, IntPtr? windowHandle, params string[] files)
        {
            var fixedFiles = files
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar))
                .ToArray();

            var path = string.Join("\0", fixedFiles) + "\0\0";


            var flags = Native.FOFlags.FOF_ALLOWUNDO
                | (showDialog ? Native.FOFlags.FOF_WANTNUKEWARNING : Native.FOFlags.FOF_NOCONFIRMATION);

            Native.SHFILEOPSTRUCT sh = new Native.SHFILEOPSTRUCT();
            sh.hwnd = windowHandle ?? IntPtr.Zero;
            sh.wFunc = Native.FOFunc.FO_DELETE;
            sh.pFrom = path;
            sh.pTo = null;
            sh.fFlags = flags;
            sh.fAnyOperationsAborted = false;
            sh.hNameMappings = IntPtr.Zero;
            sh.lpszProgressTitle = null;

            return Native.SHFileOperation(ref sh);
        }
    }

    internal static class Native
    {
        public enum FOFunc : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004
        }

        public enum FOFlags : ushort
        {
            FOF_MULTIDESTFILES = 0x0001,
            FOF_CONFIRMMOUSE = 0x0002,
            FOF_SILENT = 0x0004,  // don't create progress/report
            FOF_RENAMEONCOLLISION = 0x0008,
            FOF_NOCONFIRMATION = 0x0010,  // Don't prompt the user.
            FOF_WANTMAPPINGHANDLE = 0x0020,  // Fill in SHFILEOPSTRUCT.hNameMappings
                                             // Must be freed using SHFreeNameMappings
            FOF_ALLOWUNDO = 0x0040,
            FOF_FILESONLY = 0x0080,  // on *.*, do only files
            FOF_SIMPLEPROGRESS = 0x0100,  // means don't show names of files
            FOF_NOCONFIRMMKDIR = 0x0200,  // don't confirm making any needed dirs
            FOF_NOERRORUI = 0x0400,  // don't put up error UI
            FOF_NOCOPYSECURITYATTRIBS = 0x0800,  // dont copy NT file Security Attributes
            FOF_NORECURSION = 0x1000,  // don't recurse into directories.
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,  // don't operate on connected elements.
            FOF_WANTNUKEWARNING = 0x4000,  // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            FOF_NORECURSEREPARSE = 0x8000  // treat reparse points as objects, not containers
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FOFunc wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FOFlags fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);
    }
#endif
}
