
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransportIn.Data;

// Отличаем сущность PostgreSQL от модели для интерфейса
using EmployeeEntity = TransportIn.Data.Entities.Employee;

namespace TransportIn.Services
{
    // Модель для отображения сотрудников на странице
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "Водій";
        public string Phone { get; set; } = "";
        public string LicenseCategory { get; set; } = "C, CE";
        public string MedicalCertExpiry { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class EmployeeService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public EmployeeService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public event Action? OnChange;

        // Получение сотрудников
        public List<Employee> GetAllEmployees()
        {
            using var db = _contextFactory.CreateDbContext();

            return db.Employees
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .Select(e => new Employee
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Role = e.RoleNavigation.Name,
                    Phone = e.Phone,
                    LicenseCategory = e.LicenseCategory,
                    MedicalCertExpiry = e.MedicalCertExpiry,
                    IsActive = e.IsActive
                })
                .ToList();
        }

        // Добавление сотрудника
        public void AddEmployee(Employee employee)
        {
            using var db = _contextFactory.CreateDbContext();

            string roleName = employee.Role.Trim();

            var role = db.Roles
                .FirstOrDefault(r => r.Name == roleName);

            if (role == null)
            {
                throw new InvalidOperationException(
                    $"Роль '{roleName}' не найдена в таблице Roles."
                );
            }

            var newEmployee = new EmployeeEntity
            {
                FullName = employee.FullName,
                Role = roleName,
                Phone = employee.Phone,
                LicenseCategory = employee.LicenseCategory,
                MedicalCertExpiry = employee.MedicalCertExpiry,
                IsActive = true,
                Address = "",
                Email = "",
                PhotoPath = "",
                RoleId = role.Id
            };

            db.Employees.Add(newEmployee);
            db.SaveChanges();

            employee.Id = newEmployee.Id;

            NotifyStateChanged();
        }

        // Деактивация сотрудника
        public void DeleteEmployee(int id)
        {
            using var db = _contextFactory.CreateDbContext();

            var employee = db.Employees
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return;

            employee.IsActive = false;

            db.SaveChanges();

            NotifyStateChanged();
        }

        // Повторная активация сотрудника
        public void ActivateEmployee(int id)
        {
            using var db = _contextFactory.CreateDbContext();

            var employee = db.Employees
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return;

            employee.IsActive = true;

            db.SaveChanges();

            NotifyStateChanged();
        }

        // Асинхронное получение сотрудников
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            await using var db =
                await _contextFactory.CreateDbContextAsync();

            return await db.Employees
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .Select(e => new Employee
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Role = e.RoleNavigation.Name,
                    Phone = e.Phone,
                    LicenseCategory = e.LicenseCategory,
                    MedicalCertExpiry = e.MedicalCertExpiry,
                    IsActive = e.IsActive
                })
                .ToListAsync();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
