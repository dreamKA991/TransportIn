
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly AppDbContext _db;

        public event Action? OnChange;

        public EmployeeService(AppDbContext db)
        {
            _db = db;
        }

        // Получение активных сотрудников из PostgreSQL
        public List<Employee> GetAllEmployees()
        {
            return _db.Employees
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

        // Добавление сотрудника в PostgreSQL
        public void AddEmployee(Employee employee)
        {
            string roleName = employee.Role.Trim();

            var role = _db.Roles
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

            _db.Employees.Add(newEmployee);
            _db.SaveChanges();

            employee.Id = newEmployee.Id;

            NotifyStateChanged();
        }

        // Вместо физического удаления деактивируем сотрудника,
        // чтобы сохранить связанные документы и историю
        public void DeleteEmployee(int id)
        {
            var employee = _db.Employees
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return;

            employee.IsActive = false;

            _db.SaveChanges();

            NotifyStateChanged();
        }

        // Повторная активация сотрудника
        public void ActivateEmployee(int id)
        {
            var employee = _db.Employees
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return;

            employee.IsActive = true;

            _db.SaveChanges();

            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
