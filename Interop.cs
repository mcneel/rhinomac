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
        UnsafeNativeMethods.RUI_RegisterValueCallbacks(m_getvalue_callback, m_setvalue_callback);
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

    internal delegate IntPtr GetValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void SetValueCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name, IntPtr value);

    internal delegate void PerformActionCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate void WindowCallbackReturnVoid(IntPtr handle);
    internal delegate int WindowCallbackReturnInt(IntPtr handle);
    internal delegate int PropertyTypeCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);
    internal delegate int ActionExistsCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)]string name);

    static Dictionary<IntPtr, INotifyPropertyChanged> m_all_controllers = new Dictionary<IntPtr, INotifyPropertyChanged>();
    static Dictionary<IntPtr, Window> m_all_windows = new Dictionary<IntPtr, Window>();

    static GetValueCallback m_getvalue_callback = GetValueCalledFromC;
    static SetValueCallback m_setvalue_callback = SetValueCalledFromC;
    static PerformActionCallback m_perform_action = PerformActionCalledFromC;
    static WindowCallbackReturnVoid m_willclose_callback = WindowWillCloseCalledFromC;
    static WindowCallbackReturnInt m_shouldclose_callback = WindowShouldCloseCalledFromC;

    static IntPtr GetValueCalledFromC(IntPtr pController, string name)
    {
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler (pController, name, out item);
      if (null !=  prop)
      {
        object p = prop.GetValue(item, null);
        // If value is null and the property type is a string
        // then set p to a empty string so a valid NSString can 
        // get returned
        if (null == p && prop.PropertyType.IsAssignableFrom(typeof(System.String)))
          p = string.Empty;
        if( p is string )
        {
          var s = new MonoMac.Foundation.NSString(p as String ?? string.Empty);
          return s.Handle;
        }
        if( p is bool )
        {
          var b = new MonoMac.Foundation.NSNumber((bool)p);
          return b.Handle;
        }
        if( p is bool? )
        {
          var b = new MonoMac.Foundation.NSNumber((bool)(p ?? false));
          return b.Handle;
        }
        if( p is int )
        {
          var i = new MonoMac.Foundation.NSNumber((int)p);
          return i.Handle;
        }
        if( p is double )
        {
          var d = new MonoMac.Foundation.NSNumber((double)p);
          return d.Handle;
        }
        if( p is System.Drawing.Color )
        {
          System.Drawing.Color c = (System.Drawing.Color)p;
          var clr = MonoMac.AppKit.NSColor.FromDeviceRgba(c.R/255.0, c.G/255.0, c.B/255.0, c.A/255.0);
          return clr.Handle;
        }
        if (p is string[])
        {
          var stringList = p as string[];
          var array = MonoMac.Foundation.NSArray.FromStrings(stringList);
          return array.Handle;
        }
        if (p is List<string>)
        {
          var stringList = p as List<string>;
          var array = MonoMac.Foundation.NSArray.FromStrings(stringList.ToArray());
          return array.Handle;
        }
        // Allow NSObjects to be passed directly to Rhino
        if (p is MonoMac.Foundation.NSObject)
          return (p as MonoMac.Foundation.NSObject).Handle;
        string msg = string.Format("Do not have binding for '{0}'\ntype = {1}",name, prop.PropertyType);
        var alert = MonoMac.AppKit.NSAlert.WithMessage("GetValueCalledFromC with unknown type", null, null, null, msg);
        alert.RunModal();
      }

      return IntPtr.Zero;
    }

    static void SetValueCalledFromC(IntPtr pController, string name, IntPtr pValue)
    {
      INotifyPropertyChanged item;
      var prop = GetPropertyFromControler (pController, name, out item);
      if (null !=  prop && pValue!=IntPtr.Zero)
      {
        var obj = new MonoMac.Foundation.NSObject(pValue);
        var type = prop.PropertyType;
        if( type.Equals(typeof(string)) )
        {
          prop.SetValue(item, obj.ToString() ?? string.Empty, null);
        }
        else if( type.Equals(typeof(bool)) )
        {
          var sel = new MonoMac.ObjCRuntime.Selector("boolValue");
          if( obj.RespondsToSelector(sel) )
          {
            bool b = MonoMac.ObjCRuntime.Messaging.bool_objc_msgSend (pValue, sel.Handle);
            prop.SetValue(item, b, null);
          }
        }
        else if( type.Equals(typeof(bool?)) )
        {
          var sel = new MonoMac.ObjCRuntime.Selector("boolValue");
          if( obj.RespondsToSelector(sel) )
          {
            bool b = MonoMac.ObjCRuntime.Messaging.bool_objc_msgSend (pValue, sel.Handle);
            prop.SetValue(item, b, null);
          }
        }
        else if( type.Equals(typeof(int)) )
        {
          var sel = new MonoMac.ObjCRuntime.Selector("intValue");
          if( obj.RespondsToSelector(sel) )
          {
            int i = MonoMac.ObjCRuntime.Messaging.int_objc_msgSend (pValue, sel.Handle);
            prop.SetValue(item, i, null);
          }
        }
        else if( type == typeof(double) )
        {
          var sel = new MonoMac.ObjCRuntime.Selector("doubleValue");
          if( obj.RespondsToSelector(sel) )
          {
            double d = MonoMac.ObjCRuntime.Messaging.Double_objc_msgSend (pValue, sel.Handle);
            prop.SetValue(item, d, null);
          }
        }
        else if( type.Equals(typeof(System.Drawing.Color)) )
        {
          var sel = new MonoMac.ObjCRuntime.Selector("getRed:green:blue:alpha:");
          if( obj.RespondsToSelector(sel) )
          {
            double red, green, blue, alpha;
            MonoMac.ObjCRuntime.Messaging.void_objc_msgSend_out_Double_out_Double_out_Double_out_Double (pValue, sel.Handle, out red, out green, out blue, out alpha);
            var c4f = new Rhino.Display.Color4f((float)red, (float)green, (float)blue, (float)alpha);
            prop.SetValue(item, c4f.AsSystemColor(), null);
          }
        }
//        else if (type.Equals(typeof(MonoMac.Foundation.NSIndexSet)))
//        {
//          //var test = MonoMac.ObjCRuntime.Runtime.GetNSObject(pValue);
//          //var indexSet = Activator.CreateInstance (type, new object[] { pValue });
//          var indexSet = new MonoMac.Foundation.NSIndexSet(pValue);
//          prop.SetValue(item, indexSet, null);
//        }
        else if (typeof(MonoMac.Foundation.NSObject).IsAssignableFrom(type))
        {
          var setValue = Activator.CreateInstance (type, new object[] { pValue });
          prop.SetValue(item, setValue, null);
        }
        else
        {
          string msg = string.Format("Do not have binding for '{0}'\ntype = {1}",name, type);
          var alert = MonoMac.AppKit.NSAlert.WithMessage("SetValueCalledFromC with unknown type", null, null, null, msg);
          alert.RunModal();
        }
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
