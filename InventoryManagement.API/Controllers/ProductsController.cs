// using InventoryManagement.Core.DTO;
// using InventoryManagement.Infrastructure.Data;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Swashbuckle.AspNetCore.Annotations;
//
// namespace InventoryManagement.API.Controllers;
//
// public class ProductsController
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ProductController : ControllerBase
//     {
//         private readonly DataContext _context;
//         private readonly IConfiguration _configuration;
//         
//         public ProductController(DataContext context, IConfiguration configuration)
//         {
//             _context = context;
//             _configuration = configuration;
//         }
//
//         [HttpGet]
//         [Authorize] // Uncomment if admin-only access is required
//         [SwaggerOperation(Summary = "Get all products", Description = "Retrieve a list of all products.")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//         public async Task<IActionResult> GetProducts()
//         {
//             var products = await _context.Products.ToListAsync();
//             return Ok(products);
//         }
//         
//         [HttpGet("{id}")]
//         [Authorize] // Uncomment if admin-only access is required
//         [SwaggerOperation(Summary = "Get products by ID", Description = "Retrieve the details of a specific product by ID.")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//         public async Task<IActionResult> GetUserById(int id)
//         {
//             var products = await _context.Users.FindAsync(id);
//             if (products == null) return NotFound();
//             return Ok(products);
//         }
//
//         [HttpPost]
//         [Authorize] // Uncomment if admin-only access is required
//         [SwaggerOperation(Summary = "Add a new product", Description = "Create a new product and store it.")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status400BadRequest)]
//         [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//         public async Task<IActionResult> AddProduct([FromBody] ProductsDTO productDto)
//         {
//             if (productDto == null)
//             {
//                 return BadRequest();
//             }
//
//             await _context.Products.AddAsync(productDto);
//             await _context.SaveChangesAsync();
//
//             return Ok(productDto);
//         }
//         
//         [HttpPut("{id}")]
//         [Authorize] // Uncomment if admin-only access is required
//         [SwaggerOperation(Summary = "Update a product", Description = "Update an existing product by ID.")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//         public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductsDTO updatedProductDto)
//         {
//             var existingProduct = await _context.Products.FindAsync(id);
//             if (existingProduct == null)
//             {
//                 return NotFound();
//             }
//
//             existingProduct.Name = updatedProductDto.Name;
//             existingProduct.Price = updatedProductDto.Price;
//             existingProduct.StockQuantity = updatedProductDto.StockQuantity;
//
//             _context.Products.Update(existingProduct);
//             await _context.SaveChangesAsync();
//
//             return Ok(existingProduct);
//         }
//         
//         [HttpDelete("{id}")]
//         [Authorize] // Uncomment if admin-only access is required
//         [SwaggerOperation(Summary = "Delete a product", Description = "Delete a product by ID.")]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status404NotFound)]
//         [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//         public async Task<IActionResult> DeleteProduct(int id)
//         {
//             var product = await _context.Products.FindAsync(id);
//             if (product == null)
//             {
//                 return NotFound();
//             }
//
//             _context.Products.Remove(product);
//             await _context.SaveChangesAsync();
//
//             return Ok();
//         }
//     }
// }