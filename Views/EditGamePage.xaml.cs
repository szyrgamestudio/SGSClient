using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.ViewModels;
using static SGSClient.ViewModels.EditGameViewModel;

namespace SGSClient.Views
{
	public sealed partial class EditGamePage : Page
	{
		private int selectedGameTypeId;
		private int selectedGameEngineId;
		private int gameId;
		public string GameId { get; private set; }
		public class GameTypeItem
		{
			public int Id
			{
				get; set;
			}
			public KeyValuePair<int, string> Pair
			{
				get; set;
			}

			public GameTypeItem(int id, KeyValuePair<int, string> pair)
			{
				Id = id;
				Pair = pair;
			}

			public override string ToString()
			{
				return Pair.Value;
			}
		}
		public class GameEngineItem
		{
			public int Id
			{
				get; set;
			}
			public KeyValuePair<int, string> Pair
			{
				get; set;
			}

			public GameEngineItem(int id, KeyValuePair<int, string> pair)
			{
				Id = id;
				Pair = pair;
			}

			public override string ToString()
			{
				return Pair.Value;
			}
		}
		public EditGameViewModel ViewModel { get; }
		public EditGamePage()
		{
			ViewModel = App.GetService<EditGameViewModel>();
			InitializeComponent();
			DataContext = ViewModel;
		}
		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is string gameIdParam && int.TryParse(gameIdParam, out int parsedGameId))
			{
				gameId = parsedGameId;
				GameId = gameIdParam;

				await ViewModel.LoadGameTypes();
				await ViewModel.LoadGameEngines();
				await ViewModel.LoadGameData(gameId);
			}
		}

		private void SetComboBoxSelections()
		{
			foreach (GameTypeItem item in comboBoxGameType.Items)
			{
				if (item.Id == selectedGameTypeId)
				{
					comboBoxGameType.SelectedItem = item;
					break;
				}
			}

			foreach (GameEngineItem item in comboBoxGameEngine.Items)
			{
				if (item.Id == selectedGameEngineId)
				{
					comboBoxGameEngine.SelectedItem = item;
					break;
				}
			}
		}

		private int additionalImageCount = 0;
		private void AddImageTextBox(string imageUrl)
		{
			StackPanel imageTextBoxPanel = new StackPanel { Orientation = Orientation.Horizontal };

			TextBox newImageTextBox = new TextBox
			{
				Name = "ImageTextBox" + (additionalImageCount + 1),
				Margin = new Thickness(5),
				PlaceholderText = "Wstaw link do zdjęcia",
				Width = 400,
				Text = imageUrl
			};

			Button removeButton = new Button
			{
				Margin = new Thickness(5),
				Content = new FontIcon { Glyph = "\xE74D" }
			};
			removeButton.Click += RemoveImageButton_Click;
			ToolTipService.SetToolTip(removeButton, new ToolTip { Content = "Usuń" });

			Button previewButton = new Button
			{
				Margin = new Thickness(5),
				Content = new FontIcon { Glyph = "\xE71E" }
			};
			previewButton.Click += PreviewImageButton_Click;
			ToolTipService.SetToolTip(previewButton, new ToolTip { Content = "Podgląd" });

			imageTextBoxPanel.Children.Add(newImageTextBox);
			imageTextBoxPanel.Children.Add(removeButton);
			imageTextBoxPanel.Children.Add(previewButton);

			gameGalleryStackPanel.Children.Insert(gameGalleryStackPanel.Children.Count - 1, imageTextBoxPanel);

			additionalImageCount++;
		}

		private async void ButtonSave_Click(object sender, RoutedEventArgs e)
		{
			await ViewModel.SaveGameData(gameId);
			Frame.GoBack(new DrillInNavigationTransitionInfo());
		}

		#region Buttons
		private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.DataContext is GameImage gameImage)
			{
				ViewModel.GameImages.Remove(gameImage);
			}
		}
		private void PreviewImageButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.DataContext is GameImage gameImage)
			{
				_ = OpenImagePreviewDialog(gameImage);
			}
		}
		private void AddImageButton_Click(object sender, RoutedEventArgs e)
		{
			var newGameImage = new GameImage("ms-appx:///Assets/placeholder.png"); // Placeholder URL
			ViewModel.GameImages.Add(newGameImage); // Add to the ViewModel collection

			//var imagePanel = CreateImageGrid(newGameImage);
			//gameGalleryStackPanel.Children.Insert(gameGalleryStackPanel.Children.Count - 1, imagePanel); // Insert before the last add button
		}

		private Grid CreateImageGrid(GameImage gameImage)
		{
			// Main Grid for the image panel
			var imageGrid = new Grid
			{
				Width = 150,
				Height = 150,
				Margin = new Thickness(5),
				Background = (Brush)Application.Current.Resources["AcrylicBackgroundFillColorDefaultBrush"]
			};

			// Image to display or placeholder if URL is not set
			var image = new Image
			{
				Source = new BitmapImage(new Uri(string.IsNullOrEmpty(gameImage.Url) ? "ms-appx:///Assets/placeholder.png" : gameImage.Url)),
				Stretch = Stretch.UniformToFill
			};
			imageGrid.Children.Add(image);

			// StackPanel for buttons
			var buttonStack = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(5)
			};

			// Delete Button
			var removeButton = new Button
			{
				//ToolTipService.ToolTip = "Usuń",
				Content = new FontIcon { Glyph = "\xE74D" } // Trash icon
			};
			removeButton.Click += (s, e) => ViewModel.GameImages.Remove(gameImage);
			buttonStack.Children.Add(removeButton);

			// Preview Button
			var previewButton = new Button
			{
				//ToolTipService.ToolTip = "Podgląd",
				Content = new FontIcon { Glyph = "\xE71E" } // Magnifying glass icon
			};
			previewButton.Click += async (s, e) =>
			{
				await OpenImagePreviewDialog(gameImage);
				image.Source = new BitmapImage(new Uri(gameImage.Url)); // Update image after URL is set
			};
			buttonStack.Children.Add(previewButton);

			imageGrid.Children.Add(buttonStack);

			return imageGrid;
		}

		private async Task OpenImagePreviewDialog(GameImage gameImage)
		{
			var previewDialog = new ContentDialog
			{
				Title = "Podgląd zdjęcia",
				CloseButtonText = "Zamknij",
				PrimaryButtonText = "Zatwierdź",
				XamlRoot = this.XamlRoot
			};

			// StackPanel for dialog layout
			StackPanel dialogStackPanel = new StackPanel();

			// TextBox for entering image URL
			TextBox urlTextBox = new TextBox
			{
				Margin = new Thickness(5),
				PlaceholderText = "Wstaw link do zdjęcia",
				Text = gameImage.Url
			};
			dialogStackPanel.Children.Add(urlTextBox);

			// Image for displaying the preview
			Image imagePreview = new Image
			{
				Width = 200,
				Height = 150,
				Stretch = Stretch.Uniform,
				Margin = new Thickness(5),
				Source = new BitmapImage(new Uri(gameImage.Url)) // Preview initial URL
			};
			dialogStackPanel.Children.Add(imagePreview);

			// Update image preview as the URL changes
			urlTextBox.TextChanged += (s, e) =>
			{
				try
				{
					imagePreview.Source = new BitmapImage(new Uri(urlTextBox.Text));
				}
				catch
				{
					// Handle invalid URI
					imagePreview.Source = null;
				}
			};

			previewDialog.Content = dialogStackPanel;

			// Show the dialog and confirm changes
			var result = await previewDialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				// Update the game image URL
				gameImage.Url = urlTextBox.Text;
			}
		}
		private void gotoSGSClientWWW_Click(object sender, RoutedEventArgs e)
		{
			var URL = "https://sgsclient.m455yn.dev/upload";
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = URL,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine("Wystąpił błąd podczas otwierania linku do logo gry: " + ex.Message);
			}
		}
		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
		}
		#endregion
	}
}
