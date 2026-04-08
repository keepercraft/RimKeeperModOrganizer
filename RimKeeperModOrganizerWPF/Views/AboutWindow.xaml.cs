using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerWPF.Views.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
namespace RimKeeperModOrganizerWPF.Views;

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
        var assembly = Assembly.GetExecutingAssembly();
        var metadataAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        
        GitHubUrl = metadataAttributes.FirstOrDefault(m => m.Key == "RepositoryUrl")?.Value ?? "https://github.com";
        SupportPage = @"https://ko-fi.com/keepercraft";

        FileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? "0.0.0.0";
        ProductVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion ?? "0.0-null";

        Authors = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Nie określono autorów";
        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright ©";
        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "-";

        InitializeComponent();
    }

    public CustomCommand OpenLinkCommand => new CustomCommand(FileHelper.OpenLink);

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}