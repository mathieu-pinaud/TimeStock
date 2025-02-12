namespace Mobile;
using Client.Pages;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		RootComponents.Add("#app", typeof(Home));
	}
}
