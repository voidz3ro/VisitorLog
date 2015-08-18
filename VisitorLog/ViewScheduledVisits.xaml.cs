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
   
    public sealed partial class ViewScheduledVisits : Page
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


        public ViewScheduledVisits()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            loadVisits();
        }

        private async Task loadVisits()
        {
           
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
          
            try { 
                
                XDocument doc = XDocument.Load(file.Path);
                var ps = (
                    from actions in doc.Root.Elements("Action").Elements("Person")
                    select actions
                );
                IEnumerable<XElement> persons = (
                    from actions in doc.Root.Elements("Action")
                    where (string)actions.Attribute("type") == "Scheduled Visit"
                    from person in actions.Elements("Person")
                    //  where mylist.Contains(person.Element("ID").Value)
                    from dates in person.Elements("Date")
                    orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                   // orderby Convert.ToDateTime(person.Element("Date")); 
                    // where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
                    select person
                );
               


                foreach (XElement signin in persons)
                {
                    if (Convert.ToDateTime(signin.Element("Time-In").Value).Date == DateTime.Now.Date)
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

                foreach (XElement signin in persons)
                {
                    if (Convert.ToDateTime(signin.Element("Time-In").Value).Date != DateTime.Now.Date)
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
            catch (Exception e)
            {
                Debug.WriteLine("Error loading file...");
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.InnerException);
            }
            


           
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

        private async void txtName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
           
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            lvItems.Items.Clear();
         
             //   Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
                XDocument doc = XDocument.Load(file.Path);
                var ps = (
                    from actions in doc.Root.Elements("Action").Elements("Person")
                    select actions
                );
                IEnumerable<XElement> persons = (
                    from actions in doc.Root.Elements("Action")
                    where (string)actions.Attribute("type") == "Scheduled Visit"
                    from person in actions.Elements("Person")
                    where (person.Element("First-Name").Value.Contains(txtName.Text) || person.Element("Last-Name").Value.Contains(txtName.Text))
                  
                    from dates in person.Elements("Date")
                    orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                    // orderby Convert.ToDateTime(person.Element("Date"));
                    // where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
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

        private async void txtName_Copy_KeyUp(object sender, KeyRoutedEventArgs e)
        {
             StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            lvItems.Items.Clear();
         
             //   Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
                XDocument doc = XDocument.Load(file.Path);
                var ps = (
                    from actions in doc.Root.Elements("Action").Elements("Person")
                    select actions
                );
                IEnumerable<XElement> persons = (
                    from actions in doc.Root.Elements("Action")
                    where (string)actions.Attribute("type") == "Scheduled Visit"
                    from person in actions.Elements("Person")
                    where (person.Element("Company-Represented").Value.Contains(txtName_Copy.Text))
                    from dates in person.Elements("Date")
                    orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                    // orderby Convert.ToDateTime(person.Element("Date"));
                    // where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
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

        private async void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            lvItems.Items.Clear();

            //   Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
            XDocument doc = XDocument.Load(file.Path);
            var ps = (
                from actions in doc.Root.Elements("Action").Elements("Person")
                select actions
            );

           

            IEnumerable<XElement> persons = (
                from actions in doc.Root.Elements("Action")
                where (string)actions.Attribute("type") == "Scheduled Visit"
                from person in actions.Elements("Person")
                where (DateTimeOffset.Parse(person.Element("Time-In").Value).Date.Date == dtPicker.Date.Date)
                from dates in person.Elements("Date")
                orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                // orderby Convert.ToDateTime(person.Element("Date"));
                // where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
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

        private void lvItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.AddedItems.Count() > 0)
            {
                if(TopAppBar.IsOpen == false){
                    TopAppBar.IsOpen = true;
                }
            }
            else
            {
                TopAppBar.IsOpen = false;
            }

            /*
           ScheduledVisitTile thisTile = e.AddedItems[0] as ScheduledVisitTile;
           VisitorManager vm = new VisitorManager();
           vm.parsePerson(thisTile.person);
           this.Frame.Navigate(typeof(ScheduleVisit), vm);
            * */
        }

        private int curNid = 0;

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ScheduledVisitTile thisTile = lvItems.SelectedItem as ScheduledVisitTile; // .AddedItems[0] as ScheduledVisitTile;
            VisitorManager vm = new VisitorManager();
            vm.parsePerson(thisTile.person);
            int nid = vm.ID;
           
            MessageDialog msgbox = new MessageDialog("Are you sure you want to delete this scheduled visit?", "Delete scheduled visit?");

            msgbox.Commands.Clear();

            msgbox.Commands.Add(new UICommand("Delete", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgbox.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            curNid = nid;
            await msgbox.ShowAsync();

           
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Delete")
            {
                new VisitorManager().delete(curNid);
                lvItems.Items.Remove(lvItems.SelectedItem);
            }
            else
            {
                curNid = 0;
            }
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
//edit
            ScheduledVisitTile thisTile = lvItems.SelectedItem as ScheduledVisitTile; // .AddedItems[0] as ScheduledVisitTile;
            VisitorManager vm = new VisitorManager();
            vm.parsePerson(thisTile.person);
            this.Frame.Navigate(typeof(ScheduleVisit), vm);
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            ScheduledVisitTile thisTile = lvItems.SelectedItem as ScheduledVisitTile; 
            VisitorManager vm = new VisitorManager();
            vm.parsePerson(thisTile.person);
            this.Frame.Navigate(typeof(VisitorProcessPage1), vm);
        }

    }
}
