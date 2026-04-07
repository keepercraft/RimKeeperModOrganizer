using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerWPF.Views.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
namespace RimKeeperModOrganizerWPF.Views;

public partial class AboutWindow : Window
{
    public string VersionPrefix { get; }
    public string VersionSuffix { get; }
    public string Authors { get; }
    public string Copyright { get; }
    public string FullVersion { get; }
    public string GitHubUrl { get; }

    public AboutWindow()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0.0";
        if (informationalVersion.Contains("-"))
        {
            var parts = informationalVersion.Split('-', 2);
            VersionPrefix = parts[0];
            VersionSuffix = parts[1];
        }
        else
        {
            VersionPrefix = informationalVersion;
            VersionSuffix = "Stable/Official";
        }
        Authors = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Nie określono autorów";
        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright ©";
        var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        GitHubUrl = metadata.FirstOrDefault(m => m.Key == "RepositoryUrl")?.Value ?? "https://github.com";

        InitializeComponent();
    }

    public CustomCommand OpenLinkCommand => new CustomCommand(FileHelper.OpenLink);

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}