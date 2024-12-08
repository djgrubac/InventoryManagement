using InventoryManagement.Core.Interfaces;
using MediatR;

namespace InventoryManagement.Core.Users.Commands.Create;

public class UserCreateCommandHandler : IRequestHandler<UserCreateCommand, Guid>
{
    private readonly IUserService _userService;

    public UserCreateCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Guid> Handle(UserCreateCommand request, CancellationToken cancellationToken)
    {
        return await _userService.CreateUserAsync(request.Name, request.Email, request.Role);
    }
}