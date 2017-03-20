using System;
using System.IO;
using System.Collections;
using System.Compiler;

namespace AssemblyResolving
{
	/// <summary>
	/// AssemblyResolver is a class that is used for CCI-based applications.
	/// When a CCI-based application asks for an assembly to be loaded, CCI
	/// first looks in any cache that it has access to (e.g., that was handed
	/// to it in a call to GetAssembly). If it cannot find it, then if a 
	/// resolver has been registered, then it calls the resolver to locate
	/// the assembly.
	/// If the resolver does not return it, CCI then searches in several
	/// standard places (the current working directory, the GAC, etc.).
	/// </summary>
	public class AssemblyResolver
	{
		private readonly static string[] exts = new string[] { "dll", "exe", "winmd", };
		private string inputDirectory = "";
		private string[] directories = null;
		private IDictionary h = null;
		private bool log = true;
		private string logFile = null;
		private bool useGlobalCache = true;
		private bool debugInfo = true;
		private bool shortB = false;

        /// <summary>
        /// This object is used to locate an assembly when it is needed to be loaded
        /// (i.e., not found in the cache).
        /// </summary>
        public AssemblyResolver() { }
        /// <summary>
        /// This object is used to locate an assembly when it is needed to be loaded
        /// (i.e., not found in the cache).
        /// </summary>
        /// <param name="InputDirectory">Specifies the primary directory in which to look for assemblies.</param>
		public AssemblyResolver(string InputDirectory)
		{
			if ( InputDirectory == null )
				throw new System.ArgumentNullException();
			inputDirectory = InputDirectory;
		}
        /// <summary>
        /// This object is used to locate an assembly when it is needed to be loaded
        /// (i.e., not found in the cache).
        /// </summary>
        /// <param name="AssemblyCache">A map from assembly names (strings) to AssemblyNode. First
        /// place that is looked in for assemblies to be loaded.</param>
        public AssemblyResolver(IDictionary AssemblyCache)
		{
			h = AssemblyCache;
		}
        /// <summary>
        /// A map from string names to AssemblyNode. It is used to locate
        /// metadata references.
        /// </summary>
        public IDictionary AssemblyCache
		{
			set { h = value; }
		}
        /// <summary>
        /// The set of directory paths that will be searched for assemblies by the 
        /// assembly resolver.
        /// </summary>
        public string[] SearchDirectories
		{
			set 
			{
				if ( value == null )
					throw new System.ArgumentNullException();
				else if ( value.Length > 0 )
				{
					directories = new string[value.Length];
					value.CopyTo(directories,0);
				}
			}
		}
        /// <summary>
        /// The first directory in which an assembly is searched for if it is not found in the cache.
        /// </summary>
        public string InputDirectory 
		{
			set { 
				if ( value == null )
					throw new System.ArgumentNullException();
				inputDirectory = value;
			}
		}
		/// <summary>
		/// Place to send output log messages. Needs to be a full path, e.g., "c:\tmp\foo.log".
		/// Can be set at most once.
		/// (If the property Log is true and the LogFile is null, then Console.Out is written to.)
		/// </summary>
		public string LogFile
		{
			get { return logFile; }
			set
			{
				// only allow it to be set once (why?)
				if ( logFile == null )
					logFile = value;
				else
					throw new InvalidOperationException("AssemblyResolver: Can set the log only once.");
			}
		}
		/// <summary>
		/// Controls whether output log messages are produced during resolving. (default: true)
		/// Can be set arbitrarily.
		/// (If the property Log is true and the LogFile is null, then Console.Out is written to.)
		/// </summary>
		public bool Log
		{
			get { return log; }
			set { log = value; }
		}
		/// <summary>
		/// Controls whether CCI should use its global cache when loading assemblies. (default: true)
		/// </summary>
		public bool UseGlobalCache
		{
			get { return useGlobalCache; }
			set { useGlobalCache = value; }
		}
		/// <summary>
		/// Controls whether CCI should read in debug info when loading assemblies. (default: true)
		/// </summary>
		public bool DebugInfo
		{
			get { return debugInfo; }
			set { debugInfo = value; }
		}
		/// <summary>
		/// Controls whether CCI should preserve short branches. (default: false)
		/// </summary>
		public bool PreserveShortBranches
		{
			get { return shortB; }
			set { shortB = value; }
		}
		private void WriteToLog(string s, params object[] args)
		{
			if ( log )
			{
				if ( logFile != null )
				{
					StreamWriter writer = new System.IO.StreamWriter(logFile, true);
					writer.WriteLine(s,args);
					writer.Close();
				}
				else
				{
					Console.WriteLine(s,args);
				}
			}
		}
        /// <summary>
        /// This method is installed as hook so that each assembly that is loaded uses it to resolve
        /// any assembly references.
        /// </summary>
        /// <param name="assemblyReference">The reference that must be chased down to load.</param>
        /// <param name="referencingModule">The assembly that contains the reference.</param>
        /// <returns></returns>
		public AssemblyNode Resolve(AssemblyReference assemblyReference, Module referencingModule)
		{
			WriteToLog("AssemblyResolver: Assembly '{0}' is referencing assembly '{1}'.",
				referencingModule.Name,assemblyReference.Name);
			AssemblyNode a = null;
			try 
			{
				// Location Priority (in decreasing order):
				// 1. referencing Module's directory
				// 2. directory original assembly was in
				// 3. list of directories specified by client
        // 4. Framework directory
				//
				// Extension Priority (in decreasing order):
				// dll, exe, (any others?)
				#region Check referencing module's directory
				WriteToLog("\tAssemblyResolver: Attempting referencing assembly's directory.");
				if ( referencingModule.Directory != null ) 
				{
					foreach (string ext in exts)
					{
						bool tempDebugInfo = debugInfo;
						string fullName = Path.Combine(referencingModule.Directory, assemblyReference.Name + "." + ext);
						if ( File.Exists(fullName) ) {
							if (tempDebugInfo) {
								// Don't pass the debugInfo flag to GetAssembly unless the PDB file exists.
								string pdbFullName = Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb");
								if (!File.Exists(pdbFullName)) {
									WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.",
										assemblyReference.Name);
									tempDebugInfo = false;
								}
							}
							WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used referencing Module's directory.)",
								assemblyReference.Name,
								fullName);
							a = AssemblyNode.GetAssembly(
								fullName, // path to assembly
								h,  // global cache to use for assemblies
								true, // doNotLockFile
								tempDebugInfo, // getDebugInfo
								useGlobalCache, // useGlobalCache
								shortB  // preserveShortBranches
								);
							break;
						}
					}
				}
				else
				{
					WriteToLog("\t\tAssemblyResolver: Referencing assembly's directory is null.");
				}
				if ( a == null )
				{
					if ( referencingModule.Directory != null ) 
					{
						WriteToLog("\tAssemblyResolver: Did not find assembly in referencing assembly's directory.");
					}
				}
				else
				{
					goto End;
				}
				#endregion
				#region Check input directory
				WriteToLog("\tAssemblyResolver: Attempting input directory.");
				foreach ( string ext in exts )
				{
					bool tempDebugInfo = debugInfo;
					string fullName = Path.Combine(inputDirectory, assemblyReference.Name + "." + ext);
					if ( File.Exists(fullName) )
					{
						WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used the original input directory.)",
							assemblyReference.Name,
							fullName);
						if (tempDebugInfo) {
							// Don't pass the debugInfo flag to GetAssembly unless the PDB file exists.
							string pdbFullName = Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb");
							if (!File.Exists(pdbFullName)) {
								WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.",
									assemblyReference.Name);
								tempDebugInfo = false;
							}
						}
						a = AssemblyNode.GetAssembly(
							fullName, // path to assembly
							h,  // global cache to use for assemblies
							true, // doNotLockFile
							tempDebugInfo, // getDebugInfo
							useGlobalCache, // useGlobalCache
							shortB // preserveShortBranches
							);
						break;
					}
				}
				if ( a == null )
				{
					WriteToLog("\tAssemblyResolver: Did not find assembly in input directory.");
				}
				else
				{
					goto End;
				}
				#endregion
				#region Check user-supplied search directories
				WriteToLog("\tAssemblyResolver: Attempting user-supplied directories.");
				if ( directories != null )
				{
					foreach ( string dir in directories )
					{
						foreach ( string ext in exts )
						{
							string fullName = dir + "\\" + assemblyReference.Name + "." + ext;
							if ( File.Exists(fullName) )
							{
								bool tempDebugInfo = debugInfo;
								WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used a client-supplied directory.)",
									assemblyReference.Name,
									fullName);
								if (tempDebugInfo) {
									// Don't pass the debugInfo flag to GetAssembly unless the PDB file exists.
									string pdbFullName = Path.Combine(referencingModule.Directory, assemblyReference.Name + ".pdb");
									if (!File.Exists(pdbFullName)) {
										WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.",
											assemblyReference.Name);
										tempDebugInfo = false;
									}
								}
								a = AssemblyNode.GetAssembly( //(fullName,h, true, false, true);
									fullName, // path to assembly
									h,  // global cache to use for assemblies
									true, // doNotLockFile
									tempDebugInfo, // getDebugInfo
									useGlobalCache, // useGlobalCache
									shortB // preserveShortBranches
									);
								break;
							}
						}
						if ( a != null )
							break;
					}
				}
				else
				{
					WriteToLog("\tAssemblyResolver: No user-supplied directories.");
				}
				if ( a == null )
				{
					if ( directories != null ) 
					{
						WriteToLog("\tAssemblyResolver: Did not find assembly in user-supplied directories.");
					}
				}
				else
				{
					goto End;
				}
				#endregion
        #region Check framework directory
        WriteToLog("\tAssemblyResolver: Attempting framework directory.");
        if (TargetPlatform.PlatformAssembliesLocation != null) {
          var directory = TargetPlatform.PlatformAssembliesLocation;
          foreach (string ext in exts) {
            bool tempDebugInfo = debugInfo;
            string fullName = Path.Combine(directory, assemblyReference.Name + "." + ext);
            if (File.Exists(fullName)) {
              if (tempDebugInfo) {
                // Don't pass the debugInfo flag to GetAssembly unless the PDB file exists.
                string pdbFullName = Path.Combine(directory, assemblyReference.Name + ".pdb");
                if (!File.Exists(pdbFullName)) {
                  WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.",
                    assemblyReference.Name);
                  tempDebugInfo = false;
                }
              }
              WriteToLog("Resolved assembly reference '{0}' to '{1}'. (Used framework directory.)",
                assemblyReference.Name,
                fullName);
              a = AssemblyNode.GetAssembly(
                fullName, // path to assembly
                h,  // global cache to use for assemblies
                true, // doNotLockFile
                tempDebugInfo, // getDebugInfo
                useGlobalCache, // useGlobalCache
                shortB  // preserveShortBranches
                );
              break;
            }
          }
        } else {
          WriteToLog("\t\tAssemblyResolver: Platform assemblies location is null.");
        }
        if (a == null) {
          if (referencingModule.Directory != null) {
            WriteToLog("\tAssemblyResolver: Did not find assembly in framework directory.");
          }
        } else {
          goto End;
        }
        #endregion
      End:
				if ( a == null )
					WriteToLog("AssemblyResolver: Unable to resolve reference. (It still might be found, e.g., in the GAC.)");
			}
			catch (Exception e)
			{
				WriteToLog("AssemblyResolver: Exception occurred. Unable to resolve reference.");
				WriteToLog("Inner exception: " + e.ToString());
			}
			return a;
		}
	}
}
