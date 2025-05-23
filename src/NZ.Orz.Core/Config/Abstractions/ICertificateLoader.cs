﻿using System.Security.Cryptography.X509Certificates;

namespace NZ.Orz.Config;

public interface ICertificateLoader
{
    (X509Certificate2?, X509Certificate2Collection?) LoadCertificate(SslConfig? certInfo);
}