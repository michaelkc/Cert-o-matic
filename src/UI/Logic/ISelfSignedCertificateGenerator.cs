namespace UI.Logic
{

    internal interface ISelfSignedCertificateGenerator
    {
        PublicCertificatePfxPair Generate(string name, string pfxPassword);
    }
}
