using Microsoft.Extensions.Options;
using Moq;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Application.Commands;
using TrueCode.Users.Application.Commands.RefreshToken;
using TrueCode.Users.Application.Errors;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Tests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private const string OldRawToken = "old-refresh-token";
    private const string AccessToken = "access-token";
    private const int ExpiresIn = 900;
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserAuthHistoryRepository> _userAuthHistoryRepository = new();
    private readonly Mock<IAccessTokenIssuer> _accessTokens = new();
    private readonly Mock<TimeProvider> _clock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _clock.Setup(c => c.GetUtcNow()).Returns(UserTestData.Now);
        _accessTokens
            .Setup(a => a.Issue(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(new IssuedAccessToken(AccessToken, ExpiresIn));
        var options = Options.Create(new AuthOptions { RefreshTokenLifetime = RefreshTokenLifetime });
        var tokens = new TokenPairIssuer(_userRepository.Object, _accessTokens.Object, options);
        _handler = new RefreshTokenCommandHandler(
            _userRepository.Object,
            _userAuthHistoryRepository.Object,
            tokens,
            _clock.Object);
    }

    private void SetupUserByToken(User? user) =>
        _userRepository
            .Setup(r => r.GetByRefreshTokenHashAsync(RefreshTokens.Hash(OldRawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    [Fact]
    public async Task Handle_ValidToken_RotatesRefreshTokenAndRecordsEvent()
    {
        var oldToken = new RefreshToken(RefreshTokens.Hash(OldRawToken), UserTestData.Now.AddHours(1));
        var user = UserTestData.RestoreUser(oldToken);
        SetupUserByToken(user);

        var result = await _handler.Handle(new RefreshTokenCommand(OldRawToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(AccessToken, result.Data.AccessToken);
        Assert.NotEqual(OldRawToken, result.Data.RefreshToken);
        Assert.NotNull(user.RefreshToken);
        Assert.Equal(RefreshTokens.Hash(result.Data.RefreshToken), user.RefreshToken.Hash);
        Assert.Equal(UserTestData.Now + RefreshTokenLifetime, user.RefreshToken.ExpiresAt);
        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userAuthHistoryRepository.Verify(
            r => r.AddAsync(
                It.Is<UserAuthHistoryEntry>(e =>
                    e.UserId == user.Id &&
                    e.EventType == AuthEventType.TokenRefreshed &&
                    e.OccurredAt == UserTestData.Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsErrorAndKeepsToken()
    {
        var expiredToken = new RefreshToken(RefreshTokens.Hash(OldRawToken), UserTestData.Now.AddMinutes(-1));
        var user = UserTestData.RestoreUser(expiredToken);
        SetupUserByToken(user);
        var expected = UserErrors.InvalidRefreshToken<RefreshTokenResponse>();

        var result = await _handler.Handle(new RefreshTokenCommand(OldRawToken), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected.Errors, result.Errors);
        Assert.Same(expiredToken, user.RefreshToken);
        _userRepository.Verify(
            r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsError()
    {
        SetupUserByToken(null);
        var expected = UserErrors.InvalidRefreshToken<RefreshTokenResponse>();

        var result = await _handler.Handle(new RefreshTokenCommand(OldRawToken), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected.Errors, result.Errors);
        _userAuthHistoryRepository.Verify(
            r => r.AddAsync(It.IsAny<UserAuthHistoryEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyToken_ReturnsErrorWithoutRepositoryCall(string token)
    {
        var result = await _handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _userRepository.Verify(
            r => r.GetByRefreshTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
