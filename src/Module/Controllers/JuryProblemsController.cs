﻿using Ccs;
using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Polygon.Models;
using Polygon.Packaging;
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
    [Route("[area]/{cid}/jury/[controller]")]
    [AuditPoint(Entities.AuditlogType.Problem)]
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
            var prob = await Context.FindProblemAsync(pid);
            if (prob == null) return NotFound();
            ViewBag.Submissions = await Context.FetchSolutionsAsync(probid: pid, all: all);            
            return View(prob);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ContestProblem model)
        {
            if (null != await Context.FindProblemAsync(model.ShortName))
                ModelState.AddModelError("xys::duplicate", "Duplicate short name for problem.");

            var probDetect = await Context
                .GetRequiredService<IProblemsetStore>()
                .CheckAvailabilityAsync(Contest.Id, model.ProblemId, User.IsInRole("Administrator") ? default(int?) : int.Parse(User.GetUserId()));
            if (!probDetect.Available)
                ModelState.AddModelError("xys::prob", probDetect.Message);

            if (!ModelState.IsValid) return Window(model);
            model.Color = "#" + model.Color.TrimStart('#');
            model.ContestId = Contest.Id;

            await Context.CreateProblemAsync(
                () => new ContestProblem
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
        public async Task<IActionResult> Find(int cid, int pid)
        {
            var (_, msg) = await Context
                .GetRequiredService<IProblemsetStore>()
                .CheckAvailabilityAsync(cid, pid, User.IsInRole("Administrator") ? default(int?) : int.Parse(User.GetUserId()));
            return Content(msg);
        }


        [HttpGet("{pid}/[action]")]
        public async Task<IActionResult> Edit(int pid)
        {
            var prob = await Context.FindProblemAsync(pid);
            if (prob == null) return NotFound();
            return Window(prob);
        }


        [HttpPost("{pid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int pid, ContestProblem model)
        {
            var problems = await Context.FetchProblemsAsync();
            var origin = problems.FirstOrDefault(cp => cp.ProblemId == pid);
            if (origin == null) return NotFound();

            if (problems.Any(cp => cp.ShortName == model.ShortName && cp.ProblemId != pid))
                ModelState.AddModelError("xys::duplicate", "Duplicate short name for problem.");
            if (!ModelState.IsValid)
                return Window(model);

            model.Color = "#" + model.Color.TrimStart('#');
            await Context.UpdateProblemAsync(origin,
                () => new ContestProblem
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
        public async Task<IActionResult> Delete(int pid)
        {
            var prob = await Context.FindProblemAsync(pid);
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
            var prob = await Context.FindProblemAsync(pid);
            if (prob == null) return NotFound();

            await Context.DeleteProblemAsync(prob);
            await HttpContext.AuditAsync("detached", $"{pid}");

            StatusMessage = $"Contest problem {prob.ShortName} has been deleted.";
            return RedirectToAction("Home", "Jury");
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GenerateStatement()
        {
            var store = Context.GetRequiredService<IProblemsetStore>();
            var provider = Context.GetRequiredService<IStatementProvider>();
            var writer = Context.GetRequiredService<IStatementWriter>();
            var raw = await store.RawProblemsAsync(Contest.Id);
            var probs = await Context.FetchProblemsAsync();
            var stmts = new List<Statement>();
            foreach (var prob in raw)
            {
                var stmt = await provider.ReadAsync(prob);
                stmts.Add(new Statement(prob,
                    stmt.Description, stmt.Input, stmt.Output, stmt.Hint, stmt.Interaction,
                    probs.Find(prob.Id).ShortName, stmt.Samples));
            }

            var startTime = Contest.StartTime ?? DateTimeOffset.Now;
            var startDate = startTime.ToString("dddd, MMMM d, yyyy", CultureInfo.GetCultureInfo(1033));

            var memstream = new MemoryStream();
            using (var zip = new ZipArchive(memstream, ZipArchiveMode.Create, true))
            {
                using (var olympStream = typeof(ContestModule<,,>).Assembly.GetManifestResourceStream("SatelliteSite.ContestModule.olymp.sty"))
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
