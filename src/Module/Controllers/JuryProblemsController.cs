using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Packaging;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/problems")]
    [AuditPoint(AuditlogType.Problem)]
    public class JuryProblemsController : JuryControllerBase<IProblemContext>
    {
        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            const int perPage = 50;
            if (page < 1) return BadRequest();
            var probs = await Context.ListProblemsAsync(page, perPage, true);
            return View(probs);
        }


        [HttpGet("[action]")]
        public IActionResult Add()
        {
            return Window(new ContestProblem
            {
                ContestId = Contest.Id,
                AllowSubmit = true
            });
        }


        [HttpGet("{probid}")]
        public async Task<IActionResult> Detail(int probid, bool all = false)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();
            var sols = await Context.ListSolutionsAsync(probid: probid, all: all);
            await Context.ApplyTeamNamesAsync(sols);
            return View(new JuryViewProblemModel(sols, prob));
        }


        [HttpGet("{probid}/[action]")]
        public async Task<IActionResult> Description(int probid)
        {
            var prob = await Context.FindProblemAsync(probid, true);
            return View(prob);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Choose()
        {
            if (Contest.Kind == Ccs.CcsDefaults.KindProblemset) return StatusCode(503);

            var problems = await Context.ListProblemsAsync(true);
            var recent = await Context.ListPolygonAsync(User);
            foreach (var p in problems) recent.TryAdd(p.ProblemId, p.Title);
            ViewBag.RecentProblems = recent;

            return View(new ChooseProblemModel
            {
                Problems = problems.ToDictionary(e => e.Rank, e => (ContestProblem)e),
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Choose(ChooseProblemModel model)
        {
            if (Contest.Kind == Ccs.CcsDefaults.KindProblemset) return StatusCode(503);

            var problems = await Context.ListProblemsAsync(true);
            var recent = await Context.ListPolygonAsync(User);
            foreach (var p in problems) recent.TryAdd(p.ProblemId, p.Title);
            ViewBag.RecentProblems = recent;
            model.Problems ??= new Dictionary<int, ContestProblem>();

            var newCp = new Dictionary<int, ContestProblem>();
            var newShortName = new HashSet<string>();
            foreach (var prob in model.Problems.Values)
            {
                if (newCp.ContainsKey(prob.ProblemId))
                {
                    ModelState.AddModelError("duplicate", "Duplicate problem entry.");
                    continue;
                }

                if (!recent.ContainsKey(prob.ProblemId))
                {
                    ModelState.AddModelError("no-perm", $"Error problem p{prob.ProblemId}: No permission or no such problem.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(prob.ShortName))
                {
                    ModelState.AddModelError("dupkey", $"Problem short name is invalid.");
                    continue;
                }

                if (!newShortName.Add(prob.ShortName = prob.ShortName.Trim()))
                {
                    ModelState.AddModelError("dupkey", $"Duplicate problem short name {prob.ShortName}.");
                    continue;
                }

                prob.ContestId = Contest.Id;
                prob.Color = "#" + (prob.Color ?? "#ffffff").TrimStart('#');
                newCp.Add(prob.ProblemId, prob);
            }

            if (!ModelState.IsValid) return View(model);

            var oldCp = new Dictionary<int, ProblemModel>();
            var tmpGuid = new HashSet<string>();
            foreach (var oldProb in problems)
            {
                oldCp.Add(oldProb.ProblemId, oldProb);
                if (!newCp.TryGetValue(oldProb.ProblemId, out var newProb))
                {
                    await Context.DeleteProblemAsync(oldProb);
                    await HttpContext.AuditAsync("detached", $"{oldProb.ProblemId}");
                }
                else if (newProb.ShortName != oldProb.ShortName)
                {
                    string tempGuid;
                    do tempGuid = Guid.NewGuid().ToString()[0..10];
                    while (!tmpGuid.Add(tempGuid));

                    await Context.UpdateProblemAsync(oldProb,
                        cp => new ContestProblem { ShortName = tempGuid });
                    oldProb.ShortName = tempGuid;
                }
            }

            foreach (var newProb in newCp.Values)
            {
                if (!oldCp.TryGetValue(newProb.ProblemId, out var oldProb))
                {
                    await Context.CreateProblemAsync(newProb);
                    await HttpContext.AuditAsync("attached", $"{newProb.ProblemId}");
                }
                else
                {
                    if (oldProb.AllowSubmit == newProb.AllowSubmit
                        && oldProb.Color == newProb.Color
                        && oldProb.Score == newProb.Score
                        && oldProb.ShortName == newProb.ShortName)
                        continue;

                    await Context.UpdateProblemAsync(oldProb,
                        cp => new ContestProblem
                        {
                            AllowSubmit = newProb.AllowSubmit,
                            Color = newProb.Color,
                            Score = newProb.Score,
                            ShortName = newProb.ShortName,
                        });

                    await HttpContext.AuditAsync("updated", $"{newProb.ProblemId}");
                }
            }

            StatusMessage = "Problem batch edit finished.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Descriptions()
        {
            if (Contest.Kind == Ccs.CcsDefaults.KindProblemset) return StatusCode(503);

            var problems = await Context.ListProblemsAsync();
            var list = new List<ProblemModel>();
            foreach (var item in problems)
            {
                list.Add(await Context.FindProblemAsync(item.ProblemId, true));
            }

            return View(list);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ContestProblem model)
        {
            if (null != await Context.FindProblemAsync(model.ShortName))
                ModelState.AddModelError("xys::duplicate", "Duplicate short name for problem.");

            var probDetect = await Context.CheckProblemAsync(model.ProblemId, User);
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
            return RedirectToAction(nameof(List));
        }


        [HttpGet("[action]/{probid}")]
        public async Task<IActionResult> Find(int probid)
        {
            var result = await Context.CheckProblemAsync(probid, User);
            return Content(result.Message);
        }


        [HttpGet("{probid}/[action]")]
        public async Task<IActionResult> Edit(int probid)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();
            return Window(prob);
        }


        [HttpPost("{probid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int probid, ContestProblem model)
        {
            var origin = await Context.FindProblemAsync(probid);
            if (origin == null) return NotFound();

            var sameName = await Context.FindProblemAsync(model.ShortName);
            if (sameName != null && sameName.ProblemId != probid)
            {
                ModelState.AddModelError("xys::duplicate", "Duplicate short name for problem.");
            }

            if (!ModelState.IsValid) return Window(model);

            model.Color = "#" + model.Color.TrimStart('#');
            await Context.UpdateProblemAsync(origin,
                _ => new ContestProblem
                {
                    Color = model.Color,
                    AllowSubmit = model.AllowSubmit,
                    ShortName = model.ShortName,
                    Score = model.Score,
                });

            await HttpContext.AuditAsync("updated", $"{probid}");
            StatusMessage = $"Problem {model.ShortName} saved.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{probid}/[action]")]
        public async Task<IActionResult> Delete(int probid)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();

            return AskPost(
                title: "Delete contest problem",
                message: $"Are you sure to delete problem {prob.ShortName}?",
                area: "Contest", controller: "JuryProblems", action: "Delete",
                routeValues: new { cid = Contest.Id, probid },
                type: BootstrapColor.danger);
        }


        [HttpPost("{probid}/[action]")]
        public async Task<IActionResult> Delete(int probid, bool _ = true)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();

            await Context.DeleteProblemAsync(prob);
            await HttpContext.AuditAsync("detached", $"{probid}");

            StatusMessage = $"Contest problem {prob.ShortName} has been deleted.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GenerateStatement(
            [FromServices] IStatementWriter writer)
        {
            if (Contest.Kind == Ccs.CcsDefaults.KindProblemset) return StatusCode(503);

            var stmts = await Context.GetStatementsAsync();
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
