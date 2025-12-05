# Token-based Chunk Overlap: Implementation TODO

Goal
- Implement integer OverlapPercentage (0–100, default 0) to produce overlapping chunks by tokens, with best-effort clamping to respect MaxChunkTokenCount. Expose in CLI and Service.

Core behavior spec
- Overlap computed from previous finalized chunk tokens: overlapTokens = floor(prev.TokenCount * OverlapPercentage / 100).
- Apply header to overlapped windows; header tokens count against MaxChunkTokenCount.
- Best-effort: reduce overlap dynamically if the overlapped window would exceed MaxChunkTokenCount.
- Preserve order and metadata; base splitting remains governed by separators and MinChunkPercentage.

Options
- Add OverlapPercentage to [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8)
  - Validation: int only; clamp to [0, 100]; default 0.

Model changes
- Extend chunk model with character offsets to support windowing:
  - Add StartOffset and EndOffset to [SemanticSlicer.Models.DocumentChunk](SemanticSlicer/Models/DocumentChunk.cs:1)

Library implementation
- Initialize root offsets:
  - In [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:70), set StartOffset = 0 and EndOffset = massagedContent.Length on the initial chunk.
- Propagate offsets on split:
  - In [SemanticSlicer.Slicer.SplitChunkBySeparatorMatch()](SemanticSlicer/Slicer.cs:360), compute child offsets based on separator behavior:
    - Prefix: first.EndOffset = matchIndex; second.StartOffset = matchIndex
    - Suffix: first.EndOffset = matchIndex + matchValue.Length; second.StartOffset = matchIndex + matchValue.Length
    - Remove: first.EndOffset = matchIndex; second.StartOffset = matchIndex + matchValue.Length
  - Set first.StartOffset = parent.StartOffset; second.EndOffset = parent.EndOffset.
- Post-processing overlap pass:
  - After base split returns finalized chunks, iterate in order in [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:70).
  - For chunk[i+1], compute requested overlapTokens from chunk[i].TokenCount and OverlapPercentage.
  - Derive overlapped content window:
    - Identify a backward char span using previous EndOffset; extend the start of chunk[i+1] backward.
    - Recompute effective tokens with header; if TokenCount > MaxChunkTokenCount, shrink overlap until it fits.
  - Replace chunk[i+1] content with overlapped content; retain Metadata; recalculate TokenCount.
- Indexing:
  - Reassign Index sequentially after the overlap pass [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:109).

Safeguards
- Base split constraints unchanged: [SemanticSlicer.Slicer.IsSplitBelowThreshold()](SemanticSlicer/Slicer.cs:333) only applies during recursion.
- Header-aware limits: header is prepended to overlapped content for token counting.
- If overlap cannot be applied without exceeding MaxChunkTokenCount, use base chunk with zero overlap.

Warnings (optional, non-breaking)
- Collect internal warnings when effective overlap < 50% of requested for many chunks (e.g., >= 3).
- Future integration:
  - CLI: single-line stderr advisory [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1)
  - Service: add optional warnings field in response contract [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12)

CLI exposure
- Parse --overlap <int> (0–100, default 0) and pass into SlicerOptions [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1)
- Update usage text to document overlap flag and typical values.

Service exposure
- Add OverlapPercentage int (0–100, default 0) to request model [SemanticSlicer.Service.Models.SliceRequest](SemanticSlicer.Service/Models/SliceRequest.cs:1)
- Validate input bounds in the endpoint and forward to SlicerOptions [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12)

Testing (SemanticSlicer.Tests)
- Baseline parity: OverlapPercentage=0 produces identical results to current behavior [SemanticSlicer.Tests.SlicerTests](SemanticSlicer.Tests/SlicerTests.cs:1)
- Typical overlaps: 20%, 30%, 50% across varied separators; assert TokenCount ≤ MaxChunkTokenCount, ordering stable.
- Header impact: large header reduces available overlap; verify dynamic clamping.
- HTML strip mode: ensure overlap operates correctly after [SemanticSlicer.Slicer.RemoveNonBodyContent()](SemanticSlicer/Slicer.cs:127) and [SemanticSlicer.Slicer.CollapseWhitespace()](SemanticSlicer/Slicer.cs:189)
- Warning behavior: simulate near-cap chunks to trigger clamping advisory.

Documentation
- README: add OverlapPercentage option and examples; note interaction with MaxChunkTokenCount and header [README.md](README.md:1)
- CLI usage: document --overlap with examples [README.md](README.md:116)
- Service: update request contract and response warning note [README.md](README.md:155)

Performance review
- Cache header token count; reuse known chunk token counts where possible.
- Validate that overlap pass adds minimal overhead relative to base splitting.

Acceptance criteria
- Overlap works for typical 20–50% settings without failures; clamping engages when necessary.
- Backward compatibility preserved with default OverlapPercentage=0.
- Tests and docs updated; CLI and Service expose validated integer overlap parameter.