using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Service_SDK_Explorer.Views
{
    public sealed partial class MonitorPage : Page, INotifyPropertyChanged
    {
        public MonitorPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            IEnumerable<Device> devices = null;
            try
            {
                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(App.IoTHubConnectionString);
                devices = await registryManager.GetDevicesAsync(10);
            }
            catch (Exception exc)
            {
                tbOutput.Text = exc.ToString();
                return;
            }

            foreach (var device in devices)
            {
                cbDevices.Items.Add(device.Id);
            }

            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
            }

            //EventHubClient eventClient = EventHubClient.CreateFromConnectionString(App.IoTHubConnectionString);
            //var info = await eventClient.GetPartitionRuntimeInformationAsync(cbDevices.SelectedItem.ToString());
            ////string partition = EventHubPartitionKeyResolver.ResolveToPartition(cbDevices.SelectedItem.ToString(), eventHubPartitionsCount);
            //var eventHubReceiver = eventClient.CreateReceiver("$Default", info.PartitionId, EventPosition.FromStart());
            //while (true)
            //{
            //    IEnumerable<EventData> eventData = await eventHubReceiver.ReceiveAsync(1);
            //    int x = 9;
            //}
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(sender as TextBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }
    }
}
