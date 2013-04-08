using System;
using System.Runtime.InteropServices;

namespace RhinoMac
{
  [StructLayout(LayoutKind.Sequential)]
  struct CGRect
  {
    public double left;
    public double top;
    public double width;
    public double height;
  }

  [StructLayout(LayoutKind.Sequential)]
  struct CColor
  {
    public CColor(System.Drawing.Color color)
    {
      redComponent = greenComponent = blueComponent = 0.0;
      includesAlpha = true;
      alphaComponent = 1.0;
      Set (color, false);
    }
    public double redComponent;
    public double greenComponent;
    public double blueComponent;
    public double alphaComponent;
    public bool includesAlpha;
    int ConvertFraction(double value)
    {
      return ((int)(Math.Max (0.0, Math.Min (1.0, value)) * 255.0));
    }
    public System.Drawing.Color ToSystemColor()
    {
      return System.Drawing.Color.FromArgb (includesAlpha ? ConvertFraction(alphaComponent) : 255,
                                            ConvertFraction(redComponent),
                                            ConvertFraction(greenComponent),
                                            ConvertFraction(blueComponent));
    }
    double ToFraction(int value)
    {
      return (value * 0.003921568627450980392156862745);
    }
    public void Set(System.Drawing.Color color, bool includeAlpha)
    {
      redComponent = ToFraction (color.R);
      greenComponent = ToFraction (color.G);
      blueComponent = ToFraction (color.B);
      includesAlpha = includeAlpha;
      alphaComponent = includeAlpha ? ToFraction (color.A) : 1.0;
    }
  }

  [System.Security.SuppressUnmanagedCodeSecurity]
  static class UnsafeNativeMethods
  {
    const string nativelib = "__Internal";

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterBoolCallbacks(Interop.GetBoolValueCallback getFunc,
                                                        Interop.SetBoolValueCallback setFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterStringCallbacks(Interop.GetStringValueCallback getFunc,
                                                          Interop.SetStringValueCallback setFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterNumberCallbacks(Interop.GetNumberValueCallback getFunc,
                                                          Interop.SetNumberValueCallback setFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterColorCallbacks(Interop.GetColorValueCallback getFunc,
                                                         Interop.SetColorValueCallback setFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterActionCallback (Interop.PerformActionCallback actionFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterWindowWillCloseCallback(Interop.WindowCallbackReturnVoid willCloseFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
		public static extern void RUI_RegisterWindowShouldCloseCallback (Interop.WindowCallbackReturnInt shouldCloseFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_CloseWindow (IntPtr windowId);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterPropertyTypeCallback(Interop.PropertyTypeCallback propertyTypeFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterActionExistsCallback(Interop.ActionExistsCallback actionExistsFunc);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr RUI_CreateWindow([MarshalAs(UnmanagedType.LPWStr)]string nib, Guid pluginId);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_ValueChanged(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string value);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_ShowModalWindow(IntPtr controller);
    
    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_ShowWindow(IntPtr handle);

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr RUI_GetWindow(IntPtr createWindowHandle);


    const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";
    
    [DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
    public extern static uint uint_objc_msgSend (IntPtr receiver, IntPtr selector);
    
    [DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
    public extern static int objc_msgSend_int (IntPtr receiver, IntPtr selector);

    [DllImport (LIBOBJC_DYLIB, EntryPoint="objc_msgSend")]
    public extern static void objc_msgSend_cgrect (IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport (LIBOBJC_DYLIB, EntryPoint="sel_registerName")]
    public extern static IntPtr sel_registerName (string name);
  }
}

