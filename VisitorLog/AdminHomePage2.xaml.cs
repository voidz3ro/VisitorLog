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
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using VisitorLog.VisitorLogService;
using System.Threading.Tasks;


namespace VisitorLog
{
   
    public sealed partial class AdminHomePage2 : Page
    {
        private VisitorLogServiceClient vlClient = new VisitorLogServiceClient(new VisitorLogServiceClient.EndpointConfiguration());
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

     
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public AdminHomePage2()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }
       
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

      
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LogSettings));
        }

        private void btnUsername_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VisitorHomePage));
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void btnUsername_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UsernamePassword));
        }

        private void btnCloseLog_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }

        private void btnViewSignins_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VisitorSignout));
        }

        private void btnUsername_Copy1_Click(object sender, RoutedEventArgs e)
        {
            //schedule a visit poorlyt named, meh.
            this.Frame.Navigate(typeof(ScheduleVisit));
        }

        private void btnViewSignins_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewScheduledVisits));
        }

        private void btnViewData_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewData));
        }

        private async void  btnTransmit_Click(object sender, RoutedEventArgs e)
        {

            ProgressPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            TransmitProgress.Value = 0;

           // Debug.WriteLine("Last transmite date: " + App.appman.GetLastTransmitDate());

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
            XDocument doc = XDocument.Load(file.Path);
            IEnumerable<XElement> actions = from nactions in doc.Root.Elements("Action")
                                            select nactions;
            double total = actions.Count();
            double progInc = 100/total;
          //  Debug.WriteLine(total + " actions total");
            int foo = 0;
            foreach (XElement action in actions)
            {
                var person = action.Element("Person");
                if (DateTime.Parse(person.Element("Date").Value) >= App.appman.GetLastTransmitDate())
                {
                    string ncompany = person.Element("Company-Represented").Value;
                    string naction = action.Attribute("type").Value;
                    DateTime _datetime = DateTime.Parse(person.Element("Date").Value);
                    bool ps;
                    if(person.Element("Pre-Screened").Value == "Yes") {
                        ps = true;

                    }
                    else { ps = false; }
                    
                    try
                    {
                        string recordResult = await vlClient.RecordActionAsync(naction, ncompany, App.appman.GetCountry(), App.appman.GetCity(), App.appman.GetSublocation(), ps, App.appman.GetEntity(), _datetime);
                    }
                    catch (Exception es)
                    {
                        Debug.WriteLine("Error contacting service: " + es.Message);
                    }
               //     Debug.WriteLine(TransmitProgress.Value+progInc);

                    TransmitProgress.Value += (progInc);
                    foo++;
                }
                
            }
            
           // Debug.WriteLine(foo + " new actions sent to service...");
            ProgressPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            App.appman.localSettings.Values["Last-Transmit-Date"] = DateTime.Now.ToString();
        }
    }
}
