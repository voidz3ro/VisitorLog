﻿using VisitorLog.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Diagnostics;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VisitorLog
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class VisitorProcessPage1 : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public VisitorProcessPage1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

      
        #region NavigationHelper registration

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        private VisitorManager vm = null;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            vm = e.Parameter as VisitorManager;
            if(vm!=null){
            vm.prerecorded = true;
            txtFirstName.Text = vm.firstname;
            txtLastName.Text = vm.lastname;
            //txtCompany.Text = vm.companyrepresented;
            txtHost.Text = vm.boeinghostname;
                }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //do form verification
            //save visitor info
            if(String.IsNullOrEmpty(txtFirstName.Text) || String.IsNullOrEmpty(txtLastName.Text) ||  String.IsNullOrEmpty(txtHost.Text) ){
                MessageDialog msgbox = new MessageDialog("All fields are required.", "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync(); 
            }else
            {
                if (vm == null) {
                    vm = new VisitorManager();
                    vm.firstname = txtFirstName.Text;
                    vm.lastname = txtLastName.Text;
                    //vm.companyrepresented = txtCompany.Text;
                    vm.boeinghostname = txtHost.Text;

                    Debug.WriteLine("send to badge capture screen.");
                }
                else
                {
                    vm.firstname = txtFirstName.Text;
                    vm.lastname = txtLastName.Text;
                    //vm.companyrepresented = txtCompany.Text;
                    vm.boeinghostname = txtHost.Text;
                }
               
                this.Frame.Navigate(typeof(VisitorProcessPage2), vm);
            }
        }
    }
}
