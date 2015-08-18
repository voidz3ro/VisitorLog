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
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;
using Windows.Storage.FileProperties;
using VisitorLog;
using Windows.Data.Xml;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Windows;
using System.Threading.Tasks;



namespace VisitorLog
{
   
    public sealed partial class VisitorFindAppointment2 : Page
    {

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


        public VisitorFindAppointment2()
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

        async void txtName_KeyUp(object sender, KeyRoutedEventArgs e)
        {

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            lvItems.Items.Clear();

            Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
            XDocument doc = XDocument.Load(file.Path);
            var ps = (
                from actions in doc.Root.Elements("Action").Elements("Person")
                select actions
            );
            if (txtName.Text.Count() > 0)
            {
                IEnumerable<XElement> persons = (
                    from actions in doc.Root.Elements("Action")
                    where (string)actions.Attribute("type") == "Scheduled Visit"
                    from person in actions.Elements("Person")
                    where (person.Element("First-Name").Value.ToUpper().Contains(txtName.Text.ToUpper()) || person.Element("Last-Name").Value.ToUpper().Contains(txtName.Text.ToUpper()))

                    from dates in person.Elements("Date")
                    orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                   // where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date //this was giving them a headache
                    select person
                );

                foreach (XElement signin in persons)
                {
                    ScheduledVisitTile tile = new ScheduledVisitTile();
                    tile.person = signin;
                    tile.txtName.Text = signin.Element("Last-Name").Value.ToString() + ", " + signin.Element("First-Name").Value.ToString();
                    tile.txtCompany.Text = signin.Element("Company-Represented").Value.ToString();
                    var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                    var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                    DateTime dateToFormat = Convert.ToDateTime(signin.Element("Time-In").Value.ToString());
                    var thedate = formatter.Format(dateToFormat);
                    var thetime = formatterTime.Format(dateToFormat);
                    tile.txtDate.Text = thedate + " " + thetime;
                    lvItems.Items.Add(tile);
                }
            }

        }

        private void lvItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.AddedItems.Count() > 0)
            {
                if (TopAppBar.IsOpen == false)
                {
                    TopAppBar.IsOpen = true;
                    Storyboard1.Begin();
                    
                }
            }
            else
            {
                TopAppBar.IsOpen = false;
            }


        }
        //private async void txtName_Copy_KeyUp(object sender, KeyRoutedEventArgs e)
        //{
        //    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

        //    lvItems.Items.Clear();

        //    //   Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
        //    XDocument doc = XDocument.Load(file.Path);
        //    var ps = (
        //        from actions in doc.Root.Elements("Action").Elements("Person")
        //        select actions
        //    );

        //    if (txtName_Copy.Text.Count() > 0)
        //    {

        //        IEnumerable<XElement> persons = (
        //            from actions in doc.Root.Elements("Action")
        //            where (string)actions.Attribute("type") == "Scheduled Visit"
        //            from person in actions.Elements("Person")
        //            where (person.Element("Company-Represented").Value.ToUpper().Contains(txtName_Copy.Text.ToUpper()))
        //            from dates in person.Elements("Date")
        //            orderby DateTime.Parse(person.Element("Time-In").Value) ascending
        //            where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
        //            select person
        //        );

        //        foreach (XElement signin in persons)
        //        {
        //            ScheduledVisitTile tile = new ScheduledVisitTile();
        //            tile.person = signin;
        //            tile.txtName.Text = signin.Element("Last-Name").Value.ToString() + ", " + signin.Element("First-Name").Value.ToString();
        //            tile.txtCompany.Text = signin.Element("Company-Represented").Value.ToString();
        //            var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
        //            var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
        //            DateTime dateToFormat = Convert.ToDateTime(signin.Element("Time-In").Value.ToString());
        //            var thedate = formatter.Format(dateToFormat);
        //            var thetime = formatterTime.Format(dateToFormat);
        //            tile.txtDate.Text = thedate + " " + thetime;
        //            lvItems.Items.Add(tile);


        //        }
        //    }

        //}

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {

            ScheduledVisitTile thisTile = lvItems.SelectedItem as ScheduledVisitTile;
            if (thisTile != null)
            {
                VisitorManager vm = new VisitorManager();
                vm.parsePerson(thisTile.person);
               // this.Frame.Navigate(typeof(VisitorProcessPage1), vm);
                if (Convert.ToBoolean(App.appman.localSettings.Values["UsePrivacyStatement"]))
                {
                    this.Frame.Navigate(typeof(PrivacyStatement), vm);
                }
                else
                {
                    this.Frame.Navigate(typeof(VisitorProcessPage1), vm);
                }
                
            }
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

            }
}
