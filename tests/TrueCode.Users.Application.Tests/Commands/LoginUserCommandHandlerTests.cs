using Microsoft.Extensions.Options;
using Moq;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Application.Commands.LoginUser;
using TrueCode.Users.Application.Errors;
using TrueCode.Users.Application.Commands.Common;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Tests.Commands;

public class LoginUserCommandHandlerTests
{
    private const string AccessToken = "access-token";
    private const int ExpiresIn = 900;
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserAuthHistoryRepository> _userAuthHistoryRepository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IAccessTokenIssuer> _accessTokens = new();
    private readonly Mock<TimeProvider> _clock = new();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _clock.Setup(c => c.GetUtcNow()).Returns(UserTestData.Now);
        _accessTokens
            .Setup(a => a.Issue(It.IsAny<User>(), It.IsAny<DateTimeOffset>()))
            .Returns(new IssuedAccessToken(AccessToken, ExpiresIn));
        var options = Options.Create(new AuthOptions { RefreshTokenLifetime = RefreshTokenLifetime });
        var tokens = new TokenPairIssuer(_userRepository.Object, _accessTokens.Object, options);
        _handler = new LoginUserCommandHandler(
            _userRepository.Object,
            _userAuthHistoryRepository.Object,
            _hasher.Object,
            tokens,
            _clock.Object);
    }

    private void SetupPasswordCheck(bool isValid) =>
        _hasher
            .Setup(h => h.Verify(It.IsAny<RawPassword>(), It.IsAny<PasswordHash>()))
            .Returns(isValid);

    private void SetupUser(User? user) =>
        _userRepository
            .Setup(r => r.GetByLoginAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    [Fact]
    public async Task Handle_ValidCredentials_IssuesTokensAndRecordsSuccessfulLogin()
    {
        var user = UserTestData.RestoreUser();
        SetupUser(user);
        SetupPasswordCheck(true);

        var result = await _handler.Handle(
            new LoginUserCommand(UserTestData.LoginValue, UserTestData.PasswordValue), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(AccessToken, result.Data.AccessToken);
        Assert.Equal(ExpiresIn, result.Data.ExpiresIn);
        Assert.Equal("Bearer", result.Data.TokenType);
        Assert.False(string.IsNullOrEmpty(result.Data.RefreshToken));
        Assert.Equal(UserTestData.Now, user.LastLoginAt);
        Assert.NotNull(user.RefreshToken);
        Assert.Equal(RefreshTokens.Hash(result.Data.RefreshToken), user.RefreshToken.Hash);
        Assert.Equal(UserTestData.Now + RefreshTokenLifetime, user.RefreshToken.ExpiresAt);
        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userAuthHistoryRepository.Verify(
            r => r.AddAsync(
                It.Is<UserAuthHistoryEntry>(e =>
                    e.UserId == user.Id &&
                    e.EventType == AuthEventType.LoginSucceeded &&
                    e.OccurredAt == UserTestData.Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_RecordsFailureAndIssuesNoTokens()
    {
        var user = UserTestData.RestoreUser();
        SetupUser(user);
        SetupPasswordCheck(false);

        var result = await _handler.Handle(
            new LoginUserCommand(UserTestData.LoginValue, UserTestData.PasswordValue), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Null(user.LastLoginAt);
        Assert.Null(user.RefreshToken);
        _userAuthHistoryRepository.Verify(
            r => r.AddAsync(
                It.Is<UserAuthHistoryEntry>(e =>
                    e.UserId == user.Id &&
                    e.EventType == AuthEventType.LoginFailed),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _accessTokens.Verify(a => a.Issue(It.IsAny<User>(), It.IsAny<DateTimeOffset>()), Times.Never);
        _userRepository.Verify(
            r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownLogin_ReturnsSameErrorAsWrongPassword()
    {
        SetupUser(null);
        var expected = UserErrors.InvalidCredentials<LoginUserResponse>();

        var result = await _handler.Handle(
            new LoginUserCommand(UserTestData.LoginValue, UserTestData.PasswordValue), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected.Errors, result.Errors);
        _userAuthHistoryRepository.Verify(
            r => r.AddAsync(It.IsAny<UserAuthHistoryEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", "password1")]
    [InlineData("us", "password1")]
    [InlineData("user1", "short")]
    public async Task Handle_MalformedCredentials_ReturnsInvalidCredentialsWithoutRepositoryCall(
        string login, string password)
    {
        var expected = UserErrors.InvalidCredentials<LoginUserResponse>();

        var result = await _handler.Handle(new LoginUserCommand(login, password), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expected.Errors, result.Errors);
        _userRepository.Verify(
            r => r.GetByLoginAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
