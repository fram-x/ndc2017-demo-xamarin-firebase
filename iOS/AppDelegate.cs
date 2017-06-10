using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.iOS.Providers;
using Foundation;
using NdcDemo.iOS;
using NdcDemo.Services;
using UIKit;

namespace ndcdemo.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			Firebase.Analytics.App.Configure ();

			ServiceContainer.Logger = new Logger();
			ServiceContainer.DataProviderFactory = new DataProviderFactory(ServiceContainer.Logger);
			ServiceContainer.DataService = new DataService(ServiceContainer.DataProviderFactory, ServiceContainer.Logger);

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}
