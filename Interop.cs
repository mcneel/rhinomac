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
        UnsafeNativeMethods.RUI_RegisterNumberCallbacks(m_getnumber_callback, m_setnumber_callback);
        UnsafeNativeMethods.RUI_RegisterBoolCallbacks(m_getbool_callback, m_setbool_callback);
        UnsafeNativeMethods.RUI_RegisterStringCallbacks(m_getstring_callback, m_setstring_callback);
        UnsafeNativeMethods.RUI_RegisterActionCallback(m_perform_action);
        UnsafeNativeMethods.RUI_RegisterWindowWillCloseCallback(m_willclose_callback);
      }
    }

    public static void RegisterRhinoWindowController(IntPtr pController, INotifyPropertyChanged viewmodel)
    {
      Initialize();
      m_all_controllers.Add(pController, viewmodel);
    }
    

    internal delegate double GetNumberValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetNumberValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, double value);
    internal delegate int GetBoolValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetBoolValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, int value);
    [return: MarshalAs(UnmanagedType.LPWStr)]
    internal delegate string GetStringValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetStringValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, [MarshalAs(UnmanagedType.LPWStr)]string value);
    internal delegate void PerformActionCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void WindowWillCloseCallback(IntPtr handle);
    internal delegate int PropertyTypeCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate int ActionExistsCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    
    
    static Dictionary<IntPtr, INotifyPropertyChanged> m_all_controllers = new Dictionary<IntPtr, INotifyPropertyChanged>();
    static GetNumberValueCallback m_getnumber_callback = GetNumberCalledFromC;
    static SetNumberValueCallback m_setnumber_callback = SetNumberCalledFromC;
    static GetBoolValueCallback m_getbool_callback = GetBoolCalledFromC;
    static SetBoolValueCallback m_setbool_callback = SetBoolCalledFromC;
    static GetStringValueCallback m_getstring_callback = GetStringCalledFromC;
    static SetStringValueCallback m_setstring_callback = SetStringCalledFromC;
    static PerformActionCallback m_perform_action = PerformActionCalledFromC;
    static WindowWillCloseCallback m_willclose_callback = WindowWillCloseCalledFromC;

    static double GetNumberCalledFromC(IntPtr pController, string name)
    {
      double rc = 0;
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
        if( prop!=null )
        {
          var type = prop.PropertyType;
          if (type.Equals(typeof(double)))
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
      }
      return rc;
    }
    
    static int GetBoolCalledFromC(IntPtr pController, string name)
    {
      int rc = 0;
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
        if( prop!=null )
        {
          bool b = (bool)prop.GetValue(item, null);
          rc = b?1:0;
        }
      }
      return rc;
    }
    
    static string GetStringCalledFromC(IntPtr pController, string name)
    {
      string rc = null;
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
        if( prop!=null )
        {
          rc = prop.GetValue(item, null).ToString();
        }
      }
      return rc;
    }
    
    static void SetNumberCalledFromC(IntPtr pController, string name, double value)
    {
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
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
    }
    
    static void SetBoolCalledFromC(IntPtr pController, string name, int value)
    {
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
        if( prop!=null )
          prop.SetValue(item, value!=0, null);
      }
    }
    
    static void SetStringCalledFromC(IntPtr pController, string name, string value)
    {
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
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
    }
    
    static void PerformActionCalledFromC(IntPtr pController, string name)
    {
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var method = item.GetType().GetMethod(name);
        if( method!=null )
          method.Invoke(item, null);
      }
    }
    
    static void WindowWillCloseCalledFromC(IntPtr pController)
    {
      m_all_controllers.Remove(pController);
    }
    
    enum PropertyType : int
    {
      ruitypePropertyDoesNotExist = -1,
      ruitypeUnknown = 0,
      ruitypeBoolean,
      ruitypeString,
    }
    static int PropertyTypeCalledFromC(IntPtr pController, string name)
    {
      PropertyType rc = PropertyType.ruitypePropertyDoesNotExist;
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var prop = item.GetType().GetProperty(name);
        if( prop!=null )
        {
          rc = PropertyType.ruitypeUnknown;
          if( prop.PropertyType == typeof(bool) )
            rc = PropertyType.ruitypeBoolean;
          else if(prop.PropertyType == typeof(string) )
            rc = PropertyType.ruitypeString;
        }
      }
      return (int)rc;
    }
    
    static int ActionExistsCalledFromC(IntPtr pController, string name)
    {
      int rc = 0;
      if( m_all_controllers.ContainsKey(pController) )
      {
        var item = m_all_controllers[pController];
        var method = item.GetType().GetMethod(name);
        if( method!=null )
          rc = 1;
      }
      return rc;
    }
    
  }
}
