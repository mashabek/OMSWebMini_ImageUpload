using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMSWebMini.Data;
using OMSWebMini.Model;

namespace OMSWebMini.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[AutoValidateAntiforgeryToken]   //Enable Antiforgery Token for POST/PUT/DELETE
    public class EmployeesController : ControllerBase
    {
        private readonly NORTHWNDContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmployeesController(NORTHWNDContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Object>>> GetEmployees()
        {
            return await _context.Employees.Select(e => new Employee
            {
                EmployeeId = e.EmployeeId,
                LastName = e.LastName

            }).ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // POST: api/Employees
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee([FromForm]Employee employee)
        {
            string savedImagePath = await UploadEmployeeImageAsync();
            employee.PhotoPath = savedImagePath;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
       
            
            return CreatedAtAction("GetEmployee",
                new { id = employee.EmployeeId },
                employee);
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return employee;
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

        private async Task<string> UploadEmployeeImageAsync()
        {
            string[] permittedExtensions = { ".jpg", "jpeg", ".png" };

            
            var image = Request.Form.Files.FirstOrDefault();
            if (image == null || image.Length == 0) return null;

            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return null;
            }
            //In production use different folder(not application folders) to save files
            //this will add more security, don't allow to execute code in this folder. Make sure
            //you have read/write permissons
            //Always sanitize FileName or use RandomFileName as in this example
            var filePath = Path.Combine(_hostEnvironment.WebRootPath + "/images/",
      Path.GetRandomFileName() + ext);

            using (var stream = System.IO.File.Create(filePath))
            {
                await image.CopyToAsync(stream);
            }
            return filePath;
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

        }
    }
}
