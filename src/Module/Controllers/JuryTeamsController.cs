using Ccs.Entities;
using Ccs.Registration;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/teams")]
    [AuditPoint(AuditlogType.Team)]
    public class JuryTeamsController : JuryControllerBase<ITeamContext>
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var teams = await Context.ListTeamsAsync(t => t.Status != 3);
            var affs = await Context.ListAffiliationsAsync();
            var cats = await Context.ListCategoriesAsync();

            return View(teams.Select(t => new JuryListTeamModel
            {
                Status = t.Status,
                TeamId = t.TeamId,
                TeamName = t.TeamName,
                RegisterTime = t.RegisterTime,
                Category = cats[t.CategoryId].Name,
                Affiliation = affs[t.AffiliationId].Abbreviation,
                AffiliationName = affs[t.AffiliationId].Name
            }));
        }


        [HttpGet("{teamid}")]
        public async Task<IActionResult> Detail(int teamid, bool all_submissions = false)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            var scb = await Context.GetScoreboardAsync();
            var bq = scb.Data.GetValueOrDefault(teamid);
            var cats = await Context.ListCategoriesAsync();
            var affs = await Context.ListAffiliationsAsync();
            var sols = await Context.ListSolutionsAsync(teamid: teamid, all: all_submissions);
            var members = await Context.GetTeamMember2Async(team);

            return View(new JuryViewTeamModel
            {
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
                Kind = Contest.Kind,
                // For problemsets, don't show problems.
                Problems = Contest.Kind == Ccs.CcsDefaults.KindProblemset ? null : await Context.ListProblemsAsync(),
                Affiliation = affs[team.AffiliationId],
                Category = cats[team.CategoryId],
                Solutions = sols,
                Members = members,
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                Status = team.Status,
                ScoreCache = bq?.ScoreCache ?? Enumerable.Empty<ScoreCache>(),
                RankCache = bq?.RankCache ?? RankCache.Empty,
            });
        }


        [HttpGet("[action]")]
        public IActionResult Add()
        {
            return Window(new JuryAddTeamModel());
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(JuryAddTeamModel model)
        {
            var users = new HashSet<IUser>();
            if (model.UserName != null)
            {
                var userNames = model.UserName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var userName in userNames)
                {
                    var user = await UserManager.FindByNameAsync(userName.Trim());
                    if (user == null)
                        ModelState.AddModelError("xys::no_user", $"No such user {userName.Trim()}.");
                    else if ((await Context.FindMemberByUserAsync(user.Id)) != null)
                        ModelState.AddModelError("xys::duplicate_user", "Duplicate user.");
                    else
                        users.Add(user);
                }
            }

            var affiliations = await Context.ListAffiliationsAsync(false);
            if (!affiliations.TryGetValue(model.AffiliationId, out _))
                ModelState.AddModelError("xys::no_aff", "No such affiliation.");

            if (!ModelState.IsValid) return Window(model);

            var team = await Context.CreateTeamAsync(
                users: users.Count > 0 ? users : null,
                team: new Team
                {
                    AffiliationId = model.AffiliationId,
                    Status = 1,
                    CategoryId = model.CategoryId,
                    ContestId = Contest.Id,
                    TeamName = model.TeamName,
                });

            await HttpContext.AuditAsync("added", $"{team.TeamId}");
            return Message(
                title: "Add team",
                message: $"Team {model.TeamName} (t{team.TeamId}) added.",
                type: BootstrapColor.success);
        }


        [HttpGet("{teamid}/[action]")]
        public async Task<IActionResult> Delete(int teamid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            return AskPost(
                title: $"Delete team t{team.TeamId}",
                message: $"You are about to delete {team.TeamName} (t{team.TeamId}). Are you sure?",
                area: "Contest", controller: "JuryTeams", action: "DeleteConfirmed", routeValues: new { cid = Contest.Id, teamid },
                type: BootstrapColor.danger);
        }


        [HttpPost("{teamid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int teamid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team is null || team.Status == 3) return NotFound();

            var users = await Context.DeleteTeamAsync(team);

            await HttpContext.AuditAsync(
                "deleted", $"{team.TeamId}",
                users.Count == 0 ? null : ("with u" + string.Join(", u", users.Select(it => it.UserId))));

            StatusMessage = $"Team t{teamid} deleted.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{teamid}/[action]")]
        public async Task<IActionResult> Edit(int teamid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            return Window(new JuryEditTeamModel
            {
                AffiliationId = team.AffiliationId,
                CategoryId = team.CategoryId,
                TeamName = team.TeamName,
                TeamId = teamid,
            });
        }


        [HttpPost("{teamid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int teamid, JuryEditTeamModel model)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            await Context.UpdateTeamAsync(team,
                _ => new Team
                {
                    TeamName = model.TeamName,
                    AffiliationId = model.AffiliationId,
                    CategoryId = model.CategoryId
                });

            await HttpContext.AuditAsync("updated", $"{teamid}");

            return Message(
                title: "Edit team",
                message: $"Team {team.TeamName} (t{teamid}) updated.",
                type: BootstrapColor.success);
        }


        [HttpGet("{teamid}/[action]")]
        public async Task<IActionResult> Accept(int teamid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();
            await Context.UpdateTeamAsync(team, 1);
            await HttpContext.AuditAsync("accepted", $"{teamid}");

            return Message(
                title: "Team registration confirm",
                message: $"Team #{teamid} is now accepted.",
                type: BootstrapColor.success);
        }


        [HttpGet("{teamid}/[action]")]
        public async Task<IActionResult> Reject(int teamid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();
            await Context.UpdateTeamAsync(team, 2);
            await HttpContext.AuditAsync("rejected", $"{teamid}");

            return Message(
                title: "Team registration confirm",
                message: $"Team #{teamid} is now rejected.",
                type: BootstrapColor.success);
        }


        [HttpGet("[action]/{provider}")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public async Task<IActionResult> Import([RPBinder] IRegisterProvider provider)
        {
            if (provider == null || !provider.JuryOrContestant) return NotFound();
            var context = CreateRegisterProviderContext();
            if (!await provider.IsAvailableAsync(context)) return BadRequest();
            ViewBag.Provider = provider;
            ViewBag.Context = context;
            var model = await provider.CreateInputModelAsync(context);
            return Window(model);
        }


        [HttpPost("[action]/{provider}")]
        [Authorize(Policy = "ContestIsAdministrator")]
        [ActionName(nameof(Import))]
        public async Task<IActionResult> ImportResult([RPBinder] IRegisterProvider provider)
        {
            if (provider == null || !provider.JuryOrContestant) return NotFound();
            var context = CreateRegisterProviderContext();
            if (!await provider.IsAvailableAsync(context)) return BadRequest();
            ViewBag.Provider = provider;
            ViewBag.Context = context;
            var model = await provider.CreateInputModelAsync(context);
            await provider.ReadAsync(model, this);
            await provider.ValidateAsync(context, model, ModelState);

            if (ModelState.IsValid)
            {
                var output = await provider.ExecuteAsync(context, model);
                await HttpContext.AuditAsync("import", null, "via " + provider.Name);
                return Window("ImportResult", output);
            }
            else
            {
                // something got wrong, re-display the form.
                return Window(model);
            }
        }


        [HttpGet("[action]")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public IActionResult LockoutTemporary()
        {
            return AskPost(
                title: "Lockout temporary users",
                message: "Are you sure to lockout temporary users? " +
                    "You should only proceed this after the whole contest is over.",
                area: "Contest", controller: "JuryTeams", action: nameof(LockoutTemporary), new { cid = Contest.Id },
                type: BootstrapColor.warning);
        }


        [HttpPost("[action]")]
        [Authorize(Policy = "ContestIsAdministrator")]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(LockoutTemporary))]
        public async Task<IActionResult> LockoutTemporaryConfirmation()
        {
            await Context.LockOutTemporaryAsync(UserManager);
            await HttpContext.AuditAsync("lockout", "temporary accounts");
            StatusMessage = "Lockout finished.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{teamid}/[action]/{userid}")]
        public async Task<IActionResult> Detach(int teamid, int userid)
        {
            var member = await Context.FindMemberByUserAsync(userid);
            if (member == null || member.TeamId != teamid) return NotFound();
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();
            var user = await UserManager.FindByIdAsync(userid);
            if (user == null) return NotFound();

            return AskPost(
                title: "Detach user",
                message: $"Are you sure to " +
                    $"detach user {user.UserName} (u{userid}) " +
                    $"from team {team.TeamName} (t{teamid})?",
                type: BootstrapColor.danger);
        }


        [HttpPost("{teamid}/[action]/{userid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detach(int teamid, int userid, bool _ = true)
        {
            var member = await Context.FindMemberByUserAsync(userid);
            if (member == null || member.TeamId != teamid) return NotFound();
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            await Context.DetachMemberAsync(member);
            await HttpContext.AuditAsync("detach", $"{teamid}", $"user {userid}");
            StatusMessage = $"User {userid} detached successfully.";
            return RedirectToAction(nameof(Detail));
        }


        [HttpGet("{teamid}/[action]/{userid}")]
        public async Task<IActionResult> ResetRestrictions(int teamid, int userid)
        {
            var member = await Context.FindMemberByUserAsync(userid);
            if (member == null || member.TeamId != teamid) return NotFound();
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();
            var user = await UserManager.FindByIdAsync(userid);
            if (user == null) return NotFound();

            return AskPost(
                title: "Reset access restrictions",
                message: $"Are you sure to reset the access restrictions " +
                    $"for user {user.UserName} (u{userid}) " +
                    $"from team {team.TeamName} (t{teamid})?",
                type: BootstrapColor.danger);
        }


        [HttpPost("{teamid}/[action]/{userid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetRestrictions(int teamid, int userid, bool _ = true)
        {
            var member = await Context.FindMemberByUserAsync(userid);
            if (member == null || member.TeamId != teamid) return NotFound();
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            await Context.UpdateMemberAsync(member, _ => new Member { LastLoginIp = null });
            await HttpContext.AuditAsync("reset restriction", $"{teamid}", $"user {userid}");
            StatusMessage = $"Restrictions for user {userid} reset successfully.";
            return RedirectToAction(nameof(Detail));
        }


        [HttpGet("[action]/{userName?}")]
        public async Task<IActionResult> TestUser(string userName)
        {
            if (userName != null)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user == null) return Content("No such user.", "text/html");
                var member = await Context.FindMemberByUserAsync(user.Id);
                if (member != null) return Content("This user is attached to another team.", "text/html");
                return Content("", "text/html");
            }
            else
            {
                return Content("Please enter the username.", "text/html");
            }
        }


        [HttpGet("{teamid}/[action]")]
        public async Task<IActionResult> Attach(int teamid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();
            return Window(new JuryAssignModel());
        }


        [HttpPost("{teamid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attach(int teamid, JuryAssignModel model)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            if (team == null) return NotFound();

            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                StatusMessage = $"Error user {model.UserName} does not exist!";
                return RedirectToAction(nameof(Detail));
            }

            var member = await Context.FindMemberByUserAsync(user.Id);
            if (member != null)
            {
                StatusMessage = $"Error user {model.UserName} is attached to team {member.TeamId}.";
                return RedirectToAction(nameof(Detail));
            }

            await Context.AttachMemberAsync(team, user, false);
            StatusMessage = $"User {user.Id} attached successfully.";
            return RedirectToAction(nameof(Detail));
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Monitoring(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories)
        {
            if (categories?.Length == 0) categories = null;
            if (affiliations?.Length == 0) affiliations = null;
            var teamsCond = Expr.Of<Team>(t => t.Status == 1)
                .CombineIf(categories != null, t => categories.Contains(t.CategoryId))
                .CombineIf(affiliations != null, t => affiliations.Contains(t.AffiliationId));

            var cats = await Context.ListCategoriesAsync();
            var affs = await Context.ListAffiliationsAsync();
            var teams = await Context.GetMonitorAsync(teamsCond);

            ViewBag.Categories = cats;
            ViewBag.Affiliations = affs;
            ViewBag.FilteredCategories = categories;
            ViewBag.FilteredAffiliations = affiliations;
            return View(teams);
        }
    }
}
