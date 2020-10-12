using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace UI.Logic
{
    // Sadly, CertificateRequest is not supported on netcore3.1 Blazor
    internal class NetCoreSelfSignedCertificateGenerator : ISelfSignedCertificateGenerator
    {
        public PublicCertificatePfxPair Generate(string name, string pfxPassword)
        {
            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={name}");

            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.2") }, false));

            var cert = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
            var pfxBytes = cert.Export(X509ContentType.Pfx, pfxPassword);
            var certBytes = cert.Export(X509ContentType.Cert);
            return new PublicCertificatePfxPair { PfxBytes = pfxBytes, PublicCertificateBytes = certBytes };
        }
    }
}
