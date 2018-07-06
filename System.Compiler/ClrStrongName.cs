using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Compiler
{
    public static class ClrStrongName
    {
        private static IClrStrongName clrStrongName;
        private static IClrStrongName GetClrStrongName()
        {
            return clrStrongName ?? (clrStrongName =
                (IClrStrongName)RuntimeEnvironment.GetRuntimeInterfaceAsObject(
                    new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92"),
                    typeof(IClrStrongName).GUID));
        }

        public static void SignatureGeneration(string filePath, string keyContainer, byte[] keyBlob)
        {
            GetClrStrongName().StrongNameSignatureGeneration(filePath, keyContainer, keyBlob, keyBlob.Length, IntPtr.Zero, IntPtr.Zero);
        }

        // ReSharper disable UnusedMember.Global – All preceding method declarations are needed
        // in order for StrongNameSignatureGeneration to end up at the right COM slot number.

        [ComImport, ComConversionLoss, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")]
        private interface IClrStrongName
        {
            void GetHashFromAssemblyFile(
                string pszFilePath, 
                ref uint piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash,
                uint cchHash,
                out uint pchHash);

            void GetHashFromAssemblyFileW(
                string pwzFilePath,
                ref uint piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash,
                uint cchHash,
                out uint pchHash);

            void GetHashFromBlob(
                IntPtr pbBlob,
                uint cchBlob,
                ref uint piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] pbHash,
                uint cchHash,
                out uint pchHash);

            void GetHashFromFile(
                string pszFilePath,
                ref int piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash,
                uint cchHash,
                out uint pchHash);

            void GetHashFromFileW(
                string pwzFilePath,
                ref uint piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash,
                uint cchHash,
                out uint pchHash);

            void GetHashFromHandle(
                SafeFileHandle hFile,
                ref uint piHashAlg,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash,
                uint cchHash,
                out uint pchHash);

            uint StrongNameCompareAssemblies(string pwzAssembly1, string pwzAssembly2);

            void StrongNameFreeBuffer(IntPtr pbMemory);

            void StrongNameGetBlob(
                string pwzFilePath,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbBlob,
                ref uint pcbBlob);

            void StrongNameGetBlobFromImage(
                IntPtr pbBase,
                uint dwLength,
                [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbBlob,
                ref uint pcbBlob);

            void StrongNameGetPublicKey(
                string pwzKeyContainer,
                IntPtr pbKeyBlob,
                uint cbKeyBlob,
                out IntPtr ppbPublicKeyBlob,
                out uint pcbPublicKeyBlob);

            uint StrongNameHashSize(uint ulHashAlg);

            void StrongNameKeyDelete(string pwzKeyContainer);

            void StrongNameKeyGen(
                string pwzKeyContainer,
                uint dwFlags,
                out IntPtr ppbKeyBlob,
                out uint pcbKeyBlob);

            void StrongNameKeyGenEx(
                string pwzKeyContainer,
                uint dwFlags,
                uint dwKeySize,
                out IntPtr ppbKeyBlob,
                out uint pcbKeyBlob);

            void StrongNameKeyInstall(
                string pwzKeyContainer,
                IntPtr pbKeyBlob,
                uint cbKeyBlob);

            void StrongNameSignatureGeneration(
                string pwzFilePath,
                string pwzKeyContainer,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbKeyBlob,
                int cbKeyBlob,
                IntPtr ppbSignatureBlob,
                IntPtr pcbSignatureBlob);

            void StrongNameSignatureGenerationEx(
                string wszFilePath,
                string wszKeyContainer,
                IntPtr pbKeyBlob,
                uint cbKeyBlob,
                out IntPtr ppbSignatureBlob,
                out uint pcbSignatureBlob,
                uint dwFlags);

            uint StrongNameSignatureSize(IntPtr pbPublicKeyBlob, uint cbPublicKeyBlob);

            uint StrongNameSignatureVerification(string pwzFilePath, uint dwInFlags);

            bool StrongNameSignatureVerificationEx(string pwzFilePath, bool fForceVerification);

            uint StrongNameSignatureVerificationFromImage(IntPtr pbBase, uint dwLength, uint dwInFlags);

            void StrongNameTokenFromAssembly(string pwzFilePath, out IntPtr ppbStrongNameToken, out uint pcbStrongNameToken);

            void StrongNameTokenFromAssemblyEx(
                string pwzFilePath,
                out IntPtr ppbStrongNameToken,
                out uint pcbStrongNameToken,
                out IntPtr ppbPublicKeyBlob,
                out uint pcbPublicKeyBlob);

            void StrongNameTokenFromPublicKey(
                IntPtr pbPublicKeyBlob,
                uint cbPublicKeyBlob,
                out IntPtr ppbStrongNameToken,
                out uint pcbStrongNameToken);
        }
    }
}
