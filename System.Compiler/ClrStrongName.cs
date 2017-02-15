using System.Runtime.InteropServices;

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

        [ComImport, ComConversionLoss, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")]
        private interface IClrStrongName
        {
            void StrongNameSignatureGeneration(
                [MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
                [MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbKeyBlob,
                int cbKeyBlob,
                IntPtr ppbSignatureBlob,
                IntPtr pcbSignatureBlob);
        }
    }
}
