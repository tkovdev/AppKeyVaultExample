using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using PrefixKeyVault;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//KV URI is required to know where to grab secrets from
string vaultUri = builder.Configuration["KeyVault:Uri"] ?? throw new NullReferenceException("Missing KeyVault:URI");
//TenantId & ClientId of app registration to authenticate to KV. NOTE: KV must allow SP for AppReg to be Key Vault Secret User to be able to acquire appsettings / secrets
string tenantId = builder.Configuration["AzureAd:TenantId"] ?? throw new NullReferenceException("Missing AzureAd:TenantId");
string clientId = builder.Configuration["AzureAd:ClientId"] ?? throw new NullReferenceException("Missing AzureAd:ClientId");

//KeyVault can be used to generate the PFX, create the PFX and export it in Pkcs12. Passwords will be blank. Certificate can be included in the /Certificates path of the build. Do not commit PFX to code.
string certPath = Path.Combine(AppContext.BaseDirectory, "Certificates", "app-auth.pfx");

if (!File.Exists(certPath))
{
    throw new FileNotFoundException($"The certificate file could not be found at: {certPath}");
}

using var certificate = X509CertificateLoader.LoadPkcs12FromFile(certPath, String.Empty);

//Once a Certificate is created from KeyVault, add it to the App Registration by uploading under the Certificates & secrets section. NOTE: a separate export from KeyVault will be needed. App Regs can only import .cer and other similar cert file types.
var certificateCredential = new ClientCertificateCredential(tenantId, clientId, certificate);

builder.Configuration.AddAzureKeyVault(
    new Uri(vaultUri), 
    certificateCredential, 
    new AzureKeyVaultConfigurationOptions
    {
        // Only load secrets that start with this app's prefix
        Manager = new PrefixKeyVaultSecretManager("APIOne") 
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();