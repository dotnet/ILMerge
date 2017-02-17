using System.Security.Cryptography;

namespace ILMerging.Tests.Helpers
{
    public static class CspContainerUtils
    {
        public static void ImportBlob(bool machineLevel, string containerName, KeyNumber keyNumber, byte[] blob)
        {
            using (var rspCsp = new RSACryptoServiceProvider(new CspParameters
            {
                KeyContainerName = containerName,
                KeyNumber = (int)keyNumber,
                Flags = machineLevel ? CspProviderFlags.UseMachineKeyStore : 0
            }))
            {
                rspCsp.ImportCspBlob(blob);
            }
        }

        public static byte[] ExportBlob(bool machineLevel, string containerName, KeyNumber keyNumber, bool includePrivateParameters)
        {
            using (var rspCsp = new RSACryptoServiceProvider(new CspParameters
            {
                KeyContainerName = containerName,
                KeyNumber = (int)keyNumber,
                Flags = CspProviderFlags.UseExistingKey | (machineLevel ? CspProviderFlags.UseMachineKeyStore : 0)
            }))
            {
                return rspCsp.ExportCspBlob(includePrivateParameters);
            }
        }

        public static void Delete(bool machineLevel, string containerName, KeyNumber keyNumber)
        {
            using (var rspCsp = new RSACryptoServiceProvider(new CspParameters
            {
                KeyContainerName = containerName,
                KeyNumber = (int)keyNumber,
                Flags = machineLevel ? CspProviderFlags.UseMachineKeyStore : 0
            }))
            {
                rspCsp.PersistKeyInCsp = false;
            }
        }
    }
}
