using TheSingularityWorkshop.Economy;
using Xunit;

namespace TheSingularityWorkshop.Economy.Tests;

public sealed class CompetitionTests
{
    [Fact]
    public void TeamRosterChangesReturnNewSnapshotsAndPreservePriorTeam()
    {
        var teamId = CompetitorId.New();
        var member = new TeamMember(TeamMembershipId.New(), CompetitorId.New(), "captain", DateTimeOffset.UtcNow);
        var original = new Team(teamId, "Dream Team");
        var expanded = original.AddMember(member);

        Assert.Empty(original.Members);
        Assert.Single(expanded.Members);
        Assert.Equal(teamId, expanded.Id);
        Assert.Equal(CompetitorKind.Team, expanded.AsCompetitor().Kind);

        var reduced = expanded.RemoveMember(member.MembershipId);
        Assert.Single(expanded.Members);
        Assert.Empty(reduced.Members);
    }

    [Fact]
    public void TeamRejectsDuplicateMembersAndMembershipIdentities()
    {
        var memberId = CompetitorId.New();
        var membershipId = TeamMembershipId.New();
        var first = new TeamMember(membershipId, memberId, "forward", DateTimeOffset.UtcNow);
        var duplicateMember = new TeamMember(TeamMembershipId.New(), memberId, "captain", DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() => new Team(CompetitorId.New(), "Invalid", new[] { first, duplicateMember }));
        Assert.Throws<InvalidOperationException>(() => new Team(CompetitorId.New(), "Team", new[] { first }).AddMember(duplicateMember));
        Assert.Throws<ArgumentException>(() => new Team(CompetitorId.New(), "Invalid", new[]
        {
            first,
            new TeamMember(membershipId, CompetitorId.New(), "forward", DateTimeOffset.UtcNow)
        }));
    }

    [Fact]
    public void CompetitionRegistrationAndLifecycleAreImmutableAndValidated()
    {
        var original = new Competition(CompetitionId.New(), "Workshop Cup", CompetitionFormat.SingleElimination, maximumEntrants: 2);
        var open = original.OpenRegistration();
        var first = new Competitor(CompetitorId.New(), "Alpha", CompetitorKind.Team);
        var second = new Competitor(CompetitorId.New(), "Beta", CompetitorKind.Individual);
        var withFirst = open.Register(first);
        var withBoth = withFirst.Register(second);
        var closed = withBoth.CloseRegistration();
        var started = closed.Start();
        var completed = started.Complete();

        Assert.Equal(CompetitionStatus.Draft, original.Status);
        Assert.Empty(original.Entrants);
        Assert.Single(withFirst.Entrants);
        Assert.Equal(CompetitionStatus.RegistrationOpen, open.Status);
        Assert.Equal(CompetitionStatus.Completed, completed.Status);
        Assert.Equal(2, completed.Entrants.Count);
    }

    [Fact]
    public void CompetitionRejectsDuplicateEntrantsCapacityOverflowAndEarlyStart()
    {
        var competitor = new Competitor(CompetitorId.New(), "Alpha", CompetitorKind.Individual);
        var open = new Competition(CompetitionId.New(), "League", CompetitionFormat.League, maximumEntrants: 1)
            .OpenRegistration();
        var registered = open.Register(competitor);

        Assert.Throws<InvalidOperationException>(() => registered.Register(competitor));
        Assert.Throws<InvalidOperationException>(() => registered.Register(
            new Competitor(CompetitorId.New(), "Beta", CompetitorKind.Individual)));
        Assert.Throws<InvalidOperationException>(() => open.Start());
        Assert.Throws<InvalidOperationException>(() => open.CloseRegistration());
    }

    [Fact]
    public void CompetitionRequiresTwoEntrantsBeforeClosingRegistration()
    {
        var open = new Competition(CompetitionId.New(), "Cup", CompetitionFormat.RoundRobin).OpenRegistration();
        var one = open.Register(new Competitor(CompetitorId.New(), "Solo", CompetitorKind.Individual));
        Assert.Throws<InvalidOperationException>(() => one.CloseRegistration());
    }
}
