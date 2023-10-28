using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using Impensa.Models;
using Impensa.Repositories;
using Impensa.DTOs;
using System.Collections.Generic;

namespace Impensa.Controllers
{
    [ApiController]
    [Route("/api/v1/expenses")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<ExpenseDisplayDTO>> GetAllExpenses()
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var expenses = _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Select(e => new ExpenseDisplayDTO
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Description = e.Description,
                    Date = e.Date.ToUniversalTime(),
                    ExpenseCategoryId = e.ExpenseCategoryId,
                    CategoryName = e.Category.Name
                })
                .ToList();
            
            return expenses;
        }

        [HttpGet("{id}")]
        public ActionResult<ExpenseDisplayDTO> GetExpense(Guid id)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var expense = _context.Expenses
                .Where(e => e.Id == id && e.UserId == userId)
                .Select(e => new ExpenseDisplayDTO
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Description = e.Description,
                    Date = e.Date,
                    ExpenseCategoryId = e.ExpenseCategoryId,
                    CategoryName = e.Category.Name
                })
                .FirstOrDefault();

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }

        [HttpPost]
        public ActionResult<ExpenseDisplayDTO> CreateExpense(ExpenseCreateDTO expenseDto)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var expense = new Expense
            {
                Amount = expenseDto.Amount,
                Description = expenseDto.Description,
                Date = expenseDto.Date,
                ExpenseCategoryId = expenseDto.ExpenseCategoryId,
                UserId = userId
            };

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            var createdExpenseDto = new ExpenseDisplayDTO
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Description = expense.Description,
                Date = expense.Date,
                ExpenseCategoryId = expense.ExpenseCategoryId,
                CategoryName = expense.Category.Name
            };

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, createdExpenseDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateExpense(Guid id, ExpenseUpdateDTO expenseDto)
        {
            if (id != expenseDto.Id)
            {
                return BadRequest();
            }

            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingExpense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == userId);
            if (existingExpense == null)
            {
                return NotFound();
            }

            existingExpense.Amount = expenseDto.Amount;
            existingExpense.Description = expenseDto.Description;
            existingExpense.ExpenseCategoryId = expenseDto.ExpenseCategoryId;

            // Add any other fields you want to update

            _context.Entry(existingExpense).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteExpense(Guid id)
        {
            var userId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == userId);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
