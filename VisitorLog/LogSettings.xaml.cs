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
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Diagnostics;
using System.Collections.ObjectModel;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VisitorLog
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LogSettings : Page
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


        public LogSettings()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            if (Convert.ToBoolean(App.appman.localSettings.Values["UsePrivacyStatement"]) == true)
            {
                privacyToggle.IsOn = true;
            }
            if(App.appman.localSettings.Values["Country"] != ""){
                txtCountry.Text = App.appman.localSettings.Values["Country"].ToString();
            }
            if (App.appman.localSettings.Values["City"] != "")
            {
                txtCity.Text = App.appman.localSettings.Values["City"].ToString();
            }

            if (App.appman.localSettings.Values["Sublocation"] != "")
            {
                txtSublocation.Text = App.appman.localSettings.Values["Sublocation"].ToString();
            }

            if (App.appman.localSettings.Values["Entity"] != "")
            {
               txtEntity.Text = App.appman.localSettings.Values["Entity"].ToString();
            }

            if (App.appman.localSettings.Values["PrivacyStatement"] != "")
            {
                privacyStatement.Text = App.appman.localSettings.Values["PrivacyStatement"].ToString();
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //do they save stuff
            App.appman.localSettings.Values["Country"] = txtCountry.Text;
            App.appman.localSettings.Values["City"] = txtCity.Text;
            App.appman.localSettings.Values["Sublocation"] = txtSublocation.Text;
            App.appman.localSettings.Values["Entity"] = txtEntity.Text;
            App.appman.localSettings.Values["PrivacyStatement"] = privacyStatement.Text;
            App.appman.localSettings.Values["UsePrivacyStatement"] = privacyToggle.IsOn;
            
            //redirect
            this.Frame.Navigate(typeof(AdminHomePage2));
        }

        private async void btnSave_Copy_Click(object sender, RoutedEventArgs e)
        {
            //actually the image chooser
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Debug.WriteLine ("Picked image: " + file.Name);
                IStorageFolder folder;
                Debug.WriteLine("ApplicationData.Current.LocalFolder.GetFolderAsync(backgrounds) = " + ApplicationData.Current.LocalFolder.GetFolderAsync("backgrounds"));
                bool folderExists = false;
                try
                {
                    folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("backgrounds");
                    folderExists = true;
                }
                catch(Exception xe){
                    folderExists = false;
                }

               if(!folderExists){
                   folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("backgrounds", Windows.Storage.CreationCollisionOption.ReplaceExisting);
               }
               else
               {
                   folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("backgrounds");
               }
                
                IStorageFile f =  await file.CopyAsync(folder, file.Name, NameCollisionOption.ReplaceExisting);

                Debug.WriteLine("Copied file to: " + f.Path);

                App.appman.localSettings.Values["backgroundImage"] = f.Name;
                App.appman.SetBackgroundImage();

                Debug.WriteLine("Set backgroundImage to: " + f.Name);

            }
            else
            {
                Debug.WriteLine("Cancelled.");
            }
        }


        public ObservableCollection<Country> Countries = new ObservableCollection<Country>();

        public static List<string> CountryNames = new List<string>
            {
	        "Afghanistan",
	        "Albania",
	        "Algeria",
	        "American Samoa",
	        "Andorra",
	        "Angola",
	        "Anguilla",
	        "Antarctica",
	        "Antigua and Barbuda",
	        "Argentina",
	        "Armenia",
	        "Aruba",
	        "Australia",
	        "Austria",
	        "Azerbaijan",
	        "Bahamas",
	        "Bahrain",
	        "Bangladesh",
	        "Barbados",
	        "Belarus",
	        "Belgium",
	        "Belize",
	        "Benin",
	        "Bermuda",
	        "Bhutan",
	        "Bolivia",
	        "Bosnia and Herzegovina",
	        "Botswana",
	        "Bouvet Island",
	        "Brazil",
	        "British Indian Ocean Territory",
	        "Brunei Darussalam",
	        "Bulgaria",
	        "Burkina Faso",
	        "Burundi",
	        "Cambodia",
	        "Cameroon",
	        "Canada",
	        "Cape Verde",
	        "Cayman Islands",
	        "Central African Republic",
	        "Chad",
	        "Chile",
	        "China",
	        "Christmas Island",
	        "Cocos (Keeling) Islands",
	        "Colombia",
	        "Comoros",
	        "Congo",
	        "Congo, the Democratic Republic of the",
	        "Cook Islands",
	        "Costa Rica",
	        "Cote D'Ivoire",
	        "Croatia",
	        "Cuba",
	        "Cyprus",
	        "Czech Republic",
	        "Denmark",
	        "Djibouti",
	        "Dominica",
	        "Dominican Republic",
	        "Ecuador",
	        "Egypt",
	        "El Salvador",
	        "Equatorial Guinea",
	        "Eritrea",
	        "Estonia",
	        "Ethiopia",
	        "Falkland Islands (Malvinas)",
	        "Faroe Islands",
	        "Fiji",
	        "Finland",
	        "France",
	        "French Guiana",
	        "French Polynesia",
	        "French Southern Territories",
	        "Gabon",
	        "Gambia",
	        "Georgia",
	        "Germany",
	        "Ghana",
	        "Gibraltar",
	        "Greece",
	        "Greenland",
	        "Grenada",
	        "Guadeloupe",
	        "Guam",
	        "Guatemala",
	        "Guinea",
	        "Guinea-Bissau",
	        "Guyana",
	        "Haiti",
	        "Heard Island and Mcdonald Islands",
	        "Holy See (Vatican City State)",
	        "Honduras",
	        "Hong Kong",
	        "Hungary",
	        "Iceland",
	        "India",
	        "Indonesia",
	        "Iran, Islamic Republic of",
	        "Iraq",
	        "Ireland",
	        "Israel",
	        "Italy",
	        "Jamaica",
	        "Japan",
	        "Jordan",
	        "Kazakhstan",
	        "Kenya",
	        "Kiribati",
	        "Korea, Democratic People's Republic of",
	        "Korea, Republic of",
	        "Kuwait",
	        "Kyrgyzstan",
	        "Lao People's Democratic Republic",
	        "Latvia",
	        "Lebanon",
	        "Lesotho",
	        "Liberia",
	        "Libyan Arab Jamahiriya",
	        "Liechtenstein",
	        "Lithuania",
	        "Luxembourg",
	        "Macao",
	        "Macedonia, the Former Yugoslav Republic of",
	        "Madagascar",
	        "Malawi",
	        "Malaysia",
	        "Maldives",
	        "Mali",
	        "Malta",
	        "Marshall Islands",
	        "Martinique",
	        "Mauritania",
	        "Mauritius",
	        "Mayotte",
	        "Mexico",
	        "Micronesia, Federated States of",
	        "Moldova, Republic of",
	        "Monaco",
	        "Mongolia",
	        "Montserrat",
	        "Morocco",
	        "Mozambique",
	        "Myanmar",
	        "Namibia",
	        "Nauru",
	        "Nepal",
	        "Netherlands",
	        "Netherlands Antilles",
	        "New Caledonia",
	        "New Zealand",
	        "Nicaragua",
	        "Niger",
	        "Nigeria",
	        "Niue",
	        "Norfolk Island",
	        "Northern Mariana Islands",
	        "Norway",
	        "Oman",
	        "Pakistan",
	        "Palau",
	        "Palestinian Territory, Occupied",
	        "Panama",
	        "Papua New Guinea",
	        "Paraguay",
	        "Peru",
	        "Philippines",
	        "Pitcairn",
	        "Poland",
	        "Portugal",
	        "Puerto Rico",
	        "Qatar",
	        "Reunion",
	        "Romania",
	        "Russian Federation",
	        "Rwanda",
	        "Saint Helena",
	        "Saint Kitts and Nevis",
	        "Saint Lucia",
	        "Saint Pierre and Miquelon",
	        "Saint Vincent and the Grenadines",
	        "Samoa",
	        "San Marino",
	        "Sao Tome and Principe",
	        "Saudi Arabia",
	        "Senegal",
	        "Serbia and Montenegro",
	        "Seychelles",
	        "Sierra Leone",
	        "Singapore",
	        "Slovakia",
	        "Slovenia",
	        "Solomon Islands",
	        "Somalia",
	        "South Africa",
	        "South Georgia and the South Sandwich Islands",
	        "Spain",
	        "Sri Lanka",
	        "Sudan",
	        "Suriname",
	        "Svalbard and Jan Mayen",
	        "Swaziland",
	        "Sweden",
	        "Switzerland",
	        "Syrian Arab Republic",
	        "Taiwan, Province of China",
	        "Tajikistan",
	        "Tanzania, United Republic of",
	        "Thailand",
	        "Timor-Leste",
	        "Togo",
	        "Tokelau",
	        "Tonga",
	        "Trinidad and Tobago",
	        "Tunisia",
	        "Turkey",
	        "Turkmenistan",
	        "Turks and Caicos Islands",
	        "Tuvalu",
	        "Uganda",
	        "Ukraine",
	        "United Arab Emirates",
	        "United Kingdom",
	        "United States",
	        "United States Minor Outlying Islands",
	        "Uruguay",
	        "Uzbekistan",
	        "Vanuatu",
	        "Venezuela",
	        "Viet Nam",
	        "Virgin Islands, British",
	        "Virgin Islands, US",
	        "Wallis and Futuna",
	        "Western Sahara",
	        "Yemen",
	        "Zambia",
	        "Zimbabwe",
            };
        
        
    }

    public class Country {
        public Country()
        {
            
        }

        public void setName(string nName) {
            Name = nName;
        }
        public string Name { get; set; }

    }
}
