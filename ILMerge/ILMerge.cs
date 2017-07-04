using System;
using System.IO;
using System.Compiler;
using System.Collections;
using System.Text.RegularExpressions;
using AssemblyResolving;

namespace ILMerging {
  /// <summary>
  /// ILMerge is a class containing members and methods for
  /// merging multiple .NET assemblies into a single assembly.
  /// </summary>
  public class ILMerge {
    #region Private Variables
    private Class hiddenClass = null;
    private AssemblyNode targetAssembly = null;
    private Duplicator d = null;

    private string[] directories = null;
    private ArrayList searchDirs = new ArrayList();
    private bool log = false;
    private string logFile = null;
    private Kind targetKind = Kind.SameAsPrimaryAssembly;
    private string outputFileName = null;
    private ArrayList assemblyNames = null;
    private bool keyfileSpecified = false;
    private string keyfile = null;
    private bool keyContainerSpecified = false;
    private string keyContainer = null;
    private bool strongNameLost = false;
    private bool closed = false;
    private bool debugInfo = true;
    private bool shortB = false;
    private bool internalize = false;
    private string excludeFile = "";
    private ArrayList exemptionList = null;
    private System.Version version = null;
    private string attributeFileName = null;
    private AssemblyNode attributeAssembly = null;
    private bool copyattrs = false;
    private bool allowMultipleAssemblyLevelAttributes = false;
    private bool keepFirstOfMultipleAssemblyLevelAttributes = false;
    private bool copyXmlDocumentation = false;
#if CROSSPLATFORM
    /// <summary>
    /// clrVersion is the string that is needed by SetTargetPlatform
    /// to indicate which version of the CLR the output should run under.
    /// </summary>
    protected string clrVersion = null;
    /// <summary>
    /// clrDir is the string containing the path to the directory
    /// the Framework assemblies can be found in for the version
    /// indicated by clrVersion. It is also passed to SetTargetPlatform.
    /// </summary>
    protected string clrDir = null;
    /// <summary>
    /// Records whether the /targetplatform option was given by the user on 
    /// the command line. It isn't currently settable via the object model
    /// since in that case a user just calls SetTargetPlatform directly.
    /// </summary>
    protected bool targetPlatformSpecified = false;
#endif
    private bool usePublicKeyTokensForAssemblyReferences = true;
    private bool allowZeroPeKind = false;
    private bool allowWildCards = false;
    private bool delaySign = false;
    /// <summary>
    /// Used to represent the set of names of types to allow duplicates of.
    /// This was first implemented to allow assemblies obfuscated by Dotfuscator
    /// to be merged because that tool adds the definition of an attribute to
    /// each output assembly. The definitions are all identical and are not referenced
    /// from anywhere, so renaming this particular public type doesn't matter.
    /// </summary>
    private Hashtable/*string -> bool*/ typesToAllowDuplicatesOf = new Hashtable();
    /// <summary>
    /// If this is true, then all public types are allowed to have duplicates; the duplicates
    /// are just renamed.
    /// </summary>
    private bool allowAllDuplicates = false;
    private bool unionMerge = false;

    private Hashtable typeList = new Hashtable();
    private ArrayList memberList;
    private ArrayList resourceList = new ArrayList();

    private int fileAlignment = 512; // default in Writer

    #region External Visitor (Internal builds only)

#if INTERNAL
    private StandardVisitor externalVisitor = null;
#endif
    #endregion

    /// <summary>
    /// The string shown to the user when there is an error detected in the command line.
    /// </summary>
    protected virtual string UsageString {
      get {
        return
#if CROSSPLATFORM
 "Usage: ilmerge [/lib:directory]* [/log[:filename]] [/keyfile:filename | /keycontainer:containername [/delaysign]] [/internalize[:filename]] [/t[arget]:(library|exe|winexe)] [/closed] [/ndebug] [/ver:version] [/copyattrs [/allowMultiple] [/keepFirst]] [/xmldocs] [/attr:filename] [/targetplatform:<version>[,<platformdir>] | /v1 | /v1.1 | /v2 | /v4] [/useFullPublicKeyForReferences] [/wildcards] [/zeroPeKind] [/allowDup:type]* [/union] [/align:n] /out:filename <primary assembly> [<other assemblies>...]";
#else
          "Usage: ilmerge [/lib:directory]* [/log[:filename]] [/keyfile:filename | /keycontainer:containername [/delaysign]] [/internalize[:filename]] [/t[arget]:(library|exe|winexe)] [/closed] [/ndebug] [/ver:version] [/copyattrs [/allowMultiple] [/keepFirst]] [/xmldocs] [/attr:filename] [/useFullPublicKeyForReferences] [/wildcards] [/zeroPeKind] [/allowDup:type]* [/union] [/align:n] /out:filename <primary assembly> [<other assemblies>...]";
#endif
      }
    }
    #endregion
    #region Private Methods
    private class CloseAssemblies {
      Hashtable visitedAssemblies = new Hashtable();
      internal Hashtable assembliesToBeAdded = new Hashtable();
      private Hashtable currentlyActiveAssemblies = new Hashtable();
      AssemblyNodeList initialAssemblies = null;
      internal CloseAssemblies(AssemblyNodeList assems) {
        initialAssemblies = assems;
        for (int i = 0, n = assems.Count; i < n; i++) {
          visitedAssemblies[assems[i].UniqueKey] = assems[i];
        }
      }
      internal void Visit(AssemblyNode a) {
        if (visitedAssemblies[a.UniqueKey] != null)
          return; // all of a's references (transitively closed) have been visited and a has been considered
        this.currentlyActiveAssemblies.Add(a.UniqueKey, a);
        if (a.AssemblyReferences == null)
          goto End;
        for (int i = 0, n = a.AssemblyReferences.Count; i < n; i++) {
          AssemblyNode refAssembly = a.AssemblyReferences[i].Assembly;
          if (this.currentlyActiveAssemblies[refAssembly.UniqueKey] != null)
            // don't chase back edges!! infinite recursion from
            // cyclic references!!
            continue;
          else
            Visit(refAssembly);
        }
        // if any of a's direct references have been added to the list
        // then add a too.
        for (int i = 0, n = a.AssemblyReferences.Count; i < n; i++) {
          if (assembliesToBeAdded[a.AssemblyReferences[i].Assembly.UniqueKey] != null) {
            assembliesToBeAdded[a.UniqueKey] = a;
            goto End;
          }
        }
        // if any of a's direct references are in the initial set of assemblies,
        // then add a too.
        for (int i = 0, n = a.AssemblyReferences.Count; i < n; i++) {
          for (int j = 0, m = initialAssemblies.Count; j < m; j++) {
            if (a.AssemblyReferences[i].Assembly.StrongName.CompareTo(initialAssemblies[j].StrongName) == 0) {
              assembliesToBeAdded[a.UniqueKey] = a;
              goto End; // no need to look any further
            }
          }
        }
      End:
        visitedAssemblies[a.UniqueKey] = a; // record that a has been visited
        this.currentlyActiveAssemblies.Remove(a.UniqueKey); // but is no longer in the current stack trace
        return;
      }
    }
    private AssemblyNode CreateTargetAssembly(string outputAssemblyName, ModuleKindFlags kind) {
      AssemblyNode assem = new AssemblyNode();
      assem.Name = outputAssemblyName;
      assem.Kind = kind;
      if (assem != null) assem.Version = new Version(0, 0, 0, 0);
      assem.ModuleReferences = new ModuleReferenceList();
      var doc = new System.Xml.XmlDocument();
      doc.XmlResolver = null;
      assem.Documentation = doc;
      assem.AssemblyReferences = new AssemblyReferenceList();
      TypeNodeList types = assem.Types = new TypeNodeList();
      hiddenClass = new Class(
        assem,
        null,
        null,
        TypeFlags.Public,
        Identifier.Empty,
        Identifier.For("<Module>"),
        null,
        new InterfaceList(),
        new MemberList(0));
      types.Add(hiddenClass);
      return assem;
    }
    private bool ExemptType(TypeNode t) {
      if (exemptionList == null)
        return false;
      foreach (Regex r in exemptionList) {
        Match m = r.Match(t.FullName);
        if (m.Success)
          return true;
        m = r.Match("[" + t.DeclaringModule.Name + "]" + t.FullName);
        if (m.Success)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Call this method once for each namespace (top-level) type in an assembly!
    /// Do not call it on nested types!
    /// 
    /// There is a very special case where in one assembly a type C declares a method m
    /// that is FamilyOrAssembly and in another assembly, a type D extends C and overrides
    /// the method as just Family and now both C and D are going to be in the merged assembly.
    /// Since C.m was exported as just Family, it worked when D was in a different assembly,
    /// but when they live in the same assembly, it means that D.m is less accessible than
    /// the method it is overriding and the type fails to load.
    /// </summary>
    /// <param name="t">
    /// Namespace type to recursively visit.
    /// </param>
    /// <param name="assemblyComVisibleAttribute">
    /// When this is non-null then the type t comes from an assembly whose assembly-level
    /// specification for the ComVisible attribute is not the same as the assembly-level
    /// attribute that will be put onto the target assembly. In that case, if it is public,
    /// then the type needs to be marked with the attribute so it doesn't inherit the
    /// target assembly's assembly-level specification.
    /// But do this only if the type is not explicitly marked with the attribute.
    /// If it does then it doesn't inherit the assembly-level specification anyway.
    /// This needs to be done only at the namespace type level: this method will make
    /// sure that all of them are marked so then any nested types inherit those markings.
    /// </param>
    private void AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(TypeNode t, AttributeNode assemblyComVisibleAttribute) {

      if (assemblyComVisibleAttribute != null && t.IsPublic && t.GetAttribute(SystemTypes.ComVisibleAttribute) == null) {
        t.Attributes.Add(assemblyComVisibleAttribute);
      }

      for (int i = 0, n = t.Members.Count; i < n; i++) {
        TypeNode nestedType = t.Members[i] as TypeNode;
        if (nestedType != null) {
          // never need to mark nested types: they will inherit marking from namespace type
          AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(nestedType, null);
          continue; // below code is for members that are not types
        }

        Method m = t.Members[i] as Method;
        if (m == null) continue;
        bool newSlot = (m.Flags & MethodFlags.NewSlot) != 0;
        if (!(m.IsVirtual && m.IsFamily && !newSlot))
          continue;
        // now need to go find the method it is overriding
        // need to look all the way up the inheritance chain because it may not be the
        // closest supertype's declaration that causes this method to have its accessibility
        // adjusted.
        TypeNode baseType = t.BaseType;
        while (baseType != null) {
          Method overridenMethod = baseType.GetImplementingMethod(m, false); // don't look up the chain, doing that explicitly
          if (overridenMethod != null && overridenMethod.IsFamilyOrAssembly
              && overridenMethod.DeclaringType.DeclaringModule == m.DeclaringType.DeclaringModule) {
            m.Flags |= MethodFlags.FamORAssem;
            break; // once its accessibility has been changed, it would never get changed back, so quit searching
          }
          baseType = baseType.BaseType;
        }
      }
    }
    private bool MergeInAssembly(AssemblyNode a, bool makeNonPublic, bool targetAssemblyIsComVisible) {
      #region Check for name conflicts
      // first, make sure there are no name conflicts. It suffices to
      // just check the top-level names since all nested types are
      // qualified by their declaring types.
      // start at Type[1] because Type[0] is the special <Module>
      // class which is treated specially below.
      // If a top-level type is a duplicate, but is not public, then
      // it is safe to just give it a new name since it
      // cannot be referred to from outside of the target assembly anyway.
      // At the user's discretion, they can decide whether there are any public types they don't mind
      // having duplicates of. Primarily useful for obfuscators that define the same attribute name
      // in each obfuscated assembly, but the attribute is used only as an assembly-level attribute
      // (which probably gets lost during merging anyway unless it is on the primary assembly).
      for (int i = 1, n = a.Types.Count; i < n; i++) {
        TypeNode t = a.Types[i];
        TypeNode duplicate = targetAssembly.GetType(t.Namespace, t.Name);
        if (duplicate != null) {
          if (!t.IsPublic || this.allowAllDuplicates || this.typesToAllowDuplicatesOf.ContainsKey(t.Name.Name)) {
            string oldName = t.Name.Name;
            string newName;
            if (IsCompilerGenerated(t))
              newName = a.Name + "." + oldName;
            else
              newName = a.Name + a.UniqueKey.ToString() + "." + oldName;
            WriteToLog("Duplicate type name: modifying name of the type '{0}' (from assembly '{1}') to '{2}'",
              t.FullName, t.DeclaringModule.Name, newName);
            Identifier newId = Identifier.For(newName);
            TypeNode dup = (TypeNode)d.DuplicateFor[t.UniqueKey];
            if (dup == null)
              t.Name = newId; // t hasn't been duplicated yet
            else
              dup.Name = newId; // t has been duplicated already
            #region If a type is renamed, rename any associated resource
            for (int j = 0, m = a.Resources.Count; j < m; j++) {
              Resource r = a.Resources[j];
              if (r.Name.Equals(oldName + ".resources")) {
                WriteToLog("Duplicate resource name: modifying name of the resource '{0}' (from assembly '{1}') to '{2}.resources'",
                  r.Name, t.DeclaringModule.Name, newName);
                r.Name = newName + ".resources";
                a.Resources[j] = r;
                //WriteToLog("\tResource: " + r.Name);
                break; // at most one resource can have the same name?
              }
            }
            #endregion If a type is renamed, rename any associated resource

          } else {
            string msg = String.Format("ERROR!!: Duplicate type '{0}' found in assembly '{1}'. Do you want to use the /allowDup option?",
              t.FullName, t.DeclaringModule.Name);
            WriteToLog(msg);
            throw new InvalidOperationException("ILMerge.Merge: " + msg);
          }
        }
      }
      #endregion
      #region Process the first type, <Module>, specially
      // according to Herman Venter:
      //	The first type of each and every assembly is always the class
      // <Module>. Take its members and add them to the <Module> class
      // in the target assembly; that class was created when the target
      // assembly was created.
      if (a.Types[0].Members != null && a.Types[0].Members.Count > 0) {
        TypeNode tempTypeNode = d.TargetType;
        d.TargetType = hiddenClass;
        for (int i = 0, n = a.Types[0].Members.Count; i < n; i++) {
          Member newMember = (Member)d.Visit(a.Types[0].Members[i]);
          hiddenClass.Members.Add(newMember);
        }
        d.TargetType = tempTypeNode;
      }
      #endregion
      #region Deal with [ComVisible] and security attributes
      var thisAssemblyIsComVisible = GetComVisibleSettingForAssembly(a);
      AttributeNode assemblyComVisibleAttribute = null;
      if (thisAssemblyIsComVisible != targetAssemblyIsComVisible) {
        InstanceInitializer ctor = SystemTypes.ComVisibleAttribute.GetConstructor(SystemTypes.Boolean);
        assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, ctor), new ExpressionList(new Literal(thisAssemblyIsComVisible, SystemTypes.Boolean)));
      }
      for (int i = 0, n = a.Attributes == null ? 0 : a.Attributes.Count; i < n; i++) {
        AttributeNode aNode = a.Attributes[i];
        if (aNode == null) continue;
        if (aNode.Type == SystemTypes.ComVisibleAttribute) {
          a.Attributes[i] = null;
          continue;
        }
        if (aNode.Type == SystemTypes.SecurityCriticalAttribute
          || aNode.Type == SystemTypes.SecurityTransparentAttribute
          || aNode.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute
          || aNode.Type.FullName.Equals("System.Security.SecurityRules")
          ) {
          WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly",
            aNode.Type.FullName, a.Name);
          a.Attributes[i] = null;
          continue;
        }
      }
      #endregion
      #region Process all other types in the assembly
      // for all of the other types move the entire type into the target assembly,
      // possibly changing its visibility.
      for (int i = 1, n = a.Types.Count; i < n; i++) {
        TypeNode oldType = a.Types[i];
        Node newType = d.Visit(oldType); // If something went wrong then it doesn't return a TypeNode
        TypeNode tn = newType as TypeNode;
        if (tn != null) {
          if (makeNonPublic) {
            if (
              tn.DeclaringType == null // only change the visibility of assembly-level types
              && !ExemptType(oldType)
              ) {
              if ((tn.Flags & TypeFlags.VisibilityMask) == TypeFlags.Public)
                tn.Flags &= ~TypeFlags.Public;
            }
          }
          this.AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(tn, assemblyComVisibleAttribute); // recursively walks down into all nested types
          targetAssembly.Types.Add(tn);
        }
      }
      #endregion
      #region Merge in any assembly-level attributes, overriding matching attributes
      // But only if option is explicitly set and if an attribute assembly is not being used.
      if (copyattrs && attributeAssembly == null) {
        #region Regular Attributes
        AttributeList attrs = d.VisitAttributeList(a.Attributes);
        MergeAttributeLists(this.targetAssembly.Attributes, attrs, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
        #endregion
        #region Security Attributes
        // For security attributes, need to merge in the permission attributes within each
        // "bundle" keyed by the Action
        SecurityAttributeList secAttrs = d.VisitSecurityAttributeList(a.SecurityAttributes);
        for (int i = 0, n = secAttrs.Count; i < n; i++) {
          SecurityAttribute secAttr = secAttrs[i];
          System.Security.Permissions.SecurityAction action = secAttr.Action;
          // first, see if the targetAssembly already has a security attribute with the same action
          int j = 0;
          int m = targetAssembly.SecurityAttributes.Count;
          while (j < m) {
            if (targetAssembly.SecurityAttributes[j].Action == action) {
              break;
            }
            j++;
          }
          if (j == m) { // not found: this is a new action, so just add it
            targetAssembly.SecurityAttributes.Add(secAttr);
          } else { // need to walk the permission attributes inside of this security attribute
            AttributeList existingList = targetAssembly.SecurityAttributes[j].PermissionAttributes;
            AttributeList currentAssemblyList = secAttr.PermissionAttributes;
            MergeAttributeLists(existingList, currentAssemblyList, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
          }
        }
        #endregion
        #region Module Attributes
        attrs = d.VisitAttributeList(a.ModuleAttributes);
        MergeAttributeLists(this.targetAssembly.ModuleAttributes, attrs, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
        #endregion
      }
      #endregion
      #region Copy the resources over into the target assembly
      for (int i = 0, n = a.Resources.Count; i < n; i++) {
        Resource r = a.Resources[i];
        //WriteToLog("\tResource: " + r.Name);
        //if (r.Name.EndsWith(".licenses")){
        //  char[] foo = new char[r.Data.Length];
        //  for (int j = 0, m = r.Data.Length; j < m; j++) {
        //    foo[j] = (char)r.Data[j];
        //  }

        //  WriteToLog("\t\t" + foo.ToString());
        //}
        targetAssembly.Resources.Add(r);
      }
      #endregion
      return true;
    }

    private bool IsCompilerGenerated(TypeNode t) {
      return GetAttributeByName(t, "System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null;
    }
    private static AttributeNode/*?*/ GetAttributeByName(TypeNode typeNode, string name) {
      for (int i = 0, n = typeNode.Attributes == null ? 0 : typeNode.Attributes.Count; i < n; i++) {
        AttributeNode aNode = typeNode.Attributes[i];
        if (aNode == null) continue;
        if (aNode.Type.FullName.Equals(name)) {
          return aNode;
        }
      }
      return null;
    }

    private static bool GetComVisibleSettingForAssembly(AssemblyNode a) {
      bool isComVisible = true; // default value of ctor for [ComVisible]
      for (int i = 0, n = a.Attributes == null ? 0 : a.Attributes.Count; i < n; i++) {
        AttributeNode aNode = a.Attributes[i];
        if (aNode == null) continue;
        if (aNode.Type == SystemTypes.ComVisibleAttribute) {
          if (aNode.Expressions != null && 0 < aNode.Expressions.Count) {
            Literal l = aNode.Expressions[0] as Literal;
            if (l != null && l.Type == SystemTypes.Boolean) {
              isComVisible = (bool)l.Value;
            }
          }
        }
      }
      return isComVisible;
    }

    private bool FuzzyEqual(TypeNode t1, TypeNode t2)
      {
          return t1 == t2 ||
            (t1 != null &&
            t2 != null &&
            t1.Namespace != null &&
            t2.Namespace != null &&
            t1.Name != null &&
            t2.Name != null &&
            t1.Namespace.Name == t2.Namespace.Name &&
            t1.Name.Name == t2.Name.Name);
      }

    private bool FuzzyEqual (ParameterList xs, ParameterList ys) {
      if (xs.Count != ys.Count) return false;
      for (int i = 0, n = xs.Count; i < n; i++) {
        if (!FuzzyEqual(xs[i].Type, ys[i].Type)) return false;
      }
      return true;
    }
    private Member FuzzilyGetMatchingMember (TypeNode t, Member m) {
      MemberList ml = t.GetMembersNamed(m.Name);
      for (int i = 0, n = ml.Count; i < n; i++) {
        Member mem = ml[i];
        // type case statement would be *so* nice right now
        if (mem.NodeType != m.NodeType) continue;
        Method x = mem as Method; // handles regular Methods and InstanceInitializers
        if (x != null){
          if (FuzzyEqual(((Method)m).Parameters, x.Parameters)) return mem; else continue;
        }
        if (m.NodeType == NodeType.Field) {
          if (FuzzyEqual(((Field)m).Type, ((Field)mem).Type)) return mem; else continue;
        }
        if (m.NodeType == NodeType.Event) {
          if (FuzzyEqual(((Event)m).HandlerType, ((Event)mem).HandlerType)) return mem; else continue;
        }
        if (m.NodeType == NodeType.Property) {
          if (FuzzyEqual(((Property)m).Type, ((Property)mem).Type)) return mem; else continue;
        }
        TypeNode otherT = mem as TypeNode; // handles Class, Interface, etc.
        if (otherT != null) {
          if (FuzzyEqual((TypeNode)m, otherT)) return mem; else continue;
        }
        System.Diagnostics.Debug.Assert(false, "Pseudo-typecase failed to find a match");
      }
      return null;
    }
    private bool MergeInAssembly_Union(AssemblyNode a, bool targetAssemblyIsComVisible) {
      #region Process the first type, <Module>, specially
      // according to Herman Venter:
      //	The first type of each and every assembly is always the class
      // <Module>. Take its members and add them to the <Module> class
      // in the target assembly; that class was created when the target
      // assembly was created.
      if (a.Types[0].Members != null && a.Types[0].Members.Count > 0) {
        TypeNode tempTypeNode = d.TargetType;
        d.TargetType = hiddenClass;
        for (int i = 0, n = a.Types[0].Members.Count; i < n; i++) {
          Member newMember = (Member)d.Visit(a.Types[0].Members[i]);
          if (!hiddenClass.Members.Contains(newMember)) {
            hiddenClass.Members.Add(newMember);
          }
        }
        d.TargetType = tempTypeNode;
      }
      #endregion
      #region Deal with [ComVisible]
      var thisAssemblyIsComVisible = GetComVisibleSettingForAssembly(a);
      AttributeNode assemblyComVisibleAttribute = null;
      if (thisAssemblyIsComVisible != targetAssemblyIsComVisible) {
        InstanceInitializer ctor = SystemTypes.ComVisibleAttribute.GetConstructor(SystemTypes.Boolean);
        assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, ctor), new ExpressionList(new Literal(thisAssemblyIsComVisible, SystemTypes.Boolean)));
      }
      for (int i = 0, n = a.Attributes == null ? 0 : a.Attributes.Count; i < n; i++) {
        AttributeNode aNode = a.Attributes[i];
        if (aNode == null) continue;
        if (aNode.Type == SystemTypes.ComVisibleAttribute) {
          a.Attributes[i] = null;
          continue;
        }
        if (aNode.Type == SystemTypes.SecurityCriticalAttribute
          || aNode.Type == SystemTypes.SecurityTransparentAttribute
          || aNode.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute
          || aNode.Type.FullName.Equals("System.Security.SecurityRules")
          ) {
          WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly",
            aNode.Type.FullName, a.Name);
          a.Attributes[i] = null;
          continue;
        }
      }
      #endregion
      #region Process all other types in the assembly
      // for all of the other types move the entire type into the target assembly,
      // possibly changing its visibility.

      FuzzilyForwardReferencesFromSource2Target(this.targetAssembly, a);
      for (int i = 1, n = a.Types.Count; i < n; i++) {
        TypeNode currentType = a.Types[i];
        TypeNode targetType = targetAssembly.GetType(currentType.Namespace, currentType.Name);
        if (targetType != null) {
          memberList = (ArrayList)typeList[currentType.DocumentationId.ToString()];
          TypeNode savedTargetType = d.TargetType;
          d.TargetType = targetType;
          for (int j = 0, o = currentType.Members.Count; j < o; j++) {
            Member currentMember = currentType.Members[j];
            if (!memberList.Contains(currentMember.DocumentationId.ToString())) {
              Member newMember = d.Visit(currentMember) as Member;
              if (newMember != null) {
                targetType.Members.Add(newMember);
                memberList.Add(currentMember.DocumentationId.ToString());
              }
            }
          }
          d.TargetType = savedTargetType;
        } else {
          if (d.TypesToBeDuplicated[currentType.UniqueKey] == null) {
            d.FindTypesToBeDuplicated(new TypeNodeList(currentType));
          }

          Node newType = d.Visit(currentType); // If something went wrong then it doesn't return a TypeNode
          TypeNode newTypeNode = newType as TypeNode;
          if (newTypeNode != null) {
            this.AdjustAccessibilityAndPossiblyMarkWithComVisibleAttribute(newTypeNode, assemblyComVisibleAttribute); // recursively walks down into all nested types
            targetAssembly.Types.Add(newTypeNode);

            memberList = new ArrayList();
            for (int j = 0; j < currentType.Members.Count; j++) {
              memberList.Add(newTypeNode.Members[j].DocumentationId.ToString());
            }

            typeList.Add(newTypeNode.DocumentationId.ToString(), memberList);
          }
        }
      }
      #endregion
      #region Merge in any assembly-level attributes, overriding matching attributes
      // But only if option is explicitly set and if an attribute assembly is not being used.
      if (copyattrs && attributeAssembly == null) {
        #region Regular Attributes
        AttributeList attrs = d.VisitAttributeList(a.Attributes);
        MergeAttributeLists(this.targetAssembly.Attributes, attrs, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
        #endregion
        #region Security Attributes
        // For security attributes, need to merge in the permission attributes within each
        // "bundle" keyed by the Action
        SecurityAttributeList secAttrs = d.VisitSecurityAttributeList(a.SecurityAttributes);
        for (int i = 0, n = secAttrs.Count; i < n; i++) {
          SecurityAttribute secAttr = secAttrs[i];
          System.Security.Permissions.SecurityAction action = secAttr.Action;
          // first, see if the targetAssembly already has a security attribute with the same action
          int j = 0;
          int m = targetAssembly.SecurityAttributes.Count;
          while (j < m) {
            if (targetAssembly.SecurityAttributes[j].Action == action) {
              break;
            }
            j++;
          }
          if (j == m) { // not found: this is a new action, so just add it
            targetAssembly.SecurityAttributes.Add(secAttr);
          }
          else { // need to walk the permission attributes inside of this security attribute
            AttributeList existingList = targetAssembly.SecurityAttributes[j].PermissionAttributes;
            AttributeList currentAssemblyList = secAttr.PermissionAttributes;
            MergeAttributeLists(existingList, currentAssemblyList, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
          }
        }
        #endregion
        #region Module Attributes
        attrs = d.VisitAttributeList(a.ModuleAttributes);
        MergeAttributeLists(this.targetAssembly.ModuleAttributes, attrs, this.allowMultipleAssemblyLevelAttributes, this.keepFirstOfMultipleAssemblyLevelAttributes);
        #endregion
      }
      #endregion
      #region Copy the resources over into the target assembly
      for (int i = 0, n = a.Resources.Count; i < n; i++) {
        Resource r = a.Resources[i];

        if (!resourceList.Contains(r.Name)) {
          targetAssembly.Resources.Add(r);
          resourceList.Add(r.Name);
        }
      }
      #endregion
      return true;
    }

    private void FuzzilyForwardReferencesFromSource2Target (AssemblyNode targetAssembly, AssemblyNode sourceAssembly) {
      for (int i = 1, n = sourceAssembly.Types.Count; i < n; i++) {
        TypeNode currentType = sourceAssembly.Types[i];
        TypeNode  targetType = targetAssembly.GetType(currentType.Namespace, currentType.Name);
        if (targetType == null) {
          if (d.TypesToBeDuplicated[currentType.UniqueKey] == null) {
            d.FindTypesToBeDuplicated(new TypeNodeList(currentType));
          }
        } else {
          d.DuplicateFor[currentType.UniqueKey] = targetType;
          for (int j = 0, o = currentType.Members.Count; j < o; j++) {
            Member currentMember = currentType.Members[j];
            Member existingMember = FuzzilyGetMatchingMember(targetType, currentMember);
            if (existingMember != null)
              d.DuplicateFor[currentMember.UniqueKey] = existingMember;
          }
        }
      }
      //for (int i = 0, n = sourceAssembly.AssemblyReferences == null ? 0 : sourceAssembly.AssemblyReferences.Count; i < n; i++) {
      //  AssemblyNode sourceExternalReference = sourceAssembly.AssemblyReferences[i].Assembly;
      //  if (sourceExternalReference == null) continue;
      //  for (int j = 0, m = targetAssembly.AssemblyReferences == null ? 0 : targetAssembly.AssemblyReferences.Count; j < m; j++) {
      //    AssemblyNode targetExternalReference = targetAssembly.AssemblyReferences[j].Assembly;
      //    if (targetExternalReference == null) continue;
      //    if (sourceExternalReference.Name == targetExternalReference.Name) {
      //      // This depends on there *NOT* being circular references!
      //      FuzzilyForwardReferencesFromSource2Target(targetExternalReference, sourceExternalReference);
      //      break;
      //    }
      //  }
      //}
    }
    private void MergeAttributeLists(AttributeList targetList, AttributeList sourceList, bool allowMultiples, bool keepFirst)
    {
      var targetIsExe = this.targetAssembly.Kind == ModuleKindFlags.ConsoleApplication || this.targetAssembly.Kind == ModuleKindFlags.WindowsApplication;

      if (sourceList != null) {
        foreach (AttributeNode possiblyDuplicateAttr in sourceList) {
          if (possiblyDuplicateAttr != null) {

            // Certain types of attributes should not be merged onto the target assembly
            // when it is an executable.
            if (targetIsExe) {
              switch (possiblyDuplicateAttr.Type.FullName) {
                case "System.Security.AllowPartiallyTrustedCallersAttribute":
                case "System.Security.SecurityCriticalAttribute":
                case "System.Security.SecurityTransparentAttribute":
                  continue;
                default:
                  break;
              }
            } else {
              // Assume it is a library (dll). 
            }

            if (this.UnionMerge) {
              if (!AttributeExistsInTarget(possiblyDuplicateAttr, targetList)) {
                targetList.Add(possiblyDuplicateAttr);
              }
            }
            else if (allowMultiples && possiblyDuplicateAttr.AllowMultiple) {
              targetList.Add(possiblyDuplicateAttr);
            }
            else { // overwrite if found, add if not found
              int j = 0;
              int m = targetList.Count;
              while (j < m) {
                if (possiblyDuplicateAttr.Type == targetList[j].Type) {
                  if (!keepFirst) {
                    targetList[j] = possiblyDuplicateAttr; // overwrite
                  }
                  break;
                }
                j++;
              }
              if (j == m) {
                // then not found, can't overwrite, just add to end
                targetList.Add(possiblyDuplicateAttr);
              }
            }
          }
        }
      }
    }
    private bool AttributeExistsInTarget(AttributeNode possiblyDuplicateAttr, AttributeList targetList){
      bool addAttribute = false;
      int counter = 0;

      while (counter < targetList.Count) {
        if (possiblyDuplicateAttr.Type == targetList[counter].Type) {
          ArrayList exprList1 = new ArrayList();
          ArrayList exprList2 = new ArrayList();

          if (possiblyDuplicateAttr.Expressions != null) {
            foreach (Expression expression in possiblyDuplicateAttr.Expressions) {
              exprList1.Add(expression.ToString());
            }
          }
          if (targetList[counter].Expressions != null) {
            foreach (Expression expression in targetList[counter].Expressions) {
              exprList2.Add(expression.ToString());
            }
          }

          if (exprList1.Count == exprList2.Count) {
            bool completeMatch = true;
            foreach (string exprValue in exprList1) {
              if (!exprList2.Contains(exprValue)) {
                completeMatch = false;
                break;
              }
            }

            if (completeMatch) {
              foreach (string exprValue in exprList2) {
                if (!exprList1.Contains(exprValue)) {
                  completeMatch = false;
                  break;
                }
              }
            }

            if (completeMatch) {
              break;
            }
          }
        }
        counter++;
      }
      if (counter == targetList.Count) {
        addAttribute = true;
      }

      return addAttribute;
    }

    #endregion
    #region Protected Methods
    /// <summary>
    /// Provides a way for subtypes to create their own Duplicator to use for
    /// the merging. When not overridden, the duplicator is the standard one.
    /// </summary>
    /// <param name="module">Top level module for this duplicator to copy types into.</param>
    /// <returns>The duplicator to use for visiting the source modules.</returns>
    protected virtual Duplicator CreateDuplicator(Module module) {
      return new Duplicator(module, null);
    }
    /// <summary>
    /// Provides a way for subtypes to create their own Duplicator to use for
    /// the merging. When not overridden, the duplicator is the standard one.
    /// </summary>
    /// <param name="module">Top level module for this duplicator to copy types into.</param>
    /// <param name="typeNode"></param>
    /// <returns>The duplicator to use for visiting the source modules.</returns>
    protected virtual Duplicator CreateDuplicator(Module module, TypeNode typeNode) {
      return new Duplicator(module, typeNode);
    }
    /// <summary>
    /// Returns true iff there is a violation of the command line arguments.
    /// Currently, it doesn't check for much.
    /// </summary>
    /// <param name="args">Exactly the same array as passed to Main.</param>
    /// <returns>true iff command line args are incorrect (in some simple cases)</returns>
    protected virtual bool CheckUsage(string[] args) {
      if (args.Length < 1) {
        return true;
      }
      if (args.Length == 1 &&
        (String.Compare(args[0], "-?", true) == 0 ||
        String.Compare(args[0], "/?", true) == 0 ||
        String.Compare(args[0], "-h", true) == 0)
        ) {
        return true;
      }
      return false;
    }
    /// <summary>
    /// Checks certain arguments and their combinations to detect proper usage.
    /// Writes to Console for each violation.
    /// </summary>
    /// <returns>True iff no error is detected.</returns>
    protected virtual bool ValidateOptions() {
      bool errorInOptions = false;
      if (!(this.assemblyNames.Count > 0)) {
        Console.WriteLine("Must specify at least one input file!");
        errorInOptions = true;
      }
      if (this.outputFileName == null) {
        Console.WriteLine("Must specify an output file!");
        errorInOptions = true;
      }
      if (this.keyfileSpecified) {
        if (this.keyfile == null) {
          errorInOptions = true;
          Console.WriteLine("/keyfile option given, but no file name.");
        }
      }
      if (this.keyContainerSpecified) {
        if (this.keyContainer == null) {
          errorInOptions = true;
          Console.WriteLine("/keycontainer option given, but no container name.");
        }
      }
      if (this.delaySign && !keyfileSpecified && !keyContainerSpecified) {
        errorInOptions = true;
        Console.WriteLine("/delaysign option given, but not the /keyfile or /keycontainer options.");
      }
#if CROSSPLATFORM
      if (this.targetPlatformSpecified) {
        if (this.clrVersion == null) {
          errorInOptions = true;
          Console.WriteLine("/targetplatform option given, but couldn't set it");
        }
      }
#endif
      return !errorInOptions;
    }

    /// <summary>
    /// Sets internal state to reflect arguments specified by user on command line.
    /// </summary>
    /// <param name="args"></param>
    protected virtual bool ProcessCommandLineOptions(string[] args) {
      bool ok = true;
      this.assemblyNames = new ArrayList(args.Length); // can't be more arguments than that
      for (int i = 0, n = args.Length; i < n && ok; i++)  // stop processing if not okay
            {
        string arg = args[i];
        if (!(arg[0] == '-' || arg[0] == '/')) {
          this.assemblyNames.Add(arg); // then take it as an input assembly
        }
        else {
          string option = arg.Substring(1);
          int setTo = option.IndexOf("=");
          if (setTo < 0)
            setTo = option.IndexOf(":");
          string key = null;
          string val = null;
          if (setTo < 0) {
            key = option.Substring(0, option.Length);
            val = "";
          }
          else {
            key = option.Substring(0, setTo);
            val = option.Substring(setTo + 1);
          }

          if (String.Compare(key, "lib", true) == 0) {
            if (val != "")
              this.searchDirs.Add(val);
          }
          else if (String.Compare(key, "internalize", true) == 0) {
            this.internalize = true;
            if (val != "")
              this.excludeFile = val;
          }
          else if (String.Compare(key, "log", true) == 0) {
            this.log = true;
            if (val != "")
              this.logFile = val;
          }
          else if (String.Compare(key, "t", true) == 0
            || String.Compare(key, "target", true) == 0
            ) {
            if (val != "") {
              switch (val) {
                case "library": this.targetKind = Kind.Dll; break;
                case "exe": this.targetKind = Kind.Exe; break;
                case "winexe": this.targetKind = Kind.WinExe; break;
              }
            }
            else {
              ok = false;
              Console.WriteLine("/target given without an accompanying kind.");
            }
          }
          else if (String.Compare(key, "ndebug", true) == 0) {
            this.debugInfo = false;
          }
          else if (String.Compare(key, "closed", true) == 0) {
            this.closed = true;
          }
          else if (String.Compare(key, "short", true) == 0) {
            this.shortB = true;
          }
          else if (String.Compare(key, "copyattrs", true) == 0) {
            this.copyattrs = true;
          }
          else if (String.Compare(key, "allowMultiple", true) == 0) {
            if (!this.copyattrs) {
              Console.WriteLine("/allowMultiple specified without specifying /copyattrs");
              ok = false;
            }
            else {
              this.allowMultipleAssemblyLevelAttributes = true;
            }
          } else if (String.Compare(key, "keepFirst", true) == 0) {
            if (!this.copyattrs) {
              Console.WriteLine("/keepFirst specified without specifying /copyattrs");
              ok = false;
            } else {
              this.keepFirstOfMultipleAssemblyLevelAttributes = true;
            }
          } else if (String.Compare(key, "xmldocs", true) == 0) {
            this.copyXmlDocumentation = true;
          }
          else if (String.Compare(key, "out", true) == 0) {
            if (val != "")
              this.outputFileName = val;
          }
          else if (String.Compare(key, "attr", true) == 0) {
            if (val != "") {
              this.attributeFileName = val;
            }
            else {
              Console.WriteLine("/attr given without an accompanying assembly name.");
              ok = false;
            }
          }
          else if (String.Compare(key, "reference", true) == 0 || String.Compare(key, "r", true) == 0) {
            if (val != "") {
              this.assemblyNames.Add(val);
            }
            else {
              Console.WriteLine("/reference given without an accompanying assembly name.");
              ok = false;
            }
          }
          else if (String.Compare(key, "targetplatform", true) == 0) {
#if CROSSPLATFORM
            if (this.targetPlatformSpecified) {
              Console.WriteLine("Target platform already specified earlier on command line.");
              ok = false;
            }
            else {
              this.targetPlatformSpecified = true;
              if (val != "") {
                string s = val;
                int commaPos = s.IndexOf(",");
                if (commaPos > 0) {
                  this.clrVersion = s.Substring(0, commaPos);
                  this.clrDir = s.Substring(commaPos + 1);
                  if (!Directory.Exists(this.clrDir)) {
                    Console.WriteLine("Error: cannot find target platform directory '{0}'.", this.clrDir);
                    this.clrVersion = null;
                    this.clrDir = null;
                    ok = false;
                  }
                  //else if (!File.Exists(Path.Combine(this.clrDir, "mscorlib.dll"))) {
                  //  Console.WriteLine("Error: mscorlib.dll not found in specified target directory '{0}'.", this.clrDir);
                  //  this.clrVersion = null;
                  //  this.clrDir = null;
                  //  ok = false;
                  //}
                }
                else {
                  this.clrVersion = val;
                }
              }
              else {
                Console.WriteLine("Target platform needs at least a version specified.");
                ok = false;
              }
            }
#else
            Console.WriteLine("/targetplatform can be specified only in ILMerge v2");
            ok = false;
#endif
          }
          else if (String.Compare(key, "v1", true) == 0) {
#if CROSSPLATFORM
            if (this.targetPlatformSpecified) {
              Console.WriteLine("Target platform already specified earlier on command line.");
              ok = false;
            }
            else {
              this.targetPlatformSpecified = true;
              this.clrVersion = "v1";
            }
#else
            Console.WriteLine("/v1 can be specified only in ILMerge v2");
            ok = false;
#endif
          }
          else if (String.Compare(key, "v1.1", true) == 0) {
#if CROSSPLATFORM
            if (this.targetPlatformSpecified) {
              Console.WriteLine("Target platform already specified earlier on command line.");
              ok = false;
            }
            else {
              this.targetPlatformSpecified = true;
              this.clrVersion = "v1.1";
            }
#else
            Console.WriteLine("/v1.1 can be specified only in ILMerge v2");
            ok = false;
#endif
          }
          else if (String.Compare(key, "v2", true) == 0) {
#if CROSSPLATFORM
            if (this.targetPlatformSpecified) {
              Console.WriteLine("Target platform already specified earlier on command line.");
              ok = false;
            }
            else {
              this.targetPlatformSpecified = true;
              this.clrVersion = "v2";
            }
#else
            Console.WriteLine("/v2 can be specified only in ILMerge v2");
            ok = false;
#endif
          } else if (String.Compare(key, "v4", true) == 0) {
            if (this.targetPlatformSpecified) {
              Console.WriteLine("Target platform already specified earlier on command line.");
              ok = false;
            } else {
              this.targetPlatformSpecified = true;
              this.clrVersion = "v4";
            }

          } else if (String.Compare(key, "useFullPublicKeyForReferences", true) == 0) {
            this.usePublicKeyTokensForAssemblyReferences = false;
          } else if (String.Compare(key, "zeroPeKind", true) == 0) {
            this.allowZeroPeKind = true;
          } else if (String.Compare(key, "wildcards", true) == 0) {
            this.allowWildCards = true;
          } else if (String.Compare(key, "keyfile", true) == 0) {
            if (val != "")
              this.keyfile = val;
            this.keyfileSpecified = true;
          } else if (String.Compare(key, "keycontainer", true) == 0) {
            if (val != "")
              this.keyContainer = val;
            this.keyContainerSpecified = true;
          } else if (String.Compare(key, "ver", true) == 0) {
            if (!String.IsNullOrEmpty(val)) {
              System.Version v = null;
              try {
                v = new System.Version(val);
                // still need to make sure that all components are at most UInt16.MaxValue - 1,
                // per the spec.
                if (!(v.Major < UInt16.MaxValue)) {
                  Console.WriteLine("Invalid major version '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", v.Major);
                  ok = false;
                } else if (!(v.Minor < UInt16.MaxValue)) {
                  Console.WriteLine("Invalid minor version '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", v.Minor);
                  ok = false;
                } else if (!(v.Build < UInt16.MaxValue)) {
                  Console.WriteLine("Invalid build '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", v.Build);
                  ok = false;
                } else if (!(v.Revision < UInt16.MaxValue)) {
                  Console.WriteLine("Invalid revision '{0}' specified. It must be less than UInt16.MaxValue (0xffff).", v.Revision);
                  ok = false;
                } else {
                  this.version = v;
                }
              } catch (System.ArgumentOutOfRangeException) {
                Console.WriteLine("Invalid version '{0}' specified. A major, minor, build, or revision component is less than zero.", val);
                ok = false;
              } catch (System.ArgumentException) {
                Console.WriteLine("Invalid version '{0}' specified. It has fewer than two components or more than four components.", val);
                ok = false;
              } catch (System.FormatException) {
                Console.WriteLine("Invalid version '{0}' specified. At least one component of version does not parse to an integer.", val);
                ok = false;
              } catch (System.OverflowException) {
                Console.WriteLine("Invalid version '{0}' specified. At least one component of version represents a number greater than System.Int32.MaxValue.", val);
                ok = false;
              }
            } else {
              Console.WriteLine("/ver option specified, but no version number.");
              ok = false;
            }
          } else if (String.Compare(key, "delaysign", true) == 0) {
            this.delaySign = true;
          } else if (String.Compare(key, "allowDup", true) == 0) {
            if (val != "")
              this.typesToAllowDuplicatesOf[val] = true;
            else
              allowAllDuplicates = true;
          } else if (String.Compare(key, "union", true) == 0) {
            this.unionMerge = true;
          } else if (String.Compare(key, "align", true) == 0) {
            if (val != null && val != "") {
              try {
                int x = Int32.Parse(val);
                this.FileAlignment = x;
              } catch (FormatException) { ok = false; } catch (OverflowException) { ok = false; }
            }
          } else {
            Console.WriteLine("Ignoring unknown option: '{0}'.", option);
            ok = false;
          }
        }
      }
      if (ok) {
        this.directories = new string[this.searchDirs.Count];
        this.searchDirs.CopyTo(this.directories);
      }
      return ok;
    }
    /// <summary>
    /// Called throughout to write log messages.
    /// Does nothing if "log" is not true.
    /// If "logFile" is not set, then it writes to the Console, otherwise
    /// it writes to the stream opened on "logFile".
    /// </summary>
    /// <param name="s">String to write to log. May have placeholder args in it.</param>
    /// <param name="args">Zero or more arguments to fill the placeholder args in s.</param>
    protected void WriteToLog(string s, params object[] args) {
      if (log) {
        if (logFile != null) {
          StreamWriter writer = new System.IO.StreamWriter(logFile, true);
          writer.WriteLine(s, args);
          writer.Close();
        }
        else {
          Console.WriteLine(s, args);
        }
      }
    }
    /// <summary>
    /// Using the duplicator provided, calls FindTypesToBeDuplicated on each type
    /// in each assembly.
    /// </summary>
    /// <param name="d">The duplicator to use for the scan.</param>
    /// <param name="assems">A list of assembies that are visited in
    /// turn.
    /// The Types property of each one is passed to d's FindTypesToBeDuplicated
    /// method.</param>
    protected virtual void ScanAssemblies(Duplicator d, AssemblyNodeList assems) {
      for (int i = 0, n = assems.Count; i < n; i++) {
        AssemblyNode a = assems[i];
        d.FindTypesToBeDuplicated(a.Types);
      }
    }
    #endregion
    #region Public Interface
    #region Enums
    /// <summary>
    /// This enumeration contains values that are used to specify the kind
    /// of assembly that the target assembly should be. The options are
    /// console application, class library, windows application, or a
    /// "wild card" that matches whatever the primary assembly is.
    /// </summary>
    public enum Kind {
      /// <summary>
      /// Represents a class library.
      /// </summary>
      Dll,
      /// <summary>
      /// Represents a console application.
      /// </summary>
      Exe,
      /// <summary>
      /// Represents a Windows application.
      /// </summary>
      WinExe,
      /// <summary>
      /// Represents a "don't care" value.
      /// </summary>
      SameAsPrimaryAssembly
    }
    #endregion Enums
    #region Properties and Methods
    /// <summary>
    /// Adds a type name to the set of public types for which duplicates will be allowed.
    /// (Private types are always allowed to have duplicates.) All duplicates are renamed
    /// so for a public type, that means clients must be aware of the new name. This functionality
    /// is mainly to allow obfuscated assemblies to be merged since each obfuscated assembly defines
    /// the same attribute. Otherwise, duplicate type names cause an error.
    /// </summary>
    /// <param name="typeName">The string name of the type that duplicates should be ignored for.
    /// When it is null, that means allow all duplicate public types.</param>
    public void AllowDuplicateType(string typeName) {
      if (typeName == null)
        allowAllDuplicates = true;
      else
        typesToAllowDuplicatesOf[typeName] = true;
    }
    /// <summary>
    /// Controls whether multiple instances of assembly-level attributes are allowed in the target
    /// assembly. (Multiple instances are allowed only for attributes that are defined to allow
    /// multiple instances. All others either overwrite any previous instance found in the target assembly
    /// or else are ignored, depending on the setting of KeepFirstOfMultipleAssemblyLevelAttributes.
    /// Unless CopyAttributes is true, it is ignored. 
    /// (default: false)
    /// </summary>
    public bool AllowMultipleAssemblyLevelAttributes {
      get { return this.allowMultipleAssemblyLevelAttributes; }
      set { this.allowMultipleAssemblyLevelAttributes = value; }
    }

    /// <summary>
    /// Controls whether first occurrence wins or last occurrence wins when multiple instances
    /// of assembly-level attributes are found. Ignored unless CopyAttributes is true.
    /// (default: false)
    /// </summary>
    public bool KeepFirstOfMultipleAssemblyLevelAttributes {
      get { return this.keepFirstOfMultipleAssemblyLevelAttributes; }
      set { this.keepFirstOfMultipleAssemblyLevelAttributes = value; }
    }
    /// <summary>
    /// Controls whether the absence of the ILOnly value in an assembly's PeKind flag is tolerated.
    /// (default: false)
    /// </summary>
    public bool AllowZeroPeKind {
      get { return allowZeroPeKind; }
      set { allowZeroPeKind = value; }
    }
    /// <summary>
    /// Controls whether wild cards are allowed in input file names.
    /// (default: false)
    /// </summary>
    public bool AllowWildCards {
      get { return allowWildCards; }
      set { allowWildCards = value; }
    }
    /// <summary>
    /// If this is set before calling Merge, then it specifies the path and filename to an
    /// attribute assembly, an assembly that will be used to get all of the assembly-level
    /// attributes such as Culture, Version, etc. It will also be used to get the Win32 Resources
    /// from. It is mutually exclusive with the CopyAttributes property. When it is not specified,
    /// then the Win32 Resources from the primary assembly are copied over into the target assembly.
    /// If not a full path, e.g., "c:\tmp\foo.log", then the current directory is used.
    /// </summary>
    public string AttributeFile {
      get { return attributeFileName; }
      set { attributeFileName = value; }
    }
    /// <summary>
    /// When this is set before calling Merge, then the "transitive closure"
    /// of the input assemblies is computed and added to the list of input
    /// assemblies. An assembly is considered part of the transitive closure
    /// if it is referenced, either directly or indirectly, from one of the
    /// originally specified input assemblies and it has an external reference
    /// to one of the input assemblies, or one of the assemblies that has such
    /// a reference. Complicated, but that is life...
    /// </summary>
    public bool Closed {
      get { return closed; }
      set { closed = value; }
    }
    /// <summary>
    /// When this is set before calling Merge, then the assembly level attributes
    /// of each input assembly are copied over into the target assembly.
    /// Any duplicate attribute overwrites a previously copied attribute.
    /// This option is mutually exclusive with specifying an attribute assembly (see
    /// the property AttributeFile). When an attribute assembly is specified,
    /// then no assembly-level attributes are copied over from the input
    /// assemblies.
    /// Default: false
    /// </summary>
    public bool CopyAttributes {
      get { return copyattrs; }
      set { copyattrs = value; }
    }
    /// <summary>
    /// Controls whether debug info is preserved, if present. (default: true)
    /// </summary>
    public bool DebugInfo {
      get { return debugInfo; }
      set { debugInfo = value; }
    }
    /// <summary>
    /// Controls whether the specified key file or container is to be used as containing
    /// only a public key and so delay-signing the target assembly.
    /// </summary>
    public bool DelaySign {
      get { return delaySign; }
      set { delaySign = value; }
    }
    /// <summary>
    /// When the property Internalize is true, then this property is consulted.
    /// Its value should be the file name of a file containing those types
    /// which are to have their visibility unchanged.
    /// Needs to be a full path, e.g., "c:\tmp\foo.txt".
    /// The file should contain one type name per line. Matching will be done using
    /// regular expressions.  Any type not matching will be public in the resulting assembly.
    /// Setting it sets Internalize to true.
    /// </summary>
    public string ExcludeFile {
      get { return excludeFile; }
      set {
        if (value == null) {
          excludeFile = "";
        }
        else {
          excludeFile = value;
          internalize = true;
        }
      }
    }
    #region External Visitor (Internal builds only)
#if INTERNAL
    /// <summary>
    /// <para>After the merging is finished, if this property has been
    /// set, then the object it refers to will be invoked to visit
    /// the target assembly before it is written out.
    /// </para>
    /// </summary>
    public System.Compiler.StandardVisitor ExternalVisitor {
      set { externalVisitor = value; }
    }
#endif
    #endregion

    /// <summary>
    /// Sets the file alignment used in the target assembly. The setter
    /// will set the value to the largest power of two that is no greater
    /// than the value supplied and that is at least 512.
    /// </summary>
    public int FileAlignment {
      get { return fileAlignment; }
      set {
        if (value <= 512) {
          fileAlignment = 512;
        } else {
          // 512 < value
          int i = 512;
          while (i < value) {
            i = i << 1;
          }
          if (value < i) i = i >> 1; // want the previous iteration's value
          fileAlignment = i;
        }
      }
    }

    /// <summary>
    /// Controls whether all types in input assemblies, other than the primary assembly,
    /// are to be made non-public. (default: false)
    /// If the property Internalize is true then types can be listed in the ExcludeFile;
    /// those will remain with their visibility unchanged.
    /// </summary>
    public bool Internalize {
      get { return internalize; }
      set { internalize = value; }
    }
    /// <summary>
    /// Path to the key file that will be used to strongly-name the target assembly.
    /// Needs to be a full path, e.g., "c:\tmp\foo.snk".
    /// It should be the same key that was used to sign the primary assembly,
    /// if the primary assembly has a strong name.
    /// </summary>
    public string KeyFile {
      get { return keyfile; }
      set { keyfile = value; }
    }
    /// <summary>
    /// Name of the key container that will be used to strongly-name the target assembly.
    /// Needs to be the name of a machine-level RSA CSP container which has had an SNK blob imported.
    /// Visual Studio handles this when it remembers PFX passwords and passes the container name to MSBuild.
    /// It should be the same key that was used to sign the primary assembly,
    /// if the primary assembly has a strong name.
    /// </summary>
    public string KeyContainer {
      get { return keyContainer; }
      set { keyContainer = value; }
    }
    /// <summary>
    /// Controls whether output log messages are produced during weaving. (default: false)
    /// (If the property Log is true and the LogFile is null, then Console.Out is written to.)
    /// </summary>
    public bool Log {
      get { return log; }
      set { log = value; }
    }
    /// <summary>
    /// Place to send output log messages. Needs to be a full path, e.g., "c:\tmp\foo.log".
    /// (If the property Log is true and the LogFile is null, then Console.Out is written to.)
    /// </summary>
    public string LogFile {
      get { return logFile; }
      set { if (value != null && value != "") { logFile = value; log = true; } }
    }
    /// <summary>
    /// Path (including file name) that the target assembly will be written to.
    /// If not a full path, e.g., "c:\tmp\foo.log", then the current directory is used.
    /// </summary>
    public string OutputFile {
      get { return outputFileName; }
      set { outputFileName = value; }
    }
    /// <summary>
    /// Controls whether external assembly references are written with the full key (false) or public key token (true).
    /// (default: true)
    /// </summary>
    public bool PublicKeyTokens {
      get { return usePublicKeyTokensForAssemblyReferences; }
      set { usePublicKeyTokensForAssemblyReferences = value; }
    }
    /// <summary>
    /// Controls whether short branches in the IL should be preserved or not.
    /// (default: false)
    /// </summary>
    public bool PreserveShortBranches {
      get { return shortB; }
      set { shortB = value; }
    }
    /// <summary>
    /// <para>Pass this method a list of the assemblies that are to be merged
    /// (organized as an array of strings, one entry per directory).
    /// </para>
    /// </summary>
    public void SetInputAssemblies(string[] assems) {
      if (assems != null && assems.Length > 0) {
        assemblyNames = new ArrayList(assems.Length);
        foreach (string s in assems) {
          assemblyNames.Add(s);
        }
      }
    }
    /// <summary>
    /// <para>Pass this method a set of directories (organized as an array of strings,
    /// one entry per directory) that will be used to search for input assemblies.
    /// It is also used as the set of directories to use for assembly reference
    /// resolving. The latter usage is not needed if all referenced directories are 
    /// either in the GAC or in the directory in which the input is located
    /// (or, more generally, in the directory in which the referencing assembly is located).
    /// </para>
    /// </summary>
    public void SetSearchDirectories(string[] dirs) {
      if (dirs != null && dirs.Length > 0) {
        directories = new string[dirs.Length];
        dirs.CopyTo(directories, 0);
      }
    }
    /// <summary>
    /// <para> Needed only if the target platform differs from that in which
    /// ILMerge is running. When a directory is specified, a check *is* made
    /// that the directory actually exists and that mscorlib.dll is indeed in that
    /// directory. Exception thrown when something goes wrong with a hopefully
    /// informative message describing the problem. When a directory is not specified,
    /// an exception is thrown if the framework directory is not found.
    /// </para>
    /// </summary>
    /// <param name="platform">Must be one of: "V1", "V1.1", "V2", "postV1.1".
    /// (The "V" is case insensitive and is also optional.)
    /// (The "." can also be an underscore character: "_".)</param>
    /// <param name="dir">Directory in which to find mscorlib.dll.
    /// When null (or the empty string), the directory is probed for and
    /// doesn't need to be suplied. But if it isn't found, then an exception
    /// will be thrown.</param>
    public void SetTargetPlatform(string platform, string dir) {
      switch (platform) {
        case "V1":
        case "v1":
        case "1":
        case "1.0":
        case "1_0":
        case "V1.1":
        case "v1.1":
        case "V1_1":
        case "v1_1":
        case "1.1":
        case "1_1":
        case "postV1":
        case "postv1":
        case "postV1.1":
        case "postv1.1":
        case "postV1_1":
        case "postv1_1":
        case "post1.1":
        case "post1_1":
        case "v2":
        case "V2":
        case "2":
        case "2.0":
        case "2_0":
        case "v4":
        case "V4":
        case "4":
        case "4.0":
        case "4_0":
        case "v4.5":
        case "V4.5":
        case "4.5":
        case "4_5":
            break;
        default:
          throw new System.ArgumentException("Platform '" + platform + "' not recognized.");
      }
      if (dir != null && dir != "") {
        if (!Directory.Exists(dir))
          throw new System.ArgumentException("Directory '" + dir + "' doesn't exist.");
        //if (!File.Exists(Path.Combine(dir, "mscorlib.dll")))
        //  throw new System.ArgumentException("Directory '" + dir + "' doesn't contain mscorlib.dll.");
      }
      Version version;
      switch (platform) {
        case "V1":
        case "v1":
        case "1":
        case "1.0":
        case "1_0":
          version = new Version(1, 0, 3300);
          break;
        case "V1.1":
        case "v1.1":
        case "1.1":
        case "V1_1":
        case "v1_1":
        case "1_1":
          version = new Version(1, 0, 5000);
          break;
        case "postV1.1":
        case "postv1.1":
        case "postV1_1":
        case "postv1_1":
        case "post1.1":
        case "post1_1":
          if (dir == null || dir == "") {
            throw new System.ArgumentException("Directory must be specified for setting target platform to postv1.1.");
          }
          version = new Version(1, 1, 9999);
          break;
        case "v2":
        case "V2":
        case "2":
        case "2.0":
        case "2_0":
          version = new Version(2,0);
          break;
        case "v4":
        case "V4":
        case "4":
        case "4.0":
        case "4_0":
          version = new Version(4, 0);
          break;
        case "v4.5":
        case "V4.5":
        case "4.5":
        case "4_5":
          version = new Version(4, 5);
          break;
        default:
          throw new System.ArgumentException("Platform '" + platform + "' not recognized.");
      }
      if (dir == null || dir == "")
        TargetPlatform.SetTo(version);
      else
        TargetPlatform.SetTo(version, dir);
      if (TargetPlatform.PlatformAssembliesLocation == null) {
        if (dir == null || dir == "") {
          throw new System.ArgumentException("Could not find the platform assemblies for '" + platform + "'.");
        }
        else {
          throw new System.ArgumentException("Directory '" + dir + "' doesn't contain mscorlib.dll.");
        }
      }
      else {
        dir = TargetPlatform.PlatformAssembliesLocation;
      }
      WriteToLog("Set platform to '{0}', using directory '{1}' for mscorlib.dll",
        platform, dir);
      return;
    }
    /// <summary>
    /// <para>After the merging is finished, this property reflects whether
    /// the input had a strong name, but since no key file was specified,
    /// the output has had its public key removed and does *not* have a
    /// strong name.
    /// </para>
    /// </summary>
    public bool StrongNameLost {
      get { return strongNameLost; }
    }
    /// <summary>
    /// Controls what kind of binary the target assembly is. (default: same as primary assembly)
    /// </summary>
    public Kind TargetKind {
      get { return targetKind; }
      set { targetKind = value; }
    }
    /// <summary>
    /// Controls whether the source assemblies are merged into a union set or not.
    /// (default: false)
    /// </summary>
    public bool UnionMerge {
      get { return unionMerge; }
      set { unionMerge = value; }
    }
    /// <summary>
    /// <para>When this property has a non-null value, then it will
    /// cause the target assembly to have its Version changed to whatever
    /// this property has been set to. (default: null)
    /// </para>
    /// </summary>
    public System.Version Version {
      get { return version; }
      set { version = value; }
    }
    /// <summary>
    /// Controls whether XML documentation files are merged.
    /// (default: false)
    /// </summary>
    public bool XmlDocumentation {
      get { return this.copyXmlDocumentation; }
      set { this.copyXmlDocumentation = value; }
    }
    #endregion
    #region Merge(): the main processing function
    /// <summary>
    /// Given a set of IL assemblies, merge them into a single IL assembly.
    /// All inter-assembly references in the set are resolved to be
    /// intra-assembly references in the merged assembly.
    /// If calling this directly, need to set the InputAssemblies
    /// and the OutputFile (at least) before calling this method.
    /// </summary>
    public void Merge() {
      #region Validate Options
      if (outputFileName == null || assemblyNames == null || !(assemblyNames.Count > 0))
        throw new InvalidOperationException("ILMerge.Merge: Must set the InputAssemblies and OutputFile properties before calling this method.");
      foreach (object o in assemblyNames) {
        if (o == null) {
          throw new InvalidOperationException("ILMerge.Merge: Cannot have any null elements in the InputAssemblies.");
        }
      }
      if (targetKind == Kind.SameAsPrimaryAssembly) {
        // Assert outputFileName != null;
        // Assert assemblyNames != null;
        // Assert assemblyNames.Count > 0;
        string outExt = Path.GetExtension(outputFileName);
        string inExt = Path.GetExtension((string)assemblyNames[0]);
        if (String.Compare(inExt, outExt, true) != 0) {
          string msg = "/target not specified, but output file, '"
            + outputFileName +
            "', has a different extension than the primary assembly, '"
            + ((string)assemblyNames[0])
            + "'.";
          WriteToLog(msg);
          throw new InvalidOperationException("ILMerge.Merge: " + msg);
        }
      }
      else { // /target was set to something
        string outExt = Path.GetExtension(outputFileName);
        if (targetKind == Kind.Dll) {
          if (String.Compare(outExt, ".dll", true) != 0) {
            string msg = "/target specified as library, but output file, '"
              + outputFileName +
              "', does not have a .dll extension.";
            WriteToLog(msg);
            throw new InvalidOperationException("ILMerge.Merge: " + msg);
          }
        }
        else { // targetKind == Kind.Exe || targetKind == Kind.WinExe 
          if (String.Compare(outExt, ".exe", true) != 0) {
            string msg = "/target specified as an executable, but output file, '"
              + outputFileName +
              "', does not have a .exe extension.";
            WriteToLog(msg);
            throw new InvalidOperationException("ILMerge.Merge: " + msg);
          }
        }
      }
      if (directories != null) {
        foreach (string dir in directories) {
          if (!Directory.Exists(dir)) {
            throw new InvalidOperationException("Specified search directory '" + dir + "' not found.");
          }
        }
      }
      if (keyfile != null && !File.Exists(keyfile)) {
        throw new InvalidOperationException("Specified key file '" + keyfile + "' not found.");
      }
      if (excludeFile != "" && !File.Exists(excludeFile)) {
        throw new InvalidOperationException("Specified exclude file '" + excludeFile + "' not found.");
      }
      if (attributeFileName != null && copyattrs) {
        throw new InvalidOperationException("Cannot specify both an attribute file and to copy attributes from the input assemblies.");
      }
      if (unionMerge && allowAllDuplicates) {
        throw new InvalidOperationException("Cannot specify both /union and /allowDup.");
      }
      #endregion
      #region Setup
      #region Set Target Platform and see if reset is needed
#if CROSSPLATFORM
      if (this.targetPlatformSpecified) {
        this.SetTargetPlatform(this.clrVersion, this.clrDir);
        if (this.DebugInfo && TargetPlatform.TargetVersion.Major < 2) {
          this.WriteToLog("Target platform is not v2: turning off debug info");
          this.DebugInfo = false;
        }
      } else { // NB: Default is v2. Someday that will change...
        this.SetTargetPlatform("v4", null);
      }
      // If this is the first time Merge() is called, then this isn't necessary. Would
      // it be good to avoid doing it then?
      TargetPlatform.ResetCci(TargetPlatform.PlatformAssembliesLocation, TargetPlatform.TargetVersion, true, this.DebugInfo);

#endif
      #endregion
      #region Echo Framework Version just for debugging help
      try {
        WriteToLog("Running on Microsoft (R) .NET Framework "
          + Path.GetFileName(Path.GetDirectoryName(typeof(object).Module.Assembly.Location)));
        Version ver = typeof(object).Module.Assembly.GetName().Version;
        WriteToLog("mscorlib.dll version = " + ver);
      }
      catch (Exception e) {
        WriteToLog("Could not determine runtime version.");
        WriteToLog("Exception occurred: ");
        WriteToLog(e.ToString());
      }
      #endregion
      #region Initialize some variables needed for the process
      string baseName = Path.GetFileNameWithoutExtension(outputFileName);
      IDictionary h = TargetPlatform.StaticAssemblyCache;
      #endregion
      #region Create an assembly resolver and make it behave as this code does for logging, etc.
      AssemblyResolving.AssemblyResolver ar = new AssemblyResolving.AssemblyResolver(h);
      if (directories != null && directories.Length > 0)
        ar.SearchDirectories = directories;
      ar.DebugInfo = debugInfo;
      ar.Log = log;
      ar.LogFile = logFile;
      ar.PreserveShortBranches = shortB;
      #endregion
      #region If an exclusion file has been specified, read in each line as a regular expression
      if (excludeFile != null && excludeFile != "") {
        int i = 0;
        exemptionList = new ArrayList();
        try {
          // Create an instance of StreamReader to read from a file.
          // The using statement also closes the StreamReader.
          using (StreamReader sr = new StreamReader(excludeFile)) {
            String line;
            // Read and display lines from the file until the end of 
            // the file is reached.
            while ((line = sr.ReadLine()) != null) {
              exemptionList.Add(new Regex(line));
              i++;
            }
            WriteToLog("Read {0} lines from the exclusion file '{1}'.",
              i, excludeFile);
          }
        }
        catch (Exception e) {
          WriteToLog("Something went wrong reading the exclusion file '{0}'; read in {1} lines, continuing processing.",
            excludeFile, i);
          WriteToLog(e.Message);
        }
      }
      #endregion
      #endregion
      #region Echo input list of assemblies
      WriteToLog("The list of input assemblies is:");
      foreach (string s in assemblyNames) {
        WriteToLog("\t" + s);
      }
      #endregion
      #region Check to see if there are any duplicate assembly names
      {
        if (!this.UnionMerge) {
          Hashtable avoidNSquaredComputation = new Hashtable(assemblyNames.Count);
          foreach (string s in assemblyNames) {
            if (avoidNSquaredComputation.ContainsKey(s)) {
              string msg = "Duplicate assembly name '" + s + "'.";
              WriteToLog(msg);
              throw new InvalidOperationException("ILMerge.Merge: " + msg);
            }
            else {
              avoidNSquaredComputation.Add(s, true);
            }
          }
        }
      }
      #endregion
      #region Load all assemblies
      Hashtable assemblyCache = new Hashtable();
      System.Compiler.AssemblyNodeList assems = new AssemblyNodeList();
      for (int i = 0, n = assemblyNames.Count; i < n; i++) {
        string fileSpec = (string)assemblyNames[i];
        var currentNumberOfFoundFiles = assems.Count;
        // search in the specified directories, stop as soon as found
        for (int j = 0, m = (directories == null ? 1 : directories.Length + 1); j < m; j++) {
          string curDir = null;
          if (j == 0)
            curDir = Directory.GetCurrentDirectory();
          else
            curDir = directories[j - 1]; // iteration 0 is for current directory (REVIEW? Better way to do this?)

          // There might be wild cards in the assembly name
          // But it also might already be a full path name
          string[] files = null;
          if (allowWildCards) {
            if (Path.IsPathRooted(fileSpec))
              files = Directory.GetFiles(Path.GetDirectoryName(fileSpec), Path.GetFileName(fileSpec));
            else
              files = Directory.GetFiles(curDir, fileSpec);
            WriteToLog("The number of files matching the pattern {0} is {1}.", fileSpec, files.Length);
            foreach (string s in files) {
              WriteToLog(s);
            }
            if (files == null || !(files.Length > 0))
              continue;
          }
          else {
            string fullPath = Path.Combine(curDir, fileSpec);
            if (File.Exists(fullPath)) {
              files = new string[1];
              files[0] = fullPath;
            }
            else {
              continue;
            }
          }

          #region Try to load each assembly
          // At this point, all assemblies whose full paths are in the array "files"
          // exist in the file system. Try to load each assembly.
          foreach (string loc in files) {
            WriteToLog("Trying to read assembly from the file '{0}'.", loc);
            bool tempDebugInfo = debugInfo;
            if (tempDebugInfo) {
              // Don't pass the debugInfo flag to GetAssembly unless the PDB file exists.
              string pdbFullName = Path.ChangeExtension(loc, ".pdb");
              if (!File.Exists(pdbFullName)) {
                WriteToLog("Can not find PDB file. Debug info will not be available for assembly '{0}'.",
                  (string)assemblyNames[i]);
                tempDebugInfo = false;
              }
            }
            AssemblyNode a;

            a = AssemblyNode.GetAssembly(
              loc, // path to assembly
              h,  // global cache to use for assemblies
              true, // doNotLockFile
              tempDebugInfo, // getDebugInfo
              true, // useGlobalCache
              shortB  // preserveShortBranches
              );

            if (a == null) {
              string msg = "Could not load assembly from the location '" + loc + "'. Skipping and processing rest of arguments.";
              WriteToLog(msg);
              throw new InvalidOperationException("ILMerge.Merge: " + msg);
            }
            else {
              WriteToLog("\tSuccessfully read in assembly.");
              a.AssemblyReferenceResolution += new Module.AssemblyReferenceResolver(ar.Resolve);
              assems.Add(a);
              #region Check to see if any metadata errors were reported
              if (a.MetadataImportErrors != null && a.MetadataImportErrors.Count > 0) {
                string msg = "\tThere were errors reported in " + a.Name + "'s metadata.\n";
                foreach (Exception e in a.MetadataImportErrors) {
                  msg += "\t" + e.Message;
                }
                WriteToLog(msg);
                throw new InvalidOperationException("ILMerge.Merge: " + msg);
              }
              else {
                WriteToLog("\tThere were no errors reported in {0}'s metadata.", a.Name);
                if (this.UnionMerge) {
                  // need to remove the entry for a in h because if another assembly has the same name
                  // we want to read it in fresh. Otherwise the assembly already read in is returned
                  // from GetAssembly
                  if (a.Name != null) {
                    h.Remove(a.Name);
                  }
                  if (a.StrongName != null) {
                    h.Remove(a.StrongName);
                  }
                }
              }
              #endregion
            }
          }
          #endregion
          break; // don't check any further directories, filespec was able to be found in this iteration's directory
        }
        if (currentNumberOfFoundFiles == assems.Count) {
          string msg = "Could not find the file '" + fileSpec + "'.";
          WriteToLog(msg);
          throw new InvalidOperationException("ILMerge.Merge: " + msg);
        }
      }
      #endregion
      #region Check to see if there are any assemblies to merge in
      if (!(assems.Count > 0)) {
        string msg = "There are no assemblies to merge in. Must have been an error in reading them in?";
        WriteToLog(msg);
        throw new InvalidOperationException("ILMerge.Merge: " + msg);
      }
      #endregion
      #region If desired, compute the "transitive closure" of the references, add to input
      if (closed) {
        CloseAssemblies ca = new CloseAssemblies(assems);
        for (int i = 0, n = assems.Count; i < n; i++) {
          AssemblyNode a = assems[i];
          for (int j = 0, m = a.AssemblyReferences.Count; j < m; j++) {
            ca.Visit(a.AssemblyReferences[j].Assembly);
          }
        }
        WriteToLog("In order to close the target assembly, the number of assemblies to be added to the input is {0}.",
          ca.assembliesToBeAdded.Count);
        foreach (AssemblyNode a in ca.assembliesToBeAdded.Values) {
          WriteToLog("\tAdding assembly '{0}' to the input list.", a.Name);
          assems.Add(a);
        }
      }
      #endregion
      #region Make sure that all of the assemblies have the same value for PeKind
      // 1) All assemblies should specify IL only. (But overridable because of managed C++.)
      // 2) Zero or more of the assemblies can specify either Requires32Bit, Requires64Bit
      //    or AMD, as long as all the other assemblies specify nothing.
      //  The resulting assembly gets the flag as well.

      WriteToLog("Checking to see that all of the input assemblies have a compatible PeKind.");
      PEKindFlags peKind = assems[0].PEKind;
      WriteToLog("\t" + assems[0].Name + ".PeKind = " + assems[0].PEKind);
      // MC++ doesn't seem to indicate ILonly, so allow 0 only if the user said to
      if ((peKind & PEKindFlags.ILonly) == 0) {
        if (allowZeroPeKind) {
          peKind |= PEKindFlags.ILonly;
          WriteToLog("\tThe effective PeKind for '" + assems[0].Name + "' will be considered to be: " + peKind);
        }
        else {
          string msg = "The assembly '" + assems[0].Name + "' is not marked as containing only managed code.\n"
            + "(Consider using the /zeroPeKind option -- but read the documentation first!)";
          WriteToLog(msg);
          throw new InvalidOperationException("ILMerge.Merge: " + msg);
        }
      }
      for (int i = 1, n = assems.Count; i < n; i++) {
        AssemblyNode a = assems[i];
        PEKindFlags aPEKind = a.PEKind;
        WriteToLog("\t" + a.Name + ".PeKind = " + aPEKind);
        if ((aPEKind & PEKindFlags.ILonly) == 0) {
          if (allowZeroPeKind) {
            // see note above about MC++
            aPEKind |= PEKindFlags.ILonly;
            WriteToLog("\tThe effective PeKind for '" + a.Name + "' will be considered to be: " + aPEKind);
          }
          else {
            string msg = "The assembly '" + a.Name + "' is not marked as containing only managed code.\n"
              + "(Consider using the /zeroPeKind option -- but read the documentation first!)";
            WriteToLog(msg);
            throw new InvalidOperationException("ILMerge.Merge: " + msg);
          }
        }
        if (aPEKind == peKind) // matching peKinds are cool
          continue;
        if (peKind == PEKindFlags.ILonly) {
          // then no machine specific flags have been seen so far
          peKind = aPEKind; // use whatever machine-specific setting a has
          continue;
        }
        else if (aPEKind == PEKindFlags.ILonly) {
          // machine-specific flag has already ben seen, aPeKind must either
          // match exactly or have no machine-specific setting.
          continue;
        }
        string msg2 = "The assembly '" + a.Name + "' has a value for its PeKind flag, '"
          + aPEKind + "' that is not compatible with '" + peKind + "'.";
        WriteToLog(msg2);
        throw new InvalidOperationException("ILMerge.Merge: " + msg2);
      }
      WriteToLog("All input assemblies have a compatible PeKind value.");
      #endregion Make sure that all of the assemblies have the same value for PeKind
      #region Create the skeleton of the target assembly and a duplicator for it
      ModuleKindFlags k;
      switch (targetKind) {
        case Kind.SameAsPrimaryAssembly:
          k = assems[0].Kind;
          break;
        case Kind.Dll:
          k = ModuleKindFlags.DynamicallyLinkedLibrary;
          break;
        case Kind.Exe:
          k = ModuleKindFlags.ConsoleApplication;
          break;
        case Kind.WinExe:
          k = ModuleKindFlags.WindowsApplication;
          break;
        default:
          throw new InvalidOperationException("ILMerge.Merge: Internal error.");
      }
      targetAssembly = CreateTargetAssembly(baseName, k);
      targetAssembly.PEKind = peKind;
      targetAssembly.UsePublicKeyTokensForAssemblyReferences = PublicKeyTokens;
      d = this.CreateDuplicator(targetAssembly);
      d.CopyDocumentation = this.copyXmlDocumentation;
      #endregion
      #region Scan all assemblies, collecting all types to be defined in target assembly
      if (!this.UnionMerge) { // if we're going to union types, then we want only the first occurrence of a type
        // duplicated, otherwise, members of later occurrences will get folded into the duplicate of the first
        // occurrence;
        ScanAssemblies(d, assems);
      }
      #endregion
      bool targetAssemblyIsComVisible = true; // default value
      #region Figure out which assembly to use for assembly-level attributes
      AssemblyNode primaryAssembly = assems[0];
      // Need to set some of the assembly attributes of the target assembly to be
      // the same as those of the primary assembly (the first input assembly).
      AssemblyNode attributeAssembly = null;
      if (attributeFileName != null) {
        WriteToLog("Trying to read attribute assembly from the file '{0}'.", attributeFileName);
        attributeAssembly = AssemblyNode.GetAssembly(
          attributeFileName, // path to assembly
          h,  // global cache to use for assemblies
          true, // doNotLockFile
          false, // getDebugInfo
          true, // useGlobalCache
          false  // preserveShortBranches
          );
        if (attributeAssembly == null) {
          string msg = "The assembly '" + attributeFileName + "' could not be read in to be used for assembly-level information.";
          WriteToLog(msg);
          throw new InvalidOperationException("ILMerge.Merge: " + msg);
        }
        for (int i = 0, n = attributeAssembly.Attributes == null ? 0 : attributeAssembly.Attributes.Count; i < n; i++) {
          AttributeNode aNode = attributeAssembly.Attributes[i];
          if (aNode == null) continue;
          if (aNode.Type == SystemTypes.ComVisibleAttribute) {
            attributeAssembly.Attributes[i] = null;
            continue;
          }
          if (aNode.Type == SystemTypes.SecurityCriticalAttribute
            || aNode.Type == SystemTypes.SecurityTransparentAttribute
            || aNode.Type == SystemTypes.AllowPartiallyTrustedCallersAttribute
            || aNode.Type.FullName.Equals("System.Security.SecurityRules")
            ) {
            WriteToLog("Assembly level attribute '{0}' from assembly '{1}' being deleted from target assembly",
              aNode.Type.FullName, attributeAssembly.Name);
            attributeAssembly.Attributes[i] = null;
            continue;
          }
        }
      }
      AssemblyNode assemblyLevelAttributesAssembly =
        attributeAssembly != null ? attributeAssembly : primaryAssembly;
      if (!this.copyattrs) {
        WriteToLog("Using assembly '{0}' for assembly-level attributes for the target assembly.", assemblyLevelAttributesAssembly.Name);
      } else {
        WriteToLog("Merging assembly-level attributes from input assemblies for the target assembly.");
        // Need to record value before attribute gets wiped out
      }
      targetAssemblyIsComVisible = GetComVisibleSettingForAssembly(assemblyLevelAttributesAssembly);
      #endregion
      #region Merge each assembly into the target assembly
      AssemblyNodeList externalReferences = new AssemblyNodeList();
      for (int i = 0, n = assems.Count; i < n; i++) {
        AssemblyNode a = assems[i];
        WriteToLog("Merging assembly '{0}' into target assembly.", a.Name);
        bool makeNonPublic = false;
        if (i == 0) // primary assembly never has its types modified
          makeNonPublic = false;
        else
          makeNonPublic = internalize;
        bool success;
        if (this.UnionMerge) {
          if (i == 0) { // i.e., this is the first assembly
            for (int j = 0, m = a.AssemblyReferences == null ? 0 : a.AssemblyReferences.Count; j < m; j++) {
              //Console.WriteLine("Adding: '" + a.AssemblyReferences[j].Assembly.Name + "'");
              externalReferences.Add(a.AssemblyReferences[j].Assembly);
            }
            d.FindTypesToBeDuplicated(a.Types); // all of these must be the first occurrence of each type
          } else {
            for (int j = 0, m = a.AssemblyReferences == null ? 0 : a.AssemblyReferences.Count; j < m; j++) {
              AssemblyNode possiblyDuplicateExternalReference = a.AssemblyReferences[j].Assembly;
              int j2 = 0;
              int p = externalReferences.Count;
              while (j2 < p) {
                AssemblyNode alreadyExistingReference = externalReferences[j2];
                if (possiblyDuplicateExternalReference.Name == alreadyExistingReference.Name) {
                  FuzzilyForwardReferencesFromSource2Target(alreadyExistingReference, possiblyDuplicateExternalReference);
                  break;
                }
                j2++;
              }
              if (j2 == p) { // brand new reference
                //Console.WriteLine("Adding: '" + a.AssemblyReferences[j].Assembly.Name + "'");
                externalReferences.Add(a.AssemblyReferences[j].Assembly);
              }
            }

          }
          success = MergeInAssembly_Union(a, targetAssemblyIsComVisible);
        }
        else {
          success = MergeInAssembly(a, makeNonPublic, targetAssemblyIsComVisible);
        }
        if (!success) {
          WriteToLog("Could not merge in assembly. Skipping and processing rest of arguments.");
        }
      }
      #endregion
      #region Assembly Level Features
      #region Visibility of hidden class in the target assembly
      TypeNode inputHiddenClass = null;
      // use whatever assembly the user specified as the place to get assembly-level information
      // note that if they didn't specify anything, then this is the primary assembly anyway
      if (assemblyLevelAttributesAssembly.Types != null && assemblyLevelAttributesAssembly.Types.Count > 0) {
        inputHiddenClass = assemblyLevelAttributesAssembly.Types[0];
      }
      // even if they specified a different assembly than the primary assembly, it is possible that
      // the information couldn't be found there, so fall back to using the primary assembly
      if (inputHiddenClass == null && primaryAssembly.Types != null && primaryAssembly.Types.Count > 0) {
        inputHiddenClass = primaryAssembly.Types[0];
      }
      // just to be ultra-careful in case the hidden class couldn't be found at all.
      if (inputHiddenClass != null) {
        this.hiddenClass.Flags = inputHiddenClass.Flags;
      }
      #endregion
      #region Copy over assembly-level attributes (when /copyattrs is *not* specified)
      if (!this.copyattrs) {
        // when /copyattrs is specified, then assembly-level attributes are coppied over as part
        // of MergeInAssembly
        // Note: this is tricky in that the assemblyLevelAttributesAssembly could be the primary assembly.
        // If so, then this next line is counting on the processing in MergeInAssembly to have stripped
        // out the security attributes we don't want passed into the target assembly. So that processing
        // has to happen even if copyattrs is *not* true, but in that case it is wasted work for all
        // of the assemblies other than the primary one since none of their assembly-level attributes are
        // going to make it into the target assembly anyway.
        targetAssembly.Attributes = d.VisitAttributeList(assemblyLevelAttributesAssembly.Attributes);
        targetAssembly.SecurityAttributes = d.VisitSecurityAttributeList(assemblyLevelAttributesAssembly.SecurityAttributes);
        targetAssembly.ModuleAttributes = d.VisitAttributeList(assemblyLevelAttributesAssembly.ModuleAttributes); //Although an assembly is a module, metadata has two tables and thus two custom attribute targets
      }
      targetAssembly.Culture = assemblyLevelAttributesAssembly.Culture;
      targetAssembly.DllCharacteristics = assemblyLevelAttributesAssembly.DllCharacteristics;
      targetAssembly.Flags = assemblyLevelAttributesAssembly.Flags;
      targetAssembly.PublicKeyOrToken = (byte[])assemblyLevelAttributesAssembly.PublicKeyOrToken.Clone();
      targetAssembly.KeyContainerName = assemblyLevelAttributesAssembly.KeyContainerName;
      if (assemblyLevelAttributesAssembly.KeyBlob != null)
        targetAssembly.KeyBlob = (byte[])assemblyLevelAttributesAssembly.KeyBlob.Clone();
      targetAssembly.Version = (System.Version)assemblyLevelAttributesAssembly.Version.Clone();
      if (assemblyLevelAttributesAssembly == null ||
        assemblyLevelAttributesAssembly.Win32Resources == null ||
        assemblyLevelAttributesAssembly.Win32Resources.Count <= 0) {
        WriteToLog("No Win32 Resources found in assembly '{0}'; target assembly will (also) not have any.", assemblyLevelAttributesAssembly.Name);
      }
      else {
        int len = assemblyLevelAttributesAssembly.Win32Resources.Count;
        WriteToLog("Copying {1} Win32 Resources from assembly '{0}' into target assembly.", assemblyLevelAttributesAssembly.Name, len);
        targetAssembly.Win32Resources = new Win32ResourceList(len);
        for (int i = 0, n = len; i < n; i++) {
          targetAssembly.Win32Resources.Add(assemblyLevelAttributesAssembly.Win32Resources[i]);
        }
      }
      #endregion
      #region Set [ComVisible] on target assembly
      var comVisiblector = SystemTypes.ComVisibleAttribute.GetConstructor(SystemTypes.Boolean);
      var assemblyComVisibleAttribute = new AttributeNode(new MemberBinding(null, comVisiblector), new ExpressionList(new Literal(targetAssemblyIsComVisible, SystemTypes.Boolean)));
      targetAssembly.Attributes.Add(assemblyComVisibleAttribute);
      #endregion
      #endregion
      #region Entry Point consideration
      if (targetAssembly.Kind == ModuleKindFlags.ConsoleApplication
        || targetAssembly.Kind == ModuleKindFlags.WindowsApplication) {
        if (primaryAssembly.EntryPoint == null) {
          WriteToLog("Trying to make the target assembly an executable, but cannot find an entry point in the primary assembly, '{0}'.",
            primaryAssembly.Name);
          WriteToLog("Converting target assembly into a dll.");
          targetAssembly.Kind = ModuleKindFlags.DynamicallyLinkedLibrary;
          outputFileName = Path.ChangeExtension(outputFileName, "dll");
        }
        else {
          // need to find the equivalent method in the targetAssembly
          Method ep = primaryAssembly.EntryPoint;
          string msg = "entry point '" +
            ep.FullName + "' from assembly '" +
            primaryAssembly.Name + "' to assembly '" +
            targetAssembly.Name + "'.";
          WriteToLog("Transferring " + msg);
          Class epClass = (Class)ep.DeclaringType;
          Class dup = (Class)d.DuplicateFor[epClass.UniqueKey];
          // force evaluation of Members, otherwise ep will not have a duplicate in the table
          int i = dup.Members.Count;

          targetAssembly.EntryPoint = (Method)d.DuplicateFor[ep.UniqueKey];
          if (targetAssembly.EntryPoint == null) {
            WriteToLog("Error in transferring " + msg);
            throw new InvalidOperationException("Error in transferring " + msg);
          }
        }
      }
      #endregion
      #region Signing Assembly
      if (keyContainer != null)
      {
        targetAssembly.KeyContainerName = keyContainer;
      }
      else if (keyfile != null) {
        if (!File.Exists(keyfile))
          WriteToLog("ILMerge: Cannot open key file: '{0}'. Not trying to sign output.", keyfile);
        else {
          System.IO.FileStream fs = File.Open(keyfile, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
          long len1 = fs.Length;
          int len = (int)len1;
          byte[] contents = new byte[len];
          int n = fs.Read(contents, 0, len);
          if (n != len)
            WriteToLog("ILMerge: Error reading contents of key file: '{0}'. Not trying to sign output.", keyfile);
          else {
            WriteToLog("ILMerge: Signing assembly with the key file '{0}'.", keyfile);
            targetAssembly.KeyBlob = new byte[len];
            Array.Copy(contents, 0, targetAssembly.KeyBlob, 0, len);
          }
          fs.Close();
        }
      }
      else if (primaryAssembly.PublicKeyOrToken != null && primaryAssembly.PublicKeyOrToken.Length > 0) {
        WriteToLog("ILMerge: Important! Primary assembly had a strong name, but the output does not.");
        targetAssembly.PublicKeyOrToken = null;
        if ((targetAssembly.Flags & AssemblyFlags.PublicKey) != 0)
          targetAssembly.Flags = targetAssembly.Flags & ~AssemblyFlags.PublicKey;
        strongNameLost = true;
      }
      #endregion
      #region Modify the version number if that has been specified
      if (version != null) {
        targetAssembly.Version = version;
      }
      #endregion
      #region External Visitor (Internal builds only)
#if INTERNAL
      if (externalVisitor != null) {
        externalVisitor.Visit(targetAssembly);
      }
#endif
            #endregion
      #region Check for errors that CCI caught
      if (targetAssembly.MetadataImportErrors != null && targetAssembly.MetadataImportErrors.Count > 0) {
        WriteToLog("\tThere were errors reported in the target assembly's metadata.");
        foreach (Exception e in targetAssembly.MetadataImportErrors) {
          WriteToLog("\t" + e.Message);
        }
      }
      else {
        WriteToLog("\tThere were no errors reported in the target assembly's metadata.");
      }
      #endregion
      #region Write out target assembly
      WriteToLog("ILMerge: Writing target assembly '{0}'.", outputFileName);

      CompilerOptions co = new CompilerOptions();
      if (delaySign) {
        #region Make sure the target assembly has the AssemblyDelaySign attribute
        if (targetAssembly.Attributes == null) {
          targetAssembly.Attributes = new AttributeList(1);
        }
        int i = 0;
        int n = targetAssembly.Attributes.Count;
        while (i < n) {
          AttributeNode a = targetAssembly.Attributes[i];
          if (a != null && a.Type == SystemTypes.AssemblyDelaySignAttribute) {
            break;
          }
          i++;
        }
        if (i == n) { // not found
          InstanceInitializer ctor = SystemTypes.AssemblyDelaySignAttribute.GetConstructor(SystemTypes.Boolean);
          if (ctor != null) {
            AttributeNode a = new AttributeNode(new MemberBinding(null, ctor), new ExpressionList(Literal.True));
            targetAssembly.Attributes.Add(a);
          }
        }
        #endregion Make sure the target assembly has the AssemblyDelaySign attribute

        co.DelaySign = true;
        co.IncludeDebugInformation = debugInfo;
        co.FileAlignment = this.FileAlignment;
        // No other options are set via "co". WriteModule (and the methods it calls)
        // will pull the other information out of the target assembly.
        targetAssembly.WriteModule(outputFileName, co);
        if (this.copyXmlDocumentation)
          targetAssembly.WriteDocumentation(new StreamWriter(Path.ChangeExtension(outputFileName, "xml")));
        WriteToLog("ILMerge: Delay signed assembly '{0}'.", outputFileName);
      }
      else {
        try {
          co.IncludeDebugInformation = debugInfo;
          co.FileAlignment = this.FileAlignment;
          // No other options are set via "co". WriteModule (and the methods it calls)
          // will pull the other information out of the target assembly.
          targetAssembly.WriteModule(outputFileName, co);
          if (this.copyXmlDocumentation)
            targetAssembly.WriteDocumentation(new StreamWriter(Path.ChangeExtension(outputFileName, "xml")));
          if (keyfileSpecified) {
            WriteToLog("ILMerge: Signed assembly '{0}' with a strong name.", outputFileName);
          }
        }
        catch (AssemblyCouldNotBeSignedException ex) {
          if (ex.Message == AssemblyCouldNotBeSignedException.DefaultMessage)
            WriteToLog("ILMerge error: The target assembly was not able to be strongly named (did you forget to use the /delaysign option?).");
          else
            WriteToLog("ILMerge error: The target assembly was not able to be strongly named. " + ex.Message);
        }
      }
      #endregion
      #region Things that cannot be done until target assembly is written out
      #region Check to make sure none of the external references are to any of the input assemblies or the target assembly
      for (int i = 0, n = targetAssembly.AssemblyReferences.Count; i < n; i++) {
        AssemblyNode aref = targetAssembly.AssemblyReferences[i].Assembly;
        if (String.Compare(((Module)aref).Name, targetAssembly.Name, true) == 0) {
          string msg = "The target assembly '" + targetAssembly.Name + "' lists itself as an external reference.";
          WriteToLog(msg);
          throw new InvalidOperationException("ILMerge.Merge: " + msg);
        }
        for (int j = 0, m = assems.Count; j < m; j++) {
          if (aref == assems[j]) {
            string msg = "The assembly '" + assems[j].Name + "' was not merged in correctly. It is still listed as an external reference in the target assembly.";
            WriteToLog(msg);
            throw new InvalidOperationException("ILMerge.Merge: " + msg);
          }
        }
      }
      #endregion Check to make sure none of the external references are to any of the input assemblies or the target assembly
      #region Print some information about location of references to log
      try {
        for (int i = 0; i < targetAssembly.ModuleReferences.Count; i++) {
          Module m = targetAssembly.ModuleReferences[i].Module;
          WriteToLog("Location for referenced module '{0}' is '{1}'", m.Name, m.Location);
        }
        for (int i = 0; i < targetAssembly.AssemblyReferences.Count; i++) {
          AssemblyNode aref = targetAssembly.AssemblyReferences[i].Assembly;
          WriteToLog("Location for referenced assembly '{0}' is '{1}'", aref.Name, aref.Location);
          #region Check for errors that CCI caught
          if (aref.MetadataImportErrors != null && aref.MetadataImportErrors.Count > 0) {
            WriteToLog("\tThere were errors reported in {0}'s metadata.", aref.Name);
            foreach (Exception e in aref.MetadataImportErrors) {
              WriteToLog("\t" + e.Message);
            }
          }
          else {
            WriteToLog("\tThere were no errors reported in  {0}'s metadata.", aref.Name);
          }
          #endregion
        }
      }
      catch (Exception e) {
        WriteToLog("Exception occurred while trying to print out information on references.");
        WriteToLog(e.ToString());
      }
      #endregion
      #region Disposing
      targetAssembly.Dispose();
      TargetPlatform.Clear();
      #endregion
      #endregion Things that cannot be done until target assembly is written out
      WriteToLog("ILMerge: Done.");
      return;
    }
    #endregion
    #endregion
    #region Entry point when used as an executable
    /// <summary>
    /// Given a set of IL assemblies, merge them into a single IL assembly.
    /// All inter-assembly references in the set are resolved to be
    /// intra-assembly references in the merged assembly.
    /// </summary>
    [STAThread]
    public static int Main(string[] args) {
      ILMerge merger = new ILMerge();
      #region Check Usage
      if (merger.CheckUsage(args)) {
        Console.WriteLine(merger.UsageString);
        return 0;
      }
      #endregion
      #region Process Command Line Arguments
      if (!merger.ProcessCommandLineOptions(args)) {
        Console.WriteLine(merger.UsageString);
        return 1;
      }
      #endregion
      #region Validate Options
      if (!merger.ValidateOptions()) {
        Console.WriteLine(merger.UsageString);
        return 1;
      }
      #endregion
      #region Echo command line into log
      {
        System.Reflection.Assembly a = typeof(ILMerge).Assembly;
        merger.WriteToLog("ILMerge version " + a.GetName().Version.ToString());
        merger.WriteToLog("Copyright (C) Microsoft Corporation 2004-2006. All rights reserved.");
      }
      string commandLine = "ILMerge ";
      foreach (string s in args) {
        commandLine += s + " ";
      }
      merger.WriteToLog(commandLine);
      #endregion Echo command line into log
      #region Perform real merging
      try {
        merger.Merge();
      }
      catch (Exception e) {
        if (merger.log) {
          merger.WriteToLog("An exception occurred during merging:");
          merger.WriteToLog(e.Message);
          merger.WriteToLog(e.StackTrace);
        }
        else {
          Console.WriteLine("An exception occurred during merging:");
          Console.WriteLine(e.Message);
          Console.WriteLine(e.StackTrace);
        }
        return 1;
      }
            #endregion
      return 0;
    }
    #endregion
  }
}
