using ArenaBackend.Data;
using ArenaBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace ArenaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ArenaDbContext _context;

        public SeedController(ArenaDbContext context)
        {
            _context = context;
        }

        [HttpPost("run")]
        public IActionResult SeedData()
        {
            try
            {
                // 1) 10 Clientes (Customers)
                var customers = Enumerable.Range(1, 10).Select(i => new Customer
                {
                    Name = $"Cliente Teste {i}",
                    Phone = $"119999900{i:D2}",
                    Email = $"cliente{i}@teste.com",
                    CreatedAt = DateTime.Now
                }).ToList();

                if (!_context.Customers.Any())
                {
                    _context.Customers.AddRange(customers);
                    _context.SaveChanges();
                }
                else
                {
                    customers = _context.Customers.Take(10).ToList();
                }

                // 2) 10 Alunos (Students)
                var students = Enumerable.Range(1, 10).Select(i => new Student
                {
                    Name = $"Aluno Teste {i}",
                    Phone = $"118888800{i:D2}",
                    Email = $"aluno{i}@teste.com",
                    PlanName = "Mensal",
                    StartDate = DateTime.Now.AddMonths(-i),
                    CreatedAt = DateTime.Now
                }).ToList();

                if (!_context.Students.Any())
                {
                    _context.Students.AddRange(students);
                    _context.SaveChanges();
                }
                else
                {
                    students = _context.Students.Take(10).ToList();
                }

                // 3) 5 Funcionários (Employees)
                var role = _context.Roles.FirstOrDefault();
                if (role == null)
                {
                    role = new Role { Name = "Instrutor" };
                    _context.Roles.Add(role);
                    _context.SaveChanges();
                }

                var employees = Enumerable.Range(1, 5).Select(i => new Employee
                {
                    Name = $"Funcionario Teste {i}",
                    Phone = $"116666600{i:D2}",
                    Email = $"func{i}@teste.com",
                    RoleId = role.Id,
                    Active = true,
                    CreatedAt = DateTime.Now
                }).ToList();

                if (!_context.Employees.Any())
                {
                    _context.Employees.AddRange(employees);
                    _context.SaveChanges();
                }
                else
                {
                    employees = _context.Employees.Take(5).ToList();
                }

                // 4) 10 Produtos (Products)
                var category = _context.ProductCategories.FirstOrDefault();
                if (category == null)
                {
                    category = new ProductCategory { Name = "Geral" };
                    _context.ProductCategories.Add(category);
                    _context.SaveChanges();
                }

                var products = Enumerable.Range(1, 10).Select(i => new Product
                {
                    Name = $"Produto Teste {i}",
                    Price = 10.0m * i,
                    CostPrice = 5.0m * i,
                    Stock = 100,
                    CategoryId = category.Id,
                    IsActive = true
                }).ToList();

                if (!_context.Products.Any())
                {
                    _context.Products.AddRange(products);
                    _context.SaveChanges();
                }
                else
                {
                    products = _context.Products.Take(10).ToList();
                }

                // 5) 5 Reservas de Quadra (Courts & Reservations)
                var court = _context.Courts.FirstOrDefault();
                if (court == null)
                {
                    court = new Court { Name = "Quadra Teste", SportType = "Areia", IsActive = true };
                    _context.Courts.Add(court);
                    _context.SaveChanges();
                }

                var reservations = Enumerable.Range(1, 5).Select(i => new Reservation
                {
                    CourtId = court.Id,
                    CustomerName = customers[i % customers.Count].Name,
                    ReservationDate = DateTime.Now.AddDays(i),
                    StartTime = new TimeSpan(10 + i, 0, 0),
                    EndTime = new TimeSpan(11 + i, 0, 0),
                    Price = 100.0m,
                    Status = "Agendado",
                    PaymentType = "PIX",
                    CreatedAt = DateTime.Now
                }).ToList();

                if (!_context.Reservations.Any())
                {
                    _context.Reservations.AddRange(reservations);
                    _context.SaveChanges();
                }

                // 6) 5 Vendas no Caixa (Sales)
                var cashRegister = _context.CashRegisters.FirstOrDefault(c => c.Status == "Open");
                if (cashRegister == null)
                {
                    cashRegister = new CashRegister
                    {
                        OpenedAt = DateTime.Now,
                        OpeningAmount = 100.0m,
                        Status = "Open"
                    };
                    _context.CashRegisters.Add(cashRegister);
                    _context.SaveChanges();
                }

                var sales = Enumerable.Range(1, 5).Select(i => new Sale
                {
                    CustomerId = customers[i % customers.Count].Id,
                    SaleDate = DateTime.Now,
                    TotalAmount = products[i % products.Count].Price,
                    PaymentType = "Cartão de Crédito",
                    Status = "Completed",
                    Items = new System.Collections.Generic.List<SaleItem>
                    {
                        new SaleItem
                        {
                            ProductId = products[i % products.Count].Id,
                            Quantity = 1,
                            UnitPrice = products[i % products.Count].Price
                        }
                    }
                }).ToList();

                if (!_context.Sales.Any())
                {
                    _context.Sales.AddRange(sales);
                    _context.SaveChanges();

                    foreach (var sale in sales)
                    {
                        _context.CashTransactions.Add(new CashTransaction
                        {
                            CashRegisterId = cashRegister.Id,
                            Type = "Sale",
                            Amount = sale.TotalAmount,
                            Description = $"Venda Teste #{sale.Id}",
                            CreatedAt = DateTime.Now,
                            SaleId = sale.Id
                        });
                    }
                    _context.SaveChanges();
                }

                return Ok(new { message = "Dados de teste inseridos com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
