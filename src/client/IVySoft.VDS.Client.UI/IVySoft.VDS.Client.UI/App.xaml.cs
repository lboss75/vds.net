using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IVySoft.VDS.Client.UI.Services;
using IVySoft.VDS.Client.UI.Views;

namespace IVySoft.VDS.Client.UI
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
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
