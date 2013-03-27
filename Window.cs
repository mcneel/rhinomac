using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;


namespace RhinoMac
{
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
      UnsafeNativeMethods.RUI_ShowModalWindow(ControllerHandle);
    }
	/// <summary>
	/// Show the window in a modeless state.
	/// </summary>
    public void Show()
    {
      UnsafeNativeMethods.RUI_ShowWindow(ControllerHandle);
    }
  }
}