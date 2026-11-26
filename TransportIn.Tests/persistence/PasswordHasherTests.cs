using TransportIn.Services;

namespace TransportIn.Tests;

public class PasswordHasherTests
{
    [Theory]
    [InlineData("TestPassword42!")]
    [InlineData("Тестовий пароль 🚚")]
    public void Verify_AcceptsOriginalPassword(string password)
    {
        var hash = PasswordHasher.Hash(password);

        Assert.True(PasswordHasher.Verify(password, hash));
    }

    [Fact]
    public void Verify_RejectsDifferentPassword()
    {
        var hash = PasswordHasher.Hash("TestPassword42!");

        Assert.False(PasswordHasher.Verify("WrongPassword42!", hash));
    }

    [Fact]
    public void Hash_UsesDifferentSaltForTheSamePassword()
    {
        const string password = "TestPassword42!";
        var firstHash = PasswordHasher.Hash(password);
        var secondHash = PasswordHasher.Hash(password);

        Assert.NotEqual(firstHash.Split('$')[2], secondHash.Split('$')[2]);
        Assert.True(PasswordHasher.Verify(password, firstHash));
        Assert.True(PasswordHasher.Verify(password, secondHash));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-format")]
    [InlineData("PBKDF2$invalid-iteration-count$AA==$AA==")]
    [InlineData("PBKDF2$100000$invalid-base64$AA==")]
    public void Verify_RejectsMissingOrMalformedHash(string? hash)
    {
        Assert.False(PasswordHasher.Verify("TestPassword42!", hash));
    }
}
