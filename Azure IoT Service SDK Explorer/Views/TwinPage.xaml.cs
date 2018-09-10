using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Azure_IoT_Service_SDK_Explorer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TwinPage : Page, INotifyPropertyChanged
    {
        bool firstLaunch = true;
        RegistryManager registryManager;

        public TwinPage()
        {
            this.InitializeComponent();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            tbNew.Text = @"{";
            tbNew.Text += "\r\n 'properties': {";
            tbNew.Text += "\r\n  'desired': { }";
            tbNew.Text += "\r\n }";
            tbNew.Text += "\r\n}";

            if (!firstLaunch || App.IoTHubConnectionString == "") return;
            firstLaunch = false;

            IEnumerable<Device> devices = null;
            try
            {
                registryManager = RegistryManager.CreateFromConnectionString(App.IoTHubConnectionString);
                devices = await registryManager.GetDevicesAsync(10);
            }
            catch { }

            foreach (var device in devices)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = device;
                item.Content = device.Id;
                cbDevices.Items.Add(item);
            }

            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
            }
        }

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

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (registryManager != null)
            {
                Device device = (cbDevices.SelectedItem as ComboBoxItem).Tag as Device;
                if (device != null)
                {
                    Twin twin = await registryManager.GetTwinAsync(device.Id);
                    try
                    {
                        await registryManager.UpdateTwinAsync(device.Id, tbNew.Text, twin.ETag);

                        twin = await registryManager.GetTwinAsync(device.Id);
                        tbTwin.Text = FormatJson(twin.ToJson());
                    }
                    catch (Exception exc)
                    {
                        MessageDialog dlg = new MessageDialog(exc.ToString(), "ERROR");
                        await dlg.ShowAsync();
                    }
                }
            }
        }

        private async void cbDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (registryManager != null)
            {
                Device device = (cbDevices.SelectedItem as ComboBoxItem).Tag as Device;
                if (device != null)
                {
                    Twin twin = await registryManager.GetTwinAsync(device.Id);
                    tbTwin.Text = FormatJson(twin.ToJson());
                 }
            }
        }

        private string FormatJson(string json)
        {
            var parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
