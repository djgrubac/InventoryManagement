using InventoryManagement.Core.DTO;
using InventoryManagement.Domain.Entities;
using MediatR;

namespace InventoryManagement.Core.Users.Query;

public class GetUserQuery: IRequest<UsersDTO>
{
    public string Id { get; set; }

    public GetUserQuery(string id)
    {
        Id = Id;
    }
}