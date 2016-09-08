using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
#if FxCop
using InterfaceList = Microsoft.Cci.InterfaceCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
using Module = Microsoft.Cci.ModuleNode;
using Class = Microsoft.Cci.ClassNode;
using Interface = Microsoft.Cci.InterfaceNode;
#endif
#if CCINamespace
using Microsoft.Cci.Metadata;
#else
using System.Compiler.Metadata;
#endif

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler{
#endif
#if !FxCop
  public 
#endif    
  sealed class SystemAssemblyLocation{
    static string location;
    public static string Location{
      get
      {
        return location;
      }
      set
      {
        //Debug.Assert(location == null || location == value, string.Format("You attempted to set the mscorlib.dll location to\r\n\r\n{0}\r\n\r\nbut it was already set to\r\n\r\n{1}\r\n\r\nThis may occur if you have multiple projects that target different platforms. Make sure all of your projects target the same platform.\r\n\r\nYou may try to continue, but targeting multiple platforms during the same session is not supported, so you may see erroneous behavior.", value, location));
        location = value;
      }
    }
    public static AssemblyNode ParsedAssembly;
    /// <summary>
    /// Allows compilers to share an assembly cache for loading runtime and system dlls.
    /// </summary>
    public static System.Collections.IDictionary SystemAssemblyCache = null;
  }
#if ExtendedRuntime
  public sealed class SystemCompilerRuntimeAssemblyLocation{
    public static string Location {
      get { return location; }
      set {
        location = value;
        Identifier id = Identifier.For("System.Compiler.Runtime");
        AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[id.UniqueIdKey];
        if (aref == null) {
          aref = new AssemblyReference(typeof(ComposerAttribute).Assembly.FullName);
          TargetPlatform.AssemblyReferenceFor[id.UniqueIdKey] = aref;
        }
        aref.Location = value;
      }
    }
    private static string location = null; //Can be set by compiler in cross compilation scenarios
    public static AssemblyNode ParsedAssembly;
  }
#endif
  public sealed class SystemDllAssemblyLocation {
    public static string Location = null;
  }
#if !NoData && !ROTOR
  public sealed class SystemDataAssemblyLocation{
    public static string Location = null;
  }
#endif
#if !NoXml && !NoRuntimeXml
  public sealed class SystemXmlAssemblyLocation{
    public static string Location = null;
  }
#endif
  public sealed class SystemRuntimeCollectionsAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemDiagnosticsDebugAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemDiagnosticsToolsAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemGlobalizationAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemReflectionAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemResourceManagerAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeExtensionsAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeInteropServicesAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeWindowsRuntimeAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeIOServicesAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeSerializationAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation {
    public static string Location = null;
  }
  public sealed class SystemThreadingAssemblyLocation {
    public static string Location = null;
  }
#if !FxCop
  public 
#endif
  sealed class TargetPlatform{
    private TargetPlatform(){}
    public static bool BusyWithClear;
    public static bool DoNotLockFiles;
    public static bool GetDebugInfo;
    public static char GenericTypeNamesMangleChar = '_';

    private static bool useGenerics;

    public static bool UseGenerics {
      get {
        if (useGenerics) return true;
        Version v = TargetPlatform.TargetVersion;
        if (v == null) {
          v = CoreSystemTypes.SystemAssembly.Version;
          if (v == null)
            v = typeof(object).Assembly.GetName().Version;
        }
        return v.Major > 1 || v.Minor > 2 || v.Minor == 2 && v.Build >= 3300;
      }
      set
      {
        useGenerics = value;
      }
    }

    public static void Clear(){
      SystemAssemblyLocation.Location = null;
      SystemDllAssemblyLocation.Location = null;
      SystemRuntimeCollectionsAssemblyLocation.Location = null;
      SystemDiagnosticsDebugAssemblyLocation.Location = null;
      SystemDiagnosticsToolsAssemblyLocation.Location = null;
      SystemGlobalizationAssemblyLocation.Location = null;
      SystemReflectionAssemblyLocation.Location = null;
      SystemResourceManagerAssemblyLocation.Location = null;
      SystemRuntimeExtensionsAssemblyLocation.Location = null;
      SystemRuntimeInteropServicesAssemblyLocation.Location = null;
      SystemRuntimeWindowsRuntimeAssemblyLocation.Location = null;
      SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location = null;
      SystemRuntimeIOServicesAssemblyLocation.Location = null;
      SystemRuntimeSerializationAssemblyLocation.Location = null;
      SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location = null;
      SystemThreadingAssemblyLocation.Location = null;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = null;
#endif
#if !NoData && !ROTOR
      SystemDataAssemblyLocation.Location = null;
#endif
#if !NoXml && !NoRuntimeXml
      SystemXmlAssemblyLocation.Location = null;
#endif
      TargetPlatform.DoNotLockFiles = false;
      TargetPlatform.GetDebugInfo = false;
      TargetPlatform.PlatformAssembliesLocation = "";
      TargetPlatform.BusyWithClear = true;
      SystemTypes.Clear();
      TargetPlatform.BusyWithClear = false;
    }
    public static System.Collections.IDictionary StaticAssemblyCache {
      get { return Reader.StaticAssemblyCache; }
    }
    public static Version TargetVersion =
#if WHIDBEY
 new Version(2, 0, 50727);  // Default for a WHIDBEY compiler
#else
      new Version(1, 0, 5000);  // Default for an Everett compiler
#endif
    public static string TargetRuntimeVersion;

    public static int LinkerMajorVersion {
      get {
        switch (TargetVersion.Major) {
          case 4: return 8;
          case 2: return 8;
          case 1: return 7;
          default: return 6;
        }
      }
    }
    public static int LinkerMinorVersion {
      get {
        return TargetVersion.Minor;
      }
    }

    public static int MajorVersion { get { return TargetVersion.Major; } }
    public static int MinorVersion { get { return TargetVersion.Minor; } }
    public static int Build { get { return TargetVersion.Build; } }

    public static string/*!*/ PlatformAssembliesLocation = String.Empty;
    private static TrivialHashtable assemblyReferenceFor;
    internal static bool AssemblyReferenceForInitialized{
      get { return assemblyReferenceFor != null; }
    }
    public static TrivialHashtable/*!*/ AssemblyReferenceFor {
      get{
        if (TargetPlatform.assemblyReferenceFor == null) {
          TargetPlatform.SetupAssemblyReferenceFor();
        }
        //^ assume TargetPlatform.assemblyReferenceFor != null;
        return TargetPlatform.assemblyReferenceFor;
      }
      set{
        TargetPlatform.assemblyReferenceFor = value;
      }
    }
    private readonly static string[]/*!*/ FxAssemblyNames = 
      new string[]{"Accessibility", "CustomMarshalers", "IEExecRemote", "IEHost", "IIEHost", "ISymWrapper", 
                    "Microsoft.JScript", "Microsoft.VisualBasic", "Microsoft.VisualBasic.Vsa", "Microsoft.VisualC",
                    "Microsoft.Vsa", "Microsoft.Vsa.Vb.CodeDOMProcessor", "mscorcfg", "Regcode", "System",
                    "System.Configuration.Install", "System.Data", "System.Design", "System.DirectoryServices",
                    "System.Drawing", "System.Drawing.Design", "System.EnterpriseServices", 
                    "System.Management", "System.Messaging", "System.Runtime.Remoting", "System.Runtime.Serialization.Formatters.Soap",
                    "System.Runtime.WindowsRuntime",
                    "System.Security", "System.ServiceProcess", "System.Web", "System.Web.Mobile", "System.Web.RegularExpressions",
                    "System.Web.Services", "System.Windows.Forms", "System.Xml", "TlbExpCode", "TlbImpCode", "cscompmgd",
                    "vjswfchtml", "vjswfccw", "VJSWfcBrowserStubLib", "vjswfc", "vjslibcw", "vjslib", "vjscor", "VJSharpCodeProvider"};
    private readonly static string[]/*!*/ FxAssemblyToken =
      new string[]{"b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b77a5c561934e089",
                    "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", 
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b77a5c561934e089", "b03f5f7f11d50a3a",
                    "b77a5c561934e089",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b77a5c561934e089", "b77a5c561934e089", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a",
                    "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a", "b03f5f7f11d50a3a"};
    private readonly static string[]/*!*/ FxAssemblyVersion1 = 
      new string[]{"1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "7.0.3300.0", "7.0.3300.0", "7.0.3300.0", "7.0.3300.0",
                    "7.0.3300.0", "7.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", 
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "4.0.0.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0",
                    "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "1.0.3300.0", "7.0.3300.0"};
    private readonly static string[]/*!*/ FxAssemblyVersion1_1 = 
      new string[]{"1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "7.0.5000.0", "7.0.5000.0", "7.0.5000.0", "7.0.5000.0",
                    "7.0.5000.0", "7.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", 
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", 
                    "4.0.0.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0",
                    "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "1.0.5000.0", "7.0.5000.0"};
    private static string[]/*!*/ FxAssemblyVersion2Build3600 =
      new string[]{"2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "8.0.1200.0", "8.0.1200.0", "8.0.1200.0", "8.0.1200.0",
                    "8.0.1200.0", "8.0.1200.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", 
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", 
                    "4.0.0.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "8.0.1200.0",
                    "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "2.0.3600.0", "7.0.5000.0"};
    private static string[]/*!*/ FxAssemblyVersion2 =
      new string[]{"2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "8.0.0.0", "8.0.0.0", "8.0.0.0", "8.0.0.0",
                    "8.0.0.0", "8.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", 
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", 
                    "4.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "8.0.0.0",
                    "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0", "2.0.0.0"};
    private static string[]/*!*/ FxAssemblyVersion4 =
      new string[]{"4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0",
                    "10.0.0.0", "10.0.0.0", "10.0.0.0", "10.0.0.0",
                    "10.0.0.0", "10.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0",
                    "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0",
                    "4.0.0.0", "4.0.0.0", "4.0.0.0", 
                    "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", 
                    "4.0.0.0",
                    "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0",
                    "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "10.0.0.0",
                    "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0", "4.0.0.0"};
    private static void SetupAssemblyReferenceFor() {
      Version version = TargetPlatform.TargetVersion;
      if (version == null) version = typeof(object).Assembly.GetName().Version;
      TargetPlatform.SetTo(version);
    }
    public static void SetTo(Version/*!*/ version) {
      if (version == null) throw new ArgumentNullException();
      if (version.Major == 1) {
        if (version.Minor == 0 && version.Build == 3300) TargetPlatform.SetToV1();
        else if (version.Minor == 0 && version.Build == 5000) TargetPlatform.SetToV1_1();
        else if (version.Minor == 1 && version.Build == 9999) TargetPlatform.SetToPostV1_1(TargetPlatform.PlatformAssembliesLocation);
      } else if (version.Major == 2) {
        if (version.Minor == 0 && version.Build == 3600) TargetPlatform.SetToV2Beta1();
        else TargetPlatform.SetToV2();
      } else if (version.Major == 4) {
        if (version.Minor == 5) TargetPlatform.SetToV4_5();
        else TargetPlatform.SetToV4();
      } else
        TargetPlatform.SetToPostV2();      
    }
    public static void SetTo(Version/*!*/ version, string/*!*/ platformAssembliesLocation) {
      if (version == null || platformAssembliesLocation == null) throw new ArgumentNullException();
      if (version.Major == 1) {
        if (version.Minor == 0 && version.Build == 3300) TargetPlatform.SetToV1(platformAssembliesLocation);
        else if (version.Minor == 0 && version.Build == 5000) TargetPlatform.SetToV1_1(platformAssembliesLocation);
        else if (version.Minor == 1 && version.Build == 9999) TargetPlatform.SetToPostV1_1(platformAssembliesLocation);
      } else if (version.Major == 2) {
        if (version.Minor == 0 && version.Build == 3600) TargetPlatform.SetToV2Beta1(platformAssembliesLocation);
        else TargetPlatform.SetToV2(platformAssembliesLocation);
      } else if (version.Major == 4) {
          if (version.Minor == 5)
              TargetPlatform.SetToV4_5(platformAssembliesLocation);
          else
              TargetPlatform.SetToV4(platformAssembliesLocation);
      } else
        TargetPlatform.SetToPostV2(platformAssembliesLocation);
    }
    public static void SetToV1() {
      TargetPlatform.SetToV1(TargetPlatform.PlatformAssembliesLocation);
    }
    public static void SetToV1(string platformAssembliesLocation){
      TargetPlatform.TargetVersion = new Version(1, 0, 3300);
      TargetPlatform.TargetRuntimeVersion = "v1.0.3705";
      if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
        platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "..\\v1.0.3705");
      else
        TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
      for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++){
        string name = TargetPlatform.FxAssemblyNames[i];
        string version = TargetPlatform.FxAssemblyVersion1[i];
        string token = TargetPlatform.FxAssemblyToken[i];
        AssemblyReference aref = new AssemblyReference(name+", Version="+version+", Culture=neutral, PublicKeyToken="+token);
        aref.Location = platformAssembliesLocation+"\\"+name+".dll";
        //^ assume name != null;
        assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
      }
      TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    public static void SetToV1_1(){
      TargetPlatform.SetToV1_1(TargetPlatform.PlatformAssembliesLocation);
    }
    public static void SetToV1_1(string/*!*/ platformAssembliesLocation) {
      TargetPlatform.TargetVersion = new Version(1, 0, 5000);
      TargetPlatform.TargetRuntimeVersion = "v1.1.4322";
      if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
        platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "..\\v1.1.4322");
      else
        TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
      for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++){
        string name = TargetPlatform.FxAssemblyNames[i];
        string version = TargetPlatform.FxAssemblyVersion1_1[i];
        string token = TargetPlatform.FxAssemblyToken[i];
        AssemblyReference aref = new AssemblyReference(name+", Version="+version+", Culture=neutral, PublicKeyToken="+token);
        aref.Location = platformAssembliesLocation+"\\"+name+".dll";
        //^ assume name != null;
        assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
      }
      TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
    }
    public static void SetToV2(){
      TargetPlatform.SetToV2(TargetPlatform.PlatformAssembliesLocation);
    }
    public static void SetToV2(string platformAssembliesLocation){
      TargetPlatform.TargetVersion = new Version(2, 0, 50727);
      TargetPlatform.TargetRuntimeVersion = "v2.0.50727";
      TargetPlatform.GenericTypeNamesMangleChar = '`';
      if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
        platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "..\\v2.0.50727");
      else
        TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
      for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++){
        string name = TargetPlatform.FxAssemblyNames[i];
        string version = TargetPlatform.FxAssemblyVersion2[i];
        string token = TargetPlatform.FxAssemblyToken[i];
        AssemblyReference aref = new AssemblyReference(name+", Version="+version+", Culture=neutral, PublicKeyToken="+token);
        aref.Location = platformAssembliesLocation+"\\"+name+".dll";
        //^ assume name != null;
        assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
      }
      TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    public static void SetToV2Beta1(){
      TargetPlatform.SetToV2Beta1(TargetPlatform.PlatformAssembliesLocation);
    }
    public static void SetToV2Beta1(string/*!*/ platformAssembliesLocation) {
      TargetPlatform.TargetVersion = new Version(2, 0, 3600);
      TargetPlatform.GenericTypeNamesMangleChar = '!';
      string dotNetDirLocation = null;
      if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0) {
        DirectoryInfo dotNetDir = new FileInfo(new Uri(typeof(object).Assembly.Location).LocalPath).Directory.Parent;
        dotNetDirLocation = dotNetDir.FullName;
        if (dotNetDirLocation != null) dotNetDirLocation = dotNetDirLocation.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
        DateTime creationTime = DateTime.MinValue;
        foreach (DirectoryInfo subdir in dotNetDir.GetDirectories("v2.0*")) {
          if (subdir == null) continue;
          if (subdir.CreationTime < creationTime) continue;
          FileInfo[] mscorlibs = subdir.GetFiles("mscorlib.dll");
          if (mscorlibs != null && mscorlibs.Length == 1) {
            platformAssembliesLocation = subdir.FullName;
            creationTime = subdir.CreationTime;
          }
        }
      }else
        TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      if (dotNetDirLocation != null && (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)){
        int pos = dotNetDirLocation.IndexOf("FRAMEWORK");
        if (pos > 0 && dotNetDirLocation.IndexOf("FRAMEWORK64") < 0) {
          dotNetDirLocation = dotNetDirLocation.Replace("FRAMEWORK", "FRAMEWORK64");
          if (Directory.Exists(dotNetDirLocation)) {
            DirectoryInfo dotNetDir = new DirectoryInfo(dotNetDirLocation);
            DateTime creationTime = DateTime.MinValue;
            foreach (DirectoryInfo subdir in dotNetDir.GetDirectories("v2.0*")) {
              if (subdir == null) continue;
              if (subdir.CreationTime < creationTime) continue;
              FileInfo[] mscorlibs = subdir.GetFiles("mscorlib.dll");
              if (mscorlibs != null && mscorlibs.Length == 1) {
                platformAssembliesLocation = subdir.FullName;
                creationTime = subdir.CreationTime;
              }
            }
          }
        }
      }
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
      for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++) {
        string name = TargetPlatform.FxAssemblyNames[i];
        string version = TargetPlatform.FxAssemblyVersion2Build3600[i];
        string token = TargetPlatform.FxAssemblyToken[i];
        AssemblyReference aref = new AssemblyReference(name + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token);
        aref.Location = platformAssembliesLocation + "\\" + name + ".dll";
        //^ assume name != null;
        assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
      }
      TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    public static void SetToV4() {
      TargetPlatform.SetToV4(TargetPlatform.PlatformAssembliesLocation);
    }
    public static void SetToV4(string platformAssembliesLocation) {
      TargetPlatform.TargetVersion = new Version(4, 0);
      TargetPlatform.TargetRuntimeVersion = "v4.0.30319";
      TargetPlatform.GenericTypeNamesMangleChar = '`';
      if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
        platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "..\\v4.0.30319");
      else
        TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
      for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++) {
        string name = TargetPlatform.FxAssemblyNames[i];
        string version = TargetPlatform.FxAssemblyVersion4[i];
        string token = TargetPlatform.FxAssemblyToken[i];
        AssemblyReference aref = new AssemblyReference(name+", Version="+version+", Culture=neutral, PublicKeyToken="+token);
        aref.Location = platformAssembliesLocation+"\\"+name+".dll";
        //^ assume name != null;
        assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
      }
      TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    public static void SetToV4_5() {
      TargetPlatform.SetToV4_5(TargetPlatform.PlatformAssembliesLocation);
    }
    public static void SetToV4_5(string platformAssembliesLocation) {
      TargetPlatform.TargetVersion = new Version(4, 0);
      TargetPlatform.TargetRuntimeVersion = "v4.0.30319";
      TargetPlatform.GenericTypeNamesMangleChar = '`';
      if (platformAssembliesLocation == null || platformAssembliesLocation.Length == 0)
        platformAssembliesLocation = TargetPlatform.PlatformAssembliesLocation = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "..\\v4.0.30319");
      else
        TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, "System.Runtime", "mscorlib");
      TrivialHashtable assemblyReferenceFor = new TrivialHashtable(46);
      for (int i = 0, n = TargetPlatform.FxAssemblyNames.Length; i < n; i++) {
        string name = TargetPlatform.FxAssemblyNames[i];
        string version = TargetPlatform.FxAssemblyVersion4[i];
        string token = TargetPlatform.FxAssemblyToken[i];
        AssemblyReference aref = new AssemblyReference(name+", Version="+version+", Culture=neutral, PublicKeyToken="+token);
        aref.Location = platformAssembliesLocation+"\\"+name+".dll";
        //^ assume name != null;
        assemblyReferenceFor[Identifier.For(name).UniqueIdKey] = aref;
      }
      TargetPlatform.assemblyReferenceFor = assemblyReferenceFor;
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    public static void SetToPostV2() {
      TargetPlatform.SetToPostV2(TargetPlatform.PlatformAssembliesLocation);
    }
    /// <summary>
    /// Use this to set the target platform to a platform with a superset of the platform assemblies in version 2.0, but
    /// where the public key tokens and versions numbers are determined by reading in the actual assemblies from
    /// the supplied location. Only assemblies recognized as platform assemblies in version 2.0 will be unified.
    /// </summary>
    public static void SetToPostV2(string/*!*/ platformAssembliesLocation) {
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.TargetVersion = new Version(2, 1, 9999);
      TargetPlatform.TargetRuntimeVersion = "v2.1.9999";
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TargetPlatform.assemblyReferenceFor = new TrivialHashtable(46);
      string[] dlls = Directory.GetFiles(platformAssembliesLocation, "*.dll");
      foreach (string dll in dlls) {
        if (dll == null) continue;
        string assemName = Path.GetFileNameWithoutExtension(dll);
        int i = Array.IndexOf(TargetPlatform.FxAssemblyNames, assemName);
        if (i < 0) continue;
#if CodeContracts
        var loc = Path.Combine(platformAssembliesLocation, dll);
        var aref = new AssemblyReference(assemName);
        aref.Location = loc;
        TargetPlatform.assemblyReferenceFor[Identifier.For(assemName).UniqueIdKey] = aref;
#else
        AssemblyNode assem = AssemblyNode.GetAssembly(Path.Combine(platformAssembliesLocation, dll));
        if (assem == null) continue;
        TargetPlatform.assemblyReferenceFor[Identifier.For(assem.Name).UniqueIdKey] = new AssemblyReference(assem);
#endif
      }
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    /// <summary>
    /// Use this to set the target platform to a platform with a superset of the platform assemblies in version 1.1, but
    /// where the public key tokens and versions numbers are determined by reading in the actual assemblies from
    /// the supplied location. Only assemblies recognized as platform assemblies in version 1.1 will be unified.
    /// </summary>
    public static void SetToPostV1_1(string/*!*/ platformAssembliesLocation) {
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      TargetPlatform.TargetVersion = new Version(1, 1, 9999);
      TargetPlatform.TargetRuntimeVersion = "v1.1.9999";
      TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);
      TargetPlatform.assemblyReferenceFor = new TrivialHashtable(46);
      string[] dlls = Directory.GetFiles(platformAssembliesLocation, "*.dll");
      foreach (string dll in dlls){
        if (dll == null) continue;
        string assemName = Path.GetFileNameWithoutExtension(dll);
        int i = Array.IndexOf(TargetPlatform.FxAssemblyNames, assemName);
        if (i < 0) continue;
        AssemblyNode assem = AssemblyNode.GetAssembly(Path.Combine(platformAssembliesLocation, dll));
        if (assem == null) continue;
        TargetPlatform.assemblyReferenceFor[Identifier.For(assem.Name).UniqueIdKey] = new AssemblyReference(assem);
      }
#if ExtendedRuntime
      SystemCompilerRuntimeAssemblyLocation.Location = SystemCompilerRuntimeAssemblyLocation.Location;
#endif
    }
    private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation){
      InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, "mscorlib");
    }
    private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation, string mscorlibName, string alternateName)
    {
      var candidate = platformAssembliesLocation + "\\" + mscorlibName + ".dll";
      if (File.Exists(candidate))
      {
        InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, mscorlibName);
      }
      else
      {
        InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, alternateName);
      }
    }
    private static void InitializeStandardAssemblyLocationsWithDefaultValues(string platformAssembliesLocation, string mscorlibName) {
      SystemAssemblyLocation.Location = platformAssembliesLocation+"\\"+mscorlibName+".dll";
      if (SystemDllAssemblyLocation.Location == null)
        SystemDllAssemblyLocation.Location = platformAssembliesLocation+"\\system.dll";
#if ExtendedRuntime
      if (SystemCompilerRuntimeAssemblyLocation.Location == null)
#if CCINamespace
        SystemCompilerRuntimeAssemblyLocation.Location = platformAssembliesLocation+"\\Microsoft.Cci.Runtime.dll";
#else
        SystemCompilerRuntimeAssemblyLocation.Location = platformAssembliesLocation+"\\system.compiler.runtime.dll";
#endif
      // If the System.Compiler.Runtime assembly does not exist at this location, DO NOTHING (don't load another one)
      // as this signals the fact that the types may need to be loaded from the SystemAssembly instead.
#endif
#if !NoData && !ROTOR
      if (SystemDataAssemblyLocation.Location == null)
        SystemDataAssemblyLocation.Location = platformAssembliesLocation+"\\system.data.dll";
#endif
#if !NoXml && !NoRuntimeXml      
      if (SystemXmlAssemblyLocation.Location == null)
        SystemXmlAssemblyLocation.Location = platformAssembliesLocation+"\\system.xml.dll";
#endif
    }
    public static void ResetCci(string platformAssembliesLocation, Version targetVersion, bool doNotLockFile, bool getDebugInfo, AssemblyNode.PostAssemblyLoadProcessor postAssemblyLoad = null) {
      TargetPlatform.Clear();

      //Tell Initialize where to get the platform assemblies from
      TargetPlatform.PlatformAssembliesLocation = platformAssembliesLocation;
      if (targetVersion.Major == 4 && targetVersion.Minor >= 5) {
        TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation, "System.Runtime");
        targetVersion = new Version(4, 0, 0, 0);
      } else
        TargetPlatform.InitializeStandardAssemblyLocationsWithDefaultValues(platformAssembliesLocation);

      //Force Initialize to set the TargetVersion to the version of the SystemAssembly that gets loaded
      TargetPlatform.TargetVersion = targetVersion;        

      //Force Initialize to load the appropriate unification table for the SystemAssembly version it is loading
      TargetPlatform.assemblyReferenceFor = null;
        
      SystemTypes.Initialize(doNotLockFile, getDebugInfo, postAssemblyLoad);
    }
  }
#if ExtendedRuntime
  public sealed class ExtendedRuntimeTypes{ //TODO: move all types from System.Compiler.Runtime into here.
    public static AssemblyNode/*!*/ SystemCompilerRuntimeAssembly;

    public static Interface/*!*/ ConstrainedType;
    public static Interface/*!*/ ITemplateParameter;
    public static Interface/*!*/ ITemplate;
    public static Class/*!*/ NullableType;
    public static Class/*!*/ NonNullType;
    public static Class/*!*/ NotNullAttribute;
    public static Class/*!*/ NotNullArrayElementsAttribute;
    public static Class/*!*/ NotNullGenericArgumentsAttribute;
    public static Class/*!*/ DelayedAttribute;
    public static Class/*!*/ NotDelayedAttribute;
    public static Class/*!*/ EncodedTypeSpecAttribute;
    public static Class/*!*/ StrictReadonlyAttribute;
    public static Interface/*!*/ TupleType;
    public static Interface/*!*/ TypeAlias;
    public static Interface/*!*/ TypeDefinition;
    public static Interface/*!*/ TypeIntersection;
    public static Interface/*!*/ TypeUnion;

    public static Class OwnerClass {
      get {
        if (ownerClass == null) {
          ownerClass = (Class)GetCompilerRuntimeTypeNodeFor("Microsoft.Contracts", "Owner", ElementType.Class);
        }
        return ownerClass;
      }
    }
    private static Class ownerClass;

    public static Method nonNullTypeAssertInitialized;
    public static Method NonNullTypeAssertInitialized {
      get {
        if (nonNullTypeAssertInitialized == null) {
          if (NonNullType != null) {
            nonNullTypeAssertInitialized = NonNullType.GetMethod(Identifier.For("AssertInitialized"), SystemTypes.Object);
          }
        }
        return nonNullTypeAssertInitialized;
      }
    }

    public static Method nonNullTypeAssertInitializedGeneric;
    public static Method NonNullTypeAssertInitializedGeneric {
      get {
        if (nonNullTypeAssertInitializedGeneric != null) {
          return nonNullTypeAssertInitializedGeneric;
        }

        if (NonNullType != null) {
          MemberList ml = NonNullType.GetMembersNamed(Identifier.For("AssertInitialized"));
          if (ml != null && ml.Count > 0) {
            foreach (Member mem in ml) {
              Method m = mem as Method;
              if (m == null) continue;
              if (m.IsGeneric) {
                nonNullTypeAssertInitializedGeneric = m;
                break;
              }
            }
          }
        }

        return nonNullTypeAssertInitializedGeneric;
      }
    }
    static ExtendedRuntimeTypes(){
      ExtendedRuntimeTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
    }

    public static void Initialize(bool doNotLockFile, bool getDebugInfo) {
      SystemCompilerRuntimeAssembly = ExtendedRuntimeTypes.GetSystemCompilerRuntimeAssembly(doNotLockFile, getDebugInfo);
      if (SystemCompilerRuntimeAssembly == null) throw new InvalidOperationException(ExceptionStrings.InternalCompilerError);
#if ExtendedRuntime
      SystemTypes.UnifyMsCorlibReferences(SystemCompilerRuntimeAssembly);
#endif
#if CCINamespace
      const string CciNs = "Microsoft.Cci";
      const string ContractsNs = "Microsoft.Contracts";
      //const string CompilerGuardsNs = "Microsoft.Contracts";
#else
      const string CciNs = "System.Compiler";
      const string ContractsNs = "Microsoft.Contracts";
      //const string CompilerGuardsNs = "Microsoft.Contracts";
#endif
      ConstrainedType = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "IConstrainedType", ElementType.Class);
      ITemplateParameter = (Interface)GetCompilerRuntimeTypeNodeFor(CciNs, "ITemplateParameter", ElementType.Class);
      ITemplate = (Interface)GetCompilerRuntimeTypeNodeFor(CciNs, "ITemplate", ElementType.Class);
      NullableType = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NullableType", ElementType.Class);
      NonNullType = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NonNullType", ElementType.Class);
      NotNullAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotNullAttribute", ElementType.Class);
      NotNullArrayElementsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotNullArrayElementsAttribute", ElementType.Class);
      NotNullGenericArgumentsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotNullGenericArgumentsAttribute", ElementType.Class);
      DelayedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DelayedAttribute", ElementType.Class);
      NotDelayedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NotDelayedAttribute", ElementType.Class);
      EncodedTypeSpecAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "EncodedTypeSpecAttribute", ElementType.Class);
      StrictReadonlyAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "StrictReadonlyAttribute", ElementType.Class);
      TupleType = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITupleType", ElementType.Class);
      TypeAlias = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeAlias", ElementType.Class);
      TypeDefinition = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeDefinition", ElementType.Class);
      TypeIntersection = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeIntersection", ElementType.Class);
      TypeUnion = (Interface)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ITypeUnion", ElementType.Class);
    }

    public static void Clear(){
      lock (Module.GlobalLock){
        if (ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly != AssemblyNode.Dummy && ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly != null) {
          ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly.Dispose();
          ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly = null;
        }
        ConstrainedType = null;
        ITemplateParameter = null;
        ITemplate = null;
        NullableType = null;
        NonNullType = null;
        NotNullAttribute = null;
        NotNullArrayElementsAttribute = null;
        NotNullGenericArgumentsAttribute = null;
        DelayedAttribute = null;
        NotDelayedAttribute = null;
        EncodedTypeSpecAttribute = null;
        StrictReadonlyAttribute = null;
        TupleType = null;
        TypeAlias = null;
        TypeDefinition = null;
        TypeIntersection = null;
        TypeUnion = null;
        nonNullTypeAssertInitialized = null;
        nonNullTypeAssertInitializedGeneric = null;
      }
    }
    private static AssemblyNode/*!*/ GetSystemCompilerRuntimeAssembly(bool doNotLockFile, bool getDebugInfo)
    {
      if (SystemCompilerRuntimeAssemblyLocation.ParsedAssembly != null) 
        return SystemCompilerRuntimeAssemblyLocation.ParsedAssembly;
      if (SystemCompilerRuntimeAssemblyLocation.Location == null || SystemCompilerRuntimeAssemblyLocation.Location.Length == 0)
        SystemCompilerRuntimeAssemblyLocation.Location = typeof(ComposerAttribute).Assembly.Location;
      AssemblyNode result = null;
      if (CoreSystemTypes.SystemAssembly.GetType(Identifier.For("Microsoft.Contracts"), Identifier.For("NonNullType")) != null) {
          result = SystemTypes.SystemAssembly;
      }
      if (result == null) {
        result = (AssemblyNode)(new Reader(SystemCompilerRuntimeAssemblyLocation.Location, SystemAssemblyLocation.SystemAssemblyCache, doNotLockFile, getDebugInfo, true, false)).ReadModule();
      }
      if (result == null) {
        SystemCompilerRuntimeAssemblyLocation.Location = typeof(ComposerAttribute).Assembly.Location;
        result = (AssemblyNode)(new Reader(SystemCompilerRuntimeAssemblyLocation.Location, SystemAssemblyLocation.SystemAssemblyCache, doNotLockFile, getDebugInfo, true, false)).ReadModule();
      }
      if (result == null) {
        result = new AssemblyNode();
        System.Reflection.AssemblyName aname = typeof(ComposerAttribute).Assembly.GetName();
        result.Name = aname.Name;
        result.Version = aname.Version;
        result.PublicKeyOrToken = aname.GetPublicKeyToken();
      }
      TargetPlatform.AssemblyReferenceFor[Identifier.For(result.Name).UniqueIdKey] = new AssemblyReference(result);
      return result;
    }
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      return ExtendedRuntimeTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, 0, typeCode);
    }
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0 && numParams > 0)
        name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
      TypeNode result = null;
      if (SystemCompilerRuntimeAssembly == null)
        Debug.Assert(false);
      else
        result = SystemCompilerRuntimeAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemCompilerRuntimeAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
  }
#endif
#if !FxCop
  public
#endif
  sealed class CoreSystemTypes{
    private CoreSystemTypes(){}
    internal static bool Initialized;
    internal static bool doNotLockFile;
    internal static bool getDebugInfo;
    internal static AssemblyNode.PostAssemblyLoadProcessor postAssemblyLoad;
    internal static bool IsInitialized { get { return Initialized; } }
    //system assembly (the basic runtime)
    public static AssemblyNode/*!*/ SystemAssembly;

    //Special base types
    public static Class/*!*/ Object;
    public static Class/*!*/ String;
    public static Class/*!*/ ValueType;
    public static Class/*!*/ Enum;
    public static Class/*!*/ MulticastDelegate;
    public static Class/*!*/ Array;
    public static Class/*!*/ Type;
#if !FxCop
    public static Class/*!*/ Delegate;
    public static Class/*!*/ Exception;
    public static Class/*!*/ Attribute;
#endif
    //primitive types
    public static Struct/*!*/ Boolean;
    public static Struct/*!*/ Char;
    public static Struct/*!*/ Int8;
    public static Struct/*!*/ UInt8;
    public static Struct/*!*/ Int16;
    public static Struct/*!*/ UInt16;
    public static Struct/*!*/ Int32;
    public static Struct/*!*/ UInt32;
    public static Struct/*!*/ Int64;
    public static Struct/*!*/ UInt64;
    public static Struct/*!*/ Single;
    public static Struct/*!*/ Double;
    public static Struct/*!*/ IntPtr;
    public static Struct/*!*/ UIntPtr;
    public static Struct/*!*/ DynamicallyTypedReference;

#if !MinimalReader
    //Classes need for System.TypeCode
    public static Class/*!*/ DBNull;
    public static Struct/*!*/ DateTime;
    public static Struct/*!*/ Decimal;
#endif

    //Special types
    public static Class/*!*/ IsVolatile;
    public static Struct/*!*/ Void;
    public static Struct/*!*/ ArgIterator;
    public static Struct/*!*/ RuntimeFieldHandle;
    public static Struct/*!*/ RuntimeMethodHandle;
    public static Struct/*!*/ RuntimeTypeHandle;
#if !MinimalReader
    public static Struct/*!*/ RuntimeArgumentHandle;
#endif   
    //Special attributes    
    public static EnumNode SecurityAction;

    static CoreSystemTypes(){
      CoreSystemTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
    }

    public static void Clear(){
      lock (Module.GlobalLock){
        if (Reader.StaticAssemblyCache != null){
          foreach (AssemblyNode cachedAssembly in new System.Collections.ArrayList(Reader.StaticAssemblyCache.Values)){
            if (cachedAssembly != null) cachedAssembly.Dispose();
          }
          Reader.StaticAssemblyCache.Clear();
        }
        //Dispose the system assemblies in case they were not in the static cache. It is safe to dispose an assembly more than once.
        if (CoreSystemTypes.SystemAssembly != null && CoreSystemTypes.SystemAssembly != AssemblyNode.Dummy){
          CoreSystemTypes.SystemAssembly.Dispose();
          CoreSystemTypes.SystemAssembly = null;
        }
        CoreSystemTypes.ClearStatics();
        CoreSystemTypes.Initialized = false;
        TargetPlatform.AssemblyReferenceFor = new TrivialHashtable(0);
      }
    }
    public static void Initialize(bool doNotLockFile, bool getDebugInfo, AssemblyNode.PostAssemblyLoadProcessor postAssemblyLoad = null){
      CoreSystemTypes.doNotLockFile = doNotLockFile;
      CoreSystemTypes.getDebugInfo = getDebugInfo;
      CoreSystemTypes.postAssemblyLoad = postAssemblyLoad;
      if (CoreSystemTypes.Initialized) CoreSystemTypes.Clear();
      if (SystemAssembly == null)
        SystemAssembly = CoreSystemTypes.GetSystemAssembly(doNotLockFile, getDebugInfo, postAssemblyLoad);
      if (SystemAssembly == null) throw new InvalidOperationException(ExceptionStrings.InternalCompilerError);
      if (TargetPlatform.TargetVersion == null){
        TargetPlatform.TargetVersion = SystemAssembly.Version;
        if (TargetPlatform.TargetVersion == null)
          TargetPlatform.TargetVersion = typeof(object).Assembly.GetName().Version;
      }
      if (TargetPlatform.TargetVersion != null){
        if (TargetPlatform.TargetVersion.Major > 1 || TargetPlatform.TargetVersion.Minor > 1 ||
          (TargetPlatform.TargetVersion.Minor == 1 && TargetPlatform.TargetVersion.Build == 9999)){
          if (SystemAssembly.IsValidTypeName(StandardIds.System, Identifier.For("Nullable`1")))
            TargetPlatform.GenericTypeNamesMangleChar = '`';
          else if (SystemAssembly.IsValidTypeName(StandardIds.System, Identifier.For("Nullable!1")))
            TargetPlatform.GenericTypeNamesMangleChar = '!';
          else if (TargetPlatform.TargetVersion.Major == 1 && TargetPlatform.TargetVersion.Minor == 2)
            TargetPlatform.GenericTypeNamesMangleChar = (char)0;
        }
      }
      // This must be done in the order: Object, ValueType, Char, String
      // or else some of the generic type instantiations don't get filled
      // in correctly. (String ends up implementing IEnumerable<string>
      // instead of IEnumerable<char>.)
      Object = (Class)GetTypeNodeFor("System", "Object", ElementType.Object);
      ValueType = (Class)GetTypeNodeFor("System", "ValueType", ElementType.Class);
      Char = (Struct)GetTypeNodeFor("System", "Char", ElementType.Char);
      String = (Class)GetTypeNodeFor("System", "String", ElementType.String);
      Enum = (Class)GetTypeNodeFor("System", "Enum", ElementType.Class);
      MulticastDelegate = (Class)GetTypeNodeFor("System", "MulticastDelegate", ElementType.Class);
      Array = (Class)GetTypeNodeFor("System", "Array", ElementType.Class);
      Type = (Class)GetTypeNodeFor("System", "Type", ElementType.Class);
      Boolean = (Struct)GetTypeNodeFor("System", "Boolean", ElementType.Boolean);
      Int8 = (Struct)GetTypeNodeFor("System", "SByte", ElementType.Int8);
      UInt8 = (Struct)GetTypeNodeFor("System", "Byte", ElementType.UInt8);
      Int16 = (Struct)GetTypeNodeFor("System", "Int16", ElementType.Int16);
      UInt16 = (Struct)GetTypeNodeFor("System", "UInt16", ElementType.UInt16);
      Int32 = (Struct)GetTypeNodeFor("System", "Int32", ElementType.Int32);
      UInt32 = (Struct)GetTypeNodeFor("System", "UInt32", ElementType.UInt32);
      Int64 = (Struct)GetTypeNodeFor("System", "Int64", ElementType.Int64);
      UInt64 = (Struct)GetTypeNodeFor("System", "UInt64", ElementType.UInt64);
      Single = (Struct)GetTypeNodeFor("System", "Single", ElementType.Single);
      Double = (Struct)GetTypeNodeFor("System", "Double", ElementType.Double);
      IntPtr = (Struct)GetTypeNodeFor("System", "IntPtr", ElementType.IntPtr);
      UIntPtr = (Struct)GetTypeNodeFor("System", "UIntPtr", ElementType.UIntPtr);
      DynamicallyTypedReference = (Struct)GetTypeNodeFor("System", "TypedReference", ElementType.DynamicallyTypedReference);
#if !MinimalReader
      Delegate = (Class)GetTypeNodeFor("System", "Delegate", ElementType.Class);
      Exception = (Class)GetTypeNodeFor("System", "Exception", ElementType.Class);
      Attribute = (Class)GetTypeNodeFor("System", "Attribute", ElementType.Class);
      DBNull = (Class)GetTypeNodeFor("System", "DBNull", ElementType.Class);  //Where does this mscorlib type live in the new world of reference assemblies?
      DateTime = (Struct)GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
      Decimal = (Struct)GetTypeNodeFor("System", "Decimal", ElementType.ValueType);
#endif
      ArgIterator = (Struct)GetTypeNodeFor("System", "ArgIterator", ElementType.ValueType); //Where does this mscorlib type live in the new world of reference assemblies?
      IsVolatile = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "IsVolatile", ElementType.Class);
      Void = (Struct)GetTypeNodeFor("System", "Void", ElementType.Void);
      RuntimeFieldHandle = (Struct)GetTypeNodeFor("System", "RuntimeFieldHandle", ElementType.ValueType);
      RuntimeMethodHandle = (Struct)GetTypeNodeFor("System", "RuntimeMethodHandle", ElementType.ValueType);
      RuntimeTypeHandle = (Struct)GetTypeNodeFor("System", "RuntimeTypeHandle", ElementType.ValueType);
#if !MinimalReader
      RuntimeArgumentHandle = (Struct)GetTypeNodeFor("System", "RuntimeArgumentHandle", ElementType.ValueType); //Where does this mscorlib type live in the new world of reference assemblies?
#endif
      SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode; //Where does this mscorlib type live in the new world of reference assemblies?
      CoreSystemTypes.Initialized = true;
      CoreSystemTypes.InstantiateGenericInterfaces();
#if !NoWriter
      Literal.Initialize();
#endif
      object dummy = TargetPlatform.AssemblyReferenceFor; //Force selection of target platform
      if (dummy == null) return;
    }
    private static void ClearStatics(){
      //Special base types
      Object = null;
      String = null;
      ValueType = null;
      Enum = null;
      MulticastDelegate = null;
      Array = null;
      Type = null;
#if !MinimalReader
      Delegate = null;
      Exception = null;
      Attribute = null;
#endif
      //primitive types
      Boolean = null;
      Char = null;
      Int8 = null;
      UInt8 = null;
      Int16 = null;
      UInt16 = null;
      Int32 = null;
      UInt32 = null;
      Int64 = null;
      UInt64 = null;
      Single = null;
      Double = null;
      IntPtr = null;
      UIntPtr = null;
      DynamicallyTypedReference = null;

      //Special types
#if !MinimalReader
      DBNull = null;
      DateTime = null;
      Decimal = null;
      RuntimeArgumentHandle = null;
#endif
      ArgIterator = null;
      RuntimeFieldHandle = null;
      RuntimeMethodHandle = null;
      RuntimeTypeHandle = null;
      IsVolatile = null;
      Void = null;
      SecurityAction = null;
    }
    private static void InstantiateGenericInterfaces(){
      if (TargetPlatform.TargetVersion != null && (TargetPlatform.TargetVersion.Major < 2 && TargetPlatform.TargetVersion.Minor < 2)) return;
      InstantiateGenericInterfaces(String);
      InstantiateGenericInterfaces(Boolean);
      InstantiateGenericInterfaces(Char);
      InstantiateGenericInterfaces(Int8);
      InstantiateGenericInterfaces(UInt8);
      InstantiateGenericInterfaces(Int16);
      InstantiateGenericInterfaces(UInt16);
      InstantiateGenericInterfaces(Int32);
      InstantiateGenericInterfaces(UInt32);
      InstantiateGenericInterfaces(Int64);
      InstantiateGenericInterfaces(UInt64);
      InstantiateGenericInterfaces(Single);
      InstantiateGenericInterfaces(Double);
#if !MinimalReader
      InstantiateGenericInterfaces(DBNull);
      InstantiateGenericInterfaces(DateTime);
      InstantiateGenericInterfaces(Decimal);
#endif
    }
    private static void InstantiateGenericInterfaces(TypeNode type){
      if (type == null) return;
      InterfaceList interfaces = type.Interfaces;
      for (int i = 0, n = interfaces == null ? 0 : interfaces.Count; i < n; i++){
        InterfaceExpression ifaceExpr = interfaces[i] as InterfaceExpression;
        if (ifaceExpr == null) continue;
        if (ifaceExpr.Template == null) {Debug.Assert(false); continue;}
        TypeNodeList templArgs = ifaceExpr.TemplateArguments;
        for (int j = 0, m = templArgs.Count; j < m; j++){
          InterfaceExpression ie = templArgs[j] as InterfaceExpression;
          if (ie != null) 
            templArgs[j] = ie.Template.GetGenericTemplateInstance(type.DeclaringModule, ie.ConsolidatedTemplateArguments);
        }
        interfaces[i] = (Interface)ifaceExpr.Template.GetGenericTemplateInstance(type.DeclaringModule, ifaceExpr.ConsolidatedTemplateArguments);
      }
    }

    private static AssemblyNode/*!*/ GetSystemAssembly(bool doNotLockFile, bool getDebugInfo, AssemblyNode.PostAssemblyLoadProcessor postAssemblyLoad) {
      AssemblyNode result = SystemAssemblyLocation.ParsedAssembly;
      if (result != null) {
        result.TargetRuntimeVersion = TargetPlatform.TargetRuntimeVersion;
        result.MetadataFormatMajorVersion = 1;
        result.MetadataFormatMinorVersion = 1;
        result.LinkerMajorVersion = 8;
        result.LinkerMinorVersion = 0;
        return result;
      }
      if (string.IsNullOrEmpty(SystemAssemblyLocation.Location))
        SystemAssemblyLocation.Location = typeof(object).Assembly.Location;
      result = (AssemblyNode)(new Reader(SystemAssemblyLocation.Location, SystemAssemblyLocation.SystemAssemblyCache, doNotLockFile, getDebugInfo, true, false)).ReadModule(postAssemblyLoad);
      if (result == null && TargetPlatform.TargetVersion != null && TargetPlatform.TargetVersion == typeof(object).Assembly.GetName().Version){
        SystemAssemblyLocation.Location = typeof(object).Assembly.Location;
        result = (AssemblyNode)(new Reader(SystemAssemblyLocation.Location, SystemAssemblyLocation.SystemAssemblyCache, doNotLockFile, getDebugInfo, true, false)).ReadModule(postAssemblyLoad);
      }
      if (result == null){
        result = new AssemblyNode();
        System.Reflection.AssemblyName aname = typeof(object).Assembly.GetName();
        result.Name = aname.Name;
        result.Version = TargetPlatform.TargetVersion;
        result.PublicKeyOrToken = aname.GetPublicKeyToken();
      }
      TargetPlatform.TargetVersion = result.Version;
      TargetPlatform.TargetRuntimeVersion = result.TargetRuntimeVersion;
      return result;
    }
    private static TypeNode/*!*/ GetTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemAssembly == null)
        Debug.Assert(false);
      else
        result = SystemAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }

    internal static TypeNode/*!*/ GetDummyTypeNode(AssemblyNode declaringAssembly, string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      switch (typeCode) {
        case ElementType.Object:
        case ElementType.String:
        case ElementType.Class:
          if (name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
            result = new Interface();
          else if (name == "MulticastDelegate" || name == "Delegate")
            result = new Class();
          else if (name.EndsWith("Callback") || name.EndsWith("Handler") || name.EndsWith("Delegate") || name == "ThreadStart" || name == "FrameGuardGetter" || name == "GuardThreadStart")
            result = new DelegateNode();
          else
            result = new Class();
          break;
        default:
          if (name == "CciMemberKind")
            result = new EnumNode();
          else
            result = new Struct();
          break;
      }
      result.Name = Identifier.For(name);
      result.Namespace = Identifier.For(nspace);
      result.DeclaringModule = declaringAssembly;

      return result;
    }
  }
  internal struct Delayed<T>
    where T:TypeNode
  {
    T cached;
    Func<TypeNode> delayed;

    public Delayed(Func<TypeNode> delayed)
    {
      this.delayed = delayed;
      this.cached = null;
    }

    public static implicit operator T(Delayed<T> delayed)
    {
      return delayed.GetValue();
    }

    public static implicit operator Delayed<T>(Func<TypeNode> delayed)
    {
      return new Delayed<T>(delayed);
    }


    private T GetValue()
    {
      if (cached == null && this.delayed != null)
      {
        this.cached = (T)this.delayed();
        this.delayed = null;
      }
      return this.cached;
    }

    public void Clear()
    {
      this.cached = null;
      this.delayed = null;
    }
  }
#if !FxCop
  public
#endif
  sealed class SystemTypes{
    private SystemTypes(){}
    internal static bool Initialized;
    public static bool IsInitialized { get { return Initialized; } }
    //system assembly (the basic runtime)
    public static AssemblyNode/*!*/ SystemAssembly {
      get{
          Contract.Ensures(Contract.Result<AssemblyNode>() != null);
          return CoreSystemTypes.SystemAssembly;
      }
      set{CoreSystemTypes.SystemAssembly = value;}
    }
#if ExtendedRuntime
    public static AssemblyNode/*!*/ SystemCompilerRuntimeAssembly {
      get{return ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly;}
      set{ExtendedRuntimeTypes.SystemCompilerRuntimeAssembly = value;}
    }
#if !NoData
    public static AssemblyNode/*!*/ SystemDataAssembly;
#endif
#if !NoXml && !NoRuntimeXml
    public static AssemblyNode /*!*/SystemXmlAssembly;
#endif
#endif
    public static AssemblyNode/*!*/ CollectionsAssembly;
    public static AssemblyNode/*!*/ DiagnosticsDebugAssembly;
    public static AssemblyNode/*!*/ DiagnosticsToolsAssembly;
    public static AssemblyNode/*!*/ GlobalizationAssembly;
    public static AssemblyNode/*!*/ InteropAssembly;
    public static AssemblyNode/*!*/ IOAssembly;
    public static AssemblyNode/*!*/ ReflectionAssembly;
    public static AssemblyNode/*!*/ ResourceManagerAssembly;
    public static AssemblyNode/*!*/ SystemDllAssembly;
    public static AssemblyNode/*!*/ SystemRuntimeExtensionsAssembly;
    public static AssemblyNode/*!*/ SystemRuntimeSerializationAssembly;
    private static AssemblyNode systemRuntimeWindowsRuntimeAssembly;
    public static AssemblyNode/*!*/ SystemRuntimeWindowsRuntimeAssembly
    {
      get
      {
        if (systemRuntimeWindowsRuntimeAssembly == null) {
          systemRuntimeWindowsRuntimeAssembly = SystemTypes.GetSystemRuntimeWindowsRuntimeAssembly(CoreSystemTypes.doNotLockFile, CoreSystemTypes.getDebugInfo);
        }
        return systemRuntimeWindowsRuntimeAssembly;
      }
    }
    public static AssemblyNode/*!*/ SystemRuntimeWindowsRuntimeInteropAssembly;
    private static AssemblyNode/*!*/ systemRuntimeWindowsRuntimeUIXamlAssembly;
    public static AssemblyNode/*!*/ SystemRuntimeWindowsRuntimeUIXamlAssembly
    {
      get
      {
        if (systemRuntimeWindowsRuntimeUIXamlAssembly == null)
        {
          systemRuntimeWindowsRuntimeUIXamlAssembly = SystemTypes.GetSystemRuntimeWindowsRuntimeUIXamlAssembly(CoreSystemTypes.doNotLockFile, CoreSystemTypes.getDebugInfo);
        }
        return systemRuntimeWindowsRuntimeUIXamlAssembly;
      }
    }
    public static AssemblyNode/*!*/ ThreadingAssembly;
#if !FxCop
    //Special base types
    public static Class/*!*/ Object { 
        get {
            Contract.Ensures(Contract.Result<Class>() != null);
            return CoreSystemTypes.Object; 
        } 
    }
    public static Class/*!*/ String { 
        get {
            Contract.Ensures(Contract.Result<Class>() != null);
            return CoreSystemTypes.String; 
        } 
    }
    public static Class/*!*/ ValueType { get { return CoreSystemTypes.ValueType; } }
    public static Class/*!*/ Enum { get { return CoreSystemTypes.Enum; } }
    public static Class/*!*/ Delegate { get { return CoreSystemTypes.Delegate; } }
    public static Class/*!*/ MulticastDelegate { get { return CoreSystemTypes.MulticastDelegate; } }
    public static Class/*!*/ Array { get { return CoreSystemTypes.Array; } }
    public static Class/*!*/ Type { get { return CoreSystemTypes.Type; } }
    public static Class/*!*/ Exception { get { return CoreSystemTypes.Exception; } }
    public static Class/*!*/ Attribute { get { return CoreSystemTypes.Attribute; } }

    //primitive types
    public static Struct/*!*/ Boolean { get { return CoreSystemTypes.Boolean; } }
    public static Struct/*!*/ Char { get { return CoreSystemTypes.Char; } }
    public static Struct/*!*/ Int8 { get { return CoreSystemTypes.Int8; } }
    public static Struct/*!*/ UInt8 { get { return CoreSystemTypes.UInt8; } }
    public static Struct/*!*/ Int16 { get { return CoreSystemTypes.Int16; } }
    public static Struct/*!*/ UInt16 { get { return CoreSystemTypes.UInt16; } }
    public static Struct/*!*/ Int32 { get { return CoreSystemTypes.Int32; } }
    public static Struct/*!*/ UInt32 { get { return CoreSystemTypes.UInt32; } }
    public static Struct/*!*/ Int64 { get { return CoreSystemTypes.Int64; } }
    public static Struct/*!*/ UInt64 { get { return CoreSystemTypes.UInt64; } }
    public static Struct/*!*/ Single { get { return CoreSystemTypes.Single; } }
    public static Struct/*!*/ Double { get { return CoreSystemTypes.Double; } }
    public static Struct/*!*/ IntPtr { get { return CoreSystemTypes.IntPtr; } }
    public static Struct/*!*/ UIntPtr { get { return CoreSystemTypes.UIntPtr; } }
    public static Struct/*!*/ DynamicallyTypedReference { get { return CoreSystemTypes.DynamicallyTypedReference; } }
#endif

    // Types required for a complete rendering
    // of binary attribute information
    public static Class/*!*/ AttributeUsageAttribute;
    public static Class/*!*/ ConditionalAttribute;
    public static Class/*!*/ DefaultMemberAttribute;
    public static Class/*!*/ InternalsVisibleToAttribute;
    public static Class/*!*/ ObsoleteAttribute;

    // Types required to render arrays
    public static Interface/*!*/ GenericICollection;
    public static Interface/*!*/ GenericIEnumerable;
    public static Interface/*!*/ GenericIList;
    public static Interface/*!*/ ICloneable;
    public static Interface/*!*/ ICollection;
    public static Interface/*!*/ IEnumerable;
    public static Interface/*!*/ IList;

#if !MinimalReader
    //Special types
    public static Struct/*!*/ ArgIterator { get { return CoreSystemTypes.ArgIterator; } }
    public static Class/*!*/ IsVolatile { get { return CoreSystemTypes.IsVolatile; } }
    public static Struct/*!*/ Void { get { return CoreSystemTypes.Void; } }
    public static Struct/*!*/ RuntimeFieldHandle { get { return CoreSystemTypes.RuntimeTypeHandle; } }
    public static Struct/*!*/ RuntimeMethodHandle { get { return CoreSystemTypes.RuntimeTypeHandle; } }
    public static Struct/*!*/ RuntimeTypeHandle { get { return CoreSystemTypes.RuntimeTypeHandle; } }
    public static Struct/*!*/ RuntimeArgumentHandle { get { return CoreSystemTypes.RuntimeArgumentHandle; } }
   
    //Special attributes    
    public static Class/*!*/ AllowPartiallyTrustedCallersAttribute;
    public static Class/*!*/ AssemblyCompanyAttribute;
    public static Class/*!*/ AssemblyConfigurationAttribute;
    public static Class/*!*/ AssemblyCopyrightAttribute;
    public static Class/*!*/ AssemblyCultureAttribute;
    public static Class/*!*/ AssemblyDelaySignAttribute;
    public static Class/*!*/ AssemblyDescriptionAttribute;
    public static Class/*!*/ AssemblyFileVersionAttribute;
    public static Class/*!*/ AssemblyFlagsAttribute;
    public static Class/*!*/ AssemblyInformationalVersionAttribute;
    public static Class/*!*/ AssemblyKeyFileAttribute;
    public static Class/*!*/ AssemblyKeyNameAttribute;
    public static Class/*!*/ AssemblyProductAttribute;
    public static Class/*!*/ AssemblyTitleAttribute;
    public static Class/*!*/ AssemblyTrademarkAttribute;
    public static Class/*!*/ AssemblyVersionAttribute;
    public static Class/*!*/ ClassInterfaceAttribute;
    public static Class/*!*/ CLSCompliantAttribute;
    public static Class/*!*/ ComImportAttribute;
    public static Class/*!*/ ComRegisterFunctionAttribute;
    public static Class/*!*/ ComSourceInterfacesAttribute;
    public static Class/*!*/ ComUnregisterFunctionAttribute;
    public static Class/*!*/ ComVisibleAttribute;
    public static Class/*!*/ DebuggableAttribute;
    public static Class/*!*/ DebuggerHiddenAttribute;
    public static Class/*!*/ DebuggerStepThroughAttribute;
    public static EnumNode/*?*/ DebuggingModes;
    public static Class/*!*/ DllImportAttribute;
    public static Class/*!*/ FieldOffsetAttribute;
    public static Class/*!*/ FlagsAttribute;
    public static Class/*!*/ GuidAttribute;
    public static Class/*!*/ ImportedFromTypeLibAttribute;
    public static Class/*!*/ InAttribute;
    public static Class/*!*/ IndexerNameAttribute;
    public static Class/*!*/ InterfaceTypeAttribute;
    public static Class/*!*/ MethodImplAttribute;
    public static Class/*!*/ NonSerializedAttribute;
    public static Class/*!*/ OptionalAttribute;
    public static Class/*!*/ OutAttribute;
    public static Class/*!*/ ParamArrayAttribute;
    public static Class/*!*/ RuntimeCompatibilityAttribute;
    public static Class/*!*/ SatelliteContractVersionAttribute;
    public static Class/*!*/ SerializableAttribute;
    public static Class/*!*/ SecurityAttribute;
    public static Class/*!*/ SecurityCriticalAttribute;
    public static Class/*!*/ SecurityTransparentAttribute;
    public static Class/*!*/ SecurityTreatAsSafeAttribute;
    public static Class/*!*/ STAThreadAttribute;
    public static Class/*!*/ StructLayoutAttribute;
    public static Class/*!*/ SuppressMessageAttribute;
    public static Class/*!*/ SuppressUnmanagedCodeSecurityAttribute;
    public static EnumNode/*?*/ SecurityAction;

    //Classes need for System.TypeCode
    public static Class/*!*/ DBNull;
    public static Struct/*!*/ DateTime;
    public static Struct/*!*/ DateTimeOffset;
    public static Struct/*!*/ Decimal { get { return CoreSystemTypes.Decimal; } }
    public static Struct/*!*/ TimeSpan;

    //Classes and interfaces used by the Framework
    public static Class/*!*/ Activator;
    public static Class/*!*/ AppDomain;
    public static Class/*!*/ ApplicationException;
    public static Class/*!*/ ArgumentException;
    public static Class/*!*/ ArgumentNullException;
    public static Class/*!*/ ArgumentOutOfRangeException;
    public static Class/*!*/ ArrayList;
    public static DelegateNode/*!*/ AsyncCallback;
    public static Class/*!*/ Assembly;
    public static EnumNode/*?*/ AttributeTargets;
    public static Class/*!*/ CodeAccessPermission;
    public static Class/*!*/ CollectionBase;
    static Delayed<Struct>/*!*/ color;
    public static Struct Color { get { return color; } }
    static Delayed<Struct> cornerRadius;
    public static Struct/*!*/ CornerRadius { get { return cornerRadius; } }
    public static Class/*!*/ CultureInfo;
    public static Class/*!*/ DictionaryBase;
    public static Struct/*!*/ DictionaryEntry;
    public static Class/*!*/ DuplicateWaitObjectException;
    static Delayed<Struct> duration;
    public static Struct/*!*/ Duration { get { return duration; } }
    static Delayed<EnumNode> durationType;
    public static EnumNode/*?*/ DurationType { get { return durationType; } }
    public static Class/*!*/ Environment;
    public static Class/*!*/ EventArgs;
    public static DelegateNode/*?*/ EventHandler1;
    public static Struct/*!*/ EventRegistrationToken;
    public static Class/*!*/ ExecutionEngineException;
    static Delayed<Struct> generatorPosition;
    public static Struct/*!*/ GeneratorPosition { get { return generatorPosition; } }
    public static Struct/*!*/ GenericArraySegment;
#if !WHIDBEYwithGenerics
    public static Class/*!*/ GenericArrayToIEnumerableAdapter;
#endif
    public static Class/*!*/ GenericDictionary;
    public static Interface/*!*/ GenericIComparable;
    public static Interface/*!*/ GenericIComparer;
    public static Interface/*!*/ GenericIDictionary;
    public static Interface/*!*/ GenericIEnumerator;
    public static Interface/*!*/ GenericIReadOnlyList;
    public static Interface/*!*/ GenericIReadOnlyDictionary;
    public static Struct/*!*/ GenericKeyValuePair;
    public static Class/*!*/ GenericList;
    public static Struct/*!*/ GenericNullable;
    public static Class/*!*/ GenericQueue;
    public static Class/*!*/ GenericSortedDictionary;
    public static Class/*!*/ GenericStack;
    public static Class/*!*/ GC;
    static Delayed<Struct> gridLength;
    public static Struct/*!*/ GridLength { get { return gridLength; } }
    static Delayed<EnumNode> gridUnitType;
    public static EnumNode/*?*/ GridUnitType { get { return gridUnitType; } }
    public static Struct/*!*/ Guid;
    public static Class/*!*/ __HandleProtector;
    public static Struct/*!*/ HandleRef;
    public static Class/*!*/ Hashtable;
    public static Interface/*!*/ IASyncResult;
    public static Interface/*!*/ ICommand;
    public static Interface/*!*/ IComparable;
    public static Interface/*!*/ IDictionary;
    public static Interface/*!*/ IComparer;
    public static Interface/*!*/ IDisposable;
    public static Interface/*!*/ IEnumerator;
    public static Interface/*!*/ IFormatProvider;
    public static Interface/*!*/ IHashCodeProvider;
    public static Interface/*!*/ IMembershipCondition;
    public static Interface/*!*/ INotifyPropertyChanged;
    public static Interface/*!*/ IBindableIterable;
    public static Interface/*!*/ IBindableVector;
    public static Interface/*!*/ INotifyCollectionChanged;
    public static Class/*!*/ IndexOutOfRangeException;
    public static Class/*!*/ InvalidCastException;
    public static Class/*!*/ InvalidOperationException;
    public static Interface/*!*/ IPermission;
    public static Interface/*!*/ ISerializable;
    public static Interface/*!*/ IStackWalk;
    static Delayed<Struct> keyTime;
    public static Struct/*!*/ KeyTime { get { return keyTime; } }
    public static Class/*!*/ Marshal;
    public static Class/*!*/ MarshalByRefObject;
    static Delayed<Struct> matrix;
    public static Struct/*!*/ Matrix { get { return matrix; } }
    static Delayed<Struct> matrix3D;
    public static Struct/*!*/ Matrix3D { get { return matrix3D; } }
    public static Class/*!*/ MemberInfo;
    public static Struct/*!*/ NativeOverlapped;
    public static Class/*!*/ Monitor;
    public static EnumNode/*?*/ NotifyCollectionChangedAction;
    public static Class/*!*/ NotifyCollectionChangedEventArgs;
    public static DelegateNode/*!*/ NotifyCollectionChangedEventHandler;
    public static Class/*!*/ NotSupportedException;
    public static Class/*!*/ NullReferenceException;
    public static Class/*!*/ OutOfMemoryException;
    public static Class/*!*/ ParameterInfo;
    public static Class/*!*/ PropertyChangedEventArgs;
    public static DelegateNode/*!*/ PropertyChangedEventHandler;
    static Delayed<Struct>/*!*/ point;
    public static Struct/*!*/ Point { get { return point; } }
    public static Class/*!*/ Queue;
    public static Class/*!*/ ReadOnlyCollectionBase;
    static Delayed<Struct>/*!*/ rect;
    public static Struct/*!*/ Rect { get { return rect; } }
    static Delayed<Struct> repeatBehavior;
    public static Struct/*!*/ RepeatBehavior { get { return repeatBehavior; } }
    static Delayed<EnumNode> repeatBehaviorType;
    public static EnumNode/*?*/ RepeatBehaviorType { get { return repeatBehaviorType; } }
    public static Class/*!*/ ResourceManager;
    public static Class/*!*/ ResourceSet;
    public static Class/*!*/ SerializationInfo;
    static Delayed<Struct> size;
    public static Struct/*!*/ Size { get { return size; } }
    public static Class/*!*/ Stack;
    public static Class/*!*/ StackOverflowException;
    public static Class/*!*/ Stream;
    public static Struct/*!*/ StreamingContext;
    public static Class/*!*/ StringBuilder;
    public static Class/*!*/ StringComparer;
    public static EnumNode/*?*/ StringComparison;
    public static Class/*!*/ SystemException;
    private static Delayed<Struct> thickness;
    public static Struct/*!*/ Thickness { get { return thickness; } }
    public static Class/*!*/ Thread;
    public static Class/*!*/ Uri;
    public static Class/*!*/ WindowsImpersonationContext;
#endif
#if ExtendedRuntime
    public static Interface/*!*/ ConstrainedType { get { return ExtendedRuntimeTypes.ConstrainedType; } }
    public static Interface/*!*/ TupleType { get { return ExtendedRuntimeTypes.TupleType; } }
    public static Interface/*!*/ TypeAlias { get { return ExtendedRuntimeTypes.TypeAlias; } }
    public static Interface/*!*/ TypeDefinition { get { return ExtendedRuntimeTypes.TypeDefinition; } }
    public static Interface/*!*/ TypeIntersection { get { return ExtendedRuntimeTypes.TypeIntersection; } }
    public static Interface/*!*/ TypeUnion { get { return ExtendedRuntimeTypes.TypeUnion; } }
    public static Class/*!*/ AnonymousAttribute;
    public static TypeNode/*!*/ AnonymityEnum;
    public static Class/*!*/ ComposerAttribute;
    public static Class/*!*/ CustomVisitorAttribute;
    public static Class/*!*/ TemplateAttribute;
    public static Class/*!*/ TemplateInstanceAttribute;
    public static Class/*!*/ UnmanagedStructTemplateParameterAttribute;
    public static Class/*!*/ PointerFreeStructTemplateParameterAttribute;
    public static Class/*!*/ TemplateParameterFlagsAttribute;
    public static Struct/*!*/ GenericBoxed;
    public static Class/*!*/ GenericIEnumerableToGenericIListAdapter;
    public static Struct/*!*/ GenericInvariant;
    public static Struct/*!*/ GenericNonEmptyIEnumerable;
    public static Struct/*!*/ GenericNonNull;
    public static Class/*!*/ GenericStreamUtility;
    public static Class/*!*/ GenericUnboxer;
    public static Interface/*!*/ ITemplate { get { return ExtendedRuntimeTypes.ITemplate; } } // mark a template so that bartok would pass on checking
    public static Interface/*!*/ ITemplateParameter { get { return ExtendedRuntimeTypes.ITemplateParameter; } }
    public static Class/*!*/ ElementTypeAttribute;
    public static Interface/*!*/ IDbTransactable;
    public static Interface/*!*/ IAggregate;
    public static Interface/*!*/ IAggregateGroup;
    public static Class/*!*/ StreamNotSingletonException;
    public static EnumNode SqlHint;
    public static Class/*!*/ SqlFunctions;
    public static Class/*!*/ XmlAttributeAttributeClass;
    public static Class/*!*/ XmlChoiceIdentifierAttributeClass;
    public static Class/*!*/ XmlElementAttributeClass;
    public static Class/*!*/ XmlIgnoreAttributeClass;
    public static Class/*!*/ XmlTypeAttributeClass;
    public static Interface/*!*/ INullable;
    public static Struct/*!*/ SqlBinary;
    public static Struct/*!*/ SqlBoolean;
    public static Struct/*!*/ SqlByte;
    public static Struct/*!*/ SqlDateTime;
    public static Struct/*!*/ SqlDecimal;
    public static Struct/*!*/ SqlDouble;
    public static Struct/*!*/ SqlGuid;
    public static Struct/*!*/ SqlInt16;
    public static Struct/*!*/ SqlInt32;
    public static Struct/*!*/ SqlInt64;
    public static Struct/*!*/ SqlMoney;
    public static Struct/*!*/ SqlSingle;
    public static Struct/*!*/ SqlString;
    public static Interface/*!*/ IDbConnection;
    public static Interface/*!*/ IDbTransaction;
    public static EnumNode IsolationLevel;

    //OrdinaryExceptions
    public static Class/*!*/ NoChoiceException;
    public static Class/*!*/ IllegalUpcastException;
    public static EnumNode/*!*/ CciMemberKind;
    public static Class/*!*/ CciMemberKindAttribute;
    //NonNull  
    public static Class/*!*/ Range;
    //Invariants
    public static DelegateNode/*!*/ InitGuardSetsDelegate;
    public static DelegateNode/*!*/ CheckInvariantDelegate;
    public static DelegateNode/*!*/ FrameGuardGetter;
    public static Class/*!*/ ObjectInvariantException;
    public static DelegateNode/*!*/ ThreadConditionDelegate;
    public static DelegateNode/*!*/ GuardThreadStart;
    public static Class/*!*/ Guard;
    public static Class/*!*/ ContractMarkers;
    //public static Interface IReduction;
    public static Class/*!*/ AssertHelpers;
    public static DelegateNode/*!*/ ThreadStart;
    //CheckedExceptions
    public static Interface/*!*/ ICheckedException;
    public static Class/*!*/ CheckedException;

    // Contracts
    public static Class/*!*/ UnreachableException;
    public static Class/*!*/ ContractException;
    public static Class/*!*/ NullTypeException;
    public static Class/*!*/ AssertException;
    public static Class/*!*/ AssumeException;
    public static Class/*!*/ InvalidContractException;
    public static Class/*!*/ RequiresException;
    public static Class/*!*/ EnsuresException;
    public static Class/*!*/ ModifiesException;
    public static Class/*!*/ ThrowsException;
    public static Class/*!*/ DoesException;
    public static Class/*!*/ InvariantException;
    public static Class/*!*/ ContractMarkerException;
    public static Class/*!*/ PreAllocatedExceptions;

    public static Class/*!*/ AdditiveAttribute;
    public static Class/*!*/ InsideAttribute;
    public static Class/*!*/ SpecPublicAttribute;
    public static Class/*!*/ SpecProtectedAttribute;
    public static Class/*!*/ SpecInternalAttribute;
    public static Class/*!*/ PureAttribute;
    public static Class/*!*/ ReadsAttribute;
    public static Class/*!*/ RepAttribute;
    public static Class/*!*/ PeerAttribute;
    public static Class/*!*/ CapturedAttribute;
    public static Class/*!*/ LockProtectedAttribute;
    public static Class/*!*/ RequiresLockProtectedAttribute;
    public static Class/*!*/ ImmutableAttribute;
    public static Class/*!*/ RequiresImmutableAttribute;
    public static Class/*!*/ RequiresCanWriteAttribute;
    // TODO: Remove the next two fields after LKG > 11215 (20 December 2007)
    public static Class/*!*/ StateIndependentAttribute;
    public static Class/*!*/ ConfinedAttribute;
    public static Class/*!*/ ModelfieldContractAttribute;
    public static Class/*!*/ ModelfieldAttribute;    
    public static Class/*!*/ SatisfiesAttribute;  //Stores a satisfies clause of a modelfield
    public static Class/*!*/ ModelfieldException;

    public static Class/*!*/ OnceAttribute;
    public static Class/*!*/ WriteConfinedAttribute;
    public static Class/*!*/ WriteAttribute;
    public static Class/*!*/ ReadAttribute;
    public static Class/*!*/ GlobalReadAttribute;
    public static Class/*!*/ GlobalAccessAttribute;
    public static Class/*!*/ GlobalWriteAttribute;
    public static Class/*!*/ FreshAttribute;
    public static Class/*!*/ EscapesAttribute;
    
    public static Class/*!*/ ModelAttribute;
    public static Class/*!*/ RequiresAttribute;
    public static Class/*!*/ EnsuresAttribute;
    public static Class/*!*/ ModifiesAttribute;
    public static Class/*!*/ HasWitnessAttribute;
    public static Class/*!*/ WitnessAttribute;
    public static Class/*!*/ InferredReturnValueAttribute;
    public static Class/*!*/ ThrowsAttribute;
    public static Class/*!*/ DoesAttribute;
    public static Class/*!*/ InvariantAttribute;
    public static Class/*!*/ NoDefaultContractAttribute;
    public static Class/*!*/ ReaderAttribute;
    public static Class/*!*/ ShadowsAssemblyAttribute;
    public static Class/*!*/ VerifyAttribute;
    public static Class/*!*/ NonNullType { get { return ExtendedRuntimeTypes.NonNullType; } }
    public static Method NonNullTypeAssertInitialized { get { return ExtendedRuntimeTypes.NonNullTypeAssertInitialized; } }
    public static Method NonNullTypeAssertInitializedGeneric { get { return ExtendedRuntimeTypes.NonNullTypeAssertInitializedGeneric; } }
    public static Class/*!*/ OwnerClass { get { return ExtendedRuntimeTypes.OwnerClass; } }

    public static Class/*!*/ NullableType { get { return ExtendedRuntimeTypes.NullableType; } }
    public static Class/*!*/ NotNullAttribute { get { return ExtendedRuntimeTypes.NotNullAttribute; } }
    public static Class/*!*/ NotNullArrayElementsAttribute { get { return ExtendedRuntimeTypes.NotNullArrayElementsAttribute; } }
    public static Class/*!*/ NotNullGenericArgumentsAttribute { get { return ExtendedRuntimeTypes.NotNullGenericArgumentsAttribute; } }
    public static Class/*!*/ EncodedTypeSpecAttribute { get { return ExtendedRuntimeTypes.EncodedTypeSpecAttribute; } }
    public static Class/*!*/ DependentAttribute;
    public static Class/*!*/ ElementsRepAttribute;
    public static Class/*!*/ ElementsPeerAttribute;
    public static Class/*!*/ ElementAttribute;
    public static Class/*!*/ ElementCollectionAttribute;
    public static Class/*!*/ RecursionTerminationAttribute;
    public static Class/*!*/ NoReferenceComparisonAttribute;
    public static Class/*!*/ ResultNotNewlyAllocatedAttribute;

    // This attribute is recognized by the Bartok compiler and marks methods without heap allocation.
    // Thus the presence of this attribute implies [ResultNotNewlyAllocated] for pure methods.
    public static Class/*!*/ BartokNoHeapAllocationAttribute {
      //^ [NoDefaultContract]
      get
        //^ modifies noHeapAllocationAttribute;
      {
        if(noHeapAllocationAttribute == null) {
          noHeapAllocationAttribute = (Class)GetCompilerRuntimeTypeNodeFor(
            @"System.Runtime.CompilerServices", @"NoHeapAllocationAttribute", ElementType.Class);
        }
        return noHeapAllocationAttribute;
      }
    }
    private static Class noHeapAllocationAttribute;    
#endif

    static SystemTypes(){
      SystemTypes.Initialize(TargetPlatform.DoNotLockFiles, TargetPlatform.GetDebugInfo);
    }

#if FxCop
    internal static event EventHandler<EventArgs> ClearingSystemTypes;
    internal static void RaiseClearingSystemTypes()
    {
      EventHandler<EventArgs> handler = ClearingSystemTypes;
      if (handler != null) handler(null, EventArgs.Empty);
    }
#endif
    public static void Clear(){
      lock (Module.GlobalLock){
        CoreSystemTypes.Clear();
#if FxCop
        RaiseClearingSystemTypes(); 
#endif
#if ExtendedRuntime
        ExtendedRuntimeTypes.Clear();
        if (SystemTypes.SystemCompilerRuntimeAssembly != null && SystemTypes.SystemCompilerRuntimeAssembly != AssemblyNode.Dummy) {
          SystemTypes.SystemCompilerRuntimeAssembly.Dispose();
          SystemTypes.SystemCompilerRuntimeAssembly = null;
        }
#if !NoData && !ROTOR
        if (SystemTypes.SystemDataAssembly != null && SystemTypes.SystemDataAssembly != AssemblyNode.Dummy) {
          SystemTypes.SystemDataAssembly.Dispose();
          SystemTypes.SystemDataAssembly = null;
        }
#endif
#if !NoXml && !NoRuntimeXml
        if (SystemTypes.SystemXmlAssembly != null && SystemTypes.SystemXmlAssembly != AssemblyNode.Dummy) {
          SystemTypes.SystemXmlAssembly.Dispose();
          SystemTypes.SystemXmlAssembly = null;
        }
#endif
#endif
        SystemTypes.ClearStatics();
        SystemTypes.Initialized = false;
      }
    }
    public static void Initialize(bool doNotLockFile, bool getDebugInfo, AssemblyNode.PostAssemblyLoadProcessor postAssemblyLoad = null){
      if (TargetPlatform.BusyWithClear) return;
      if (SystemTypes.Initialized){
        SystemTypes.Clear();
        CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo, postAssemblyLoad);
#if ExtendedRuntime
        ExtendedRuntimeTypes.Initialize(doNotLockFile, getDebugInfo);
#endif
      }else if (!CoreSystemTypes.Initialized){
        CoreSystemTypes.Initialize(doNotLockFile, getDebugInfo, postAssemblyLoad);
#if ExtendedRuntime
        ExtendedRuntimeTypes.Clear();
        ExtendedRuntimeTypes.Initialize(doNotLockFile, getDebugInfo);
#endif
      }

      if (TargetPlatform.TargetVersion == null){
        TargetPlatform.TargetVersion = SystemAssembly.Version;
        if (TargetPlatform.TargetVersion == null)
          TargetPlatform.TargetVersion = typeof(object).Assembly.GetName().Version;
      }
      //TODO: throw an exception when the result is null
#if ExtendedRuntime
#if !NoData && !ROTOR
      SystemDataAssembly = SystemTypes.GetSystemDataAssembly(doNotLockFile, getDebugInfo);
#endif
#if !NoXml && !NoRuntimeXml
      SystemXmlAssembly = SystemTypes.GetSystemXmlAssembly(doNotLockFile, getDebugInfo);
#endif
#endif
      CollectionsAssembly = SystemTypes.GetCollectionsAssembly(doNotLockFile, getDebugInfo);
      DiagnosticsDebugAssembly = SystemTypes.GetDiagnosticsDebugAssembly(doNotLockFile, getDebugInfo);
      DiagnosticsToolsAssembly = SystemTypes.GetDiagnosticsToolsAssembly(doNotLockFile, getDebugInfo);
      GlobalizationAssembly = SystemTypes.GetGlobalizationAssembly(doNotLockFile, getDebugInfo);  
      InteropAssembly = SystemTypes.GetInteropAssembly(doNotLockFile, getDebugInfo);
      IOAssembly = SystemTypes.GetIOAssembly(doNotLockFile, getDebugInfo);
      ReflectionAssembly = SystemTypes.GetReflectionAssembly(doNotLockFile, getDebugInfo);
      ResourceManagerAssembly = SystemTypes.GetResourceManagerAssembly(doNotLockFile, getDebugInfo);
      SystemDllAssembly = SystemTypes.GetSystemDllAssembly(doNotLockFile, getDebugInfo);
      SystemRuntimeExtensionsAssembly = SystemTypes.GetRuntimeExtensionsAssembly(doNotLockFile, getDebugInfo);
      SystemRuntimeSerializationAssembly = SystemTypes.GetRuntimeSerializationAssembly(doNotLockFile, getDebugInfo);
      SystemRuntimeWindowsRuntimeInteropAssembly = SystemTypes.GetWindowsRuntimeInteropAssembly(doNotLockFile, getDebugInfo);
      ThreadingAssembly = SystemTypes.GetThreadingAssembly(doNotLockFile, getDebugInfo);

      AttributeUsageAttribute = (Class)GetTypeNodeFor("System", "AttributeUsageAttribute", ElementType.Class);
      ConditionalAttribute = (Class)GetTypeNodeFor("System.Diagnostics", "ConditionalAttribute", ElementType.Class);
      DefaultMemberAttribute = (Class)GetTypeNodeFor("System.Reflection", "DefaultMemberAttribute", ElementType.Class);
      InternalsVisibleToAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "InternalsVisibleToAttribute", ElementType.Class);
      ObsoleteAttribute = (Class)GetTypeNodeFor("System", "ObsoleteAttribute", ElementType.Class);

      GenericICollection = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "ICollection", 1, ElementType.Class);
      GenericIEnumerable = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IEnumerable", 1, ElementType.Class);
      GenericIList = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IList", 1, ElementType.Class);
      ICloneable = (Interface)GetTypeNodeFor("System", "ICloneable", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      ICollection = (Interface)GetTypeNodeFor("System.Collections", "ICollection", ElementType.Class);
      IEnumerable = (Interface)GetTypeNodeFor("System.Collections", "IEnumerable", ElementType.Class);
      IList = (Interface)GetTypeNodeFor("System.Collections", "IList", ElementType.Class);

#if !MinimalReader
      AllowPartiallyTrustedCallersAttribute = (Class)GetTypeNodeFor("System.Security", "AllowPartiallyTrustedCallersAttribute", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      AssemblyCompanyAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyCompanyAttribute", ElementType.Class);
      AssemblyConfigurationAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyConfigurationAttribute", ElementType.Class);
      AssemblyCopyrightAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyCopyrightAttribute", ElementType.Class);
      AssemblyCultureAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyCultureAttribute", ElementType.Class);
      AssemblyDelaySignAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyDelaySignAttribute", ElementType.Class);
      AssemblyDescriptionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyDescriptionAttribute", ElementType.Class);
      AssemblyFileVersionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyFileVersionAttribute", ElementType.Class);
      AssemblyFlagsAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyFlagsAttribute", ElementType.Class);
      AssemblyInformationalVersionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyInformationalVersionAttribute", ElementType.Class);
      AssemblyKeyFileAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyKeyFileAttribute", ElementType.Class);
      AssemblyKeyNameAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyKeyNameAttribute", ElementType.Class); 
      AssemblyProductAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyProductAttribute", ElementType.Class);
      AssemblyTitleAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyTitleAttribute", ElementType.Class);
      AssemblyTrademarkAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyTrademarkAttribute", ElementType.Class);
      AssemblyVersionAttribute = (Class)GetTypeNodeFor("System.Reflection", "AssemblyVersionAttribute", ElementType.Class);
      AttributeTargets =  GetTypeNodeFor("System", "AttributeTargets", ElementType.ValueType) as EnumNode;
      color = new Func<TypeNode>(() => GetWindowsRuntimeTypeNodeFor("Windows.UI", "Color", ElementType.ValueType)); //projected from Windows.UI.Xaml.Media, but to where?
      cornerRadius = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "CornerRadius", ElementType.ValueType)); //projected from Windows.UI.Xaml but to where?
      ClassInterfaceAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "ClassInterfaceAttribute", ElementType.Class);
      CLSCompliantAttribute = (Class)GetTypeNodeFor("System", "CLSCompliantAttribute", ElementType.Class);
      ComImportAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComImportAttribute", ElementType.Class);
      ComRegisterFunctionAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComRegisterFunctionAttribute", ElementType.Class);
      ComSourceInterfacesAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComSourceInterfacesAttribute", ElementType.Class);
      ComUnregisterFunctionAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "ComUnregisterFunctionAttribute", ElementType.Class);
      ComVisibleAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "ComVisibleAttribute", ElementType.Class);      
      DebuggableAttribute = (Class)GetTypeNodeFor("System.Diagnostics", "DebuggableAttribute", ElementType.Class);
      DebuggerHiddenAttribute = (Class)GetDiagnosticsDebugTypeNodeFor("System.Diagnostics", "DebuggerHiddenAttribute", ElementType.Class);
      DebuggerStepThroughAttribute = (Class)GetDiagnosticsDebugTypeNodeFor("System.Diagnostics", "DebuggerStepThroughAttribute", ElementType.Class);
      DebuggingModes = DebuggableAttribute == null ? null : DebuggableAttribute.GetNestedType(Identifier.For("DebuggingModes")) as EnumNode;
      DllImportAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "DllImportAttribute", ElementType.Class);
      duration = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "Duration", ElementType.ValueType)); //projected from Windows.UI.Xaml but to where?
      durationType = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "DurationType", ElementType.ValueType) as EnumNode); //projected from Windows.UI.Xaml but to where?
      FieldOffsetAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "FieldOffsetAttribute", ElementType.Class);
      FlagsAttribute = (Class)GetTypeNodeFor("System", "FlagsAttribute", ElementType.Class);
      generatorPosition = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition", ElementType.ValueType)); //projected from Windows.UI.Xaml.Controls.Primitives but to where?
      Guid = (Struct)GetTypeNodeFor("System", "Guid", ElementType.ValueType);
      GuidAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "GuidAttribute", ElementType.Class);
      gridLength = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "GridLength", ElementType.ValueType)); //projected from Windows.UI.Xaml but to where?
      gridUnitType = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "GridUnitType", ElementType.ValueType) as EnumNode); //projected from Windows.UI.Xaml but to where?
      ImportedFromTypeLibAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "ImportedFromTypeLibAttribute", ElementType.Class);
      InAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "InAttribute", ElementType.Class);
      IndexerNameAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "IndexerNameAttribute", ElementType.Class);
      InterfaceTypeAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "InterfaceTypeAttribute", ElementType.Class);
      keyTime = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Animation", "KeyTime", ElementType.ValueType)); //projected from Windows.UI.Xaml.Media.Animation but to where?
      repeatBehavior = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Animation", "RepeatBehavior", ElementType.ValueType)); //projected from Windows.UI.Xaml.Media.Animation but to where?
      repeatBehaviorType = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType", ElementType.ValueType) as EnumNode); //projected from ?? to ??
      MethodImplAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "MethodImplAttribute", ElementType.Class);
      matrix = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media", "Matrix", ElementType.ValueType)); //projected from Windows.UI.Xaml.Media but to where?
      matrix3D = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml.Media.Media3D", "Matrix3D", ElementType.ValueType)); //projected from Windows.UI.Xaml.Media.Media3D but to where?
      NonSerializedAttribute = (Class)GetTypeNodeFor("System", "NonSerializedAttribute", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?     
      OptionalAttribute = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "OptionalAttribute", ElementType.Class);
      OutAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "OutAttribute", ElementType.Class);
      ParamArrayAttribute = (Class)GetTypeNodeFor("System", "ParamArrayAttribute", ElementType.Class);
      point = new Func<TypeNode>(() => GetWindowsRuntimeTypeNodeFor("Windows.Foundation", "Point", ElementType.ValueType));
      rect = new Func<TypeNode>(() => GetWindowsRuntimeTypeNodeFor("Windows.Foundation", "Rect", ElementType.ValueType));
      RuntimeCompatibilityAttribute = (Class)GetTypeNodeFor("System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute", ElementType.Class);
      SatelliteContractVersionAttribute = (Class)GetResourceManagerTypeNodeFor("System.Resources", "SatelliteContractVersionAttribute", ElementType.Class);
      SerializableAttribute = (Class)GetTypeNodeFor("System", "SerializableAttribute", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      SecurityAttribute = (Class)GetTypeNodeFor("System.Security.Permissions", "SecurityAttribute", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      SecurityCriticalAttribute = (Class)GetTypeNodeFor("System.Security", "SecurityCriticalAttribute", ElementType.Class);
      SecurityTransparentAttribute = (Class)GetTypeNodeFor("System.Security", "SecurityTransparentAttribute", ElementType.Class);
      SecurityTreatAsSafeAttribute = (Class)GetTypeNodeFor("System.Security", "SecurityTreatAsSafeAttribute", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      size = new Func<TypeNode>(() => GetWindowsRuntimeTypeNodeFor("Windows.Foundation", "Size", ElementType.ValueType));
      STAThreadAttribute = (Class)GetTypeNodeFor("System", "STAThreadAttribute", ElementType.Class);
      StructLayoutAttribute = (Class)GetTypeNodeFor("System.Runtime.InteropServices", "StructLayoutAttribute", ElementType.Class);
      SuppressMessageAttribute = (Class)GetDiagnosticsToolsTypeNodeFor("System.Diagnostics.CodeAnalysis", "SuppressMessageAttribute", ElementType.Class);
      SuppressUnmanagedCodeSecurityAttribute = (Class)GetTypeNodeFor("System.Security", "SuppressUnmanagedCodeSecurityAttribute", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      SecurityAction = GetTypeNodeFor("System.Security.Permissions", "SecurityAction", ElementType.ValueType) as EnumNode; //Where does this mscorlib type live in the new world of reference assemblies?
      DBNull = (Class)GetTypeNodeFor("System", "DBNull", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      DateTime = (Struct)GetTypeNodeFor("System", "DateTime", ElementType.ValueType);
      DateTimeOffset = (Struct)GetTypeNodeFor("System", "DateTimeOffset", ElementType.ValueType);
      TimeSpan = (Struct)GetTypeNodeFor("System", "TimeSpan", ElementType.ValueType);
      Activator = (Class)GetTypeNodeFor("System", "Activator", ElementType.Class);
      AppDomain = (Class)GetTypeNodeFor("System", "AppDomain", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      ApplicationException = (Class)GetTypeNodeFor("System", "ApplicationException", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      ArgumentException = (Class)GetTypeNodeFor("System", "ArgumentException", ElementType.Class);
      ArgumentNullException = (Class)GetTypeNodeFor("System", "ArgumentNullException", ElementType.Class);
      ArgumentOutOfRangeException = (Class)GetTypeNodeFor("System", "ArgumentOutOfRangeException", ElementType.Class);
      ArrayList = (Class)GetTypeNodeFor("System.Collections", "ArrayList", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      AsyncCallback = (DelegateNode)GetTypeNodeFor("System", "AsyncCallback", ElementType.Class);
      Assembly = (Class)GetReflectionTypeNodeFor("System.Reflection", "Assembly", ElementType.Class);
      CodeAccessPermission = (Class)GetTypeNodeFor("System.Security", "CodeAccessPermission", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      CollectionBase = (Class)GetTypeNodeFor("System.Collections", "CollectionBase", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      CultureInfo = (Class)GetGlobalizationTypeNodeFor("System.Globalization", "CultureInfo", ElementType.Class);
      DictionaryBase = (Class)GetTypeNodeFor("System.Collections", "DictionaryBase", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      DictionaryEntry = (Struct)GetTypeNodeFor("System.Collections", "DictionaryEntry", ElementType.ValueType); //Where does this mscorlib type live in the new world of reference assemblies?
      DuplicateWaitObjectException = (Class)GetTypeNodeFor("System", "DuplicateWaitObjectException", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      Environment = (Class)GetRuntimeExtensionsTypeNodeFor("System", "Environment", ElementType.Class);
      EventArgs = (Class)GetTypeNodeFor("System", "EventArgs", ElementType.Class);
      EventHandler1 = GetGenericRuntimeTypeNodeFor("System", "EventHandler", 1, ElementType.Class) as DelegateNode;
      EventRegistrationToken = (Struct)GetWindowsRuntimeInteropTypeNodeFor("System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken", ElementType.ValueType);
      ExecutionEngineException = (Class)GetTypeNodeFor("System", "ExecutionEngineException", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      GenericArraySegment = (Struct)GetGenericRuntimeTypeNodeFor("System", "ArraySegment", 1, ElementType.ValueType);
      GenericDictionary = (Class)GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "Dictionary", 2, ElementType.Class);
      GenericIComparable = (Interface)GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "IComparable", 1, ElementType.Class);
      GenericIComparer = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IComparer", 1, ElementType.Class);
      GenericIDictionary = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IDictionary", 2, ElementType.Class);
      GenericIEnumerator = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IEnumerator", 1, ElementType.Class);
      GenericIReadOnlyDictionary = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IReadOnlyDictionary", 2, ElementType.Class);
      GenericIReadOnlyList = (Interface)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "IReadOnlyList", 1, ElementType.Class);
      GenericKeyValuePair = (Struct)GetGenericRuntimeTypeNodeFor("System.Collections.Generic", "KeyValuePair", 2, ElementType.ValueType);
      GenericList = (Class)GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "List", 1, ElementType.Class);
      GenericNullable = (Struct)GetGenericRuntimeTypeNodeFor("System", "Nullable", 1, ElementType.ValueType);
      GenericQueue = (Class)GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "Queue", 1, ElementType.Class);
      GenericSortedDictionary = (Class)GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "SortedDictionary", 2, ElementType.Class);
      GenericStack = (Class)GetCollectionsGenericRuntimeTypeNodeFor("System.Collections.Generic", "Stack", 1, ElementType.Class);
      GC = (Class)GetRuntimeExtensionsTypeNodeFor("System", "GC", ElementType.Class);
      __HandleProtector = (Class)GetTypeNodeFor("System.Threading", "__HandleProtector", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      HandleRef = (Struct)GetInteropTypeNodeFor("System.Runtime.InteropServices", "HandleRef", ElementType.ValueType);
      Hashtable = (Class)GetTypeNodeFor("System.Collections", "Hashtable", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      IASyncResult = (Interface)GetTypeNodeFor("System", "IAsyncResult", ElementType.Class);
      ICommand = (Interface)GetTypeNodeFor("System.Windows.Input", "ICommand", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      IComparable = (Interface)GetTypeNodeFor("System", "IComparable", ElementType.Class);
      IComparer = (Interface)GetTypeNodeFor("System.Collections", "IComparer", ElementType.Class);
      IDictionary = (Interface)GetTypeNodeFor("System.Collections", "IDictionary", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      IDisposable = (Interface)GetTypeNodeFor("System", "IDisposable", ElementType.Class);
      IEnumerator = (Interface)GetTypeNodeFor("System.Collections", "IEnumerator", ElementType.Class);
      IFormatProvider = (Interface)GetTypeNodeFor("System", "IFormatProvider", ElementType.Class);
      IHashCodeProvider = (Interface)GetTypeNodeFor("System.Collections", "IHashCodeProvider", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      IMembershipCondition = (Interface)GetTypeNodeFor("System.Security.Policy", "IMembershipCondition", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      IndexOutOfRangeException = (Class)GetTypeNodeFor("System", "IndexOutOfRangeException", ElementType.Class);
      IBindableIterable = (Interface)GetTypeNodeFor("System.Collections", "IBindableIterable", ElementType.Class);
      IBindableVector = (Interface)GetTypeNodeFor("System.Collections", "IBindableVector", ElementType.Class);
      INotifyCollectionChanged = (Interface)GetSystemTypeNodeFor("System.Collections.Specialized", "INotifyCollectionChanged", ElementType.Class);
      INotifyPropertyChanged = (Interface)GetSystemTypeNodeFor("System.ComponentModel", "INotifyPropertyChanged", ElementType.Class);
      InvalidCastException = (Class)GetTypeNodeFor("System", "InvalidCastException", ElementType.Class);
      InvalidOperationException = (Class)GetTypeNodeFor("System", "InvalidOperationException", ElementType.Class);
      IPermission = (Interface)GetTypeNodeFor("System.Security", "IPermission", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      ISerializable = (Interface)GetTypeNodeFor("System.Runtime.Serialization", "ISerializable", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      IStackWalk = (Interface)GetTypeNodeFor("System.Security", "IStackWalk", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      Marshal = (Class)GetInteropTypeNodeFor("System.Runtime.InteropServices", "Marshal", ElementType.Class);
      MarshalByRefObject = (Class)GetTypeNodeFor("System", "MarshalByRefObject", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      MemberInfo = (Class)GetReflectionTypeNodeFor("System.Reflection", "MemberInfo", ElementType.Class);
      Monitor = (Class)GetThreadingTypeNodeFor("System.Threading", "Monitor", ElementType.Class);
      NativeOverlapped = (Struct)GetThreadingTypeNodeFor("System.Threading", "NativeOverlapped", ElementType.ValueType);
      NotifyCollectionChangedAction = GetSystemTypeNodeFor("System.Collections.Specialized", "NotifyCollectionChangedAction", ElementType.ValueType) as EnumNode;
      NotifyCollectionChangedEventArgs = (Class)GetSystemTypeNodeFor("System.Collections.Specialized", "NotifyCollectionChangedEventArgs", ElementType.Class);
      NotifyCollectionChangedEventHandler = (DelegateNode)GetSystemTypeNodeFor("System.Collections.Specialized", "NotifyCollectionChangedEventHandler", ElementType.Class);
      NotSupportedException = (Class)GetTypeNodeFor("System", "NotSupportedException", ElementType.Class);
      NullReferenceException = (Class)GetTypeNodeFor("System", "NullReferenceException", ElementType.Class);
      OutOfMemoryException = (Class)GetTypeNodeFor("System", "OutOfMemoryException", ElementType.Class);
      ParameterInfo = (Class)GetReflectionTypeNodeFor("System.Reflection", "ParameterInfo", ElementType.Class);
      PropertyChangedEventArgs = (Class)GetSystemTypeNodeFor("System.ComponentModel", "PropertyChangedEventArgs", ElementType.Class);
      PropertyChangedEventHandler = (DelegateNode)GetSystemTypeNodeFor("System.ComponentModel", "PropertyChangedEventHandler", ElementType.Class);
      Queue = (Class)GetCollectionsTypeNodeFor("System.Collections", "Queue", ElementType.Class);
      ReadOnlyCollectionBase = (Class)GetTypeNodeFor("System.Collections", "ReadOnlyCollectionBase", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      ResourceManager = (Class)GetResourceManagerTypeNodeFor("System.Resources", "ResourceManager", ElementType.Class);
      ResourceSet = (Class)GetTypeNodeFor("System.Resources", "ResourceSet", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      SerializationInfo = (Class)GetTypeNodeFor("System.Runtime.Serialization", "SerializationInfo", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      Stack = (Class)GetTypeNodeFor("System.Collections", "Stack", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      StackOverflowException = (Class)GetTypeNodeFor("System", "StackOverflowException", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      Stream = (Class)GetIOTypeNodeFor("System.IO", "Stream", ElementType.Class);
      StreamingContext = (Struct)GetRuntimeSerializationTypeNodeFor("System.Runtime.Serialization", "StreamingContext", ElementType.ValueType);
      StringBuilder = (Class)GetTypeNodeFor("System.Text", "StringBuilder", ElementType.Class);
      StringComparer = (Class)GetRuntimeExtensionsTypeNodeFor("System", "StringComparer", ElementType.Class);
      StringComparison =  GetTypeNodeFor("System", "StringComparison", ElementType.ValueType) as EnumNode;
      SystemException = (Class)GetTypeNodeFor("System", "SystemException", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
      Thread = (Class)GetThreadingTypeNodeFor("System.Threading", "Thread", ElementType.Class);
      thickness = new Func<TypeNode>(() => GetWindowsRuntimeUIXamlTypeNodeFor("Windows.UI.Xaml", "Thickness", ElementType.ValueType)); //projected from Windows.UI.Xaml but to where?
      Uri = (Class)GetSystemTypeNodeFor("System", "Uri", ElementType.Class);
      WindowsImpersonationContext = (Class)GetTypeNodeFor("System.Security.Principal", "WindowsImpersonationContext", ElementType.Class); //Where does this mscorlib type live in the new world of reference assemblies?
#endif      
#if ExtendedRuntime
#if !NoXml && !NoRuntimeXml
      XmlAttributeAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlAttributeAttribute", ElementType.Class);
      XmlChoiceIdentifierAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlChoiceIdentifierAttribute", ElementType.Class);
      XmlElementAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlElementAttribute", ElementType.Class);
      XmlIgnoreAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlIgnoreAttribute", ElementType.Class);
      XmlTypeAttributeClass = (Class)GetXmlTypeNodeFor("System.Xml.Serialization", "XmlTypeAttribute", ElementType.Class);
#endif

#if !NoData
      INullable = (Interface) GetDataTypeNodeFor("System.Data.SqlTypes", "INullable", ElementType.Class);
      SqlBinary = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlBinary", ElementType.ValueType);
      SqlBoolean = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlBoolean", ElementType.ValueType);
      SqlByte = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlByte", ElementType.ValueType);
      SqlDateTime = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlDateTime", ElementType.ValueType);
      SqlDecimal = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlDecimal", ElementType.ValueType);
      SqlDouble = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlDouble", ElementType.ValueType);
      SqlGuid = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlGuid", ElementType.ValueType);
      SqlInt16 = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlInt16", ElementType.ValueType);
      SqlInt32 = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlInt32", ElementType.ValueType);
      SqlInt64 = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlInt64", ElementType.ValueType);
      SqlMoney = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlMoney", ElementType.ValueType);
      SqlSingle = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlSingle", ElementType.ValueType);
      SqlString = (Struct)GetDataTypeNodeFor("System.Data.SqlTypes", "SqlString", ElementType.ValueType);
      IDbConnection = (Interface)GetDataTypeNodeFor("System.Data", "IDbConnection", ElementType.Class);
      IDbTransaction = (Interface)GetDataTypeNodeFor("System.Data", "IDbTransaction", ElementType.Class);
      IsolationLevel = GetDataTypeNodeFor("System.Data", "IsolationLevel", ElementType.ValueType) as EnumNode;
#endif
#if CCINamespace
      const string CciNs = "Microsoft.Cci";
      const string ContractsNs = "Microsoft.Contracts";
      const string CompilerGuardsNs = "Microsoft.Contracts";
#else
      const string CciNs = "System.Compiler";
      const string ContractsNs = "Microsoft.Contracts";
      const string CompilerGuardsNs = "Microsoft.Contracts";
#endif
      const string GuardsNs = "Microsoft.Contracts";
      AnonymousAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "AnonymousAttribute", ElementType.Class);
      AnonymityEnum = GetCompilerRuntimeTypeNodeFor(CciNs, "Anonymity", ElementType.ValueType);
      ComposerAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "ComposerAttribute", ElementType.Class);
      CustomVisitorAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "CustomVisitorAttribute", ElementType.Class);
      TemplateAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "TemplateAttribute", ElementType.Class);
      TemplateInstanceAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "TemplateInstanceAttribute", ElementType.Class);
      UnmanagedStructTemplateParameterAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "UnmanagedStructTemplateParameterAttribute", ElementType.Class);
      PointerFreeStructTemplateParameterAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "PointerFreeStructTemplateParameterAttribute", ElementType.Class);
      TemplateParameterFlagsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "TemplateParameterFlagsAttribute", ElementType.Class);
#if !WHIDBEYwithGenerics
      GenericArrayToIEnumerableAdapter = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "ArrayToIEnumerableAdapter", 1, ElementType.Class);
#endif
      GenericBoxed = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "Boxed", 1, ElementType.ValueType);
      GenericIEnumerableToGenericIListAdapter = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "GenericIEnumerableToGenericIListAdapter", 1, ElementType.Class);
      GenericInvariant = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "Invariant", 1, ElementType.ValueType);
      GenericNonEmptyIEnumerable = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "NonEmptyIEnumerable", 1, ElementType.ValueType);
      GenericNonNull = (Struct)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "NonNull", 1, ElementType.ValueType);
      GenericStreamUtility = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "StreamUtility", 1, ElementType.Class);
      GenericUnboxer = (Class)GetCompilerRuntimeTypeNodeFor("StructuralTypes", "Unboxer", 1, ElementType.Class);
      ElementTypeAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "ElementTypeAttribute", ElementType.Class);
      IDbTransactable = (Interface)GetCompilerRuntimeTypeNodeFor("System.Data", "IDbTransactable", ElementType.Class);
      IAggregate = (Interface)GetCompilerRuntimeTypeNodeFor("System.Query", "IAggregate", ElementType.Class);
      IAggregateGroup = (Interface)GetCompilerRuntimeTypeNodeFor("System.Query", "IAggregateGroup", ElementType.Class);
      StreamNotSingletonException = (Class)GetCompilerRuntimeTypeNodeFor("System.Query", "StreamNotSingletonException", ElementType.Class);
      SqlHint = GetCompilerRuntimeTypeNodeFor("System.Query", "SqlHint", ElementType.ValueType) as EnumNode;
      SqlFunctions = (Class)GetCompilerRuntimeTypeNodeFor("System.Query", "SqlFunctions", ElementType.Class);

      #region Contracts
      Range = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "Range", ElementType.Class);
      //Ordinary Exceptions
      NoChoiceException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NoChoiceException", ElementType.Class);
      IllegalUpcastException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "IllegalUpcastException", ElementType.Class);
      CciMemberKind = (EnumNode)GetCompilerRuntimeTypeNodeFor(CciNs, "CciMemberKind", ElementType.ValueType);
      CciMemberKindAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CciNs, "CciMemberKindAttribute", ElementType.Class);
      //Checked Exceptions
      ICheckedException = (Interface)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ICheckedException", ElementType.Class);
      CheckedException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "CheckedException", ElementType.Class);
      ContractMarkers = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ContractMarkers", ElementType.Class);
      //Invariant
      InitGuardSetsDelegate = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "InitGuardSetsDelegate", ElementType.Class);
      CheckInvariantDelegate = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "CheckInvariantDelegate", ElementType.Class);
      FrameGuardGetter = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "FrameGuardGetter", ElementType.Class);
      ObjectInvariantException = (Class)GetCompilerRuntimeTypeNodeFor("Microsoft.Contracts", "ObjectInvariantException", ElementType.Class);
      ThreadConditionDelegate = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "ThreadConditionDelegate", ElementType.Class);
      GuardThreadStart = (DelegateNode) GetCompilerRuntimeTypeNodeFor(GuardsNs, "GuardThreadStart", ElementType.Class);
      Guard = (Class) GetCompilerRuntimeTypeNodeFor(GuardsNs, "Guard", ElementType.Class);
      ThreadStart = (DelegateNode) GetTypeNodeFor("System.Threading", "ThreadStart", ElementType.Class);
      AssertHelpers = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AssertHelpers", ElementType.Class);
      #region Exceptions
      UnreachableException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "UnreachableException", ElementType.Class);
      ContractException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ContractException", ElementType.Class);
      NullTypeException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NullTypeException", ElementType.Class);
      AssertException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AssertException", ElementType.Class);
      AssumeException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AssumeException", ElementType.Class);
      InvalidContractException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InvalidContractException", ElementType.Class);
      RequiresException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "RequiresException", ElementType.Class);
      EnsuresException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "EnsuresException", ElementType.Class);
      ModifiesException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModifiesException", ElementType.Class);
      ThrowsException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ThrowsException", ElementType.Class);
      DoesException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DoesException", ElementType.Class);
      InvariantException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InvariantException", ElementType.Class);
      ContractMarkerException = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ContractMarkerException", ElementType.Class);
      PreAllocatedExceptions = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "PreAllocatedExceptions", ElementType.Class);
      #endregion
      #region Attributes
      AdditiveAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "AdditiveAttribute", ElementType.Class);
      InsideAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InsideAttribute", ElementType.Class);
      PureAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "PureAttribute", ElementType.Class);
      ReadsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ReadsAttribute", ElementType.Class);
      ConfinedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ConfinedAttribute", ElementType.Class);

      #region modelfield attributes and exceptions
      ModelfieldContractAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModelfieldContractAttribute", ElementType.Class);
      ModelfieldAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModelfieldAttribute", ElementType.Class);
      SatisfiesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SatisfiesAttribute", ElementType.Class);
      ModelfieldException = (Class)GetCompilerRuntimeTypeNodeFor("Microsoft.Contracts", "ModelfieldException", ElementType.Class);
      #endregion

      /* Diego's Attributes for Purity and WriteEffects */
        OnceAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "OnceAttribute", ElementType.Class);
        WriteConfinedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "WriteConfinedAttribute", ElementType.Class);
        WriteAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "WriteAttribute", ElementType.Class);
        ReadAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ReadAttribute", ElementType.Class);
        GlobalReadAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "GlobalReadAttribute", ElementType.Class);
        GlobalWriteAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "GlobalWriteAttribute", ElementType.Class);
        GlobalAccessAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "GlobalAccessAttribute", ElementType.Class);
        FreshAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "FreshAttribute", ElementType.Class);
        EscapesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "EscapesAttribute", ElementType.Class);
        /*  */

      StateIndependentAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "StateIndependentAttribute", ElementType.Class);
      SpecPublicAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SpecPublicAttribute", ElementType.Class);
      SpecProtectedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SpecProtectedAttribute", ElementType.Class);
      SpecInternalAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "SpecInternalAttribute", ElementType.Class);

      RepAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RepAttribute", ElementType.Class);
      PeerAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "PeerAttribute", ElementType.Class);
      CapturedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "CapturedAttribute", ElementType.Class);
      LockProtectedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "LockProtectedAttribute", ElementType.Class);
      RequiresLockProtectedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RequiresLockProtectedAttribute", ElementType.Class);
      ImmutableAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ImmutableAttribute", ElementType.Class);
      RequiresImmutableAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RequiresImmutableAttribute", ElementType.Class);
      RequiresCanWriteAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "RequiresCanWriteAttribute", ElementType.Class);

      ModelAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModelAttribute", ElementType.Class);
      RequiresAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "RequiresAttribute", ElementType.Class);
      EnsuresAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "EnsuresAttribute", ElementType.Class);
      ModifiesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ModifiesAttribute", ElementType.Class);
      HasWitnessAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "HasWitnessAttribute", ElementType.Class);
      WitnessAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "WitnessAttribute", ElementType.Class);
      InferredReturnValueAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InferredReturnValueAttribute", ElementType.Class);
      ThrowsAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ThrowsAttribute", ElementType.Class);
      DoesAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DoesAttribute", ElementType.Class);
      InvariantAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "InvariantAttribute", ElementType.Class);
      NoDefaultContractAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "NoDefaultContractAttribute", ElementType.Class);
      ReaderAttribute = (Class)GetCompilerRuntimeTypeNodeFor(CompilerGuardsNs, "ReaderAttribute", ElementType.Class);

      ShadowsAssemblyAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ShadowsAssemblyAttribute", ElementType.Class);
      VerifyAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "VerifyAttribute", ElementType.Class);
      DependentAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "DependentAttribute", ElementType.Class);
      ElementsRepAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementsRepAttribute", ElementType.Class);
      ElementsPeerAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementsPeerAttribute", ElementType.Class);
      ElementAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementAttribute", ElementType.Class);
      ElementCollectionAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ElementCollectionAttribute", ElementType.Class);
      RecursionTerminationAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "RecursionTerminationAttribute", ElementType.Class);
      NoReferenceComparisonAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "NoReferenceComparisonAttribute", ElementType.Class);
      ResultNotNewlyAllocatedAttribute = (Class)GetCompilerRuntimeTypeNodeFor(ContractsNs, "ResultNotNewlyAllocatedAttribute", ElementType.Class);
#endregion
      #endregion
#endif
      SystemTypes.Initialized = true;
      object dummy = TargetPlatform.AssemblyReferenceFor; //Force selection of target platform
      if (dummy == null) return;
    }
    private static void ClearStatics(){
      systemRuntimeWindowsRuntimeAssembly = null;
      systemRuntimeWindowsRuntimeUIXamlAssembly = null;
      AttributeUsageAttribute = null;
      ConditionalAttribute = null;
      DefaultMemberAttribute = null;
      InternalsVisibleToAttribute = null;
      ObsoleteAttribute = null;

      GenericICollection = null;
      GenericIEnumerable = null;
      GenericIList = null;
      ICloneable = null;
      ICollection = null;
      IEnumerable = null;
      IList = null;
#if !MinimalReader
      //Special attributes    
      AllowPartiallyTrustedCallersAttribute = null;
      AssemblyCompanyAttribute = null;
      AssemblyConfigurationAttribute = null;
      AssemblyCopyrightAttribute = null;
      AssemblyCultureAttribute = null;
      AssemblyDelaySignAttribute = null;
      AssemblyDescriptionAttribute = null;
      AssemblyFileVersionAttribute = null;
      AssemblyFlagsAttribute = null;
      AssemblyInformationalVersionAttribute = null;
      AssemblyKeyFileAttribute = null;
      AssemblyKeyNameAttribute = null;
      AssemblyProductAttribute = null;
      AssemblyTitleAttribute = null;
      AssemblyTrademarkAttribute = null;
      AssemblyVersionAttribute = null;
      ClassInterfaceAttribute = null;
      CLSCompliantAttribute = null;
      ComImportAttribute = null;
      ComRegisterFunctionAttribute = null;
      ComSourceInterfacesAttribute = null;
      ComUnregisterFunctionAttribute = null;
      ComVisibleAttribute = null;
      DebuggableAttribute = null;
      DebuggerHiddenAttribute = null;
      DebuggerStepThroughAttribute = null;
      DebuggingModes = null;
      DllImportAttribute = null;
      FieldOffsetAttribute = null;
      FlagsAttribute = null;
      GuidAttribute = null;
      ImportedFromTypeLibAttribute = null;
      InAttribute = null;
      IndexerNameAttribute = null;
      InterfaceTypeAttribute = null;
      MethodImplAttribute = null;
      NonSerializedAttribute = null;
      OptionalAttribute = null;
      OutAttribute = null;
      ParamArrayAttribute = null;
      RuntimeCompatibilityAttribute = null;
      SatelliteContractVersionAttribute = null;
      SerializableAttribute = null;
      SecurityAttribute = null;
      SecurityCriticalAttribute = null;
      SecurityTransparentAttribute = null;
      SecurityTreatAsSafeAttribute = null;
      STAThreadAttribute = null;
      StructLayoutAttribute = null;
      SuppressMessageAttribute = null;
      SuppressUnmanagedCodeSecurityAttribute = null;
      SecurityAction = null;

      //Classes need for System.TypeCode
      DBNull = null;
      DateTime = null;
      DateTimeOffset = null;
      TimeSpan = null;

      //Classes and interfaces used by the Framework
      Activator = null;
      AppDomain = null;
      ApplicationException = null;
      ArgumentException = null;
      ArgumentNullException = null;
      ArgumentOutOfRangeException = null;
      ArrayList = null;
      AsyncCallback = null;
      Assembly = null;
      AttributeTargets = null;
      CodeAccessPermission = null;
      CollectionBase = null;
      color.Clear();
      cornerRadius.Clear();
      CultureInfo = null;
      DictionaryBase = null;
      DictionaryEntry = null;
      DuplicateWaitObjectException = null;
      duration.Clear();
      durationType.Clear();
      Environment = null;
      EventArgs = null;
      EventRegistrationToken = null;
      ExecutionEngineException = null;
      generatorPosition.Clear();
      GenericArraySegment = null;
  #if !WHIDBEYwithGenerics
      GenericArrayToIEnumerableAdapter = null;
  #endif
      GenericDictionary = null;
      GenericIComparable = null;
      GenericIComparer = null;
      GenericIDictionary = null;
      GenericIEnumerator = null;
      GenericKeyValuePair = null;
      GenericList = null;
      GenericNullable = null;
      GenericQueue = null;
      GenericSortedDictionary = null;
      GenericStack = null;
      GC = null;
      gridLength.Clear();
      gridUnitType.Clear();
      Guid = null;
      __HandleProtector = null;
      HandleRef = null;
      Hashtable = null;
      IASyncResult = null;
      IComparable = null;
      IDictionary = null;
      IComparer = null;
      IDisposable = null;
      IEnumerator = null;
      IFormatProvider = null;
      IHashCodeProvider = null;
      IMembershipCondition = null;
      IndexOutOfRangeException = null;
      InvalidCastException = null;
      InvalidOperationException = null;
      IPermission = null;
      ISerializable = null;
      IStackWalk = null;
      keyTime.Clear();
      Marshal = null;
      MarshalByRefObject = null;
      matrix.Clear();
      matrix3D.Clear();
      MemberInfo = null;
      NativeOverlapped = null;
      Monitor = null;
      NotSupportedException = null;
      NullReferenceException = null;
      OutOfMemoryException = null;
      ParameterInfo = null;
      point.Clear();
      Queue = null;
      ReadOnlyCollectionBase = null;
      rect.Clear();
      repeatBehavior.Clear();
      repeatBehaviorType.Clear();
      ResourceManager = null;
      ResourceSet = null;
      SerializationInfo = null;
      size.Clear();
      Stack = null;
      StackOverflowException = null;
      Stream = null;
      StreamingContext = null;
      StringBuilder = null;
      StringComparer = null;
      StringComparison = null;
      SystemException = null;
      thickness.Clear();
      Thread = null;
      Uri = null;
      WindowsImpersonationContext = null;
#endif
#if ExtendedRuntime
      AnonymousAttribute = null;
      AnonymityEnum = null;
      ComposerAttribute = null;
      CustomVisitorAttribute = null;
      TemplateAttribute = null;
      TemplateInstanceAttribute = null;
      UnmanagedStructTemplateParameterAttribute = null;
      TemplateParameterFlagsAttribute = null;
      GenericBoxed = null;
      GenericIEnumerableToGenericIListAdapter = null;
      GenericInvariant = null;
      GenericNonEmptyIEnumerable = null;
      GenericNonNull = null;
      GenericStreamUtility = null;
      GenericUnboxer = null;
      ElementTypeAttribute = null;
      IDbTransactable = null;
      IAggregate = null;
      IAggregateGroup = null;
      StreamNotSingletonException = null;
      SqlHint = null;
      SqlFunctions = null;
      XmlAttributeAttributeClass = null;
      XmlChoiceIdentifierAttributeClass = null;
      XmlElementAttributeClass = null;
      XmlIgnoreAttributeClass = null;
      XmlTypeAttributeClass = null;
      INullable = null;
      SqlBinary = null;
      SqlBoolean = null;
      SqlByte = null;
      SqlDateTime = null;
      SqlDecimal = null;
      SqlDouble = null;
      SqlGuid = null;
      SqlInt16 = null;
      SqlInt32 = null;
      SqlInt64 = null;
      SqlMoney = null;
      SqlSingle = null;
      SqlString = null;
      IDbConnection = null;
      IDbTransaction = null;
      IsolationLevel = null;

      //OrdinaryExceptions
      NoChoiceException = null;
      IllegalUpcastException = null;
      //NonNull  
      Range = null;
      //Invariants
      InitGuardSetsDelegate = null;
      CheckInvariantDelegate = null;
      ObjectInvariantException = null;
      ThreadConditionDelegate = null;
      GuardThreadStart = null;
      Guard = null;
      ContractMarkers = null;
      //IReduction = null;
      AssertHelpers = null;
      ThreadStart = null;
      //CheckedExceptions
      ICheckedException = null;
      CheckedException = null;

      // Contracts
      UnreachableException = null;
      ContractException = null;
      NullTypeException = null;
      AssertException = null;
      AssumeException = null;
      InvalidContractException = null;
      RequiresException = null;
      EnsuresException = null;
      ModifiesException = null;
      ThrowsException = null;
      DoesException = null;
      InvariantException = null;
      ContractMarkerException = null;
      PreAllocatedExceptions = null;

      AdditiveAttribute = null;
      InsideAttribute = null;
      SpecPublicAttribute = null;
      SpecProtectedAttribute = null;
      SpecInternalAttribute = null;
      PureAttribute = null;
      ReadsAttribute = null;
      RepAttribute = null;
      PeerAttribute = null;
      CapturedAttribute = null;
      LockProtectedAttribute = null;
      RequiresLockProtectedAttribute = null;
      ImmutableAttribute = null;
      RequiresImmutableAttribute = null;
      RequiresCanWriteAttribute = null;
      StateIndependentAttribute = null;
      ConfinedAttribute = null;
      ModelfieldContractAttribute = null;
      ModelfieldAttribute = null;
      SatisfiesAttribute = null;
      ModelfieldException = null;

        /* Diego's Attributes for Purity Analysis and Write effects */
      OnceAttribute = null;
      WriteConfinedAttribute = null;
      WriteAttribute = null;
      ReadAttribute = null;
      GlobalReadAttribute = null;
      GlobalWriteAttribute = null;
      GlobalAccessAttribute = null;
      FreshAttribute = null;
      EscapesAttribute = null;
        /* */

      ModelAttribute = null;
      RequiresAttribute = null;
      EnsuresAttribute = null;
      ModifiesAttribute = null;
      HasWitnessAttribute = null;
      WitnessAttribute = null;
      InferredReturnValueAttribute = null;
      ThrowsAttribute = null;
      DoesAttribute = null;
      InvariantAttribute = null;
      NoDefaultContractAttribute = null;
      ReaderAttribute = null;
      ShadowsAssemblyAttribute = null;
      VerifyAttribute = null;
      DependentAttribute = null;
      ElementsRepAttribute = null;
      ElementsPeerAttribute = null;
      ElementAttribute = null;
      ElementCollectionAttribute = null;
      RecursionTerminationAttribute = null;
      NoReferenceComparisonAttribute = null;
      ResultNotNewlyAllocatedAttribute = null;
      noHeapAllocationAttribute = null;
#endif
    }

    private static AssemblyNode/*!*/ GetSystemDllAssembly(bool doNotLockFile, bool getDebugInfo) {
      System.Reflection.AssemblyName aName = typeof(System.Uri).Assembly.GetName();
      Identifier SystemId = Identifier.For(aName.Name);
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[SystemId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = aName.Name;
        aref.PublicKeyOrToken = aName.GetPublicKeyToken();
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[SystemId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemDllAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemDllAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.dll");
        else
          SystemDllAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemDllAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
#if !NoData && !ROTOR
    private static AssemblyNode/*!*/ GetSystemDataAssembly(bool doNotLockFile, bool getDebugInfo) {
      System.Reflection.AssemblyName aName = typeof(System.Data.IDataReader).Assembly.GetName();
      Identifier SystemDataId = Identifier.For(aName.Name);
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[SystemDataId.UniqueIdKey];
      if (aref == null){
        aref = new AssemblyReference();
        aref.Name = aName.Name;
        aref.PublicKeyOrToken = aName.GetPublicKeyToken();
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[SystemDataId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemDataAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemDataAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Data.dll");
        else
          SystemDataAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemDataAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref);
    }
#endif
#if !NoXml && !NoRuntimeXml
    private static AssemblyNode/*!*/ GetSystemXmlAssembly(bool doNotLockFile, bool getDebugInfo) {
      System.Reflection.AssemblyName aName = typeof(System.Xml.XmlNode).Assembly.GetName();
      Identifier SystemXmlId = Identifier.For(aName.Name);
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[SystemXmlId.UniqueIdKey];
      if (aref == null){
        aref = new AssemblyReference();
        aref.Name = aName.Name;
        aref.PublicKeyOrToken = aName.GetPublicKeyToken();
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[SystemXmlId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemXmlAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemXmlAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Xml.dll");
        else
          SystemXmlAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemXmlAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref);
    }
#endif
    private static TypeNode/*!*/ GetGenericRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0) name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
#if ExtendedRuntime
      if (TargetPlatform.TargetVersion != null && TargetPlatform.TargetVersion.Major == 1 && TargetPlatform.TargetVersion.Minor < 2)
        return SystemTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, typeCode);
      else
#endif
        return SystemTypes.GetTypeNodeFor(nspace, name, typeCode);
    }
    private static TypeNode/*!*/ GetTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemAssembly == null)
        Debug.Assert(false);
      else
        result = SystemAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static TypeNode/*!*/ GetSystemTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemDllAssembly == null)
        Debug.Assert(false);
      else
        result = SystemDllAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemDllAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetCollectionsAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Collections");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Collections";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeCollectionsAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeCollectionsAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Collections.dll");
        else
          SystemRuntimeCollectionsAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeCollectionsAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetCollectionsGenericRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0) name = name + TargetPlatform.GenericTypeNamesMangleChar +numParams;
#if ExtendedRuntime
      if (TargetPlatform.TargetVersion != null && TargetPlatform.TargetVersion.Major == 1 && TargetPlatform.TargetVersion.Minor < 2)
        return SystemTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, typeCode);
      else
#endif
      return SystemTypes.GetCollectionsTypeNodeFor(nspace, name, typeCode);
    }
    private static TypeNode/*!*/ GetCollectionsTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (CollectionsAssembly == null)
        Debug.Assert(false);
      else
        result = CollectionsAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(CollectionsAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetDiagnosticsDebugAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Diagnostics.Debug");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Diagnostics.Debug";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemDiagnosticsDebugAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemDiagnosticsDebugAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Diagnostics.Debug.dll");
        else
          SystemDiagnosticsDebugAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemDiagnosticsDebugAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetDiagnosticsDebugTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (DiagnosticsDebugAssembly == null)
        Debug.Assert(false);
      else
        result = DiagnosticsDebugAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(DiagnosticsDebugAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetDiagnosticsToolsAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Diagnostics.Tools");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Diagnostics.Debug";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemDiagnosticsToolsAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemDiagnosticsToolsAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Diagnostics.Tools.dll");
        else
          SystemDiagnosticsToolsAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemDiagnosticsToolsAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetDiagnosticsToolsTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (DiagnosticsToolsAssembly == null)
        Debug.Assert(false);
      else
        result = DiagnosticsToolsAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(DiagnosticsToolsAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetInteropAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Runtime.InteropServices");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Runtime.InteropServices";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeInteropServicesAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeInteropServicesAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.InteropServices.dll");
        else
          SystemRuntimeInteropServicesAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeInteropServicesAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetInteropTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemAssembly == null)
        Debug.Assert(false);
      else
        result = InteropAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(InteropAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetIOAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.IO");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.IO";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeIOServicesAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeIOServicesAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.IO.dll");
        else
          SystemRuntimeIOServicesAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeIOServicesAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetIOTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (IOAssembly == null)
        Debug.Assert(false);
      else
        result = IOAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(IOAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetReflectionAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Reflection");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Reflection";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemReflectionAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemReflectionAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Reflection.dll");
        else
          SystemReflectionAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemReflectionAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetReflectionTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (ReflectionAssembly == null)
        Debug.Assert(false);
      else
        result = ReflectionAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(ReflectionAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetResourceManagerAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Resources.ResourceManager");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Resources.ResourceManager";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemResourceManagerAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemResourceManagerAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Resources.ResourceManager.dll");
        else
          SystemResourceManagerAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemResourceManagerAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetResourceManagerTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (ReflectionAssembly == null)
        Debug.Assert(false);
      else
        result = ReflectionAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(ReflectionAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetGlobalizationAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Globalization");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Globalization";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemGlobalizationAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemGlobalizationAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Globalization.dll");
        else
          SystemGlobalizationAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemGlobalizationAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetGlobalizationTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (GlobalizationAssembly == null)
        Debug.Assert(false);
      else
        result = GlobalizationAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(GlobalizationAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetRuntimeExtensionsAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Runtime.Extensions");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Runtime.Extensions";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeExtensionsAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeExtensionsAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.Extensions.dll");
        else
          SystemRuntimeExtensionsAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeExtensionsAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetRuntimeExtensionsTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemRuntimeExtensionsAssembly == null)
        Debug.Assert(false);
      else
        result = SystemRuntimeExtensionsAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeExtensionsAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetRuntimeSerializationAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Serialization.DataContract");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Serialization.DataContract";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeSerializationAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeSerializationAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Serialization.DataContract.dll");
        else
          SystemRuntimeSerializationAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeSerializationAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetRuntimeSerializationTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemRuntimeSerializationAssembly == null)
        Debug.Assert(false);
      else
        result = SystemRuntimeSerializationAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeSerializationAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetThreadingAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier ThreadingId = Identifier.For("System.Threading");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[ThreadingId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Threading";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[ThreadingId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemThreadingAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemThreadingAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Threading.dll");
        else
          SystemThreadingAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemThreadingAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetThreadingTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (ThreadingAssembly == null)
        Debug.Assert(false);
      else
        result = ThreadingAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(ThreadingAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetWindowsRuntimeInteropAssembly(bool doNotLockFile, bool getDebugInfo) {
      if (SystemAssembly.Name == "mscorlib") return SystemAssembly;
      Identifier AssemblyId = Identifier.For("System.Runtime.InteropServices.WindowsRuntime");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Runtime.InteropServices.WindowsRuntime";
        aref.PublicKeyOrToken = new byte[] { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.InteropServices.WindowsRuntime.dll");
        else
          SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeWindowsRuntimeInteropServicesAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetWindowsRuntimeInteropTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemRuntimeWindowsRuntimeInteropAssembly == null)
        Debug.Assert(false);
      else
        result = SystemRuntimeWindowsRuntimeInteropAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeWindowsRuntimeInteropAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetSystemRuntimeWindowsRuntimeAssembly(bool doNotLockFile, bool getDebugInfo) {
      Identifier AssemblyId = Identifier.For("System.Runtime.WindowsRuntime");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Runtime.WindowsRuntime";
        aref.PublicKeyOrToken = new byte[] {0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeWindowsRuntimeAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeWindowsRuntimeAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.WindowsRuntime.dll");
        else
          SystemRuntimeWindowsRuntimeAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeWindowsRuntimeAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }    
    private static TypeNode/*!*/ GetGenericWindowsRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0) name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
      return SystemTypes.GetWindowsRuntimeTypeNodeFor(nspace, name, typeCode);
    }
    private static TypeNode/*!*/ GetWindowsRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemRuntimeWindowsRuntimeAssembly == null)
        Debug.Assert(false);
      else
        result = SystemRuntimeWindowsRuntimeAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeWindowsRuntimeAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
    private static AssemblyNode/*!*/ GetSystemRuntimeWindowsRuntimeUIXamlAssembly(bool doNotLockFile, bool getDebugInfo) {
      Identifier AssemblyId = Identifier.For("System.Runtime.WindowsRuntime.UI.Xaml");
      AssemblyReference aref = (AssemblyReference)TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey];
      if (aref == null) {
        aref = new AssemblyReference();
        aref.Name = "System.Runtime.WindowsRuntime.UI.Xaml";
        aref.PublicKeyOrToken = new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
        aref.Version = TargetPlatform.TargetVersion;
        TargetPlatform.AssemblyReferenceFor[AssemblyId.UniqueIdKey] = aref;
      }
      if (string.IsNullOrEmpty(SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location)) {
        if (aref.Location == null)
          SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location = Path.Combine(Path.GetDirectoryName(SystemAssemblyLocation.Location), "System.Runtime.WindowsRuntime.UI.Xaml.dll");
        else
          SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location = aref.Location;
      }
      if (aref.assembly == null) aref.Location = SystemRuntimeWindowsRuntimeUIXamlAssemblyLocation.Location;
      return aref.assembly = AssemblyNode.GetAssembly(aref, doNotLockFile, getDebugInfo, true);
    }
    private static TypeNode/*!*/ GetGenericWindowsRuntimeUIXamlTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0) name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
      return SystemTypes.GetWindowsRuntimeUIXamlTypeNodeFor(nspace, name, typeCode);
    }
    private static TypeNode/*!*/ GetWindowsRuntimeUIXamlTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemRuntimeWindowsRuntimeUIXamlAssembly == null)
        Debug.Assert(false);
      else
        result = SystemRuntimeWindowsRuntimeUIXamlAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemRuntimeWindowsRuntimeUIXamlAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#if ExtendedRuntime
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      return SystemTypes.GetCompilerRuntimeTypeNodeFor(nspace, name, 0, typeCode);
    }
    private static TypeNode/*!*/ GetCompilerRuntimeTypeNodeFor(string/*!*/ nspace, string/*!*/ name, int numParams, ElementType typeCode) {
      if (TargetPlatform.GenericTypeNamesMangleChar != 0 && numParams > 0)
        name = name + TargetPlatform.GenericTypeNamesMangleChar + numParams;
      TypeNode result = null;
      if (SystemCompilerRuntimeAssembly == null)
        Debug.Assert(false);
      else
        result = SystemCompilerRuntimeAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemCompilerRuntimeAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#if !NoData
    private static TypeNode/*!*/ GetDataTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemDataAssembly == null)
        Debug.Assert(false);
      else
        result = SystemDataAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemDataAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#endif
#if !NoXml && !NoRuntimeXml
    private static TypeNode/*!*/ GetXmlTypeNodeFor(string/*!*/ nspace, string/*!*/ name, ElementType typeCode) {
      TypeNode result = null;
      if (SystemXmlAssembly == null)
        Debug.Assert(false);
      else
        result = SystemXmlAssembly.GetType(Identifier.For(nspace), Identifier.For(name));
      if (result == null) result = CoreSystemTypes.GetDummyTypeNode(SystemXmlAssembly, nspace, name, typeCode);
      result.typeCode = typeCode;
      return result;
    }
#endif
#endif
#if ExtendedRuntime
    /// <summary>
    /// Finds assembly references to mscorlib in assem and redirects them to SystemTypes.SystemAssembly
    /// </summary>
    public static void UnifyMsCorlibReferences(AssemblyNode assem)
    {
      if (assem == null) return;
      foreach (AssemblyReference aref in assem.AssemblyReferences)
      {
        if (aref.Name == "mscorlib")
        {
          aref.Assembly = SystemTypes.SystemAssembly;
        }
      }
    }
#endif

  }
}
