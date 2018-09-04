using Microsoft.Azure.Devices;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Service_SDK_Explorer.Views
{
    public sealed partial class ConnectPage : Page, INotifyPropertyChanged
    {
        public ConnectPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("hubConnectionString"))
            {
                tbConnectionString.Text = ApplicationData.Current.LocalSettings.Values["hubConnectionString"].ToString();
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

        private void btnCreate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TransportType transportType = TransportType.Amqp;
            if (rbAmqpWeb.IsChecked == true)
            {
                transportType = TransportType.Amqp_WebSocket_Only;
            }

            App.ServiceClient = ServiceClient.CreateFromConnectionString(tbConnectionString.Text, transportType);
            App.IoTHubConnectionString = tbConnectionString.Text;
            ApplicationData.Current.LocalSettings.Values["hubConnectionString"] = tbConnectionString.Text;

            var builder = IotHubConnectionStringBuilder.Create(tbConnectionString.Text);
            string iotHubName = builder.HostName.Split('.')[0];
            tbHubName.Text = "IotHubName = " + iotHubName.ToString();
            tbHostName.Text = "HostName = " + builder.HostName.ToString();
            tbSharedAccessKeyName.Text = "SharedAccessKeyName = " + builder.SharedAccessKeyName.ToString();
            statusBorder.BorderBrush = new SolidColorBrush(Colors.Green);
        }
    }
}
