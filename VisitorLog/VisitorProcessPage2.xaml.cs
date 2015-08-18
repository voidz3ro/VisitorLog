using VisitorLog.Common;
using System;
using System.Diagnostics;
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
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VisitorLog
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class VisitorProcessPage2 : Page
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

        private VisitorManager vm = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            navigationHelper.OnNavigatedTo(e);
            vm = e.Parameter as VisitorManager;

            //default to no boeing emp
            RadBoeEmpNo.IsChecked = true;

            if (vm.bemsid != 0)
            {
                //togPrescreened.IsOn = true;
                //txtCompany.Text = vm.bemsid.ToString();
                if (RadBoeEmpYes != null) { RadBoeEmpYes.IsChecked = true; }
               
                txtCompany.Text = vm.bemsid.ToString();
            }
            else
            {
                if (vm.companyrepresented!=null)
                {
                    txtCompany.Text = vm.companyrepresented;
                }
                
            }
        }
        


        public VisitorProcessPage2()
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

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

     
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //save the additional values
            vm.badgenumber = txtBadgeNumber.Text;
            double bemsNum = 0;

            if (txtCompany.Text == "")
            {
                String msg = "";
                if (RadBoeEmpYes.IsChecked==true)
                {
                    msg = "BEMSID is required";
                }
                else
                {
                    msg = "Company is required";
                }

                MessageDialog msgbox = new MessageDialog(msg, "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync();

            }
            else if (!(Double.TryParse(txtCompany.Text, out bemsNum)) && RadBoeEmpYes.IsChecked==true)
            {
                MessageDialog msgbox = new MessageDialog("BEMSID must be numeric", "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync();
            }
            else
            {



                if (RadBoeEmpYes.IsChecked==true)
                {
                    // vm.PreScreenedByInitials = App.appman.GetCurrentUser(); //need to make this a routine that grabs the current username or something.
                    //this is boeingemp now
                    vm.boeingEmp = true;
                    vm.bemsid = Convert.ToInt64(txtCompany.Text);
                    vm.companyrepresented = "Boeing";
                }
                else
                {
                    vm.boeingEmp = false;
                    vm.companyrepresented = txtCompany.Text;
                }



                if (!vm.prerecorded)
                {
                    await vm.record("Sign In");
                }
                else
                {
                    await vm.update("Sign In");
                }

                //send em to confirmation
                this.Frame.Navigate(typeof(VisitorProcessPage3));
            }
        }


        private void togPrescreened_Toggled_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void RadBoeEmpYes_Checked(object sender, RoutedEventArgs e)
        {
            if (RadBoeEmpYes != null && RadBoeEmpYes.IsChecked == true)
            {
                pageTitle_Copy2.Text = "BEMSID:";
                txtCompany.PlaceholderText = "Enter your BEMSID...";
                txtCompany.Text = "";
            }
            else
            {
                pageTitle_Copy2.Text = "Company Represented";
                txtCompany.PlaceholderText = "Who do you work for?";
                txtCompany.Text = "";
            }
        }

     
    }
}
