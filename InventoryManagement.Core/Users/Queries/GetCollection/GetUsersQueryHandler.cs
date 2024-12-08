using InventoryManagement.Core.DTO;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Core.Users.Commands.Create;
using InventoryManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Core.Users.Query;

public class GetUsersQueryHandler: IRequestHandler<GetUsersQuery, List<UsersDTO>>
{
    private readonly UserManager<User> _userManager;

    public GetUsersQueryHandler(UserManager<User> userManager)   
    {
        _userManager = userManager;
    }

    public async Task<List<UsersDTO>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userManager.Users.Select(u=>new UsersDTO
        {
            Id = u.Id,
            Username = u.UserName,
            Role = u.Role.ToString()
        }).ToListAsync(cancellationToken);
    }
}