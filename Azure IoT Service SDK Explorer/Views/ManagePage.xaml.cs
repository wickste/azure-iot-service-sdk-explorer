using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Service_SDK_Explorer.Views
{
    public sealed partial class ManagePage : Page, INotifyPropertyChanged
    {
        bool firstLaunch = true;
        RegistryManager registryManager = null;

        public ManagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!firstLaunch || App.IoTHubConnectionString == "") return;
            firstLaunch = false;

            UpdateDeviceList();
        }

        private async void UpdateDeviceList()
        {
            IEnumerable<Device> devices = null;
            try
            {
                registryManager = RegistryManager.CreateFromConnectionString(App.IoTHubConnectionString);
                devices = await registryManager.GetDevicesAsync(10);
            }
            catch { }

            foreach (var device in devices)
            {
                lvDevices.Items.Add(device);
            }

            if (lvDevices.Items.Count > 0)
            {
                lvDevices.SelectedIndex = 0;
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

        private void btnRefresh_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            lvDevices.Items.Clear();
            UpdateDeviceList();
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (registryManager != null)
            {
                Device device = lvDevices.SelectedItem as Device;
                if (device != null)
                    await registryManager.RemoveDeviceAsync(device);

                lvDevices.Items.Clear();
                UpdateDeviceList();
            }
        }

        private async void btnCreate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                Device device = new Device(tbName.Text);
                await registryManager.AddDeviceAsync(device);

                lvDevices.Items.Clear();
                UpdateDeviceList();
            }
            catch (Exception exc)
            {
                MessageDialog dlg = new MessageDialog(exc.ToString(), "ERROR");
                await dlg.ShowAsync();
            }
        }
    }
}
