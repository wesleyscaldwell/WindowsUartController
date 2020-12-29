using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WindowsArduinoUartController
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new MainPage();
            MainPage = new Views.Drone.DroneTestingPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
