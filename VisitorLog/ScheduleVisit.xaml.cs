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
    public sealed partial class ScheduleVisit : Page
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


        public ScheduleVisit()
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
            
            if(String.IsNullOrEmpty(txtFirstName.Text) || String.IsNullOrEmpty(txtLastName.Text) || String.IsNullOrEmpty(txtHost.Text) ){
                MessageDialog msgbox = new MessageDialog("All fields are required.", "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync();
            }
            else 
            { 
            VisitorManager vm = new VisitorManager();
            vm.firstname = txtFirstName.Text; vm.lastname = txtLastName.Text; /*vm.companyrepresented = txtCompany.Text;*/ vm.boeinghostname = txtHost.Text;
             
            TimeSpan ts = dpTime.Time;
            Debug.WriteLine("the time is " + ts);
            DateTimeOffset dto = dpDate.Date;
            DateTime dt = dto.DateTime; 
           // DateTime ndt = dt + ts; //no worky
            DateTime date = new DateTime(dpDate.Date.Year, dpDate.Date.Month, dpDate.Date.Day, dpTime.Time.Hours, dpTime.Time.Minutes, 00);

            Debug.WriteLine("the date is " + date);

            vm.timein = date;
            vm.EnteredByAdminDate = DateTime.Now;
            vm.EnteredByAdmin = 1;
            vm.prerecorded = true;
           
            if(!alreadyEntered){
                Debug.WriteLine("adding the node");
                await vm.record("Scheduled Visit");
            }
            else
            {
                vm.ID = thisID;
                Debug.WriteLine("updating node" + vm.ID);
                await vm.update("Scheduled Visit");
            }
            
            this.Frame.Navigate(typeof(ScheduleVisit3));
            }
        }

        private bool alreadyEntered = false;
        private int thisID = 0;
        private VisitorManager vm = null;
        private long thisBEMS = 0;
        string company = "";
        string screenedby = "";
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            vm = e.Parameter as VisitorManager;
           
            if (vm != null)
            {
                txtFirstName.Text = vm.firstname;
                txtLastName.Text = vm.lastname;
                //txtCompany.Text = vm.companyrepresented;
                txtHost.Text = vm.boeinghostname;
                //how about hte date
                dpDate.Date = DateTimeOffset.Parse(vm.timein.ToString());
                dpTime.Time = TimeSpan.Parse(vm.timein.TimeOfDay.ToString());
                alreadyEntered = true;
                thisID = vm.ID;
                thisBEMS = vm.bemsid;
                company = vm.companyrepresented;
                screenedby = vm.PreScreenedByInitials;
                Debug.WriteLine("Hello from " + vm.firstname + " " + vm.lastname + ", ID:" + vm.ID +  " bemsid: "  + vm.bemsid);
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
           this.Frame.Navigate(typeof(VisitorProcessPage1), vm);
        }

        private async void btnSave_Click_1(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtFirstName.Text) || String.IsNullOrEmpty(txtLastName.Text) || String.IsNullOrEmpty(txtHost.Text))
            {
                MessageDialog msgbox = new MessageDialog("All fields are required.", "Problem...");
                msgbox.Commands.Clear();
                msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                var res = await msgbox.ShowAsync();
            }
            else
            {
                VisitorManager vm = new VisitorManager();
                vm.firstname = txtFirstName.Text; vm.lastname = txtLastName.Text;  vm.boeinghostname = txtHost.Text;

                TimeSpan ts = dpTime.Time;
                Debug.WriteLine("the time is " + ts);
                DateTimeOffset dto = dpDate.Date;
                DateTime dt = dto.DateTime;
                DateTime date = new DateTime(dpDate.Date.Year, dpDate.Date.Month, dpDate.Date.Day, dpTime.Time.Hours, dpTime.Time.Minutes, 00);

               // Debug.WriteLine("the date is " + date);

                vm.timein = date;
                vm.EnteredByAdminDate = DateTime.Now;
                vm.EnteredByAdmin = 1;
                vm.prerecorded = true;


                if (!alreadyEntered)
                {
                   // Debug.WriteLine("adding the node");
                    await vm.record("Scheduled Visit");

                }
                else
                {
                    vm.ID = thisID;
                    vm.bemsid = thisBEMS;
                    vm.companyrepresented = company;
                    vm.PreScreenedByInitials = screenedby;
                    Debug.WriteLine("updating bemsid: " + vm.bemsid);
                   // Debug.WriteLine("updating node" + vm.ID);
                    await vm.update("Scheduled Visit");
                }


                this.Frame.Navigate(typeof(ScheduleVisit2), vm);
            }
        }
    }
}
