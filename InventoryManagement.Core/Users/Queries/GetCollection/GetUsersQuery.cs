using InventoryManagement.Core.DTO;
using MediatR;

namespace InventoryManagement.Core.Users.Query;

public class GetUsersQuery: IRequest<List<UsersDTO>> {}