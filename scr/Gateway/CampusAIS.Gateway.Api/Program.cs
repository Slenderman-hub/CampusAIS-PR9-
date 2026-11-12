using CampusAIS.Gateway.Api;
using CampusAIS.Gateway.Booking;
using CampusAIS.Gateway.Sync;
using CampusAIS.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация зависимостей
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddSingleton<IScheduleSyncService, ScheduleSyncService>();

// Регистрация сервисов для работы с пропусками и Outbox
builder.Services.AddSingleton<PassService>();
builder.Services.AddHostedService<OutboxWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/schedule", () =>
{
    var lessons = new List<LessonDto>
    {
        new(Guid.NewGuid(), "Математика", "Иванов И.И.", "Ауд. 101", DateTime.Today.AddHours(9), DateTime.Today.AddHours(10.5)),
        new(Guid.NewGuid(), "Программирование", "Петров П.П.", "Ауд. 202", DateTime.Today.AddHours(11), DateTime.Today.AddHours(12.5))
    };
    return Results.Ok(new ScheduleGridDto(lessons));
});

app.MapPost("/rooms/book", async (RoomBookingRequest request, IBookingService bookingService) =>
{
    var result = await bookingService.BookRoomAsync(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// Отзыв пропуска с публикацией события
app.MapPost("/passes/{id}/revoke", (Guid id, PassService passService) =>
{
    passService.RevokePass(id);
    return Results.Ok(new { Message = "Pass revoked successfully." });
});

app.Run();