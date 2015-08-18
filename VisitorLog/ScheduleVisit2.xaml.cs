using VisitorLog.Common;
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
    public sealed partial class ScheduleVisit2 : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private VisitorManager vm;
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


        private bool alreadyEntered = false;
        private int thisID = 0;
       
       
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
            navigationHelper.OnNavigatedTo(e);
            vm = e.Parameter as VisitorManager;
           // Debug.WriteLine("Hey, it's ScheduledVisit2. I see you've handed me a visitor '" + vm.firstname + " " + vm.lastname + ", ID: " + vm.ID);
           // Debug.WriteLine("Their bemsid is: " + vm.bemsid);
            thisID = vm.ID;
            if (vm.PreScreenedByInitials != "")
            {
                togPrescreened.IsOn = true;
            }


            if(vm.bemsid != 0){
                togBoeing.IsOn = true;
                lblCompBems.Text = "BEMSID";
                txtCompBems.Text = vm.bemsid.ToString();
            }

            if (vm.companyrepresented != null && vm.companyrepresented != "" && vm.bemsid == 0)
            {
                txtCompBems.Text = vm.companyrepresented;
            }
        }

        public ScheduleVisit2()
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

       

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            double bemsNum = 0;

            if (String.IsNullOrEmpty(txtCompBems.Text))
            {
                MessageDialog msgbox = new MessageDialog("Company or Boeing BEMSID required.", "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync();
            }
            else if (!(Double.TryParse(txtCompBems.Text, out bemsNum)) && togBoeing.IsOn)
            {
                MessageDialog msgbox = new MessageDialog("BEMSID must be numeric.", "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync();
            }
            else
            {
                vm.SetPrescreened(togPrescreened.IsOn);

                if (togBoeing.IsOn)
                {
                    vm.companyrepresented = "Boeing";
                    vm.bemsid = Convert.ToInt64(txtCompBems.Text);
                }
                else
                {
                    vm.companyrepresented = txtCompBems.Text;
                    vm.bemsid = 0;
                }

             
               // Debug.WriteLine("updating node" + thisID);
                await vm.update("Scheduled Visit");

                this.Frame.Navigate(typeof(ScheduleVisit3));
            }
            

        }

        private void togBoeing_Toggled(object sender, RoutedEventArgs e)
        {
            if (togBoeing.IsOn)
            {
                lblCompBems.Text = "BEMSID";
                txtCompBems.PlaceholderText = "Enter BEMSID of employee";
                txtCompBems.Text = "";
                
            }
            else
            {
                lblCompBems.Text = "Company";
                txtCompBems.PlaceholderText = "Who do they represent?";
                txtCompBems.Text = "";
            }
        }
    }
}
