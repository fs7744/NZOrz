using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NZ.Orz.Config;

public class SslConfig
{
    public SslProtocols SupportSslProtocols { get; set; } = SslProtocols.None;
    public bool Passthrough { get; set; }
    public Func<Stream, SslStream> SslStreamFactory { get; internal set; }
    public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(1);
    public bool ClientCertificateRequired { get; set; }
    public bool CheckCertificateRevocation { get; set; }

    public bool IsFileCert => !string.IsNullOrEmpty(Path);

    public string? Path { get; init; }

    public string? KeyPath { get; init; }

    public string? Password { get; init; }

    [MemberNotNullWhen(true, nameof(Subject))]
    public bool IsStoreCert => !string.IsNullOrEmpty(Subject);

    public string? Subject { get; init; }

    public string? Store { get; init; }

    public string? Location { get; init; }

    public bool? AllowInvalid { get; init; }

    public SslServerAuthenticationOptions Options { get; internal set; }

    internal void Init()
    {
        SslStreamFactory = s => new SslStream(s);

        Options = new SslServerAuthenticationOptions
        {
            ServerCertificate = LoadServerCertificate(),
            ClientCertificateRequired = ClientCertificateRequired,
            EnabledSslProtocols = SupportSslProtocols,
            CertificateRevocationCheckMode = CheckCertificateRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck,
            EncryptionPolicy = EncryptionPolicy.RequireEncryption,
        };
        //RemoteCertificateValidationCallback? remoteCertificateValidationCallback = ClientCertificateMode == ClientCertificateMode.NoCertificate ?
        //   (RemoteCertificateValidationCallback?)null : RemoteCertificateValidationCallback;
        //SslStreamFactory = s => new SslStream(s, leaveInnerStreamOpen: false, userCertificateValidationCallback: remoteCertificateValidationCallback);
    }

    private X509Certificate2 LoadServerCertificate()
    {
        throw new NotImplementedException();
    }

    //private bool RemoteCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    //{
    //    return RemoteCertificateValidationCallback(ClientCertificateMode, null, certificate, chain, sslPolicyErrors);
    //}

    //internal static bool RemoteCertificateValidationCallback(
    //ClientCertificateMode clientCertificateMode,
    //Func<X509Certificate2, X509Chain?, SslPolicyErrors, bool>? clientCertificateValidation,
    //X509Certificate? certificate,
    //X509Chain? chain,
    //SslPolicyErrors sslPolicyErrors)
    //{
    //    if (certificate == null)
    //    {
    //        return clientCertificateMode != ClientCertificateMode.RequireCertificate;
    //    }

    //    if (clientCertificateValidation == null)
    //    {
    //        if (sslPolicyErrors != SslPolicyErrors.None)
    //        {
    //            return false;
    //        }
    //    }

    //    var certificate2 = ConvertToX509Certificate2(certificate);
    //    if (certificate2 == null)
    //    {
    //        return false;
    //    }

    //    if (clientCertificateValidation != null)
    //    {
    //        if (!clientCertificateValidation(certificate2, chain, sslPolicyErrors))
    //        {
    //            return false;
    //        }
    //    }

    //    return true;
    //}

    //private static X509Certificate2? ConvertToX509Certificate2(X509Certificate? certificate)
    //{
    //    if (certificate == null)
    //    {
    //        return null;
    //    }

    //    if (certificate is X509Certificate2 cert2)
    //    {
    //        return cert2;
    //    }

    //    return new X509Certificate2(certificate);
    //}
}