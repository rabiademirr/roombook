using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RoomBook.Api;

namespace RoomBook.Api.Tests;

public class BookingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BookingEndpointTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateBooking_ValidRequest_Returns201()
    {
        using var client = _factory.CreateClient();
        var request = new CreateBookingRequest("alpha", Utc(9, 0), Utc(9, 30), "Ada", "Standup");

        var response = await client.PostAsJsonAsync("/bookings", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();
        Assert.NotNull(body);
        Assert.Equal("alpha", body!.RoomId);
    }

    [Fact]
    public async Task CreateBooking_UnknownRoom_Returns404NoConflictsOrSuggestions()
    {
        using var client = _factory.CreateClient();
        var request = new CreateBookingRequest("unknown-room", Utc(9, 0), Utc(9, 30), "Ada", "Standup");

        var response = await client.PostAsJsonAsync("/bookings", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RoomNotFoundResponse>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task CreateBooking_InvalidDuration_Returns400()
    {
        using var client = _factory.CreateClient();
        var request = new CreateBookingRequest("beta", Utc(9, 0), Utc(9, 0), "Ada", "Standup");

        var response = await client.PostAsJsonAsync("/bookings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<InvalidBookingResponse>();
        Assert.Contains("InvalidDuration", body!.Violations);
    }

    [Fact]
    public async Task CreateBooking_ConflictingRequest_Returns409WithConflictAndSuggestionDetails()
    {
        using var client = _factory.CreateClient();
        var first = new CreateBookingRequest("gamma", Utc(9, 0), Utc(9, 30), "Grace", "Planning");
        var firstResponse = await client.PostAsJsonAsync("/bookings", first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var conflicting = new CreateBookingRequest("gamma", Utc(9, 15), Utc(9, 45), "Ada", "Standup");
        var response = await client.PostAsJsonAsync("/bookings", conflicting);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConflictResponseBody>();
        Assert.NotNull(body);
        Assert.Single(body!.Conflicts);
        Assert.Equal("Grace", body.Conflicts[0].Organizer);
        Assert.NotEmpty(body.Suggestions);
    }

    [Fact]
    public async Task CreateBooking_ResponseTimes_AreUtc()
    {
        using var client = _factory.CreateClient();
        var request = new CreateBookingRequest("alpha", Utc(10, 0), Utc(10, 30), "Ada", "1:1");

        var response = await client.PostAsJsonAsync("/bookings", request);

        var body = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();
        Assert.Equal(TimeSpan.Zero, body!.Start.Offset);
        Assert.Equal(TimeSpan.Zero, body.End.Offset);
    }
}
