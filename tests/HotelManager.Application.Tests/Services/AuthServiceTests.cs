using HotelManager.Application.DTOs.Auth;
using HotelManager.Application.Services;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Application.Tests.TestCommon;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using FluentAssertions;
using Moq;

namespace HotelManager.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenService> _jwtMock;

    public AuthServiceTests()
    {
        _jwtMock = new Mock<IJwtTokenService>();
        _jwtMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(("test-token", DateTime.UtcNow.AddHours(8)));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role = UserRole.Owner }
        };
        var ctx = MockDbContext.CreateWithData(users: users);
        var service = new AuthService(ctx, _jwtMock.Object);

        var result = await service.LoginAsync(new LoginRequest { Username = "admin", Password = "Admin123!" });

        result.Token.Should().Be("test-token");
        result.Role.Should().Be("Owner");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role = UserRole.Owner }
        };
        var ctx = MockDbContext.CreateWithData(users: users);
        var service = new AuthService(ctx, _jwtMock.Object);

        var act = () => service.LoginAsync(new LoginRequest { Username = "admin", Password = "wrong" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ThrowsUnauthorized()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new AuthService(ctx, _jwtMock.Object);

        var act = () => service.LoginAsync(new LoginRequest { Username = "nobody", Password = "x" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_UpdatesPassword()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("oldPass");
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", PasswordHash = hash, Role = UserRole.Owner }
        };
        var ctx = MockDbContext.CreateWithData(users: users);
        var service = new AuthService(ctx, _jwtMock.Object);

        await service.ChangePasswordAsync(1, new ChangePasswordRequest { CurrentPassword = "oldPass", NewPassword = "newPass123" });

        var updated = await ctx.Users.FindAsync(1);
        BCrypt.Net.BCrypt.Verify("newPass123", updated!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrent_ThrowsUnauthorized()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("oldPass");
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", PasswordHash = hash, Role = UserRole.Owner }
        };
        var ctx = MockDbContext.CreateWithData(users: users);
        var service = new AuthService(ctx, _jwtMock.Object);

        var act = () => service.ChangePasswordAsync(1, new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "newPass123" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Current password is incorrect.");
    }

    [Fact]
    public async Task ChangePasswordAsync_ShortNewPassword_Throws()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("oldPass");
        var users = new List<User>
        {
            new() { Id = 1, Username = "admin", PasswordHash = hash, Role = UserRole.Owner }
        };
        var ctx = MockDbContext.CreateWithData(users: users);
        var service = new AuthService(ctx, _jwtMock.Object);

        var act = () => service.ChangePasswordAsync(1, new ChangePasswordRequest { CurrentPassword = "oldPass", NewPassword = "short" });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("New password must be at least 8 characters.");
    }
}
