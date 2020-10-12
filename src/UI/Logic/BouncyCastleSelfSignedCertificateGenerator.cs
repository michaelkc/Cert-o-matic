using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace UI.Logic
{
    internal class BouncyCastleSelfSignedCertificateGenerator : ISelfSignedCertificateGenerator
    {
        public PublicCertificatePfxPair Generate(string name, string pfxPassword)
        {
            var keyStrength = 2048;
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyGenerator = new RsaKeyPairGenerator();
            keyGenerator.Init(keyGenerationParameters);
            Console.WriteLine("Generating key pair...");
            var subjectKeyPair = keyGenerator.GenerateKeyPair();
            Console.WriteLine("Keypair generated.");
            var privateKey = (RsaPrivateCrtKeyParameters)subjectKeyPair.Private;
            var publicKey = (RsaKeyParameters)subjectKeyPair.Public;
            Console.WriteLine($"Public key exponent: {publicKey.Exponent}");
            Console.WriteLine($"Public key modulus: {publicKey.Modulus}");
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(PkcsObjectIdentifiers.Sha256WithRsaEncryption.ToString(), privateKey);
            var certificateGenerator = new X509V3CertificateGenerator();
            BigInteger serialNo = BigInteger.ProbablePrime(128, random);
            var subjectName = new X509Name($"CN={name}");
            certificateGenerator.SetSerialNumber(serialNo);
            certificateGenerator.SetSubjectDN(subjectName);
            certificateGenerator.SetIssuerDN(subjectName);
            var notBefore = new DateTime(2000, 1, 1);
            var notAfter = notBefore.AddYears(1000);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            var certificatePermissions = new List<KeyPurposeID>()
            {
                 KeyPurposeID.IdKPServerAuth,
                 KeyPurposeID.IdKPClientAuth,
                 KeyPurposeID.AnyExtendedKeyUsage
            };

            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false, new ExtendedKeyUsage(certificatePermissions));
            var newCert = certificateGenerator.Generate(signatureFactory);

            // DotNetUtilities.ToX509Certificate(newCert).Export(X509ContentType.Pfx, pfxPassword);
            // produced a pfx file where the private key could not be loaded
            var pfxBytes = CreatePfxFile(newCert, privateKey, name, pfxPassword);
            var certBytes = DotNetUtilities.ToX509Certificate(newCert).Export(System.Security.Cryptography.X509Certificates.X509ContentType.Cert);
            return new PublicCertificatePfxPair { PfxBytes = pfxBytes, PublicCertificateBytes = certBytes };
        }

        private static byte[] CreatePfxFile(X509Certificate certificate, AsymmetricKeyParameter privateKey, string name, string password)
        {
            var certEntry = new X509CertificateEntry(certificate);

            var builder = new Pkcs12StoreBuilder();
            builder.SetUseDerEncoding(true);
            var store = builder.Build();

            store.SetKeyEntry(name, new AsymmetricKeyEntry(privateKey), new X509CertificateEntry[] { certEntry });

            using MemoryStream stream = new MemoryStream();
            store.Save(stream, password.ToCharArray(), new SecureRandom());
            var pfxBytes = stream.ToArray();
            var result = Pkcs12Utilities.ConvertToDefiniteLength(pfxBytes);
            return result;
        }
    }


}
