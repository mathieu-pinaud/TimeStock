using Microsoft.AspNetCore.Components.WebView.Maui;
using Client.Pages;

namespace Mobile;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var blazorWebView = this.FindByName<BlazorWebView>("BlazorView");
		blazorWebView?.RootComponents.Add(new RootComponent(typeof(Home)));
    }
}
