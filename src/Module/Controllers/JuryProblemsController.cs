using Ccs.Entities;
using Microsoft.AspNetCore.Mvc;
using Polygon.Packaging;
using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/jury/[controller]")]
    [AuditPoint(AuditlogType.Problem)]
    public class ProblemsController : JuryControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult Add()
        {
            return Window(new ContestProblem
            {
                ContestId = Contest.Id,
                AllowSubmit = true
            });
        }


        [HttpGet("{pid}")]
        public async Task<IActionResult> Detail(int pid, bool all = false)
        {
            var prob = Problems.Find(pid);
            if (prob == null) return NotFound();
            ViewBag.Submissions = await Context.FetchSolutionsAsync(probid: pid, all: all);            
            return View(prob);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ContestProblem model)
        {
            if (null != Problems.Find(model.ShortName))
                ModelState.AddModelError("xys::duplicate", "Duplicate short name for problem.");

            var probDetect = await Context.CheckProblemAvailabilityAsync(model.ProblemId, User);
            if (!probDetect.Success)
                ModelState.AddModelError("xys::prob", probDetect.Message);

            if (!ModelState.IsValid) return Window(model);
            model.Color = "#" + model.Color.TrimStart('#');
            model.ContestId = Contest.Id;

            await Context.CreateProblemAsync(
                new ContestProblem
                {
                    AllowSubmit = model.AllowSubmit,
                    Color = model.Color,
                    ContestId = model.ContestId,
                    ProblemId = model.ProblemId,
                    Score = model.Score,
                    ShortName = model.ShortName,
                });

            await HttpContext.AuditAsync("attached", $"{model.ProblemId}");
            StatusMessage = $"Problem {model.ShortName} saved.";
            return RedirectToAction("Home", "Jury");
        }


        [HttpGet("[action]/{pid}")]
        public async Task<IActionResult> Find(int pid)
        {
            var result = await Context.CheckProblemAvailabilityAsync(pid, User);
            return Content(result.Message);
        }


        [HttpGet("{pid}/[action]")]
        public IActionResult Edit(int pid)
        {
            var prob = Problems.Find(pid);
            if (prob == null) return NotFound();
            return Window(prob);
        }


        [HttpPost("{pid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int pid, ContestProblem model)
        {
            var origin = Problems.Find(pid);
            if (origin == null) return NotFound();

            if (Problems.Any(cp => cp.ShortName == model.ShortName && cp.ProblemId != pid))
                ModelState.AddModelError("xys::duplicate", "Duplicate short name for problem.");
            if (!ModelState.IsValid)
                return Window(model);

            model.Color = "#" + model.Color.TrimStart('#');
            await Context.UpdateProblemAsync(origin,
                _ => new ContestProblem
                {
                    Color = model.Color,
                    AllowSubmit = model.AllowSubmit,
                    ShortName = model.ShortName,
                    Score = model.Score,
                });

            await HttpContext.AuditAsync("updated", $"{pid}");
            return GoBackHome($"Problem {model.ShortName} saved.");
        }


        [HttpGet("{pid}/[action]")]
        public IActionResult Delete(int pid)
        {
            var prob = Problems.Find(pid);
            if (prob == null) return NotFound();

            return AskPost(
                title: "Delete contest problem",
                message: $"Are you sure to delete problem {prob.ShortName}?",
                area: "Contest", controller: "Problems", action: "Delete",
                routeValues: new { cid = Contest.Id, pid },
                type: BootstrapColor.danger);
        }


        [HttpPost("{pid}/[action]")]
        public async Task<IActionResult> Delete(int pid, bool _ = true)
        {
            var prob = Problems.Find(pid);
            if (prob == null) return NotFound();

            await Context.DeleteProblemAsync(prob);
            await HttpContext.AuditAsync("detached", $"{pid}");

            StatusMessage = $"Contest problem {prob.ShortName} has been deleted.";
            return RedirectToAction("Home", "Jury");
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GenerateStatement(
            [FromServices] IStatementWriter writer)
        {
            var stmts = await Context.FetchRawStatementsAsync();
            var startTime = Contest.StartTime ?? DateTimeOffset.Now;
            var startDate = startTime.ToString("dddd, MMMM d, yyyy", CultureInfo.GetCultureInfo(1033));

            var memstream = new MemoryStream();
            using (var zip = new ZipArchive(memstream, ZipArchiveMode.Create, true))
            {
                using (var olympStream = typeof(ContestModule<>).Assembly.GetManifestResourceStream("SatelliteSite.ContestModule.olymp.sty"))
                    await zip.CreateEntryFromStream(olympStream, "olymp.sty");

                var documentBuilder = new System.Text.StringBuilder()
                    .Append(@"
\documentclass [11pt, a4paper, oneside] {article}

\usepackage {import}
\usepackage [T2A] {fontenc}
\usepackage [utf8] {inputenc}
\usepackage [english, russian] {babel}
\usepackage {amsmath}
\usepackage {amssymb}
\usepackage {olymp}
\usepackage {comment}
\usepackage {epigraph}
\usepackage {expdlist}
\usepackage {listings}
\usepackage {graphicx}
\usepackage {ulem}
\usepackage {xeCJK}

\lstset{basicstyle=\ttfamily}


\begin {document}

\contest
{").Append(Contest.Name).Append(@"}%
{ACM.XYLAB.FUN}%
{").Append(startDate).Append(@"}%

\binoppenalty=10000
\relpenalty=10000

\renewcommand{\t}{\texttt}

");

                foreach (var item in stmts)
                {
                    var folderPrefix = $"{item.ShortName}/";
                    writer.BuildLatex(zip, item, folderPrefix);

                    documentBuilder
                        .AppendLine("\\graphicspath{{./" + item.ShortName + "/}}")
                        .AppendLine("\\import{./" + item.ShortName + "/}{./problem.tex}")
                        .AppendLine();
                }

                documentBuilder.Append("\\end{document}\n\n");
                zip.CreateEntryFromString(documentBuilder.ToString(), "contest.tex");
            }

            memstream.Position = 0;
            return File(memstream, "application/zip", $"contest-{Contest.Id}-statements.zip");
        }
    }
}
