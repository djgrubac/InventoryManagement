using InventoryManagement.Core.DTO;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Core.Users.Query;

public class GetUserQueryHandler: IRequestHandler<GetUserQuery, UsersDTO>
{
    private readonly UserManager<User> _userManager;

    public GetUserQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<UsersDTO> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.Id} not found.");
        }

        return new UsersDTO
        {
            Id = user.Id,
            Username = user.UserName,
            Role = user.Role.ToString(),
        };
    }
}