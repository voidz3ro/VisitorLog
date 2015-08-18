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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VisitorLog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VisitorFindVisit : Page
    {
        public VisitorFindVisit()
        {
            this.InitializeComponent();
        }

        async void txtName_KeyUp(object sender, KeyRoutedEventArgs e)
        {

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");

            lvItems.Items.Clear();

            //   Debug.WriteLine("Attempting to read file  " + file.Path + " into xdocument");
            XDocument doc = XDocument.Load(file.Path);
            var ps = (
                from actions in doc.Root.Elements("Action").Elements("Person")
                select actions
            );
            if(txtName.Text.Count() > 0 ) {
                    IEnumerable<XElement> persons = (
                        from actions in doc.Root.Elements("Action")
                        where (string)actions.Attribute("type") == "Scheduled Visit"
                        from person in actions.Elements("Person")
                        where (person.Element("First-Name").Value.ToUpper().Contains(txtName.Text.ToUpper()) || person.Element("Last-Name").Value.ToUpper().Contains(txtName.Text.ToUpper()))

                        from dates in person.Elements("Date")
                        orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                         where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
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
                }
            }
            else
            {
                TopAppBar.IsOpen = false;
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

            if(txtName_Copy.Text.Count() > 0) {

                IEnumerable<XElement> persons = (
                    from actions in doc.Root.Elements("Action")
                    where (string)actions.Attribute("type") == "Scheduled Visit"
                    from person in actions.Elements("Person")
                    where (person.Element("Company-Represented").Value.ToUpper().Contains(txtName_Copy.Text.ToUpper()))
                    from dates in person.Elements("Date")
                    orderby DateTime.Parse(person.Element("Time-In").Value) ascending
                    where Convert.ToDateTime(dates.Value).Date == DateTime.Now.Date
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

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
           
            ScheduledVisitTile thisTile = lvItems.SelectedItem as ScheduledVisitTile;
           if(thisTile!=null){
                    VisitorManager vm = new VisitorManager();
                    vm.parsePerson(thisTile.person);
                    this.Frame.Navigate(typeof(VisitorProcessPage1), vm);
               }
        }
    }
}
