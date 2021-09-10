using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TreeViewCaliburn.FolderBrowser
{
    public static class ShellIcon
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Shfileinfo
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
        [DllImport("shell32.dll")] internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref Shfileinfo psfi, uint cbSizeFileInfo, uint uFlags);
        public static ImageSource GetShellIcon(string filePath)
        {
            ImageSource shellIcon = null;
            try
            {
                Shfileinfo shinfo = new Shfileinfo();

                /*
                private const uint SHGFI_ICON = 0x100;
                private const uint SHGFI_LARGEICON = 0x0;
                private const uint SHGFI_SMALLICON = 0x000000001;
                */
                SHGetFileInfo(
                        filePath,
                        0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                        0x100 | 0x0);

                using (System.Drawing.Icon i = System.Drawing.Icon.FromHandle(shinfo.hIcon))
                {
                    //Convert icon to a Bitmap source
                    shellIcon = Imaging.CreateBitmapSourceFromHIcon(
                                            i.Handle,
                                            new Int32Rect(0, 0, i.Width, i.Height),
                                            BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch
            {
                try
                {
                    System.Drawing.Icon fileIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    if (fileIcon != null)
                    {
                        using (Bitmap iconBitmap = fileIcon.ToBitmap())
                        {
                            System.IO.MemoryStream ms = new System.IO.MemoryStream();
                            iconBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            shellIcon = BitmapFrame.Create(ms);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return shellIcon;
        }
    }
}
