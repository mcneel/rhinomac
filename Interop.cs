using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RhinoMac
{
  static class Interop
  {
    static bool g_initialized = false;
    public static void Initialize()
    {
      if( !g_initialized )
      {
        UnsafeNativeMethods.RUI_RegisterColorCallbacks(m_getcolor_callback, m_setcolor_callback);
        UnsafeNativeMethods.RUI_RegisterNumberCallbacks(m_getnumber_callback, m_setnumber_callback);
        UnsafeNativeMethods.RUI_RegisterBoolCallbacks(m_getbool_callback, m_setbool_callback);
        UnsafeNativeMethods.RUI_RegisterStringCallbacks(m_getstring_callback, m_setstring_callback);
        UnsafeNativeMethods.RUI_RegisterActionCallback(m_perform_action);
        UnsafeNativeMethods.RUI_RegisterWindowWillCloseCallback(m_willclose_callback);
        UnsafeNativeMethods.RUI_RegisterWindowShouldCloseCallback(m_shouldclose_callback);
      }
    }

    public static void RegisterRhinoWindowController(IntPtr pController, INotifyPropertyChanged viewmodel)
    {
      Initialize();
      m_all_controllers.Add(pController, viewmodel);
    }
    
    public static void RegisterRhinoWindowController(IntPtr pController, Window window)
    {
      Initialize();
      m_all_windows.Add(pController, window);
    }

    internal delegate CColor GetColorValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetColorValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, CColor value);
    internal delegate double GetNumberValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetNumberValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, double value);
    internal delegate int GetBoolValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetBoolValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, int value);
    [return: MarshalAs(UnmanagedType.LPWStr)]
    internal delegate string GetStringValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetStringValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, [MarshalAs(UnmanagedType.LPWStr)]string value);
    internal delegate void PerformActionCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void WindowCallbackReturnVoid(IntPtr handle);
    internal delegate int WindowCallbackReturnInt(IntPtr handle);
    internal delegate int PropertyTypeCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate int ActionExistsCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);

    static Dictionary<IntPtr, INotifyPropertyChanged> m_all_controllers = new Dictionary<IntPtr, INotifyPropertyChanged>();
    static Dictionary<IntPtr, Window> m_all_windows = new Dictionary<IntPtr, Window>();

    static GetColorValueCallback m_getcolor_callback = GetColorCalledFromC;
    static SetColorValueCallback m_setcolor_callback = SetColorCalledFromC;
    static GetNumberValueCallback m_getnumber_callback = GetNumberCalledFromC;
    static SetNumberValueCallback m_setnumber_callback = SetNumberCalledFromC;
    static GetBoolValueCallback m_getbool_callback = GetBoolCalledFromC;
    static SetBoolValueCallback m_setbool_callback = SetBoolCalledFromC;
    static GetStringValueCallback m_getstring_callback = GetStringCalledFromC;
    static SetStringValueCallback m_setstring_callback = SetStringCalledFromC;
    static PerformActionCallback m_perform_action = PerformActionCalledFromC;
    static WindowCallbackReturnVoid m_willclose_callback = WindowWillCloseCalledFromC;
    static WindowCallbackReturnInt m_shouldclose_callback = WindowShouldCloseCalledFromC;

    static CColor GetColorCalledFromC(IntPtr pController, string name)
    {
      var rc = System.Drawing.Color.Black;
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler (pController, name, out item);
      if (null !=  prop)
      {
        var type = prop.PropertyType;
        if (type.Equals(typeof(System.Drawing.Color)))
          rc = (System.Drawing.Color)prop.GetValue(item, null);
        else
          throw new Exception("Value is not a color");
      }
      return new CColor (rc);
    }

    static double GetNumberCalledFromC(IntPtr pController, string name)
    {
      double rc = 0;
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler(pController, name, out item);
      if( prop!=null )
      {
        var type = prop.PropertyType;
        if (type.Equals(typeof(ulong)))
          rc = (double)((ulong)prop.GetValue(item, null));
        else if (type.Equals(typeof(ushort)))
          rc = (double)((ushort)prop.GetValue(item, null));
        else if (type.Equals(typeof(double)))
          rc = (double)prop.GetValue(item, null);
        else if (type.Equals(typeof(float)))
          rc = (double)((float)prop.GetValue(item, null));
        else if (type.Equals(typeof(int)))
          rc = (double)((int)prop.GetValue(item, null));
        else if (type.Equals(typeof(decimal)))
          rc = (double)((decimal)prop.GetValue(item, null));
        else if (type.Equals(typeof(string)))
        {
          string s = (string)prop.GetValue(item, null);
          if (!double.TryParse(s, out rc))
            throw new Exception("Value is not a number");
        }
        else
          throw new Exception("Value is not a number");
      }
      return rc;
    }
    
    static int GetBoolCalledFromC(IntPtr pController, string name)
    {
      int rc = 0;
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler(pController, name, out item);
      if( prop!=null )
      {
        bool b = (bool)prop.GetValue(item, null);
        rc = b?1:0;
      }
      return rc;
    }
    
    static string GetStringCalledFromC(IntPtr pController, string name)
    {
      string rc = null;
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler(pController, name, out item);
      if( prop!=null )
      {
        rc = prop.GetValue(item, null).ToString();
      }
      return (rc ?? string.Empty);
    }

    static void SetColorCalledFromC(IntPtr pController, string name, CColor value)
    {
      var item = GetControllerFromPointer (pController);
      if (null != item)
      {
        var prop = item.GetType ().GetProperty (name);
        if (prop != null)
        {
          var type = prop.PropertyType;
          if (type.Equals (typeof(System.Drawing.Color)))
            prop.SetValue (item, value.ToSystemColor(), null);
          else
            throw new Exception("Value is not a color");
        }
      }
    }

    static void SetNumberCalledFromC(IntPtr pController, string name, double value)
    {
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler(pController, name, out item);
      if( prop!=null )
      {
        var type = prop.PropertyType;
        if (type.Equals(typeof(double)))
          prop.SetValue(item, value, null);
        else if (type.Equals(typeof(float)))
          prop.SetValue(item, (float)value, null);
        else if (type.Equals(typeof(int)))
          prop.SetValue(item, (int)value, null);
        else if (type.Equals(typeof(decimal)))
          prop.SetValue(item, new decimal(value), null);
        else if (type.Equals(typeof(string)))
          prop.SetValue(item, value.ToString(), null);
        else
          throw new Exception("Value is not a number");
      }
    }
    
    static void SetBoolCalledFromC(IntPtr pController, string name, int value)
    {
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler(pController, name, out item);
      if( prop!=null )
        prop.SetValue(item, value!=0, null);
    }
    
    static void SetStringCalledFromC(IntPtr pController, string name, string value)
    {
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler(pController, name, out item);
      if( prop!=null )
      {
        var type = prop.PropertyType;
        if (type.Equals(typeof(double)))
        {
          double x;
          if(double.TryParse(value, out x))
            prop.SetValue(item, x, null);
        }
        else if (type.Equals(typeof(float)))
        {
          float x;
          if(float.TryParse(value, out x))
            prop.SetValue(item, x, null);
        }
        else if (type.Equals(typeof(int)))
        {
          int x;
          if(int.TryParse(value, out x))
            prop.SetValue(item, x, null);
        }
        else if (type.Equals(typeof(decimal)))
        {
          decimal x;
          if(decimal.TryParse(value, out x))
            prop.SetValue(item, x, null);
        }
        else
          prop.SetValue(item, value, null);
      }
    }
    
    static void PerformActionCalledFromC(IntPtr pController, string name)
    {
      if (name == "Cancel" || name == "OK")
      {
        var window = GetWindowFromController(pController);
        if( null != window)
        {
          window.DialogResult = (name == "OK");
          UnsafeNativeMethods.RUI_CloseWindow(pController);
          return;
        }
      }
      INotifyPropertyChanged controller;
      var method = GetMethodFromController(pController, name, out controller);
      if (null != method)
        method.Invoke(controller, null);
    }
    
    static void WindowWillCloseCalledFromC(IntPtr pController)
    {
      INotifyPropertyChanged controller;
      var methodInfo = GetMethodFromController (pController, "WindowWillClose", out controller);
      if (null != methodInfo)
        methodInfo.Invoke(controller, null);
      m_all_controllers.Remove(pController);
      m_all_windows.Remove(pController);
    }

    static int WindowShouldCloseCalledFromC(IntPtr pController)
    {
      int rc = 1;
      INotifyPropertyChanged controller;
      var methodInfo = GetMethodFromController (pController, "WindowShouldClose", out controller);
      if (null != methodInfo)
      {
        var returnType = methodInfo.ReturnType;
        if (typeof(bool).Equals(returnType))
          rc = (bool)methodInfo.Invoke(controller, null) ? 1 : 0;
        else if (typeof(int).Equals(returnType))
          rc = (int)methodInfo.Invoke(controller, null);
      }
      return rc;
    }

    enum PropertyType : int
    {
      ruitypePropertyDoesNotExist = -1,
      ruitypeUnknown = 0,
      ruitypeBoolean,
      ruitypeString,
    }

    static Window GetWindowFromController(IntPtr pController)
    {
      if (m_all_windows.ContainsKey (pController))
        return m_all_windows [pController];
      return null;
    }
    
    static INotifyPropertyChanged GetControllerFromPointer(IntPtr pController)
    {
      if (m_all_controllers.ContainsKey (pController))
        return m_all_controllers [pController];
      return null;
    }
    
    static System.Reflection.PropertyInfo GetPropertyFromControler(IntPtr pController, string propertyName, out INotifyPropertyChanged controller)
    {
      controller = GetControllerFromPointer (pController);
      if (null != controller)
      {
        var prop = controller.GetType().GetProperty(propertyName);
        if (null == prop && !string.IsNullOrEmpty(propertyName))
        {
          var prefix = propertyName.Substring(0, 1);
          var sufix = propertyName.Substring(1);
          propertyName = prefix.ToUpper() + sufix;
          prop = controller.GetType().GetProperty(propertyName);
        }
        var method = null == prop ? null : prop.GetGetMethod(false);
        if (null != prop && null != method && !method.IsStatic)
          return prop;
      }
      return null;
    }

    static System.Reflection.MethodInfo GetMethodFromController(IntPtr pController, string methodName, out INotifyPropertyChanged controller)
    {
      controller = GetControllerFromPointer (pController);
      if (null != controller)
      {
        var method = controller.GetType().GetMethod(methodName);
        if (null != method && !method.IsStatic && method.IsPublic)
          return method;
      }
      return null;
    }
  }
}
