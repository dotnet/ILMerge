using System;
using System.Collections;
using System.Diagnostics;
#if CodeContracts
using System.Diagnostics.Contracts;
#endif
#if FxCop
using AttributeList = Microsoft.Cci.AttributeNodeCollection;
using BlockList = Microsoft.Cci.BlockCollection;
using ExpressionList = Microsoft.Cci.ExpressionCollection;
using InstructionList = Microsoft.Cci.InstructionCollection;
using InterfaceList = Microsoft.Cci.InterfaceCollection;
using MemberList = Microsoft.Cci.MemberCollection;
using MethodList = Microsoft.Cci.MethodCollection;
using ModuleReferenceList = Microsoft.Cci.ModuleReferenceCollection;
using NamespaceList = Microsoft.Cci.NamespaceCollection;
using NodeList = Microsoft.Cci.NodeCollection;
using ParameterList = Microsoft.Cci.ParameterCollection;
using SecurityAttributeList = Microsoft.Cci.SecurityAttributeCollection;
using StatementList = Microsoft.Cci.StatementCollection;
using TypeNodeList = Microsoft.Cci.TypeNodeCollection;
using Property = Microsoft.Cci.PropertyNode;
using Module = Microsoft.Cci.ModuleNode;
using Return = Microsoft.Cci.ReturnNode;
using Class = Microsoft.Cci.ClassNode;
using Event = Microsoft.Cci.EventNode;
using Throw = Microsoft.Cci.ThrowNode;
#endif
#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler{
#endif
  /* The idea here is to do a tree traversal of the IR graph, rewriting the IR with duplicate nodes from the bottom up. Nodes that may appear
   * more than once in the graph keep track of their duplicates in the DuplicateFor hashtable and all references to these nodes are replaced
   * with references to the corresponding duplicates.
   * 
   * A complication arises because of the need to duplicate IR subgraphs, such as Methods, Types, CompilationUnits and individual Modules.
   * The subgraphs contain references to "foreign" nodes that should not be duplicated and it is thus necessary to be able to tell whether
   * or not a node should be duplicated. This done by tracking all the types that are members of the subgraph to be duplicated in the
   * TypesToBeDuplicated hashtable. Types are duplicated only if they are members of this table, while fields and methods are duplicated
   * only if their declaring types are members of this table. 
   * 
   * Since every type contains a reference to its declaring module, the module in which duplicated types will be inserted must be specified
   * to the constructor.
   * */

  /// <summary>
  /// Walks an IR, duplicating it while fixing up self references to point to the duplicate IR. Only good for one duplication. 
  /// Largest unit of duplication is a single module.
  /// </summary>
#if !FxCop 
  public
#endif
  class Duplicator : StandardVisitor{
    public TrivialHashtable/*!*/ DuplicateFor { get; private set; }
    public TrivialHashtable/*!*/ TypesToBeDuplicated { get; private set; }
    public Module/*!*/ TargetModule;
    public TypeNode TargetType;
    public Method TargetMethod;
    public TypeNode OriginalTargetType;
    public bool SkipBodies;
    public bool RecordOriginalAsTemplate;
#if !NoXml    
    public bool CopyDocumentation;
#endif

#if CodeContracts
    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(TypesToBeDuplicated != null);
      Contract.Invariant(DuplicateFor != null);
    }

#endif

    /// <param name="module">The module into which the duplicate IR will be grafted.</param>
    /// <param name="type">The type into which the duplicate Member will be grafted. Ignored if entire type, or larger unit is duplicated.</param>
    public Duplicator(Module/*!*/ module, TypeNode type)
      : this(module, type, 4)
    {
      
    }
    /// <param name="module">The module into which the duplicate IR will be grafted.</param>
    /// <param name="type">The type into which the duplicate Member will be grafted. Ignored if entire type, or larger unit is duplicated.</param>
    /// <param name="initialCapacity">initial capacity of dup forwarding table. Default 4</param>
    public Duplicator(Module/*!*/ module, TypeNode type, int initialCapacity)
    {
      this.TargetModule = module;
      this.TargetType = this.OriginalTargetType = type;
      this.DuplicateFor = new TrivialHashtable(initialCapacity);
      this.TypesToBeDuplicated = new TrivialHashtable();
      //^ base();
    }
#if !MinimalReader && !CodeContracts
    public Duplicator(Visitor/*!*/ callingVisitor)
      : base(callingVisitor){
      /*^
      //Dummy initializations to satisfy compiler.
      Duplicator cdv = callingVisitor as Duplicator;
      if (cdv == null) {
        this.DuplicateFor = new TrivialHashtable();
        this.TypesToBeDuplicated = new TrivialHashtable();
        this.TargetModule = new Module();
        this.TargetType = this.OriginalTargetType = new Class();
      } else {
        this.DuplicateFor = cdv.DuplicateFor;
        this.TypesToBeDuplicated = cdv.TypesToBeDuplicated;
        this.TargetModule = cdv.TargetModule;
        this.TargetType = cdv.TargetType;
        this.OriginalTargetType = cdv.OriginalTargetType;
      }
      base; //The real initializations happen here
      ^*/
    }
    public override void TransferStateTo(Visitor targetVisitor){
      base.TransferStateTo(targetVisitor);
      Duplicator target = targetVisitor as Duplicator;
      if (target == null) return;
      target.DuplicateFor = this.DuplicateFor;
      target.OriginalTargetType = this.OriginalTargetType;
      target.RecordOriginalAsTemplate = this.RecordOriginalAsTemplate;
      target.SkipBodies = this.SkipBodies;
      target.TargetMethod = this.TargetMethod;
      target.TargetModule = this.TargetModule;
      target.TargetType = this.TargetType;
      target.TypesToBeDuplicated = this.TypesToBeDuplicated;
    }
    public virtual void FindTypesToBeDuplicated(Node node){
    }
    public virtual void FindTypesToBeDuplicated(NodeList nodes){
      if (nodes == null) return;
      for (int i = 0, n = nodes.Count; i < n; i++){
        Node node = nodes[i];
        if (node == null) continue;
        if (node is Namespace) this.FindTypesToBeDuplicated((Namespace)node);
        else this.FindTypesToBeDuplicated(node);
      }
    }
    public virtual void FindTypesToBeDuplicated(Namespace nspace){
      if (nspace == null) return;
      this.FindTypesToBeDuplicated(nspace.Types);
      this.FindTypesToBeDuplicated(nspace.NestedNamespaces);
    }
#endif
    public virtual void FindTypesToBeDuplicated(NamespaceList namespaces){
      if (namespaces == null) return;
      for (int i = 0, n = namespaces.Count; i < n; i++){
        Namespace nspace = namespaces[i];
        if (nspace == null) continue;
        this.FindTypesToBeDuplicated(nspace.Types);
#if !MinimalReader && !CodeContracts
        this.FindTypesToBeDuplicated(nspace.NestedNamespaces);
#endif
      }
    }
    public virtual void FindTypesToBeDuplicated(TypeNodeList types){
      if (types == null) return;
      for (int i = 0, n = types.Count; i < n; i++){
        TypeNode t = types[i];
        RegisterTypeToBeDuplicated(t);
      }
    }
    bool RegisterTypeToBeDuplicated(TypeNode t)
    {
      if (t == null) return false;
      if (this.DuplicateFor[t.UniqueKey] != null) return false;
      if (this.TypesToBeDuplicated[t.UniqueKey] != null) return false;
      Debug.Assert(TypeNode.IsCompleteTemplate(t));

      this.TypesToBeDuplicated[t.UniqueKey] = t;
      // dup the type now and fill it in later
      var dup = this.VisitTypeNode(t, null, null, null, true);
      var savedTargetType = this.TargetType;
      this.TargetType = dup;
      this.FindTypesToBeDuplicated(t.TemplateParameters);
      this.FindMembersToBeDuplicated(t.Members);
      this.TargetType = savedTargetType;
      return true;
    }
    bool RegisterMemberToBeDuplicated(Member m)
    {
      if (m == null) return false;
      TypeNode nested = m as TypeNode;
      if (nested != null) return RegisterTypeToBeDuplicated(nested);

      return false;
    }
    public virtual void FindMembersToBeDuplicated(MemberList members)
    {
      if (members == null) return;
      for (int i = 0; i < members.Count; i++)
      {
        RegisterMemberToBeDuplicated(members[i]);
      }
    }
    public override Node Visit(Node node)
    {
      node = base.Visit(node);
      Expression e = node as Expression;
      if (e != null) e.Type = this.VisitTypeReference(e.Type);
      return node;
    }
    public override Expression VisitAddressDereference(AddressDereference addr){
      if (addr == null) return null;
      return base.VisitAddressDereference((AddressDereference)addr.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override AliasDefinition VisitAliasDefinition(AliasDefinition aliasDefinition){
      if (aliasDefinition == null) return null;
      return base.VisitAliasDefinition((AliasDefinition)aliasDefinition.Clone());
    }
    public override AliasDefinitionList VisitAliasDefinitionList(AliasDefinitionList aliasDefinitions){
      if (aliasDefinitions == null) return null;
      return base.VisitAliasDefinitionList(aliasDefinitions.Clone());
    }
    public override Expression VisitAnonymousNestedFunction(AnonymousNestedFunction func){
      if (func == null) return null;
      AnonymousNestedFunction dup = (AnonymousNestedFunction)func.Clone();
      if (func.Method != null){
        dup.Method = this.VisitMethod(func.Method);
        //^ assume dup.Method != null;
        dup.Parameters = dup.Method.Parameters;
        dup.Body = dup.Method.Body;
        return dup;
      }
      return base.VisitAnonymousNestedFunction(dup);
    }    
    public override Expression VisitApplyToAll(ApplyToAll applyToAll){
      if (applyToAll == null) return null;
      return base.VisitApplyToAll((ApplyToAll)applyToAll.Clone());
    }
#endif
    public override AssemblyNode VisitAssembly(AssemblyNode assembly){
      if (assembly == null) return null;
      this.FindTypesToBeDuplicated(assembly.Types);
      return base.VisitAssembly((AssemblyNode)assembly.Clone());
    }
    public override AssemblyReference VisitAssemblyReference(AssemblyReference assemblyReference){
      if (assemblyReference == null) return null;
      return base.VisitAssemblyReference((AssemblyReference)assemblyReference.Clone());
    }
#if !MinimalReader
    public override Statement VisitAssertion(Assertion assertion){
      if (assertion == null) return null;
      return base.VisitAssertion((Assertion)assertion.Clone());
    }
    public override Statement VisitAssumption(Assumption Assumption){
      if (Assumption == null) return null;
      return base.VisitAssumption((Assumption)Assumption.Clone());
    }
    public override Expression VisitAssignmentExpression(AssignmentExpression assignment){
      if (assignment == null) return null;
      return base.VisitAssignmentExpression((AssignmentExpression)assignment.Clone());
    }
#endif    
    public override Statement VisitAssignmentStatement(AssignmentStatement assignment){
      if (assignment == null) return null;
      return base.VisitAssignmentStatement((AssignmentStatement)assignment.Clone());
    }
    public override Expression VisitAttributeConstructor(AttributeNode attribute){
      if (attribute == null || attribute.Constructor == null) return null;
      return this.VisitExpression((Expression)attribute.Constructor.Clone());
    }
    public override AttributeNode VisitAttributeNode(AttributeNode attribute){
      if (attribute == null) return null;
      return base.VisitAttributeNode((AttributeNode)attribute.Clone());
    }
    public override AttributeList VisitAttributeList(AttributeList attributes){
#if CodeContracts
      Contract.Ensures(Contract.Result<AttributeList>() != null || attributes == null);
#endif
      if (attributes == null) return null;
      return base.VisitAttributeList(attributes.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Expression VisitBase(Base Base){
      if (Base == null) return null;
      return base.VisitBase((Base)Base.Clone());
    }
#endif
    public override Expression VisitBinaryExpression(BinaryExpression binaryExpression){
      if (binaryExpression == null) return null;
      binaryExpression = (BinaryExpression)base.VisitBinaryExpression((BinaryExpression)binaryExpression.Clone());
      return binaryExpression;
    }
    public override Block VisitBlock(Block block){
      if (block == null) return null;
      Block dup = (Block)this.DuplicateFor[block.UniqueKey];
      if (dup != null) return dup;     
      this.DuplicateFor[block.UniqueKey] = dup = (Block)block.Clone();
      return base.VisitBlock(dup);
    }
#if !MinimalReader
    public override Expression VisitBlockExpression(BlockExpression blockExpression){
      if (blockExpression == null) return null;
      return base.VisitBlockExpression((BlockExpression)blockExpression.Clone());
    }
#endif
    public override BlockList VisitBlockList(BlockList blockList){
      if (blockList == null) return null;
      return base.VisitBlockList(blockList.Clone());
    }
    public override Statement VisitBranch(Branch branch){
      if (branch == null) return null;
      branch = (Branch)base.VisitBranch((Branch)branch.Clone());
      if (branch == null) return null;
      branch.Target = this.VisitBlock(branch.Target);
      return branch;
    }
#if !MinimalReader && !CodeContracts
    public override Statement VisitCatch(Catch Catch){
      if (Catch == null) return null;
      return base.VisitCatch((Catch)Catch.Clone());
    }
    public override CatchList VisitCatchList(CatchList catchers){
      if (catchers == null) return null;
      return base.VisitCatchList(catchers.Clone());
    }
    public override Expression VisitCoerceTuple(CoerceTuple coerceTuple){
      if (coerceTuple == null) return null;
      return base.VisitCoerceTuple((CoerceTuple)coerceTuple.Clone());
    }
    public override CollectionEnumerator VisitCollectionEnumerator(CollectionEnumerator ce){
      if (ce == null) return null;
      return base.VisitCollectionEnumerator((CollectionEnumerator)ce.Clone());
    }
    public override Compilation VisitCompilation(Compilation compilation){
      if (compilation == null || compilation.TargetModule == null) return null;
      this.FindTypesToBeDuplicated(compilation.TargetModule.Types);
      return base.VisitCompilation((Compilation)compilation.Clone());
    }
    public override CompilationUnit VisitCompilationUnit(CompilationUnit cUnit){
      if (cUnit == null) return null;
      this.FindTypesToBeDuplicated(cUnit.Nodes);
      return base.VisitCompilationUnit((CompilationUnit)cUnit.Clone());
    }
    public override CompilationUnit VisitCompilationUnitSnippet(CompilationUnitSnippet snippet){
      if (snippet == null) return null;
      return base.VisitCompilationUnitSnippet((CompilationUnitSnippet)snippet.Clone());
    }
    public override Node VisitComposition(Composition comp){
      if (comp == null) return null;
      return base.VisitComposition((Composition)comp.Clone());
    }
#endif
    public override Expression VisitConstruct(Construct cons){
      if (cons == null) return null;
      return base.VisitConstruct((Construct)cons.Clone());
    }
    public override Expression VisitConstructArray(ConstructArray consArr){
      if (consArr == null) return null;
      return base.VisitConstructArray((ConstructArray)consArr.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Expression VisitConstructDelegate(ConstructDelegate consDelegate){
      if (consDelegate == null) return null;
      return base.VisitConstructDelegate((ConstructDelegate)consDelegate.Clone());
    }
    public override Expression VisitConstructFlexArray(ConstructFlexArray consArr){
      if (consArr == null) return null;
      return base.VisitConstructFlexArray((ConstructFlexArray)consArr.Clone());
    }
    public override Expression VisitConstructIterator(ConstructIterator consIterator){
      if (consIterator == null) return null;
      return base.VisitConstructIterator((ConstructIterator)consIterator.Clone());
    }
    public override Expression VisitConstructTuple(ConstructTuple consTuple){
      if (consTuple == null) return null;
      return base.VisitConstructTuple((ConstructTuple)consTuple.Clone());
    }
#endif
#if ExtendedRuntime
    public override TypeNode VisitConstrainedType(ConstrainedType cType){
      if (cType == null) return null;
      return base.VisitConstrainedType((ConstrainedType)cType.Clone());
    }
#endif
#if !MinimalReader && !CodeContracts
    public override Statement VisitContinue(Continue Continue){
      if (Continue == null) return null;
      return base.VisitContinue((Continue)Continue.Clone());
    }
    public override Expression VisitCurrentClosure(CurrentClosure currentClosure){
      if (currentClosure == null) return null;
      return base.VisitCurrentClosure((CurrentClosure)currentClosure.Clone());
    }
#endif    
    public override DelegateNode VisitDelegateNode(DelegateNode delegateNode){
      return this.VisitTypeNode(delegateNode) as DelegateNode;
    }
#if !MinimalReader && !CodeContracts
    public override Statement VisitDoWhile(DoWhile doWhile){
      if (doWhile == null) return null;
      return base.VisitDoWhile((DoWhile)doWhile.Clone());
    }
#endif
    public override Statement VisitEndFilter(EndFilter endFilter){
      if (endFilter == null) return null;
      return base.VisitEndFilter((EndFilter)endFilter.Clone());
    }
    public override Statement VisitEndFinally(EndFinally endFinally){
      if (endFinally == null) return null;
      return base.VisitEndFinally((EndFinally)endFinally.Clone());
    }
#if ExtendedRuntime || CodeContracts
    public override EnsuresList VisitEnsuresList(EnsuresList Ensures){
      if (Ensures == null) return null;
      return base.VisitEnsuresList(Ensures.Clone());
    }
#endif
    public override Event VisitEvent(Event evnt){
      if (evnt == null) return null;
      Event dup = (Event)this.DuplicateFor[evnt.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[evnt.UniqueKey] = dup = (Event)evnt.Clone();
#if !NoXml
      if (this.CopyDocumentation) dup.Documentation = evnt.Documentation;
#endif
      dup.HandlerAdder = this.VisitMethod(evnt.HandlerAdder);
      dup.HandlerCaller = this.VisitMethod(evnt.HandlerCaller);
      dup.HandlerRemover = this.VisitMethod(evnt.HandlerRemover);
      dup.OtherMethods = this.VisitMethodList(evnt.OtherMethods);
      dup.DeclaringType = this.TargetType;
      return base.VisitEvent(dup);
    }
#if !FxCop
    public virtual ExceptionHandler VisitExceptionHandler(ExceptionHandler handler){
      if (handler == null) return null;
      handler = (ExceptionHandler)handler.Clone();
      handler.BlockAfterHandlerEnd = this.VisitBlock(handler.BlockAfterHandlerEnd);
      handler.BlockAfterTryEnd = this.VisitBlock(handler.BlockAfterTryEnd);
      handler.FilterExpression = this.VisitBlock(handler.FilterExpression);
      handler.FilterType = this.VisitTypeReference(handler.FilterType);
      handler.HandlerStartBlock = this.VisitBlock(handler.HandlerStartBlock);
      handler.TryStartBlock = this.VisitBlock(handler.TryStartBlock);
      return handler;
    }
    public virtual ExceptionHandlerList VisitExceptionHandlerList(ExceptionHandlerList handlers){
      if (handlers == null) return null;
      int n = handlers.Count;
      ExceptionHandlerList result = new ExceptionHandlerList(n);
      for (int i = 0; i < n; i++)
        result.Add(this.VisitExceptionHandler(handlers[i]));
      return result;
    }
#endif
#if ExtendedRuntime || CodeContracts
    public override EnsuresExceptional VisitEnsuresExceptional(EnsuresExceptional exceptional)
    {
      if (exceptional == null) return null;
      return base.VisitEnsuresExceptional((EnsuresExceptional)exceptional.Clone());
    }
#endif
#if !MinimalReader && !CodeContracts
    public override Statement VisitExit(Exit exit)
    {
      if (exit == null) return null;
      return base.VisitExit((Exit)exit.Clone());
    }
    public override Statement VisitExpose(Expose Expose){
      if (Expose == null) return null;
      return base.VisitExpose((Expose)Expose.Clone());
    }
#endif
    public override Expression VisitExpression(Expression expression){
      if (expression == null) return null;
      switch(expression.NodeType){
        case NodeType.Dup: 
        case NodeType.Arglist:
          expression = (Expression)expression.Clone();
          break;
        case NodeType.Pop:
          UnaryExpression uex = expression as UnaryExpression;
          if (uex != null){
            uex = (UnaryExpression)uex.Clone();
            uex.Operand = this.VisitExpression(uex.Operand);
            expression = uex;
          }else
            expression = (Expression)expression.Clone();
          break;
        default:
          expression = (Expression)this.Visit(expression);
          break;
      }
      if (expression == null) return null;
      expression.Type = this.VisitTypeReference(expression.Type);
      return expression;
    }
    public override ExpressionList VisitExpressionList(ExpressionList expressions){
      if (expressions == null) return null;
      return base.VisitExpressionList(expressions.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Expression VisitExpressionSnippet(ExpressionSnippet snippet){
      if (snippet == null) return null;
      return base.VisitExpressionSnippet((ExpressionSnippet)snippet.Clone());
    }
#endif    
    public override Statement VisitExpressionStatement(ExpressionStatement statement){
      if (statement == null) return null;
      return base.VisitExpressionStatement((ExpressionStatement)statement.Clone());
    }
#if !MinimalReader
    public override Statement VisitFaultHandler(FaultHandler faultHandler){
      if (faultHandler == null) return null;
      return base.VisitFaultHandler((FaultHandler)faultHandler.Clone());
    }
    public override FaultHandlerList VisitFaultHandlerList(FaultHandlerList faultHandlers){
      if (faultHandlers == null) return null;
      return base.VisitFaultHandlerList(faultHandlers.Clone());
    }
#endif
    public override Field VisitField(Field field){
      if (field == null) return null;
      Field dup = (Field)this.DuplicateFor[field.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[field.UniqueKey] = dup = (Field)field.Clone();
      if (field.MarshallingInformation != null)
        dup.MarshallingInformation = field.MarshallingInformation.Clone();
#if !MinimalReader
      ParameterField pField = dup as ParameterField;
      if (pField != null)
        pField.Parameter = (Parameter)this.VisitParameter(pField.Parameter);
#endif
      dup.DeclaringType = this.TargetType;
#if !NoXml
      if (this.CopyDocumentation) dup.Documentation = field.Documentation;
#endif      
      return base.VisitField(dup);
    }
#if !MinimalReader && !CodeContracts
    public override Block VisitFieldInitializerBlock(FieldInitializerBlock block){
      if (block == null) return null;
      return base.VisitFieldInitializerBlock((FieldInitializerBlock)block.Clone());
    }
    public override FieldList VisitFieldList(FieldList fields){
      if (fields == null) return null;
      return base.VisitFieldList(fields.Clone());
    }
    public override Statement VisitFilter(Filter filter){
      if (filter == null) return null;
      return base.VisitFilter((Filter)filter.Clone());
    }
    public override FilterList VisitFilterList(FilterList filters){
      if (filters == null) return null;
      return base.VisitFilterList(filters.Clone());
    }
    public override Statement VisitFinally(Finally Finally){
      if (Finally == null) return null;
      return base.VisitFinally((Finally)Finally.Clone());
    }
    public override Statement VisitFixed(Fixed Fixed){
      if (Fixed == null) return null;
      return base.VisitFixed((Fixed)Fixed.Clone());
    }
    public override Statement VisitFor(For For){
      if (For == null) return null;
      return base.VisitFor((For)For.Clone());
    }
    public override Statement VisitForEach(ForEach forEach){
      if (forEach == null) return null;
      return base.VisitForEach((ForEach)forEach.Clone());
    }
    public override Statement VisitFunctionDeclaration(FunctionDeclaration functionDeclaration){
      if (functionDeclaration == null) return null;
      return base.VisitFunctionDeclaration((FunctionDeclaration)functionDeclaration.Clone());
    }
    public override Statement VisitGoto(Goto Goto){
      if (Goto == null) return null;
      return base.VisitGoto((Goto)Goto.Clone());
    }
    public override Statement VisitGotoCase(GotoCase gotoCase){
      if (gotoCase == null) return null;
      return base.VisitGotoCase((GotoCase)gotoCase.Clone());
    }
#endif
    public override Expression VisitIdentifier(Identifier identifier){
      if (identifier == null) return null;
      return base.VisitIdentifier((Identifier)identifier.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Statement VisitIf(If If){
      if (If == null) return null;
      return base.VisitIf((If)If.Clone());
    }
    public override Expression VisitImplicitThis(ImplicitThis implicitThis){
      if (implicitThis == null) return null;
      return base.VisitImplicitThis((ImplicitThis)implicitThis.Clone());
    }
#endif    
    public override Expression VisitIndexer(Indexer indexer){
      if (indexer == null) return null;
      indexer = (Indexer)base.VisitIndexer((Indexer)indexer.Clone());
      if (indexer == null) return null;
      indexer.ElementType = this.VisitTypeReference(indexer.ElementType);
      return indexer;
    }
    public override InterfaceList VisitInterfaceReferenceList(InterfaceList interfaceReferences){
      if (interfaceReferences == null) return null;
      return base.VisitInterfaceReferenceList(interfaceReferences.Clone());
    }
#if ExtendedRuntime || CodeContractsc
    public override InvariantList VisitInvariantList(InvariantList Invariants){
      if (Invariants == null) return null;
      return base.VisitInvariantList(Invariants.Clone());
    }
#endif
#if !MinimalReader
    public override Statement VisitLabeledStatement(LabeledStatement lStatement){
      if (lStatement == null) return null;
      return base.VisitLabeledStatement((LabeledStatement)lStatement.Clone());
    }
#endif
    public override Expression VisitLiteral(Literal literal){
      if (literal == null) return null;
      TypeNode cloneType = this.VisitTypeReference(literal.Type);
      TypeNode t = literal.Value as TypeNode;
      if (t != null)
        return new Literal(this.VisitTypeReference(t), cloneType, literal.SourceContext);
      TypeNode[] tarr = literal.Value as TypeNode[];
      if (tarr != null) {
        int len = tarr == null ? 0 : tarr.Length;
        TypeNode[] newarr = tarr == null ? null : new TypeNode[len];
        for (int i = 0; i < len; i++) newarr[i] = this.VisitTypeReference(tarr[i]);
        return new Literal(newarr, cloneType);
      }
      object[] arr = literal.Value as object[];
      if (arr != null) {
        int len = arr.Length;
        object[] newarr = new object[len];
        for (int i = 0; i < len; i++) {
          Literal litelt = arr[i] as Literal;
          if (litelt != null)
            newarr[i] = VisitLiteral(litelt);
          else
            newarr[i] = arr[i];
        }
        return new Literal(newarr, cloneType);
      }
      Literal result = (Literal)literal.Clone();
      result.Type = cloneType;
      return result;
    }
    public override Expression VisitLocal(Local local){
      if (local == null) return null;
      Local dup = (Local)this.DuplicateFor[local.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[local.UniqueKey] = dup = (Local)local.Clone();
      return base.VisitLocal(dup);
    }
#if !MinimalReader && !CodeContracts
    public override LocalDeclaration VisitLocalDeclaration(LocalDeclaration localDeclaration){
      if (localDeclaration == null) return null;
      return base.VisitLocalDeclaration((LocalDeclaration)localDeclaration.Clone());
    }
    public override LocalDeclarationList VisitLocalDeclarationList(LocalDeclarationList localDeclarations){
      if (localDeclarations == null) return null;
      return base.VisitLocalDeclarationList(localDeclarations.Clone());
    }
    public override Statement VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations){
      if (localDeclarations == null) return null;
      return base.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)localDeclarations.Clone());
    }
    public override Statement VisitLock(Lock Lock){
      if (Lock == null) return null;
      return base.VisitLock((Lock)Lock.Clone());
    }
    public override Statement VisitAcquire(Acquire acquire){
      if (acquire == null) return null;
      return base.VisitAcquire((Acquire) acquire.Clone());
    }
    public override Statement VisitResourceUse(ResourceUse resourceUse){
      if (resourceUse == null) return null;
      return base.VisitResourceUse((ResourceUse)resourceUse.Clone());
    }
    public override Expression VisitLRExpression(LRExpression expr){
      if (expr == null) return null;
      return base.VisitLRExpression((LRExpression)expr.Clone());
    }
#endif
    public override Expression VisitMemberBinding(MemberBinding memberBinding){
      if (memberBinding == null) return null;
      memberBinding = (MemberBinding)memberBinding.Clone();
      memberBinding.TargetObject = this.VisitExpression(memberBinding.TargetObject);
      memberBinding.Type = this.VisitTypeReference(memberBinding.Type);
      memberBinding.BoundMember = this.VisitMemberReference(memberBinding.BoundMember);
      return memberBinding;
    }
    public override MemberList VisitMemberList(MemberList members){
      if (members == null) return null;
      var dup = members.Clone();
      for (int i = 0; i < dup.Count; i++)
      {
        var member = dup[i];
        if (this.RecordOriginalAsTemplate && member is TypeNode)
        {
          dup[i] = null;
        }
        else
        {
          dup[i] = (Member)this.Visit(member);
          Debug.Assert(member == null || dup[i] != null);
        }
      }
      return dup;
    }
    public virtual Member VisitMemberReference(Member member){
      if (member == null) return null;
      Member dup = (Member)this.DuplicateFor[member.UniqueKey];
      if (dup != null) return dup;
#if !MinimalReader && !CodeContracts
      if (member is ParameterField && !(member.DeclaringType is ClosureClass)) return member; //Can happen when duplicating expressions within a method
#endif
      TypeNode t = member as TypeNode;
      if (t != null) return this.VisitTypeReference(t);

      if (this.RecordOriginalAsTemplate) return member; // mapping done in Specializer
      Method method = member as Method;
      if (method != null && method.Template != null && method.TemplateArguments != null && method.TemplateArguments.Count > 0){
        Method template = this.VisitMemberReference(method.Template) as Method;
        bool needNewInstance = template != null && template != method.Template;
        TypeNodeList args = method.TemplateArguments.Clone();
        for (int i = 0, n = args.Count; i < n; i++){
          TypeNode arg = this.VisitTypeReference(args[i]);
          if (arg != null && arg != args[i]){
            args[i] = arg;
            needNewInstance = true;
          }
        }
        if (needNewInstance) {
          //^ assert template != null;
          return template.GetTemplateInstance(this.TargetType, args);
        }
        return method;
      }
      TypeNode declaringType = this.VisitTypeReference(member.DeclaringType);
      if (declaringType == null) return member;
      if (declaringType == member.DeclaringType) return member;
      // this could delay things...if (declaringType.Template == null && this.TypesToBeDuplicated[declaringType.UniqueKey] == null) return member;
      // TypeNode tgtType = this.VisitTypeReference(declaringType); //duplicates its members

      dup = (Member)this.DuplicateFor[member.UniqueKey];
      if (dup == null){
        dup = Specializer.GetCorrespondingMember(member, declaringType);
        if (dup != null) return dup;
        Debug.Assert(false);
        //Can get here when declaringType has not yet been completely duplicated
        TypeNode savedTargetType = this.TargetType;
        this.TargetType = declaringType;
        dup = (Member)this.Visit(member);
        this.TargetType = savedTargetType;

      }
      return dup;
    }
    public virtual MemberList VisitMemberReferenceList(MemberList members){
      if (members == null) return null;
      int n = members.Count;
      MemberList dup = new MemberList(n);
      for (int i = 0; i < n; i++)
        dup.Add(this.VisitMemberReference(members[i]));
      return dup;
    }
        [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public readonly Block DummyBody = new Block();

    public override Method VisitMethod(Method method) {
      if (method == null) return null;
      Method dup = (Method)this.DuplicateFor[method.UniqueKey];
      if (dup != null) return dup;

      if (TargetPlatform.UseGenerics
          && !this.RecordOriginalAsTemplate
         )
      {
          // leave generic template parameters unchanged if we create an instance
          this.FindTypesToBeDuplicated(method.TemplateParameters);
      }
      return VisitMethodInternal(method);
    }

    /// <summary>
    /// Does not copy the method's template parameters unless they are marked for duplication already.
    /// </summary>
    public Method VisitMethodInternal(Method method){
      if (method == null) return null;
      Method dup = (Method)this.DuplicateFor[method.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[method.UniqueKey] = dup = (Method)method.Clone();
      dup.ProviderHandle = null;
#if !FxCop
      dup.LocalList = null;
#endif
      Method savedTarget = this.TargetMethod;
      this.TargetMethod = dup;
      var savedTemplateParameters = method.TemplateParameters;
      if (TargetPlatform.UseGenerics
          && !this.RecordOriginalAsTemplate
          )
      {
          dup.TemplateParameters = this.VisitTypeParameterList(savedTemplateParameters);
          savedTemplateParameters = null;
      }
      else
      {
          dup.TemplateParameters = null; // avoid visiting them
      }

#if !MinimalReader && !CodeContracts
      if (dup.Scope != null) {
        this.TypesToBeDuplicated[dup.Scope.UniqueKey] = dup.Scope;
        dup.Scope = this.VisitTypeNode(dup.Scope) as MethodScope;
      }
#endif
#if !NoXml
      if (this.CopyDocumentation) dup.Documentation = method.Documentation;
#endif
      dup.OverriddenMember = null; // let instantiation be recomputed
      dup.ImplementedInterfaceMethods = this.VisitMethodReferenceList(method.ImplementedInterfaceMethods);
      dup.ImplicitlyImplementedInterfaceMethods = null; // just reset it so it gets recomputed if ever asked for
      dup.DeclaringType = this.TargetType;
      if (!method.IsAbstract) dup.Body = this.DummyBody;
      if (this.RecordOriginalAsTemplate) {
        if (method.Template != null)
          dup.Template = method.Template;
        else
          dup.Template = method;
      }
      dup.PInvokeModule = this.VisitModuleReference(dup.PInvokeModule);
      if (method.ReturnTypeMarshallingInformation != null)
        dup.ReturnTypeMarshallingInformation = method.ReturnTypeMarshallingInformation.Clone();
      dup.ThisParameter = (This)this.VisitParameter(dup.ThisParameter);
#if ExtendedRuntime || CodeContracts
      dup.ProvideContract = null;
      dup.contract = null;
#endif
      dup = base.VisitMethod(dup);
      //^ assume dup != null;

      // restore template parameters if we need to
      if (savedTemplateParameters != null)
      {
          dup.TemplateParameters = savedTemplateParameters.Clone();
          savedTemplateParameters = null;
      }
      // Visiting the declaring member can cause this method to be re-entered,
      // so only visit after we duplicated the other properties so that we 
      // do not return a half-duplicated method.

      // it doesn't make sense to copy a getter/setter/event method without the corresponding event
      // (one can always pre-populate if necessary)
      dup.DeclaringMember = (Member)this.Visit(dup.DeclaringMember);
      dup.fullName = null;
#if !NoXml
      dup.DocumentationId = null;
#endif
      dup.ProviderHandle = method; // we always need the handle, as we may use it for attributes.
      dup.Attributes = null;
      dup.ProvideMethodAttributes = new Method.MethodAttributeProvider(this.ProvideMethodAttributes);
#if ExtendedRuntime || CodeContracts
      dup.ProvideContract = new Method.MethodContractProvider(this.ProvideMethodContract);
#endif
      if (!this.SkipBodies && !method.IsAbstract)
      {
        dup.Body = null;
        dup.ProvideBody = new Method.MethodBodyProvider(this.ProvideMethodBody);
      }
      if (this.SkipBodies) dup.Instructions = new InstructionList(0);
      
      this.TargetMethod = savedTarget;
      return dup;
    }
    public override Expression VisitMethodCall(MethodCall call){
      if (call == null) return null;
      return base.VisitMethodCall((MethodCall)call.Clone());
    }
#if ExtendedRuntime || CodeContracts
    public override MethodContract VisitMethodContract(MethodContract contract)
    {
      if (contract == null) return null;
      MethodContract dup = (MethodContract)this.DuplicateFor[contract.UniqueKey];
      if (dup != null) return dup;
      //Make sure not to break the relation between contract.LocalForResult and
      //references to contract.LocalForResult in the contract:
      //Revised: seems that code for implementing an interface property depends on relation being broken.
      /*
      Local localForResult = contract.LocalForResult;
      if (localForResult != null) {
        localForResult = new Local(localForResult.Name, localForResult.Type);
        this.DuplicateFor[contract.LocalForResult.UniqueKey] = localForResult;
      }
      */      
      dup = (MethodContract)contract.Clone();
      //contract.LocalForResult = localForResult;
      //^ assume this.TargetMethod != null;
      dup.contractInitializer = this.VisitBlock(contract.ContractInitializer);
      dup.postPreamble = this.VisitBlock(contract.PostPreamble);
      dup.DeclaringMethod = this.TargetMethod;
      dup.ensures = this.VisitEnsuresList(contract.Ensures);
      dup.asyncEnsures = this.VisitEnsuresList(contract.AsyncEnsures);
      dup.modelEnsures = this.VisitEnsuresList(contract.ModelEnsures);
      dup.modifies = this.VisitExpressionList(contract.Modifies);
      dup.requires = this.VisitRequiresList(contract.Requires);
      return dup;
    }
#endif
    public virtual MethodList VisitMethodList(MethodList methods)
    {
      if (methods == null) return null;
      int n = methods.Count;
      MethodList dup = new MethodList(n);
      for (int i = 0; i < n; i++)
        dup.Add(this.VisitMethod(methods[i]));
      return dup;
    }
    public virtual MethodList VisitMethodReferenceList(MethodList methods){
      if (methods == null) return null;
      int n = methods.Count;
      MethodList dup = new MethodList(n);
      for (int i = 0; i < n; i++)
        dup.Add((Method)this.VisitMemberReference(methods[i]));
      return dup;
    }
    public override Module VisitModule(Module module){
      if (module == null) return null;
      Module dup = (Module)module.Clone();
      if (this.TargetModule == null) this.TargetModule = dup;
      this.FindTypesToBeDuplicated(module.Types);
      return base.VisitModule(dup);
    }
    public virtual Module VisitModuleReference(Module module){
      if (module == null) return null;
      Module dup = (Module)this.DuplicateFor[module.UniqueKey];
      if (dup != null) return dup;
      for (int i = 0, n = this.TargetModule.ModuleReferences == null ? 0 : this.TargetModule.ModuleReferences.Count; i < n; i++){
        //^ assert this.TargetModule.ModuleReferences != null;
        ModuleReference modRef = this.TargetModule.ModuleReferences[i];
        if (modRef == null) continue;
        if (string.Compare(module.Name, modRef.Name, true, System.Globalization.CultureInfo.InvariantCulture) != 0) continue;
        this.DuplicateFor[module.UniqueKey] = modRef.Module; return modRef.Module;
      }
      if (this.TargetModule.ModuleReferences == null)
        this.TargetModule.ModuleReferences = new ModuleReferenceList();
      this.TargetModule.ModuleReferences.Add(new ModuleReference(module.Name, module));
      this.DuplicateFor[module.UniqueKey] = module;
      return module;
    }
    public override ModuleReference VisitModuleReference(ModuleReference moduleReference){
      if (moduleReference == null) return null;
      return base.VisitModuleReference((ModuleReference)moduleReference.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Expression VisitNameBinding(NameBinding nameBinding){
      if (nameBinding == null) return null;
      nameBinding = (NameBinding)nameBinding.Clone();
      nameBinding.BoundMember = this.VisitExpression(nameBinding.BoundMember);
      nameBinding.BoundMembers = this.VisitMemberReferenceList(nameBinding.BoundMembers);
      return nameBinding;
    }
#endif
    public override Expression VisitNamedArgument(NamedArgument namedArgument){
      if (namedArgument == null) return null;
      return base.VisitNamedArgument((NamedArgument)namedArgument.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Namespace VisitNamespace(Namespace nspace){
      if (nspace == null) return null;
      return base.VisitNamespace((Namespace)nspace.Clone());
    }
    public override NamespaceList VisitNamespaceList(NamespaceList namespaces){
      if (namespaces == null) return null;
      return base.VisitNamespaceList(namespaces.Clone());
    }
    public override NodeList VisitNodeList(NodeList nodes){
      if (nodes == null) return null;
      return base.VisitNodeList(nodes.Clone());
    }
#endif
#if ExtendedRuntime || CodeContracts
    public override EnsuresNormal VisitEnsuresNormal(EnsuresNormal normal)
    {
      if (normal == null) return null;
      return base.VisitEnsuresNormal((EnsuresNormal)normal.Clone());
    }
    public override Expression VisitOldExpression (OldExpression oldExpression) {
      if (oldExpression == null) return null;
      return base.VisitOldExpression((OldExpression)oldExpression.Clone());
    }
    public override Expression VisitReturnValue(ReturnValue retval)
    {
      if (retval == null) return null;
      ReturnValue dup = (ReturnValue)this.DuplicateFor[retval.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[retval.UniqueKey] = dup = (ReturnValue)retval.Clone();
      return base.VisitReturnValue(dup);
    }
    public override RequiresOtherwise VisitRequiresOtherwise(RequiresOtherwise otherwise)
    {
      if (otherwise == null) return null;
      RequiresOtherwise dup = (RequiresOtherwise)this.DuplicateFor[otherwise.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[otherwise.UniqueKey] = dup = (RequiresOtherwise)otherwise.Clone();
      return base.VisitRequiresOtherwise(dup);
    }
#endif
    public override Expression VisitParameter(Parameter parameter)
    {
      if (parameter == null) return null;
      Parameter dup = (Parameter)this.DuplicateFor[parameter.UniqueKey];
      if (dup != null) {
        if (dup.DeclaringMethod == null) dup.DeclaringMethod = this.TargetMethod;
        return dup;
      }
      this.DuplicateFor[parameter.UniqueKey] = dup = (Parameter)parameter.Clone();
      if (dup.MarshallingInformation != null)
        dup.MarshallingInformation = dup.MarshallingInformation.Clone();
      dup.DeclaringMethod = this.TargetMethod;
#if !MinimalReader
      dup.paramArrayElementType = null;
#endif
      return base.VisitParameter(dup);
    }
    public override ParameterList VisitParameterList(ParameterList parameterList){
      if (parameterList == null) return null;
      return base.VisitParameterList(parameterList.Clone());
    }
#if ExtendedRuntime || CodeContracts
    public override RequiresPlain VisitRequiresPlain(RequiresPlain plain)
    {
      if (plain == null) return null;
      var dup = (RequiresPlain)this.DuplicateFor[plain.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[plain.UniqueKey] = dup = (RequiresPlain)plain.Clone();

      var result = base.VisitRequiresPlain(dup);
      return result;
    }
#endif
#if !MinimalReader
    public override Expression VisitPrefixExpression(PrefixExpression pExpr)
    {
      if (pExpr == null) return null;
      return base.VisitPrefixExpression((PrefixExpression)pExpr.Clone());
    }
    public override Expression VisitPostfixExpression(PostfixExpression pExpr){
      if (pExpr == null) return null;
      return base.VisitPostfixExpression((PostfixExpression)pExpr.Clone());
    }
#endif
    public override Property VisitProperty(Property property){
      if (property == null) return null;
      Property dup = (Property)this.DuplicateFor[property.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[property.UniqueKey] = dup = (Property)property.Clone();
      dup.Attributes = this.VisitAttributeList(property.Attributes);
#if !NoXml      
      if (this.CopyDocumentation) dup.Documentation = property.Documentation;
#endif      
      dup.Type = this.VisitTypeReference(property.Type);
      dup.Getter = this.VisitMethod(property.Getter);
      dup.Setter = this.VisitMethod(property.Setter);
      dup.OtherMethods = this.VisitMethodList(property.OtherMethods);
      dup.DeclaringType = this.TargetType;
      dup.Parameters = this.VisitParameterList(dup.Parameters);
      return dup;
    }
#if !MinimalReader && !CodeContracts
    public override Expression VisitQuantifier(Quantifier quantifier) {
      if (quantifier == null) return null;
      return base.VisitQuantifier((Quantifier)quantifier.Clone());
    }
    public override Expression VisitComprehension(Comprehension Comprehension){
      if (Comprehension == null) return null;
      return base.VisitComprehension((Comprehension)Comprehension.Clone());
    }
    public override ComprehensionBinding VisitComprehensionBinding(ComprehensionBinding comprehensionBinding) {
      if (comprehensionBinding == null) return null;
      return base.VisitComprehensionBinding((ComprehensionBinding)comprehensionBinding.Clone());
    }
    public override Expression VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier){
      if (qualifiedIdentifier == null) return null;
      return base.VisitQualifiedIdentifier((QualifiedIdentifier)qualifiedIdentifier.Clone());
    }
    public override Statement VisitRepeat(Repeat repeat){
      if (repeat == null) return null;
      return base.VisitRepeat((Repeat)repeat.Clone());
    }
#endif
#if ExtendedRuntime || CodeContracts
    public override RequiresList VisitRequiresList(RequiresList Requires)
    {
      if (Requires == null) return null;
      return base.VisitRequiresList(Requires.Clone());
    }
#endif
    public override Statement VisitReturn(Return Return)
    {
      if (Return == null) return null;
      return base.VisitReturn((Return)Return.Clone());
    }
    public override SecurityAttribute VisitSecurityAttribute(SecurityAttribute attribute){
      if (attribute == null) return null;
      return base.VisitSecurityAttribute((SecurityAttribute)attribute.Clone());;
    }
    public override SecurityAttributeList VisitSecurityAttributeList(SecurityAttributeList attributes){
      if (attributes == null) return null;
      return base.VisitSecurityAttributeList(attributes.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Expression VisitSetterValue(SetterValue value){
      if (value == null) return null;
      return base.VisitSetterValue((SetterValue)value.Clone());
    }
#endif
    public override StatementList VisitStatementList(StatementList statements){
      if (statements == null) return null;
      return base.VisitStatementList(statements.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override StatementSnippet VisitStatementSnippet(StatementSnippet snippet){
      if (snippet == null) return null;
      return base.VisitStatementSnippet((StatementSnippet)snippet.Clone());
    }
    public override Statement VisitSwitch(Switch Switch){
      if (Switch == null) return null;
      return base.VisitSwitch((Switch)Switch.Clone());
    }
    public override SwitchCase VisitSwitchCase(SwitchCase switchCase){
      if (switchCase == null) return null;
      return base.VisitSwitchCase((SwitchCase)switchCase.Clone());
    }
    public override SwitchCaseList VisitSwitchCaseList(SwitchCaseList switchCases){
      if (switchCases == null) return null;
      return base.VisitSwitchCaseList(switchCases.Clone());
    }
#endif
    public override Statement VisitSwitchInstruction(SwitchInstruction switchInstruction){
      if (switchInstruction == null) return null;
      switchInstruction = (SwitchInstruction)base.VisitSwitchInstruction((SwitchInstruction)switchInstruction.Clone());
      if (switchInstruction == null) return null;
      switchInstruction.Targets = this.VisitBlockList(switchInstruction.Targets);
      return switchInstruction;
    }
#if !MinimalReader && !CodeContracts
    public override Statement VisitTypeswitch(Typeswitch Typeswitch){
      if (Typeswitch == null) return null;
      return base.VisitTypeswitch((Typeswitch)Typeswitch.Clone());
    }
    public override TypeswitchCase VisitTypeswitchCase(TypeswitchCase typeswitchCase){
      if (typeswitchCase == null) return null;
      return base.VisitTypeswitchCase((TypeswitchCase)typeswitchCase.Clone());
    }
    public override TypeswitchCaseList VisitTypeswitchCaseList(TypeswitchCaseList typeswitchCases){
      if (typeswitchCases == null) return null;
      return base.VisitTypeswitchCaseList(typeswitchCases.Clone());
    }
#endif
    public override Expression VisitTernaryExpression(TernaryExpression expression){
      if (expression == null) return null;
      return base.VisitTernaryExpression((TernaryExpression)expression.Clone());
    }
    public override Expression VisitThis(This This){
      if (This == null) return null;
      This dup = (This)this.DuplicateFor[This.UniqueKey];
      if (dup != null) return dup;      
      this.DuplicateFor[This.UniqueKey] = dup = (This)This.Clone();
      return base.VisitThis(dup);
    }
    public override Statement VisitThrow(Throw Throw){
      if (Throw == null) return null;
      return base.VisitThrow((Throw)Throw.Clone());
    }
#if !MinimalReader && !CodeContracts
    public override Statement VisitTry(Try Try){
      if (Try == null) return null;
      return base.VisitTry((Try)Try.Clone());
    }
#endif
#if ExtendedRuntime    
    public override TypeAlias VisitTypeAlias(TypeAlias tAlias){
      if (tAlias == null) return null;
      TypeAlias dup = (TypeAlias)this.DuplicateFor[tAlias.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[tAlias.UniqueKey] = dup = (TypeAlias)tAlias.Clone();
      dup.Name = tAlias.Name;
      if (tAlias.AliasedType is ConstrainedType)
        //The type alias defines the constrained type, rather than just referencing it
        dup.AliasedType = this.VisitConstrainedType((ConstrainedType)tAlias.AliasedType);
      else
        dup.AliasedType = this.VisitTypeReference(tAlias.AliasedType);
      dup.DeclaringType = this.TargetType;
      dup.DeclaringModule = this.TargetModule;
      dup.ProvideMembers();
      return dup;
    }
#endif
#if ExtendedRuntime || CodeContracts
    public override TypeContract VisitTypeContract(TypeContract contract){
      if (this.RecordOriginalAsTemplate) return null; //A type template instance does not need invariants
      if (contract == null) return null;
      contract = (TypeContract)contract.Clone();
      contract.DeclaringType = this.VisitTypeReference(contract.DeclaringType);
      contract.Invariants = this.VisitInvariantList(contract.invariants);
      return contract;
    }
#endif
    public override TypeModifier VisitTypeModifier(TypeModifier typeModifier){
      if (typeModifier == null) return null;
      return base.VisitTypeModifier((TypeModifier)typeModifier.Clone());
    }
    public override TypeNode VisitTypeNode(TypeNode type){
      if (type == null) return null;
      TypeNode dup = this.VisitTypeNode(type, null, null, null, true);
      //^ assume dup != null;
      TypeNodeList nestedTypes = type.NestedTypes;
      if (nestedTypes != null && nestedTypes.Count > 0)
      {
        Debug.Assert(dup != type);
        this.VisitNestedTypes(dup, nestedTypes);
      }
      return dup;
    }
    internal TypeNode VisitTypeNode(TypeNode type, Identifier mangledName, TypeNodeList templateArguments, TypeNode template, bool delayVisitToNestedTypes){
      if (type == null) return null;
      Debug.Assert(this.TypesToBeDuplicated[type.UniqueKey] != null);
      TypeNode dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
      if (dup != null) return dup;
      this.DuplicateFor[type.UniqueKey] = dup = (TypeNode)type.Clone();
      //if (mangledName != null)
      if (templateArguments != null)
      {
        //this.TargetModule.StructurallyEquivalentType[mangledName.UniqueIdKey] = dup;
        dup.TemplateArguments = templateArguments;
      }
      dup.arrayTypes = null;
      dup.constructors = null;
      dup.consolidatedTemplateArguments = null;
      dup.consolidatedTemplateParameters = null;
#if DEBUG && !MinimalReader
      dup.DebugLabel = null;
#endif
#if !NoXml      
      dup.DocumentationId = null;
      if (this.CopyDocumentation) dup.Documentation = type.Documentation;
#endif
      dup.defaultMembers = null;
#if !MinimalReader
      dup.explicitCoercionFromTable = null;
      dup.explicitCoercionMethods = null;
      dup.implicitCoercionFromTable = null;
      dup.implicitCoercionMethods = null;
      dup.implicitCoercionToTable = null;
#endif
      dup.memberCount = 0;
      dup.memberTable = null;
      dup.modifierTable = null;
      dup.NestedTypes = null;
      dup.pointerType = null;
      dup.ProviderHandle = null;
      dup.ProvideTypeAttributes = null;
      dup.ProvideTypeMembers = null;
      dup.ProvideNestedTypes = null;
      dup.referenceType = null;
#if !NoReflection
      dup.runtimeType = null;
#endif
      dup.structurallyEquivalentMethod = null;
      dup.ClearTemplateInstanceCache();
      TypeParameter tp = dup as TypeParameter;
      if (tp != null) tp.structuralElementTypes = null;
      ClassParameter cp = dup as ClassParameter;
      if (cp != null) cp.structuralElementTypes = null;
      dup.szArrayTypes = null;
      if (this.RecordOriginalAsTemplate && !(dup is ITypeParameter)) dup.Template = type;
      dup.TemplateArguments = null;
      dup.DeclaringModule = this.TargetModule;
      dup.DeclaringType = (dup is ITypeParameter) ? null : this.TargetType;
      dup.ProviderHandle = type;
      dup.Attributes = null;
      dup.SecurityAttributes = null;
      dup.ProvideTypeAttributes = new TypeNode.TypeAttributeProvider(this.ProvideTypeAttributes);
      Class c = dup as Class;
      if (c != null) { c.BaseClass = null; }
      dup.Interfaces = null;
      dup.templateParameters = null;
      dup.consolidatedTemplateParameters = null;
      dup.ProvideTypeSignature = new TypeNode.TypeSignatureProvider(this.ProvideTypeSignature);
#if !MinimalReader && !CodeContracts
      if (dup is MethodScope)
        dup.members = this.VisitMemberList(type.members);
      else
#endif 
      if (!this.RecordOriginalAsTemplate){
#if false
        if (!delayVisitToNestedTypes)
          dup.nestedTypes = this.VisitNestedTypes(dup, type.NestedTypes);
#endif
        dup.members = null;
        dup.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
      } else {
        dup.members = null;
        //dup.ProvideNestedTypes = new TypeNode.NestedTypeProvider(this.ProvideNestedTypes);
        dup.ProvideTypeMembers = new TypeNode.TypeMemberProvider(this.ProvideTypeMembers);
      }
      DelegateNode delegateNode = dup as DelegateNode;
      if (delegateNode != null){
#if false && !MinimalReader
        if (!delegateNode.IsNormalized || !this.RecordOriginalAsTemplate){
          if (!delegateNode.IsNormalized) 
            ((DelegateNode)type).ProvideMembers();
          delegateNode.Parameters = this.VisitParameterList(delegateNode.Parameters);
          delegateNode.ReturnType = this.VisitTypeReference(delegateNode.ReturnType);
        }else
#endif
        {
          delegateNode.Parameters = null;
          delegateNode.ReturnType = null;
        }
      }
#if ExtendedRuntime || CodeContracts
      dup.Contract = null;
#endif
      dup.membersBeingPopulated = false;
      return dup;
    }
    private void ProvideTypeSignature(TypeNode/*!*/ dup, object/*!*/ handle)
    {
      TypeNode type = (TypeNode)handle;
      TypeNode savedTargetType = this.TargetType;
      this.TargetType = dup;
      this.TargetModule = dup.DeclaringModule;
      
      // There could be type instantiations of this thing during the following visits.
      // They need to know what type parameters we have.
      // Null check is here so if someone copies a type and then immediately updates the template parameters
      //  we don't override it
      if (!this.RecordOriginalAsTemplate && dup.templateParameters == null)
      {
        dup.TemplateParameters = this.VisitTypeReferenceList(type.TemplateParameters);
      }
      if (dup.DeclaringType == null && !(dup is ITypeParameter))
      {
        var originalDeclaringType = type.DeclaringType;
        var declaringType = this.VisitTypeReference(originalDeclaringType);
        if (originalDeclaringType != null && (declaringType == null || declaringType == originalDeclaringType))
        {
          dup.DeclaringType = this.OriginalTargetType;
        }
        else
        {
          dup.DeclaringType = declaringType;
        }
      }
      Class c = dup as Class;
      if (c != null && c.baseClass == null)
      {
        Class templateClass = (Class)type;
        c.BaseClass = (Class)this.VisitTypeReference(templateClass.BaseClass);
      }
      dup.Interfaces = this.VisitInterfaceReferenceList(type.Interfaces);

      this.TargetType = savedTargetType;
    }
    private void ProvideNestedTypes(TypeNode/*!*/ dup, object/*!*/ handle) {
      TypeNode template = (TypeNode)handle;
      TypeNode savedTargetType = this.TargetType;
      Module savedTargetModule = this.TargetModule;
      this.TargetType = dup;
      //^ assume dup.DeclaringModule != null;
      this.TargetModule = dup.DeclaringModule;
      this.FindTypesToBeDuplicated(template.NestedTypes);
      dup.NestedTypes = this.VisitNestedTypes(dup, template.NestedTypes);
      this.TargetModule = savedTargetModule;
      this.TargetType = savedTargetType;
    }
    private void ProvideTypeMembers(TypeNode/*!*/ dup, object/*!*/ handle) {
      TypeNode template = (TypeNode)handle;
      Debug.Assert(!template.membersBeingPopulated);
      TypeNode savedTargetType = this.TargetType;
      Module savedTargetModule = this.TargetModule;
      this.TargetType = dup;
      //^ assume dup.DeclaringModule != null;
      this.TargetModule = dup.DeclaringModule;
      //if (!this.RecordOriginalAsTemplate) this.FindTypesToBeDuplicated(template.NestedTypes);
      dup.Members = this.VisitMemberList(template.Members);
      DelegateNode delegateNode = dup as DelegateNode;
      if (delegateNode != null && delegateNode.IsNormalized){
        Debug.Assert(dup.Members != null && dup.Members.Count > 0 && dup.Members[0] != null);
        DelegateNode templateDelegateNode = template as DelegateNode;
        delegateNode.Parameters = this.VisitParameterList(templateDelegateNode.Parameters);
        delegateNode.ReturnType = this.VisitTypeReference(templateDelegateNode.ReturnType);
      }
#if ExtendedRuntime || CodeContracts
      dup.Contract = this.VisitTypeContract(template.Contract);
#endif

      this.TargetModule = savedTargetModule;
      this.TargetType = savedTargetType;
    }
    protected virtual void ProvideMethodBody(Method/*!*/ dup, object/*!*/ handle, bool asInstructionList) {
      if (asInstructionList) {
        // We don't really have a way to provide instructions, but we set it to an empty list
        dup.Instructions = new InstructionList(0);
        return;
      }
      Method template = (Method)handle;
      Block tbody = template.Body;
      if (tbody == null){
        dup.ProvideBody = null;
        return;
      }
      TypeNode savedTargetType = this.TargetType;
      this.TargetType = dup.DeclaringType;
      dup.Body = this.VisitBlock(tbody);
#if !FxCop
      dup.ExceptionHandlers = this.VisitExceptionHandlerList(template.ExceptionHandlers);
#endif
      this.TargetType = savedTargetType;
    }
    protected virtual void ProvideMethodAttributes(Method/*!*/ dup, object/*!*/ handle) {
      Method template = (Method)handle;
      AttributeList tattributes = template.Attributes;
      SecurityAttributeList tSecurityAttributes = template.SecurityAttributes;
      if (tattributes == null && tSecurityAttributes == null) {
        dup.ProvideMethodAttributes = null;
        return;
      }
      TypeNode savedTargetType = this.TargetType;
      this.TargetType = dup.DeclaringType;
      dup.Attributes = this.VisitAttributeList(tattributes);
      dup.SecurityAttributes = this.VisitSecurityAttributeList(tSecurityAttributes);
      this.TargetType = savedTargetType;
    }
#if ExtendedRuntime || CodeContracts
    protected virtual void ProvideMethodContract(Method dup, object handle)
    {
      dup.ProvideContract = null;
      Method template = (Method)handle;
      var tcontract = template.Contract;
      if (tcontract == null)
      {
        return;
      }
      TypeNode savedTargetType = this.TargetType;
      Method savedTargetMethod = this.TargetMethod;
      this.TargetType = dup.DeclaringType;
      this.TargetMethod = dup;
      dup.contract = this.VisitMethodContract(tcontract);
      this.TargetType = savedTargetType;
      this.TargetMethod = savedTargetMethod;
    }
#endif
    private void ProvideTypeAttributes(TypeNode/*!*/ dup, object/*!*/ handle) {
      TypeNode template = (TypeNode)handle;
      AttributeList templateAttributes = template.Attributes;
      SecurityAttributeList templateSecurityAttributes = template.SecurityAttributes;
      if (templateAttributes == null && templateSecurityAttributes == null) {
        return;
      }
      TypeNode savedTargetType = this.TargetType;
      this.TargetType = dup;
      dup.Attributes = this.VisitAttributeList(templateAttributes);
      dup.SecurityAttributes = this.VisitSecurityAttributeList(templateSecurityAttributes);
      this.TargetType = savedTargetType;
    }
    public virtual TypeNodeList VisitNestedTypes(TypeNode/*!*/ declaringType, TypeNodeList types) {
      if (types == null) return null;
      var savedTargetType = this.TargetType;
      this.TargetType = declaringType;
      TypeNodeList dupTypes = types.Clone();
      for (int i = 0, n = types.Count; i < n; i++){
        TypeNode nt = types[i];
        if (nt == null) continue;
        TypeNode ntdup;
        if (TargetPlatform.UseGenerics) {
          ntdup = dupTypes[i] = this.VisitTypeNode(nt, null, null, null, true);
        }
        else {
          ntdup = dupTypes[i] = this.VisitTypeReference(nt);
        }
        Debug.Assert(ntdup != nt);
        if (ntdup != nt && ntdup != null){
          if (this.RecordOriginalAsTemplate) ntdup.Template = nt;
          ntdup.DeclaringType = declaringType;
          ntdup.DeclaringModule = declaringType.DeclaringModule;
        }
      }
      for (int i = 0, n = types.Count; i < n; i++) {
        TypeNode nt = types[i];
        if (nt == null) continue;
        TypeNodeList nestedTypes = nt.NestedTypes;
        if (nestedTypes == null || nestedTypes.Count == 0) continue;
        TypeNode ntDup = dupTypes[i];
        if (ntDup == null) { Debug.Fail(""); continue; }
        Debug.Assert(ntDup != nt);
        this.VisitNestedTypes(ntDup, nestedTypes);
      }
      this.TargetType = savedTargetType;
      return dupTypes;
    }
    public override TypeNodeList VisitTypeNodeList(TypeNodeList types){
      if (types == null) return null;
      types = base.VisitTypeNodeList(types.Clone());
      if (this.TargetModule == null) return types;
      if (types == null) return null;
      if (this.TargetModule.Types == null) this.TargetModule.Types = new TypeNodeList();
      for (int i = 0, n = types.Count; i < n; i++)
        this.TargetModule.Types.Add(types[i]);
      return types;
    }
    public override TypeNode VisitTypeParameter(TypeNode typeParameter){
      if (typeParameter == null) return null;

      var dup = (TypeNode)this.DuplicateFor[typeParameter.UniqueKey];
      if (dup == null) {
        if (this.TypesToBeDuplicated[typeParameter.UniqueKey] != null) {

          dup = this.VisitTypeNode(typeParameter);
          TypeParameter tp = typeParameter as TypeParameter;
          if (tp != null) {
            var dupTP = (TypeParameter)dup;
            dupTP.structuralElementTypes = this.VisitTypeReferenceList(tp.StructuralElementTypes);
          } else {
            ClassParameter cp = typeParameter as ClassParameter;
            if (cp != null) {
              var dupCP = (ClassParameter)dup;
              dupCP.structuralElementTypes = this.VisitTypeReferenceList(cp.StructuralElementTypes);
            }
          }
        }
        return base.VisitTypeParameter(typeParameter);
      }
      return base.VisitTypeParameter(dup);
    }
    public override TypeNodeList VisitTypeParameterList(TypeNodeList typeParameters){
      if (typeParameters == null) return null;
      return base.VisitTypeParameterList(typeParameters.Clone());
    }
    public override TypeNode VisitTypeReference(TypeNode type){
      if (type == null) return null;
      TypeNode dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
      if (dup != null && (dup.Template != type || this.RecordOriginalAsTemplate)) return dup;
      if (this.RecordOriginalAsTemplate) {
          // [11/1/12 MAF: There was a bug that made it not possible to skip the copy here because generic methods have 
          // type parameters that needed to be duplicated including types instantiated with these type parameters, 
          // e.g.,  Task<T> Foo<T>().
          // Returning here had left Task<T> whereas the method has been changed to Task<T'>
          // Fixing this required not copying the generic method parameters when creating an instance, and instead copying
          // the template parameters during specializing. This fixed some bugs in the specializer too where we lost constraints
          // or mistakenly updated constraints of the generic type rather than the instance.
        return type; // mapping will be done by Specializer
      }
      switch (type.NodeType){
        case NodeType.ArrayType:
          ArrayType arrType = (ArrayType)type;
          TypeNode elemType = this.VisitTypeReference(arrType.ElementType);
          if (elemType == arrType.ElementType) return arrType;
          if (elemType == null) { Debug.Fail(""); return null; }
          //this.TypesToBeDuplicated[arrType.UniqueKey] = arrType;
          dup = elemType.GetArrayType(arrType.Rank, arrType.Sizes, arrType.LowerBounds);
          break;
        case NodeType.ClassParameter:
        case NodeType.TypeParameter:
          if (this.RecordOriginalAsTemplate) return type;
          if (this.TypesToBeDuplicated[type.UniqueKey] == null) return type;
          dup = this.VisitTypeNode(type);
          break;
#if !MinimalReader
        case NodeType.DelegateNode:{
          FunctionType ftype = type as FunctionType;
          if (ftype == null) goto default;
          dup = FunctionType.For(this.VisitTypeReference(ftype.ReturnType), this.VisitParameterList(ftype.Parameters), this.TargetType);
          break;}
#endif
        case NodeType.Pointer:
          Pointer pType = (Pointer)type;
          elemType = this.VisitTypeReference(pType.ElementType);
          if (elemType == pType.ElementType) return pType;
          if (elemType == null) { Debug.Fail(""); return null; }
          dup = elemType.GetPointerType();
          break;
        case NodeType.Reference:
          Reference rType = (Reference)type;
          elemType = this.VisitTypeReference(rType.ElementType);
          if (elemType == rType.ElementType) return rType;
          if (elemType == null) { Debug.Fail(""); return null; }
          dup = elemType.GetReferenceType();
          break;
#if ExtendedRuntime    
        case NodeType.TupleType:{
          TupleType tType = (TupleType)type;
          bool reconstruct = false;
          MemberList members = tType.Members;
          int n = members == null ? 0 : members.Count;
          FieldList fields = new FieldList(n);
          for (int i = 0; i < n; i++){
            //^ assert members != null;
            Field f = members[i] as Field;
            if (f == null) continue;
            f = (Field)f.Clone();
            fields.Add(f);
            TypeNode oft = f.Type;
            TypeNode ft = f.Type = this.VisitTypeReference(f.Type);
            if (ft != oft) reconstruct = true;
          }
          if (!reconstruct) return tType;
          dup = TupleType.For(fields, this.TargetType);
          break;}
        case NodeType.TypeIntersection:
          TypeIntersection tIntersect = (TypeIntersection)type;
          dup = TypeIntersection.For(this.VisitTypeReferenceList(tIntersect.Types), this.TargetType);
          break;
        case NodeType.TypeUnion:
          TypeUnion tUnion = (TypeUnion)type;
          TypeNodeList types = this.VisitTypeReferenceList(tUnion.Types);
          if (types == null) { Debug.Fail(""); return null; }
          if (this.TargetType == null)
            dup = TypeUnion.For(types, TargetModule);
          else
            dup = TypeUnion.For(types, this.TargetType);
          break;
#endif
#if !MinimalReader
        //These types typically have only one reference and do not have pointer identity. Just duplicate them.
        case NodeType.ArrayTypeExpression:
          ArrayTypeExpression aExpr = (ArrayTypeExpression)type.Clone();
          elemType = this.VisitTypeReference(aExpr.ElementType);
          if (elemType == null) { Debug.Fail(""); return aExpr; }
          aExpr.ElementType = elemType;
          return aExpr;
        case NodeType.BoxedTypeExpression:
          BoxedTypeExpression bExpr = (BoxedTypeExpression)type.Clone();
          bExpr.ElementType = this.VisitTypeReference(bExpr.ElementType);
          return bExpr;
        case NodeType.ClassExpression:
          ClassExpression cExpr = (ClassExpression)type.Clone();
          cExpr.Expression = this.VisitExpression(cExpr.Expression);
          cExpr.TemplateArguments = this.VisitTypeReferenceList(cExpr.TemplateArguments);
          return cExpr;   
#endif
#if ExtendedRuntime
        case NodeType.ConstrainedType:
          ConstrainedType conType = (ConstrainedType)type;
          TypeNode underlyingType = this.VisitTypeReference(conType.UnderlyingType);
          Expression constraint = this.VisitExpression(conType.Constraint);
          if (underlyingType == null || constraint == null) { Debug.Fail(""); return null; }
          if (this.TargetType == null)
            return null;
          else
            return new ConstrainedType(underlyingType, constraint, this.TargetType);
#endif
#if !MinimalReader        
        case NodeType.FlexArrayTypeExpression:
          FlexArrayTypeExpression flExpr = (FlexArrayTypeExpression)type.Clone();
          flExpr.ElementType = this.VisitTypeReference(flExpr.ElementType);
          return flExpr;
#endif
        case NodeType.FunctionPointer:
          FunctionPointer funcPointer = (FunctionPointer)type.Clone();
          funcPointer.ParameterTypes = this.VisitTypeReferenceList(funcPointer.ParameterTypes);
          funcPointer.ReturnType = this.VisitTypeReference(funcPointer.ReturnType);
          return funcPointer;
#if !MinimalReader
        case NodeType.FunctionTypeExpression:
          FunctionTypeExpression ftExpr = (FunctionTypeExpression)type.Clone();
          ftExpr.Parameters = this.VisitParameterList(ftExpr.Parameters);
          ftExpr.ReturnType = this.VisitTypeReference(ftExpr.ReturnType);
          return ftExpr;
        case NodeType.InvariantTypeExpression:
          InvariantTypeExpression invExpr = (InvariantTypeExpression)type.Clone();
          invExpr.ElementType = this.VisitTypeReference(invExpr.ElementType);
          return invExpr;
#endif
        case NodeType.InterfaceExpression:
          InterfaceExpression iExpr = (InterfaceExpression)type.Clone();
          iExpr.Expression = this.VisitExpression(iExpr.Expression);
          iExpr.TemplateArguments = this.VisitTypeReferenceList(iExpr.TemplateArguments);
          return iExpr;
#if !MinimalReader
        case NodeType.NonEmptyStreamTypeExpression:
          NonEmptyStreamTypeExpression neExpr = (NonEmptyStreamTypeExpression)type.Clone();
          neExpr.ElementType = this.VisitTypeReference(neExpr.ElementType);
          return neExpr;
        case NodeType.NonNullTypeExpression:
          NonNullTypeExpression nnExpr = (NonNullTypeExpression)type.Clone();
          nnExpr.ElementType = this.VisitTypeReference(nnExpr.ElementType);
          return nnExpr;
        case NodeType.NonNullableTypeExpression:
          NonNullableTypeExpression nbExpr = (NonNullableTypeExpression)type.Clone();
          nbExpr.ElementType = this.VisitTypeReference(nbExpr.ElementType);
          return nbExpr;
        case NodeType.NullableTypeExpression:
          NullableTypeExpression nuExpr = (NullableTypeExpression)type.Clone();
          nuExpr.ElementType = this.VisitTypeReference(nuExpr.ElementType);
          return nuExpr;
#endif
        case NodeType.OptionalModifier:
          TypeModifier modType = (TypeModifier)type;
          TypeNode modified = this.VisitTypeReference(modType.ModifiedType);
          TypeNode modifier = this.VisitTypeReference(modType.Modifier);
          if (modified == null || modifier == null) { Debug.Fail(""); return null; }
          return OptionalModifier.For(modifier, modified);
        case NodeType.RequiredModifier:
          modType = (TypeModifier)type;
          modified = this.VisitTypeReference(modType.ModifiedType);
          modifier = this.VisitTypeReference(modType.Modifier);
          if (modified == null || modifier == null) { Debug.Fail(""); return null; }
          return RequiredModifier.For(modifier, modified);
#if !MinimalReader && !CodeContracts
        case NodeType.OptionalModifierTypeExpression:
          OptionalModifierTypeExpression optmodType = (OptionalModifierTypeExpression)type.Clone();
          optmodType.ModifiedType = this.VisitTypeReference(optmodType.ModifiedType);
          optmodType.Modifier = this.VisitTypeReference(optmodType.Modifier);
          return optmodType;
        case NodeType.RequiredModifierTypeExpression:
          RequiredModifierTypeExpression reqmodType = (RequiredModifierTypeExpression)type.Clone();
          reqmodType.ModifiedType = this.VisitTypeReference(reqmodType.ModifiedType);
          reqmodType.Modifier = this.VisitTypeReference(reqmodType.Modifier);
          return reqmodType;
        case NodeType.PointerTypeExpression:
          PointerTypeExpression pExpr = (PointerTypeExpression)type.Clone();
          elemType = this.VisitTypeReference(pExpr.ElementType);
          if (elemType == null) { Debug.Fail(""); return pExpr; }
          pExpr.ElementType = elemType;
          return pExpr;
        case NodeType.ReferenceTypeExpression:
          ReferenceTypeExpression rExpr = (ReferenceTypeExpression)type.Clone();
          elemType = this.VisitTypeReference(rExpr.ElementType);
          if (elemType == null) { Debug.Fail(""); return rExpr; }
          rExpr.ElementType = elemType;
          return rExpr;
        case NodeType.StreamTypeExpression:
          StreamTypeExpression sExpr = (StreamTypeExpression)type.Clone();
          sExpr.ElementType = this.VisitTypeReference(sExpr.ElementType);
          return sExpr;
        case NodeType.TupleTypeExpression:
          TupleTypeExpression tuExpr = (TupleTypeExpression)type.Clone();
          tuExpr.Domains = this.VisitFieldList(tuExpr.Domains);
          return tuExpr;
        case NodeType.TypeExpression:
          TypeExpression tExpr = (TypeExpression)type.Clone();
          tExpr.Expression = this.VisitExpression(tExpr.Expression);
          tExpr.TemplateArguments = this.VisitTypeReferenceList(tExpr.TemplateArguments);
          return tExpr;
        case NodeType.TypeIntersectionExpression:
          TypeIntersectionExpression tiExpr = (TypeIntersectionExpression)type.Clone();
          tiExpr.Types = this.VisitTypeReferenceList(tiExpr.Types);
          return tiExpr;
        case NodeType.TypeUnionExpression:
          TypeUnionExpression tyuExpr = (TypeUnionExpression)type.Clone();
          tyuExpr.Types = this.VisitTypeReferenceList(tyuExpr.Types);
          return tyuExpr;
#endif
        default:
          if (type.Template != null)
          {
            var templ = type.Template;
            Debug.Assert(TypeNode.IsCompleteTemplate(templ));
            if (!this.RecordOriginalAsTemplate)
            {
              templ = this.VisitTemplateTypeReference(type.Template);
              Debug.Assert(templ.DeclaringType == null || type.Template.DeclaringType != null);
              Debug.Assert(templ.DeclaringType != null || type.Template.DeclaringType == null);
              Debug.Assert(TypeNode.IsCompleteTemplate(templ));
            }
            bool duplicateReference = templ != type.Template;
            var originalConsolidatedParameterCount = type.Template.ConsolidatedTemplateParameters.Count;
            var newConsolidatedParameterCount = templ.ConsolidatedTemplateParameters.Count;
            TypeNodeList targs;
            if (newConsolidatedParameterCount != originalConsolidatedParameterCount)
            {
              var missing = newConsolidatedParameterCount - originalConsolidatedParameterCount;
              Debug.Assert(missing > 0);
              Debug.Assert(duplicateReference);
              // prefill with new template parameters
              targs = new TypeNodeList(newConsolidatedParameterCount);
              for (int i = 0; i < newConsolidatedParameterCount; i++)
              {
                if (i < missing)
                {
                  targs.Add(templ.ConsolidatedTemplateParameters[i]);
                }
                else
                {
                  targs.Add(this.VisitTypeReference(type.ConsolidatedTemplateArguments[i - missing]));
                }
              }
            }
            else
            {
              targs = type.ConsolidatedTemplateArguments == null ? new TypeNodeList() : type.ConsolidatedTemplateArguments.Clone();
              for (int i = 0, n = targs == null ? 0 : targs.Count; i < n; i++)
              {
                TypeNode targ = targs[i];
                if (targ == null) continue;
                TypeNode targDup = this.VisitTypeReference(targ);
                if (targ != targDup) duplicateReference = true;
                targs[i] = targDup;
              }
            }
            if (!duplicateReference)
            {
              // cache translation
              Debug.Assert(this.TypesToBeDuplicated[type.UniqueKey] == null);
              this.DuplicateFor[type.UniqueKey] = type;
              return type;
            }
            dup = templ.GetGenericTemplateInstance(this.TargetModule, targs);
            Debug.Assert(dup != type);
            this.DuplicateFor[type.UniqueKey] = dup;
            return dup;
            
          }
          // old
#if false
          if (type.Template != null && type.Template != type && (type.TemplateArguments != null || 
          (!this.RecordOriginalAsTemplate && type.ConsolidatedTemplateArguments != null && type.ConsolidatedTemplateArguments.Count > 0))) {
            TypeNode templ = this.VisitTypeReference(type.Template);
            //^ assume templ != null;
            if (TargetPlatform.UseGenerics) {
              if (templ.Template != null) {
                if (this.RecordOriginalAsTemplate)
                  templ = templ.Template;
                else
                  templ = this.VisitTypeReference(templ.Template);
                //^ assume templ != null;
              }
              if (type.DeclaringType != null) {
                TypeNode declType = this.VisitTypeReference(type.DeclaringType);
                if (declType != null) {
                  TypeNode typeDup = declType.GetNestedType(type.Template.Name);
                  if (typeDup == null) {
                    //Can happen when templ is nested in a type that is still being duplicated
                    typeDup = (TypeNode)templ.Clone();
                    typeDup.DeclaringModule = this.TargetModule;
                    typeDup.Template = templ;
                    declType.NestedTypes.Add(typeDup);
                    templ = typeDup;
                  } else {
                    templ = typeDup;
                    if (templ.Template != null) {
                      if (this.RecordOriginalAsTemplate)
                        templ = templ.Template;
                      else {
                        if (templ.Template.DeclaringType == null)
                          templ.Template.DeclaringType = templ.DeclaringType.Template;
                        templ = this.VisitTypeReference(templ.Template);
                      }
                      //^ assume templ != null;
                    }
                  }
                }
              }
            }
            else
            {
              if (templ.Template != null)
              {
                // cache translation
                this.DuplicateFor[type.UniqueKey] = type;
                return type;
              }
            }
            bool duplicateReference = templ != type.Template;
            TypeNodeList targs = type.TemplateArguments == null ? new TypeNodeList() : type.TemplateArguments.Clone();
            if (!this.RecordOriginalAsTemplate)
              targs = type.ConsolidatedTemplateArguments == null ? new TypeNodeList() : type.ConsolidatedTemplateArguments.Clone();
            for (int i = 0, n = targs == null ? 0 : targs.Count; i < n; i++) {
              TypeNode targ = targs[i];
              if (targ == null) continue;
              TypeNode targDup = this.VisitTypeReference(targ);
              if (targ != targDup) duplicateReference = true;
              targs[i] = targDup;
            }
            if (!duplicateReference)
            {
              // cache translation
              this.DuplicateFor[type.UniqueKey] = type;
              return type;
            }
            if (!this.RecordOriginalAsTemplate)
              dup = templ.GetGenericTemplateInstance(this.TargetModule, targs);
            else
              dup = templ.GetTemplateInstance(this.TargetModule, this.TargetType, type.DeclaringType, targs);
            this.DuplicateFor[type.UniqueKey] = dup;
            return dup;
          }
#endif
          // Must be ground and not copied, so just return it.
          Debug.Assert(this.TypesToBeDuplicated[type.UniqueKey] == null);

          return type;
#if false
          Debug.Assert(type.DeclaringType == null || type.DeclaringType.Template == null);
          var declaringType = this.VisitTypeReference(type.DeclaringType);
          Debug.Assert(declaringType == null || declaringType.Template == null || this.RecordOriginalAsTemplate);

          if (declaringType != null && declaringType.Template != null)
          {
            // we are refering to a nested type that we are instantiating. This means the reference must be
            // used as a template and it stays as is.
            return type;
          }
          dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
          if (dup != null) break;
          // deal with non-generic nested type
          if (declaringType != null && declaringType != type.DeclaringType)
          {
            dup = declaringType.GetNestedType(type.Name);
            if (dup != null) break;
          }
          if (this.TypesToBeDuplicated[type.UniqueKey] == null)
          {
            dup = type;
            break;
          }
          // need to duplicate, but haven't yet
          TypeNode savedTargetType = this.TargetType;
          if (declaringType != null)
          {
            if (declaringType == type.DeclaringType) {
              //Trying to duplicate a nested type into a type that is not the duplicate of the declaring type.
              //In this case, type is being duplicated into the original target type.
              declaringType = this.OriginalTargetType;
            }
          }
          this.TargetType = declaringType;
          dup = (TypeNode)this.Visit(type);
          this.TargetType = savedTargetType;
          break;
#endif
      }
      Debug.Assert(this.TypesToBeDuplicated[type.UniqueKey] == null || type != dup);
      this.DuplicateFor[type.UniqueKey] = dup;
      return dup;
    }
    public virtual TypeNode VisitTemplateTypeReference(TypeNode type)
    {
      if (type == null) return null;
      Debug.Assert(TypeNode.IsCompleteTemplate(type));

      // template type refs can be dealt with as follows:
      //   if we are doing a template instantiation, then we always return the original.
      //   otherwise, if the template itself is to be duplicated, dup it, otherwise return original
      if (this.RecordOriginalAsTemplate) return type;
      var dup = (TypeNode)this.DuplicateFor[type.UniqueKey];
      if (dup != null) return dup;

      if (this.TypesToBeDuplicated[type.UniqueKey] != null)
      {
        Debug.Assert(false, "we already duped all types on entry");
        // dup it
        var savedTargetType = this.TargetType;
        this.TargetType = this.VisitTemplateTypeReference(type.DeclaringType);
        dup = this.VisitTypeNode(type, null, null, null, true);
        this.TargetType = savedTargetType;
        return dup;
      }
      return type;
    }
    public override TypeNodeList VisitTypeReferenceList(TypeNodeList typeReferences){
      if (typeReferences == null) return null;
      return base.VisitTypeReferenceList(typeReferences.Clone());
    }
    public override Expression VisitUnaryExpression(UnaryExpression unaryExpression){
      if (unaryExpression == null) return null;
      unaryExpression = (UnaryExpression)base.VisitUnaryExpression((UnaryExpression)unaryExpression.Clone());
      return unaryExpression;
    }
#if !MinimalReader && !CodeContracts
    public override Statement VisitVariableDeclaration(VariableDeclaration variableDeclaration){
      if (variableDeclaration == null) return null;
      return base.VisitVariableDeclaration((VariableDeclaration)variableDeclaration.Clone());
    }
    public override UsedNamespace VisitUsedNamespace(UsedNamespace usedNamespace){
      if (usedNamespace == null) return null;
      return base.VisitUsedNamespace((UsedNamespace)usedNamespace.Clone());
    }
    public override UsedNamespaceList VisitUsedNamespaceList(UsedNamespaceList usedNspaces){
      if (usedNspaces == null) return null;
      return base.VisitUsedNamespaceList(usedNspaces.Clone());
    }
    public override Statement VisitWhile(While While){
      if (While == null) return null;
      return base.VisitWhile((While)While.Clone());
    }
    public override Statement VisitYield(Yield Yield){
      if (Yield == null) return null;
      return base.VisitYield((Yield)Yield.Clone());
    }
#endif
#if ExtendedRuntime
    // query nodes
    public override Node VisitQueryAggregate(QueryAggregate qa){
      if (qa == null) return null;
      return base.VisitQueryAggregate((QueryAggregate)qa.Clone());
    }
    public override Node VisitQueryAlias(QueryAlias alias){
      if (alias == null) return null;
      return base.VisitQueryAlias((QueryAlias)alias.Clone());
    }
    public override Node VisitQueryAxis(QueryAxis axis){
      if (axis == null) return null;
      return base.VisitQueryAxis((QueryAxis)axis.Clone());
    }
    public override Node VisitQueryCommit(QueryCommit qc){
      if (qc == null) return null;
      return base.VisitQueryCommit((QueryCommit)qc.Clone());
    }
    public override Node VisitQueryContext(QueryContext context){
      if (context == null) return null;
      return base.VisitQueryContext((QueryContext)context.Clone());
    }
    public override Node VisitQueryDelete(QueryDelete delete){
      if (delete == null) return null;
      return base.VisitQueryDelete((QueryDelete)delete.Clone());
    }
    public override Node VisitQueryDifference(QueryDifference diff){
      if (diff == null) return null;
      return base.VisitQueryDifference((QueryDifference)diff.Clone());
    }
    public override Node VisitQueryDistinct(QueryDistinct distinct){
      if (distinct == null) return null;
      return base.VisitQueryDistinct((QueryDistinct)distinct.Clone());
    }
    public override Node VisitQueryExists(QueryExists exists){
      if (exists == null) return null;
      return base.VisitQueryExists((QueryExists)exists.Clone());
    }
    public override Node VisitQueryFilter(QueryFilter filter){
      if (filter == null) return null;
      return base.VisitQueryFilter((QueryFilter)filter.Clone());
    }
    public override Node VisitQueryGroupBy(QueryGroupBy groupby){
      if (groupby == null) return null;
      return base.VisitQueryGroupBy((QueryGroupBy)groupby.Clone());
    }
    public override Statement VisitQueryGeneratedType(QueryGeneratedType qgt){
      if (qgt == null) return null;
      return base.VisitQueryGeneratedType((QueryGeneratedType)qgt.Clone());
    }
    public override Node VisitQueryInsert(QueryInsert insert){
      if (insert == null) return null;
      return base.VisitQueryInsert((QueryInsert)insert.Clone());
    }
    public override Node VisitQueryIntersection(QueryIntersection intersection){
      if (intersection == null) return intersection;
      return base.VisitQueryIntersection((QueryIntersection)intersection.Clone());
    }
    public override Node VisitQueryIterator(QueryIterator xiterator){
      if (xiterator == null) return xiterator;
      return base.VisitQueryIterator((QueryIterator)xiterator.Clone());
    }
    public override Node VisitQueryJoin(QueryJoin join){
      if (join == null) return null;
      return base.VisitQueryJoin((QueryJoin)join.Clone());
    }
    public override Node VisitQueryLimit(QueryLimit limit){
      if (limit == null) return null;
      return base.VisitQueryLimit((QueryLimit)limit.Clone());
    }
    public override Node VisitQueryOrderBy(QueryOrderBy orderby){
      if (orderby == null) return null;
      return base.VisitQueryOrderBy((QueryOrderBy)orderby.Clone());
    }
    public override Node VisitQueryOrderItem(QueryOrderItem item){
      if (item == null) return null;
      return base.VisitQueryOrderItem((QueryOrderItem)item.Clone());
    }
    public override Node VisitQueryPosition(QueryPosition position){
      if (position == null) return null;
      return base.VisitQueryPosition((QueryPosition)position.Clone());
    }
    public override Node VisitQueryProject(QueryProject project){
      if (project == null) return null;
      return base.VisitQueryProject((QueryProject)project.Clone());
    }
    public override Node VisitQueryRollback(QueryRollback qr){
      if (qr == null) return null;
      return base.VisitQueryRollback((QueryRollback)qr.Clone());
    }
    public override Node VisitQueryQuantifier(QueryQuantifier qq){
      if (qq == null) return null;
      return base.VisitQueryQuantifier((QueryQuantifier)qq.Clone());
    }
    public override Node VisitQueryQuantifiedExpression(QueryQuantifiedExpression qqe){
      if (qqe == null) return null;
      return base.VisitQueryQuantifiedExpression((QueryQuantifiedExpression)qqe.Clone());
    }
    public override Node VisitQuerySelect(QuerySelect select){
      if (select == null) return null;
      return base.VisitQuerySelect((QuerySelect)select.Clone());
    }
    public override Node VisitQuerySingleton(QuerySingleton singleton){
      if (singleton == null) return null;
      return base.VisitQuerySingleton((QuerySingleton)singleton.Clone());
    }
    public override Node VisitQueryTransact(QueryTransact qt){
      if (qt == null) return null;
      return base.VisitQueryTransact((QueryTransact)qt.Clone());
    }
    public override Node VisitQueryTypeFilter(QueryTypeFilter filter){
      if (filter == null) return null;
      return base.VisitQueryTypeFilter((QueryTypeFilter)filter.Clone());
    }
    public override Node VisitQueryUnion(QueryUnion union){
      if (union == null) return null;
      return base.VisitQueryUnion((QueryUnion)union.Clone());
    }
    public override Node VisitQueryUpdate(QueryUpdate update){
      if (update == null) return null;
      return base.VisitQueryUpdate((QueryUpdate)update.Clone());
    }    
    public override Node VisitQueryYielder(QueryYielder yielder){
      if (yielder == null) return null;
      return base.VisitQueryYielder((QueryYielder)yielder.Clone());
    }
#endif
  }
}
