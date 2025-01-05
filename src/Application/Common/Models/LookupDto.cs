using Inventory_Management.Domain.Entities;
using InventoryManagement.Core.DTO;

namespace Inventory_Management.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Products, LookupDto>();
        }
    }
}
