using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
#if FxCop
using AttributeList = Microsoft.Cci.AttributeNodeCollection;
using BlockList = Microsoft.Cci.BlockCollection;
using ExpressionList = Microsoft.Cci.ExpressionCollection;
using InterfaceList = Microsoft.Cci.InterfaceCollection;
using MemberList = Microsoft.Cci.MemberCollection;
using NodeList = Microsoft.Cci.NodeCollection;
using ParameterList = Microsoft.Cci.ParameterCollection;
using SecurityAttributeList = Microsoft.Cci.SecurityAttributeCollection;
using StatementList = Microsoft.Cci.StatementCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
using Property = Microsoft.Cci.PropertyNode;
using Module = Microsoft.Cci.ModuleNode;
using Return = Microsoft.Cci.ReturnNode;
using Class = Microsoft.Cci.ClassNode;
using Interface = Microsoft.Cci.InterfaceNode;
using Event = Microsoft.Cci.EventNode;
using Throw = Microsoft.Cci.ThrowNode;
#endif
#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler
{
#endif
  public class Inspector
  {
    public Inspector()
    {
    }
    public virtual void VisitUnknownNodeType(Node node)
    {
      Debug.Assert(false);
    }
    public virtual void Visit(Node node)
    {
      if (node == null) return;
      switch (node.NodeType)
      {
#if !MinimalReader && !CodeContracts
        case NodeType.Acquire:
          this.VisitAcquire((Acquire)node);
          return;
#endif
        case NodeType.AddressDereference:
          this.VisitAddressDereference((AddressDereference)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.AliasDefinition:
          this.VisitAliasDefinition((AliasDefinition)node);
          return;
        case NodeType.AnonymousNestedFunction:
          this.VisitAnonymousNestedFunction((AnonymousNestedFunction)node);
          return;
        case NodeType.ApplyToAll:
          this.VisitApplyToAll((ApplyToAll)node);
          return;
#endif
        case NodeType.Arglist:
          this.VisitExpression((Expression)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.ArglistArgumentExpression:
          this.VisitArglistArgumentExpression((ArglistArgumentExpression)node);
          return;
        case NodeType.ArglistExpression:
          this.VisitArglistExpression((ArglistExpression)node);
          return;
#endif
        case NodeType.ArrayType:
          Debug.Assert(false); return;
        case NodeType.Assembly:
          this.VisitAssembly((AssemblyNode)node);
          return;
        case NodeType.AssemblyReference:
          this.VisitAssemblyReference((AssemblyReference)node);
          return;
#if !MinimalReader
        case NodeType.Assertion:
          this.VisitAssertion((Assertion)node);
          return;
        case NodeType.Assumption:
          this.VisitAssumption((Assumption)node);
          return;
        case NodeType.AssignmentExpression:
          this.VisitAssignmentExpression((AssignmentExpression)node);
          return;
#endif
        case NodeType.AssignmentStatement:
          this.VisitAssignmentStatement((AssignmentStatement)node);
          return;
        case NodeType.Attribute:
          this.VisitAttributeNode((AttributeNode)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Base:
          this.VisitBase((Base)node);
          return;
#endif
        case NodeType.Block:
          this.VisitBlock((Block)node);
          return;
#if !MinimalReader
        case NodeType.BlockExpression:
          this.VisitBlockExpression((BlockExpression)node);
          return;
#endif
        case NodeType.Branch:
          this.VisitBranch((Branch)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Compilation:
          this.VisitCompilation((Compilation)node);
          return;
        case NodeType.CompilationUnit:
          this.VisitCompilationUnit((CompilationUnit)node);
          return;
        case NodeType.CompilationUnitSnippet:
          this.VisitCompilationUnitSnippet((CompilationUnitSnippet)node);
          return;
#endif
#if ExtendedRuntime
        case NodeType.ConstrainedType:
          this.VisitConstrainedType((ConstrainedType)node);
          return;
#endif
#if !MinimalReader && !CodeContracts
        case NodeType.Continue:
          this.VisitContinue((Continue)node);
          return;
        case NodeType.CurrentClosure:
          this.VisitCurrentClosure((CurrentClosure)node);
          return;
#endif
        case NodeType.DebugBreak:
          return;
        case NodeType.Call:
        case NodeType.Calli:
        case NodeType.Callvirt:
        case NodeType.Jmp:
#if !MinimalReader
        case NodeType.MethodCall:
#endif
          this.VisitMethodCall((MethodCall)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Catch:
          this.VisitCatch((Catch)node);
          return;
#endif
        case NodeType.Class:
          this.VisitClass((Class)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.CoerceTuple:
          this.VisitCoerceTuple((CoerceTuple)node);
          return;
        case NodeType.CollectionEnumerator:
          this.VisitCollectionEnumerator((CollectionEnumerator)node);
          return;
        case NodeType.Composition:
          this.VisitComposition((Composition)node);
          return;
#endif
        case NodeType.Construct:
          this.VisitConstruct((Construct)node);
          return;
        case NodeType.ConstructArray:
          this.VisitConstructArray((ConstructArray)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.ConstructDelegate:
          this.VisitConstructDelegate((ConstructDelegate)node);
          return;
        case NodeType.ConstructFlexArray:
          this.VisitConstructFlexArray((ConstructFlexArray)node);
          return;
        case NodeType.ConstructIterator:
          this.VisitConstructIterator((ConstructIterator)node);
          return;
        case NodeType.ConstructTuple:
          this.VisitConstructTuple((ConstructTuple)node);
          return;
#endif
        case NodeType.DelegateNode:
          this.VisitDelegateNode((DelegateNode)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.DoWhile:
          this.VisitDoWhile((DoWhile)node);
          return;
#endif
        case NodeType.Dup:
          this.VisitExpression((Expression)node);
          return;
        case NodeType.EndFilter:
          this.VisitEndFilter((EndFilter)node);
          return;
        case NodeType.EndFinally:
          this.VisitEndFinally((EndFinally)node);
          return;
        case NodeType.EnumNode:
          this.VisitEnumNode((EnumNode)node);
          return;
        case NodeType.Event:
          this.VisitEvent((Event)node);
          return;
#if ExtendedRuntime || CodeContracts
        case NodeType.EnsuresExceptional:
          this.VisitEnsuresExceptional((EnsuresExceptional)node);
          return;
#endif
#if !MinimalReader && !CodeContracts
        case NodeType.Exit:
          this.VisitExit((Exit)node);
          return;
        case NodeType.Read:
        case NodeType.Write:
          this.VisitExpose((Expose)node);
          return;
        case NodeType.ExpressionSnippet:
          this.VisitExpressionSnippet((ExpressionSnippet)node);
          return;
#endif
        case NodeType.ExpressionStatement:
          this.VisitExpressionStatement((ExpressionStatement)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.FaultHandler:
          this.VisitFaultHandler((FaultHandler)node);
          return;
#endif
        case NodeType.Field:
          this.VisitField((Field)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.FieldInitializerBlock:
          this.VisitFieldInitializerBlock((FieldInitializerBlock)node);
          return;
        case NodeType.Finally:
          this.VisitFinally((Finally)node);
          return;
        case NodeType.Filter:
          this.VisitFilter((Filter)node);
          return;
        case NodeType.Fixed:
          this.VisitFixed((Fixed)node);
          return;
        case NodeType.For:
          this.VisitFor((For)node);
          return;
        case NodeType.ForEach:
          this.VisitForEach((ForEach)node);
          return;
        case NodeType.FunctionDeclaration:
          this.VisitFunctionDeclaration((FunctionDeclaration)node);
          return;
        case NodeType.Goto:
          this.VisitGoto((Goto)node);
          return;
        case NodeType.GotoCase:
          this.VisitGotoCase((GotoCase)node);
          return;
#endif
        case NodeType.Identifier:
          this.VisitIdentifier((Identifier)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.If:
          this.VisitIf((If)node);
          return;
        case NodeType.ImplicitThis:
          this.VisitImplicitThis((ImplicitThis)node);
          return;
#endif
        case NodeType.Indexer:
          this.VisitIndexer((Indexer)node);
          return;
        case NodeType.InstanceInitializer:
          this.VisitInstanceInitializer((InstanceInitializer)node);
          return;
#if ExtendedRuntime || CodeContracts
        case NodeType.Invariant:
          this.VisitInvariant((Invariant)node);
          return;
#endif
        case NodeType.StaticInitializer:
          this.VisitStaticInitializer((StaticInitializer)node);
          return;
        case NodeType.Method:
          this.VisitMethod((Method)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.TemplateInstance:
          this.VisitTemplateInstance((TemplateInstance)node);
          return;
        case NodeType.StackAlloc:
          this.VisitStackAlloc((StackAlloc)node);
          return;
#endif
        case NodeType.Interface:
          this.VisitInterface((Interface)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.LabeledStatement:
          this.VisitLabeledStatement((LabeledStatement)node);
          return;
#endif
        case NodeType.Literal:
          this.VisitLiteral((Literal)node);
          return;
        case NodeType.Local:
          this.VisitLocal((Local)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.LocalDeclaration:
          this.VisitLocalDeclaration((LocalDeclaration)node);
          return;
        case NodeType.LocalDeclarationsStatement:
          this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node);
          return;
        case NodeType.Lock:
          this.VisitLock((Lock)node);
          return;
        case NodeType.LRExpression:
          this.VisitLRExpression((LRExpression)node);
          return;
#endif
        case NodeType.MemberBinding:
          this.VisitMemberBinding((MemberBinding)node);
          return;
#if ExtendedRuntime || CodeContracts
        case NodeType.MethodContract:
          this.VisitMethodContract((MethodContract)node);
          return;
#endif
        case NodeType.Module:
          this.VisitModule((Module)node);
          return;
        case NodeType.ModuleReference:
          this.VisitModuleReference((ModuleReference)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.NameBinding:
          this.VisitNameBinding((NameBinding)node);
          return;
#endif
        case NodeType.NamedArgument:
          this.VisitNamedArgument((NamedArgument)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Namespace:
          this.VisitNamespace((Namespace)node);
          return;
#endif
        case NodeType.Nop:
#if !MinimalReader && !CodeContracts
        case NodeType.SwitchCaseBottom:
#endif
          return;
#if ExtendedRuntime || CodeContracts
        case NodeType.EnsuresNormal:
          this.VisitEnsuresNormal((EnsuresNormal)node);
          return;
        case NodeType.OldExpression:
          this.VisitOldExpression((OldExpression)node);
          return;
        case NodeType.ReturnValue:
          this.VisitReturnValue((ReturnValue)node);
          return;
        case NodeType.RequiresOtherwise:
          this.VisitRequiresOtherwise((RequiresOtherwise)node);
          return;
        case NodeType.RequiresPlain:
          this.VisitRequiresPlain((RequiresPlain)node);
          return;
#endif
        case NodeType.OptionalModifier:
        case NodeType.RequiredModifier:
          //TODO: type modifers should only be visited via VisitTypeReference
          this.VisitTypeModifier((TypeModifier)node);
          return;
        case NodeType.Parameter:
          this.VisitParameter((Parameter)node);
          return;
        case NodeType.Pop:
          this.VisitExpression((Expression)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.PrefixExpression:
          this.VisitPrefixExpression((PrefixExpression)node);
          return;
        case NodeType.PostfixExpression:
          this.VisitPostfixExpression((PostfixExpression)node);
          return;
#endif
        case NodeType.Property:
          this.VisitProperty((Property)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Quantifier:
          this.VisitQuantifier((Quantifier)node);
          return;
        case NodeType.Comprehension:
          this.VisitComprehension((Comprehension)node);
          return;
        case NodeType.ComprehensionBinding:
          this.VisitComprehensionBinding((ComprehensionBinding)node);
          return;
        case NodeType.QualifiedIdentifer:
          this.VisitQualifiedIdentifier((QualifiedIdentifier)node);
          return;
#endif
        case NodeType.Rethrow:
        case NodeType.Throw:
          this.VisitThrow((Throw)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.RefValueExpression:
          this.VisitRefValueExpression((RefValueExpression)node);
          return;
        case NodeType.RefTypeExpression:
          this.VisitRefTypeExpression((RefTypeExpression)node);
          return;
#endif
        case NodeType.Return:
          this.VisitReturn((Return)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Repeat:
          this.VisitRepeat((Repeat)node);
          return;
        case NodeType.ResourceUse:
          this.VisitResourceUse((ResourceUse)node);
          return;
#endif
        case NodeType.SecurityAttribute:
          this.VisitSecurityAttribute((SecurityAttribute)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.SetterValue:
          this.VisitSetterValue((SetterValue)node);
          return;
        case NodeType.StatementSnippet:
          this.VisitStatementSnippet((StatementSnippet)node);
          return;
#endif
        case NodeType.Struct:
          this.VisitStruct((Struct)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Switch:
          this.VisitSwitch((Switch)node);
          return;
        case NodeType.SwitchCase:
          this.VisitSwitchCase((SwitchCase)node);
          return;
#endif
        case NodeType.SwitchInstruction:
          this.VisitSwitchInstruction((SwitchInstruction)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Typeswitch:
          this.VisitTypeswitch((Typeswitch)node);
          return;
        case NodeType.TypeswitchCase:
          this.VisitTypeswitchCase((TypeswitchCase)node);
          return;
#endif
        case NodeType.This:
          this.VisitThis((This)node);
          return;
#if !MinimalReader && !CodeContracts
        case NodeType.Try:
          this.VisitTry((Try)node);
          return;
#endif
#if ExtendedRuntime || CodeContracts
        case NodeType.TypeContract:
          this.VisitTypeContract((TypeContract)node);
          return;
#endif
#if ExtendedRuntime
        case NodeType.TupleType:
          this.VisitTupleType((TupleType)node);
          return;
        case NodeType.TypeAlias:
          this.VisitTypeAlias((TypeAlias)node);
          return;
        case NodeType.TypeIntersection:
          this.VisitTypeIntersection((TypeIntersection)node);
          return;
#endif
#if !MinimalReader && !CodeContracts
        case NodeType.TypeMemberSnippet:
          this.VisitTypeMemberSnippet((TypeMemberSnippet)node);
          return;
#endif
        case NodeType.ClassParameter:
        case NodeType.TypeParameter:
          this.VisitTypeParameter((TypeNode)node);
          return;
#if ExtendedRuntime
        case NodeType.TypeUnion:
          this.VisitTypeUnion((TypeUnion)node);
          return;
#endif
#if !MinimalReader && !CodeContracts
        case NodeType.TypeReference:
          this.VisitTypeReference((TypeReference)node);
          return;
        case NodeType.UsedNamespace:
          this.VisitUsedNamespace((UsedNamespace)node);
          return;
        case NodeType.VariableDeclaration:
          this.VisitVariableDeclaration((VariableDeclaration)node);
          return;
        case NodeType.While:
          this.VisitWhile((While)node);
          return;
        case NodeType.Yield:
          this.VisitYield((Yield)node);
          return;

        case NodeType.Conditional:
#endif
        case NodeType.Cpblk:
        case NodeType.Initblk:
          this.VisitTernaryExpression((TernaryExpression)node);
          return;

        case NodeType.Add:
        case NodeType.Add_Ovf:
        case NodeType.Add_Ovf_Un:
#if !MinimalReader && !CodeContracts
        case NodeType.AddEventHandler:
#endif
        case NodeType.And:
#if !MinimalReader && !CodeContracts
        case NodeType.As:
#endif
        case NodeType.Box:
        case NodeType.Castclass:
        case NodeType.Ceq:
        case NodeType.Cgt:
        case NodeType.Cgt_Un:
        case NodeType.Clt:
        case NodeType.Clt_Un:
#if !MinimalReader && !CodeContracts
        case NodeType.Comma:
#endif
        case NodeType.Div:
        case NodeType.Div_Un:
        case NodeType.Eq:
#if !MinimalReader && !CodeContracts
        case NodeType.ExplicitCoercion:
#endif
        case NodeType.Ge:
        case NodeType.Gt:
#if !MinimalReader && !CodeContracts
        case NodeType.Is:
        case NodeType.Iff:
        case NodeType.Implies:
#endif
        case NodeType.Isinst:
        case NodeType.Ldvirtftn:
        case NodeType.Le:
#if !MinimalReader && !CodeContracts
        case NodeType.LogicalAnd:
        case NodeType.LogicalOr:
#endif
        case NodeType.Lt:
        case NodeType.Mkrefany:
#if !MinimalReader && !CodeContracts
        case NodeType.Maplet:
#endif
        case NodeType.Mul:
        case NodeType.Mul_Ovf:
        case NodeType.Mul_Ovf_Un:
        case NodeType.Ne:
        case NodeType.Or:
#if !MinimalReader && !CodeContracts
        case NodeType.NullCoalesingExpression:
        case NodeType.Range:
#endif
        case NodeType.Refanyval:
        case NodeType.Rem:
        case NodeType.Rem_Un:
#if !MinimalReader && !CodeContracts
        case NodeType.RemoveEventHandler:
#endif
        case NodeType.Shl:
        case NodeType.Shr:
        case NodeType.Shr_Un:
        case NodeType.Sub:
        case NodeType.Sub_Ovf:
        case NodeType.Sub_Ovf_Un:
        case NodeType.Unbox:
        case NodeType.UnboxAny:
        case NodeType.Xor:
          this.VisitBinaryExpression((BinaryExpression)node);
          return;

        case NodeType.AddressOf:
#if !MinimalReader && !CodeContracts
        case NodeType.OutAddress:
        case NodeType.RefAddress:
#endif
        case NodeType.Ckfinite:
        case NodeType.Conv_I:
        case NodeType.Conv_I1:
        case NodeType.Conv_I2:
        case NodeType.Conv_I4:
        case NodeType.Conv_I8:
        case NodeType.Conv_Ovf_I:
        case NodeType.Conv_Ovf_I1:
        case NodeType.Conv_Ovf_I1_Un:
        case NodeType.Conv_Ovf_I2:
        case NodeType.Conv_Ovf_I2_Un:
        case NodeType.Conv_Ovf_I4:
        case NodeType.Conv_Ovf_I4_Un:
        case NodeType.Conv_Ovf_I8:
        case NodeType.Conv_Ovf_I8_Un:
        case NodeType.Conv_Ovf_I_Un:
        case NodeType.Conv_Ovf_U:
        case NodeType.Conv_Ovf_U1:
        case NodeType.Conv_Ovf_U1_Un:
        case NodeType.Conv_Ovf_U2:
        case NodeType.Conv_Ovf_U2_Un:
        case NodeType.Conv_Ovf_U4:
        case NodeType.Conv_Ovf_U4_Un:
        case NodeType.Conv_Ovf_U8:
        case NodeType.Conv_Ovf_U8_Un:
        case NodeType.Conv_Ovf_U_Un:
        case NodeType.Conv_R4:
        case NodeType.Conv_R8:
        case NodeType.Conv_R_Un:
        case NodeType.Conv_U:
        case NodeType.Conv_U1:
        case NodeType.Conv_U2:
        case NodeType.Conv_U4:
        case NodeType.Conv_U8:
#if !MinimalReader && !CodeContracts
        case NodeType.Decrement:
        case NodeType.DefaultValue:
        case NodeType.Increment:
#endif
        case NodeType.Ldftn:
        case NodeType.Ldlen:
        case NodeType.Ldtoken:
        case NodeType.Localloc:
        case NodeType.LogicalNot:
        case NodeType.Neg:
        case NodeType.Not:
#if !MinimalReader && !CodeContracts
        case NodeType.Parentheses:
#endif
        case NodeType.Refanytype:
        case NodeType.ReadOnlyAddressOf:
        case NodeType.Sizeof:
        case NodeType.SkipCheck:
#if !MinimalReader && !CodeContracts
        case NodeType.Typeof:
        case NodeType.UnaryPlus:
#endif
          this.VisitUnaryExpression((UnaryExpression)node);
          return;
        default:
          this.VisitUnknownNodeType(node);
          return;
      }
    }
    public virtual void VisitAddressDereference(AddressDereference addr)
    {
      if (addr == null) return;
      this.VisitExpression(addr.Address);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitAliasDefinition(AliasDefinition aliasDefinition)
    {
      if (aliasDefinition == null) return;
      this.VisitTypeReference(aliasDefinition.AliasedType);
    }
    public virtual void VisitAliasDefinitionList(AliasDefinitionList aliasDefinitions)
    {
      if (aliasDefinitions == null) return;
      for (int i = 0, n = aliasDefinitions.Count; i < n; i++)
        this.VisitAliasDefinition(aliasDefinitions[i]);
    }
    public virtual void VisitAnonymousNestedFunction(AnonymousNestedFunction func)
    {
      if (func == null) return;
      this.VisitParameterList(func.Parameters);
      this.VisitBlock(func.Body);
    }
    public virtual void VisitApplyToAll(ApplyToAll applyToAll)
    {
      if (applyToAll == null) return;
      this.VisitExpression(applyToAll.Operand1);
      this.VisitExpression(applyToAll.Operand2);
    }
    public void VisitArrayType(ArrayType array)
    {
      Debug.Assert(false, "An array type exists only at runtime. It should be referred to, but never visited.");
    }
#endif
    public virtual void VisitAssembly(AssemblyNode assembly)
    {
      if (assembly == null) return;
      this.VisitModule(assembly);
      this.VisitAttributeList(assembly.ModuleAttributes);
      this.VisitSecurityAttributeList(assembly.SecurityAttributes);
    }
    public virtual AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference)
    {
      return assemblyReference;
    }
#if !MinimalReader
    public virtual void VisitAssertion(Assertion assertion)
    {
      if (assertion == null) return;
      this.VisitExpression(assertion.Condition);
    }
    public virtual void VisitAssumption(Assumption assumption)
    {
      if (assumption == null) return;
      this.VisitExpression(assumption.Condition);
    }
    public virtual void VisitAssignmentExpression(AssignmentExpression assignment)
    {
      if (assignment == null) return;
      this.Visit(assignment.AssignmentStatement);
    }
#endif
    public virtual void VisitAssignmentStatement(AssignmentStatement assignment)
    {
      if (assignment == null) return;
      this.VisitTargetExpression(assignment.Target);
      this.VisitExpression(assignment.Source);
    }
    public virtual void VisitAttributeConstructor(AttributeNode attribute)
    {
      if (attribute == null) return;
      this.VisitExpression(attribute.Constructor);
    }
    public virtual void VisitAttributeNode(AttributeNode attribute)
    {
      if (attribute == null) return;
      this.VisitAttributeConstructor(attribute);
      this.VisitExpressionList(attribute.Expressions);
    }
    public virtual void VisitAttributeList(AttributeList attributes)
    {
      if (attributes == null) return;
      for (int i = 0, n = attributes.Count; i < n; i++)
        this.VisitAttributeNode(attributes[i]);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitBase(Base Base)
    {
    }
#endif
    public virtual void VisitBinaryExpression(BinaryExpression binaryExpression)
    {
      if (binaryExpression == null) return;
      this.VisitExpression(binaryExpression.Operand1);
      this.VisitExpression(binaryExpression.Operand2);
    }
    public virtual void VisitBlock(Block block)
    {
      if (block == null) return;
      this.VisitStatementList(block.Statements);
    }
#if !MinimalReader
    public virtual void VisitBlockExpression(BlockExpression blockExpression)
    {
      if (blockExpression == null) return;
      this.VisitBlock(blockExpression.Block);
    }
#endif
    public virtual void VisitBlockList(BlockList blockList)
    {
      if (blockList == null) return;
      for (int i = 0, n = blockList.Count; i < n; i++)
        this.VisitBlock(blockList[i]);
    }
    public virtual void VisitBranch(Branch branch)
    {
      if (branch == null) return;
      this.VisitExpression(branch.Condition);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitCatch(Catch Catch)
    {
      if (Catch == null) return;
      this.VisitTargetExpression(Catch.Variable);
      this.VisitTypeReference(Catch.Type);
      this.VisitBlock(Catch.Block);
    }
    public virtual void VisitCatchList(CatchList catchers)
    {
      if (catchers == null) return;
      for (int i = 0, n = catchers.Count; i < n; i++)
        this.VisitCatch(catchers[i]);
    }
#endif
    public virtual void VisitClass(Class Class)
    {
      this.VisitTypeNode(Class);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitCoerceTuple(CoerceTuple coerceTuple)
    {
      if (coerceTuple == null) return;
      this.VisitExpression(coerceTuple.OriginalTuple);
      this.VisitConstructTuple(coerceTuple);
    }
    public virtual void VisitCollectionEnumerator(CollectionEnumerator ce)
    {
      if (ce == null) return;
      this.VisitExpression(ce.Collection);
    }
    public virtual void VisitCompilation(Compilation compilation)
    {
      if (compilation == null) return;
      Module module = compilation.TargetModule;
      if (module != null)
        this.VisitAttributeList(module.Attributes);
      AssemblyNode assem = module as AssemblyNode;
      if (assem != null)
       this.VisitAttributeList(assem.ModuleAttributes);
      this.VisitCompilationUnitList(compilation.CompilationUnits);
    }
    public virtual void VisitCompilationUnit(CompilationUnit cUnit)
    {
      if (cUnit == null) return;
      this.VisitNodeList(cUnit.Nodes);
    }
    public virtual void VisitNodeList(NodeList nodes)
    {
      if (nodes == null) return;
      for (int i = 0, n = nodes.Count; i < n; i++)
        this.Visit(nodes[i]);
    }
    public virtual void VisitCompilationUnitList(CompilationUnitList compilationUnits)
    {
      if (compilationUnits == null) return;
      for (int i = 0, n = compilationUnits.Count; i < n; i++)
        this.Visit(compilationUnits[i]);
    }
    public virtual void VisitCompilationUnitSnippet(CompilationUnitSnippet snippet)
    {
      this.VisitCompilationUnit(snippet);
    }
    public virtual void VisitComposition(Composition comp)
    {
      if (comp == null) return;
      if (comp.GetType() == typeof(Composition))
      {
        this.Visit(comp.Expression);
      }
    }
#endif
    public virtual void VisitConstruct(Construct cons)
    {
      if (cons == null) return;
      this.VisitExpression(cons.Constructor);
      this.VisitExpressionList(cons.Operands);
#if !MinimalReader && !CodeContracts
      this.VisitExpression(cons.Owner);
#endif
    }
    public virtual void VisitConstructArray(ConstructArray consArr)
    {
      if (consArr == null) return;
      this.VisitTypeReference(consArr.ElementType);
      this.VisitExpressionList(consArr.Operands);
#if !MinimalReader
      this.VisitExpressionList(consArr.Initializers);
      this.VisitExpression(consArr.Owner);
#endif
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitConstructDelegate(ConstructDelegate consDelegate)
    {
      if (consDelegate == null) return;
      this.VisitTypeReference(consDelegate.DelegateType);
      this.VisitExpression(consDelegate.TargetObject);
    }
    public virtual void VisitConstructFlexArray(ConstructFlexArray consArr)
    {
      if (consArr == null) return;
      this.VisitTypeReference(consArr.ElementType);
      this.VisitExpressionList(consArr.Operands);
      this.VisitExpressionList(consArr.Initializers);
    }
    public virtual Expression VisitConstructIterator(ConstructIterator consIterator)
    {
      return consIterator;
    }
    public virtual void VisitConstructTuple(ConstructTuple consTuple)
    {
      if (consTuple == null) return;
      this.VisitFieldList(consTuple.Fields);
    }
#endif
#if ExtendedRuntime
    public virtual TypeNode VisitConstrainedType(ConstrainedType cType){
      if (cType == null) return null;
      this.VisitTypeReference(cType.UnderlyingType);
      this.VisitExpression(cType.Constraint);
      return cType;
    }
#endif
#if !MinimalReader && !CodeContracts
    public virtual void VisitContinue(Continue Continue)
    {
    }
    public virtual void VisitCurrentClosure(CurrentClosure currentClosure)
    {
    }
#endif
    public virtual void VisitDelegateNode(DelegateNode delegateNode)
    {
      if (delegateNode == null) return;
      this.VisitTypeNode(delegateNode);
      this.VisitParameterList(delegateNode.Parameters);
      this.VisitTypeReference(delegateNode.ReturnType);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitDoWhile(DoWhile doWhile)
    {
      if (doWhile == null) return;
      this.VisitLoopInvariantList(doWhile.Invariants);
      this.VisitBlock(doWhile.Body);
      this.VisitExpression(doWhile.Condition);
    }
#endif
    public virtual void VisitEndFilter(EndFilter endFilter)
    {
      if (endFilter == null) return;
      this.VisitExpression(endFilter.Value);
    }
    public virtual Statement VisitEndFinally(EndFinally endFinally)
    {
      return endFinally;
    }
#if ExtendedRuntime || CodeContracts
    public virtual void VisitEnsuresList(EnsuresList Ensures)
    {
      if (Ensures == null) return;
      for (int i = 0, n = Ensures.Count; i < n; i++)
        this.Visit(Ensures[i]);
    }
#endif
    public virtual void VisitEnumNode(EnumNode enumNode)
    {
      this.VisitTypeNode(enumNode);
    }
    public virtual void VisitEvent(Event evnt)
    {
      if (evnt == null) return;
      this.VisitAttributeList(evnt.Attributes);
      this.VisitTypeReference(evnt.HandlerType);
    }
#if ExtendedRuntime || CodeContracts
    public virtual void VisitEnsuresExceptional(EnsuresExceptional exceptional)
    {
      if (exceptional == null) return;
      this.VisitExpression(exceptional.PostCondition);
      this.VisitTypeReference(exceptional.Type);
      this.VisitExpression(exceptional.Variable);
    }
#endif
#if !MinimalReader && !CodeContracts
    public virtual Statement VisitExit(Exit exit)
    {
      return exit;
    }
    public virtual void VisitExpose(Expose @expose)
    {
      if (@expose == null) return;
      this.VisitExpression(@expose.Instance);
      this.VisitBlock(expose.Body);
    }
#endif

    public virtual void VisitExpression(Expression expression)
    {
      if (expression == null) return;
      switch (expression.NodeType)
      {
        case NodeType.Dup:
        case NodeType.Arglist:
          return;
        case NodeType.Pop:
          UnaryExpression uex = expression as UnaryExpression;
          if (uex != null)
          {
            this.VisitExpression(uex.Operand);
            return;
          }
          return;
        default:
          this.Visit(expression);
          return;
      }
    }
    public void VisitExpressionList(ExpressionList expressions)
    {
      if (expressions == null) return;
      for (int i = 0, n = expressions.Count; i < n; i++)
       this.VisitExpression(expressions[i]);
    }
#if !MinimalReader && !CodeContracts
    public virtual Expression VisitExpressionSnippet(ExpressionSnippet snippet)
    {
      return snippet;
    }
#endif
    public virtual void VisitExpressionStatement(ExpressionStatement statement)
    {
      if (statement == null) return;
      this.VisitExpression(statement.Expression);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitFaultHandler(FaultHandler faultHandler)
    {
      if (faultHandler == null) return;
      this.VisitBlock(faultHandler.Block);
    }
    public virtual void VisitFaultHandlerList(FaultHandlerList faultHandlers)
    {
      if (faultHandlers == null) return;
      for (int i = 0, n = faultHandlers.Count; i < n; i++)
        this.VisitFaultHandler(faultHandlers[i]);
    }
#endif
    public virtual void VisitField(Field field)
    {
      if (field == null) return;
      this.VisitAttributeList(field.Attributes);
      this.VisitTypeReference(field.Type);
#if !MinimalReader && !CodeContracts
      this.VisitExpression(field.Initializer);
      this.VisitInterfaceReferenceList(field.ImplementedInterfaces);
#endif
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitFieldInitializerBlock(FieldInitializerBlock block)
    {
      if (block == null) return;
      this.VisitTypeReference(block.Type);
      this.VisitBlock(block);
    }
    public virtual void VisitFieldList(FieldList fields)
    {
      if (fields == null) return;
      for (int i = 0, n = fields.Count; i < n; i++)
        this.VisitField(fields[i]);
    }
    public virtual void VisitFilter(Filter filter)
    {
      if (filter == null) return;
      this.VisitExpression(filter.Expression);
      this.VisitBlock(filter.Block);
    }
    public virtual void VisitFilterList(FilterList filters)
    {
      if (filters == null) return;
      for (int i = 0, n = filters.Count; i < n; i++)
        this.VisitFilter(filters[i]);
    }
    public virtual void VisitFinally(Finally Finally)
    {
      if (Finally == null) return;
      this.VisitBlock(Finally.Block);
    }
    public virtual void VisitFixed(Fixed Fixed)
    {
      if (Fixed == null) return;
      this.Visit(Fixed.Declarators);
      this.VisitBlock(Fixed.Body);
    }
    public virtual void VisitFor(For For)
    {
      if (For == null) return;
      this.VisitStatementList(For.Initializer);
      this.VisitLoopInvariantList(For.Invariants);
      this.VisitExpression(For.Condition);
      this.VisitStatementList(For.Incrementer);
      this.VisitBlock(For.Body);
    }
    public virtual void VisitForEach(ForEach forEach)
    {
      if (forEach == null) return;
      this.VisitTypeReference(forEach.TargetVariableType);
      this.VisitTargetExpression(forEach.TargetVariable);
      this.VisitExpression(forEach.SourceEnumerable);
      this.VisitTargetExpression(forEach.InductionVariable);
      this.VisitLoopInvariantList(forEach.Invariants);
      this.VisitBlock(forEach.Body);
    }
    public virtual void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
    {
      if (functionDeclaration == null) return;
      this.VisitParameterList(functionDeclaration.Parameters);
      this.VisitTypeReference(functionDeclaration.ReturnType);
      this.VisitBlock(functionDeclaration.Body);
    }
    public virtual void VisitTemplateInstance(TemplateInstance templateInstance)
    {
      if (templateInstance == null) return;
      this.VisitExpression(templateInstance.Expression);
      this.VisitTypeReferenceList(templateInstance.TypeArguments);
    }
    public virtual void VisitStackAlloc(StackAlloc alloc)
    {
      if (alloc == null) return;
      this.VisitTypeReference(alloc.ElementType);
      this.VisitExpression(alloc.NumberOfElements);
    }
    public virtual void VisitGoto(Goto Goto)
    {
    }
    public virtual void VisitGotoCase(GotoCase gotoCase)
    {
      if (gotoCase == null) return;
      this.VisitExpression(gotoCase.CaseLabel);
    }
#endif
    public virtual void VisitIdentifier(Identifier identifier)
    {
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitIf(If If)
    {
      if (If == null) return;
      this.VisitExpression(If.Condition);
      this.VisitBlock(If.TrueBlock);
      this.VisitBlock(If.FalseBlock);
    }
    public virtual void VisitImplicitThis(ImplicitThis implicitThis)
    {
    }
#endif
    public virtual void VisitIndexer(Indexer indexer)
    {
      if (indexer == null) return;
      this.VisitExpression(indexer.Object);
      this.VisitExpressionList(indexer.Operands);
    }
    public virtual void VisitInterface(Interface Interface)
    {
      this.VisitTypeNode(Interface);
    }
    public virtual void VisitInterfaceReference(Interface Interface)
    {
      this.VisitTypeReference(Interface);
    }
    public virtual void VisitInterfaceReferenceList(InterfaceList interfaceReferences)
    {
      if (interfaceReferences == null) return;
      for (int i = 0, n = interfaceReferences.Count; i < n; i++)
        this.VisitInterfaceReference(interfaceReferences[i]);
    }
#if ExtendedRuntime || CodeContracts
    public virtual void VisitInvariant(Invariant @invariant)
    {
      if (@invariant == null) return;
      VisitExpression(@invariant.Condition);
    }
    public virtual void VisitInvariantList(InvariantList invariants)
    {
      if (invariants == null) return;
      for (int i = 0, n = invariants.Count; i < n; i++)
        this.VisitInvariant(invariants[i]);
    }
#endif
#if ExtendedRuntime
    public virtual void VisitModelfieldContract(ModelfieldContract mfC) {
      if (mfC == null) return;
      this.VisitExpression(mfC.Witness);
      for (int i = 0, n = mfC.SatisfiesList.Count; i < n; i++)
        this.VisitExpression(mfC.SatisfiesList[i]);
    }
    public virtual void VisitModelfieldContractList(ModelfieldContractList mfCs) {
      if (mfCs == null) return;
      for (int i = 0, n = mfCs.Count; i < n; i++)
        this.VisitModelfieldContract(mfCs[i]);
    }
#endif
    public virtual void VisitInstanceInitializer(InstanceInitializer cons)
    {
      this.VisitMethod(cons);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitLabeledStatement(LabeledStatement lStatement)
    {
      if (lStatement == null) return;
      this.Visit(lStatement.Statement);
    }
#endif
    public virtual void VisitLiteral(Literal literal)
    {
    }
    public virtual void VisitLocal(Local local)
    {
      if (local == null) return;
      this.VisitTypeReference(local.Type);
#if !MinimalReader && !CodeContracts
      LocalBinding lb = local as LocalBinding;
      if (lb != null)
      {
        this.VisitLocal(lb.BoundLocal);
      }
#endif
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations)
    {
      if (localDeclarations == null) return;
      this.VisitTypeReference(localDeclarations.Type);
      this.VisitLocalDeclarationList(localDeclarations.Declarations);
    }
    public virtual void VisitLocalDeclarationList(LocalDeclarationList localDeclarations)
    {
      if (localDeclarations == null) return;
      for (int i = 0, n = localDeclarations.Count; i < n; i++)
        this.VisitLocalDeclaration(localDeclarations[i]);
    }
    public virtual void VisitLocalDeclaration(LocalDeclaration localDeclaration)
    {
      if (localDeclaration == null) return;
      this.VisitExpression(localDeclaration.InitialValue);
    }
    public virtual void VisitLock(Lock Lock)
    {
      if (Lock == null) return;
      this.VisitExpression(Lock.Guard);
      this.VisitBlock(Lock.Body);
    }
    public virtual void VisitLRExpression(LRExpression expr)
    {
      if (expr == null) return;
      this.VisitExpression(expr.Expression);
    }
#endif
    public virtual void VisitMemberBinding(MemberBinding memberBinding)
    {
      if (memberBinding == null) return;
      this.VisitExpression(memberBinding.TargetObject);
    }
    public virtual void VisitMemberList(MemberList members)
    {
      if (members == null) return;
      for (int i = 0, n = members.Count; i < n; i++)
      {
        this.Visit(members[i]);
      }
    }
    public virtual void VisitMethod(Method method)
    {
      if (method == null) return;
      this.VisitAttributeList(method.Attributes);
      this.VisitAttributeList(method.ReturnAttributes);
      this.VisitSecurityAttributeList(method.SecurityAttributes);
      this.VisitTypeReference(method.ReturnType);
#if !MinimalReader && !CodeContracts
      this.VisitTypeReferenceList(method.ImplementedTypes);
#endif
      this.VisitParameterList(method.Parameters);
      if (TargetPlatform.UseGenerics)
      {
        this.VisitTypeReferenceList(method.TemplateArguments);
        this.VisitTypeParameterList(method.TemplateParameters);
      }
#if ExtendedRuntime || CodeContracts
      this.VisitMethodContract(method.Contract);
#endif
      this.VisitBlock(method.Body);
    }
    public virtual void VisitMethodCall(MethodCall call)
    {
      if (call == null) return;
      this.VisitExpression(call.Callee);
      this.VisitExpressionList(call.Operands);
      this.VisitTypeReference(call.Constraint);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitArglistArgumentExpression(ArglistArgumentExpression argexp)
    {
      if (argexp == null) return;
      this.VisitExpressionList(argexp.Operands);
    }
    public virtual void VisitArglistExpression(ArglistExpression argexp)
    {
    }
#endif
#if ExtendedRuntime || CodeContracts
    public virtual void VisitMethodContract(MethodContract contract)
    {
      if (contract == null) return;
      // don't visit contract.DeclaringMethod
      // don't visit contract.OverriddenMethods
      this.VisitRequiresList(contract.Requires);
      this.VisitEnsuresList(contract.Ensures);
      this.VisitEnsuresList(contract.ModelEnsures);
      this.VisitExpressionList(contract.Modifies);
      this.VisitEnsuresList(contract.AsyncEnsures);
    }
#endif
    public virtual void VisitModule(Module module)
    {
      if (module == null) return;
      this.VisitAttributeList(module.Attributes);
      this.VisitTypeNodeList(module.Types);
    }
    public virtual void VisitModuleReference(ModuleReference moduleReference)
    {
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitNameBinding(NameBinding nameBinding)
    {
    }
#endif
    public virtual void VisitNamedArgument(NamedArgument namedArgument)
    {
      if (namedArgument == null) return;
      this.VisitExpression(namedArgument.Value);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitNamespace(Namespace nspace)
    {
      if (nspace == null) return;
      this.VisitAliasDefinitionList(nspace.AliasDefinitions);
      this.VisitUsedNamespaceList(nspace.UsedNamespaces);
      this.VisitAttributeList(nspace.Attributes);
      this.VisitTypeNodeList(nspace.Types);
      this.VisitNamespaceList(nspace.NestedNamespaces);
    }
    public virtual void VisitNamespaceList(NamespaceList namespaces)
    {
      if (namespaces == null) return;
      for (int i = 0, n = namespaces.Count; i < n; i++)
        this.VisitNamespace(namespaces[i]);
    }
#endif
#if ExtendedRuntime || CodeContracts
    public virtual void VisitEnsuresNormal(EnsuresNormal normal)
    {
      if (normal == null) return;
      this.VisitExpression(normal.PostCondition);
    }
    public virtual void VisitOldExpression(OldExpression oldExpression)
    {
      if (oldExpression == null) return;
      this.VisitExpression(oldExpression.expression);
    }
    public virtual void VisitReturnValue(ReturnValue returnValue)
    {
      if (returnValue == null) return;
      this.VisitTypeReference(returnValue.Type);
    }
    public virtual void VisitRequiresOtherwise(RequiresOtherwise otherwise)
    {
      if (otherwise == null) return;
      this.VisitExpression(otherwise.Condition);
      this.VisitExpression(otherwise.ThrowException);
    }
    public virtual void VisitRequiresPlain(RequiresPlain plain)
    {
      if (plain == null) return;
      this.VisitExpression(plain.Condition);
    }
#endif
    public virtual void VisitParameter(Parameter parameter)
    {
      if (parameter == null) return;
      this.VisitAttributeList(parameter.Attributes);
      this.VisitTypeReference(parameter.Type);
      this.VisitExpression(parameter.DefaultValue);
#if !MinimalReader && !CodeContracts
      ParameterBinding pb = parameter as ParameterBinding;
      if (pb != null)
      {
        this.VisitParameter(pb.BoundParameter);
      }
#endif
    }
    public virtual void VisitParameterList(ParameterList parameterList)
    {
      if (parameterList == null) return;
      for (int i = 0, n = parameterList.Count; i < n; i++)
        this.VisitParameter(parameterList[i]);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitPrefixExpression(PrefixExpression pExpr)
    {
      if (pExpr == null) return;
      this.VisitExpression(pExpr.Expression);
    }
    public virtual void VisitPostfixExpression(PostfixExpression pExpr)
    {
      if (pExpr == null) return;
      this.VisitExpression(pExpr.Expression);
    }
#endif
    public virtual void VisitProperty(Property property)
    {
      if (property == null) return;
      this.VisitAttributeList(property.Attributes);
      this.VisitParameterList(property.Parameters);
      this.VisitTypeReference(property.Type);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitQuantifier(Quantifier quantifier)
    {
      if (quantifier == null) return;
      this.VisitComprehension(quantifier.Comprehension);
    }
    public virtual void VisitComprehension(Comprehension comprehension)
    {
      if (comprehension == null) return;
      this.VisitExpressionList(comprehension.BindingsAndFilters);
      this.VisitExpressionList(comprehension.Elements);
    }
    public virtual void VisitComprehensionBinding(ComprehensionBinding comprehensionBinding)
    {
      if (comprehensionBinding == null) return;
      this.VisitTypeReference(comprehensionBinding.TargetVariableType);
      this.VisitTargetExpression(comprehensionBinding.TargetVariable);
      this.VisitTypeReference(comprehensionBinding.AsTargetVariableType);
      this.VisitExpression(comprehensionBinding.SourceEnumerable);
    }
    public virtual void VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier)
    {
      if (qualifiedIdentifier == null) return;
      this.VisitExpression(qualifiedIdentifier.Qualifier);
    }
    public virtual void VisitRefValueExpression(RefValueExpression refvalexp)
    {
      if (refvalexp == null) return;
      this.VisitExpression(refvalexp.Operand1);
      this.VisitExpression(refvalexp.Operand2);
    }
    public virtual void VisitRefTypeExpression(RefTypeExpression reftypexp)
    {
      if (reftypexp == null) return;
      this.VisitExpression(reftypexp.Operand);
    }
    public virtual void VisitRepeat(Repeat repeat)
    {
      if (repeat == null) return;
      this.VisitBlock(repeat.Body);
      this.VisitExpression(repeat.Condition);
    }
#endif
#if ExtendedRuntime || CodeContracts
    public virtual void VisitRequiresList(RequiresList Requires)
    {
      if (Requires == null) return;
      for (int i = 0, n = Requires.Count; i < n; i++)
        this.Visit(Requires[i]);
    }
#endif
    public virtual void VisitReturn(Return Return)
    {
      if (Return == null) return;
      this.VisitExpression(Return.Expression);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitAcquire(Acquire @acquire)
    {
      if (@acquire == null) return;
      this.Visit(@acquire.Target);
      this.VisitExpression(@acquire.Condition);
      this.VisitExpression(@acquire.ConditionFunction);
      this.VisitBlock(@acquire.Body);
    }
    public virtual void VisitResourceUse(ResourceUse resourceUse)
    {
      if (resourceUse == null) return;
      this.Visit(resourceUse.ResourceAcquisition);
      this.VisitBlock(resourceUse.Body);
    }
#endif
    public virtual void VisitSecurityAttribute(SecurityAttribute attribute)
    {
    }
    public virtual void VisitSecurityAttributeList(SecurityAttributeList attributes)
    {
      if (attributes == null) return;
      for (int i = 0, n = attributes.Count; i < n; i++)
        this.VisitSecurityAttribute(attributes[i]);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitSetterValue(SetterValue value)
    {
    }
#endif
    public virtual void VisitStatementList(StatementList statements)
    {
      if (statements == null) return;
      for (int i = 0, n = statements.Count; i < n; i++)
        this.Visit(statements[i]);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitStatementSnippet(StatementSnippet snippet)
    {
    }
#endif
    public virtual void VisitStaticInitializer(StaticInitializer cons)
    {
      this.VisitMethod(cons);
    }
    public virtual void VisitStruct(Struct Struct)
    {
      this.VisitTypeNode(Struct);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitSwitch(Switch Switch)
    {
      if (Switch == null) return;
      this.VisitExpression(Switch.Expression);
      this.VisitSwitchCaseList(Switch.Cases);
    }
    public virtual void VisitSwitchCase(SwitchCase switchCase)
    {
      if (switchCase == null) return;
      this.VisitExpression(switchCase.Label);
      this.VisitBlock(switchCase.Body);
    }
    public virtual void VisitSwitchCaseList(SwitchCaseList switchCases)
    {
      if (switchCases == null) return;
      for (int i = 0, n = switchCases.Count; i < n; i++)
        this.Visit(switchCases[i]);
    }
#endif
    public virtual void VisitSwitchInstruction(SwitchInstruction switchInstruction)
    {
      if (switchInstruction == null) return;
      this.VisitExpression(switchInstruction.Expression);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitTypeswitch(Typeswitch Typeswitch)
    {
      if (Typeswitch == null) return;
      this.VisitExpression(Typeswitch.Expression);
      this.VisitTypeswitchCaseList(Typeswitch.Cases);
    }
    public virtual void VisitTypeswitchCase(TypeswitchCase typeswitchCase)
    {
      if (typeswitchCase == null) return;
      this.VisitTypeReference(typeswitchCase.LabelType);
      this.VisitTargetExpression(typeswitchCase.LabelVariable);
      this.VisitBlock(typeswitchCase.Body);
    }
    public virtual void VisitTypeswitchCaseList(TypeswitchCaseList typeswitchCases)
    {
      if (typeswitchCases == null) return;
      for (int i = 0, n = typeswitchCases.Count; i < n; i++)
        this.VisitTypeswitchCase(typeswitchCases[i]);
    }
#endif
    public virtual void VisitTargetExpression(Expression expression)
    {
      this.VisitExpression(expression);
    }
    public virtual void VisitTernaryExpression(TernaryExpression expression)
    {
      if (expression == null) return;
      this.VisitExpression(expression.Operand1);
      this.VisitExpression(expression.Operand2);
      this.VisitExpression(expression.Operand3);
    }
    public virtual void VisitThis(This This)
    {
      if (This == null) return;
      this.VisitTypeReference(This.Type);
#if !MinimalReader && !CodeContracts
      ThisBinding tb = This as ThisBinding;
      if (tb != null)
      {
        this.VisitThis(tb.BoundThis);
      }
#endif
    }
    public virtual void VisitThrow(Throw Throw)
    {
      if (Throw == null) return;
      this.VisitExpression(Throw.Expression);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitTry(Try Try)
    {
      if (Try == null) return;
      this.VisitBlock(Try.TryBlock);
      this.VisitCatchList(Try.Catchers);
      this.VisitFilterList(Try.Filters);
      this.VisitFaultHandlerList(Try.FaultHandlers);
      this.VisitFinally(Try.Finally);
    }
#endif
#if ExtendedRuntime    
    public virtual void VisitTupleType(TupleType tuple){
      this.VisitTypeNode(tuple);
    }
    public virtual void VisitTypeAlias(TypeAlias tAlias){
      if (tAlias == null) return;
      if (tAlias.AliasedType is ConstrainedType)
        //The type alias defines the constrained type, rather than just referencing it
        this.VisitConstrainedType((ConstrainedType)tAlias.AliasedType);
      else
        this.VisitTypeReference(tAlias.AliasedType);
    }

    public virtual void VisitTypeIntersection(TypeIntersection typeIntersection){
      this.VisitTypeNode(typeIntersection);
    }
#endif
#if ExtendedRuntime || CodeContracts
    public virtual void VisitTypeContract(TypeContract contract)
    {
      if (contract == null) return;
      // don't visit contract.DeclaringType
      // don't visit contract.InheritedContracts
      this.VisitInvariantList(contract.Invariants);
#if ExtendedRuntime
      this.VisitModelfieldContractList(contract.ModelfieldContracts);
#endif
    }
#endif
#if !MinimalReader && !CodeContracts
    public virtual void VisitTypeMemberSnippet(TypeMemberSnippet snippet)
    {
    }
#endif
    public virtual void VisitTypeModifier(TypeModifier typeModifier)
    {
      if (typeModifier == null) return;
      this.VisitTypeReference(typeModifier.Modifier);
      this.VisitTypeReference(typeModifier.ModifiedType);
    }
    public virtual void VisitTypeNode(TypeNode typeNode)
    {
      if (typeNode == null) return;
      this.VisitAttributeList(typeNode.Attributes);
      this.VisitSecurityAttributeList(typeNode.SecurityAttributes);
      Class c = typeNode as Class;
      if (c != null) this.VisitTypeReference(c.BaseClass);
      this.VisitInterfaceReferenceList(typeNode.Interfaces);
      this.VisitTypeReferenceList(typeNode.TemplateArguments);
      this.VisitTypeParameterList(typeNode.TemplateParameters);
      this.VisitMemberList(typeNode.Members);
#if ExtendedRuntime || CodeContracts
      // have to visit this *after* visiting the members since in Normalizer
      // it creates normalized method bodies for the invariant methods and
      // those shouldn't be visited again!!
      // REVIEW!! I don't think the method bodies created in Normalizer are necessarily normalized anymore!!
      this.VisitTypeContract(typeNode.Contract);
#endif
    }
    public virtual void VisitTypeNodeList(TypeNodeList types)
    {
      if (types == null) return;
      for (int i = 0; i < types.Count; i++) //Visiting a type may result in a new type being appended to this list
        this.Visit(types[i]);
    }
    public virtual void VisitTypeParameter(TypeNode typeParameter)
    {
      if (typeParameter == null) return;
      Class cl = typeParameter as Class;
      if (cl != null) this.VisitTypeReference(cl.BaseClass);
      this.VisitAttributeList(typeParameter.Attributes);
      this.VisitInterfaceReferenceList(typeParameter.Interfaces);
    }
    public virtual void VisitTypeParameterList(TypeNodeList typeParameters)
    {
      if (typeParameters == null) return;
      for (int i = 0, n = typeParameters.Count; i < n; i++)
        this.VisitTypeParameter(typeParameters[i]);
    }
    public virtual void VisitTypeReference(TypeNode type)
    {
    }
#if !MinimalReader
    public virtual void VisitTypeReference(TypeReference type)
    {
    }
#endif
    public virtual void VisitTypeReferenceList(TypeNodeList typeReferences)
    {
      if (typeReferences == null) return;
      for (int i = 0, n = typeReferences.Count; i < n; i++)
        this.VisitTypeReference(typeReferences[i]);
    }
#if ExtendedRuntime 
    public virtual void VisitTypeUnion(TypeUnion typeUnion){
      this.VisitTypeNode(typeUnion);
    }
#endif
    public virtual void VisitUnaryExpression(UnaryExpression unaryExpression)
    {
      if (unaryExpression == null) return;
      this.VisitExpression(unaryExpression.Operand);
    }
#if !MinimalReader && !CodeContracts
    public virtual void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
    {
      if (variableDeclaration == null) return;
      this.VisitTypeReference(variableDeclaration.Type);
      this.VisitExpression(variableDeclaration.Initializer);
    }
    public virtual void VisitUsedNamespace(UsedNamespace usedNamespace)
    {
    }
    public virtual void VisitUsedNamespaceList(UsedNamespaceList usedNspaces)
    {
      if (usedNspaces == null) return;
      for (int i = 0, n = usedNspaces.Count; i < n; i++)
        this.VisitUsedNamespace(usedNspaces[i]);
    }
    public virtual void VisitLoopInvariantList(ExpressionList expressions)
    {
      if (expressions == null) return;
      for (int i = 0, n = expressions.Count; i < n; i++)
        this.VisitExpression(expressions[i]);
    }
    public virtual void VisitWhile(While While)
    {
      if (While == null) return;
      this.VisitExpression(While.Condition);
      this.VisitLoopInvariantList(While.Invariants);
      this.VisitBlock(While.Body);
    }
    public virtual void VisitYield(Yield Yield)
    {
      if (Yield == null) return;
      this.VisitExpression(Yield.Expression);
    }
#endif
#if ExtendedRuntime
    // query nodes
    public virtual void VisitQueryAggregate(QueryAggregate qa){
      if (qa == null) return;
      this.VisitExpression(qa.Expression);
    }
    public virtual void VisitQueryAlias(QueryAlias alias){
      if (alias == null) return;
      this.VisitExpression(alias.Expression);
    }
    public virtual void VisitQueryAxis(QueryAxis axis){
      if (axis == null) return;
      this.VisitExpression( axis.Source );
    }
    public virtual void VisitQueryCommit(QueryCommit qc){
    }
    public virtual void VisitQueryContext(QueryContext context){
    }
    public virtual void VisitQueryDelete(QueryDelete delete){
      if (delete == null) return;
      this.VisitExpression(delete.Source);
      /*delete.Target =*/ this.VisitExpression(delete.Target); //REVIEW: why should this not be updated?
    }
    public virtual void VisitQueryDifference(QueryDifference diff){
      if (diff == null) return;
      this.VisitExpression(diff.LeftSource);
      this.VisitExpression(diff.RightSource);
    }
    public virtual void VisitQueryDistinct(QueryDistinct distinct){
      if (distinct == null) return;
      this.VisitExpression(distinct.Source);
    }
    public virtual void VisitQueryExists(QueryExists exists){
      if (exists == null) return;
      this.VisitExpression(exists.Source);
    }
    public virtual void VisitQueryFilter(QueryFilter filter){
      if (filter == null) return;
      this.VisitExpression(filter.Source);
      this.VisitExpression(filter.Expression);
    }
    public virtual void VisitQueryGroupBy(QueryGroupBy groupby){
      if (groupby == null) return;
      this.VisitExpression(groupby.Source);
      this.VisitExpressionList(groupby.GroupList);
      this.VisitExpression(groupby.Having);
    }
    public virtual void VisitQueryGeneratedType(QueryGeneratedType qgt){
    }
    public virtual void VisitQueryInsert(QueryInsert insert){
      if (insert == null) return;
      this.VisitExpression(insert.Location);
      this.VisitExpressionList(insert.HintList);
      this.VisitExpressionList(insert.InsertList);
    }
    public virtual void VisitQueryIntersection(QueryIntersection intersection){
      if (intersection == null) return;
      this.VisitExpression(intersection.LeftSource);
      this.VisitExpression(intersection.RightSource);
    }
    public virtual void VisitQueryIterator(QueryIterator xiterator){
      if (xiterator == null) return;
      this.VisitExpression(xiterator.Expression);
      this.VisitExpressionList(xiterator.HintList);
    }
    public virtual void VisitQueryJoin(QueryJoin join){
      if (join == null) return;
      this.VisitExpression(join.LeftOperand);
      this.VisitExpression(join.RightOperand);
      this.VisitExpression(join.JoinExpression);
    }
    public virtual void VisitQueryLimit(QueryLimit limit){
      if (limit == null) return;
      this.VisitExpression(limit.Source);
      this.VisitExpression(limit.Expression);
    }
    public virtual void VisitQueryOrderBy(QueryOrderBy orderby){
      if (orderby == null) return;
      this.VisitExpression(orderby.Source);
      this.VisitExpressionList(orderby.OrderList);
    }
    public virtual void VisitQueryOrderItem(QueryOrderItem item){
      if (item == null) return;
      this.VisitExpression(item.Expression);
    }
    public virtual void VisitQueryPosition(QueryPosition position){
    }
    public virtual void VisitQueryProject(QueryProject project){
      if (project == null) return;
      this.VisitExpression(project.Source);
      this.VisitExpressionList(project.ProjectionList);
    }
    public virtual void VisitQueryRollback(QueryRollback qr){
    }
    public virtual void VisitQueryQuantifier(QueryQuantifier qq){
      if (qq == null) return;
      this.VisitExpression(qq.Expression);
    }
    public virtual void VisitQueryQuantifiedExpression(QueryQuantifiedExpression qqe){
      if (qqe == null) return;
      this.VisitExpression(qqe.Expression);
    }
    public virtual void VisitQuerySelect(QuerySelect select){
      if (select == null) return;
      this.VisitExpression(select.Source);
    }
    public virtual void VisitQuerySingleton(QuerySingleton singleton){
      if (singleton == null) return;
      this.VisitExpression(singleton.Source);
    }
    public virtual void VisitQueryTransact(QueryTransact qt){
      if (qt == null) return;
      this.VisitExpression(qt.Source);
      this.VisitBlock(qt.Body);
      this.VisitBlock(qt.CommitBody);
      this.VisitBlock(qt.RollbackBody);
    }
    public virtual void VisitQueryTypeFilter(QueryTypeFilter filter){
      if (filter == null) return;
      this.VisitExpression(filter.Source);
    }
    public virtual void VisitQueryUnion(QueryUnion union){
      if (union == null) return;
      this.VisitExpression(union.LeftSource);
      this.VisitExpression(union.RightSource);
    }
    public virtual void VisitQueryUpdate(QueryUpdate update){
      if (update == null) return;
      this.VisitExpression(update.Source);
      this.VisitExpressionList(update.UpdateList);
    }    
    public virtual void VisitQueryYielder(QueryYielder yielder){
      if (yielder == null) return;
      this.VisitExpression(yielder.Source);
      this.VisitExpression(yielder.Target);
      this.VisitBlock(yielder.Body);
    }
#endif
#if !MinimalReader && !CodeContracts
    /// <summary>
    /// Return a type viewer for the current scope.
    /// [The type viewer acts like the identity function, except for dialects (e.g. Extensible Sing#)
    /// that allow extensions and differing views of types.]
    /// null can be returned to represent an identity-function type viewer.
    /// </summary>
    public virtual TypeViewer TypeViewer
    {
      get
      {
        return null;
      }
    }
    /// <summary>
    /// Return the current scope's view of the argument type, by asking the current scope's type viewer.
    /// </summary>
    public virtual TypeNode/*!*/ GetTypeView(TypeNode/*!*/ type)
    {
      return TypeViewer.GetTypeView(this.TypeViewer, type);
    }
#endif
  }

}