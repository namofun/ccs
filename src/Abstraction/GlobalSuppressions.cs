// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Source code should be base64 to prevent encoding problems.", Scope = "member", Target = "~P:Ccs.Entities.Printing.SourceCode")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Specifications.Team.Members")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Specifications.Team.GroupIds")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Specifications.Language.Extensions")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Specifications.Award.TeamIds")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Specifications.Submission.Files")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Specifications.Submission.FileMeta")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Specifications.Scoreboard.Row")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Specifications.Scoreboard.Problem")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Specifications.Scoreboard.Score")]
[assembly: SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "No value-type-like requirements.", Scope = "type", Target = "~T:Ccs.Models.ContestListModel")]
