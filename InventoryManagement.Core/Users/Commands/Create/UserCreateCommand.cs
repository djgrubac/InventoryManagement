using InventoryManagement.Domain.Enums;
using MediatR;

namespace InventoryManagement.Core.Users.Commands.Create;

public class UserCreateCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public string Password { get; set; }
}