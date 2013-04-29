using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RhinoMac
{
  /// <summary>
  /// Window on the mac.
  /// 
  /// Binding "OK:" or "Cancel:" to buttons
  /// will automatically case the Window to close and set the 
  /// DialogResult accordingly; null == closed by clicking the
  /// "X" in the window, true == closed by "OK:", and false ==
  /// closed by "Cancel:".
  /// 

  /// The following methods will get called on the view model
  /// when appropriate if they exist and are public:
  ///   void WindowWillClose()
  ///   bool WindowShouldClose()
  ///   ulong TreeNodeCount(string name, ref ulong[] indexes, ulong length)
  /// 
  /// </summary>
  [CLSCompliant(false)]
  public class Window : MonoMac.AppKit.NSWindow
  {
    IntPtr m_pDNWindowController = IntPtr.Zero; // DNWindowController*
  	/// <summary>
  	/// Create a Window from a NIB file and attach the view model as the window
  	/// controller.
  	/// </summary>
  	/// <returns>
  	/// Returns a new Window object bound to the specified NIB file on success
  	/// or null on error.
  	/// </returns>
  	/// <param name="nib">NIB file to load.</param>
  	/// <param name="viewModel">View model to attach to the new window.</param>
    public static Window FromNib(string nib, INotifyPropertyChanged viewModel)
    {
      var pi = Rhino.PlugIns.PlugIn.Find(viewModel.GetType().Assembly);
      IntPtr pDNWindowController = UnsafeNativeMethods.RUI_CreateWindow(nib, pi.Id);
      if( pDNWindowController==IntPtr.Zero )
        return null;
      Interop.RegisterRhinoWindowController(pDNWindowController, viewModel);
      IntPtr pWindow = UnsafeNativeMethods.RUI_GetWindow(pDNWindowController);
      Window rc = new Window(viewModel, pDNWindowController, pWindow);
      Interop.RegisterRhinoWindowController(pDNWindowController, rc);
      return rc;
    }
    /// <summary>
  	/// Initializes a new instance of the <see cref="RhinoMac.Window"/> class.
  	/// </summary>
  	/// <param name="viewmodel">Viewmodel.</param>
  	/// <param name="pController">P controller.</param>
  	/// <param name="pWindow">P window.</param>
  	Window(INotifyPropertyChanged viewmodel, IntPtr pController, IntPtr pWindow)
        : base(pWindow)
    {
      m_pDNWindowController = pController;
      ViewModel = viewmodel;
      ViewModel.PropertyChanged += HandlePropertyChanged;
      // If there is a Window property on the view model and it is of
      // type Window then set its value to this window.
      var property = ViewModel.GetType ().GetProperty ("Window", typeof(RhinoMac.Window));
      if (null != property)
        property.SetValue (ViewModel, this, null);
    }
  	/// <summary>
  	/// Handles the property changed.
  	/// </summary>
  	/// <param name="sender">Sender.</param>
  	/// <param name="e">E.</param>
    void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
    {
      if( m_pDNWindowController!=IntPtr.Zero )
        UnsafeNativeMethods.RUI_ValueChanged(m_pDNWindowController, e.PropertyName);
    }
  	/// <summary>
  	/// Gets the view model controler passed to FromNib.
  	/// </summary>
  	/// <value>The view model.</value>
    public INotifyPropertyChanged ViewModel { get; private set; }
    IntPtr ControllerHandle { get { return m_pDNWindowController; } }
  	/// <summary>
  	/// Shows the window in a modal state.
  	/// </summary>
    public void ShowModal()
    {
      m_ShowModalList.Add (this);
      UnsafeNativeMethods.RUI_ShowModalWindow(ControllerHandle);
      if (m_ShowModalList.Contains(this))
        m_ShowModalList.Remove (this);
    }
    /// <summary>
    /// Modal dialog stack used by PickPoint
    /// </summary>
    static List<Window>m_ShowModalList = new List<Window>();
    /// <summary>
    /// Picks the point.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="pickPointDelegate">Pick point delegate.</param>
    public void PushPickButton(object sender, EventHandler<EventArgs> pickPointDelegate)
    {
      if (null == pickPointDelegate)
        return;
      // Find this window in the modal list
      // Windows get added to m_ShowModalList by Window.ShowModal(), find this
      // window and work backwards to root
      int index = m_ShowModalList.IndexOf (this);
      var _controllers = new List<MonoMac.Foundation.NSObject> ();
      for (var i = index; i >= 0; i--)
      {
        // Get the window to unhook and hide
        var window = m_ShowModalList[i];
        // Get the window controller
        var controller = window.WindowController;
        // Save the window controller so it can be used when the window is restored
        _controllers.Add(controller);
        // Unhook and hide the window
        window.OrderOut (controller);
      }
      //
      // Code from RhinoCommon
      //
      // Flush main window message pump
      Rhino.RhinoApp.Wait();
      // Set focus to the main Rhino window
      Rhino.RhinoApp.SetFocusToMainWindow();
      // Invoke the Get... call
      pickPointDelegate(sender, EventArgs.Empty);
      // Restore the unhooked and hidden windows in the opposite
      // order of how they were hidden
      for (var i = 0; i <= index; i++)
      {
        var item = m_ShowModalList [i];
        item.MakeKeyAndOrderFront (_controllers[index-i]);
      }
    }
  	/// <summary>
  	/// Show the window in a modeless state.
  	/// </summary>
    public void Show()
    {
      UnsafeNativeMethods.RUI_ShowWindow(ControllerHandle);
    }

    public void RhinoCloseWindow()
    {
      UnsafeNativeMethods.RUI_CloseWindow (ControllerHandle);
    }
    /// <summary>
    /// Gets or sets the dialog result.
    /// </summary>
    /// <value>The dialog result.</value>
    public bool? DialogResult { get; set; }
  }
}
