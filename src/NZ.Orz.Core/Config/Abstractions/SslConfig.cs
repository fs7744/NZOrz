using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NZ.Orz.Config;

public class SslConfig
{
    public SslProtocols? SupportSslProtocols { get; set; }
    public bool Passthrough { get; set; }
    public ClientCertificateMode ClientCertificateMode { get; set; }
    public Func<Stream, SslStream> SslStreamFactory { get; internal set; }

    internal void Init()
    {
        RemoteCertificateValidationCallback? remoteCertificateValidationCallback = ClientCertificateMode == ClientCertificateMode.NoCertificate ?
           (RemoteCertificateValidationCallback?)null : RemoteCertificateValidationCallback;
        SslStreamFactory = s => new SslStream(s, leaveInnerStreamOpen: false, userCertificateValidationCallback: remoteCertificateValidationCallback);
    }

    private bool RemoteCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return RemoteCertificateValidationCallback(ClientCertificateMode, null, certificate, chain, sslPolicyErrors);
    }

    internal static bool RemoteCertificateValidationCallback(
    ClientCertificateMode clientCertificateMode,
    Func<X509Certificate2, X509Chain?, SslPolicyErrors, bool>? clientCertificateValidation,
    X509Certificate? certificate,
    X509Chain? chain,
    SslPolicyErrors sslPolicyErrors)
    {
        if (certificate == null)
        {
            return clientCertificateMode != ClientCertificateMode.RequireCertificate;
        }

        if (clientCertificateValidation == null)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }
        }

        var certificate2 = ConvertToX509Certificate2(certificate);
        if (certificate2 == null)
        {
            return false;
        }

        if (clientCertificateValidation != null)
        {
            if (!clientCertificateValidation(certificate2, chain, sslPolicyErrors))
            {
                return false;
            }
        }

        return true;
    }

    private static X509Certificate2? ConvertToX509Certificate2(X509Certificate? certificate)
    {
        if (certificate == null)
        {
            return null;
        }

        if (certificate is X509Certificate2 cert2)
        {
            return cert2;
        }

        return new X509Certificate2(certificate);
    }
}