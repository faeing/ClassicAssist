﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ClassicAssist.Misc
{
    public static class Utility
    {
        public static BitmapSource ToBitmapSource(this Bitmap bmp)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
        }

        public static T GetDelegateForFunctionPointer<T>(IntPtr ptr) where T : Delegate
        {
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
        }
    }
}