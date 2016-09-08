#if !MinimalReader && !CodeContracts
using System;
using System.Diagnostics;

#if CCINamespace
namespace Microsoft.Cci{
#else
namespace System.Compiler{
#endif
  /// <summary>
  /// Performs a deep value comparison between two IR trees of the same type.
  /// </summary>
  public class Comparer{
    public Comparer CallingVisitor;
    public TrivialHashtable MemberMapping;
    public MemberList MembersThatHaveChanged;
    public bool DoNotCompareBodies;
    public Module OriginalModule;
    public Module NewModule;

    public Comparer(){
    }

    public Comparer(Comparer callingVisitor){
      this.CallingVisitor = callingVisitor;      
    }
    public virtual void TransferStateTo(Comparer targetVisitor){
      if (targetVisitor == null) return;
      targetVisitor.MemberMapping = this.MemberMapping;
      targetVisitor.MembersThatHaveChanged = this.MembersThatHaveChanged;
      targetVisitor.DoNotCompareBodies = this.DoNotCompareBodies;
      targetVisitor.OriginalModule = this.OriginalModule;
      targetVisitor.NewModule = this.NewModule;
    }

    public virtual bool ValuesAreEqual(byte[] array1, byte[] array2){
      if (array1 == array2) return true;
      if (array1 == null || array2 == null) return false;
      int n = array1.Length; if (n != array2.Length) return false;
      for (int i = 0; i < n; i++) if (array1[i] != array2[i]) return false;
      return true;
    }
    public virtual bool ValuesAreEqual(MarshallingInformation mi1, MarshallingInformation mi2){
      if (mi1 == mi2) return true;
      if (mi1 == null || mi2 == null) return false;
      if (mi1.Class != mi2.Class) return false;
      if (mi1.Cookie != mi2.Cookie) return false;
      if (mi1.ElementSize != mi2.ElementSize) return false;
      if (mi1.ElementType != mi2.ElementType) return false;
      if (mi1.NativeType != mi2.NativeType) return false;
      if (mi1.NumberOfElements != mi2.NumberOfElements) return false;
      if (mi1.ParamIndex != mi2.ParamIndex) return false;
      if (mi1.Size != mi2.Size) return false;
      return true;
    }
    public virtual AttributeNode GetClosestMatch(AttributeNode/*!*/ nd1, AttributeList/*!*/ list1, AttributeList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null ||  list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      AttributeNode closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        AttributeNode nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          AttributeNode nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          AttributeNode nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          AttributeNode newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual Block GetClosestMatch(Block/*!*/ nd1, BlockList/*!*/ list1, BlockList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      Block closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        Block nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          Block nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          Block nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          Block newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual Catch GetClosestMatch(Catch/*!*/ nd1, CatchList/*!*/ list1, CatchList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      Catch closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        Catch nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          Catch nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          Catch nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          Catch newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual Expression GetClosestMatch(Expression/*!*/ nd1, ExpressionList/*!*/ list1, ExpressionList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      Expression closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        Expression nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          Expression nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          Expression nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          Expression newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual FaultHandler GetClosestMatch(FaultHandler/*!*/ nd1, FaultHandlerList/*!*/ list1, FaultHandlerList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      FaultHandler closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        FaultHandler nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          FaultHandler nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          FaultHandler nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          FaultHandler newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual Filter GetClosestMatch(Filter/*!*/ nd1, FilterList/*!*/ list1, FilterList list2, int list1pos, ref int list2start,
      TrivialHashtable matchedNodes, out Differences closestDifferences, out int list2pos){
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      Filter closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        Filter nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          Filter nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          Filter nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          Filter newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual LocalDeclaration GetClosestMatch(LocalDeclaration/*!*/ nd1, LocalDeclarationList/*!*/ list1, LocalDeclarationList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      LocalDeclaration closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        LocalDeclaration nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          LocalDeclaration nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          LocalDeclaration nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          LocalDeclaration newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual Node GetClosestMatch(Node/*!*/ nd1, NodeList/*!*/ list1, NodeList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      Node closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        Node nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          Node nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          Node nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          Node newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual SecurityAttribute GetClosestMatch(SecurityAttribute/*!*/ nd1, SecurityAttributeList/*!*/ list1, SecurityAttributeList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      SecurityAttribute closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        SecurityAttribute nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          SecurityAttribute nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          SecurityAttribute nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          SecurityAttribute newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual Statement GetClosestMatch(Statement/*!*/ nd1, StatementList/*!*/ list1, StatementList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      Statement closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        Statement nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          Statement nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          Statement nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          Statement newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual SwitchCase GetClosestMatch(SwitchCase/*!*/ nd1, SwitchCaseList/*!*/ list1, SwitchCaseList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null ||  matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      SwitchCase closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        SwitchCase nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          SwitchCase nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          SwitchCase nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          SwitchCase newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual TypeswitchCase GetClosestMatch(TypeswitchCase/*!*/ nd1, TypeswitchCaseList/*!*/ list1, TypeswitchCaseList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null ||  matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      TypeswitchCase closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        TypeswitchCase nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          TypeswitchCase nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          TypeswitchCase nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          TypeswitchCase newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    public virtual UsedNamespace GetClosestMatch(UsedNamespace/*!*/ nd1, UsedNamespaceList/*!*/ list1, UsedNamespaceList list2, int list1pos, ref int list2start,
      TrivialHashtable/*!*/ matchedNodes, out Differences closestDifferences, out int list2pos) {
      closestDifferences = null; list2pos = -1;
      if (list2 == null) return null;
      if (nd1 == null || list1 == null || matchedNodes == null || list1pos < 0 || list1pos >= list1.Count || list2start < 0 || list2start >= list2.Count) {
        Debug.Assert(false); return null;
      }
      UsedNamespace closest = null;
      Differences winnerSoFar = null;
      for (int j = list2start, m = list2.Count; j < m; j++){
        UsedNamespace nd2 = list2[j];
        if (list2start == j) list2start++;
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        Differences diff = this.GetDifferences(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.Similarity <= 0.5){
          //Not a good enough match
          if (list2start == j+1) list2start--; //The next call to GetClosestMatch will start looking at list2start, so this node will be considered then
          continue; //ignore it for the rest of this call
        }
        if (winnerSoFar != null && winnerSoFar.Similarity >= diff.Similarity) continue;
        winnerSoFar = closestDifferences = diff;
        closest = nd2;
        list2pos = j;
        if (diff.NumberOfDifferences == 0) return closest; //Perfect match, no need to look for other matches
      }
      if (closest != null){
        //^ assert winnerSoFar != null;
        //closest is closer to nd1 than any other node in list2, but this is no good if some other node in list1 has a better claim on closest
        for (int i = list1pos+1, n = list1.Count; i < n; i++){
          UsedNamespace nd1alt = list1[i];
          if (nd1alt == null) continue;
          if (matchedNodes[nd1alt.UniqueKey] != null) continue;
          Differences diff = this.GetDifferences(nd1alt, closest);
          if (diff == null){Debug.Assert(false); continue;}
          if (diff.Similarity <= winnerSoFar.Similarity) continue;
          //nd1alt has a better claim on closest. See if it wants closest.
          Differences diff2;
          int j, k = list2start;
          UsedNamespace nd2alt = this.GetClosestMatch(nd1alt, list1, list2, i, ref k,  matchedNodes, out diff2, out j);
          if (nd2alt != closest){
            Debug.Assert(nd2alt != null && diff2 != null && diff2.Similarity >= diff.Similarity);
            continue; //nd1alt prefers nd2alt to closest, so closest is still available
          }
          //nd1alt wants closest, take it out of the running
          matchedNodes[closest.UniqueKey] = nd1alt;
          //Now that closest is out of the running, try again
          k = list2start;
          UsedNamespace newClosest = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out winnerSoFar, out list2pos);
          //put closest back in the running so that the next call to this routine will pick it up
          matchedNodes[closest.UniqueKey] = closest;
          closest = newClosest;
          break;
        }
      }
      closestDifferences = winnerSoFar;
      return closest;
    }
    protected TrivialHashtable differencesMapFor;
    /// <summary>
    /// Gets a difference object representing the differences between node1 and node2. Caches the result and returns it for subsequent calls.
    /// I.e. this is a caching factory method for Differences instances. Calls GetNewDifferences to construct new instances when needed.
    /// </summary>
    public virtual Differences GetDifferences(Node/*!*/ node1, Node node2) {
      if (node1 == null || node2 == null) return new Differences(node1, node2);
      if (this.differencesMapFor == null) this.differencesMapFor = new TrivialHashtable();
      TrivialHashtable map = this.differencesMapFor[node1.UniqueKey] as TrivialHashtable;
      if (map == null) this.differencesMapFor[node1.UniqueKey] = map = new TrivialHashtable();
      Differences differences = map[node2.UniqueKey] as Differences;
      if (differences == null){
        map[node2.UniqueKey] = differences = this.Visit(node1, node2);
        if (differences != null && differences.NumberOfDifferences > 0 && differences.Changes == null)
          differences.Changes = node2;
      }
      return differences;
    }
    protected TrivialHashtable memberDifferencesMapFor;
    public virtual Differences GetMemberDifferences(Member member1, Member member2) {
      if (member1 == null || member2 == null) return new Differences(member1, member2);
      if (this.memberDifferencesMapFor == null) this.memberDifferencesMapFor = new TrivialHashtable();
      TrivialHashtable map = this.memberDifferencesMapFor[member1.UniqueKey] as TrivialHashtable;
      if (map == null) this.memberDifferencesMapFor[member1.UniqueKey] = map = new TrivialHashtable();
      Differences differences = map[member2.UniqueKey] as Differences;
      if (differences == null){
        map[member2.UniqueKey] = differences = new Differences(member1, member2);
        if (member1 == member2) differences.NumberOfSimilarities++;
      }
      return differences;
    }
    public virtual Comparer GetVisitorFor(Node/*!*/ node1){
      object visitor = node1.GetVisitorFor(this, this.GetType().Name);
      if (visitor != null) Debug.Assert(visitor is Comparer);
      return visitor as Comparer;
    }
    public virtual Differences Visit(Node node1, Node node2) {
      if (node1 == null){
        Differences differences = new Differences(null, node2);
        if (node2 != null) differences.NumberOfDifferences++;
        return differences;
      }
      switch (node1.NodeType){
        case NodeType.AddressDereference:
          return this.VisitAddressDereference((AddressDereference)node1, node2 as AddressDereference);
        case NodeType.AliasDefinition :
          return this.VisitAliasDefinition((AliasDefinition)node1, node2 as AliasDefinition);
        case NodeType.AnonymousNestedFunction:
          return this.VisitAnonymousNestedFunction((AnonymousNestedFunction)node1, node2 as AnonymousNestedFunction);
        case NodeType.ApplyToAll :
          return this.VisitApplyToAll((ApplyToAll)node1, node2 as ApplyToAll);
        case NodeType.Arglist :
          return this.VisitExpression((Expression)node1, node2 as Expression);
        case NodeType.ArrayType : 
          Debug.Assert(false); return null;
        case NodeType.Assembly : 
          return this.VisitAssembly((AssemblyNode)node1, node2 as AssemblyNode);
        case NodeType.AssemblyReference :
          return this.VisitAssemblyReference((AssemblyReference)node1, node2 as AssemblyReference);
        case NodeType.Assertion:
          return this.VisitAssertion((Assertion)node1, node2 as Assertion);
        case NodeType.AssignmentExpression:
          return this.VisitAssignmentExpression((AssignmentExpression)node1, node2 as AssignmentExpression);
        case NodeType.AssignmentStatement : 
          return this.VisitAssignmentStatement((AssignmentStatement)node1, node2 as AssignmentStatement);
        case NodeType.Attribute :
          return this.VisitAttributeNode((AttributeNode)node1, node2 as AttributeNode);
        case NodeType.Base :
          return this.VisitBase((Base)node1, node2 as Base);
        case NodeType.Block : 
          return this.VisitBlock((Block)node1, node2 as Block);
        case NodeType.BlockExpression :
          return this.VisitBlockExpression((BlockExpression)node1, node2 as BlockExpression);
        case NodeType.Branch :
          Debug.Assert(false); return null;
        case NodeType.Compilation:
          return this.VisitCompilation((Compilation)node1, node2 as Compilation);
        case NodeType.CompilationUnit:
          return this.VisitCompilationUnit((CompilationUnit)node1, node2 as CompilationUnit);
        case NodeType.CompilationUnitSnippet:
          return this.VisitCompilationUnitSnippet((CompilationUnitSnippet)node1, node2 as CompilationUnitSnippet);
#if ExtendedRuntime
        case NodeType.ConstrainedType:
          return this.VisitConstrainedType((ConstrainedType)node1, node2 as ConstrainedType);
#endif
        case NodeType.Continue :
          return this.VisitContinue((Continue)node1, node2 as Continue);
        case NodeType.CurrentClosure :
          return this.VisitCurrentClosure((CurrentClosure)node1, node2 as CurrentClosure);
        case NodeType.DebugBreak :
          return null;
        case NodeType.Call :
        case NodeType.Calli :
        case NodeType.Callvirt :
        case NodeType.Jmp :
        case NodeType.MethodCall :
          return this.VisitMethodCall((MethodCall)node1, node2 as MethodCall);
        case NodeType.Catch :
          return this.VisitCatch((Catch)node1, node2 as Catch);
        case NodeType.Class :
          if (node2 is Class)
            return this.VisitClass((Class)node1, node2 as Class);
          else
            return this.VisitTypeNode((TypeNode)node1, node2 as TypeNode);
        case NodeType.CoerceTuple :
          return this.VisitCoerceTuple((CoerceTuple)node1, node2 as CoerceTuple);
        case NodeType.CollectionEnumerator :
          return this.VisitCollectionEnumerator((CollectionEnumerator)node1, node2 as CollectionEnumerator);
        case NodeType.Composition :
          return this.VisitComposition((Composition)node1, node2 as Composition);
        case NodeType.Comprehension: 
          return this.VisitComprehension((Comprehension)node1, (Comprehension)node2);
        case NodeType.ComprehensionBinding: 
          return this.VisitComprehensionBinding((ComprehensionBinding)node1, (ComprehensionBinding)node2);
        case NodeType.Construct :
          return this.VisitConstruct((Construct)node1, node2 as Construct);
        case NodeType.ConstructArray :
          return this.VisitConstructArray((ConstructArray)node1, node2 as ConstructArray);
        case NodeType.ConstructDelegate :
          return this.VisitConstructDelegate((ConstructDelegate)node1, node2 as ConstructDelegate);
        case NodeType.ConstructFlexArray :
          return this.VisitConstructFlexArray((ConstructFlexArray)node1, node2 as ConstructFlexArray);
        case NodeType.ConstructIterator :
          return this.VisitConstructIterator((ConstructIterator)node1, node2 as ConstructIterator);
        case NodeType.ConstructTuple :
          return this.VisitConstructTuple((ConstructTuple)node1, node2 as ConstructTuple);
        case NodeType.DelegateNode :
          return this.VisitDelegateNode((DelegateNode)node1, node2 as DelegateNode);
        case NodeType.DoWhile:
          return this.VisitDoWhile((DoWhile)node1, node2 as DoWhile);
        case NodeType.Dup :
          return this.VisitExpression((Expression)node1, node2 as Expression);
        case NodeType.EndFilter :
          return this.VisitEndFilter((EndFilter)node1, node2 as EndFilter);
        case NodeType.EndFinally:
          return this.VisitEndFinally((EndFinally)node1, node2 as EndFinally);
        case NodeType.EnumNode :
          return this.VisitEnumNode((EnumNode)node1, node2 as EnumNode);
        case NodeType.Event: 
          return this.VisitEvent((Event)node1, node2 as Event);
        case NodeType.Exit :
          return this.VisitExit((Exit)node1, node2 as Exit);
        case NodeType.ExpressionSnippet:
          return this.VisitExpressionSnippet((ExpressionSnippet)node1, node2 as ExpressionSnippet);
        case NodeType.ExpressionStatement :
          return this.VisitExpressionStatement((ExpressionStatement)node1, node2 as ExpressionStatement);
        case NodeType.FaultHandler :
          return this.VisitFaultHandler((FaultHandler)node1, node2 as FaultHandler);
        case NodeType.Field :
          return this.VisitField((Field)node1, node2 as Field);
        case NodeType.FieldInitializerBlock:
          return this.VisitFieldInitializerBlock((FieldInitializerBlock)node1, node2 as FieldInitializerBlock);
        case NodeType.Finally :
          return this.VisitFinally((Finally)node1, node2 as Finally);
        case NodeType.Filter :
          return this.VisitFilter((Filter)node1, node2 as Filter);
        case NodeType.Fixed :
          return this.VisitFixed((Fixed)node1, node2 as Fixed);
        case NodeType.For :
          return this.VisitFor((For)node1, node2 as For);
        case NodeType.ForEach :
          return this.VisitForEach((ForEach)node1, node2 as ForEach);
        case NodeType.FunctionDeclaration:
          return this.VisitFunctionDeclaration((FunctionDeclaration)node1, node2 as FunctionDeclaration);
        case NodeType.Goto :
          return this.VisitGoto((Goto)node1, node2 as Goto);
        case NodeType.GotoCase :
          return this.VisitGotoCase((GotoCase)node1, node2 as GotoCase);
        case NodeType.Identifier :
          return this.VisitIdentifier((Identifier)node1, node2 as Identifier);
        case NodeType.If :
          return this.VisitIf((If)node1, node2 as If);
        case NodeType.ImplicitThis :
          return this.VisitImplicitThis((ImplicitThis)node1, node2 as ImplicitThis);
        case NodeType.Indexer :
          return this.VisitIndexer((Indexer)node1, node2 as Indexer);
        case NodeType.InstanceInitializer :
          return this.VisitInstanceInitializer((InstanceInitializer)node1, node2 as InstanceInitializer);
        case NodeType.StaticInitializer :
          return this.VisitStaticInitializer((StaticInitializer)node1, node2 as StaticInitializer);
        case NodeType.Method: 
          return this.VisitMethod((Method)node1, node2 as Method);
        case NodeType.Interface :
          if (node2 is Interface)
            return this.VisitInterface((Interface)node1, node2 as Interface);
          else
            return this.VisitTypeNode((TypeNode)node1, node2 as TypeNode);
        case NodeType.LabeledStatement :
          return this.VisitLabeledStatement((LabeledStatement)node1, node2 as LabeledStatement);
        case NodeType.Literal:
          return this.VisitLiteral((Literal)node1, node2 as Literal);
        case NodeType.Local :
          return this.VisitLocal((Local)node1, node2 as Local);
        case NodeType.LocalDeclaration:
          return this.VisitLocalDeclaration((LocalDeclaration)node1, node2 as LocalDeclaration);
        case NodeType.LocalDeclarationsStatement:
          return this.VisitLocalDeclarationsStatement((LocalDeclarationsStatement)node1, node2 as LocalDeclarationsStatement);
        case NodeType.Lock:
          return this.VisitLock((Lock)node1, node2 as Lock);
        case NodeType.LRExpression:
          return this.VisitLRExpression((LRExpression)node1, node2 as LRExpression);
        case NodeType.MemberBinding :
          return this.VisitMemberBinding((MemberBinding)node1, node2 as MemberBinding);
        case NodeType.TemplateInstance:
          return this.VisitTemplateInstance((TemplateInstance)node1, node2 as TemplateInstance);
        case NodeType.StackAlloc:
          return this.VisitStackAlloc((StackAlloc)node1, node2 as StackAlloc);
        case NodeType.Module :
          return this.VisitModule((Module)node1, node2 as Module);
        case NodeType.ModuleReference :
          return this.VisitModuleReference((ModuleReference)node1, node2 as ModuleReference);
        case NodeType.NameBinding :
          return this.VisitNameBinding((NameBinding)node1, node2 as NameBinding);
        case NodeType.NamedArgument :
          return this.VisitNamedArgument((NamedArgument)node1, node2 as NamedArgument);
        case NodeType.Namespace :
          return this.VisitNamespace((Namespace)node1, node2 as Namespace);
        case NodeType.Nop :
          return null;
        case NodeType.OptionalModifier:
        case NodeType.RequiredModifier:
          return this.VisitTypeModifier((TypeModifier)node1, node2 as TypeModifier);
        case NodeType.Parameter :
          return this.VisitParameter((Parameter)node1, node2 as Parameter);
        case NodeType.Pop :
          return this.VisitExpression((Expression)node1, node2 as Expression);
        case NodeType.PrefixExpression:
          return this.VisitPrefixExpression((PrefixExpression)node1, node2 as PrefixExpression);
        case NodeType.PostfixExpression:
          return this.VisitPostfixExpression((PostfixExpression)node1, node2 as PostfixExpression);
        case NodeType.Property: 
          return this.VisitProperty((Property)node1, node2 as Property);
        case NodeType.QualifiedIdentifer :
          return this.VisitQualifiedIdentifier((QualifiedIdentifier)node1, node2 as QualifiedIdentifier);
        case NodeType.Rethrow :
        case NodeType.Throw :
          return this.VisitThrow((Throw)node1, node2 as Throw);
        case NodeType.Return:
          return this.VisitReturn((Return)node1, node2 as Return);
        case NodeType.Repeat:
          return this.VisitRepeat((Repeat)node1, node2 as Repeat);
        case NodeType.ResourceUse:
          return this.VisitResourceUse((ResourceUse)node1, node2 as ResourceUse);
        case NodeType.SecurityAttribute:
          return this.VisitSecurityAttribute((SecurityAttribute)node1, node2 as SecurityAttribute);
        case NodeType.SetterValue:
          return this.VisitSetterValue((SetterValue)node1, node2 as SetterValue);
        case NodeType.StatementSnippet:
          return this.VisitStatementSnippet((StatementSnippet)node1, node2 as StatementSnippet);
        case NodeType.Struct :
          if (node2 is Struct)
            return this.VisitStruct((Struct)node1, node2 as Struct);
          else
            return this.VisitTypeNode((TypeNode)node1, node2 as TypeNode);
        case NodeType.Switch :
          return this.VisitSwitch((Switch)node1, node2 as Switch);
        case NodeType.SwitchCase :
          return this.VisitSwitchCase((SwitchCase)node1, node2 as SwitchCase);
        case NodeType.SwitchInstruction :
          return this.VisitSwitchInstruction((SwitchInstruction)node1, node2 as SwitchInstruction);
        case NodeType.Typeswitch :
          return this.VisitTypeswitch((Typeswitch)node1, node2 as Typeswitch);
        case NodeType.TypeswitchCase :
          return this.VisitTypeswitchCase((TypeswitchCase)node1, node2 as TypeswitchCase);
        case NodeType.This :
          return this.VisitThis((This)node1, node2 as This);
        case NodeType.Try :
          return this.VisitTry((Try)node1, node2 as Try);
#if ExtendedRuntime
        case NodeType.TupleType:
          return this.VisitTupleType((TupleType)node1, node2 as TupleType);
        case NodeType.TypeAlias:
          return this.VisitTypeAlias((TypeAlias)node1, node2 as TypeAlias);
        case NodeType.TypeIntersection:
          return this.VisitTypeIntersection((TypeIntersection)node1, node2 as TypeIntersection);
#endif
        case NodeType.TypeMemberSnippet:
          return this.VisitTypeMemberSnippet((TypeMemberSnippet)node1, node2 as TypeMemberSnippet);
        case NodeType.ClassParameter:
        case NodeType.TypeParameter:
          return this.VisitTypeNode((TypeNode)node1, node2 as TypeNode);
#if ExtendedRuntime
        case NodeType.TypeUnion:
          return this.VisitTypeUnion((TypeUnion)node1, node2 as TypeUnion);
#endif
        case NodeType.TypeReference:
          return this.VisitTypeReference((TypeReference)node1, node2 as TypeReference);
        case NodeType.UsedNamespace :
          return this.VisitUsedNamespace((UsedNamespace)node1, node2 as UsedNamespace);
        case NodeType.VariableDeclaration:
          return this.VisitVariableDeclaration((VariableDeclaration)node1, node2 as VariableDeclaration);
        case NodeType.While:
          return this.VisitWhile((While)node1, node2 as While);
        case NodeType.Yield:
          return this.VisitYield((Yield)node1, node2 as Yield);

        case NodeType.Conditional :
        case NodeType.Cpblk :
        case NodeType.Initblk :
          return this.VisitTernaryExpression((TernaryExpression)node1, node2 as TernaryExpression);

        case NodeType.Add : 
        case NodeType.Add_Ovf : 
        case NodeType.Add_Ovf_Un : 
        case NodeType.AddEventHandler :
        case NodeType.And : 
        case NodeType.As :
        case NodeType.Box :
        case NodeType.Castclass : 
        case NodeType.Ceq : 
        case NodeType.Cgt : 
        case NodeType.Cgt_Un : 
        case NodeType.Clt : 
        case NodeType.Clt_Un : 
        case NodeType.Comma :
        case NodeType.Div : 
        case NodeType.Div_Un : 
        case NodeType.Eq : 
        case NodeType.ExplicitCoercion :
        case NodeType.Ge : 
        case NodeType.Gt : 
        case NodeType.Is : 
        case NodeType.Iff : 
        case NodeType.Implies : 
        case NodeType.Isinst : 
        case NodeType.Ldvirtftn :
        case NodeType.Le : 
        case NodeType.LogicalAnd :
        case NodeType.LogicalOr :
        case NodeType.Lt : 
        case NodeType.Mkrefany :
        case NodeType.Mul : 
        case NodeType.Mul_Ovf : 
        case NodeType.Mul_Ovf_Un : 
        case NodeType.Ne : 
        case NodeType.Or : 
        case NodeType.Range :
        case NodeType.Refanyval :
        case NodeType.Rem : 
        case NodeType.Rem_Un : 
        case NodeType.RemoveEventHandler :
        case NodeType.Shl : 
        case NodeType.Shr : 
        case NodeType.Shr_Un : 
        case NodeType.Sub : 
        case NodeType.Sub_Ovf : 
        case NodeType.Sub_Ovf_Un : 
        case NodeType.Unbox : 
        case NodeType.UnboxAny :
        case NodeType.Xor : 
          return this.VisitBinaryExpression((BinaryExpression)node1, node2 as BinaryExpression);
        
        case NodeType.AddressOf:
        case NodeType.OutAddress:
        case NodeType.RefAddress:
        case NodeType.Ckfinite :
        case NodeType.Conv_I :
        case NodeType.Conv_I1 :
        case NodeType.Conv_I2 :
        case NodeType.Conv_I4 :
        case NodeType.Conv_I8 :
        case NodeType.Conv_Ovf_I :
        case NodeType.Conv_Ovf_I1 :
        case NodeType.Conv_Ovf_I1_Un :
        case NodeType.Conv_Ovf_I2 :
        case NodeType.Conv_Ovf_I2_Un :
        case NodeType.Conv_Ovf_I4 :
        case NodeType.Conv_Ovf_I4_Un :
        case NodeType.Conv_Ovf_I8 :
        case NodeType.Conv_Ovf_I8_Un :
        case NodeType.Conv_Ovf_I_Un :
        case NodeType.Conv_Ovf_U :
        case NodeType.Conv_Ovf_U1 :
        case NodeType.Conv_Ovf_U1_Un :
        case NodeType.Conv_Ovf_U2 :
        case NodeType.Conv_Ovf_U2_Un :
        case NodeType.Conv_Ovf_U4 :
        case NodeType.Conv_Ovf_U4_Un :
        case NodeType.Conv_Ovf_U8 :
        case NodeType.Conv_Ovf_U8_Un :
        case NodeType.Conv_Ovf_U_Un :
        case NodeType.Conv_R4 :
        case NodeType.Conv_R8 :
        case NodeType.Conv_R_Un :
        case NodeType.Conv_U :
        case NodeType.Conv_U1 :
        case NodeType.Conv_U2 :
        case NodeType.Conv_U4 :
        case NodeType.Conv_U8 :
        case NodeType.Decrement :
        case NodeType.DefaultValue:
        case NodeType.Increment :
        case NodeType.Ldftn :
        case NodeType.Ldlen :
        case NodeType.Ldtoken :
        case NodeType.Localloc :
        case NodeType.LogicalNot :
        case NodeType.Neg :
        case NodeType.Not :
        case NodeType.Parentheses :
        case NodeType.Refanytype :
        case NodeType.Sizeof :
        case NodeType.SkipCheck :
        case NodeType.Typeof :
        case NodeType.UnaryPlus :
          return this.VisitUnaryExpression((UnaryExpression)node1, node2 as UnaryExpression);
#if ExtendedRuntime
          // query node1 types
        case NodeType.QueryAggregate:
          return this.VisitQueryAggregate((QueryAggregate)node1, node2 as QueryAggregate);
        case NodeType.QueryAlias:
          return this.VisitQueryAlias((QueryAlias)node1, node2 as QueryAlias);
        case NodeType.QueryAll:
        case NodeType.QueryAny:
          return this.VisitQueryQuantifier((QueryQuantifier)node1, node2 as QueryQuantifier);
        case NodeType.QueryAxis:
          return this.VisitQueryAxis((QueryAxis)node1, node2 as QueryAxis);
        case NodeType.QueryCommit:
          return this.VisitQueryCommit((QueryCommit)node1, node2 as QueryCommit);
        case NodeType.QueryContext:
          return this.VisitQueryContext((QueryContext)node1, node2 as QueryContext);
        case NodeType.QueryDelete:
          return this.VisitQueryDelete((QueryDelete)node1, node2 as QueryDelete);
        case NodeType.QueryDifference:
          return this.VisitQueryDifference((QueryDifference)node1, node2 as QueryDifference);
        case NodeType.QueryDistinct:
          return this.VisitQueryDistinct((QueryDistinct)node1, node2 as QueryDistinct);
        case NodeType.QueryExists:
          return this.VisitQueryExists((QueryExists)node1, node2 as QueryExists);
        case NodeType.QueryFilter:
          return this.VisitQueryFilter((QueryFilter)node1, node2 as QueryFilter);
        case NodeType.QueryGeneratedType:
          return this.VisitQueryGeneratedType((QueryGeneratedType)node1, node2 as QueryGeneratedType);
        case NodeType.QueryGroupBy:
          return this.VisitQueryGroupBy((QueryGroupBy)node1, node2 as QueryGroupBy);
        case NodeType.QueryInsert:
          return this.VisitQueryInsert((QueryInsert)node1, node2 as QueryInsert);
        case NodeType.QueryIntersection:
          return this.VisitQueryIntersection((QueryIntersection)node1, node2 as QueryIntersection);
        case NodeType.QueryIterator:
          return this.VisitQueryIterator((QueryIterator)node1, node2 as QueryIterator);
        case NodeType.QueryJoin:
          return this.VisitQueryJoin((QueryJoin)node1, node2 as QueryJoin);
        case NodeType.QueryLimit:
          return this.VisitQueryLimit((QueryLimit)node1, node2 as QueryLimit);
        case NodeType.QueryOrderBy:        
          return this.VisitQueryOrderBy((QueryOrderBy)node1, node2 as QueryOrderBy);
        case NodeType.QueryOrderItem:
          return this.VisitQueryOrderItem((QueryOrderItem)node1, node2 as QueryOrderItem);
        case NodeType.QueryPosition:
          return this.VisitQueryPosition((QueryPosition)node1, node2 as QueryPosition);
        case NodeType.QueryProject:
          return this.VisitQueryProject((QueryProject)node1, node2 as QueryProject);          
        case NodeType.QueryQuantifiedExpression:
          return this.VisitQueryQuantifiedExpression((QueryQuantifiedExpression)node1, node2 as QueryQuantifiedExpression);
        case NodeType.QueryRollback:
          return this.VisitQueryRollback((QueryRollback)node1, node2 as QueryRollback);
        case NodeType.QuerySelect:
          return this.VisitQuerySelect((QuerySelect)node1, node2 as QuerySelect);
        case NodeType.QuerySingleton:
          return this.VisitQuerySingleton((QuerySingleton)node1, node2 as QuerySingleton);
        case NodeType.QueryTransact:
          return this.VisitQueryTransact((QueryTransact)node1, node2 as QueryTransact);
        case NodeType.QueryTypeFilter:
          return this.VisitQueryTypeFilter((QueryTypeFilter)node1, node2 as QueryTypeFilter);
        case NodeType.QueryUnion:
          return this.VisitQueryUnion((QueryUnion)node1, node2 as QueryUnion);
        case NodeType.QueryUpdate:
          return this.VisitQueryUpdate((QueryUpdate)node1, node2 as QueryUpdate);
        case NodeType.QueryYielder:
          return this.VisitQueryYielder((QueryYielder)node1, node2 as QueryYielder);
#endif
        default:
          return this.VisitUnknownNodeType(node1, node2);
      }
    }
    public virtual Differences VisitUnknownNodeType(Node/*!*/ node1, Node node2) {
      Differences differences = new Differences(node1, node2);
      if (node1 == null || node2 == null){
        if (node1 != node2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Comparer visitor = this.GetVisitorFor(node1);
      if (visitor == null) return null;
      if (this.CallingVisitor != null)
        //Allow specialized state (unknown to this visitor) to propagate all the way down to the new visitor
        this.CallingVisitor.TransferStateTo(visitor);
      this.TransferStateTo(visitor);
      differences = visitor.Visit(node1, node2);
      visitor.TransferStateTo(this);
      if (this.CallingVisitor != null)
        //Propagate specialized state (unknown to this visitor) all the way up the chain
        visitor.TransferStateTo(this.CallingVisitor);
      if (differences != null && differences.NumberOfDifferences > 0 && differences.Changes == null)
        differences.Changes = node2;
      return differences;
    }
    public virtual Differences VisitAddressDereference(AddressDereference/*!*/ addr1, AddressDereference addr2) {
      Differences differences = new Differences(addr1, addr2);
      if (addr1 == null || addr2 == null){
        if (addr1 != addr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AddressDereference changes = (AddressDereference)addr2.Clone();
      AddressDereference deletions = (AddressDereference)addr2.Clone();
      AddressDereference insertions = (AddressDereference)addr2.Clone();
  
      Differences diff = this.VisitExpression(addr1.Address, addr2.Address);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Address = diff.Changes as Expression;
      deletions.Address = diff.Deletions as Expression;
      insertions.Address = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Address && diff.Deletions == deletions.Address && diff.Insertions == insertions.Address);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (addr1.Alignment == addr2.Alignment) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (addr1.Volatile == addr2.Volatile) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAliasDefinition(AliasDefinition/*!*/ aliasDefinition1, AliasDefinition aliasDefinition2) {
      Differences differences = new Differences(aliasDefinition1, aliasDefinition2);
      if (aliasDefinition1 == null || aliasDefinition2 == null){
        if (aliasDefinition1 != aliasDefinition2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AliasDefinition changes = (AliasDefinition)aliasDefinition2.Clone();
      AliasDefinition deletions = (AliasDefinition)aliasDefinition2.Clone();
      AliasDefinition insertions = (AliasDefinition)aliasDefinition2.Clone();

      Differences diff = this.VisitIdentifier(aliasDefinition1.Alias, aliasDefinition2.Alias);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Alias = diff.Changes as Identifier;
      deletions.Alias = diff.Deletions as Identifier;
      insertions.Alias = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Alias && diff.Deletions == deletions.Alias && diff.Insertions == insertions.Alias);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(aliasDefinition1.AliasedExpression, aliasDefinition2.AliasedExpression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AliasedExpression = diff.Changes as Expression;
      deletions.AliasedExpression = diff.Deletions as Expression;
      insertions.AliasedExpression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.AliasedExpression && diff.Deletions == deletions.AliasedExpression && diff.Insertions == insertions.AliasedExpression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(aliasDefinition1.AliasedUri, aliasDefinition2.AliasedUri);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AliasedUri = diff.Changes as Identifier;
      deletions.AliasedUri = diff.Deletions as Identifier;
      insertions.AliasedUri = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.AliasedUri && diff.Deletions == deletions.AliasedUri && diff.Insertions == insertions.AliasedUri);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAliasDefinitionList(AliasDefinitionList list1, AliasDefinitionList list2, 
      out AliasDefinitionList changes, out AliasDefinitionList deletions, out AliasDefinitionList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new AliasDefinitionList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare alias definitions that have matching aliases
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        AliasDefinition nd2 = list2[j];
        if (nd2 == null || nd2.Alias == null) continue;
        matchingPosFor[nd2.Alias.UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        AliasDefinition nd1 = list1[i];
        if (nd1 == null || nd1.Alias == null) continue;
        object pos = matchingPosFor[nd1.Alias.UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        AliasDefinition nd2 = list2[j];
        //^ assume nd2 != null; //since there was entry int matchingPosFor
        //nd1 and nd2 define the same alias name and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitAliasDefinition(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as AliasDefinition;
          deletions[i] = diff.Deletions as AliasDefinition;
          insertions[i] = diff.Insertions as AliasDefinition;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        AliasDefinition nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        AliasDefinition nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitAnonymousNestedFunction(AnonymousNestedFunction/*!*/ func1, AnonymousNestedFunction func2) {
      Differences differences = new Differences(func1, func2);
      if (func1 == null || func2 == null){
        if (func1 != func2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AnonymousNestedFunction changes = (AnonymousNestedFunction)func2.Clone();
      AnonymousNestedFunction deletions = (AnonymousNestedFunction)func2.Clone();
      AnonymousNestedFunction insertions = (AnonymousNestedFunction)func2.Clone();
       
      Differences diff = this.VisitBlock(func1.Body, func2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ParameterList parChanges, parDeletions, parInsertions;
      diff = this.VisitParameterList(func1.Parameters, func2.Parameters, out parChanges, out parDeletions, out parInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Parameters = parChanges;
      deletions.Parameters = parDeletions;
      insertions.Parameters = parInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitApplyToAll(ApplyToAll/*!*/ applyToAll1, ApplyToAll applyToAll2) {
      Differences differences = new Differences(applyToAll1, applyToAll2);
      if (applyToAll1 == null || applyToAll2 == null){
        if (applyToAll1 != applyToAll2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ApplyToAll changes = (ApplyToAll)applyToAll2.Clone();
      ApplyToAll deletions = (ApplyToAll)applyToAll2.Clone();
      ApplyToAll insertions = (ApplyToAll)applyToAll2.Clone();

      Differences diff = this.VisitExpression(applyToAll1.Operand1, applyToAll2.Operand1);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand1 = diff.Changes as Expression;
      deletions.Operand1 = diff.Deletions as Expression;
      insertions.Operand1 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand1 && diff.Deletions == deletions.Operand1 && diff.Insertions == insertions.Operand1);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(applyToAll1.Operand2, applyToAll2.Operand2);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand2 = diff.Changes as Expression;
      deletions.Operand2 = diff.Deletions as Expression;
      insertions.Operand2 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand2 && diff.Deletions == deletions.Operand2 && diff.Insertions == insertions.Operand2);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAssembly(AssemblyNode/*!*/ assembly1, AssemblyNode assembly2) {
      Differences differences = new Differences(assembly1, assembly2);
      if (assembly1 == null || assembly2 == null){
        if (assembly1 != assembly2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AssemblyNode changes = (AssemblyNode)assembly2.Clone();
      AssemblyNode deletions = (AssemblyNode)assembly2.Clone();
      AssemblyNode insertions = (AssemblyNode)assembly2.Clone();

      this.OriginalModule = assembly1;
      this.NewModule = assembly2;

      AssemblyReferenceList arChanges, arDeletions, arInsertions;
      Differences diff = this.VisitAssemblyReferenceList(assembly1.AssemblyReferences, assembly2.AssemblyReferences, out arChanges, out arDeletions, out arInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AssemblyReferences = arChanges;
      deletions.AssemblyReferences = arDeletions;
      insertions.AssemblyReferences = arInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      AttributeList attrChanges, attrDeletions, attrInsertions;
      diff = this.VisitAttributeList(assembly1.Attributes, assembly2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (assembly1.Culture == assembly2.Culture) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(assembly1.ExportedTypes, assembly2.ExportedTypes, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Types = typeChanges;
      deletions.Types = typeDeletions;
      insertions.Types = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (assembly1.Flags == assembly2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (assembly1.Kind == assembly2.Kind) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitAttributeList(assembly1.ModuleAttributes, assembly2.ModuleAttributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ModuleReferenceList mrChanges, mrDeletions, mrInsertions;
      diff = this.VisitModuleReferenceList(assembly1.ModuleReferences, assembly2.ModuleReferences, out mrChanges, out mrDeletions, out mrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ModuleReferences = mrChanges;
      deletions.ModuleReferences = mrDeletions;
      insertions.ModuleReferences = mrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (assembly1.Name == assembly2.Name) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      SecurityAttributeList secChanges, secDeletions, secInsertions;
      diff = this.VisitSecurityAttributeList(assembly1.SecurityAttributes, assembly2.SecurityAttributes, out secChanges, out secDeletions, out secInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SecurityAttributes = secChanges;
      deletions.SecurityAttributes = secDeletions;
      insertions.SecurityAttributes = secInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNodeList(assembly1.Types, assembly2.Types, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Types = typeChanges;
      deletions.Types = typeDeletions;
      insertions.Types = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (assembly1.Version == assembly2.Version) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAssemblyReference(AssemblyReference/*!*/ assemblyReference1, AssemblyReference assemblyReference2) {
      Differences differences = new Differences(assemblyReference1, assemblyReference2);
      if (assemblyReference1 == null || assemblyReference2 == null){
        if (assemblyReference1 != assemblyReference2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AddressDereference changes = (AddressDereference)assemblyReference2.Clone();
      AddressDereference deletions = (AddressDereference)assemblyReference2.Clone();
      AddressDereference insertions = (AddressDereference)assemblyReference2.Clone();

      if (assemblyReference1.Culture == assemblyReference2.Culture) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (assemblyReference1.Flags == assemblyReference2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (assemblyReference1.Name == assemblyReference2.Name) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (this.ValuesAreEqual(assemblyReference1.PublicKeyOrToken, assemblyReference2.PublicKeyOrToken)) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (assemblyReference1.Version == assemblyReference2.Version) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAssemblyReferenceList(AssemblyReferenceList list1, AssemblyReferenceList list2,
      out AssemblyReferenceList changes, out AssemblyReferenceList deletions, out AssemblyReferenceList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new AssemblyReferenceList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare references that have matching assembly names
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        AssemblyReference nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[Identifier.For(nd2.Name).UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        AssemblyReference nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[Identifier.For(nd1.Name).UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        AssemblyReference nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 define the same alias name and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitAssemblyReference(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as AssemblyReference;
          deletions[i] = diff.Deletions as AssemblyReference;
          insertions[i] = diff.Insertions as AssemblyReference;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        AssemblyReference nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        AssemblyReference nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1;
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitAssertion(Assertion assertion1, Assertion assertion2){
      Differences differences = new Differences(assertion1, assertion2);
      if (assertion1 == null || assertion2 == null){
        if (assertion1 != assertion2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Assertion changes = (Assertion)assertion2.Clone();
      Assertion deletions = (Assertion)assertion2.Clone();
      Assertion insertions = (Assertion)assertion2.Clone();
  
      Differences diff = this.VisitExpression(assertion1.Condition, assertion2.Condition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Condition = diff.Changes as Expression;
      deletions.Condition = diff.Deletions as Expression;
      insertions.Condition = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Condition && diff.Deletions == deletions.Condition && diff.Insertions == insertions.Condition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAssignmentExpression(AssignmentExpression assignment1, AssignmentExpression assignment2){
      Differences differences = new Differences(assignment1, assignment2);
      if (assignment1 == null || assignment2 == null){
        if (assignment1 != assignment2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AssignmentExpression changes = (AssignmentExpression)assignment2.Clone();
      AssignmentExpression deletions = (AssignmentExpression)assignment2.Clone();
      AssignmentExpression insertions = (AssignmentExpression)assignment2.Clone();
  
      Differences diff = this.VisitStatement(assignment1.AssignmentStatement, assignment2.AssignmentStatement);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AssignmentStatement = diff.Changes as AssignmentStatement;
      deletions.AssignmentStatement = diff.Deletions as AssignmentStatement;
      insertions.AssignmentStatement = diff.Insertions as AssignmentStatement;
      Debug.Assert(diff.Changes == changes.AssignmentStatement && diff.Deletions == deletions.AssignmentStatement && diff.Insertions == insertions.AssignmentStatement);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAssignmentStatement(AssignmentStatement assignment1, AssignmentStatement assignment2){
      Differences differences = new Differences(assignment1, assignment2);
      if (assignment1 == null || assignment2 == null){
        if (assignment1 != assignment2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AssignmentStatement changes = (AssignmentStatement)assignment2.Clone();
      AssignmentStatement deletions = (AssignmentStatement)assignment2.Clone();
      AssignmentStatement insertions = (AssignmentStatement)assignment2.Clone();

      if (assignment1.Operator == assignment2.Operator) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitExpression(assignment1.Source, assignment2.Source);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Source = diff.Changes as Expression;
      deletions.Source = diff.Deletions as Expression;
      insertions.Source = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Source && diff.Deletions == deletions.Source && diff.Insertions == insertions.Source);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(assignment1.Target, assignment2.Target);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Target = diff.Changes as Expression;
      deletions.Target = diff.Deletions as Expression;
      insertions.Target = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Target && diff.Deletions == deletions.Target && diff.Insertions == insertions.Target);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAttributeNode(AttributeNode attribute1, AttributeNode attribute2){
      Differences differences = new Differences(attribute1, attribute2);
      if (attribute1 == null || attribute2 == null){
        if (attribute1 != attribute2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      AttributeNode changes = (AttributeNode)attribute2.Clone();
      AttributeNode deletions = (AttributeNode)attribute2.Clone();
      AttributeNode insertions = (AttributeNode)attribute2.Clone();

      if (attribute1.AllowMultiple == attribute2.AllowMultiple) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitExpression(attribute1.Constructor, attribute2.Constructor);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Constructor = diff.Changes as Expression;
      deletions.Constructor = diff.Deletions as Expression;
      insertions.Constructor = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Constructor && diff.Deletions == deletions.Constructor && diff.Insertions == insertions.Constructor);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      diff = this.VisitExpressionList(attribute1.Expressions, attribute2.Expressions, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expressions = exprChanges;
      deletions.Expressions = exprDeletions;
      insertions.Expressions = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (attribute1.Target == attribute2.Target) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitAttributeList(AttributeList list1, AttributeList list2,
      out AttributeList changes, out AttributeList deletions, out AttributeList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new AttributeList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        AttributeNode nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        AttributeNode nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        AttributeNode nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as AttributeNode;
        deletions[i] = diff.Deletions as AttributeNode;
        insertions[i] = diff.Insertions as AttributeNode;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        AttributeNode nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        AttributeNode nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitBase(Base base1, Base base2){
      Differences differences = new Differences(base1, base2);
      if (base1 == null || base2 == null){
        if (base1 != base2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else{
        differences.NumberOfSimilarities++;
        differences.Changes = null;
      }
      return differences;
    }
    public virtual Differences VisitBinaryExpression(BinaryExpression binaryExpression1, BinaryExpression binaryExpression2){
      Differences differences = new Differences(binaryExpression1, binaryExpression2);
      if (binaryExpression1 == null || binaryExpression2 == null){
        if (binaryExpression1 != binaryExpression2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      BinaryExpression changes = (BinaryExpression)binaryExpression2.Clone();
      BinaryExpression deletions = (BinaryExpression)binaryExpression2.Clone();
      BinaryExpression insertions = (BinaryExpression)binaryExpression2.Clone();

      if (binaryExpression1.NodeType == binaryExpression2.NodeType) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitExpression(binaryExpression1.Operand1, binaryExpression2.Operand1);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand1 = diff.Changes as Expression;
      deletions.Operand1 = diff.Deletions as Expression;
      insertions.Operand1 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand1 && diff.Deletions == deletions.Operand1 && diff.Insertions == insertions.Operand1);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(binaryExpression1.Operand2, binaryExpression2.Operand2);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand2 = diff.Changes as Expression;
      deletions.Operand2 = diff.Deletions as Expression;
      insertions.Operand2 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand2 && diff.Deletions == deletions.Operand2 && diff.Insertions == insertions.Operand2);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitBlock(Block block1, Block block2){
      Differences differences = new Differences(block1, block2);
      if (block1 == null || block2 == null){
        if (block1 != block2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Block changes = (Block)block2.Clone();
      Block deletions = (Block)block2.Clone();
      Block insertions = (Block)block2.Clone();

      if (block1.Checked == block2.Checked) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      StatementList statChanges, statDeletions, statInsertions;
      Differences diff = this.VisitStatementList(block1.Statements, block2.Statements, out statChanges, out statDeletions, out statInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Statements = statChanges;
      deletions.Statements = statDeletions;
      insertions.Statements = statInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (block1.SuppressCheck == block2.SuppressCheck) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitBlockExpression(BlockExpression blockExpression1, BlockExpression blockExpression2){
      Differences differences = new Differences(blockExpression1, blockExpression2);
      if (blockExpression1 == null || blockExpression2 == null){
        if (blockExpression1 != blockExpression2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      BlockExpression changes = (BlockExpression)blockExpression2.Clone();
      BlockExpression deletions = (BlockExpression)blockExpression2.Clone();
      BlockExpression insertions = (BlockExpression)blockExpression2.Clone();
  
      Differences diff = this.VisitBlock(blockExpression1.Block, blockExpression2.Block);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Block = diff.Changes as Block;
      deletions.Block = diff.Deletions as Block;
      insertions.Block = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Block && diff.Deletions == deletions.Block && diff.Insertions == insertions.Block);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitBlockList(BlockList list1, BlockList list2,
      out BlockList changes, out BlockList deletions, out BlockList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new BlockList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Block nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Block nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        Block nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as Block;
        deletions[i] = diff.Deletions as Block;
        insertions[i] = diff.Insertions as Block;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Block nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Block nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitCatch(Catch catch1, Catch catch2){
      Differences differences = new Differences(catch1, catch2);
      if (catch1 == null || catch2 == null){
        if (catch1 != catch2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Catch changes = (Catch)catch2.Clone();
      Catch deletions = (Catch)catch2.Clone();
      Catch insertions = (Catch)catch2.Clone();

      Differences diff = this.VisitBlock(catch1.Block, catch2.Block);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Block = diff.Changes as Block;
      deletions.Block = diff.Deletions as Block;
      insertions.Block = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Block && diff.Deletions == deletions.Block && diff.Insertions == insertions.Block);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(catch1.Type, catch2.Type);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Type = diff.Changes as TypeNode;
      deletions.Type = diff.Deletions as TypeNode;
      insertions.Type = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Type && diff.Deletions == deletions.Type && diff.Insertions == insertions.Type);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(catch1.Variable, catch2.Variable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Variable = diff.Changes as Expression;
      deletions.Variable = diff.Deletions as Expression;
      insertions.Variable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Variable && diff.Deletions == deletions.Variable && diff.Insertions == insertions.Variable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitCatchList(CatchList list1, CatchList list2,
      out CatchList changes, out CatchList deletions, out CatchList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new CatchList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Catch nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Catch nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        Catch nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as Catch;
        deletions[i] = diff.Deletions as Catch;
        insertions[i] = diff.Insertions as Catch;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Catch nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Catch nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitClass(Class class1, Class class2){
      Differences differences = this.VisitTypeNode(class1, class2);
      if (differences == null){Debug.Assert(false); differences = new Differences(class1, class2);}
      if (differences.NumberOfDifferences == 0) return differences;
      if (class1 == null || class2 == null) return differences;
      Class changes = differences.Changes as Class;
      Class deletions = differences.Changes as Class;
      Class insertions = differences.Changes as Class;
      if (changes == null || deletions == null || insertions == null){
        Debug.Assert(false);
        differences.Changes = changes = (Class)class2.Clone();
        differences.Deletions = deletions = (Class)class2.Clone();
        differences.Insertions = insertions = (Class)class2.Clone();
      }

      Differences diff = this.VisitClass(class1.BaseClass, class2.BaseClass);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.BaseClass = diff.Changes as Class;
      deletions.BaseClass = diff.Deletions as Class;
      insertions.BaseClass = diff.Insertions as Class;
      //Debug.Assert(diff.Changes == changes.BaseClass && diff.Deletions == deletions.BaseClass && diff.Insertions == insertions.BaseClass);

      return differences;
    }
    public virtual Differences VisitCoerceTuple(CoerceTuple coerceTuple1, CoerceTuple coerceTuple2){
      Differences differences = new Differences(coerceTuple1, coerceTuple2);
      if (coerceTuple1 == null || coerceTuple2 == null){
        if (coerceTuple1 != coerceTuple2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      CoerceTuple changes = (CoerceTuple)coerceTuple2.Clone();
      CoerceTuple deletions = (CoerceTuple)coerceTuple2.Clone();
      CoerceTuple insertions = (CoerceTuple)coerceTuple2.Clone();

      FieldList fieldChanges, fieldDeletions, fieldInsertions;
      Differences diff = this.VisitFieldList(coerceTuple1.Fields, coerceTuple2.Fields, out fieldChanges, out fieldDeletions, out fieldInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Fields = fieldChanges;
      deletions.Fields = fieldDeletions;
      insertions.Fields = fieldInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(coerceTuple1.OriginalTuple, coerceTuple2.OriginalTuple);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.OriginalTuple = diff.Changes as Expression;
      deletions.OriginalTuple = diff.Deletions as Expression;
      insertions.OriginalTuple = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.OriginalTuple && diff.Deletions == deletions.OriginalTuple && diff.Insertions == insertions.OriginalTuple);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitCollectionEnumerator(CollectionEnumerator ce1, CollectionEnumerator ce2){
      Differences differences = new Differences(ce1, ce2);
      if (ce1 == null || ce2 == null){
        if (ce1 != ce2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      CollectionEnumerator changes = (CollectionEnumerator)ce2.Clone();
      CollectionEnumerator deletions = (CollectionEnumerator)ce2.Clone();
      CollectionEnumerator insertions = (CollectionEnumerator)ce2.Clone();

      Differences diff = this.VisitExpression(ce1.Collection, ce2.Collection);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Collection = diff.Changes as Expression;
      deletions.Collection = diff.Deletions as Expression;
      insertions.Collection = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Collection && diff.Deletions == deletions.Collection && diff.Insertions == insertions.Collection);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitCompilation(Compilation compilation1, Compilation compilation2){
      Differences differences = new Differences(compilation1, compilation2);
      if (compilation1 == null || compilation2 == null){
        if (compilation1 != compilation2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Compilation changes = (Compilation)compilation2.Clone();
      Compilation deletions = (Compilation)compilation2.Clone();
      Compilation insertions = (Compilation)compilation2.Clone();

      this.OriginalModule = compilation1.TargetModule;
      this.NewModule = compilation2.TargetModule;

      CompilationUnitList cuChanges, cuDeletions, cuInsertions;
      Differences diff = this.VisitCompilationUnitList(compilation1.CompilationUnits, compilation2.CompilationUnits, out cuChanges, out cuDeletions, out cuInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.CompilationUnits = cuChanges;
      deletions.CompilationUnits = cuDeletions;
      insertions.CompilationUnits = cuInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitCompilationUnit(CompilationUnit cUnit1, CompilationUnit cUnit2){
      Differences differences = new Differences(cUnit1, cUnit2);
      if (cUnit1 == null || cUnit2 == null){
        if (cUnit1 != cUnit2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      CompilationUnit changes = (CompilationUnit)cUnit2.Clone();
      CompilationUnit deletions = (CompilationUnit)cUnit2.Clone();
      CompilationUnit insertions = (CompilationUnit)cUnit2.Clone();

      NodeList nodeChanges, nodeDeletions, nodeInsertions;
      Differences diff = this.VisitNodeList(cUnit1.Nodes, cUnit2.Nodes, out nodeChanges, out nodeDeletions, out nodeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Nodes = nodeChanges;
      deletions.Nodes = nodeDeletions;
      insertions.Nodes = nodeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitCompilationUnitList(CompilationUnitList list1, CompilationUnitList list2,
      out CompilationUnitList changes, out CompilationUnitList deletions, out CompilationUnitList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new CompilationUnitList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        CompilationUnit nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[nd2.Name.UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        CompilationUnit nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[nd1.Name.UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        CompilationUnit nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitCompilationUnit(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as CompilationUnit;
          deletions[i] = diff.Deletions as CompilationUnit;
          insertions[i] = diff.Insertions as CompilationUnit;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        CompilationUnit nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        CompilationUnit nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitCompilationUnitSnippet(CompilationUnitSnippet snippet1, CompilationUnitSnippet snippet2){
      Differences differences = new Differences(snippet1, snippet2);
      if (snippet1 == null || snippet2 == null){
        if (snippet1 != snippet2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      CompilationUnitSnippet changes = (CompilationUnitSnippet)snippet2.Clone();
      CompilationUnitSnippet deletions = (CompilationUnitSnippet)snippet2.Clone();
      CompilationUnitSnippet insertions = (CompilationUnitSnippet)snippet2.Clone();

      if (snippet1.SourceContext.Document == null || snippet2.SourceContext.Document == null){
        if (snippet1.SourceContext.Document == snippet2.SourceContext.Document) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      }else if (snippet1.SourceContext.Document.Name == snippet2.SourceContext.Document.Name) 
        differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitComposition(Composition comp1, Composition comp2){
      Differences differences = new Differences(comp1, comp2);
      if (comp1 == null || comp2 == null){
        if (comp1 != comp2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Composition changes = (Composition)comp2.Clone();
      Composition deletions = (Composition)comp2.Clone();
      Composition insertions = (Composition)comp2.Clone();

      if (comp1.Composer != comp2.Composer && comp1.Composer != null && comp2.Composer != null && comp1.Composer.ToString() != comp2.Composer.ToString())
        differences.NumberOfDifferences++;
      else{
        differences.NumberOfSimilarities++;
        Differences diff = this.VisitExpression(comp1.Expression, comp2.Expression);
        if (diff == null){Debug.Assert(false); return differences;}
        changes.Expression = diff.Changes as Expression;
        deletions.Expression = diff.Deletions as Expression;
        insertions.Expression = diff.Insertions as Expression;
        Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQuantifier(Quantifier quantifier1, Quantifier quantifier2){
      Differences differences = new Differences(quantifier1, quantifier2);
      if (quantifier1 == null || quantifier2 == null){
        if (quantifier1 != quantifier2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Quantifier changes = (Quantifier)quantifier2.Clone();
      Quantifier deletions = (Quantifier)quantifier2.Clone();
      Quantifier insertions = (Quantifier)quantifier2.Clone();

      Differences diff = this.VisitComprehension((Comprehension)quantifier1.Comprehension, (Comprehension)quantifier2.Comprehension);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Comprehension = diff.Changes as Comprehension;
      deletions.Comprehension = diff.Deletions as Comprehension;
      insertions.Comprehension = diff.Insertions as Comprehension;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (quantifier1.QuantifierType == quantifier2.QuantifierType) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitComprehension(Comprehension comprehension1, Comprehension comprehension2){
      Differences differences = new Differences(comprehension1, comprehension2);
      if (comprehension1 == null || comprehension2 == null){
        if (comprehension1 != comprehension2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Comprehension changes = (Comprehension)comprehension2.Clone();
      Comprehension deletions = (Comprehension)comprehension2.Clone();
      Comprehension insertions = (Comprehension)comprehension2.Clone();

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      Differences diff = this.VisitExpressionList(comprehension1.BindingsAndFilters, comprehension2.BindingsAndFilters, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.BindingsAndFilters = exprChanges;
      deletions.BindingsAndFilters = exprDeletions;
      insertions.BindingsAndFilters = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(comprehension1.Elements, comprehension2.Elements, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Elements = exprChanges;
      deletions.Elements = exprDeletions;
      insertions.Elements = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (comprehension1.Mode == comprehension2.Mode) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitComprehensionBinding(ComprehensionBinding comprehensionBinding1, ComprehensionBinding comprehensionBinding2){
      Differences differences = new Differences(comprehensionBinding1, comprehensionBinding2);
      if (comprehensionBinding1 == null || comprehensionBinding2 == null){
        if (comprehensionBinding1 != comprehensionBinding2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ComprehensionBinding changes = (ComprehensionBinding)comprehensionBinding2.Clone();
      ComprehensionBinding deletions = (ComprehensionBinding)comprehensionBinding2.Clone();
      ComprehensionBinding insertions = (ComprehensionBinding)comprehensionBinding2.Clone();

      Differences diff = this.VisitTypeNode(comprehensionBinding1.AsTargetVariableType, comprehensionBinding2.AsTargetVariableType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AsTargetVariableType = diff.Changes as TypeNode;
      deletions.AsTargetVariableType = diff.Deletions as TypeNode;
      insertions.AsTargetVariableType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.AsTargetVariableType && diff.Deletions == deletions.AsTargetVariableType && diff.Insertions == insertions.AsTargetVariableType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (comprehensionBinding1.Mode == comprehensionBinding2.Mode) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitExpression(comprehensionBinding1.SourceEnumerable, comprehensionBinding2.SourceEnumerable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SourceEnumerable = diff.Changes as Expression;
      deletions.SourceEnumerable = diff.Deletions as Expression;
      insertions.SourceEnumerable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.SourceEnumerable && diff.Deletions == deletions.SourceEnumerable && diff.Insertions == insertions.SourceEnumerable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(comprehensionBinding1.TargetVariable, comprehensionBinding2.TargetVariable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TargetVariable = diff.Changes as Expression;
      deletions.TargetVariable = diff.Deletions as Expression;
      insertions.TargetVariable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.TargetVariable && diff.Deletions == deletions.TargetVariable && diff.Insertions == insertions.TargetVariable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(comprehensionBinding1.TargetVariableType, comprehensionBinding2.TargetVariableType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TargetVariableType = diff.Changes as TypeNode;
      deletions.TargetVariableType = diff.Deletions as TypeNode;
      insertions.TargetVariableType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.TargetVariableType && diff.Deletions == deletions.TargetVariableType && diff.Insertions == insertions.TargetVariableType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitConstruct(Construct cons1, Construct cons2){
      Differences differences = new Differences(cons1, cons2);
      if (cons1 == null || cons2 == null){
        if (cons1 != cons2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Construct changes = (Construct)cons2.Clone();
      Construct deletions = (Construct)cons2.Clone();
      Construct insertions = (Construct)cons2.Clone();

      Differences diff = this.VisitExpression(cons1.Constructor, cons2.Constructor);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Constructor = diff.Changes as Expression;
      deletions.Constructor = diff.Deletions as Expression;
      insertions.Constructor = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Constructor && diff.Deletions == deletions.Constructor && diff.Insertions == insertions.Constructor);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      diff = this.VisitExpressionList(cons1.Operands, cons2.Operands, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(cons1.Owner, cons2.Owner);
      if (diff == null) { Debug.Assert(false); return differences; }
      changes.Owner = diff.Changes as Expression;
      deletions.Owner = diff.Deletions as Expression;
      insertions.Owner = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Owner && diff.Deletions == deletions.Owner && diff.Insertions == insertions.Owner);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0) {
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitConstructArray(ConstructArray consArr1, ConstructArray consArr2){
      Differences differences = new Differences(consArr1, consArr2);
      if (consArr1 == null || consArr2 == null){
        if (consArr1 != consArr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ConstructArray changes = (ConstructArray)consArr2.Clone();
      ConstructArray deletions = (ConstructArray)consArr2.Clone();
      ConstructArray insertions = (ConstructArray)consArr2.Clone();

      Differences diff = this.VisitTypeNode(consArr1.ElementType, consArr2.ElementType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ElementType = diff.Changes as TypeNode;
      deletions.ElementType = diff.Deletions as TypeNode;
      insertions.ElementType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ElementType && diff.Deletions == deletions.ElementType && diff.Insertions == insertions.ElementType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      diff = this.VisitExpressionList(consArr1.Initializers, consArr2.Initializers, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(consArr1.Operands, consArr2.Operands, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (consArr1.Rank == consArr2.Rank) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitExpression(consArr1.Owner, consArr2.Owner);
      if (diff == null) { Debug.Assert(false); return differences; }
      changes.Owner = diff.Changes as Expression;
      deletions.Owner = diff.Deletions as Expression;
      insertions.Owner = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Owner && diff.Deletions == deletions.Owner && diff.Insertions == insertions.Owner);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitConstructDelegate(ConstructDelegate consDelegate1, ConstructDelegate consDelegate2){
      Differences differences = new Differences(consDelegate1, consDelegate2);
      if (consDelegate1 == null || consDelegate2 == null){
        if (consDelegate1 != consDelegate2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ConstructDelegate changes = (ConstructDelegate)consDelegate2.Clone();
      ConstructDelegate deletions = (ConstructDelegate)consDelegate2.Clone();
      ConstructDelegate insertions = (ConstructDelegate)consDelegate2.Clone();

      Differences diff = this.VisitTypeNode(consDelegate1.DelegateType, consDelegate2.DelegateType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.DelegateType = diff.Changes as TypeNode;
      deletions.DelegateType = diff.Deletions as TypeNode;
      insertions.DelegateType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.DelegateType && diff.Deletions == deletions.DelegateType && diff.Insertions == insertions.DelegateType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(consDelegate1.MethodName, consDelegate2.MethodName);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.MethodName = diff.Changes as Identifier;
      deletions.MethodName = diff.Deletions as Identifier;
      insertions.MethodName = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.MethodName && diff.Deletions == deletions.MethodName && diff.Insertions == insertions.MethodName);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitConstructFlexArray(ConstructFlexArray consArr1, ConstructFlexArray consArr2){
      Differences differences = new Differences(consArr1, consArr2);
      if (consArr1 == null || consArr2 == null){
        if (consArr1 != consArr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ConstructFlexArray changes = (ConstructFlexArray)consArr2.Clone();
      ConstructFlexArray deletions = (ConstructFlexArray)consArr2.Clone();
      ConstructFlexArray insertions = (ConstructFlexArray)consArr2.Clone();

      Differences diff = this.VisitTypeNode(consArr1.ElementType, consArr2.ElementType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ElementType = diff.Changes as TypeNode;
      deletions.ElementType = diff.Deletions as TypeNode;
      insertions.ElementType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ElementType && diff.Deletions == deletions.ElementType && diff.Insertions == insertions.ElementType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      diff = this.VisitExpressionList(consArr1.Initializers, consArr2.Initializers, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(consArr1.Operands, consArr2.Operands, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitConstructIterator(ConstructIterator consIterator1, ConstructIterator consIterator2){
      Differences differences = new Differences(consIterator1, consIterator2);
      if (consIterator1 == null || consIterator2 == null){
        if (consIterator1 != consIterator2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ConstructIterator changes = (ConstructIterator)consIterator2.Clone();
      ConstructIterator deletions = (ConstructIterator)consIterator2.Clone();
      ConstructIterator insertions = (ConstructIterator)consIterator2.Clone();

      Differences diff = this.VisitBlock(consIterator1.Body, consIterator2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(consIterator1.ElementType, consIterator2.ElementType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ElementType = diff.Changes as TypeNode;
      deletions.ElementType = diff.Deletions as TypeNode;
      insertions.ElementType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ElementType && diff.Deletions == deletions.ElementType && diff.Insertions == insertions.ElementType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitClass(consIterator1.State, consIterator2.State);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.State = diff.Changes as Class;
      deletions.State = diff.Deletions as Class;
      insertions.State = diff.Insertions as Class;
      //Debug.Assert(diff.Changes == changes.State && diff.Deletions == deletions.State && diff.Insertions == insertions.State);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitConstructTuple(ConstructTuple consTuple1, ConstructTuple consTuple2){
      Differences differences = new Differences(consTuple1, consTuple2);
      if (consTuple1 == null || consTuple2 == null){
        if (consTuple1 != consTuple2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ConstructTuple changes = (ConstructTuple)consTuple2.Clone();
      ConstructTuple deletions = (ConstructTuple)consTuple2.Clone();
      ConstructTuple insertions = (ConstructTuple)consTuple2.Clone();

      FieldList fieldChanges, fieldDeletions, fieldInsertions;
      Differences diff = this.VisitFieldList(consTuple1.Fields, consTuple2.Fields, out fieldChanges, out fieldDeletions, out fieldInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Fields = fieldChanges;
      deletions.Fields = fieldDeletions;
      insertions.Fields = fieldInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#if ExtendedRuntime    
    public virtual Differences VisitConstrainedType(ConstrainedType cType1, ConstrainedType cType2){
      Differences differences = new Differences(cType1, cType2);
      if (cType1 == null || cType2 == null){
        if (cType1 != cType2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ConstrainedType changes = (ConstrainedType)cType2.Clone();
      ConstrainedType deletions = (ConstrainedType)cType2.Clone();
      ConstrainedType insertions = (ConstrainedType)cType2.Clone();

      Differences diff = this.VisitExpression(cType1.Constraint, cType2.Constraint);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Constraint = diff.Changes as Expression;
      deletions.Constraint = diff.Deletions as Expression;
      insertions.Constraint = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Constraint && diff.Deletions == deletions.Constraint && diff.Insertions == insertions.Constraint);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(cType1.UnderlyingType, cType2.UnderlyingType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.UnderlyingType = diff.Changes as TypeNode;
      deletions.UnderlyingType = diff.Deletions as TypeNode;
      insertions.UnderlyingType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.UnderlyingType && diff.Deletions == deletions.UnderlyingType && diff.Insertions == insertions.UnderlyingType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#endif
    public virtual Differences VisitContinue(Continue continue1, Continue continue2){
      Differences differences = new Differences(continue1, continue2);
      if (continue1 == null || continue2 == null){
        if (continue1 != continue2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Continue changes = (Continue)continue2.Clone();
      Continue deletions = (Continue)continue2.Clone();
      Continue insertions = (Continue)continue2.Clone();

      differences.NumberOfSimilarities++;
      if (continue1.Level == continue2.Level) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitCurrentClosure(CurrentClosure currentClosure1, CurrentClosure currentClosure2){
      Differences differences = new Differences(currentClosure1, currentClosure2);
      if (currentClosure1 == null || currentClosure2 == null){
        if (currentClosure1 != currentClosure2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else{
        differences.NumberOfSimilarities++;
        differences.Changes = null;
      }
      return differences;
    }
    public virtual Differences VisitDelegateNode(DelegateNode delegateNode1, DelegateNode delegateNode2){
      Differences differences = new Differences(delegateNode1, delegateNode2);
      if (delegateNode1 == null || delegateNode2 == null){
        if (delegateNode1 != delegateNode2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      DelegateNode changes = (DelegateNode)delegateNode2.Clone();
      DelegateNode deletions = (DelegateNode)delegateNode2.Clone();
      DelegateNode insertions = (DelegateNode)delegateNode2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(delegateNode1.Attributes, delegateNode2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (delegateNode1.Flags == delegateNode2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      ParameterList parChanges, parDeletions, parInsertions;
      diff = this.VisitParameterList(delegateNode1.Parameters, delegateNode2.Parameters, out parChanges, out parDeletions, out parInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Parameters = parChanges;
      deletions.Parameters = parDeletions;
      insertions.Parameters = parInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(delegateNode1.ReturnType, delegateNode2.ReturnType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ReturnType = diff.Changes as TypeNode;
      deletions.ReturnType = diff.Deletions as TypeNode;
      insertions.ReturnType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ReturnType && diff.Deletions == deletions.ReturnType && diff.Insertions == insertions.ReturnType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      SecurityAttributeList secChanges, secDeletions, secInsertions;
      diff = this.VisitSecurityAttributeList(delegateNode1.SecurityAttributes, delegateNode2.SecurityAttributes, out secChanges, out secDeletions, out secInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SecurityAttributes = secChanges;
      deletions.SecurityAttributes = secDeletions;
      insertions.SecurityAttributes = secInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(delegateNode1.TemplateParameters, delegateNode2.TemplateParameters, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateParameters = typeChanges;
      deletions.TemplateParameters = typeDeletions;
      insertions.TemplateParameters = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitDoWhile(DoWhile doWhile1, DoWhile doWhile2){
      Differences differences = new Differences(doWhile1, doWhile2);
      if (doWhile1 == null || doWhile2 == null){
        if (doWhile1 != doWhile2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      DoWhile changes = (DoWhile)doWhile2.Clone();
      DoWhile deletions = (DoWhile)doWhile2.Clone();
      DoWhile insertions = (DoWhile)doWhile2.Clone();

      Differences diff = this.VisitBlock(doWhile1.Body, doWhile2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(doWhile1.Invariants, doWhile2.Invariants, out changes.Invariants, out deletions.Invariants, out insertions.Invariants);
      if (diff == null){Debug.Assert(false); return differences;}
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(doWhile1.Condition, doWhile2.Condition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Condition = diff.Changes as Expression;
      deletions.Condition = diff.Deletions as Expression;
      insertions.Condition = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Condition && diff.Deletions == deletions.Condition && diff.Insertions == insertions.Condition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitEndFilter(EndFilter endFilter1, EndFilter endFilter2){
      Differences differences = new Differences(endFilter1, endFilter2);
      if (endFilter1 == null || endFilter2 == null){
        if (endFilter1 != endFilter2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      EndFilter changes = (EndFilter)endFilter2.Clone();
      EndFilter deletions = (EndFilter)endFilter2.Clone();
      EndFilter insertions = (EndFilter)endFilter2.Clone();

      Differences diff = this.VisitExpression(endFilter1.Value, endFilter2.Value);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Value = diff.Changes as Expression;
      deletions.Value = diff.Deletions as Expression;
      insertions.Value = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Value && diff.Deletions == deletions.Value && diff.Insertions == insertions.Value);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitEndFinally(EndFinally endFinally1, EndFinally endFinally2){
      Differences differences = new Differences(endFinally1, endFinally2);
      if (endFinally1 == null || endFinally2 == null){
        if (endFinally1 != endFinally2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      differences.Changes = null;
      return differences;
    }
    public virtual Differences VisitEnumNode(EnumNode enumNode1, EnumNode enumNode2){
      Differences differences = this.GetMemberDifferences(enumNode1, enumNode2);
      if (differences == null){Debug.Assert(false); differences = new Differences(enumNode1, enumNode2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (enumNode1 == null || enumNode2 == null){
        if (enumNode1 != enumNode2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Differences diff = this.VisitTypeNode(enumNode1, enumNode2);
      if (diff == null){Debug.Assert(false); return differences;}
      differences.NumberOfDifferences = diff.NumberOfDifferences;
      differences.NumberOfSimilarities = diff.NumberOfSimilarities;
      EnumNode changes = (EnumNode)diff.Changes;
      EnumNode deletions = (EnumNode)diff.Deletions;
      EnumNode insertions = (EnumNode)diff.Insertions;

      diff = this.VisitTypeNode(enumNode1.UnderlyingType, enumNode2.UnderlyingType);
      if (diff == null){Debug.Assert(false); return differences;}
      if (changes != null) changes.UnderlyingType = diff.Changes as TypeNode;
      if (deletions != null) deletions.UnderlyingType = diff.Deletions as TypeNode;
      if (insertions != null) insertions.UnderlyingType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.UnderlyingType && diff.Deletions == deletions.UnderlyingType && diff.Insertions == insertions.UnderlyingType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitEvent(Event evnt1, Event evnt2){
      Differences differences = this.GetMemberDifferences(evnt1, evnt2);
      if (differences == null){Debug.Assert(false); differences = new Differences(evnt1, evnt2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (evnt1 == null || evnt2 == null){
        if (evnt1 != evnt2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Event changes = (Event)evnt2.Clone();
      Event deletions = (Event)evnt2.Clone();
      Event insertions = (Event)evnt2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(evnt1.Attributes, evnt2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (evnt1.Flags == evnt2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitMethod(evnt1.HandlerAdder, evnt2.HandlerAdder);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.HandlerAdder = diff.Changes as Method;
      deletions.HandlerAdder = diff.Deletions as Method;
      insertions.HandlerAdder = diff.Insertions as Method;
      Debug.Assert(diff.Changes == changes.HandlerAdder && diff.Deletions == deletions.HandlerAdder && diff.Insertions == insertions.HandlerAdder);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitMethod(evnt1.HandlerCaller, evnt2.HandlerCaller);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.HandlerCaller = diff.Changes as Method;
      deletions.HandlerCaller = diff.Deletions as Method;
      insertions.HandlerCaller = diff.Insertions as Method;
      Debug.Assert(diff.Changes == changes.HandlerCaller && diff.Deletions == deletions.HandlerCaller && diff.Insertions == insertions.HandlerCaller);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (evnt1.HandlerFlags == evnt2.HandlerFlags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitMethod(evnt1.HandlerRemover, evnt2.HandlerRemover);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.HandlerRemover = diff.Changes as Method;
      deletions.HandlerRemover = diff.Deletions as Method;
      insertions.HandlerRemover = diff.Insertions as Method;
      Debug.Assert(diff.Changes == changes.HandlerRemover && diff.Deletions == deletions.HandlerRemover && diff.Insertions == insertions.HandlerRemover);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(evnt1.HandlerType, evnt2.HandlerType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.HandlerType = diff.Changes as TypeNode;
      deletions.HandlerType = diff.Deletions as TypeNode;
      insertions.HandlerType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.HandlerType && diff.Deletions == deletions.HandlerType && diff.Insertions == insertions.HandlerType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(evnt1.InitialHandler, evnt2.InitialHandler);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.InitialHandler = diff.Changes as Expression;
      deletions.InitialHandler = diff.Deletions as Expression;
      insertions.InitialHandler = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.InitialHandler && diff.Deletions == deletions.InitialHandler && diff.Insertions == insertions.InitialHandler);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(evnt1.Name, evnt2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      MethodList methChanges, methDeletions, methInsertions;
      diff = this.VisitMethodList(evnt1.OtherMethods, evnt2.OtherMethods, out methChanges, out methDeletions, out methInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.OtherMethods = methChanges;
      deletions.OtherMethods = methDeletions;
      insertions.OtherMethods = methInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (evnt1.OverridesBaseClassMember == evnt2.OverridesBaseClassMember) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitExit(Exit exit1, Exit exit2){
      Differences differences = new Differences(exit1, exit2);
      if (exit1 == null || exit2 == null){
        if (exit1 != exit2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Exit changes = (Exit)exit2.Clone();
      Exit deletions = (Exit)exit2.Clone();
      Exit insertions = (Exit)exit2.Clone();

      differences.NumberOfSimilarities++;
      if (exit1.Level == exit2.Level) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitExpression(Expression expression1, Expression expression2){
      Differences differences = this.Visit(expression1, expression2);
      if (differences != null && differences.Changes == null && differences.NumberOfDifferences > 0)
        differences.Changes = expression2; //Happens when different types of expressions are compared (e.g. UnaryExpression vs BinaryExpression)
      return differences;
    }
    public virtual Differences VisitExpressionList(ExpressionList list1, ExpressionList list2,
      out ExpressionList changes, out ExpressionList deletions, out ExpressionList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new ExpressionList() : list1.Clone();
      //^ assert insertions != null;

      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Expression nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Expression nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        Expression nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as Expression;
        deletions[i] = diff.Deletions as Expression;
        insertions[i] = diff.Insertions as Expression;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Expression nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Expression nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitExpressionSnippet(ExpressionSnippet snippet1, ExpressionSnippet snippet2){
      Differences differences = new Differences(snippet1, snippet2);
      if (snippet1 == null || snippet2 == null){
        if (snippet1 != snippet2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ExpressionSnippet changes = (ExpressionSnippet)snippet2.Clone();
      ExpressionSnippet deletions = (ExpressionSnippet)snippet2.Clone();
      ExpressionSnippet insertions = (ExpressionSnippet)snippet2.Clone();

      if (snippet1.SourceContext.Document == null || snippet2.SourceContext.Document == null){
        if (snippet1.SourceContext.Document == snippet2.SourceContext.Document) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      }else if (snippet1.SourceContext.Document.Name == snippet2.SourceContext.Document.Name) 
        differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitExpressionStatement(ExpressionStatement statement1, ExpressionStatement statement2){
      Differences differences = new Differences(statement1, statement2);
      if (statement1 == null || statement2 == null){
        if (statement1 != statement2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ExpressionStatement changes = (ExpressionStatement)statement2.Clone();
      ExpressionStatement deletions = (ExpressionStatement)statement2.Clone();
      ExpressionStatement insertions = (ExpressionStatement)statement2.Clone();

      Differences diff = this.VisitExpression(statement1.Expression, statement2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFaultHandler(FaultHandler faultHandler1, FaultHandler faultHandler2){
      Differences differences = new Differences(faultHandler1, faultHandler2);
      if (faultHandler1 == null || faultHandler2 == null){
        if (faultHandler1 != faultHandler2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      FaultHandler changes = (FaultHandler)faultHandler2.Clone();
      FaultHandler deletions = (FaultHandler)faultHandler2.Clone();
      FaultHandler insertions = (FaultHandler)faultHandler2.Clone();

      Differences diff = this.VisitBlock(faultHandler1.Block, faultHandler2.Block);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Block = diff.Changes as Block;
      deletions.Block = diff.Deletions as Block;
      insertions.Block = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Block && diff.Deletions == deletions.Block && diff.Insertions == insertions.Block);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFaultHandlerList(FaultHandlerList list1, FaultHandlerList list2,
      out FaultHandlerList changes, out FaultHandlerList deletions, out FaultHandlerList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new FaultHandlerList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        FaultHandler nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        FaultHandler nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        FaultHandler nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as FaultHandler;
        deletions[i] = diff.Deletions as FaultHandler;
        insertions[i] = diff.Insertions as FaultHandler;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        FaultHandler nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        FaultHandler nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitField(Field field1, Field field2){
      Differences differences = this.GetMemberDifferences(field1, field2);
      if (differences == null){Debug.Assert(false); differences = new Differences(field1, field2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (field1 == null || field2 == null){
        if (field1 != field2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Field changes = (Field)field2.Clone();
      Field deletions = (Field)field2.Clone();
      Field insertions = (Field)field2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(field1.Attributes, field2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitLiteral(field1.DefaultValue, field2.DefaultValue);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.DefaultValue = diff.Changes as Literal;
      deletions.DefaultValue = diff.Deletions as Literal;
      insertions.DefaultValue = diff.Insertions as Literal;
      Debug.Assert(diff.Changes == changes.DefaultValue && diff.Deletions == deletions.DefaultValue && diff.Insertions == insertions.DefaultValue);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (field1.Flags == field2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (field1.HidesBaseClassMember == field2.HidesBaseClassMember) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      InterfaceList ifaceChanges, ifaceDeletions, ifaceInsertions;
      diff = this.VisitInterfaceReferenceList(field1.ImplementedInterfaces, field2.ImplementedInterfaces, out ifaceChanges, out ifaceDeletions, out ifaceInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ImplementedInterfaces = ifaceChanges;
      deletions.ImplementedInterfaces = ifaceDeletions;
      insertions.ImplementedInterfaces = ifaceInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (this.ValuesAreEqual(field1.InitialData, field2.InitialData)) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitExpression(field1.Initializer, field2.Initializer);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Initializer = diff.Changes as Expression;
      deletions.Initializer = diff.Deletions as Expression;
      insertions.Initializer = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Initializer && diff.Deletions == deletions.Initializer && diff.Insertions == insertions.Initializer);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (this.ValuesAreEqual(field1.MarshallingInformation, field2.MarshallingInformation)) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitIdentifier(field1.Name, field2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (field1.Offset == field2.Offset) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (field1.OverridesBaseClassMember == field2.OverridesBaseClassMember) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (field1.Section == field2.Section) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitTypeNode(field1.Type, field2.Type);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Type = diff.Changes as TypeNode;
      deletions.Type = diff.Deletions as TypeNode;
      insertions.Type = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Type && diff.Deletions == deletions.Type && diff.Insertions == insertions.Type);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFieldInitializerBlock(FieldInitializerBlock block1, FieldInitializerBlock block2){
      Differences differences = new Differences(block1, block2);
      if (block1 == null || block2 == null){
        if (block1 != block2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Filter changes = (Filter)block2.Clone();
      Filter deletions = (Filter)block2.Clone();
      Filter insertions = (Filter)block2.Clone();
      
      if (block1.IsStatic == block2.IsStatic) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFieldList(FieldList list1, FieldList list2,
      out FieldList changes, out FieldList deletions, out FieldList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new FieldList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Field nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[nd2.Name.UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Field nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[nd1.Name.UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        Field nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitField(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as Field;
          deletions[i] = diff.Deletions as Field;
          insertions[i] = diff.Insertions as Field;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Field nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Field nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitFilter(Filter filter1, Filter filter2){
      Differences differences = new Differences(filter1, filter2);
      if (filter1 == null || filter2 == null){
        if (filter1 != filter2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Filter changes = (Filter)filter2.Clone();
      Filter deletions = (Filter)filter2.Clone();
      Filter insertions = (Filter)filter2.Clone();

      Differences diff = this.VisitBlock(filter1.Block, filter2.Block);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Block = diff.Changes as Block;
      deletions.Block = diff.Deletions as Block;
      insertions.Block = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Block && diff.Deletions == deletions.Block && diff.Insertions == insertions.Block);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(filter1.Expression, filter2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFilterList(FilterList list1, FilterList list2,
      out FilterList changes, out FilterList deletions, out FilterList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new FilterList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Filter nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Filter nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        Filter nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as Filter;
        deletions[i] = diff.Deletions as Filter;
        insertions[i] = diff.Insertions as Filter;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Filter nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Filter nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitFinally(Finally finally1, Finally finally2){
      Differences differences = new Differences(finally1, finally2);
      if (finally1 == null || finally2 == null){
        if (finally1 != finally2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Finally changes = (Finally)finally2.Clone();
      Finally deletions = (Finally)finally2.Clone();
      Finally insertions = (Finally)finally2.Clone();

      Differences diff = this.VisitBlock(finally1.Block, finally2.Block);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Block = diff.Changes as Block;
      deletions.Block = diff.Deletions as Block;
      insertions.Block = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Block && diff.Deletions == deletions.Block && diff.Insertions == insertions.Block);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFixed(Fixed fixed1, Fixed fixed2){
      Differences differences = new Differences(fixed1, fixed2);
      if (fixed1 == null || fixed2 == null){
        if (fixed1 != fixed2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Fixed changes = (Fixed)fixed2.Clone();
      Fixed deletions = (Fixed)fixed2.Clone();
      Fixed insertions = (Fixed)fixed2.Clone();

      Differences diff = this.VisitBlock(fixed1.Body, fixed2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitStatement(fixed1.Declarators, fixed2.Declarators);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Declarators = diff.Changes as Statement;
      deletions.Declarators = diff.Deletions as Statement;
      insertions.Declarators = diff.Insertions as Statement;
      Debug.Assert(diff.Changes == changes.Declarators && diff.Deletions == deletions.Declarators && diff.Insertions == insertions.Declarators);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFor(For for1, For for2){
      Differences differences = new Differences(for1, for2);
      if (for1 == null || for2 == null){
        if (for1 != for2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      For changes = (For)for2.Clone();
      For deletions = (For)for2.Clone();
      For insertions = (For)for2.Clone();

      Differences diff = this.VisitBlock(for1.Body, for2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(for1.Invariants, for2.Invariants, out changes.Invariants, out deletions.Invariants, out insertions.Invariants);
      if (diff == null){Debug.Assert(false); return differences;}
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(for1.Condition, for2.Condition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Condition = diff.Changes as Expression;
      deletions.Condition = diff.Deletions as Expression;
      insertions.Condition = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Condition && diff.Deletions == deletions.Condition && diff.Insertions == insertions.Condition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      StatementList statChanges, statDeletions, statInsertions;
      diff = this.VisitStatementList(for1.Incrementer, for2.Incrementer, out statChanges, out statDeletions, out statInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Initializer = statChanges;
      deletions.Initializer = statDeletions;
      insertions.Initializer = statInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitStatementList(for1.Initializer, for2.Initializer, out statChanges, out statDeletions, out statInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Initializer = statChanges;
      deletions.Initializer = statDeletions;
      insertions.Initializer = statInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitForEach(ForEach forEach1, ForEach forEach2){
      Differences differences = new Differences(forEach1, forEach2);
      if (forEach1 == null || forEach2 == null){
        if (forEach1 != forEach2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ForEach changes = (ForEach)forEach2.Clone();
      ForEach deletions = (ForEach)forEach2.Clone();
      ForEach insertions = (ForEach)forEach2.Clone();

      Differences diff = this.VisitBlock(forEach1.Body, forEach2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(forEach1.SourceEnumerable, forEach2.SourceEnumerable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SourceEnumerable = diff.Changes as Expression;
      deletions.SourceEnumerable = diff.Deletions as Expression;
      insertions.SourceEnumerable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.SourceEnumerable && diff.Deletions == deletions.SourceEnumerable && diff.Insertions == insertions.SourceEnumerable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(forEach1.Invariants, forEach2.Invariants, out changes.Invariants, out deletions.Invariants, out insertions.Invariants);
      if (diff == null){Debug.Assert(false); return differences;}
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(forEach1.InductionVariable, forEach2.InductionVariable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.InductionVariable = diff.Changes as Expression;
      deletions.InductionVariable = diff.Deletions as Expression;
      insertions.InductionVariable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.InductionVariable && diff.Deletions == deletions.InductionVariable && diff.Insertions == insertions.InductionVariable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(forEach1.TargetVariable, forEach2.TargetVariable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TargetVariable = diff.Changes as Expression;
      deletions.TargetVariable = diff.Deletions as Expression;
      insertions.TargetVariable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.TargetVariable && diff.Deletions == deletions.TargetVariable && diff.Insertions == insertions.TargetVariable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(forEach1.TargetVariableType, forEach2.TargetVariableType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TargetVariableType = diff.Changes as TypeNode;
      deletions.TargetVariableType = diff.Deletions as TypeNode;
      insertions.TargetVariableType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.TargetVariableType && diff.Deletions == deletions.TargetVariableType && diff.Insertions == insertions.TargetVariableType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (forEach1.StatementTerminatesNormallyIfEnumerableIsNull == forEach2.StatementTerminatesNormallyIfEnumerableIsNull) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (forEach1.StatementTerminatesNormallyIfEnumeratorIsNull == forEach2.StatementTerminatesNormallyIfEnumeratorIsNull) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitFunctionDeclaration(FunctionDeclaration functionDeclaration1, FunctionDeclaration functionDeclaration2){
      Differences differences = new Differences(functionDeclaration1, functionDeclaration2);
      if (functionDeclaration1 == null || functionDeclaration2 == null){
        if (functionDeclaration1 != functionDeclaration2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      FunctionDeclaration changes = (FunctionDeclaration)functionDeclaration2.Clone();
      FunctionDeclaration deletions = (FunctionDeclaration)functionDeclaration2.Clone();
      FunctionDeclaration insertions = (FunctionDeclaration)functionDeclaration2.Clone();

      Differences diff = this.VisitBlock(functionDeclaration1.Body, functionDeclaration2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(functionDeclaration1.Name, functionDeclaration2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ParameterList parChanges, parDeletions, parInsertions;
      diff = this.VisitParameterList(functionDeclaration1.Parameters, functionDeclaration2.Parameters, out parChanges, out parDeletions, out parInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Parameters = parChanges;
      deletions.Parameters = parDeletions;
      insertions.Parameters = parInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(functionDeclaration1.ReturnType, functionDeclaration2.ReturnType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ReturnType = diff.Changes as TypeNode;
      deletions.ReturnType = diff.Deletions as TypeNode;
      insertions.ReturnType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ReturnType && diff.Deletions == deletions.ReturnType && diff.Insertions == insertions.ReturnType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTemplateInstance(TemplateInstance instance1, TemplateInstance instance2){
      Differences differences = new Differences(instance1, instance2);
      if (instance1 == null || instance2 == null){
        if (instance1 != instance2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TemplateInstance changes = (TemplateInstance)instance2.Clone();
      TemplateInstance deletions = (TemplateInstance)instance2.Clone();
      TemplateInstance insertions = (TemplateInstance)instance2.Clone();

      Differences diff = this.VisitExpression(instance1.Expression, instance2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(instance1.TypeArguments, instance2.TypeArguments, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TypeArguments = typeChanges;
      deletions.TypeArguments = typeDeletions;
      insertions.TypeArguments = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitStackAlloc(StackAlloc alloc1, StackAlloc alloc2){
      Differences differences = new Differences(alloc1, alloc2);
      if (alloc1 == null || alloc2 == null){
        if (alloc1 != alloc2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      StackAlloc changes = (StackAlloc)alloc2.Clone();
      StackAlloc deletions = (StackAlloc)alloc2.Clone();
      StackAlloc insertions = (StackAlloc)alloc2.Clone();


      Differences diff = this.VisitTypeNode(alloc1.ElementType, alloc2.ElementType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ElementType = diff.Changes as TypeNode;
      deletions.ElementType = diff.Deletions as TypeNode;
      insertions.ElementType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ElementType && diff.Deletions == deletions.ElementType && diff.Insertions == insertions.ElementType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      
      diff = this.VisitExpression(alloc1.NumberOfElements, alloc2.NumberOfElements);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.NumberOfElements = diff.Changes as Expression;
      deletions.NumberOfElements = diff.Deletions as Expression;
      insertions.NumberOfElements = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.NumberOfElements && diff.Deletions == deletions.NumberOfElements && diff.Insertions == insertions.NumberOfElements);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitGoto(Goto goto1, Goto goto2){
      Differences differences = new Differences(goto1, goto2);
      if (goto1 == null || goto2 == null){
        if (goto1 != goto2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Goto changes = (Goto)goto2.Clone();
      Goto deletions = (Goto)goto2.Clone();
      Goto insertions = (Goto)goto2.Clone();

      Differences diff = this.VisitIdentifier(goto1.TargetLabel, goto2.TargetLabel);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TargetLabel = diff.Changes as Identifier;
      deletions.TargetLabel = diff.Deletions as Identifier;
      insertions.TargetLabel = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.TargetLabel && diff.Deletions == deletions.TargetLabel && diff.Insertions == insertions.TargetLabel);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitGotoCase(GotoCase gotoCase1, GotoCase gotoCase2){
      Differences differences = new Differences(gotoCase1, gotoCase2);
      if (gotoCase1 == null || gotoCase2 == null){
        if (gotoCase1 != gotoCase2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      GotoCase changes = (GotoCase)gotoCase2.Clone();
      GotoCase deletions = (GotoCase)gotoCase2.Clone();
      GotoCase insertions = (GotoCase)gotoCase2.Clone();

      Differences diff = this.VisitExpression(gotoCase1.CaseLabel, gotoCase2.CaseLabel);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.CaseLabel = diff.Changes as Identifier;
      deletions.CaseLabel = diff.Deletions as Identifier;
      insertions.CaseLabel = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.CaseLabel && diff.Deletions == deletions.CaseLabel && diff.Insertions == insertions.CaseLabel);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitIdentifier(Identifier identifier1, Identifier identifier2){
      Differences differences = new Differences(identifier1, identifier2);
      if (identifier1 == null || identifier2 == null){
        if (identifier1 != identifier2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else if (identifier1.UniqueIdKey == identifier2.UniqueIdKey) 
        differences.NumberOfSimilarities++; 
      else
        differences.NumberOfDifferences++;
      return differences;
    }
    public virtual Differences VisitIf(If if1, If if2){
      Differences differences = new Differences(if1, if2);
      if (if1 == null || if2 == null){
        if (if1 != if2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      If changes = (If)if2.Clone();
      If deletions = (If)if2.Clone();
      If insertions = (If)if2.Clone();

      Differences diff = this.VisitExpression(if1.Condition, if2.Condition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Condition = diff.Changes as Expression;
      deletions.Condition = diff.Deletions as Expression;
      insertions.Condition = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Condition && diff.Deletions == deletions.Condition && diff.Insertions == insertions.Condition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitBlock(if1.FalseBlock, if2.FalseBlock);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.FalseBlock = diff.Changes as Block;
      deletions.FalseBlock = diff.Deletions as Block;
      insertions.FalseBlock = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.FalseBlock && diff.Deletions == deletions.FalseBlock && diff.Insertions == insertions.FalseBlock);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitBlock(if1.TrueBlock, if2.TrueBlock);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TrueBlock = diff.Changes as Block;
      deletions.TrueBlock = diff.Deletions as Block;
      insertions.TrueBlock = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.TrueBlock && diff.Deletions == deletions.TrueBlock && diff.Insertions == insertions.TrueBlock);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitImplicitThis(ImplicitThis implicitThis1, ImplicitThis implicitThis2){
      Differences differences = new Differences(implicitThis1, implicitThis2);
      if (implicitThis1 == null || implicitThis2 == null){
        if (implicitThis1 != implicitThis2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else{
        differences.NumberOfSimilarities++;
        differences.Changes = null;
      }
      return differences;
    }
    public virtual Differences VisitIndexer(Indexer indexer1, Indexer indexer2){
      Differences differences = new Differences(indexer1, indexer2);
      if (indexer1 == null || indexer2 == null){
        if (indexer1 != indexer2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Indexer changes = (Indexer)indexer2.Clone();
      Indexer deletions = (Indexer)indexer2.Clone();
      Indexer insertions = (Indexer)indexer2.Clone();

      Differences diff = this.VisitExpression(indexer1.Object, indexer2.Object);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Object = diff.Changes as Expression;
      deletions.Object = diff.Deletions as Expression;
      insertions.Object = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Object && diff.Deletions == deletions.Object && diff.Insertions == insertions.Object);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      diff = this.VisitExpressionList(indexer1.Operands, indexer2.Operands, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitInterface(Interface interface1, Interface interface2){
      return this.VisitTypeNode(interface1, interface2);
    }
    public virtual Differences VisitInterfaceReferenceList(InterfaceList list1, InterfaceList list2,
      out InterfaceList changes, out InterfaceList deletions, out InterfaceList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new InterfaceList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Interface nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[nd2.Name.UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Interface nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[nd1.Name.UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        Interface nd2 = list2[j];
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        //^ assume nd2 != null;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitInterface(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as Interface;
          deletions[i] = diff.Deletions as Interface;
          insertions[i] = diff.Insertions as Interface;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          //Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Interface nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Interface nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitInstanceInitializer(InstanceInitializer cons1, InstanceInitializer cons2){
      return this.VisitMethod(cons1, cons2);
    }
    public virtual Differences VisitLabeledStatement(LabeledStatement lStatement1, LabeledStatement lStatement2){
      Differences differences = new Differences(lStatement1, lStatement2);
      if (lStatement1 == null || lStatement2 == null){
        if (lStatement1 != lStatement2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      LabeledStatement changes = (LabeledStatement)lStatement2.Clone();
      LabeledStatement deletions = (LabeledStatement)lStatement2.Clone();
      LabeledStatement insertions = (LabeledStatement)lStatement2.Clone();

      Differences diff = this.VisitIdentifier(lStatement1.Label, lStatement2.Label);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Label = diff.Changes as Identifier;
      deletions.Label = diff.Deletions as Identifier;
      insertions.Label = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Label && diff.Deletions == deletions.Label && diff.Insertions == insertions.Label);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitStatement(lStatement1.Statement, lStatement2.Statement);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Statement = diff.Changes as Statement;
      deletions.Statement = diff.Deletions as Statement;
      insertions.Statement = diff.Insertions as Statement;
      Debug.Assert(diff.Changes == changes.Statement && diff.Deletions == deletions.Statement && diff.Insertions == insertions.Statement);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitLiteral(Literal literal1, Literal literal2){
      Differences differences = new Differences(literal1, literal2);
      if (literal1 == null || literal2 == null){
        if (literal1 != literal2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Literal changes = (Literal)literal2.Clone();
      Literal deletions = (Literal)literal2.Clone();
      Literal insertions = (Literal)literal2.Clone();

      if (literal1.Value == literal2.Value || (literal1.Value != null && literal1.Value.Equals(literal2.Value)))
        differences.NumberOfSimilarities++;
      else
        differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitLocal(Local local1, Local local2){
      Differences differences = new Differences(local1, local2);
      if (local1 == null || local2 == null){
        if (local1 != local2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Local changes = (Local)local2.Clone();
      Local deletions = (Local)local2.Clone();
      Local insertions = (Local)local2.Clone();

      if (local1.Index == local2.Index) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (local1.InitOnly == local2.InitOnly) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitIdentifier(local1.Name, local2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (local1.Pinned == local2.Pinned) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitTypeNode(local1.Type, local2.Type);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Type = diff.Changes as TypeNode;
      deletions.Type = diff.Deletions as TypeNode;
      insertions.Type = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Type && diff.Deletions == deletions.Type && diff.Insertions == insertions.Type);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitLocalDeclaration(LocalDeclaration localDeclaration1, LocalDeclaration localDeclaration2){
      Differences differences = new Differences(localDeclaration1, localDeclaration2);
      if (localDeclaration1 == null || localDeclaration2 == null){
        if (localDeclaration1 != localDeclaration2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      LocalDeclaration changes = (LocalDeclaration)localDeclaration2.Clone();
      LocalDeclaration deletions = (LocalDeclaration)localDeclaration2.Clone();
      LocalDeclaration insertions = (LocalDeclaration)localDeclaration2.Clone();

      Differences diff = this.VisitExpression(localDeclaration1.InitialValue, localDeclaration2.InitialValue);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.InitialValue = diff.Changes as Expression;
      deletions.InitialValue = diff.Deletions as Expression;
      insertions.InitialValue = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.InitialValue && diff.Deletions == deletions.InitialValue && diff.Insertions == insertions.InitialValue);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(localDeclaration1.Name, localDeclaration2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitLocalDeclarationList(LocalDeclarationList list1, LocalDeclarationList list2,
      out LocalDeclarationList changes, out LocalDeclarationList deletions, out LocalDeclarationList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new LocalDeclarationList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        LocalDeclaration nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        LocalDeclaration nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        LocalDeclaration nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as LocalDeclaration;
        deletions[i] = diff.Deletions as LocalDeclaration;
        insertions[i] = diff.Insertions as LocalDeclaration;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        LocalDeclaration nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        LocalDeclaration nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;

    }
    public virtual Differences VisitLocalDeclarationsStatement(LocalDeclarationsStatement localDeclarations1, LocalDeclarationsStatement localDeclarations2){
      Differences differences = new Differences(localDeclarations1, localDeclarations2);
      if (localDeclarations1 == null || localDeclarations2 == null){
        if (localDeclarations1 != localDeclarations2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      LocalDeclarationsStatement changes = (LocalDeclarationsStatement)localDeclarations2.Clone();
      LocalDeclarationsStatement deletions = (LocalDeclarationsStatement)localDeclarations2.Clone();
      LocalDeclarationsStatement insertions = (LocalDeclarationsStatement)localDeclarations2.Clone();

      if (localDeclarations1.Constant == localDeclarations2.Constant) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      LocalDeclarationList declChanges, declDeletions, declInsertions;
      Differences diff = this.VisitLocalDeclarationList(localDeclarations1.Declarations, localDeclarations2.Declarations, out declChanges, out declDeletions, out declInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Declarations = declChanges;
      deletions.Declarations = declDeletions;
      insertions.Declarations = declInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (localDeclarations1.InitOnly == localDeclarations2.InitOnly) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      
      diff = this.VisitTypeNode(localDeclarations1.Type, localDeclarations2.Type);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Type = diff.Changes as TypeNode;
      deletions.Type = diff.Deletions as TypeNode;
      insertions.Type = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Type && diff.Deletions == deletions.Type && diff.Insertions == insertions.Type);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitLock(Lock lock1, Lock lock2){
      Differences differences = new Differences(lock1, lock2);
      if (lock1 == null || lock2 == null){
        if (lock1 != lock2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Lock changes = (Lock)lock2.Clone();
      Lock deletions = (Lock)lock2.Clone();
      Lock insertions = (Lock)lock2.Clone();

      Differences diff = this.VisitBlock(lock1.Body, lock2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(lock1.Guard, lock2.Guard);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Guard = diff.Changes as Expression;
      deletions.Guard = diff.Deletions as Expression;
      insertions.Guard = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Guard && diff.Deletions == deletions.Guard && diff.Insertions == insertions.Guard);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitLRExpression(LRExpression expr1, LRExpression expr2){
      Differences differences = new Differences(expr1, expr2);
      if (expr1 == null || expr2 == null){
        if (expr1 != expr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      LRExpression changes = (LRExpression)expr2.Clone();
      LRExpression deletions = (LRExpression)expr2.Clone();
      LRExpression insertions = (LRExpression)expr2.Clone();

      Differences diff = this.VisitExpression(expr1.Expression, expr2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitMember(Member member1, Member member2){
      Differences differences = this.Visit(member1, member2);
      if (differences != null && differences.Changes == null && differences.NumberOfDifferences > 0)
        differences.Changes = member2; //Happens when different types of members are compared (e.g. Method vs Field)
      return differences;
    }
    public virtual Differences VisitMemberBinding(MemberBinding memberBinding1, MemberBinding memberBinding2){
      Differences differences = new Differences(memberBinding1, memberBinding2);
      if (memberBinding1 == null || memberBinding2 == null){
        if (memberBinding1 != memberBinding2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      MemberBinding changes = (MemberBinding)memberBinding2.Clone();
      MemberBinding deletions = (MemberBinding)memberBinding2.Clone();
      MemberBinding insertions = (MemberBinding)memberBinding2.Clone();

      if (memberBinding1.Alignment == memberBinding2.Alignment) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitMember(memberBinding1.BoundMember, memberBinding2.BoundMember);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.BoundMember = diff.Changes as Member;
      deletions.BoundMember = diff.Deletions as Member;
      insertions.BoundMember = diff.Insertions as Member;
      Debug.Assert(diff.Changes == changes.BoundMember && diff.Deletions == deletions.BoundMember && diff.Insertions == insertions.BoundMember);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(memberBinding1.TargetObject, memberBinding2.TargetObject);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TargetObject = diff.Changes as Expression;
      deletions.TargetObject = diff.Deletions as Expression;
      insertions.TargetObject = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.TargetObject && diff.Deletions == deletions.TargetObject && diff.Insertions == insertions.TargetObject);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (memberBinding1.Volatile == memberBinding2.Volatile) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitMemberList(MemberList list1, MemberList list2,
      out MemberList changes, out MemberList deletions, out MemberList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new MemberList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Member nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        string fullName = nd2.FullName;
        if (fullName == null) continue;
        matchingPosFor[Identifier.For(fullName).UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Member nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        string fullName = nd1.FullName;
        if (fullName == null) continue;
        object pos = matchingPosFor[Identifier.For(fullName).UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        Member nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitMember(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as Member;
          deletions[i] = diff.Deletions as Member;
          insertions[i] = diff.Insertions as Member;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          if ((nd1.DeclaringType != null && nd1.DeclaringType.DeclaringModule == this.OriginalModule) ||
            (nd1 is TypeNode && ((TypeNode)nd1).DeclaringModule == this.OriginalModule)){
            if (this.MembersThatHaveChanged == null) this.MembersThatHaveChanged = new MemberList();
            this.MembersThatHaveChanged.Add(nd1);
          }
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Member nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
        if ((nd1.DeclaringType != null && nd1.DeclaringType.DeclaringModule == this.OriginalModule) ||
          (nd1 is TypeNode && ((TypeNode)nd1).DeclaringModule == this.OriginalModule)){
          if (this.MembersThatHaveChanged == null) this.MembersThatHaveChanged = new MemberList();
          this.MembersThatHaveChanged.Add(nd1);
        }
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Member nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitMethod(Method method1, Method method2){
      Differences differences = this.GetMemberDifferences(method1, method2);
      if (differences == null){Debug.Assert(false); differences = new Differences(method1, method2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (method1 == null || method2 == null){
        if (method1 != method2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Method changes = (Method)method2.Clone();
      Method deletions = (Method)method2.Clone();
      Method insertions = (Method)method2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(method1.Attributes, method2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (!this.DoNotCompareBodies){
        diff = this.VisitBlock(method1.Body, method2.Body);
        if (diff == null){Debug.Assert(false); return differences;}
        changes.Body = diff.Changes as Block;
        deletions.Body = diff.Deletions as Block;
        insertions.Body = diff.Insertions as Block;
        Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }

      if (method1.CallingConvention == method2.CallingConvention) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitMember(method1.DeclaringMember, method2.DeclaringMember);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.DeclaringMember = diff.Changes as Member;
      deletions.DeclaringMember = diff.Deletions as Member;
      insertions.DeclaringMember = diff.Insertions as Member;
      Debug.Assert(diff.Changes == changes.DeclaringMember && diff.Deletions == deletions.DeclaringMember && diff.Insertions == insertions.DeclaringMember);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (method1.Flags == method2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (method1.HasCompilerGeneratedSignature == method2.HasCompilerGeneratedSignature) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (method1.ImplFlags == method2.ImplFlags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (method1.InitLocals == method2.InitLocals) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (method1.IsGeneric == method2.IsGeneric) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(method1.ImplementedTypes, method2.ImplementedTypes, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ImplementedTypes = typeChanges;
      deletions.ImplementedTypes = typeDeletions;
      insertions.ImplementedTypes = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(method1.Name, method2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (method1.NodeType == method2.NodeType) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      ParameterList parChanges, parDeletions, parInsertions;
      diff = this.VisitParameterList(method1.Parameters, method2.Parameters, out parChanges, out parDeletions, out parInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Parameters = parChanges;
      deletions.Parameters = parDeletions;
      insertions.Parameters = parInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (method1.PInvokeFlags == method2.PInvokeFlags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (method1.PInvokeImportName == method2.PInvokeImportName) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (method1.PInvokeModule == method2.PInvokeModule ||
        (method1.PInvokeModule != null && method2.PInvokeModule != null && method1.PInvokeModule.Name == method2.PInvokeModule.Name))
        differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitAttributeList(method1.ReturnAttributes, method2.ReturnAttributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ReturnAttributes = attrChanges;
      deletions.ReturnAttributes = attrDeletions;
      insertions.ReturnAttributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(method1.ReturnType, method2.ReturnType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ReturnType = diff.Changes as TypeNode;
      deletions.ReturnType = diff.Deletions as TypeNode;
      insertions.ReturnType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ReturnType && diff.Deletions == deletions.ReturnType && diff.Insertions == insertions.ReturnType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      SecurityAttributeList secChanges, secDeletions, secInsertions;
      diff = this.VisitSecurityAttributeList(method1.SecurityAttributes, method2.SecurityAttributes, out secChanges, out secDeletions, out secInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SecurityAttributes = secChanges;
      deletions.SecurityAttributes = secDeletions;
      insertions.SecurityAttributes = secInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitMethod(method1.Template, method2.Template);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Template = diff.Changes as Method;
      deletions.Template = diff.Deletions as Method;
      insertions.Template = diff.Insertions as Method;
      Debug.Assert(diff.Changes == changes.Template && diff.Deletions == deletions.Template && diff.Insertions == insertions.Template);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNodeList(method1.TemplateArguments, method2.TemplateArguments, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateArguments = typeChanges;
      deletions.TemplateArguments = typeDeletions;
      insertions.TemplateArguments = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNodeList(method1.TemplateParameters, method2.TemplateParameters, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateParameters = typeChanges;
      deletions.TemplateParameters = typeDeletions;
      insertions.TemplateParameters = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitMethodList(MethodList list1, MethodList list2,
      out MethodList changes, out MethodList deletions, out MethodList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new MethodList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Method nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        string fullName = nd2.FullName;
        if (fullName == null) continue;
        matchingPosFor[Identifier.For(fullName).UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Method nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        string fullName = nd1.FullName;
        if (fullName == null) continue;
        object pos = matchingPosFor[Identifier.For(fullName).UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        Method nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitMethod(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as Method;
          deletions[i] = diff.Deletions as Method;
          insertions[i] = diff.Insertions as Method;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Method nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Method nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitMethodCall(MethodCall call1, MethodCall call2){
      Differences differences = new Differences(call1, call2);
      if (call1 == null || call2 == null){
        if (call1 != call2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      MethodCall changes = (MethodCall)call2.Clone();
      MethodCall deletions = (MethodCall)call2.Clone();
      MethodCall insertions = (MethodCall)call2.Clone();

      Differences diff = this.VisitExpression(call1.Callee, call2.Callee);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Callee = diff.Changes as Expression;
      deletions.Callee = diff.Deletions as Expression;
      insertions.Callee = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Callee && diff.Deletions == deletions.Callee && diff.Insertions == insertions.Callee);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (call1.IsTailCall == call2.IsTailCall) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      ExpressionList exprChanges, exprDeletions, exprInsertions;
      diff = this.VisitExpressionList(call1.Operands, call2.Operands, out exprChanges, out exprDeletions, out exprInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operands = exprChanges;
      deletions.Operands = exprDeletions;
      insertions.Operands = exprInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitModule(Module module1, Module module2){
      Differences differences = new Differences(module1, module2);
      if (module1 == null || module2 == null){
        if (module1 != module2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Module changes = (Module)module2.Clone();
      Module deletions = (Module)module2.Clone();
      Module insertions = (Module)module2.Clone();

      this.OriginalModule = module1;
      this.NewModule = module2;

      AssemblyReferenceList arChanges, arDeletions, arInsertions;
      Differences diff = this.VisitAssemblyReferenceList(module1.AssemblyReferences, module2.AssemblyReferences, out arChanges, out arDeletions, out arInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AssemblyReferences = arChanges;
      deletions.AssemblyReferences = arDeletions;
      insertions.AssemblyReferences = arInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      AttributeList attrChanges, attrDeletions, attrInsertions;
      diff = this.VisitAttributeList(module1.Attributes, module2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (module1.HashAlgorithm == module2.HashAlgorithm) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (module1.Kind == module2.Kind) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      ModuleReferenceList mrChanges, mrDeletions, mrInsertions;
      diff = this.VisitModuleReferenceList(module1.ModuleReferences, module2.ModuleReferences, out mrChanges, out mrDeletions, out mrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ModuleReferences = mrChanges;
      deletions.ModuleReferences = mrDeletions;
      insertions.ModuleReferences = mrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (module1.Name == module2.Name) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (module1.PEKind == module2.PEKind) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      SecurityAttributeList secChanges, secDeletions, secInsertions;
      diff = this.VisitSecurityAttributeList(module1.SecurityAttributes, module2.SecurityAttributes, out secChanges, out secDeletions, out secInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SecurityAttributes = secChanges;
      deletions.SecurityAttributes = secDeletions;
      insertions.SecurityAttributes = secInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (module1.TargetRuntimeVersion == module2.TargetRuntimeVersion) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (module1.TrackDebugData == module2.TrackDebugData) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(module1.Types, module2.Types, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Types = typeChanges;
      deletions.Types = typeDeletions;
      insertions.Types = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitModuleReference(ModuleReference moduleReference1, ModuleReference moduleReference2){
      Differences differences = new Differences(moduleReference1, moduleReference2);
      if (moduleReference1 == null || moduleReference2 == null){
        if (moduleReference1 != moduleReference2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ModuleReference changes = (ModuleReference)moduleReference2.Clone();
      ModuleReference deletions = (ModuleReference)moduleReference2.Clone();
      ModuleReference insertions = (ModuleReference)moduleReference2.Clone();

      if (moduleReference1.Name == moduleReference2.Name) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitModuleReferenceList(ModuleReferenceList list1, ModuleReferenceList list2,
      out ModuleReferenceList changes, out ModuleReferenceList deletions, out ModuleReferenceList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new ModuleReferenceList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        ModuleReference nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[Identifier.For(nd2.Name).UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        ModuleReference nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[Identifier.For(nd1.Name).UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        ModuleReference nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitModuleReference(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as ModuleReference;
          deletions[i] = diff.Deletions as ModuleReference;
          insertions[i] = diff.Insertions as ModuleReference;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        ModuleReference nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        ModuleReference nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitNameBinding(NameBinding nameBinding1, NameBinding nameBinding2){
      Differences differences = new Differences(nameBinding1, nameBinding2);
      if (nameBinding1 == null || nameBinding2 == null){
        if (nameBinding1 != nameBinding2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      NameBinding changes = (NameBinding)nameBinding2.Clone();
      NameBinding deletions = (NameBinding)nameBinding2.Clone();
      NameBinding insertions = (NameBinding)nameBinding2.Clone();

      MemberList memChanges, memDeletions, memInsertions;
      Differences diff = this.VisitMemberList(nameBinding1.BoundMembers, nameBinding2.BoundMembers, out memChanges, out memDeletions, out memInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.BoundMembers = memChanges;
      deletions.BoundMembers = memDeletions;
      insertions.BoundMembers = memInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(nameBinding1.Identifier, nameBinding2.Identifier);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Identifier = diff.Changes as Identifier;
      deletions.Identifier = diff.Deletions as Identifier;
      insertions.Identifier = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Identifier && diff.Deletions == deletions.Identifier && diff.Insertions == insertions.Identifier);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitNamedArgument(NamedArgument namedArgument1, NamedArgument namedArgument2){
      Differences differences = new Differences(namedArgument1, namedArgument2);
      if (namedArgument1 == null || namedArgument2 == null){
        if (namedArgument1 != namedArgument2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      NamedArgument changes = (NamedArgument)namedArgument2.Clone();
      NamedArgument deletions = (NamedArgument)namedArgument2.Clone();
      NamedArgument insertions = (NamedArgument)namedArgument2.Clone();

      if (namedArgument1.IsCustomAttributeProperty == namedArgument2.IsCustomAttributeProperty) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitIdentifier(namedArgument1.Name, namedArgument2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(namedArgument1.Value, namedArgument2.Value);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Value = diff.Changes as Expression;
      deletions.Value = diff.Deletions as Expression;
      insertions.Value = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Value && diff.Deletions == deletions.Value && diff.Insertions == insertions.Value);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitNamespace(Namespace nspace1, Namespace nspace2){
      Differences differences = new Differences(nspace1, nspace2);
      if (nspace1 == null || nspace2 == null){
        if (nspace1 != nspace2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Namespace changes = (Namespace)nspace2.Clone();
      Namespace deletions = (Namespace)nspace2.Clone();
      Namespace insertions = (Namespace)nspace2.Clone();

      AliasDefinitionList aliasChanges, aliasDeletions, aliasInsertions;
      Differences diff = this.VisitAliasDefinitionList(nspace1.AliasDefinitions, nspace2.AliasDefinitions, out aliasChanges, out aliasDeletions, out aliasInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AliasDefinitions = aliasChanges;
      deletions.AliasDefinitions = aliasDeletions;
      insertions.AliasDefinitions = aliasInsertions;
      //check not null
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      AttributeList attrChanges, attrDeletions, attrInsertions;
      diff = this.VisitAttributeList(nspace1.Attributes, nspace2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(nspace1.Name, nspace2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      NamespaceList nsChanges, nsDeletions, nsInsertions;
      diff = this.VisitNamespaceList(nspace1.NestedNamespaces, nspace2.NestedNamespaces, out nsChanges, out nsDeletions, out nsInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.NestedNamespaces = nsChanges;
      deletions.NestedNamespaces = nsDeletions;
      insertions.NestedNamespaces = nsInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(nspace1.URI, nspace2.URI);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.URI = diff.Changes as Identifier;
      deletions.URI = diff.Deletions as Identifier;
      insertions.URI = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.URI && diff.Deletions == deletions.URI && diff.Insertions == insertions.URI);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      UsedNamespaceList usedChanges, usedDeletions, usedInsertions;
      diff = this.VisitUsedNamespaceList(nspace1.UsedNamespaces, nspace2.UsedNamespaces, out usedChanges, out usedDeletions, out usedInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.UsedNamespaces = usedChanges;
      deletions.UsedNamespaces = usedDeletions;
      insertions.UsedNamespaces = usedInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(nspace1.Types, nspace2.Types, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Types = typeChanges;
      deletions.Types = typeDeletions;
      insertions.Types = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitNamespaceList(NamespaceList list1, NamespaceList list2,
      out NamespaceList changes, out NamespaceList deletions, out NamespaceList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new NamespaceList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Namespace nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[nd2.Name.UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Namespace nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[nd1.Name.UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        Namespace nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitNamespace(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as Namespace;
          deletions[i] = diff.Deletions as Namespace;
          insertions[i] = diff.Insertions as Namespace;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Namespace nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Namespace nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitNodeList(NodeList list1, NodeList list2,
      out NodeList changes, out NodeList deletions, out NodeList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new NodeList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Node nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Node nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        Node nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as Node;
        deletions[i] = diff.Deletions as Node;
        insertions[i] = diff.Insertions as Node;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Node nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Node nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitParameter(Parameter parameter1, Parameter parameter2){
      Differences differences = new Differences(parameter1, parameter2);
      if (parameter1 == null || parameter2 == null){
        if (parameter1 != parameter2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Parameter changes = (Parameter)parameter2.Clone();
      Parameter deletions = (Parameter)parameter2.Clone();
      Parameter insertions = (Parameter)parameter2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(parameter1.Attributes, parameter2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(parameter1.DefaultValue, parameter2.DefaultValue);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.DefaultValue = diff.Changes as Expression;
      deletions.DefaultValue = diff.Deletions as Expression;
      insertions.DefaultValue = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.DefaultValue && diff.Deletions == deletions.DefaultValue && diff.Insertions == insertions.DefaultValue);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (parameter1.Flags == parameter2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (this.ValuesAreEqual(parameter1.MarshallingInformation,parameter2.MarshallingInformation)) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;      

      diff = this.VisitIdentifier(parameter1.Name, parameter2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitParameterList(ParameterList list1, ParameterList list2,
      out ParameterList changes, out ParameterList deletions, out ParameterList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new ParameterList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Parameter nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        matchingPosFor[nd2.Name.UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Parameter nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        object pos = matchingPosFor[nd1.Name.UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        int j = (int)pos;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        Parameter nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitParameter(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as Parameter;
          deletions[i] = diff.Deletions as Parameter;
          insertions[i] = diff.Insertions as Parameter;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Parameter nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Parameter nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
    public virtual Differences VisitPrefixExpression(PrefixExpression pExpr1, PrefixExpression pExpr2){
      Differences differences = new Differences(pExpr1, pExpr2);
      if (pExpr1 == null || pExpr2 == null){
        if (pExpr1 != pExpr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      PrefixExpression changes = (PrefixExpression)pExpr2.Clone();
      PrefixExpression deletions = (PrefixExpression)pExpr2.Clone();
      PrefixExpression insertions = (PrefixExpression)pExpr2.Clone();

      Differences diff = this.VisitExpression(pExpr1.Expression, pExpr2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (pExpr1.Operator == pExpr2.Operator) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitPostfixExpression(PostfixExpression pExpr1, PostfixExpression pExpr2){
      Differences differences = new Differences(pExpr1, pExpr2);
      if (pExpr1 == null || pExpr2 == null){
        if (pExpr1 != pExpr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      PostfixExpression changes = (PostfixExpression)pExpr2.Clone();
      PostfixExpression deletions = (PostfixExpression)pExpr2.Clone();
      PostfixExpression insertions = (PostfixExpression)pExpr2.Clone();

      Differences diff = this.VisitExpression(pExpr1.Expression, pExpr2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (pExpr1.Operator == pExpr2.Operator) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitProperty(Property property1, Property property2){
      Differences differences = this.GetMemberDifferences(property1, property2);
      if (differences == null){Debug.Assert(false); differences = new Differences(property1, property2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (property1 == null || property2 == null){
        if (property1 != property2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Property changes = (Property)property2.Clone();
      Property deletions = (Property)property2.Clone();
      Property insertions = (Property)property2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(property1.Attributes, property2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (property1.Flags == property2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitMethod(property1.Getter, property2.Getter);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Getter = diff.Changes as Method;
      deletions.Getter = diff.Deletions as Method;
      insertions.Getter = diff.Insertions as Method;
      Debug.Assert(diff.Changes == changes.Getter && diff.Deletions == deletions.Getter && diff.Insertions == insertions.Getter);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(property1.ImplementedTypes, property2.ImplementedTypes, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ImplementedTypes = typeChanges;
      deletions.ImplementedTypes = typeDeletions;
      insertions.ImplementedTypes = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(property1.Name, property2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      MethodList methChanges, methDeletions, methInsertions;
      diff = this.VisitMethodList(property1.OtherMethods, property2.OtherMethods, out methChanges, out methDeletions, out methInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.OtherMethods = methChanges;
      deletions.OtherMethods = methDeletions;
      insertions.OtherMethods = methInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (property1.OverridesBaseClassMember == property2.OverridesBaseClassMember) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      ParameterList parChanges, parDeletions, parInsertions;
      diff = this.VisitParameterList(property1.Parameters, property2.Parameters, out parChanges, out parDeletions, out parInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Parameters = parChanges;
      deletions.Parameters = parDeletions;
      insertions.Parameters = parInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitMethod(property1.Setter, property2.Setter);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Setter = diff.Changes as Method;
      deletions.Setter = diff.Deletions as Method;
      insertions.Setter = diff.Insertions as Method;
      Debug.Assert(diff.Changes == changes.Setter && diff.Deletions == deletions.Setter && diff.Insertions == insertions.Setter);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQualifiedIdentifier(QualifiedIdentifier qualifiedIdentifier1, QualifiedIdentifier qualifiedIdentifier2){
      Differences differences = new Differences(qualifiedIdentifier1, qualifiedIdentifier2);
      if (qualifiedIdentifier1 == null || qualifiedIdentifier2 == null){
        if (qualifiedIdentifier1 != qualifiedIdentifier2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QualifiedIdentifier changes = (QualifiedIdentifier)qualifiedIdentifier2.Clone();
      QualifiedIdentifier deletions = (QualifiedIdentifier)qualifiedIdentifier2.Clone();
      QualifiedIdentifier insertions = (QualifiedIdentifier)qualifiedIdentifier2.Clone();

      Differences diff = this.VisitIdentifier(qualifiedIdentifier1.Identifier, qualifiedIdentifier2.Identifier);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Identifier = diff.Changes as Identifier;
      deletions.Identifier = diff.Deletions as Identifier;
      insertions.Identifier = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Identifier && diff.Deletions == deletions.Identifier && diff.Insertions == insertions.Identifier);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(qualifiedIdentifier1.Qualifier, qualifiedIdentifier2.Qualifier);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Qualifier = diff.Changes as Expression;
      deletions.Qualifier = diff.Deletions as Expression;
      insertions.Qualifier = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Qualifier && diff.Deletions == deletions.Qualifier && diff.Insertions == insertions.Qualifier);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitRepeat(Repeat repeat1, Repeat repeat2){
      Differences differences = new Differences(repeat1, repeat2);
      if (repeat1 == null || repeat2 == null){
        if (repeat1 != repeat2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Repeat changes = (Repeat)repeat2.Clone();
      Repeat deletions = (Repeat)repeat2.Clone();
      Repeat insertions = (Repeat)repeat2.Clone();

      Differences diff = this.VisitBlock(repeat1.Body, repeat2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(repeat1.Condition, repeat2.Condition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Condition = diff.Changes as Expression;
      deletions.Condition = diff.Deletions as Expression;
      insertions.Condition = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Condition && diff.Deletions == deletions.Condition && diff.Insertions == insertions.Condition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitResourceUse(ResourceUse resourceUse1, ResourceUse resourceUse2){
      Differences differences = new Differences(resourceUse1, resourceUse2);
      if (resourceUse1 == null || resourceUse2 == null){
        if (resourceUse1 != resourceUse2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      ResourceUse changes = (ResourceUse)resourceUse2.Clone();
      ResourceUse deletions = (ResourceUse)resourceUse2.Clone();
      ResourceUse insertions = (ResourceUse)resourceUse2.Clone();

      Differences diff = this.VisitBlock(resourceUse1.Body, resourceUse2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitStatement(resourceUse1.ResourceAcquisition, resourceUse2.ResourceAcquisition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ResourceAcquisition = diff.Changes as Statement;
      deletions.ResourceAcquisition = diff.Deletions as Statement;
      insertions.ResourceAcquisition = diff.Insertions as Statement;
      Debug.Assert(diff.Changes == changes.ResourceAcquisition && diff.Deletions == deletions.ResourceAcquisition && diff.Insertions == insertions.ResourceAcquisition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitReturn(Return return1, Return return2){
      Differences differences = new Differences(return1, return2);
      if (return1 == null || return2 == null){
        if (return1 != return2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Return changes = (Return)return2.Clone();
      Return deletions = (Return)return2.Clone();
      Return insertions = (Return)return2.Clone();

      Differences diff = this.VisitExpression(return1.Expression, return2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitSecurityAttribute(SecurityAttribute attribute1, SecurityAttribute attribute2){
      Differences differences = new Differences(attribute1, attribute2);
      if (attribute1 == null || attribute2 == null){
        if (attribute1 != attribute2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      SecurityAttribute changes = (SecurityAttribute)attribute2.Clone();
      SecurityAttribute deletions = (SecurityAttribute)attribute2.Clone();
      SecurityAttribute insertions = (SecurityAttribute)attribute2.Clone();

      if (attribute1.Action == attribute2.Action) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(attribute1.PermissionAttributes, attribute2.PermissionAttributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.PermissionAttributes = attrChanges;
      deletions.PermissionAttributes = attrDeletions;
      insertions.PermissionAttributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (attribute1.PermissionAttributes == null && attribute2.PermissionAttributes == null &&
        attribute1.SerializedPermissions == attribute2.SerializedPermissions) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitSecurityAttributeList(SecurityAttributeList list1, SecurityAttributeList list2,
      out SecurityAttributeList changes, out SecurityAttributeList deletions, out SecurityAttributeList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new SecurityAttributeList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        SecurityAttribute nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        SecurityAttribute nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        SecurityAttribute nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as SecurityAttribute;
        deletions[i] = diff.Deletions as SecurityAttribute;
        insertions[i] = diff.Insertions as SecurityAttribute;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null;
        SecurityAttribute nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        if (changes != null) changes[i] = null;
        if (deletions != null) deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        SecurityAttribute nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitSetterValue(SetterValue value1, SetterValue value2){
      Differences differences = new Differences(value1, value2);
      if (value1 == null || value2 == null){
        if (value1 != value2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else{
        differences.NumberOfSimilarities++;
        differences.Changes = null;
      }
      return differences;
    }
    public virtual Differences VisitStatement(Statement statement1, Statement statement2){
      Differences differences = this.Visit(statement1, statement2);
      if (differences != null && differences.Changes == null && differences.NumberOfDifferences > 0)
        differences.Changes = statement2; //Happens when different types of statemetns are compared (e.g. If vs For)
      return differences;
    }
    public virtual Differences VisitStatementList(StatementList list1, StatementList list2,
      out StatementList changes, out StatementList deletions, out StatementList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new StatementList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        Statement nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Statement nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        Statement nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as Statement;
        deletions[i] = diff.Deletions as Statement;
        insertions[i] = diff.Insertions as Statement;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        Statement nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        Statement nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitStatementSnippet(StatementSnippet snippet1, StatementSnippet snippet2){
      Differences differences = new Differences(snippet1, snippet2);
      if (snippet1 == null || snippet2 == null){
        if (snippet1 != snippet2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      StatementSnippet changes = (StatementSnippet)snippet2.Clone();
      StatementSnippet deletions = (StatementSnippet)snippet2.Clone();
      StatementSnippet insertions = (StatementSnippet)snippet2.Clone();

      if (snippet1.SourceContext.Document == null || snippet2.SourceContext.Document == null){
        if (snippet1.SourceContext.Document == snippet2.SourceContext.Document) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      }else if (snippet1.SourceContext.Document.Name == snippet2.SourceContext.Document.Name) 
        differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitStaticInitializer(StaticInitializer cons1, StaticInitializer cons2){
      return this.VisitMethod(cons1, cons2);
    }
    public virtual Differences VisitStruct(Struct struct1, Struct struct2){
      return this.VisitTypeNode(struct1, struct2);
    }
    public virtual Differences VisitSwitch(Switch switch1, Switch switch2){
      Differences differences = new Differences(switch1, switch2);
      if (switch1 == null || switch2 == null){
        if (switch1 != switch2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Switch changes = (Switch)switch2.Clone();
      Switch deletions = (Switch)switch2.Clone();
      Switch insertions = (Switch)switch2.Clone();
      
      SwitchCaseList caseChanges, caseDeletions, caseInsertions;
      Differences diff = this.VisitSwitchCaseList(switch1.Cases, switch2.Cases, out caseChanges, out caseDeletions, out caseInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Cases = caseChanges;
      deletions.Cases = caseDeletions;
      insertions.Cases = caseInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(switch1.Expression, switch2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitSwitchCase(SwitchCase switchCase1, SwitchCase switchCase2){
      Differences differences = new Differences(switchCase1, switchCase2);
      if (switchCase1 == null || switchCase2 == null){
        if (switchCase1 != switchCase2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      SwitchCase changes = (SwitchCase)switchCase2.Clone();
      SwitchCase deletions = (SwitchCase)switchCase2.Clone();
      SwitchCase insertions = (SwitchCase)switchCase2.Clone();

      Differences diff = this.VisitBlock(switchCase1.Body, switchCase2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(switchCase1.Label, switchCase2.Label);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Label = diff.Changes as Expression;
      deletions.Label = diff.Deletions as Expression;
      insertions.Label = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Label && diff.Deletions == deletions.Label && diff.Insertions == insertions.Label);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitSwitchCaseList(SwitchCaseList list1, SwitchCaseList list2,
      out SwitchCaseList changes, out SwitchCaseList deletions, out SwitchCaseList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new SwitchCaseList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        SwitchCase nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        SwitchCase nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        SwitchCase nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as SwitchCase;
        deletions[i] = diff.Deletions as SwitchCase;
        insertions[i] = diff.Insertions as SwitchCase;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        SwitchCase nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        SwitchCase nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitSwitchInstruction(SwitchInstruction switchInstruction1, SwitchInstruction switchInstruction2){
      Differences differences = new Differences(switchInstruction1, switchInstruction2);
      if (switchInstruction1 == null || switchInstruction2 == null){
        if (switchInstruction1 != switchInstruction2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      SwitchInstruction changes = (SwitchInstruction)switchInstruction2.Clone();
      SwitchInstruction deletions = (SwitchInstruction)switchInstruction2.Clone();
      SwitchInstruction insertions = (SwitchInstruction)switchInstruction2.Clone();

      Differences diff = this.VisitExpression(switchInstruction1.Expression, switchInstruction2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      BlockList blockChanges, blockDeletions, blockInsertions;
      diff = this.VisitBlockList(switchInstruction1.Targets, switchInstruction2.Targets, out blockChanges, out blockDeletions, out blockInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Targets = blockChanges;
      deletions.Targets = blockDeletions;
      insertions.Targets = blockInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeswitch(Typeswitch typeswitch1, Typeswitch typeswitch2){
      Differences differences = new Differences(typeswitch1, typeswitch2);
      if (typeswitch1 == null || typeswitch2 == null){
        if (typeswitch1 != typeswitch2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Typeswitch changes = (Typeswitch)typeswitch2.Clone();
      Typeswitch deletions = (Typeswitch)typeswitch2.Clone();
      Typeswitch insertions = (Typeswitch)typeswitch2.Clone();

      TypeswitchCaseList caseChanges, caseDeletions, caseInsertions;
      Differences diff = this.VisitTypeswitchCaseList(typeswitch1.Cases, typeswitch2.Cases, out caseChanges, out caseDeletions, out caseInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Cases = caseChanges;
      deletions.Cases = caseDeletions;
      insertions.Cases = caseInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(typeswitch1.Expression, typeswitch2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeswitchCase(TypeswitchCase typeswitchCase1, TypeswitchCase typeswitchCase2){
      Differences differences = new Differences(typeswitchCase1, typeswitchCase2);
      if (typeswitchCase1 == null || typeswitchCase2 == null){
        if (typeswitchCase1 != typeswitchCase2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeswitchCase changes = (TypeswitchCase)typeswitchCase2.Clone();
      TypeswitchCase deletions = (TypeswitchCase)typeswitchCase2.Clone();
      TypeswitchCase insertions = (TypeswitchCase)typeswitchCase2.Clone();

      Differences diff = this.VisitBlock(typeswitchCase1.Body, typeswitchCase2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(typeswitchCase1.LabelType, typeswitchCase2.LabelType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.LabelType = diff.Changes as TypeNode;
      deletions.LabelType = diff.Deletions as TypeNode;
      insertions.LabelType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.LabelType && diff.Deletions == deletions.LabelType && diff.Insertions == insertions.LabelType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(typeswitchCase1.LabelVariable, typeswitchCase2.LabelVariable);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.LabelVariable = diff.Changes as Expression;
      deletions.LabelVariable = diff.Deletions as Expression;
      insertions.LabelVariable = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.LabelVariable && diff.Deletions == deletions.LabelVariable && diff.Insertions == insertions.LabelVariable);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeswitchCaseList(TypeswitchCaseList list1, TypeswitchCaseList list2,
      out TypeswitchCaseList changes, out TypeswitchCaseList deletions, out TypeswitchCaseList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new TypeswitchCaseList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        TypeswitchCase nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        TypeswitchCase nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        TypeswitchCase nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as TypeswitchCase;
        deletions[i] = diff.Deletions as TypeswitchCase;
        insertions[i] = diff.Insertions as TypeswitchCase;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        TypeswitchCase nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        TypeswitchCase nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitTernaryExpression(TernaryExpression expression1, TernaryExpression expression2){
      Differences differences = new Differences(expression1, expression2);
      if (expression1 == null || expression2 == null){
        if (expression1 != expression2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TernaryExpression changes = (TernaryExpression)expression2.Clone();
      TernaryExpression deletions = (TernaryExpression)expression2.Clone();
      TernaryExpression insertions = (TernaryExpression)expression2.Clone();

      if (expression1.NodeType == expression2.NodeType) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      Differences diff = this.VisitExpression(expression1.Operand1, expression2.Operand1);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand1 = diff.Changes as Expression;
      deletions.Operand1 = diff.Deletions as Expression;
      insertions.Operand1 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand1 && diff.Deletions == deletions.Operand1 && diff.Insertions == insertions.Operand1);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(expression1.Operand2, expression2.Operand2);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand2 = diff.Changes as Expression;
      deletions.Operand2 = diff.Deletions as Expression;
      insertions.Operand2 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand2 && diff.Deletions == deletions.Operand2 && diff.Insertions == insertions.Operand2);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(expression1.Operand3, expression2.Operand3);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand3 = diff.Changes as Expression;
      deletions.Operand3 = diff.Deletions as Expression;
      insertions.Operand3 = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand3 && diff.Deletions == deletions.Operand3 && diff.Insertions == insertions.Operand3);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitThis(This this1, This this2){
      Differences differences = new Differences(this1, this2);
      if (this1 == null || this2 == null){
        if (this1 != this2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else{
        differences.NumberOfSimilarities++;
        differences.Changes = null;
      }
      return differences;
    }
    public virtual Differences VisitThrow(Throw throw1, Throw throw2){
      Differences differences = new Differences(throw1, throw2);
      if (throw1 == null || throw2 == null){
        if (throw1 != throw2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Throw changes = (Throw)throw2.Clone();
      Throw deletions = (Throw)throw2.Clone();
      Throw insertions = (Throw)throw2.Clone();

      Differences diff = this.VisitExpression(throw1.Expression, throw2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTry(Try try1, Try try2){
      Differences differences = new Differences(try1, try2);
      if (try1 == null || try2 == null){
        if (try1 != try2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Try changes = (Try)try2.Clone();
      Try deletions = (Try)try2.Clone();
      Try insertions = (Try)try2.Clone();

      CatchList catchChanges, catchDeletions, catchInsertions;
      Differences diff = this.VisitCatchList(try1.Catchers, try2.Catchers, out catchChanges, out catchDeletions, out catchInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Catchers = catchChanges;
      deletions.Catchers = catchDeletions;
      insertions.Catchers = catchInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      FaultHandlerList faultChanges, faultDeletions, faultInsertions;
      diff = this.VisitFaultHandlerList(try1.FaultHandlers, try2.FaultHandlers, out faultChanges, out faultDeletions, out faultInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.FaultHandlers = faultChanges;
      deletions.FaultHandlers = faultDeletions;
      insertions.FaultHandlers = faultInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      FilterList filterChanges, filterDeletions, filterInsertions;
      diff = this.VisitFilterList(try1.Filters, try2.Filters, out filterChanges, out filterDeletions, out filterInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Filters = filterChanges;
      deletions.Filters = filterDeletions;
      insertions.Filters = filterInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitFinally(try1.Finally, try2.Finally);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Finally = diff.Changes as Finally;
      deletions.Finally = diff.Deletions as Finally;
      insertions.Finally = diff.Insertions as Finally;
      Debug.Assert(diff.Changes == changes.Finally && diff.Deletions == deletions.Finally && diff.Insertions == insertions.Finally);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitBlock(try1.TryBlock, try2.TryBlock);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TryBlock = diff.Changes as Block;
      deletions.TryBlock = diff.Deletions as Block;
      insertions.TryBlock = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.TryBlock && diff.Deletions == deletions.TryBlock && diff.Insertions == insertions.TryBlock);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#if ExtendedRuntime    
    public virtual Differences VisitTupleType(TupleType tuple1, TupleType tuple2){
      return this.VisitTypeNode(tuple1, tuple2);
    }
    public virtual Differences VisitTypeAlias(TypeAlias tAlias1, TypeAlias tAlias2){
      Differences differences = this.GetMemberDifferences(tAlias1, tAlias2);
      if (differences == null){Debug.Assert(false); differences = new Differences(tAlias1, tAlias2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (tAlias1 == null || tAlias2 == null){
        if (tAlias1 != tAlias2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeAlias changes = (TypeAlias)tAlias2.Clone();
      TypeAlias deletions = (TypeAlias)tAlias2.Clone();
      TypeAlias insertions = (TypeAlias)tAlias2.Clone();

      Differences diff = this.VisitTypeNode(tAlias1.AliasedType, tAlias2.AliasedType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AliasedType = diff.Changes as TypeNode;
      deletions.AliasedType = diff.Deletions as TypeNode;
      insertions.AliasedType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.AliasedType && diff.Deletions == deletions.AliasedType && diff.Insertions == insertions.AliasedType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      AttributeList attrChanges, attrDeletions, attrInsertions;
      diff = this.VisitAttributeList(tAlias1.Attributes, tAlias2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (tAlias1.Flags == tAlias2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitIdentifier(tAlias1.Name, tAlias2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(tAlias1.TemplateParameters, tAlias2.TemplateParameters, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateParameters = typeChanges;
      deletions.TemplateParameters = typeDeletions;
      insertions.TemplateParameters = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeIntersection(TypeIntersection typeIntersection1, TypeIntersection typeIntersection2){
      Differences differences = this.GetMemberDifferences(typeIntersection1, typeIntersection2);
      if (differences == null){Debug.Assert(false); differences = new Differences(typeIntersection1, typeIntersection2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (typeIntersection1 == null || typeIntersection2 == null){
        if (typeIntersection1 != typeIntersection2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeIntersection changes = (TypeIntersection)typeIntersection2.Clone();
      TypeIntersection deletions = (TypeIntersection)typeIntersection2.Clone();
      TypeIntersection insertions = (TypeIntersection)typeIntersection2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(typeIntersection1.Attributes, typeIntersection2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (typeIntersection1.Flags == typeIntersection2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitIdentifier(typeIntersection1.Name, typeIntersection2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(typeIntersection1.TemplateParameters, typeIntersection2.TemplateParameters, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateParameters = typeChanges;
      deletions.TemplateParameters = typeDeletions;
      insertions.TemplateParameters = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNodeList(typeIntersection1.Types, typeIntersection2.Types, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Types = typeChanges;
      deletions.Types = typeDeletions;
      insertions.Types = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#endif
    public virtual Differences VisitTypeMemberSnippet(TypeMemberSnippet snippet1, TypeMemberSnippet snippet2){
      Differences differences = new Differences(snippet1, snippet2);
      if (snippet1 == null || snippet2 == null){
        if (snippet1 != snippet2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeMemberSnippet changes = (TypeMemberSnippet)snippet2.Clone();
      TypeMemberSnippet deletions = (TypeMemberSnippet)snippet2.Clone();
      TypeMemberSnippet insertions = (TypeMemberSnippet)snippet2.Clone();

      if (snippet1.SourceContext.Document == null || snippet2.SourceContext.Document == null){
        if (snippet1.SourceContext.Document == snippet2.SourceContext.Document) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      }else if (snippet1.SourceContext.Document.Name == snippet2.SourceContext.Document.Name) 
        differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeModifier(TypeModifier typeModifier1, TypeModifier typeModifier2){
      Differences differences = new Differences(typeModifier1, typeModifier2);
      if (typeModifier1 == null || typeModifier2 == null){
        if (typeModifier1 != typeModifier2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeModifier changes = (TypeModifier)typeModifier2.Clone();
      TypeModifier deletions = (TypeModifier)typeModifier2.Clone();
      TypeModifier insertions = (TypeModifier)typeModifier2.Clone();

      Differences diff = this.VisitTypeNode(typeModifier1.ModifiedType, typeModifier2.ModifiedType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.ModifiedType = diff.Changes as TypeNode;
      deletions.ModifiedType = diff.Deletions as TypeNode;
      insertions.ModifiedType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.ModifiedType && diff.Deletions == deletions.ModifiedType && diff.Insertions == insertions.ModifiedType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(typeModifier1.Modifier, typeModifier2.Modifier);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Modifier = diff.Changes as TypeNode;
      deletions.Modifier = diff.Deletions as TypeNode;
      insertions.Modifier = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Modifier && diff.Deletions == deletions.Modifier && diff.Insertions == insertions.Modifier);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeNode(TypeNode typeNode1, TypeNode typeNode2){
      Differences differences = this.GetMemberDifferences(typeNode1, typeNode2);
      if (differences == null){Debug.Assert(false); differences = new Differences(typeNode1, typeNode2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (typeNode1 == null || typeNode2 == null){
        if (typeNode1 != typeNode2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeNode changes = (TypeNode)typeNode2.Clone();
      TypeNode deletions = (TypeNode)typeNode2.Clone();
      TypeNode insertions = (TypeNode)typeNode2.Clone();

      //Short circuit comparison if assembly qualified full name of typeNode1 is not the same as that of typeNode2
      string tName1 = typeNode1.FullName;
      string tName2 = typeNode2.FullName;
      if (string.CompareOrdinal(tName1, tName2) != 0){
        differences.NumberOfDifferences++;
        goto done;
      }
      if (typeNode1.DeclaringModule != this.OriginalModule || typeNode2.DeclaringModule != this.NewModule){
        differences.NumberOfDifferences++;
        goto done;
      }

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(typeNode1.Attributes, typeNode2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(typeNode1.BaseType, typeNode2.BaseType);
      if (diff == null){Debug.Assert(false); return differences;}
      //BaseType is read only. Class overrides this method and updates changes, deletions and insertions
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (typeNode1.ClassSize == typeNode2.ClassSize) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitTypeNode(typeNode1.DeclaringType, typeNode2.DeclaringType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.DeclaringType = diff.Changes as TypeNode;
      deletions.DeclaringType = diff.Deletions as TypeNode;
      insertions.DeclaringType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.DeclaringType && diff.Deletions == deletions.DeclaringType && diff.Insertions == insertions.DeclaringType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (typeNode1.Flags == typeNode2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (typeNode1.HidesBaseClassMember == typeNode2.HidesBaseClassMember) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      InterfaceList ifaceChanges, ifaceDeletions, ifaceInsertions;
      diff = this.VisitInterfaceReferenceList(typeNode1.Interfaces, typeNode2.Interfaces, out ifaceChanges, out ifaceDeletions, out ifaceInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Interfaces = ifaceChanges;
      deletions.Interfaces = ifaceDeletions;
      insertions.Interfaces = ifaceInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (typeNode1.IsGeneric == typeNode2.IsGeneric) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      MemberList memChanges, memDeletions, memInsertions;
      diff = this.VisitMemberList(typeNode1.Members, typeNode2.Members, out memChanges, out memDeletions, out memInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Members = memChanges;
      deletions.Members = memDeletions;
      insertions.Members = memInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(typeNode1.Name, typeNode2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(typeNode1.Namespace, typeNode2.Namespace);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Namespace = diff.Changes as Identifier;
      deletions.Namespace = diff.Deletions as Identifier;
      insertions.Namespace = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Namespace && diff.Deletions == deletions.Namespace && diff.Insertions == insertions.Namespace);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (typeNode1.NodeType == typeNode2.NodeType) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      if (typeNode1.PackingSize == typeNode2.PackingSize) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      SecurityAttributeList secChanges, secDeletions, secInsertions;
      diff = this.VisitSecurityAttributeList(typeNode1.SecurityAttributes, typeNode2.SecurityAttributes, out secChanges, out secDeletions, out secInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.SecurityAttributes = secChanges;
      deletions.SecurityAttributes = secDeletions;
      insertions.SecurityAttributes = secInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNode(typeNode1.Template, typeNode2.Template);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Template = diff.Changes as TypeNode;
      deletions.Template = diff.Deletions as TypeNode;
      insertions.Template = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Template && diff.Deletions == deletions.Template && diff.Insertions == insertions.Template);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(typeNode1.TemplateArguments, typeNode2.TemplateArguments, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateArguments = typeChanges;
      deletions.TemplateArguments = typeDeletions;
      insertions.TemplateArguments = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNodeList(typeNode1.TemplateParameters, typeNode2.TemplateParameters, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateParameters = typeChanges;
      deletions.TemplateParameters = typeDeletions;
      insertions.TemplateParameters = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

    done:
      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitTypeNodeList(TypeNodeList list1, TypeNodeList list2,
      out TypeNodeList changes, out TypeNodeList deletions, out TypeNodeList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new TypeNodeList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      //Compare definitions that have matching key attributes
      TrivialHashtable matchingPosFor = new TrivialHashtable();
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        TypeNode nd2 = list2[j];
        if (nd2 == null || nd2.Name == null) continue;
        string fullName = nd2.FullName;
        if (fullName == null) continue;
        matchingPosFor[Identifier.For(fullName).UniqueIdKey] = j;
        insertions.Add(null);
      }
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        TypeNode nd1 = list1[i];
        if (nd1 == null || nd1.Name == null) continue;
        string fullName = nd1.FullName;
        if (fullName == null) continue;
        object pos = matchingPosFor[Identifier.For(fullName).UniqueIdKey];
        if (!(pos is int)) continue;
        //^ assert pos != null;
        //^ assume list2 != null; //since there was entry int matchingPosFor
        int j = (int)pos;
        TypeNode nd2 = list2[j];
        //^ assume nd2 != null;
        //nd1 and nd2 have the same key attributes and are therefore treated as the same entity
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        //nd1 and nd2 may still be different, though, so find out how different
        Differences diff = this.VisitTypeNode(nd1, nd2);
        if (diff == null){Debug.Assert(false); continue;}
        if (diff.NumberOfDifferences != 0){
          changes[i] = diff.Changes as TypeNode;
          deletions[i] = diff.Deletions as TypeNode;
          insertions[i] = diff.Insertions as TypeNode;
          insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
          //Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
          differences.NumberOfDifferences += diff.NumberOfDifferences;
          differences.NumberOfSimilarities += diff.NumberOfSimilarities;
          if (nd1.DeclaringModule == this.OriginalModule || 
            (nd1.DeclaringType != null && nd1.DeclaringType.DeclaringModule == this.OriginalModule)){
            if (this.MembersThatHaveChanged == null) this.MembersThatHaveChanged = new MemberList();
            this.MembersThatHaveChanged.Add(nd1);
          }
          continue;
        }
        changes[i] = null;
        deletions[i] = null;
        insertions[i] = null;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        TypeNode nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
        if (nd1.DeclaringModule == this.OriginalModule || 
          (nd1.DeclaringType != null && nd1.DeclaringType.DeclaringModule == this.OriginalModule)){
          if (this.MembersThatHaveChanged == null) this.MembersThatHaveChanged = new MemberList();
          this.MembersThatHaveChanged.Add(nd1);
        }
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        TypeNode nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      return differences;
    }
#if ExtendedRuntime    
    public virtual Differences VisitTypeUnion(TypeUnion typeUnion1, TypeUnion typeUnion2){
      Differences differences = this.GetMemberDifferences(typeUnion1, typeUnion2);
      if (differences == null){Debug.Assert(false); differences = new Differences(typeUnion1, typeUnion2);}
      if (differences.NumberOfDifferences > 0 || differences.NumberOfSimilarities > 0) return differences;
      if (typeUnion1 == null || typeUnion2 == null){
        if (typeUnion1 != typeUnion2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      TypeUnion changes = (TypeUnion)typeUnion2.Clone();
      TypeUnion deletions = (TypeUnion)typeUnion2.Clone();
      TypeUnion insertions = (TypeUnion)typeUnion2.Clone();

      AttributeList attrChanges, attrDeletions, attrInsertions;
      Differences diff = this.VisitAttributeList(typeUnion1.Attributes, typeUnion2.Attributes, out attrChanges, out attrDeletions, out attrInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Attributes = attrChanges;
      deletions.Attributes = attrDeletions;
      insertions.Attributes = attrInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (typeUnion1.Flags == typeUnion2.Flags) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;

      diff = this.VisitIdentifier(typeUnion1.Name, typeUnion2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      TypeNodeList typeChanges, typeDeletions, typeInsertions;
      diff = this.VisitTypeNodeList(typeUnion1.TemplateParameters, typeUnion2.TemplateParameters, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.TemplateParameters = typeChanges;
      deletions.TemplateParameters = typeDeletions;
      insertions.TemplateParameters = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitTypeNodeList(typeUnion1.Types, typeUnion2.Types, out typeChanges, out typeDeletions, out typeInsertions);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Types = typeChanges;
      deletions.Types = typeDeletions;
      insertions.Types = typeInsertions;
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#endif
    public virtual Differences VisitTypeReference(TypeReference reference1, TypeReference reference2){
      Differences differences = new Differences(reference1, reference2);
      if (reference1 == null || reference2 == null){
        if (reference1 != reference2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      UnaryExpression changes = (UnaryExpression)reference2.Clone();
      UnaryExpression deletions = (UnaryExpression)reference2.Clone();
      UnaryExpression insertions = (UnaryExpression)reference2.Clone();

      Differences diff = this.VisitTypeNode(reference1.Type, reference2.Type);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Type = diff.Changes as TypeNode;
      deletions.Type = diff.Deletions as TypeNode;
      insertions.Type = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.Type && diff.Deletions == deletions.Type && diff.Insertions == insertions.Type);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitUnaryExpression(UnaryExpression unaryExpression1, UnaryExpression unaryExpression2){
      Differences differences = new Differences(unaryExpression1, unaryExpression2);
      if (unaryExpression1 == null || unaryExpression2 == null){
        if (unaryExpression1 != unaryExpression2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      UnaryExpression changes = (UnaryExpression)unaryExpression2.Clone();
      UnaryExpression deletions = (UnaryExpression)unaryExpression2.Clone();
      UnaryExpression insertions = (UnaryExpression)unaryExpression2.Clone();

      if (unaryExpression1.NodeType == unaryExpression2.NodeType) differences.NumberOfSimilarities++; else differences.NumberOfDifferences++;
      
      Differences diff = this.VisitExpression(unaryExpression1.Operand, unaryExpression2.Operand);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Operand = diff.Changes as Expression;
      deletions.Operand = diff.Deletions as Expression;
      insertions.Operand = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Operand && diff.Deletions == deletions.Operand && diff.Insertions == insertions.Operand);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitVariableDeclaration(VariableDeclaration variableDeclaration1, VariableDeclaration variableDeclaration2){
      Differences differences = new Differences(variableDeclaration1, variableDeclaration2);
      if (variableDeclaration1 == null || variableDeclaration2 == null){
        if (variableDeclaration1 != variableDeclaration2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      VariableDeclaration changes = (VariableDeclaration)variableDeclaration2.Clone();
      VariableDeclaration deletions = (VariableDeclaration)variableDeclaration2.Clone();
      VariableDeclaration insertions = (VariableDeclaration)variableDeclaration2.Clone();

      //      variableDeclaration1.Field;
      //      variableDeclaration1.Initializer;
      //      variableDeclaration1.Name;
      //      variableDeclaration1.Type;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitUsedNamespace(UsedNamespace usedNamespace1, UsedNamespace usedNamespace2){
      Differences differences = new Differences(usedNamespace1, usedNamespace2);
      if (usedNamespace1 == null || usedNamespace2 == null){
        if (usedNamespace1 != usedNamespace2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      UsedNamespace changes = (UsedNamespace)usedNamespace2.Clone();
      UsedNamespace deletions = (UsedNamespace)usedNamespace2.Clone();
      UsedNamespace insertions = (UsedNamespace)usedNamespace2.Clone();

      Differences diff = this.VisitIdentifier(usedNamespace1.Namespace, usedNamespace2.Namespace);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Namespace = diff.Changes as Identifier;
      deletions.Namespace = diff.Deletions as Identifier;
      insertions.Namespace = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Namespace && diff.Deletions == deletions.Namespace && diff.Insertions == insertions.Namespace);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(usedNamespace1.URI, usedNamespace2.URI);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.URI = diff.Changes as Identifier;
      deletions.URI = diff.Deletions as Identifier;
      insertions.URI = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.URI && diff.Deletions == deletions.URI && diff.Insertions == insertions.URI);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitUsedNamespaceList(UsedNamespaceList list1, UsedNamespaceList list2,
      out UsedNamespaceList changes, out UsedNamespaceList deletions, out UsedNamespaceList insertions){
      changes = list1 == null ? null : list1.Clone();
      deletions = list1 == null ? null : list1.Clone();
      insertions = list1 == null ? new UsedNamespaceList() : list1.Clone();
      //^ assert insertions != null;
      Differences differences = new Differences();
      for (int j = 0, n = list2 == null ? 0 : list2.Count; j < n; j++){
        //^ assert list2 != null;
        UsedNamespace nd2 = list2[j];
        if (nd2 == null) continue;
        insertions.Add(null);
      }
      TrivialHashtable savedDifferencesMapFor = this.differencesMapFor;
      this.differencesMapFor = null;
      TrivialHashtable matchedNodes = new TrivialHashtable();
      for (int i = 0, k = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        UsedNamespace nd1 = list1[i]; 
        if (nd1 == null) continue;
        Differences diff;
        int j;
        UsedNamespace nd2 = this.GetClosestMatch(nd1, list1, list2, i, ref k, matchedNodes, out diff, out j);
        if (nd2 == null || diff == null){Debug.Assert(nd2 == null && diff == null); continue;}
        matchedNodes[nd1.UniqueKey] = nd1;
        matchedNodes[nd2.UniqueKey] = nd2;
        changes[i] = diff.Changes as UsedNamespace;
        deletions[i] = diff.Deletions as UsedNamespace;
        insertions[i] = diff.Insertions as UsedNamespace;
        insertions[n+j] = nd1; //Records the position of nd2 in list2 in case the change involved a permutation
        Debug.Assert(diff.Changes == changes[i] && diff.Deletions == deletions[i] && diff.Insertions == insertions[i]);
        differences.NumberOfDifferences += diff.NumberOfDifferences;
        differences.NumberOfSimilarities += diff.NumberOfSimilarities;
      }
      //Find deletions
      for (int i = 0, n = list1 == null ? 0 : list1.Count; i < n; i++){
        //^ assert list1 != null && changes != null && deletions != null;
        UsedNamespace nd1 = list1[i]; 
        if (nd1 == null) continue;
        if (matchedNodes[nd1.UniqueKey] != null) continue;
        changes[i] = null;
        deletions[i] = nd1;
        insertions[i] = null;
        differences.NumberOfDifferences += 1;
      }
      //Find insertions
      for (int j = 0, n = list1 == null ? 0 : list1.Count, m = list2 == null ? 0 : list2.Count; j < m; j++){
        //^ assert list2 != null;
        UsedNamespace nd2 = list2[j]; 
        if (nd2 == null) continue;
        if (matchedNodes[nd2.UniqueKey] != null) continue;
        insertions[n+j] = nd2;  //Records nd2 as an insertion into list1, along with its position in list2
        differences.NumberOfDifferences += 1; //REVIEW: put the size of the tree here?
      }
      if (differences.NumberOfDifferences == 0){
        changes = null;
        deletions = null;
        insertions = null;
      }
      this.differencesMapFor = savedDifferencesMapFor;
      return differences;
    }
    public virtual Differences VisitWhile(While while1, While while2){
      Differences differences = new Differences(while1, while2);
      if (while1 == null || while2 == null){
        if (while1 != while2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      While changes = (While)while2.Clone();
      While deletions = (While)while2.Clone();
      While insertions = (While)while2.Clone();

      Differences diff = this.VisitBlock(while1.Body, while2.Body);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Body = diff.Changes as Block;
      deletions.Body = diff.Deletions as Block;
      insertions.Body = diff.Insertions as Block;
      Debug.Assert(diff.Changes == changes.Body && diff.Deletions == deletions.Body && diff.Insertions == insertions.Body);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpressionList(while1.Invariants, while2.Invariants, out changes.Invariants, out deletions.Invariants, out insertions.Invariants);
      if (diff == null){Debug.Assert(false); return differences;}
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(while1.Condition, while2.Condition);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Condition = diff.Changes as Expression;
      deletions.Condition = diff.Deletions as Expression;
      insertions.Condition = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Condition && diff.Deletions == deletions.Condition && diff.Insertions == insertions.Condition);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitYield(Yield yield1, Yield yield2){
      Differences differences = new Differences(yield1, yield2);
      if (yield1 == null || yield2 == null){
        if (yield1 != yield2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      Yield changes = (Yield)yield2.Clone();
      Yield deletions = (Yield)yield2.Clone();
      Yield insertions = (Yield)yield2.Clone();

      Differences diff = this.VisitExpression(yield1.Expression, yield2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#if ExtendedRuntime
    // query nodes
    public virtual Differences VisitQueryAggregate(QueryAggregate qa1, QueryAggregate qa2){
      Differences differences = new Differences(qa1, qa2);
      if (qa1 == null || qa2 == null){
        if (qa1 != qa2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryAggregate changes = (QueryAggregate)qa2.Clone();
      QueryAggregate deletions = (QueryAggregate)qa2.Clone();
      QueryAggregate insertions = (QueryAggregate)qa2.Clone();

      Differences diff = this.VisitTypeNode(qa1.AggregateType, qa2.AggregateType);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.AggregateType = diff.Changes as TypeNode;
      deletions.AggregateType = diff.Deletions as TypeNode;
      insertions.AggregateType = diff.Insertions as TypeNode;
      //Debug.Assert(diff.Changes == changes.AggregateType && diff.Deletions == deletions.AggregateType && diff.Insertions == insertions.AggregateType);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitExpression(qa1.Expression, qa2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitQueryGroupBy(qa1.Group, qa2.Group);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Group = diff.Changes as QueryGroupBy;
      deletions.Group = diff.Deletions as QueryGroupBy;
      insertions.Group = diff.Insertions as QueryGroupBy;
      Debug.Assert(diff.Changes == changes.Group && diff.Deletions == deletions.Group && diff.Insertions == insertions.Group);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(qa1.Name, qa2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryAlias(QueryAlias alias1, QueryAlias alias2){
      Differences differences = new Differences(alias1, alias2);
      if (alias1 == null || alias2 == null){
        if (alias1 != alias2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryAlias changes = (QueryAlias)alias2.Clone();
      QueryAlias deletions = (QueryAlias)alias2.Clone();
      QueryAlias insertions = (QueryAlias)alias2.Clone();

      Differences diff = this.VisitExpression(alias1.Expression, alias2.Expression);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Expression = diff.Changes as Expression;
      deletions.Expression = diff.Deletions as Expression;
      insertions.Expression = diff.Insertions as Expression;
      Debug.Assert(diff.Changes == changes.Expression && diff.Deletions == deletions.Expression && diff.Insertions == insertions.Expression);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      diff = this.VisitIdentifier(alias1.Name, alias2.Name);
      if (diff == null){Debug.Assert(false); return differences;}
      changes.Name = diff.Changes as Identifier;
      deletions.Name = diff.Deletions as Identifier;
      insertions.Name = diff.Insertions as Identifier;
      Debug.Assert(diff.Changes == changes.Name && diff.Deletions == deletions.Name && diff.Insertions == insertions.Name);
      differences.NumberOfDifferences += diff.NumberOfDifferences;
      differences.NumberOfSimilarities += diff.NumberOfSimilarities;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryAxis(QueryAxis axis1, QueryAxis axis2){
      Differences differences = new Differences(axis1, axis2);
      if (axis1 == null || axis2 == null){
        if (axis1 != axis2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryAxis changes = (QueryAxis)axis2.Clone();
      QueryAxis deletions = (QueryAxis)axis2.Clone();
      QueryAxis insertions = (QueryAxis)axis2.Clone();

      //      axis1.AccessPlan;
      //      axis1.Cardinality;
      //      axis1.IsCyclic;
      //      axis1.IsDescendant;
      //      axis1.IsIterative;
      //      axis1.Name;
      //      axis1.Namespace;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryCommit(QueryCommit qc1, QueryCommit qc2){
      return new Differences(qc1);
    }
    public virtual Differences VisitQueryContext(QueryContext context1, QueryContext context2){
      Differences differences = new Differences(context1, context2);
      if (context1 == null || context2 == null){
        if (context1 != context2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryContext changes = (QueryContext)context2.Clone();
      QueryContext deletions = (QueryContext)context2.Clone();
      QueryContext insertions = (QueryContext)context2.Clone();

      //context1.Scope;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryDelete(QueryDelete delete1, QueryDelete delete2){
      Differences differences = new Differences(delete1, delete2);
      if (delete1 == null || delete2 == null){
        if (delete1 != delete2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryDelete changes = (QueryDelete)delete2.Clone();
      QueryDelete deletions = (QueryDelete)delete2.Clone();
      QueryDelete insertions = (QueryDelete)delete2.Clone();

      //      delete1.Context;
      //      delete1.Source;
      //      delete1.Target;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryDifference(QueryDifference diff1, QueryDifference diff2){
      Differences differences = new Differences(diff1, diff2);
      if (diff1 == null || diff2 == null){
        if (diff1 != diff2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryDifference changes = (QueryDifference)diff2.Clone();
      QueryDifference deletions = (QueryDifference)diff2.Clone();
      QueryDifference insertions = (QueryDifference)diff2.Clone();

      //      diff1.LeftSource;
      //      diff1.RightSource;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryDistinct(QueryDistinct distinct1, QueryDistinct distinct2){
      Differences differences = new Differences(distinct1, distinct2);
      if (distinct1 == null || distinct2 == null){
        if (distinct1 != distinct2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryDistinct changes = (QueryDistinct)distinct2.Clone();
      QueryDistinct deletions = (QueryDistinct)distinct2.Clone();
      QueryDistinct insertions = (QueryDistinct)distinct2.Clone();

      //      distinct1.Context;
      //      distinct1.Group;
      //      distinct1.GroupTarget;
      //      distinct1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryExists(QueryExists exists1, QueryExists exists2){
      Differences differences = new Differences(exists1, exists2);
      if (exists1 == null || exists2 == null){
        if (exists1 != exists2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryExists changes = (QueryExists)exists2.Clone();
      QueryExists deletions = (QueryExists)exists2.Clone();
      QueryExists insertions = (QueryExists)exists2.Clone();

      //      exists1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryFilter(QueryFilter filter1, QueryFilter filter2){
      Differences differences = new Differences(filter1, filter2);
      if (filter1 == null || filter2 == null){
        if (filter1 != filter2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryFilter changes = (QueryFilter)filter2.Clone();
      QueryFilter deletions = (QueryFilter)filter2.Clone();
      QueryFilter insertions = (QueryFilter)filter2.Clone();

      //      filter1.Context;
      //      filter1.Expression;
      //      filter1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryGroupBy(QueryGroupBy groupby1, QueryGroupBy groupby2){
      Differences differences = new Differences(groupby1, groupby2);
      if (groupby1 == null || groupby2 == null){
        if (groupby1 != groupby2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryGroupBy changes = (QueryGroupBy)groupby2.Clone();
      QueryGroupBy deletions = (QueryGroupBy)groupby2.Clone();
      QueryGroupBy insertions = (QueryGroupBy)groupby2.Clone();

      //      groupby1.AggregateList;
      //      groupby1.GroupContext;
      //      groupby1.GroupList;
      //      groupby1.Having;
      //      groupby1.HavingContext;
      //      groupby1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryGeneratedType(QueryGeneratedType qgt1, QueryGeneratedType qgt2){
      Differences differences = new Differences(qgt1, qgt2);
      if (qgt1 == null || qgt2 == null){
        if (qgt1 != qgt2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryGeneratedType changes = (QueryGeneratedType)qgt2.Clone();
      QueryGeneratedType deletions = (QueryGeneratedType)qgt2.Clone();
      QueryGeneratedType insertions = (QueryGeneratedType)qgt2.Clone();

      //qgt1.Type;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryInsert(QueryInsert insert1, QueryInsert insert2){
      Differences differences = new Differences(insert1, insert2);
      if (insert1 == null || insert2 == null){
        if (insert1 != insert2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryInsert changes = (QueryInsert)insert2.Clone();
      QueryInsert deletions = (QueryInsert)insert2.Clone();
      QueryInsert insertions = (QueryInsert)insert2.Clone();

      //      insert1.HintList;
      //      insert1.InsertList;
      //      insert1.IsBracket;
      //      insert1.Location;
      //      insert1.Position;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryIntersection(QueryIntersection intersection1, QueryIntersection intersection2){
      Differences differences = new Differences(intersection1, intersection2);
      if (intersection1 == null || intersection2 == null){
        if (intersection1 != intersection2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryIntersection changes = (QueryIntersection)intersection2.Clone();
      QueryIntersection deletions = (QueryIntersection)intersection2.Clone();
      QueryIntersection insertions = (QueryIntersection)intersection2.Clone();

      //      intersection1.LeftSource;
      //      intersection1.RightSource;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryIterator(QueryIterator iterator1, QueryIterator iterator2){
      Differences differences = new Differences(iterator1, iterator2);
      if (iterator1 == null || iterator2 == null){
        if (iterator1 != iterator2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryIterator changes = (QueryIterator)iterator2.Clone();
      QueryIterator deletions = (QueryIterator)iterator2.Clone();
      QueryIterator insertions = (QueryIterator)iterator2.Clone();

      //      iterator1.ElementType;
      //      iterator1.Expression;
      //      iterator1.HintList;
      //      iterator1.Name;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryJoin(QueryJoin join1, QueryJoin join2){
      Differences differences = new Differences(join1, join2);
      if (join1 == null || join2 == null){
        if (join1 != join2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryJoin changes = (QueryJoin)join2.Clone();
      QueryJoin deletions = (QueryJoin)join2.Clone();
      QueryJoin insertions = (QueryJoin)join2.Clone();

      //      join1.JoinContext;
      //      join1.JoinExpression;
      //      join1.JoinType;
      //      join1.LeftOperand;
      //      join1.RightOperand;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryLimit(QueryLimit limit1, QueryLimit limit2){
      Differences differences = new Differences(limit1, limit2);
      if (limit1 == null || limit2 == null){
        if (limit1 != limit2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryLimit changes = (QueryLimit)limit2.Clone();
      QueryLimit deletions = (QueryLimit)limit2.Clone();
      QueryLimit insertions = (QueryLimit)limit2.Clone();

      //      limit1.Expression;
      //      limit1.IsPercent;
      //      limit1.IsWithTies;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryOrderBy(QueryOrderBy orderby1, QueryOrderBy orderby2){
      Differences differences = new Differences(orderby1, orderby2);
      if (orderby1 == null || orderby2 == null){
        if (orderby1 != orderby2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryOrderBy changes = (QueryOrderBy)orderby2.Clone();
      QueryOrderBy deletions = (QueryOrderBy)orderby2.Clone();
      QueryOrderBy insertions = (QueryOrderBy)orderby2.Clone();

      //      orderby1.Context;
      //      orderby1.OrderList;
      //      orderby1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryOrderItem(QueryOrderItem item1, QueryOrderItem item2){
      Differences differences = new Differences(item1, item2);
      if (item1 == null || item2 == null){
        if (item1 != item2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryOrderItem changes = (QueryOrderItem)item2.Clone();
      QueryOrderItem deletions = (QueryOrderItem)item2.Clone();
      QueryOrderItem insertions = (QueryOrderItem)item2.Clone();

      //      item1.Expression;
      //      item1.OrderType;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryPosition(QueryPosition position1, QueryPosition position2){
      Differences differences = new Differences(position1, position2);
      if (position1 == null || position2 == null){
        if (position1 != position2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryPosition changes = (QueryPosition)position2.Clone();
      QueryPosition deletions = (QueryPosition)position2.Clone();
      QueryPosition insertions = (QueryPosition)position2.Clone();

      //position1.Context;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryProject(QueryProject project1, QueryProject project2){
      Differences differences = new Differences(project1, project2);
      if (project1 == null || project2 == null){
        if (project1 != project2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryProject changes = (QueryProject)project2.Clone();
      QueryProject deletions = (QueryProject)project2.Clone();
      QueryProject insertions = (QueryProject)project2.Clone();

      //      project1.Context;
      //      project1.Members;
      //      project1.ProjectedType;
      //      project1.ProjectionList;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryRollback(QueryRollback qr1, QueryRollback qr2){
      Differences differences = new Differences(qr1, qr2);
      if (qr1 == null || qr2 == null){
        if (qr1 != qr2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
      }else
        differences.Changes = null;
      return differences;
    }
    public virtual Differences VisitQueryQuantifier(QueryQuantifier qq1, QueryQuantifier qq2){
      Differences differences = new Differences(qq1, qq2);
      if (qq1 == null || qq2 == null){
        if (qq1 != qq2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryQuantifier changes = (QueryQuantifier)qq2.Clone();
      QueryQuantifier deletions = (QueryQuantifier)qq2.Clone();
      QueryQuantifier insertions = (QueryQuantifier)qq2.Clone();

      //      qq1.Expression;
      //      qq1.Target;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryQuantifiedExpression(QueryQuantifiedExpression qqe1, QueryQuantifiedExpression qqe2){
      Differences differences = new Differences(qqe1, qqe2);
      if (qqe1 == null || qqe2 == null){
        if (qqe1 != qqe2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryQuantifiedExpression changes = (QueryQuantifiedExpression)qqe2.Clone();
      QueryQuantifiedExpression deletions = (QueryQuantifiedExpression)qqe2.Clone();
      QueryQuantifiedExpression insertions = (QueryQuantifiedExpression)qqe2.Clone();

      //      qqe1.Expression;
      //      qqe1.Left;
      //      qqe1.Right;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQuerySelect(QuerySelect select1, QuerySelect select2){
      Differences differences = new Differences(select1, select2);
      if (select1 == null || select2 == null){
        if (select1 != select2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QuerySelect changes = (QuerySelect)select2.Clone();
      QuerySelect deletions = (QuerySelect)select2.Clone();
      QuerySelect insertions = (QuerySelect)select2.Clone();

      //      select1.Access;
      //      select1.Direction;
      //      select1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQuerySingleton(QuerySingleton singleton1, QuerySingleton singleton2){
      Differences differences = new Differences(singleton1, singleton2);
      if (singleton1 == null || singleton2 == null){
        if (singleton1 != singleton2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QuerySingleton changes = (QuerySingleton)singleton2.Clone();
      QuerySingleton deletions = (QuerySingleton)singleton2.Clone();
      QuerySingleton insertions = (QuerySingleton)singleton2.Clone();

      //singleton1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryTransact(QueryTransact qt1, QueryTransact qt2){
      Differences differences = new Differences(qt1, qt2);
      if (qt1 == null || qt2 == null){
        if (qt1 != qt2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryTransact changes = (QueryTransact)qt2.Clone();
      QueryTransact deletions = (QueryTransact)qt2.Clone();
      QueryTransact insertions = (QueryTransact)qt2.Clone();

      //      qt1.Body;
      //      qt1.CommitBody;
      //      qt1.Isolation;
      //      qt1.RollbackBody;
      //      qt1.Transaction;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryTypeFilter(QueryTypeFilter filter1, QueryTypeFilter filter2){
      Differences differences = new Differences(filter1, filter2);
      if (filter1 == null || filter2 == null){
        if (filter1 != filter2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryTypeFilter changes = (QueryTypeFilter)filter2.Clone();
      QueryTypeFilter deletions = (QueryTypeFilter)filter2.Clone();
      QueryTypeFilter insertions = (QueryTypeFilter)filter2.Clone();

      //      filter1.Constraint;
      //      filter1.Source;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryUnion(QueryUnion union1, QueryUnion union2){
      Differences differences = new Differences(union1, union2);
      if (union1 == null || union2 == null){
        if (union1 != union2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryUnion changes = (QueryUnion)union2.Clone();
      QueryUnion deletions = (QueryUnion)union2.Clone();
      QueryUnion insertions = (QueryUnion)union2.Clone();

      //      union1.LeftSource;
      //      union1.RightSource;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
    public virtual Differences VisitQueryUpdate(QueryUpdate update1, QueryUpdate update2){
      Differences differences = new Differences(update1, update2);
      if (update1 == null || update2 == null){
        if (update1 != update2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryUpdate changes = (QueryUpdate)update2.Clone();
      QueryUpdate deletions = (QueryUpdate)update2.Clone();
      QueryUpdate insertions = (QueryUpdate)update2.Clone();

      //      update1.Context;
      //      update1.Source;
      //      update1.UpdateList;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }    
    public virtual Differences VisitQueryYielder(QueryYielder yielder1, QueryYielder yielder2){
      Differences differences = new Differences(yielder1, yielder2);
      if (yielder1 == null || yielder2 == null){
        if (yielder1 != yielder2) differences.NumberOfDifferences++; else differences.NumberOfSimilarities++;
        return differences;
      }
      QueryYielder changes = (QueryYielder)yielder2.Clone();
      QueryYielder deletions = (QueryYielder)yielder2.Clone();
      QueryYielder insertions = (QueryYielder)yielder2.Clone();

      //      yielder1.Body;
      //      yielder1.Source;
      //      yielder1.State;
      //      yielder1.Target;

      if (differences.NumberOfDifferences == 0){
        differences.Changes = null;
        differences.Deletions = null;
        differences.Insertions = null;
      }else{
        differences.Changes = changes;
        differences.Deletions = deletions;
        differences.Insertions = insertions;
      }
      return differences;
    }
#endif
  }
  public class Differences : UpdateSpecification{
    public int NumberOfDifferences;
    public int NumberOfSimilarities;
    public double Similarity{
      get{
        return this.NumberOfSimilarities / (double)(this.NumberOfDifferences+this.NumberOfSimilarities);
      }
    }
    public Differences(){
    }
    public Differences(Node original){
      this.Original = original;
    }
    public Differences(Node original, Node changes){
      this.Original = original;
      this.Changes = changes;
    }
  }
}
#endif