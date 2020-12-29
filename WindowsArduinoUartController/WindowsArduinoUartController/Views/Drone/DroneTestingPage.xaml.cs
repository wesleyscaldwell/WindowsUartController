using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WindowsArduinoUartController.Views.Drone
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DroneTestingPage : ContentPage
    {

        WindowsArduinoUartController.Interfaces.IUartService uart;
        Timer timer = null;
        public DroneTestingPage()
        {
            InitializeComponent();

            try
            {
                uart = DependencyService.Get<WindowsArduinoUartController.Interfaces.IUartService>();
            }
            catch (Exception ex)
            {
                this.statusLabel.Text = $@"Error Loading: {ex.Message}";
            }

            try
            {
                //uart.Serial("UART1", 115200);
                uart.Serial("UART1", 115200);
            }
            catch (Exception ex) { this.statusLabel.Text = $@"Error Set Serial: {ex.Message}"; }

            try
            {
                timer = new Timer() { Interval = 1000, Enabled = true };
                timer.Start();
                timer.Elapsed += async (object sender, System.Timers.ElapsedEventArgs e) =>
                {
                    timer.Stop();
                    try
                    {
                        var response = await uart.ReadSerialAsync();
                        //Device.BeginInvokeOnMainThread(() =>
                        //{
                        //    LastScanned.Text = response;
                        //});
                    }
                    catch (Exception ex) { statusLabel.Text = $@"Error4: {ex.Message}"; }
                    finally { timer.Start(); }
                };
            }
            catch (Exception ex) { this.statusLabel.Text = $@"Error Set Timer: {ex.Message}"; }

            try
            {
                Task task = new Task(async () =>
                {
                    try 
                    {
                        await Task.Delay(5000);
                        await uart.SendStringToConnectedUart(new List<string>() { "MV23I0000" });
                        //await uart.SendStringToConnectedUart(new List<string>() { "Testing Message" });
                        //await Task.Delay(5000);
                        //await uart.SendStringToConnectedUart(new List<string>() { "MV05S5512;MV07S89556;MV08S89556" });
                        //await Task.Delay(5000);
                        //await uart.SendStringToConnectedUart(new List<string>() { "MV12S89556" });
                        //await Task.Delay(5000);
                        //await uart.SendStringToConnectedUart(new List<string>() { "MV13S89556;" });
                        //await Task.Delay(5000);
                        //await uart.SendStringToConnectedUart(new List<string>() { ";MV14S89556" });
                    }
                    catch (Exception ex) { this.statusLabel.Text = $@"Error Processing Wait: {ex.Message}"; }
                });
                task.Start();
            }
            catch (Exception ex) { this.statusLabel.Text = $@"Error Sending request: {ex.Message}"; }

            this.statusLabel.Text = "Loaded";
        }

        private async void motor1potvalue_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                double potValue = e.NewValue;
                string sendStringValue = $@"MV23S{potValue.ToString()}";
                System.Diagnostics.Debug.WriteLine(sendStringValue);
                await uart.SendStringToConnectedUart(new List<string>() { sendStringValue });
            }
            catch (Exception ex) { this.statusLabel.Text = $@"Sending motor Value: {ex.Message}"; }
        }

        private async void motor1potvalue_DragCompleted(object sender, EventArgs e)
        {
            double potValue = this.motor1potvalue.Value;
            string sendStringValue = $@"MV23S{potValue.ToString()}";
            System.Diagnostics.Debug.WriteLine(sendStringValue);
            //await uart.SendStringToConnectedUart(new List<string>() { sendStringValue });
        }
    }
}