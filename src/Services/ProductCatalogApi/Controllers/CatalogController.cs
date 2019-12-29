using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;
using ProductCatalogApi.Domain;
using ProductCatalogApi.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private CatalogContext Context { get; }
        private IOptionsSnapshot<CatalogSettings> Settings { get; }

        public CatalogController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings)
        {
            Context = context;
            Settings = settings;
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CatalogTypes()
        {
            var items = await Context.CatalogTypes.ToListAsync();
            return Ok(items);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CatalogBrands()
        {
            var items = await Context.CatalogBrands.ToListAsync();
            return Ok(items);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetItemById(int id)
        {
            if (id <= 0)
                return BadRequest($"Invalid id {id}");

            var item = await Context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);
            if (item != null)
            {
                item.PictureUrl = item.PictureUrl.Replace("https://externalcatalogbaseurltobereplaced", Settings.Value.ExternalCatalogBaseUrl); 
                return Ok(item);
            }

            return NotFound();
        }

        // item.PictureUrl = item.PictureUrl.Replace("https://externalcatalogbaseurltobereplaced", Settings.Value.ExternalCatalogBaseUrl);

        private IEnumerable<CatalogItem> ChangeUrlPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach(var item in items)
            {
                item.PictureUrl = item.PictureUrl.Replace("https://externalcatalogbaseurltobereplaced", Settings.Value.ExternalCatalogBaseUrl);
            }
            return items;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Items([FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            int totalItems = await Context.CatalogItems.CountAsync();
            var itemsOnPage = await Context.CatalogItems
                .OrderBy(ci => ci.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUrlPlaceholder(itemsOnPage).ToList();
            var model = new PaginatedItemsViewModel<CatalogItem>(pageSize, pageIndex, totalItems, itemsOnPage);
            return Ok(itemsOnPage);
        }

        [HttpGet("[action]/withname/{name:minlength(1)}")]
        public async Task<IActionResult> Items(string name, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            int totalItems = await Context.CatalogItems
                .Where(ci => ci.Name.StartsWith(name))
                .CountAsync();
            var itemsOnPage = await Context.CatalogItems
                .Where(ci => ci.Name.StartsWith(name))
                .OrderBy(ci => ci.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUrlPlaceholder(itemsOnPage).ToList();
            var model = new PaginatedItemsViewModel<CatalogItem>(pageSize, pageIndex, totalItems, itemsOnPage);
            return Ok(itemsOnPage);
        }

        [HttpGet("[action]/type/{catalogTypeId}/brand/{catalogBrandId}")]
        public async Task<IActionResult> Items(int? catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0)
        {
            int totalItems = await Context.CatalogItems
                .Where(ci => catalogTypeId == null || ci.CatalogTypeId == catalogTypeId)
                .Where(ci => catalogBrandId == null || ci.CatalogBrandId == catalogBrandId)
                .CountAsync();
            var itemsOnPage = await Context.CatalogItems
                .Where(ci => catalogTypeId == null || ci.CatalogTypeId == catalogTypeId)
                .Where(ci => catalogBrandId == null || ci.CatalogBrandId == catalogBrandId)
                .OrderBy(ci => ci.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUrlPlaceholder(itemsOnPage).ToList();
            var model = new PaginatedItemsViewModel<CatalogItem>(pageSize, pageIndex, totalItems, itemsOnPage);
            return Ok(itemsOnPage);
        }

        [HttpPost("items")]
        public async Task<IActionResult> CreateProduct([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price
            };
            Context.CatalogItems.Add(item);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id });
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateProduct([FromBody] CatalogItem catalogItem)
        {
            var item = Context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == catalogItem.Id);
            if (item == null)
                return NotFound();

            Context.CatalogItems.Update(catalogItem);
            await Context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id });
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await Context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);
            if (product == null)
                return NotFound();

            Context.CatalogItems.Remove(product);
            await Context.SaveChangesAsync();
            return NoContent();
        }
    }
}