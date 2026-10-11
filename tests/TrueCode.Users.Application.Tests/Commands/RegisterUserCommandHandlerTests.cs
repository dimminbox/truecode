using Moq;
using TrueCode.Users.Application.Commands;
using TrueCode.Users.Application.Commands.RegisterUser;
using TrueCode.Users.Domain.UserAggregage;
using TrueCode.Users.Domain.UserAggregage.ValueObjects;
using TrueCode.Users.Domain.Users;

namespace TrueCode.Users.Application.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<TimeProvider> _clock = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _clock.Setup(c => c.GetUtcNow()).Returns(UserTestData.Now);
        _hasher
            .Setup(h => h.Hash(It.IsAny<RawPassword>()))
            .Returns(new PasswordHash(UserTestData.PasswordHashValue));
        _handler = new RegisterUserCommandHandler(_userRepository.Object, _hasher.Object, _clock.Object);
    }

    [Fact]
    public async Task Handle_NewUser_AddsUserWithHashedPasswordAndNormalizedLogin()
    {
        _userRepository
            .Setup(r => r.ExistsByLoginAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var command = new RegisterUserCommand("Иван", "User1", UserTestData.PasswordValue);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEqual(Guid.Empty, result.Data.Id);
        _userRepository.Verify(
            r => r.AddAsync(
                It.Is<User>(u =>
                    u.Id == result.Data.Id &&
                    u.Name.Value == "Иван" &&
                    u.Login.Value == UserTestData.LoginValue &&
                    u.PasswordHash.Value == UserTestData.PasswordHashValue &&
                    u.CreatedAt == UserTestData.Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LoginTaken_ReturnsErrorWithoutAdding()
    {
        _userRepository
            .Setup(r => r.ExistsByLoginAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var command = new RegisterUserCommand("Иван", UserTestData.LoginValue, UserTestData.PasswordValue);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains(UserTestData.LoginValue, result.GetErrorsString());
        _userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", "user1", "password1")]
    [InlineData("Иван", "us", "password1")]
    [InlineData("Иван", "user 1", "password1")]
    [InlineData("Иван", "user1", "short1")]
    [InlineData("Иван", "user1", "onlyletters")]
    public async Task Handle_InvalidData_ReturnsErrorWithoutRepositoryCalls(string name, string login, string password)
    {
        var result = await _handler.Handle(new RegisterUserCommand(name, login, password), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        _userRepository.Verify(
            r => r.ExistsByLoginAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
