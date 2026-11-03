namespace TheSingularityWorkshop.Economy;

/// <summary>Stable identity for a competition, league, tournament, or organized activity.</summary>
public readonly record struct CompetitionId(Guid Value)
{
    public static CompetitionId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for an individual competitor or an assembled team.</summary>
public readonly record struct CompetitorId(Guid Value)
{
    public static CompetitorId New() => new(Guid.NewGuid());
}

/// <summary>Stable identity for a team roster membership.</summary>
public readonly record struct TeamMembershipId(Guid Value)
{
    public static TeamMembershipId New() => new(Guid.NewGuid());
}

/// <summary>How a competitor participates in an organized competition.</summary>
public enum CompetitorKind
{
    Individual,
    Team
}

/// <summary>Broad competition structures supported by the domain vocabulary.</summary>
/// <remarks>These values describe intent; bracket generation and scheduling engines are not implemented yet.</remarks>
public enum CompetitionFormat
{
    Unspecified,
    SingleElimination,
    DoubleElimination,
    RoundRobin,
    League,
    Swiss,
    Ladder,
    Season
}

/// <summary>Lifecycle of registration and play for a competition.</summary>
public enum CompetitionStatus
{
    Draft,
    RegistrationOpen,
    RegistrationClosed,
    InProgress,
    Completed,
    Cancelled
}

/// <summary>A competitor identity independent of sport, game, or activity.</summary>
public sealed record Competitor
{
    public Competitor(CompetitorId id, string displayName, CompetitorKind kind)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A competitor identifier is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("A competitor display name is required.", nameof(displayName));
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        Id = id;
        DisplayName = displayName.Trim();
        Kind = kind;
    }

    public CompetitorId Id { get; }
    public string DisplayName { get; }
    public CompetitorKind Kind { get; }
}

/// <summary>A person's or entity's membership in an assembled team.</summary>
public sealed record TeamMember
{
    public TeamMember(TeamMembershipId membershipId, CompetitorId memberId, string role, DateTimeOffset joinedAt)
    {
        if (membershipId.Value == Guid.Empty)
            throw new ArgumentException("A membership identifier is required.", nameof(membershipId));
        if (memberId.Value == Guid.Empty)
            throw new ArgumentException("A member competitor identifier is required.", nameof(memberId));
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("A roster role is required.", nameof(role));
        MembershipId = membershipId;
        MemberId = memberId;
        Role = role.Trim();
        JoinedAt = joinedAt;
    }

    public TeamMembershipId MembershipId { get; }
    public CompetitorId MemberId { get; }
    /// <summary>Domain-defined position or responsibility, such as goalkeeper, captain, or strategist.</summary>
    public string Role { get; }
    public DateTimeOffset JoinedAt { get; }
}

/// <summary>
/// An assembled competitor with a stable identity and an explicit roster.
/// Teams can represent sports teams, esports squads, fantasy/dream teams, guilds, or project groups.
/// </summary>
/// <remarks>
/// This first slice models membership, not player statistics, salary caps, draft rules, eligibility,
/// scoring, ownership transfers, or live roster locking. Those belong to policies layered on this primitive.
/// </remarks>
public sealed record Team
{
    private readonly IReadOnlyList<TeamMember> _members;

    public Team(CompetitorId id, string displayName, IEnumerable<TeamMember>? members = null)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A team identifier is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("A team display name is required.", nameof(displayName));

        var snapshot = (members ?? Array.Empty<TeamMember>()).ToArray();
        if (snapshot.Any(member => member is null))
            throw new ArgumentException("A team roster cannot contain null memberships.", nameof(members));
        if (snapshot.Select(member => member.MembershipId).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Membership identifiers must be unique within a team.", nameof(members));
        if (snapshot.Select(member => member.MemberId).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("A competitor can occupy only one roster membership in a team.", nameof(members));

        Id = id;
        DisplayName = displayName.Trim();
        _members = Array.AsReadOnly(snapshot);
    }

    public CompetitorId Id { get; }
    public string DisplayName { get; }
    public IReadOnlyList<TeamMember> Members => _members;
    public Competitor AsCompetitor() => new(Id, DisplayName, CompetitorKind.Team);

    public Team AddMember(TeamMember member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (_members.Any(existing => existing.MemberId == member.MemberId))
            throw new InvalidOperationException("The competitor is already on this team.");
        if (_members.Any(existing => existing.MembershipId == member.MembershipId))
            throw new InvalidOperationException("The membership identifier is already used by this team.");
        return new Team(Id, DisplayName, _members.Append(member));
    }

    public Team RemoveMember(TeamMembershipId membershipId)
    {
        if (membershipId.Value == Guid.Empty)
            throw new ArgumentException("A membership identifier is required.", nameof(membershipId));
        if (!_members.Any(member => member.MembershipId == membershipId))
            throw new KeyNotFoundException("The membership does not exist on this team.");
        return new Team(Id, DisplayName, _members.Where(member => member.MembershipId != membershipId));
    }
}

/// <summary>
/// Immutable competition definition and registration snapshot.
/// Registration returns a new value so callers can retain an auditable prior version.
/// </summary>
public sealed record Competition
{
    private readonly IReadOnlyList<Competitor> _entrants;

    public Competition(
        CompetitionId id,
        string name,
        CompetitionFormat format,
        CompetitionStatus status = CompetitionStatus.Draft,
        IEnumerable<Competitor>? entrants = null,
        int? maximumEntrants = null)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A competition identifier is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A competition name is required.", nameof(name));
        if (!Enum.IsDefined(format))
            throw new ArgumentOutOfRangeException(nameof(format));
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));
        if (maximumEntrants is <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumEntrants));

        var snapshot = (entrants ?? Array.Empty<Competitor>()).ToArray();
        if (snapshot.Any(entrant => entrant is null))
            throw new ArgumentException("A competition cannot contain null entrants.", nameof(entrants));
        if (snapshot.Select(entrant => entrant.Id).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("A competitor can register only once.", nameof(entrants));
        if (maximumEntrants.HasValue && snapshot.Length > maximumEntrants.Value)
            throw new ArgumentException("The entrant count exceeds the competition capacity.", nameof(entrants));

        Id = id;
        Name = name.Trim();
        Format = format;
        Status = status;
        MaximumEntrants = maximumEntrants;
        _entrants = Array.AsReadOnly(snapshot);
    }

    public CompetitionId Id { get; }
    public string Name { get; }
    public CompetitionFormat Format { get; }
    public CompetitionStatus Status { get; }
    public int? MaximumEntrants { get; }
    public IReadOnlyList<Competitor> Entrants => _entrants;

    public Competition OpenRegistration()
    {
        if (Status != CompetitionStatus.Draft)
            throw new InvalidOperationException("Only a draft competition can open registration.");
        return Copy(CompetitionStatus.RegistrationOpen, _entrants);
    }

    public Competition Register(Competitor competitor)
    {
        ArgumentNullException.ThrowIfNull(competitor);
        if (Status != CompetitionStatus.RegistrationOpen)
            throw new InvalidOperationException("Competitor registration is not open.");
        if (_entrants.Any(entrant => entrant.Id == competitor.Id))
            throw new InvalidOperationException("The competitor is already registered.");
        if (MaximumEntrants.HasValue && _entrants.Count >= MaximumEntrants.Value)
            throw new InvalidOperationException("The competition has reached its entrant capacity.");
        return Copy(Status, _entrants.Append(competitor));
    }

    public Competition CloseRegistration()
    {
        if (Status != CompetitionStatus.RegistrationOpen)
            throw new InvalidOperationException("Only an open registration can be closed.");
        if (_entrants.Count < 2)
            throw new InvalidOperationException("A competition requires at least two entrants to proceed.");
        return Copy(CompetitionStatus.RegistrationClosed, _entrants);
    }

    public Competition Start()
    {
        if (Status != CompetitionStatus.RegistrationClosed)
            throw new InvalidOperationException("Registration must be closed before competition starts.");
        return Copy(CompetitionStatus.InProgress, _entrants);
    }

    public Competition Complete()
    {
        if (Status != CompetitionStatus.InProgress)
            throw new InvalidOperationException("Only an in-progress competition can be completed.");
        return Copy(CompetitionStatus.Completed, _entrants);
    }

    public Competition Cancel()
    {
        if (Status is CompetitionStatus.Completed or CompetitionStatus.Cancelled)
            throw new InvalidOperationException("A completed or cancelled competition cannot be cancelled again.");
        return Copy(CompetitionStatus.Cancelled, _entrants);
    }

    private Competition Copy(CompetitionStatus status, IEnumerable<Competitor> entrants) =>
        new(Id, Name, Format, status, entrants, MaximumEntrants);
}
