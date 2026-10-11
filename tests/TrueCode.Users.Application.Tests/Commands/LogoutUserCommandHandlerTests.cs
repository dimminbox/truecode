using Moq;
using TrueCode.Users.Application.Auth;
using TrueCode.Users.Application.Commands;
using TrueCode.Users.Domain.AuthHistory;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Tests.Commands;

public class LogoutUserCommandHandlerTests
{
    private const string RawToken = "refresh-token";

    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserAuthHistoryRepository> _userAuthHistoryRepository = new();
    private readonly Mock<TimeProvider> _clock = new();
    private readonly LogoutUserCommandHandler _handler;

    public LogoutUserCommandHandlerTests()
    {
        _clock.Setup(c => c.GetUtcNow()).Returns(UserTestData.Now);
        _handler = new LogoutUserCommandHandler(
            _userRepository.Object, _userAuthHistoryRepository.Object, _clock.Object);
    }

    [Fact]
    public async Task Handle_KnownToken_RevokesTokenAndRecordsLogout()
    {
        var token = new RefreshToken(RefreshTokens.Hash(RawToken), UserTestData.Now.AddHours(1));
        var user = UserTestData.RestoreUser(token);
        _userRepository
            .Setup(r => r.GetByRefreshTokenHashAsync(RefreshTokens.Hash(RawToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new LogoutUserCommand(RawToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(user.RefreshToken);
        _userRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userAuthHistoryRepository.Verify(
            r => r.AddAsync(
                It.Is<UserAuthHistoryEntry>(e =>
                    e.UserId == user.Id &&
                    e.EventType == AuthEventType.LoggedOut &&
                    e.OccurredAt == UserTestData.Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyToken_ReturnsSuccessWithoutRepositoryCall(string token)
    {
        var result = await _handler.Handle(new LogoutUserCommand(token), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _userRepository.Verify(
            r => r.GetByRefreshTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
