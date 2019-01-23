using System;
using System.Collections.ObjectModel;
using System.Linq;
using NdcDemo.Services;
using NdcDemo.Services.Dtos;
using Xamarin.Forms;
using NdcDemo.Services.Providers;

namespace ndcdemo
{
	public class App : Application
	{
		readonly ObservableCollection<Message> _messages;

		Entry _nameInput;
		Entry _textInput;

		public App()
		{
			_messages = new ObservableCollection<Message>();

			MainPage = new NavigationPage(BuildMainPage());
			ObserveServerMessages();
		}

		ContentPage BuildMainPage()
		{
			var messageCell = new DataTemplate(typeof(TextCell));
			messageCell.SetBinding(TextCell.TextProperty, nameof(Message.Text));
			messageCell.SetBinding(TextCell.DetailProperty, nameof(Message.Name));

			var messageView = new ListView { ItemTemplate = messageCell };
			messageView.ItemsSource = _messages;

			_nameInput = new Entry { WidthRequest = 200 };
			_textInput = new Entry { WidthRequest = 200 };

			var postButton = new Button { Text = "Post" };
			postButton.Clicked += (sender, e) => PostNewMessage();

			var content = new ContentPage {
				Title = "ndcdemo",
				Content = new StackLayout {
					Children = {
						messageView,
						new StackLayout {
							Orientation = StackOrientation.Horizontal,
							Margin= new Thickness(15,5),
							Children = {
								new Label { Text = "Name", WidthRequest=50 },
								_nameInput
							}
						},
						new StackLayout {
							Orientation = StackOrientation.Horizontal,
							Margin= new Thickness(15,5),
							Children = {
								new Label { Text = "Text", WidthRequest=50},
								_textInput
							}
						},
						postButton
					}
				}
			};

			return content;
		}

		void ObserveServerMessages()
		{
			var dataService = ServiceContainer.DataService;
			dataService.ObserveMessages((obsType, msg) => {
				if (obsType == ObservationType.ChildAdded) {
					AddMessage(msg);
				}
				else if (obsType == ObservationType.ChildRemoved) {
					DeleteMessage(msg);
				}
			});
		}

		void AddMessage(Message msg)
		{
			var msgAfter = _messages
				.Where(m => m.Date < msg.Date)
				.FirstOrDefault();

			if (msgAfter == null) {
				_messages.Add(msg);
			}
			else {
				var idx = _messages.IndexOf(msgAfter);
				_messages.Insert(idx, msg);
			}
		}

		void DeleteMessage(Message msg)
		{
			var msgToDelete = _messages.FirstOrDefault(m => m.Id == msg.Id);
			if (msgToDelete != null) {
				_messages.Remove(msgToDelete);
			}
		}

		void PostNewMessage()
		{
			if (string.IsNullOrWhiteSpace(_nameInput.Text) || string.IsNullOrWhiteSpace(_textInput.Text)) return;

			var dataService = ServiceContainer.DataService;
			dataService.PostMessage(new Message { Name = _nameInput.Text, Text = _textInput.Text, Date = DateTime.Now });
		}
	}
}
