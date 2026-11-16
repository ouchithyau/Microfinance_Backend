using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Centers.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Centers.Queries;

public record GetAllCentersQuery(Guid? BranchId, Guid? FieldOfficerId) : IRequest<Result<List<CenterDetailDto>>>;
public record GetCenterByIdQuery(Guid Id) : IRequest<Result<CenterDetailDto>>;
public record GetGroupsByCenterQuery(Guid CenterId) : IRequest<Result<List<SolidarityGroupDetailDto>>>;
public record CheckGroupLoanEligibilityQuery(Guid GroupId) : IRequest<Result<GroupLoanEligibilityDto>>;
public record GetCenterGradingQuery(Guid CenterId) : IRequest<Result<CenterGradingDto>>;
public record GetMeetingAttendanceQuery(Guid MeetingId) : IRequest<Result<object>>;

public class CenterEnterpriseQueryHandlers :
    IRequestHandler<GetAllCentersQuery, Result<List<CenterDetailDto>>>,
    IRequestHandler<GetCenterByIdQuery, Result<CenterDetailDto>>,
    IRequestHandler<GetGroupsByCenterQuery, Result<List<SolidarityGroupDetailDto>>>,
    IRequestHandler<CheckGroupLoanEligibilityQuery, Result<GroupLoanEligibilityDto>>,
    IRequestHandler<GetCenterGradingQuery, Result<CenterGradingDto>>,
    IRequestHandler<GetMeetingAttendanceQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;
    public CenterEnterpriseQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<CenterDetailDto>>> Handle(GetAllCentersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Centers.AsQueryable();

        if (request.BranchId.HasValue)
            query = query.Where(c => c.BranchId == request.BranchId.Value);

        if (request.FieldOfficerId.HasValue)
            query = query.Where(c => c.FieldOfficerId == request.FieldOfficerId.Value);

        var centers = await query
            .OrderBy(c => c.CenterCode)
            .Select(c => new CenterDetailDto(
                c.Id,
                c.CenterCode,
                c.CenterName,
                c.BranchId,
                c.FieldOfficerId,
                c.MeetingDay,
                c.MeetingTime,
                c.LocationAddress,
                c.Latitude,
                c.Longitude,
                c.MaxGroupsAllowed,
                _context.SolidarityGroups.Count(g => g.CenterId == c.Id && g.IsActive),
                c.CenterLeaderId,
                c.CenterSecretaryId,
                c.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result<List<CenterDetailDto>>.Success(centers);
    }

    public async Task<Result<CenterDetailDto>> Handle(GetCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.Centers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (c == null)
            return Result<CenterDetailDto>.Failure("Center not found.");

        var groupCount = await _context.SolidarityGroups.CountAsync(g => g.CenterId == c.Id && g.IsActive, cancellationToken);

        return Result<CenterDetailDto>.Success(new CenterDetailDto(
            c.Id, c.CenterCode, c.CenterName, c.BranchId, c.FieldOfficerId, c.MeetingDay, c.MeetingTime,
            c.LocationAddress, c.Latitude, c.Longitude, c.MaxGroupsAllowed, groupCount,
            c.CenterLeaderId, c.CenterSecretaryId, c.IsActive));
    }

    public async Task<Result<List<SolidarityGroupDetailDto>>> Handle(GetGroupsByCenterQuery request, CancellationToken cancellationToken)
    {
        var groups = await _context.SolidarityGroups
            .Where(g => g.CenterId == request.CenterId)
            .OrderBy(g => g.GroupCode)
            .Select(g => new SolidarityGroupDetailDto(
                g.Id,
                g.GroupCode,
                g.GroupName,
                g.CenterId,
                g.LeaderCustomerId,
                _context.Customers.Count(c => c.SolidarityGroupId == g.Id && !c.IsDeleted),
                g.CompletedCgtSessions,
                g.IsGrtApproved,
                g.GroupSecuritySavings,
                g.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result<List<SolidarityGroupDetailDto>>.Success(groups);
    }

    public async Task<Result<GroupLoanEligibilityDto>> Handle(CheckGroupLoanEligibilityQuery request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        if (group == null)
            return Result<GroupLoanEligibilityDto>.Failure("Solidarity Group not found.");

        var blockingReasons = new List<string>();

        if (!group.IsActive)
            blockingReasons.Add("Solidarity Group is inactive.");

        if (!group.IsGrtApproved)
            blockingReasons.Add("Group has not passed the Group Recognition Test (GRT).");

        var members = await _context.Customers
            .Where(c => c.SolidarityGroupId == group.Id && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (members.Count < 3 || members.Count > 5)
            blockingReasons.Add($"Group must have 3 to 5 members (Current: {members.Count}).");

        var memberIds = members.Select(m => m.Id).ToList();

        var overdueCount = await _context.LoanAccounts.CountAsync(
            l => memberIds.Contains(l.CustomerId) && l.Status == LoanStatus.WrittenOff, cancellationToken);

        if (overdueCount > 0)
            blockingReasons.Add($"Joint Liability Lock: {overdueCount} member(s) in this group currently have defaulted loan arrears.");

        var activeLoansCount = await _context.LoanAccounts.CountAsync(
            l => memberIds.Contains(l.CustomerId) && l.Status == LoanStatus.Active, cancellationToken);

        var completedLoansCount = await _context.LoanAccounts.CountAsync(
            l => memberIds.Contains(l.CustomerId) && l.Status == LoanStatus.Settled, cancellationToken);

        string stageRule = completedLoansCount == 0
            ? (activeLoansCount < 2
                ? "Cycle 1 - Stage 1: Up to 2 members eligible for initial disbursement."
                : activeLoansCount < 4
                    ? "Cycle 1 - Stage 2: Next 2 members eligible after 4 weeks of on-time payments by Stage 1."
                    : "Cycle 1 - Stage 3: Group Leader eligible as final member.")
            : "Cycle 2+: Full group eligible simultaneously.";

        return Result<GroupLoanEligibilityDto>.Success(new GroupLoanEligibilityDto(
            group.Id,
            group.GroupName,
            blockingReasons.Count == 0,
            members.Count,
            group.IsGrtApproved,
            group.GroupSecuritySavings,
            stageRule,
            blockingReasons
        ));
    }

    public async Task<Result<CenterGradingDto>> Handle(GetCenterGradingQuery request, CancellationToken cancellationToken)
    {
        var center = await _context.Centers.FirstOrDefaultAsync(c => c.Id == request.CenterId, cancellationToken);
        if (center == null)
            return Result<CenterGradingDto>.Failure("Center not found.");

        var meetingIds = await _context.CenterMeetingSchedules
            .Where(m => m.CenterId == center.Id)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        double attendancePct = 100.0;
        if (meetingIds.Count > 0)
        {
            var totalRecords = await _context.CenterMeetingAttendances.CountAsync(a => meetingIds.Contains(a.CenterMeetingId), cancellationToken);
            if (totalRecords > 0)
            {
                var presentRecords = await _context.CenterMeetingAttendances.CountAsync(a => meetingIds.Contains(a.CenterMeetingId) && a.IsPresent, cancellationToken);
                attendancePct = Math.Round((double)presentRecords / totalRecords * 100.0, 2);
            }
        }

        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Where(l => l.Customer.CenterId == center.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.WrittenOff))
            .ToListAsync(cancellationToken);

        var overdueLoans = loans.Count(l => l.Status == LoanStatus.WrittenOff);
        double onTimePct = loans.Count == 0 ? 100.0 : Math.Round((double)(loans.Count - overdueLoans) / loans.Count * 100.0, 2);

        string grade;
        string recommendation;

        if (attendancePct >= 90 && onTimePct == 100)
        {
            grade = "Grade A (Platinum Center)";
            recommendation = "Eligible for next loan cycle limit increment (+25%) and preferential interest rate.";
        }
        else if (attendancePct >= 80 && onTimePct >= 95)
        {
            grade = "Grade B (Standard Good)";
            recommendation = "Eligible for standard loan renewals.";
        }
        else if (attendancePct >= 70 && onTimePct >= 85)
        {
            grade = "Grade C (Watchlist)";
            recommendation = "Branch Manager counseling required; freeze loan limit increments.";
        }
        else
        {
            grade = "Grade D (High Risk / Frozen)";
            recommendation = "Suspend new disbursements until arrears are cleared and attendance exceeds 80%.";
        }

        return Result<CenterGradingDto>.Success(new CenterGradingDto(
            center.Id, center.CenterName, grade, attendancePct, onTimePct, loans.Count, overdueLoans, recommendation));
    }

    public async Task<Result<object>> Handle(GetMeetingAttendanceQuery request, CancellationToken cancellationToken)
    {
        var records = await _context.CenterMeetingAttendances
            .Where(a => a.CenterMeetingId == request.MeetingId)
            .Join(_context.Customers,
                a => a.CustomerId,
                c => c.Id,
                (a, c) => new
                {
                    a.CustomerId,
                    CustomerName = c.FirstName + " " + c.LastName,
                    a.IsPresent,
                    a.SavingsCollected,
                    a.Remarks
                })
            .ToListAsync(cancellationToken);

        return Result<object>.Success(records);
    }
}