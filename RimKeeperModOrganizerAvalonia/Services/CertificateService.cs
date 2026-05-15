using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
namespace RimKeeperModOrganizerAvalonia.Services;

public static class CertificateService
{
    //private const string CertSubject = "CN=Keepercraft DEV, O=Keepercraft";
    private const string thumbprint = "B681FAF16773ED5D16B6811FCEEDBBECB5ED85B8";
    private const string CarName = "Keepercraft_DEV.cer";

    public static void EnsureCertificateInstalled()
    {
        
        if (!IsCertInStore())
        {
            InstallFromResource();
            if (!IsCertInStore())
            {
                //MessageBox.Show($"CertificateService: don't find: {CarName}");
                return;
            }
        }
    }

    private static bool IsCertInStore()
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindByThumbprint,thumbprint,validOnly: false);
            bool exists = certs.Count > 0;
            foreach (var c in certs) c.Dispose();
            return exists;
        }
        catch 
        {
            //MessageBox.Show($"CertificateService: certificates error: {CarName}");
            return false; 
        }
    }

    private static void InstallFromResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"{nameof(RimKeeperModOrganizerAvalonia)}.{CarName}";
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            //MessageBox.Show($"CertificateService: don't find resource: {resourceName} !");
            return;
        }

        string tempPath = Path.Combine(Path.GetTempPath(), CarName);
        try
        {
            using (var fs = new FileStream(tempPath, FileMode.Create))
            {
                stream.CopyTo(fs);
            }
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c certutil -addstore -f \"Root\" \"{tempPath}\"",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi)?.WaitForExit();
        }
        catch
        {
            //MessageBox.Show($"CertificateService: processStartInfo error: {tempPath}");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}