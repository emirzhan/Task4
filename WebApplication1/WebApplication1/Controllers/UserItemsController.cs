using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserItemsController : ControllerBase
    {
        private readonly IUserItemRepository _repository;

        public UserItemsController(IUserItemRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserItemDTO>>> GetTodoItems()
        {
            try
            {
                var items = await _repository.GetUserItemsAsync();
                return items.Select(x => ItemToDTO(x)).ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserItemDTO>> GetTodoItem(long id)
        {
            try
            {
                var userItem = await _repository.GetUserItemByIdAsync(id);

                if (userItem == null)
                {
                    return NotFound();
                }

                return ItemToDTO(userItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, UserItemDTO userDTO)
        {
            if (id != userDTO.Id)
            {
                return BadRequest();
            }

            try
            {
                var userItem = await _repository.GetUserItemByIdAsync(id);
                if (userItem == null)
                {
                    return NotFound();
                }

                userItem.Name = userDTO.Name;
                userItem.Age = userDTO.Age;
                userItem.Gender = userDTO.Gender;

                await _repository.UpdateUserItemAsync(userItem);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _repository.UserItemExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500, "Internal server error");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserItemDTO>> PostTodoItem(UserItemDTO userDTO)
        {
            try
            {
                var userItem = new UserItem
                {
                    Age = userDTO.Age,
                    Name = userDTO.Name,
                    Gender = userDTO.Gender,
                };

                await _repository.AddUserItemAsync(userItem);

                return CreatedAtAction(nameof(GetTodoItem), new { id = userItem.Id }, ItemToDTO(userItem));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            try
            {
                var userItem = await _repository.GetUserItemByIdAsync(id);
                if (userItem == null)
                {
                    return NotFound();
                }

                await _repository.DeleteUserItemAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("age/{age}")]
        public async Task<ActionResult<IEnumerable<UserItemDTO>>> GetUsersByAge(int age)
        {
            try
            {
                var items = await _repository.GetUsersByAgeAsync(age);
                return items.Select(x => ItemToDTO(x)).ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("age/above/{age}")]
        public async Task<ActionResult<IEnumerable<UserItemDTO>>> GetUsersByHigherAge(int age)
        {
            try
            {
                var items = await _repository.GetUsersByHigherAgeAsync(age);
                return items.Select(x => ItemToDTO(x)).ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("age/below/{age}")]
        public async Task<ActionResult<IEnumerable<UserItemDTO>>> GetUsersByBelowAge(int age)
        {
            try
            {
                var items = await _repository.GetUsersByBelowAgeAsync(age);
                return items.Select(x => ItemToDTO(x)).ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private static UserItemDTO ItemToDTO(UserItem userItem) =>
           new UserItemDTO
           {
               Id = userItem.Id,
               Name = userItem.Name,
               Age = userItem.Age,
               Gender = userItem.Gender,
           };
    }
}
