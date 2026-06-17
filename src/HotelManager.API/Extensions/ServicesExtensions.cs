using FluentValidation;
using FluentValidation.AspNetCore;
using HotelManager.Application.Services;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Application.Validators;
using HotelManager.Domain.Interfaces;
using HotelManager.Infrastructure.Persistence;
using HotelManager.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.API.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddValidatorsFromAssemblyContaining<CreateRoomRequestValidator>();
        services.AddFluentValidationAutoValidation();

        return services;
    }
}
