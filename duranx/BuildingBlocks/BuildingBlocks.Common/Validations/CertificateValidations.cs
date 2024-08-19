using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BuildingBlocks.Common.Validations
{
    public static class CertificateValidations
    {
        public static bool ValidateServerCertificateBasic(
            X509Certificate? certificate,
            SslPolicyErrors sslPolicyErrors,
            string caCertPath)
        {
            if (certificate == null)
            {
                Console.WriteLine("Certificate is null.");
                return false;
            }

            // Cargar el certificado CA
            using var caCert = new X509Certificate2(caCertPath);

            // Verificar la validez de las fechas
            if (DateTime.Parse(certificate.GetEffectiveDateString()) > DateTime.Now || DateTime.Parse(certificate.GetExpirationDateString()) < DateTime.Now)
            {
                Console.WriteLine("Certificate is not valid due to date constraints.");
                return false;
            }

            // Verificación del CA
            bool verdict = (certificate.Issuer == caCert.Subject);
            if (!verdict)
            {
                Console.WriteLine("Certificate issuer does not match the expected CA.");
                return false;
            }

            Console.WriteLine("Certificate is valid.");
            return true;
        }
    }
}
