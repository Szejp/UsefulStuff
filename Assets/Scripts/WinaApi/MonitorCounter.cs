using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class MonitorCounter {

    private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);
    [DllImport("user32")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

    private static MonitorEnumProc callback;
    private static int monCount;

    static MonitorCounter() {
        callback = (IntPtr hDesktop, IntPtr hdc, ref Rect prect, int d) => ++monCount > 0;
    }

    public static int Count() {
        monCount = 0;
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);
        return monCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
