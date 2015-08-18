using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Windows.UI;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Data.Xml;
using Windows.Data.Xml.Dom;

namespace VisitorLog
{
    public class AppManager
    {
       public  Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
       public  Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

       public async Task SetBackgroundImage(){
           StorageFile file = null;
           string filPath = localFolder.Path + "\\backgrounds\\" + localSettings.Values["backgroundImage"].ToString();
           //Debug.WriteLine(filPath);
           
           
            if (localSettings.Values["backgroundImage"].ToString() != "default.jpg")
           {
               file = await StorageFile.GetFileFromPathAsync(filPath);
           }
           

           Frame rootFrame = Window.Current.Content as Frame;

           if (file != null)
           {
               Debug.WriteLine("thar be a bg img!");
               BitmapImage bitmapImage = new BitmapImage();
               FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
               bitmapImage.SetSource(stream);
               Image image = new Image();
               image.Source = bitmapImage;

               rootFrame.Background = new ImageBrush
               {
                   Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill,
                   ImageSource = bitmapImage
               };

           }
           else
           {
               
                // Set the application background Image
               rootFrame.Background = new ImageBrush
               {
                   Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill,
                   ImageSource =
                   new BitmapImage { UriSource = new Uri("ms-appx:///Assets/default.jpg") }
               };
           }
           //set a transparent layer under the ui controls??
           //Windows.UI.Xaml.Shapes.Rectangle rect = new Windows.UI.Xaml.Shapes.Rectangle();
           //rect.Width = rootFrame.Width;
           //rect.Height = (rootFrame.Height - 200);
           //rect.Fill = new SolidColorBrush(Color.FromArgb(0, 19, 19, 19));
           //rootFrame.Foreground = new SolidColorBrush(Color.FromArgb(0, 19, 19, 19));
           
           
       }

       public DateTime GetLastTransmitDate()
       {
           if (localSettings.Values["Last-Transmit-Date"] == null)
           { 
            return DateTime.Parse("1901-01-01");
       } else{
           return  DateTime.Parse(localSettings.Values["Last-Transmit-Date"].ToString());
            }
           
       }

       public string GetCountry()
       {
           return localSettings.Values["Country"].ToString();
       }

       public string GetCity()
       {
           return localSettings.Values["City"].ToString();
       }

       public string GetEntity()
       {
           return localSettings.Values["Entity"].ToString();
       }

       public string GetSublocation()
       {
           return localSettings.Values["Sublocation"].ToString();
       }

       public string GetEntrance()
       {
           return localSettings.Values["Description"].ToString();
       }

       public string GetCurrentUser()
       {
           return localSettings.Values["Username"].ToString();
       }

       public string GetAdminInitials()
       {
           return localSettings.Values["Username"].ToString();
       }


       public async Task checkFileSystem()
       {
           StorageFile file;
           bool there = true;
           try
           {
               file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
           }
           catch (FileNotFoundException)
           {
               Debug.WriteLine("No file found");
               there = false;
           }
           if (there == false)
           {
               //Debug.WriteLine("Creating File");
               XmlDocument doc = new XmlDocument();
               XmlElement root = doc.CreateElement("LOG");
               doc.AppendChild(root);
               file = await ApplicationData.Current.LocalFolder.CreateFileAsync("logdata.xml");
               await FileIO.WriteTextAsync(file, doc.GetXml());
               Debug.WriteLine("Done creating file.");
               //await doc.SaveToFileAsync(st);
           }
       }

       public AppManager()
        {
            //Debug.WriteLine("appman init");
            //comment for prod
          // localSettings.Values["FirstTime"] = null;

            if (localSettings.Values["FirstTime"] == null)
            {
                //Debug.WriteLine("Deteceted first time use");
                localSettings.Values["AdminUserName"] = "admin";
                localSettings.Values["AdminPassword"] = "boeing123";
                localSettings.Values["FirstTime"] = true;
                localSettings.Values["Username"] = null;
                localSettings.Values["Password"] = null;
                localSettings.Values["Country"] = "";
                localSettings.Values["City"] = "";
                localSettings.Values["Sublocation"] = "";
                localSettings.Values["Location"] = "";
                localSettings.Values["Entrance"] = "";
                localSettings.Values["Entity"] = "";
                localSettings.Values["UsePrivacyStatement"] = false;
                localSettings.Values["Last-Transmit-Date"] = DateTime.Now.ToString();
                localSettings.Values["PrivacyStatement"] = "The Boeing Company and its affiliates (collectively, “Boeing”) recognize and respect the importance of your privacy.  The sign-in Information being requested will be used to manage your visit to this Boeing facility." +

                " If not previously accomplished, prior to your entry into this Boeing facility we are required by law to enter your information (nationality and company/organization represented) into the Denied Party Screening process. In this process, Boeing reviews a number of lists published by national and multi-national authorities to ensure that Boeing legally can enter into a business transaction with you and any company or organization that you represent.  The information requested will be used for the sole purpose of the aforementioned required screening. This information will be retained locally at this facility and potentially on a Boeing server located in the United States in accordance with the applicable laws and regulations. This information is retained for 90 days or longer, if required by law.  It will not be shared with any third party unless also required by law.  If you or your company’s/organization’s name appears on any published list your entry into the Boeing facility will be postponed or denied until such time that your information can be verified and you are cleared for entry."+

                " You may refuse to provide the information requested.  In that case, Boeing will be unable to legally enter into any business transaction with you, and may deny your entry into the Boeing facility. If you have questions about your privacy, please contact your Boeing sponsor.";
                localSettings.Values["backgroundImage"] = "default.jpg";
                localSettings.Values["fileDump"] = "\\\\d1sd-prd-app04\\VisitorLogData";
                checkFileSystem();
                launchTimer();
            }
          
        }

       private DispatcherTimer timer = new DispatcherTimer();
       

       private void launchTimer()
       {
           timer.Interval = new TimeSpan(0, 0, 5);
           timer.Start();
           timer.Tick += timer_Tick;
       }

        private async Task sendFileData(){
            Debug.WriteLine("Sending data...");
            try {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("logdata.xml");
                //file.Name = localSettings.Values["Location"].ToString() + "_" + localSettings.Values["Description"].ToString() + "_dump_" + DateTime.Now.ToString();
                var folder  = await StorageFolder.GetFolderFromPathAsync(@"\\Mediacenter\storage\TRANSFER\");
                Debug.WriteLine(folder.Path);
                await file.CopyAsync(folder);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error sending file:" + e.InnerException);
                Debug.WriteLine("Error sending file:" + e.Message);
            }
        }

        async void timer_Tick(object sender, object e)
        {
          // await  sendFileData();

        }

     

        public bool checkAdmin(string user, string passw)
        {
            if (user == Convert.ToString(localSettings.Values["AdminUserName"]) && passw == Convert.ToString(localSettings.Values["AdminPassword"]))
            {
                return true;
            }else if (user == Convert.ToString(localSettings.Values["Username"]) && passw == Convert.ToString(localSettings.Values["Password"])){
                return true;
            }else{
                return false;
            }

        }
    }
}
