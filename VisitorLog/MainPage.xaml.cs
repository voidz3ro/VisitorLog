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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VisitorLog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string err = "";
            if(txtUsername.Text.Length == 0 || txtPass.Password.Length == 0){
                err += "Username and password are required";
            }else{
                if(App.appman.checkAdmin(txtUsername.Text, txtPass.Password)){
                    this.Frame.Navigate(typeof(AdminHomePage2));
                }else{
                    err += "Username and password incorrect";                   
                }
            }

            if(err!=""){
                MessageDialog msgbox = new MessageDialog(err, "Problems");

                    msgbox.Commands.Clear();
                  
                    msgbox.Commands.Add(new UICommand { Label = "Ok", Id = 0});

                    var res = await msgbox.ShowAsync(); 
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VisitorHomePage));
        }
    }
}
