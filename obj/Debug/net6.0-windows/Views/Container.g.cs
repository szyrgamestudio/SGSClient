﻿#pragma checksum "..\..\..\..\Views\Container.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "859C61A13C622955C8BB32C22B6C182BC5C2B50D"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.42000
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using SGSClient.Views.Pages;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Wpf.Ui;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Markup;


namespace SGSClient.Views {
    
    
    /// <summary>
    /// Container
    /// </summary>
    public partial class Container : Wpf.Ui.Controls.UiWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 32 "..\..\..\..\Views\Container.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid RootMainGrid;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\Views\Container.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.NavigationStore RootNavigation;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\..\Views\Container.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Frame RootFrame;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\..\Views\Container.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Snackbar RootSnackbar;
        
        #line default
        #line hidden
        
        
        #line 118 "..\..\..\..\Views\Container.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid RootWelcomeGrid;
        
        #line default
        #line hidden
        
        
        #line 149 "..\..\..\..\Views\Container.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Dialog updateDialog;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.2.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SGSClient;component/views/container.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Container.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.2.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.RootMainGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.RootNavigation = ((Wpf.Ui.Controls.NavigationStore)(target));
            
            #line 46 "..\..\..\..\Views\Container.xaml"
            this.RootNavigation.Navigated += new Wpf.Ui.Common.RoutedNavigationEvent(this.RootNavigation_OnNavigated);
            
            #line default
            #line hidden
            return;
            case 3:
            this.RootFrame = ((System.Windows.Controls.Frame)(target));
            return;
            case 4:
            this.RootSnackbar = ((Wpf.Ui.Controls.Snackbar)(target));
            return;
            case 5:
            this.RootWelcomeGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 6:
            this.updateDialog = ((Wpf.Ui.Controls.Dialog)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

