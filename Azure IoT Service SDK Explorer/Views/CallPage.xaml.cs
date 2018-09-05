using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Service_SDK_Explorer.Views
{
    public sealed partial class CallPage : Page, INotifyPropertyChanged
    {
        private bool firstLaunch = true;
        public CallPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (!firstLaunch || App.IoTHubConnectionString == "") return;
            firstLaunch = false;

            IEnumerable<Device> devices = null;
            try
            {
                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(App.IoTHubConnectionString);
                devices = await registryManager.GetDevicesAsync(10);
            }
            catch { }

            foreach (var device in devices)
            {
                cbDevices.Items.Add(device.Id);
            }

            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void btnCall_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string deviceId = cbDevices.SelectedItem.ToString();

            try
            {
                CloudToDeviceMethod method = new CloudToDeviceMethod(tb1.Text, TimeSpan.FromSeconds(60d));
                method.SetPayloadJson(tbBody.Text);
                CancellationTokenSource ctsForDeviceMethod = new CancellationTokenSource();
                await App.ServiceClient.InvokeDeviceMethodAsync(deviceId, method, ctsForDeviceMethod.Token);
            }
            catch (Exception exc)
            {
                MessageDialog dlg = new MessageDialog(exc.ToString(), "ERROR");
                await dlg.ShowAsync();
            }
        }
    }
}
