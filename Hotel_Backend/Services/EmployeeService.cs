using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Hotel_Backend.Services
{
    public class EmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public EmployeeService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> UploadPhotoAsync(int id, IFormFile photo)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return false;
            }

            var basePath = _configuration.GetSection("AttachmentConfig").GetValue<string>("BasePath");
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = @"C:\hotel attachment";
            }

            var employeeDir = Path.Combine(basePath, "employee");
            if (!Directory.Exists(employeeDir))
            {
                Directory.CreateDirectory(employeeDir);
            }

            var ext = Path.GetExtension(photo.FileName);
            var fileName = $"{id}{ext}";
            var fullPath = Path.Combine(employeeDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            employee.PhotoPath = $"/attachments/employee/{fileName}";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmployeeAsync(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return false;
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return false;
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
