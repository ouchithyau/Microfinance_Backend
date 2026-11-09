using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Application.Features.Users.Commands;

public record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    Guid BranchId,
    Guid RoleId) : IRequest<Result<Guid>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Username).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.Password).NotEmpty().MinimumLength(6);
        RuleFor(v => v.FullName).NotEmpty().MaximumLength(150);
        RuleFor(v => v.PhoneNumber).NotEmpty().Matches(@"^\+?\d{10,12}$");
        RuleFor(v => v.BranchId).NotEmpty();
        RuleFor(v => v.RoleId).NotEmpty();
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email, cancellationToken);
        if (exists)
        {
            return Result<Guid>.Failure("Username or Email already registered.");
        }

        // Production එකේදී BCrypt.Net භාවිත වේ
        var user = new ApplicationUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Password)),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            BranchId = request.BranchId,
            RoleId = request.RoleId,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id, "User created successfully.");
    }
}