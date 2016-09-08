#if !FxCop
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Microsoft.Cci")]
[assembly: AssemblyDescription("Compiler oriented replacement for System.Reflection and System.Reflection.Emit")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Microsoft (R) .NET Framework")]
[assembly: AssemblyCopyright("Copyright (C) Microsoft Corp. 2002, 2003, 20004. All rights reserved")]
[assembly: AssemblyTrademark("Microsoft and Windows are either registered trademarks or trademarks of Microsoft Corporation in the U.S. and/or other countries")]
[assembly:System.Resources.NeutralResourcesLanguage("en-US")]
#if DelaySign
//[assembly: AssemblyDelaySign(true)]
//[assembly: AssemblyKeyFile("..\\..\\..\\Common\\FinalPublicKey.snk")]
#else
//[assembly: AssemblyKeyFile("..\\..\\..\\Common\\InterimKey.snk")]
#endif
#if !ROTOR
[assembly: System.Runtime.InteropServices.ComVisible(false)]
#endif
[assembly: CLSCompliant(false)]
#endif