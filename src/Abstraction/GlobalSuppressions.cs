// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Source code should be base64 to prevent encoding problems.", Scope = "member", Target = "~P:Ccs.Entities.Printing.SourceCode")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Events.Team.Members")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Events.Team.GroupIds")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Events.Language.Extensions")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Events.Award.TeamIds")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "API specification.", Scope = "member", Target = "~P:Ccs.Events.Submission.Files")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Events.Submission.FileMeta")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Events.Scoreboard.Row")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Events.Scoreboard.Problem")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "API specification.", Scope = "type", Target = "~T:Ccs.Events.Scoreboard.Score")]
