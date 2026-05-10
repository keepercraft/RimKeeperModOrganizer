using Avalonia.Controls;
using Avalonia.Interactivity;
using KeeperBaseSheredLib;
using RimKeeperModOrganizerAvalonia.Converters;
using RimKeeperModOrganizerLib.Helpers;
using System.Linq;
using System.Reflection;
namespace RimKeeperModOrganizerAvalonia.Views;

public partial class AboutWindow : Window
{
    public string Authors { get; }
    public string Copyright { get; }
    public string GitHubUrl { get; }
    public string FileVersion { get; }
    public string ProductVersion { get; }
    public string SupportPage { get; }
    public string Description { get; }

    public AboutWindow()
    {
        var ico = ModIconConverter.Get("RimworldLogoIcon");
        if (ico != null)
            this.Icon = ModIconConverter.CreateIconFromDrawingImage(ico);
        InitializeComponent();


        var assembly = Assembly.GetExecutingAssembly();
        var metadataAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();

        GitHubUrl = metadataAttributes.FirstOrDefault(m => m.Key == "RepositoryUrl")?.Value ?? "https://github.com";
        SupportPage = @"https://ko-fi.com/keepercraft";
        FileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0.0.0";
        ProductVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0.0";
        Authors = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Nie określono autorów";
        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright ©";
        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "-";

        DataContext = this;
    }

    public CustomCommand OpenLinkCommand2 => new CustomCommand(FileHelper.OpenLink);

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    public void OpenLinkCommand(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is AboutWindow vm && sender is TextBlock context)
        {
            vm.OpenLinkCommand2.Execute(context.Text);
        }
    }
}