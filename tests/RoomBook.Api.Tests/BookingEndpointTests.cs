using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RoomBook.Api;

namespace RoomBook.Api.Tests;

public class BookingEndpointTests
{
    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateBooking_ValidRequest_Returns201()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
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
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var request = new CreateBookingRequest("unknown-room", Utc(9, 0), Utc(9, 30), "Ada", "Standup");

        var response = await client.PostAsJsonAsync("/bookings", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RoomNotFoundResponse>();
        Assert.NotNull(body);
    }

    [Theory]
    [InlineData(null, "Standup")]
    [InlineData("", "Standup")]
    [InlineData("   ", "Standup")]
    [InlineData("Ada", null)]
    [InlineData("Ada", "")]
    public async Task CreateBooking_MissingOrganizerOrTitle_Returns400(string? organizer, string? title)
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var payload = new
        {
            RoomId = "alpha",
            Start = Utc(11, 0),
            End = Utc(11, 30),
            Organizer = organizer,
            Title = title,
        };

        var response = await client.PostAsJsonAsync("/bookings", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<InvalidBookingResponse>();
        Assert.NotEmpty(body!.Violations);
    }

    [Fact]
    public async Task CreateBooking_InvalidDuration_Returns400()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var request = new CreateBookingRequest("beta", Utc(9, 0), Utc(9, 0), "Ada", "Standup");

        var response = await client.PostAsJsonAsync("/bookings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<InvalidBookingResponse>();
        Assert.Contains("InvalidDuration", body!.Violations);
    }

    [Fact]
    public async Task CreateBooking_FullOverlap_Returns409WithConflictDetails()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var first = new CreateBookingRequest("gamma", Utc(9, 0), Utc(9, 30), "Grace", "Planning");
        var firstResponse = await client.PostAsJsonAsync("/bookings", first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var conflicting = new CreateBookingRequest("gamma", Utc(9, 0), Utc(9, 30), "Ada", "Standup");
        var response = await client.PostAsJsonAsync("/bookings", conflicting);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConflictResponseBody>();
        Assert.NotNull(body);
        Assert.Single(body!.Conflicts);
        Assert.Equal("Grace", body.Conflicts[0].Organizer);
        Assert.Equal(Utc(9, 0), body.Conflicts[0].Start);
        Assert.Equal(Utc(9, 30), body.Conflicts[0].End);
        Assert.NotEmpty(body.Suggestions);
    }

    [Fact]
    public async Task CreateBooking_ResponseTimes_AreUtc()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var request = new CreateBookingRequest("alpha", Utc(10, 0), Utc(10, 30), "Ada", "1:1");

        var response = await client.PostAsJsonAsync("/bookings", request);

        var body = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();
        Assert.Equal(TimeSpan.Zero, body!.Start.Offset);
        Assert.Equal(TimeSpan.Zero, body.End.Offset);
    }
}
