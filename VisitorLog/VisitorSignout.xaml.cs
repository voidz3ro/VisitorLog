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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VisitorLog
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class VisitorSignout : Page
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


        public VisitorSignout()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            loadSignIns();
        }


        private async Task loadSignIns()
        {
            Debug.WriteLine("loading sign ins.");
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
            Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
            XDocument doc = XDocument.Load(file.Path);
            var ps = (
                    from actions in doc.Root.Elements("Action").Elements("Person")
                    select actions
                );

            var normalized = ps.Select(x => new
            {
                FullName = (string)x.Element("First-Name") + " " + (string)x.Element("Last-Name"),
                id = x.Descendants("ID").Max(y => (int)y)
            });

            var maxids = (from t in normalized
                          group t by t.FullName
                              into g
                              select new
                              {
                                  //FullName = g.Key,
                                  ID = (from t2 in g select t2.id).Max()
                              }
                            );



            List<string> mylist = new List<string>();
            foreach (var i in maxids)
            {
                mylist.Add(i.ID.ToString());
            }



            IEnumerable<XElement> persons = (
                    from actions in doc.Root.Elements("Action")
                    where (string)actions.Attribute("type") == "Sign In"
                    from person in actions.Elements("Person")
                    where mylist.Contains(person.Element("ID").Value)
                    from dates in person.Elements("Date")
                    where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date //just signins for today - uncertain about this..
                    select person
                );

            Debug.WriteLine(persons.Count());
            Debug.WriteLine(persons);

            //for (var i = 0; i <= 25; i++)
            //{
            //    VisitorTile tile = new VisitorTile();
            //    tile.txtName.Text = "TEst";
            //    tile.txtCompany.Text = "Bobs Butrgers";
            //    tile.txtTimeIn.Text = DateTime.Now.TimeOfDay.ToString();
            //    lvSignIns.Items.Add(tile);

            //}


                foreach (XElement signin in persons)
                {
                    Debug.WriteLine("Adding Tile.");
                    VisitorTile tile = new VisitorTile();
                    tile.person = signin;
                    tile.txtName.Text = signin.Element("Last-Name").Value.ToString() + ", " + signin.Element("First-Name").Value.ToString();
                    tile.txtCompany.Text = signin.Element("Company-Represented").Value.ToString();

                    var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate");
                    var formatterTime = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime");
                    DateTime dateToFormat = Convert.ToDateTime(signin.Element("Time-In").Value.ToString());

                    var thedate = formatter.Format(dateToFormat);
                    var thetime = formatterTime.Format(dateToFormat);

                    tile.txtTimeIn.Text = thedate + " " + thetime; 
                    
                    lvSignIns.Items.Add(tile);
                }

            if(persons.Count() == 0){
                VisitorTile tile = new VisitorTile();

                tile.txtName.Text = "No current visitors";
                tile.txtCompany.Text = "";
                tile.txtTimeIn.Text = "";
                lvSignIns.Items.Add(tile);
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

        private void lvSignIns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisitorTile thisTile =  e.AddedItems[0] as VisitorTile;
          //  Debug.WriteLine("clicked on " + thisTile.person.Element("First-Name").Value.ToString());
           
            //Debug.WriteLine(thisTile.person);
            if(thisTile.person != null)
            {
                this.Frame.Navigate(typeof(VisitorSignOutConfirmation), thisTile.person);
            }
          
           
         //  Debug.WriteLine("wtf");

        }
    }
}
