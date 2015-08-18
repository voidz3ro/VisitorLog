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
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VisitorLog
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ViewData : Page
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


        public ViewData()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            startDate.Date = DateTime.Now.AddMonths(-3);
            endDate.Date = DateTime.Now.AddDays(1);
            loadLog();
        }


        public async Task loadLog()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
            Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
            XDocument doc = XDocument.Load(file.Path);
            var ps = (
                    from actions in doc.Root.Elements("Action").Elements("Person")
                    select actions
                );

            IEnumerable<XElement> nactions = (
                  from actionsa in doc.Root.Elements("Action")
                 // where (string)actions.Attribute("type") == "Sign In"
                  from person in actionsa.Elements("Person")
                  from dates in person.Elements("Date")
                  orderby (Convert.ToDateTime(person.Element("Date").Value)) descending
                  select actionsa
              );

            foreach (XElement action in nactions)
            {
                LogRecordTile tile = new LogRecordTile();
                tile.txtName.Text = action.Element("Person").Element("First-Name").Value + " " + action.Element("Person").Element("Last-Name").Value;
                tile.txtCompany.Text = action.Element("Person").Element("Company-Represented").Value;
                tile.txtHost.Text = action.Element("Person").Element("Boeing-Host-Name").Value;
                tile.txtAction.Text = action.Attribute("type").Value;
                var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                DateTime dateToFormat = Convert.ToDateTime(action.Element("Person").Element("Time-In").Value.ToString());
                var thedate = formatter.Format(dateToFormat);
                var thetime = formatterTime.Format(dateToFormat);
                tile.txtDate.Text = thedate + " " + thetime; 
                theGrid.Items.Add(tile);
            }
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            DateTimeOffset sd = startDate.Date;
            DateTimeOffset ed = endDate.Date;
            string searchStr = txtSearchString.Text;

            Debug.WriteLine("Applying Filter...");
         

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
            Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
            XDocument doc = XDocument.Load(file.Path);
            var ps = (
                    from nodes in doc.Root.Elements("Action").Elements("Person")
                    select nodes
                );

            IEnumerable<XElement> actions = (
                  from nodes in doc.Root.Elements("Action")
                  from person in nodes.Elements("Person")
                  from dates in person.Elements("Date")
                  where (Convert.ToDateTime(person.Element("Time-In").Value) < ed.ToUniversalTime())
                  where (Convert.ToDateTime(person.Element("Time-In").Value) > sd.ToUniversalTime())
                  orderby (Convert.ToDateTime(person.Element("Date").Value)) descending
                  select nodes
              );

            Debug.WriteLine("found " + actions.Count() + "matching records.");

            theGrid.Items.Clear();

            foreach (XElement action in actions)
            {

                if (txtSearchString.Text != "" || txtCompany.Text != "" || txtHost.Text != "")
                {
                    if (txtSearchString.Text != "")
                    {
                        string filter = action.Element("Person").Element("First-Name").Value + " " + action.Element("Person").Element("Last-Name").Value;
                        if (filter.ToUpper().Contains(txtSearchString.Text.ToUpper()))
                        {
                            LogRecordTile tile = new LogRecordTile();
                            tile.txtAction.Text = action.Attribute("type").Value;
                            tile.txtName.Text = action.Element("Person").Element("First-Name").Value + " " + action.Element("Person").Element("Last-Name").Value;
                            tile.txtCompany.Text = action.Element("Person").Element("Company-Represented").Value;
                            tile.txtHost.Text = action.Element("Person").Element("Boeing-Host-Name").Value;
                            var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                            var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                            DateTime dateToFormat = Convert.ToDateTime(action.Element("Person").Element("Time-In").Value.ToString());
                            var thedate = formatter.Format(dateToFormat);
                            var thetime = formatterTime.Format(dateToFormat);
                            tile.txtDate.Text = thedate + " " + thetime;
                            theGrid.Items.Add(tile);
                        }
                    }

                    if(txtCompany.Text != ""){
                        string filter = action.Element("Person").Element("Company-Represented").Value;
                        if (filter.ToUpper().Contains(txtCompany.Text.ToUpper()))
                        {
                            LogRecordTile tile = new LogRecordTile();
                            tile.txtAction.Text = action.Attribute("type").Value;
                            tile.txtName.Text = action.Element("Person").Element("First-Name").Value + " " + action.Element("Person").Element("Last-Name").Value;
                            tile.txtCompany.Text = action.Element("Person").Element("Company-Represented").Value;
                            tile.txtHost.Text = action.Element("Person").Element("Boeing-Host-Name").Value;
                            var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                            var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                            DateTime dateToFormat = Convert.ToDateTime(action.Element("Person").Element("Time-In").Value.ToString());
                            var thedate = formatter.Format(dateToFormat);
                            var thetime = formatterTime.Format(dateToFormat);
                            tile.txtDate.Text = thedate + " " + thetime;
                            theGrid.Items.Add(tile);
                        }
                    }

                    if (txtHost.Text != "")
                    {
                        string filter = action.Element("Person").Element("Boeing-Host-Name").Value;
                        if (filter.ToUpper().Contains(txtHost.Text.ToUpper()))
                        {
                            LogRecordTile tile = new LogRecordTile();
                            tile.txtAction.Text = action.Attribute("type").Value;
                            tile.txtName.Text = action.Element("Person").Element("First-Name").Value + " " + action.Element("Person").Element("Last-Name").Value;
                            tile.txtCompany.Text = action.Element("Person").Element("Company-Represented").Value;
                            tile.txtHost.Text = action.Element("Person").Element("Boeing-Host-Name").Value;
                            var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                            var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                            DateTime dateToFormat = Convert.ToDateTime(action.Element("Person").Element("Time-In").Value.ToString());
                            var thedate = formatter.Format(dateToFormat);
                            var thetime = formatterTime.Format(dateToFormat);
                            tile.txtDate.Text = thedate + " " + thetime;
                            theGrid.Items.Add(tile);
                        }
                    }

                }
                else
                {
                    LogRecordTile tile = new LogRecordTile();
                    tile.txtAction.Text = action.Attribute("type").Value;
                    tile.txtName.Text = action.Element("Person").Element("First-Name").Value + " " + action.Element("Person").Element("Last-Name").Value;
                    tile.txtCompany.Text = action.Element("Person").Element("Company-Represented").Value;
                    tile.txtHost.Text = action.Element("Person").Element("Boeing-Host-Name").Value;
                    var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                    var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                    DateTime dateToFormat = Convert.ToDateTime(action.Element("Person").Element("Time-In").Value.ToString());
                    var thedate = formatter.Format(dateToFormat);
                    var thetime = formatterTime.Format(dateToFormat);
                    tile.txtDate.Text = thedate + " " + thetime;
                    theGrid.Items.Add(tile);

                }

            }

        }

        private async void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
           // Debug.WriteLine("Saving File");
            FileSavePicker fsp = new FileSavePicker();
            fsp.SuggestedFileName = "VisitorLogExport_" + DateTime.Now + ".xml";
            fsp.FileTypeChoices.Add("XML", new List<string>() { ".xml" });
            fsp.FileTypeChoices.Add("Excel", new List<string>() { ".xls",".xlsx" });
            fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var file = await fsp.PickSaveFileAsync();
            StorageFile curFile = await StorageFile.GetFileFromPathAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\logdata.xml");
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

              //  await curFile.CopyAndReplaceAsync(file);

                Debug.WriteLine("got the file, querying...");

               //this now needs to be customized because dubai can't use excel.
                XDocument logDoc = XDocument.Load(curFile.Path);
                IEnumerable<XElement> actions = logDoc.Root.Elements("Action");
                Debug.WriteLine(actions.Count() + "orignal actions.");
                //create a new XDocument
                XElement newDoc = new XElement("root");

                var querySignIns = from nodes in actions
                                   where nodes.Attribute("type").Value == "Sign In"
                                   select nodes;

                var querySignOuts = from nodes in actions
                                    where nodes.Attribute("type").Value == "Sign Out"
                                    select nodes;

                foreach (XElement action in querySignIns)
                {
                     XElement nAction = action.Element("Person");
                     string thisFirstName = nAction.Element("First-Name").Value;
                     string thisLastName = nAction.Element("Last-Name").Value;

                    // Debug.WriteLine("Finding signout for " + thisFirstName + " " + thisLastName);

                     var qrySignOut = (from nodes in querySignOuts
                                       where nodes.Element("Person").Element("First-Name").Value == nAction.Element("First-Name").Value
                                       where nodes.Element("Person").Element("Last-Name").Value == nAction.Element("Last-Name").Value
                                       select nodes).Take(1);

                     string signOutTime = "";
                     foreach (XElement SignOutAction in qrySignOut)
                     {
                       DateTime nsoDate = DateTime.Parse(SignOutAction.Element("Person").Element("Date").Value);
                       signOutTime = nsoDate.ToLocalTime().ToString();
                     }
                   //create new element.
                   
                    DateTime originalDate = DateTime.Parse(nAction.Element("Date").Value);
                    string signInDate = originalDate.Month.ToString() + "-" + originalDate.Day.ToString() +"-" + originalDate.Year.ToString();
                    string signInTime = originalDate.ToLocalTime().ToString();


                    string ncountry = nAction.Element("Country") == null ? "N/A" : nAction.Element("Country").Value;
                    string ncity = nAction.Element("City") == null ? "N/A" : nAction.Element("City").Value;
                    string nsublocation = nAction.Element("Sublocation") == null ? "N/A" : nAction.Element("Sublocation").Value;


                    XElement visit = new XElement("Visit",

                    new XElement("First-Name", nAction.Element("First-Name").Value),
                    new XElement("Last-Name", nAction.Element("Last-Name").Value),
                    new XElement("Company-Represented", nAction.Element("Company-Represented").Value),
                    new XElement("Country", ncountry),
                    new XElement("City", ncity),
                    new XElement("Sublocation", nsublocation),
                    new XElement("Location", nAction.Element("Location").Value),
                    new XElement("Entrance", nAction.Element("Entrance").Value),
                   /* new XElement("sign-in-date", signInDate),                                  ///this is sign-in date
                    new XElement("sign-out-date", ""),*/
                    new XElement("Badge-Number", nAction.Element("Badge-Number").Value),
                    new XElement("Boeing-Employee", nAction.Element("Boeing-Employee").Value),
                    new XElement("BEMSID", nAction.Element("BEMSID").Value),
                    new XElement("Boeing-Host-Name", nAction.Element("Boeing-Host-Name").Value),
                    new XElement("Time-In", signInTime),
                    new XElement("Time-Out", signOutTime),
                    new XElement("Admin-Initials-Out", nAction.Element("Admin-Initials-Out").Value),
                    new XElement("Pre-Screened", nAction.Element("Pre-Screened").Value),
                    new XElement("Pre-Screened-by", nAction.Element("Pre-Screened-by").Value),
                    new XElement("EnteredByAdmin", nAction.Element("EnteredByAdmin").Value),
                    new XElement("Agree-To-Privacy-Statement", true),
                    new XElement("EnteredByAdminDate", nAction.Element("EnteredByAdminDate").Value)
                    );
                    newDoc.Add(visit);
                }



                await FileIO.WriteTextAsync(file, newDoc.ToString());

               
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    //Debug.WriteLine("File " + file.Name + " was saved.");
                    MessageDialog msgbox = new MessageDialog("File " + file.Name + " was saved.", "");
                    msgbox.Commands.Clear();
                    msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                    var res = await msgbox.ShowAsync(); 
                }
                else
                {
                    //Debug.WriteLine("File " + file.Name + " couldn't be saved.");
                    MessageDialog msgbox = new MessageDialog("File " + file.Name + " couldn't be saved.", "");
                    msgbox.Commands.Clear();
                    msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
                    var res = await msgbox.ShowAsync(); 
                }
            }
        }

        private async void btnSaveAs_Copy_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgbox = new MessageDialog("This will erase all log data. You should use 'save as' to make a copy of the log data if you would like to keep the data.", "Clear Log:");
            msgbox.Commands.Clear();
            msgbox.Commands.Add(new UICommand("Clear Log Data", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgbox.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgbox.CancelCommandIndex = 1;
            var res = await msgbox.ShowAsync();
        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            //Debug.WriteLine("The '" + command.Label + "' command has been selected.");
            if(command.Label == "Clear Log Data"){
                StorageFile dataFile = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
                XDocument doc = XDocument.Load(dataFile.Path);
                doc.Root.Elements().Remove();
                await FileIO.WriteTextAsync(dataFile, doc.Root.ToString());
                theGrid.Items.Clear();
            }
        }

        private async void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            StorageFile dataFile = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
            FileSavePicker fsp = new FileSavePicker();
            fsp.SuggestedFileName = "VisitorLogDataBackup_" + DateTime.Now + ".xml";
            fsp.FileTypeChoices.Add("XML", new List<string>() { ".xml" });
            fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var file = await fsp.PickSaveFileAsync();
            Debug.WriteLine("got file " + dataFile.Path + ". Attempting copy");
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                await dataFile.CopyAndReplaceAsync(file);
            }

            MessageDialog msgbox = new MessageDialog("The log data has been saved to " + file.Path + ". It's now safe to clear the log or update the app.", "Data saved!");
            msgbox.Commands.Clear();
            msgbox.Commands.Add(new UICommand("OK, got it", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgbox.CancelCommandIndex = 0;
            await msgbox.ShowAsync(); 
        }

        private async void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fop = new FileOpenPicker();
            
            fop.FileTypeFilter.Add(".xml");
            fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            var file = await fop.PickSingleFileAsync();
            StorageFile dataFile = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                await file.CopyAndReplaceAsync(dataFile);
            }

            MessageDialog msgbox = new MessageDialog("The log data has been restored from backup.", "Data restored!");
            msgbox.Commands.Clear();
            msgbox.Commands.Add(new UICommand("OK, got it", new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msgbox.CancelCommandIndex = 0;
            await msgbox.ShowAsync();   

        }
    }
}
