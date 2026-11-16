namespace Microfinance.Application.Features.Centers.Models;

public record CenterDetailDto(
    Guid Id,
    string CenterCode,
    string CenterName,
    Guid BranchId,
    Guid? FieldOfficerId,
    DayOfWeek MeetingDay,
    TimeSpan MeetingTime,
    string LocationAddress,
    double Latitude,
    double Longitude,
    int MaxGroupsAllowed,
    int CurrentGroupCount,
    Guid? CenterLeaderId,
    Guid? CenterSecretaryId,
    bool IsActive
);

public record CreateCenterEnterpriseRequest(
    string CenterCode,
    string CenterName,
    Guid BranchId,
    Guid? FieldOfficerId,
    DayOfWeek MeetingDay,
    TimeSpan MeetingTime,
    string LocationAddress,
    double Latitude,
    double Longitude,
    int MaxGroupsAllowed
);

public record UpdateCenterRequest(
    string CenterName,
    Guid? FieldOfficerId,
    DayOfWeek MeetingDay,
    TimeSpan MeetingTime,
    string LocationAddress,
    double Latitude,
    double Longitude,
    int MaxGroupsAllowed
);

public record AssignCenterLeadershipRequest(
    Guid CenterLeaderId,
    Guid CenterSecretaryId
);

public record SolidarityGroupDetailDto(
    Guid Id,
    string GroupCode,
    string GroupName,
    Guid CenterId,
    Guid? GroupLeaderId,
    int MemberCount,
    int CompletedCgtSessions,
    bool IsGrtApproved,
    decimal GroupSecuritySavings,
    bool IsActive
);

public record CreateSolidarityGroupRequest(
    string GroupCode,
    string GroupName,
    Guid CenterId
);

public record UpdateSolidarityGroupRequest(
    string GroupName,
    int CompletedCgtSessions,
    bool IsActive
);

public record ReplaceGroupMemberRequest(
    Guid OutgoingCustomerId,
    Guid IncomingCustomerId
);

public record GroupLoanEligibilityDto(
    Guid GroupId,
    string GroupName,
    bool IsEligibleForNewLoans,
    int MemberCount,
    bool IsGrtApproved,
    decimal GroupSecuritySavings,
    string DisbursementStageRule,
    List<string> BlockingReasons
);

public record MemberAttendanceInput(
    Guid CustomerId,
    bool IsPresent,
    decimal SavingsCollected,
    string? Remarks
);

public record RecordCenterMeetingRequest(
    DateTime MeetingDate,
    double OfficerLatitude,
    double OfficerLongitude,
    string Notes,
    List<MemberAttendanceInput> Attendances
);

public record CenterGradingDto(
    Guid CenterId,
    string CenterName,
    string Grade,
    double AverageAttendancePercentage,
    double OnTimeRepaymentPercentage,
    int TotalActiveLoans,
    int OverdueLoansCount,
    string Recommendation
);