using System;
using System.Runtime.InteropServices;

namespace RhinoMac
{
  [System.Security.SuppressUnmanagedCodeSecurity]
  static class UnsafeNativeMethods
  {
    const string nativelib = "__Internal";

    [DllImport(nativelib, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RUI_RegisterValueCallbacks (Interop.GetValueCallback getFunc,
                                                          Interop.SetValueCallback setFunc);
    /*
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
    */
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
  }
}

