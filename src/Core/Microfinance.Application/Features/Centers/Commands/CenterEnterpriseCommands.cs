using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Centers.Models;
using Microfinance.Domain.Entities.Centers;
using Microfinance.Domain.Entities.Groups;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Centers.Commands;

public record CreateCenterEnterpriseCommand(CreateCenterEnterpriseRequest Request) : IRequest<Result<Guid>>;
public record UpdateCenterCommand(Guid Id, UpdateCenterRequest Request) : IRequest<Result<string>>;
public record ToggleCenterStatusCommand(Guid Id, bool IsActive) : IRequest<Result<string>>;
public record AssignCenterLeadershipCommand(Guid CenterId, Guid CenterLeaderId, Guid CenterSecretaryId) : IRequest<Result<string>>;

public record CreateSolidarityGroupEnterpriseCommand(CreateSolidarityGroupRequest Request) : IRequest<Result<Guid>>;
public record UpdateSolidarityGroupCommand(Guid GroupId, UpdateSolidarityGroupRequest Request) : IRequest<Result<string>>;
public record ApproveGroupGrtCommand(Guid GroupId) : IRequest<Result<string>>;
public record AssignGroupLeaderEnterpriseCommand(Guid GroupId, Guid LeaderCustomerId) : IRequest<Result<string>>;
public record AddMemberToGroupCommand(Guid GroupId, Guid CustomerId) : IRequest<Result<string>>;
public record ReplaceGroupMemberCommand(Guid GroupId, Guid OutgoingCustomerId, Guid IncomingCustomerId) : IRequest<Result<string>>;
public record RecordCenterMeetingCommand(Guid CenterId, RecordCenterMeetingRequest Request) : IRequest<Result<string>>;

public class CenterEnterpriseCommandHandlers :
    IRequestHandler<CreateCenterEnterpriseCommand, Result<Guid>>,
    IRequestHandler<UpdateCenterCommand, Result<string>>,
    IRequestHandler<ToggleCenterStatusCommand, Result<string>>,
    IRequestHandler<AssignCenterLeadershipCommand, Result<string>>,
    IRequestHandler<CreateSolidarityGroupEnterpriseCommand, Result<Guid>>,
    IRequestHandler<UpdateSolidarityGroupCommand, Result<string>>,
    IRequestHandler<ApproveGroupGrtCommand, Result<string>>,
    IRequestHandler<AssignGroupLeaderEnterpriseCommand, Result<string>>,
    IRequestHandler<AddMemberToGroupCommand, Result<string>>,
    IRequestHandler<ReplaceGroupMemberCommand, Result<string>>,
    IRequestHandler<RecordCenterMeetingCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public CenterEnterpriseCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateCenterEnterpriseCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var code = req.CenterCode.Trim().ToUpperInvariant();

        if (await _context.Centers.AnyAsync(c => c.CenterCode == code, cancellationToken))
            return Result<Guid>.Failure($"Center code '{code}' already exists.");

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == req.BranchId && b.IsActive, cancellationToken);
        if (branch == null)
            return Result<Guid>.Failure("Active branch not found.");

        if (req.FieldOfficerId.HasValue)
        {
            var conflict = await _context.Centers.AnyAsync(c =>
                c.IsActive &&
                c.FieldOfficerId == req.FieldOfficerId.Value &&
                c.MeetingDay == req.MeetingDay &&
                Math.Abs((c.MeetingTime - req.MeetingTime).TotalMinutes) < 60, cancellationToken);

            if (conflict)
                return Result<Guid>.Failure("Field Officer already has another Center meeting scheduled within 1 hour of this day/time.");
        }

        var center = new Center
        {
            CenterCode = code,
            CenterName = req.CenterName.Trim(),
            BranchId = req.BranchId,
            FieldOfficerId = req.FieldOfficerId,
            MeetingDay = req.MeetingDay,
            MeetingTime = req.MeetingTime,
            LocationAddress = req.LocationAddress.Trim(),
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            MaxGroupsAllowed = req.MaxGroupsAllowed > 0 ? req.MaxGroupsAllowed : 8,
            IsActive = true
        };

        _context.Centers.Add(center);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(center.Id);
    }

    public async Task<Result<string>> Handle(UpdateCenterCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var center = await _context.Centers.FirstOrDefaultAsync(c => c.Id == cmd.Id, cancellationToken);
        if (center == null)
            return Result<string>.Failure("Center not found.");

        if (req.FieldOfficerId.HasValue)
        {
            var conflict = await _context.Centers.AnyAsync(c =>
                c.Id != cmd.Id &&
                c.IsActive &&
                c.FieldOfficerId == req.FieldOfficerId.Value &&
                c.MeetingDay == req.MeetingDay &&
                Math.Abs((c.MeetingTime - req.MeetingTime).TotalMinutes) < 60, cancellationToken);

            if (conflict)
                return Result<string>.Failure("Schedule conflict: Field Officer has another Center meeting at this time.");
        }

        center.CenterName = req.CenterName.Trim();
        center.FieldOfficerId = req.FieldOfficerId;
        center.MeetingDay = req.MeetingDay;
        center.MeetingTime = req.MeetingTime;
        center.LocationAddress = req.LocationAddress.Trim();
        center.Latitude = req.Latitude;
        center.Longitude = req.Longitude;
        center.MaxGroupsAllowed = req.MaxGroupsAllowed;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Center updated successfully.");
    }

    public async Task<Result<string>> Handle(ToggleCenterStatusCommand request, CancellationToken cancellationToken)
    {
        var center = await _context.Centers.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center == null)
            return Result<string>.Failure("Center not found.");

        if (!request.IsActive)
        {
            var hasActiveLoans = await _context.LoanAccounts
                .Include(l => l.Customer)
                .AnyAsync(l => l.Customer.CenterId == center.Id && l.Status == LoanStatus.Active, cancellationToken);

            if (hasActiveLoans)
                return Result<string>.Failure("Cannot deactivate Center while members have active loans.");
        }

        center.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(request.IsActive ? "Center activated." : "Center deactivated.");
    }

    public async Task<Result<string>> Handle(AssignCenterLeadershipCommand request, CancellationToken cancellationToken)
    {
        if (request.CenterLeaderId == request.CenterSecretaryId)
            return Result<string>.Failure("Center Leader and Center Secretary must be two different members.");

        var center = await _context.Centers.FirstOrDefaultAsync(c => c.Id == request.CenterId, cancellationToken);
        if (center == null)
            return Result<string>.Failure("Center not found.");

        var leaderValid = await _context.Customers.AnyAsync(c => c.Id == request.CenterLeaderId && c.CenterId == center.Id && !c.IsDeleted, cancellationToken);
        var secretaryValid = await _context.Customers.AnyAsync(c => c.Id == request.CenterSecretaryId && c.CenterId == center.Id && !c.IsDeleted, cancellationToken);

        if (!leaderValid || !secretaryValid)
            return Result<string>.Failure("Both Leader and Secretary must be active members of this Center.");

        var hasArrears = await _context.LoanAccounts.AnyAsync(
            l => (l.CustomerId == request.CenterLeaderId || l.CustomerId == request.CenterSecretaryId) && l.Status == LoanStatus.WrittenOff, cancellationToken);

        if (hasArrears)
            return Result<string>.Failure("Members with written-off/defaulted loans cannot be appointed as Center Leader or Secretary.");

        center.CenterLeaderId = request.CenterLeaderId;
        center.CenterSecretaryId = request.CenterSecretaryId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Center Leader and Secretary appointed successfully.");
    }

    public async Task<Result<Guid>> Handle(CreateSolidarityGroupEnterpriseCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var center = await _context.Centers.FirstOrDefaultAsync(c => c.Id == req.CenterId && c.IsActive, cancellationToken);
        if (center == null)
            return Result<Guid>.Failure("Active Center not found.");

        var currentGroupsCount = await _context.SolidarityGroups.CountAsync(g => g.CenterId == center.Id && g.IsActive, cancellationToken);
        if (currentGroupsCount >= center.MaxGroupsAllowed)
            return Result<Guid>.Failure($"Center capacity limit reached ({center.MaxGroupsAllowed} groups max).");

        var code = req.GroupCode.Trim().ToUpperInvariant();
        if (await _context.SolidarityGroups.AnyAsync(g => g.GroupCode == code, cancellationToken))
            return Result<Guid>.Failure($"Group code '{code}' already exists.");

        var group = new SolidarityGroup
        {
            GroupCode = code,
            GroupName = req.GroupName.Trim(),
            CenterId = center.Id,
            CompletedCgtSessions = 0,
            IsGrtApproved = false,
            GroupSecuritySavings = 0m,
            IsActive = true
        };

        _context.SolidarityGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(group.Id);
    }

    public async Task<Result<string>> Handle(UpdateSolidarityGroupCommand cmd, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FirstOrDefaultAsync(g => g.Id == cmd.GroupId, cancellationToken);
        if (group == null)
            return Result<string>.Failure("Solidarity Group not found.");

        group.GroupName = cmd.Request.GroupName.Trim();
        group.CompletedCgtSessions = cmd.Request.CompletedCgtSessions;
        group.IsActive = cmd.Request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Solidarity Group updated successfully.");
    }

    public async Task<Result<string>> Handle(ApproveGroupGrtCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null)
            return Result<string>.Failure("Solidarity Group not found.");

        var memberCount = await _context.Customers.CountAsync(c => c.SolidarityGroupId == group.Id && !c.IsDeleted, cancellationToken);
        if (memberCount < 3 || memberCount > 5)
            return Result<string>.Failure($"GRT Failed: Solidarity group must have between 3 and 5 members (Current: {memberCount}).");

        if (group.CompletedCgtSessions < 5)
            return Result<string>.Failure($"GRT Failed: Group must complete at least 5 Compulsory Group Training (CGT) sessions (Completed: {group.CompletedCgtSessions}).");

        if (!group.LeaderCustomerId.HasValue)
            return Result<string>.Failure("GRT Failed: Assign a Group Leader before GRT certification.");

        group.IsGrtApproved = true;
        group.GrtApprovedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success($"Group '{group.GroupName}' passed Group Recognition Test (GRT) and is certified for lending.");
    }

    public async Task<Result<string>> Handle(AssignGroupLeaderEnterpriseCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null)
            return Result<string>.Failure("Solidarity Group not found.");

        var isMember = await _context.Customers.AnyAsync(
            c => c.Id == request.LeaderCustomerId && c.SolidarityGroupId == group.Id && !c.IsDeleted, cancellationToken);

        if (!isMember)
            return Result<string>.Failure("Selected customer is not an active member of this group.");

        var hasOverdue = await _context.LoanAccounts.AnyAsync(
            l => l.CustomerId == request.LeaderCustomerId && l.Status == LoanStatus.WrittenOff, cancellationToken);

        if (hasOverdue)
            return Result<string>.Failure("A member with written-off/defaulted loans cannot be appointed as Group Leader.");

        group.LeaderCustomerId = request.LeaderCustomerId;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Group Leader assigned successfully.");
    }

    public async Task<Result<string>> Handle(AddMemberToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId && g.IsActive, cancellationToken);
        if (group == null)
            return Result<string>.Failure("Active Solidarity Group not found.");

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
            return Result<string>.Failure("Customer not found.");

        if (customer.SolidarityGroupId.HasValue)
            return Result<string>.Failure("Customer already belongs to a Solidarity Group. A customer can only join one group.");

        var existingMembers = await _context.Customers
            .Where(c => c.SolidarityGroupId == group.Id && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (existingMembers.Count >= 5)
            return Result<string>.Failure("Solidarity Group capacity full (Maximum 5 members allowed).");

        var normalizedAddress = customer.Address.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(normalizedAddress) &&
            existingMembers.Any(m => m.Address.Trim().ToLowerInvariant() == normalizedAddress))
        {
            return Result<string>.Failure("Household Exclusion Rule: Two members from the same household/address cannot join the same Solidarity Group.");
        }

        customer.CenterId = group.CenterId;
        customer.SolidarityGroupId = group.Id;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success($"Customer '{customer.FirstName} {customer.LastName}' added to group '{group.GroupName}' ({existingMembers.Count + 1}/5).");
    }

    public async Task<Result<string>> Handle(ReplaceGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null)
            return Result<string>.Failure("Solidarity Group not found.");

        var outgoing = await _context.Customers.FirstOrDefaultAsync(
            c => c.Id == request.OutgoingCustomerId && c.SolidarityGroupId == group.Id, cancellationToken);

        var incoming = await _context.Customers.FirstOrDefaultAsync(
            c => c.Id == request.IncomingCustomerId && !c.IsDeleted, cancellationToken);

        if (outgoing == null || incoming == null)
            return Result<string>.Failure("Outgoing or incoming customer not found.");

        if (incoming.SolidarityGroupId.HasValue)
            return Result<string>.Failure("Incoming customer already belongs to another group.");

        var outgoingHasActiveLoan = await _context.LoanAccounts.AnyAsync(
            l => l.CustomerId == outgoing.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.WrittenOff), cancellationToken);

        if (outgoingHasActiveLoan)
            return Result<string>.Failure("Cannot remove member who still has an active or written-off loan.");

        var remainingMembers = await _context.Customers
            .Where(c => c.SolidarityGroupId == group.Id && c.Id != outgoing.Id && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var incomingAddr = incoming.Address.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(incomingAddr) &&
            remainingMembers.Any(m => m.Address.Trim().ToLowerInvariant() == incomingAddr))
        {
            return Result<string>.Failure("Household Exclusion Rule: Incoming member shares an address with an existing group member.");
        }

        outgoing.SolidarityGroupId = null;
        outgoing.CenterId = null;
        if (group.LeaderCustomerId == outgoing.Id)
            group.LeaderCustomerId = null;

        incoming.SolidarityGroupId = group.Id;
        incoming.CenterId = group.CenterId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Group member replaced safely while preserving group size and joint liability.");
    }

    public async Task<Result<string>> Handle(RecordCenterMeetingCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var center = await _context.Centers.FirstOrDefaultAsync(c => c.Id == cmd.CenterId && c.IsActive, cancellationToken);
        if (center == null)
            return Result<string>.Failure("Active Center not found.");

        var utcDate = DateTime.SpecifyKind(req.MeetingDate.Date, DateTimeKind.Utc);

        var isHoliday = await _context.BranchHolidays.AnyAsync(
            h => h.BranchId == center.BranchId && h.HolidayDate == utcDate, cancellationToken);

        if (isHoliday)
            return Result<string>.Failure("Selected date is a Branch Holiday. Shift the Center meeting to the next working day.");

        var distanceMeters = CalculateDistanceMeters(center.Latitude, center.Longitude, req.OfficerLatitude, req.OfficerLongitude);
        bool isGeoVerified = (center.Latitude == 0 && center.Longitude == 0) || distanceMeters <= 150.0;

        var totalSavings = req.Attendances.Sum(a => a.SavingsCollected);

        var meeting = new CenterMeetingSchedule
        {
            CenterId = center.Id,
            MeetingDate = utcDate,
            Notes = req.Notes.Trim(),
            RecordedLatitude = req.OfficerLatitude,
            RecordedLongitude = req.OfficerLongitude,
            IsGeoVerified = isGeoVerified,
            TotalSavingsCollected = totalSavings
        };

        _context.CenterMeetingSchedules.Add(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        var customerIds = req.Attendances.Select(a => a.CustomerId).ToList();
        var customers = await _context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var groups = await _context.SolidarityGroups
            .Where(g => g.CenterId == center.Id)
            .ToDictionaryAsync(g => g.Id, cancellationToken);

        foreach (var att in req.Attendances)
        {
            _context.CenterMeetingAttendances.Add(new CenterMeetingAttendance
            {
                CenterMeetingId = meeting.Id,
                CustomerId = att.CustomerId,
                IsPresent = att.IsPresent,
                SavingsCollected = att.SavingsCollected,
                Remarks = att.Remarks
            });

            if (att.SavingsCollected > 0 &&
                customers.TryGetValue(att.CustomerId, out var cust) &&
                cust.SolidarityGroupId.HasValue &&
                groups.TryGetValue(cust.SolidarityGroupId.Value, out var grp))
            {
                grp.GroupSecuritySavings += att.SavingsCollected;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var geoMsg = isGeoVerified
            ? "GPS Location Verified."
            : $"WARNING: Officer GPS location is {distanceMeters:F0}m away from registered Center coordinates!";

        return Result<string>.Success($"Center meeting recorded. Total Savings Collected: {totalSavings:N2}. {geoMsg}");
    }

    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}