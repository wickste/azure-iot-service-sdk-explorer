using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common;
using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Service_SDK_Explorer.Views
{
    public sealed partial class MonitorPage : Page, INotifyPropertyChanged
    {
        private EventHubClient s_eventHubClient = null;
        private bool firstLaunch = true;

        public MonitorPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!firstLaunch || App.IoTHubConnectionString == "") return;
            firstLaunch = false;

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("eventHubName"))
            {
                tb1.Text = ApplicationData.Current.LocalSettings.Values["eventHubName"].ToString();
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("eventHubEndpoint"))
            {
                tb2.Text = ApplicationData.Current.LocalSettings.Values["eventHubEndpoint"].ToString();
            }

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

        private async void btnUpdate(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IotHubConnectionStringBuilder builder = IotHubConnectionStringBuilder.Create(App.IoTHubConnectionString);
            string[] segments = tb2.Text.Split(';');
            string uri = segments[0].Substring(segments[0].IndexOf("sb:/"));
            var connectionString = new EventHubsConnectionStringBuilder(new Uri(uri), tb1.Text, "iothubowner", builder.SharedAccessKey);
            s_eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());
            ApplicationData.Current.LocalSettings.Values["eventHubName"] = tb1.Text;
            ApplicationData.Current.LocalSettings.Values["eventHubEndpoint"] = tb2.Text;

            // Create a PartitionReciever for each partition on the hub.
            var runtimeInfo = await s_eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            foreach (string partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition, cts.Token);
            }
        }

        private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = s_eventHubClient.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            tbOutput.Text += "\r\nListening for events on partition: " + partition;
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                var events = await eventHubReceiver.ReceiveAsync(100);

                // If there is data in the batch, process it.
                if (events == null) continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    tbOutput.Text += string.Format("\r\n\r\nMessage received on partition {0}:", partition);
                    tbOutput.Text += string.Format("  {0}:", data);
                    tbOutput.Text += "Application properties (set by device):";
                    foreach (var prop in eventData.Properties)
                    {
                        tbOutput.Text += string.Format("  {0}: {1}", prop.Key, prop.Value);
                    }
                    tbOutput.Text += "System properties (set by IoT Hub):";
                    foreach (var prop in eventData.SystemProperties)
                    {
                        tbOutput.Text += string.Format("  {0}: {1}", prop.Key, prop.Value);
                    }
                }
            }
        }
    }
}
