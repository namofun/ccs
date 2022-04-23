using System.Text.Json.Serialization;
using Xylab.Polygon.Entities;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Judgement types are the possible responses from the system when judging a submission.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Judgement_Types">More detail</a>
    /// </summary>
    public class JudgementType : AbstractEvent
    {
        /// <summary>
        /// Identifier of the judgement type
        /// </summary>
        /// <remarks>A 2-3 letter capitalized shorthand, see table below.</remarks>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Name of the judgement
        /// </summary>
        /// <remarks>Might not match table below, e.g. if localised.</remarks>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Whether this judgement causes penalty time
        /// </summary>
        /// <remarks>Must be present if and only if <c>contest:penalty_time</c> is present.</remarks>
        [JsonPropertyName("penalty")]
        public bool Penalty { get; }

        /// <summary>
        /// Whether this judgement is considered correct
        /// </summary>
        [JsonPropertyName("solved")]
        public bool Solved { get; }

        /// <inheritdoc />
        protected override string EndpointType => "judgement-types";

        /// <summary>
        /// Construct a <see cref="JudgementType"/>.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <param name="name">The entity name.</param>
        /// <param name="penalty">The <c>penalty</c>.</param>
        /// <param name="solved">The <c>solved</c>.</param>
        private JudgementType(string id, string name, bool penalty, bool solved)
        {
            Id = id;
            Name = name;
            Penalty = penalty;
            Solved = solved;
        }

        /// <summary>
        /// The default list.
        /// </summary>
        public static readonly JudgementType[] Defaults = new[]
        {
            new JudgementType("CE", "compiler error", false, false),
            new JudgementType("MLE", "memory limit", true, false),
            new JudgementType("OLE", "output limit", true, false),
            new JudgementType("RTE", "run error", true, false),
            new JudgementType("TLE", "timelimit", true, false),
            new JudgementType("WA", "wrong answer", true, false),
            new JudgementType("PE", "presentation error", true, false),
            new JudgementType("AC", "correct", false, true),
            new JudgementType("JE", "judge error", false, false),
            // new JudgementType("RE", "rejected", true, false),
        };

        /// <summary>
        /// Convert the legacy judgement type names to <see cref="Verdict"/>.
        /// </summary>
        /// <param name="verdict">The verdict string.</param>
        /// <returns>The verdict value.</returns>
        public static Verdict For(string verdict)
        {
            return verdict switch
            {
                "CE" => Verdict.CompileError,
                "MLE" => Verdict.MemoryLimitExceeded,
                "OLE" => Verdict.OutputLimitExceeded,
                "RTE" => Verdict.RuntimeError,
                "TLE" => Verdict.TimeLimitExceeded,
                "WA" => Verdict.WrongAnswer,
#pragma warning disable CS0612
                "PE" => Verdict.PresentationError,
#pragma warning restore CS0612
                "AC" => Verdict.Accepted,
                "JE" => Verdict.UndefinedError,
                _ => Verdict.Unknown,
            };
        }

        /// <summary>
        /// Convert the <see cref="Verdict"/> to legacy judgement type names.
        /// </summary>
        /// <param name="verdict">The verdict value.</param>
        /// <returns>The verdict name.</returns>
        public static string? For(Verdict verdict)
        {
            return verdict switch
            {
                Verdict.TimeLimitExceeded => Defaults[4].Id,
                Verdict.MemoryLimitExceeded => Defaults[1].Id,
                Verdict.RuntimeError => Defaults[3].Id,
                Verdict.OutputLimitExceeded => Defaults[2].Id,
                Verdict.WrongAnswer => Defaults[5].Id,
                Verdict.CompileError => Defaults[0].Id,
#pragma warning disable CS0612
                Verdict.PresentationError => Defaults[6].Id,
#pragma warning restore CS0612
                Verdict.Accepted => Defaults[7].Id,
                Verdict.Pending => null,
                Verdict.Running => null,
                Verdict.Unknown => Defaults[8].Id,
                Verdict.UndefinedError => Defaults[8].Id,
                _ => Defaults[8].Id,
            };
        }
    }
}
