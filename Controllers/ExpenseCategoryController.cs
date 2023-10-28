using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Impensa.DTOs;
using Impensa.Models;
using Impensa.Repositories;

namespace Impensa.Controllers
{
    [ApiController]
    [Route("/api/v1/categories")]
    [Authorize]
    public class ExpenseCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseCategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<ExpenseCategoryDisplayDto>> GetCategories()
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var categories = _context.ExpenseCategories
                .Where(c => c.UserId == userId)
                .Select(c => new ExpenseCategoryDisplayDto
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList();

            return categories;
        }

        [HttpGet("{id}")]
        public ActionResult<ExpenseCategoryDisplayDto> GetCategory(Guid id)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var category = _context.ExpenseCategories
                .Where(c => c.Id == id && c.UserId == userId)
                .Select(c => new ExpenseCategoryDisplayDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpPost]
        public ActionResult<ExpenseCategoryDisplayDto> CreateCategory(ExpenseCategoryCreateDto categoryDto)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingCategory = _context.ExpenseCategories
                .FirstOrDefault(c => c.UserId == userId && c.Name == categoryDto.Name);

            if (existingCategory != null)
            {
                return Conflict(new { message = "Category with this name already exists for the user." });
            }

            var newCategory = new ExpenseCategory
            {
                Name = categoryDto.Name,
                UserId = userId
            };

            _context.ExpenseCategories.Add(newCategory);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, new ExpenseCategoryDisplayDto
            {
                Id = newCategory.Id,
                Name = newCategory.Name
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(Guid id, ExpenseCategoryUpdateDto categoryDto)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (id != categoryDto.Id)
            {
                return BadRequest();
            }

            var existingCategory = _context.ExpenseCategories
                .FirstOrDefault(c => c.Id == id && c.UserId == userId);

            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = categoryDto.Name;
            _context.Entry(existingCategory).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(Guid id)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var category = _context.ExpenseCategories.FirstOrDefault(c => c.Id == id && c.UserId == userId);
            if (category == null)
            {
                return NotFound();
            }

            _context.ExpenseCategories.Remove(category);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
