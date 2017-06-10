using System;
using NdcDemo.Services;
using NdcDemo.Services.Dtos;
using Xamarin.Forms;

namespace ndcdemo
{
	public class App : Application
	{
		public App()
		{
			var testButton = new Button { Text = "Test database" };
			testButton.Clicked += (sender, e) => TestFirebase();

			// The root page of your application
			var content = new ContentPage {
				Title = "ndcdemo",
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Label {
							HorizontalTextAlignment = TextAlignment.Center,
							Text = "Welcome to Xamarin Forms!"
						},
						testButton
					}
				}
			};

			MainPage = new NavigationPage(content);
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

		void TestFirebase()
		{
			var dataService = ServiceContainer.DataService;
			dataService.PostMessage(new Message { Name="Test", Text="Hello from Xamarin", Date=DateTime.Now });
		}
	}
}
