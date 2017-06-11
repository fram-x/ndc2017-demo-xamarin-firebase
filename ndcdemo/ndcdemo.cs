using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NdcDemo.Services;
using NdcDemo.Services.Dtos;
using Xamarin.Forms;
using NdcDemo.Services.Providers;

namespace ndcdemo
{
	public class App : Application
	{
		ObservableCollection<Message> Messages;

		public App()
		{	
			Messages = new ObservableCollection<Message>();
			MainPage = new NavigationPage(BuildMainPage());
			LoadMessagesAndObserveAdditions();
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

		ContentPage BuildMainPage() 
		{
			var testButton = new Button { Text = "Test database" };
			testButton.Clicked += (sender, e) => TestFirebase();

			var messageCell = new DataTemplate(typeof(TextCell));
			messageCell.SetBinding(TextCell.TextProperty, nameof(Message.Text));
			messageCell.SetBinding(TextCell.DetailProperty, nameof(Message.Name));

			var messageView = new ListView { ItemTemplate = messageCell };
			messageView.ItemsSource = Messages;

			// The root page of your application
			var content = new ContentPage {
				Title = "ndcdemo",
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Fill,
					Children = {
						messageView,
						new Label {
							HorizontalTextAlignment = TextAlignment.Center,
							Text = "Welcome to Xamarin Forms!"
						},
						testButton
					}
				}
			};

			return content;
		}

		void LoadMessagesAndObserveAdditions()
		{
			var dataService = ServiceContainer.DataService;
			dataService.ObserveMessages((obsType, msg) => {
				if (obsType == ObservationType.ChildAdded) {
					AddMessage(msg);
				}
			});

			//Task.Run(async () => {
			//	var dataService = ServiceContainer.DataService;
			//	var messages = await dataService.GetMessagesAsync();

			//	foreach (var m in messages.OrderByDescending(m => m.Date)) {
			//		Messages.Add(m);
			//	}

			//	dataService.ObserveMessages();
			//});
		}

		void AddMessage(Message msg)
		{
			var msgAfter = Messages
				.Where(m => m.Date < msg.Date)
				.FirstOrDefault();

			if (msgAfter == null) {
				Messages.Add(msg);
			}
			else {
				var idx = Messages.IndexOf(msgAfter);
				Messages.Insert(idx, msg);
			}
		}

		void TestFirebase()
		{
			var dataService = ServiceContainer.DataService;
			dataService.PostMessage(new Message { Name="Test", Text="Hello from Xamarin", Date=DateTime.Now });
		}
	}
}
