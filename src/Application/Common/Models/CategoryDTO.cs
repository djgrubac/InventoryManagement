using System.Linq.Expressions;

namespace Inventory_Management.Application.Common.Models;

public class CategoryDTO
{
    public Guid Uid { get; set; }
    public string? Caption { get; set; }

    public static Expression<Func<Domain.Entities.Category, CategoryDTO>> Projection
    {
        get
        {
            return entity => new CategoryDTO() 
            { 
                Uid = entity.Uid, 
                Caption = entity.Caption 
            };
        }
    }
}
