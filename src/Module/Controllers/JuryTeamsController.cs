using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize]
    [Route("[area]/{cid:c(7)}/jury/teams")]
    [AuditPoint(AuditlogType.Team)]
    public partial class JuryTeamsController : JuryControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var teams = await Context.ListTeamsAsync(t => t.Status != 3);
            var affs = await Context.FetchAffiliationsAsync();
            var cats = await Context.FetchCategoriesAsync();

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

            var scb = await Context.FetchScoreboardAsync();
            var bq = scb.Data.GetValueOrDefault(teamid);
            var cats = await Context.FetchCategoriesAsync();
            var affs = await Context.FetchAffiliationsAsync();
            var sols = await Context.FetchSolutionsAsync(teamid: teamid, all: all_submissions);
            var members = await Context.FetchTeamMemberAsync(team);

            return View(new JuryViewTeamModel
            {
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
                Problems = Problems,
                Affiliation = affs[team.AffiliationId],
                Category = cats[team.CategoryId],
                Solutions = sols,
                Members = members,
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                Status = team.Status,
                ScoreCache = bq?.ScoreCache,
                RankCache = bq?.RankCache,
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

            var affiliations = await Context.FetchAffiliationsAsync(false);
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
            await Context.UpdateTeamAsync(team, _ => new Team { Status = 1 });
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
            await Context.UpdateTeamAsync(team, _ => new Team { Status = 2 });
            await HttpContext.AuditAsync("rejected", $"{teamid}");

            return Message(
                title: "Team registration confirm",
                message: $"Team #{teamid} is now rejected.",
                type: BootstrapColor.success);
        }
    }
}
