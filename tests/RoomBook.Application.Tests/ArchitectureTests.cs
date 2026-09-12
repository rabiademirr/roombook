using NetArchTest.Rules;
using RoomBook.Application;
using RoomBook.Domain;

namespace RoomBook.Application.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_DoesNotDependOnApplicationInfrastructureApiOrAspNetCore()
    {
        var result = Types.InAssembly(typeof(Room).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "RoomBook.Application",
                "RoomBook.Infrastructure",
                "RoomBook.Api",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureApiOrAspNetCore()
    {
        var result = Types.InAssembly(typeof(CreateBookingService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "RoomBook.Infrastructure",
                "RoomBook.Api",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
