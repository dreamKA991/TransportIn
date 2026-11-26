using TransportIn.Services;

namespace TransportIn.Tests;

public class AuthServiceTests
{
    [Theory]
    [InlineData(null, "TestPassword42!")]
    [InlineData("", "TestPassword42!")]
    [InlineData("   ", "TestPassword42!")]
    [InlineData("test-user", null)]
    [InlineData("test-user", "")]
    [InlineData("test-user", "   ")]
    public void TryLogin_RejectsMissingCredentialsBeforeAccessingDatabase(
        string? username, string? password)
    {
        var auth = new AuthService(new NoDatabaseContextFactory());

        var loggedIn = auth.TryLogin(username, password, out var error);

        Assert.False(loggedIn);
        Assert.Equal("Будь ласка, заповніть всі поля для входу!", error);
        Assert.Null(auth.CurrentUser);
    }
}
