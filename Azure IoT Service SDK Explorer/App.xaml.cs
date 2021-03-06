﻿using System;

using Azure_IoT_Service_SDK_Explorer.Services;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Azure.Devices;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Azure_IoT_Service_SDK_Explorer
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;

        public static ServiceClient ServiceClient = null;
        public static string IoTHubConnectionString = "";

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            // TODO WTS: Add your app in the app center and set your secret here. More at https://docs.microsoft.com/en-us/appcenter/sdk/getting-started/uwp
            AppCenter.Start("178f1114-5421-4749-a7b3-18a194490464", typeof(Analytics), typeof(Crashes));

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.ConnectPage), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}
