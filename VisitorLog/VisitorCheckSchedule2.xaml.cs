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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VisitorLog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VisitorCheckSchedule2 : Page
    {
        public VisitorCheckSchedule2()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VisitorHomePage));
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            //show them a visitor version of the scheduled visits page limiting the scheduled appointments to the current date.
            this.Frame.Navigate(typeof(VisitorFindAppointment2));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(App.appman.localSettings.Values["UsePrivacyStatement"]) == true)
            {

                this.Frame.Navigate(typeof(PrivacyStatement));
            }
            else
            {
                this.Frame.Navigate(typeof(VisitorProcessPage1));
            }
        }

       
    }
}
