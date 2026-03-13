using ArenaBackend.Data;
using ArenaBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ArenaDbContext _context;

        public AnalyticsService(ArenaDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetDashboardAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var salesToday = await _context.Sales
                .Where(s => s.SaleDate >= today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var resToday = await _context.Reservations
                .Where(r => r.ReservationDate >= today && r.Status == "Finalizado")
                .SumAsync(r => (decimal?)r.Price) ?? 0;

            var payToday = await _context.StudentPayments
                .Where(p => p.PaymentDate >= today && p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var totalToday = salesToday + resToday + payToday;

            var salesMonth = await _context.Sales
                .Where(s => s.SaleDate >= firstDayOfMonth)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var resMonth = await _context.Reservations
                .Where(r => r.ReservationDate >= firstDayOfMonth && r.Status == "Finalizado")
                .SumAsync(r => (decimal?)r.Price) ?? 0;

            var payMonth = await _context.StudentPayments
                .Where(p => p.PaymentDate >= firstDayOfMonth && p.Status == "Paid")
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var totalMonth = salesMonth + resMonth + payMonth;

            var activeStudents = await _context.Students.CountAsync();
            var openTabs = await _context.Tabs.CountAsync(t => t.Status == "Open");
            var lowStock = await _context.Products.CountAsync(p => p.IsActive && p.Stock < 5);
            var reservationsToday = await _context.Reservations.CountAsync(r => r.ReservationDate >= today && r.ReservationDate < today.AddDays(1));
            var cashRegisterOpen = await _context.CashRegisters.AnyAsync(cr => cr.Status == "Open");

            return new DashboardSummaryDto
            {
                SalesToday = totalToday,
                SalesMonth = totalMonth,
                ActiveStudents = activeStudents,
                OpenTabs = openTabs,
                LowStockProducts = lowStock,
                TotalReservationsToday = reservationsToday,
                CashRegisterOpen = cashRegisterOpen
            };
        }

        public async Task<IEnumerable<object>> GetSalesByDayAsync(int days)
        {
            var from = DateTime.Now.Date.AddDays(-days + 1);

            var grouped = await _context.Sales
                .Where(s => s.SaleDate >= from)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(s => s.TotalAmount)
                })
                .ToListAsync();

            return grouped
                .Select(g => new SalesByDayDto
                {
                    Date = g.Date.ToString("yyyy-MM-dd"),
                    Total = g.Total
                })
                .OrderBy(x => x.Date);
        }

        public async Task<IEnumerable<object>> GetSalesByCategoryAsync()
        {
            return await _context.SaleItems
                .Include(si => si.Product).ThenInclude(p => p!.Category)
                .GroupBy(si => si.Product != null && si.Product.Category != null
                    ? si.Product.Category.Name
                    : "Sem Categoria")
                .Select(g => new SalesByCategoryDto
                {
                    Category = g.Key,
                    Total = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTopProductsAsync(int limit)
        {
            return await _context.SaleItems
                .Include(si => si.Product)
                .GroupBy(si => new { si.ProductId, Name = si.Product != null ? si.Product.Name : "Desconhecido" })
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key.Name,
                    TotalQty = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetSalesByProductAsync(int days)
        {
            var from = DateTime.Now.Date.AddDays(-days + 1);

            return await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .Where(si => si.Sale != null && si.Sale.SaleDate >= from)
                .GroupBy(si => new { si.ProductId, Name = si.Product != null ? si.Product.Name : "Desconhecido" })
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    TotalQty = g.Sum(si => si.Quantity),
                    TotalRevenue = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetInventorySummaryAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "Sem Categoria",
                    p.Stock,
                    p.Price,
                    p.CostPrice,
                    IsLowStock = p.Stock < 5
                })
                .OrderBy(p => p.Stock)
                .ToListAsync();
        }

        public async Task<object> GetBillingMetricsAsync()
        {
            var today = DateTime.Now.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalActiveStudents = await _context.StudentSubscriptions
                .Where(s => s.Status == "Active")
                .Select(s => s.StudentId)
                .Distinct()
                .CountAsync();

            var monthlyRecurringRevenue = await _context.StudentSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.Status == "Active" && s.Plan != null)
                .SumAsync(s => s.Plan!.Price);

            var overduePayments = await _context.StudentPayments
                .CountAsync(p => p.Status == "Overdue");

            var paymentsReceivedThisMonth = await _context.StudentPayments
                .Where(p => p.Status == "Paid" && p.PaymentDate >= startOfMonth)
                .SumAsync(p => p.Amount);

            return new
            {
                TotalActiveStudents = totalActiveStudents,
                MonthlyRecurringRevenue = monthlyRecurringRevenue,
                OverduePayments = overduePayments,
                PaymentsReceivedThisMonth = paymentsReceivedThisMonth
            };
        }

        public async Task<IEnumerable<PaymentHistoryDto>> GetPaymentHistoryAsync()
        {
            var history = new List<PaymentHistoryDto>();

            // 1. Sales (PDV and Comandas)
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Student)
                .ToListAsync();

            var tabs = await _context.Tabs.ToListAsync();
            var tabSaleIds = tabs.Where(t => t.SaleId.HasValue).Select(t => t.SaleId!.Value).ToHashSet();

            foreach (var sale in sales)
            {
                var isComanda = tabSaleIds.Contains(sale.Id);
                history.Add(new PaymentHistoryDto
                {
                    Id = sale.Id,
                    Type = isComanda ? "Comanda" : "PDV",
                    Date = sale.SaleDate,
                    ClientName = sale.Student?.Name ?? sale.Customer?.Name ?? "Consumidor",
                    PaymentType = sale.PaymentType ?? "Desconhecido",
                    TotalAmount = sale.TotalAmount,
                    Reference = $"Venda #{sale.Id}"
                });
            }

            // 2. Reservations (Quadras)
            var reservations = await _context.Reservations
                .Where(r => r.Status == "Finalizado")
                .ToListAsync();

            foreach (var res in reservations)
            {
                history.Add(new PaymentHistoryDto
                {
                    Id = res.Id,
                    Type = "Quadras",
                    Date = res.ReservationDate.Date.Add(res.StartTime),
                    ClientName = res.CustomerName ?? "-",
                    PaymentType = res.PaymentType ?? "Desconhecido",
                    TotalAmount = res.Price,
                    Reference = $"Reserva #{res.Id}"
                });
            }

            // 3. Payments (Mensalidades/Planos Avulsos)
            var payments = await _context.StudentPayments
                .Include(p => p.StudentSubscription).ThenInclude(ss => ss!.Student)
                .Where(p => p.Status == "Paid")
                .ToListAsync();

            foreach (var pay in payments)
            {
                history.Add(new PaymentHistoryDto
                {
                    Id = pay.Id,
                    Type = "Alunos",
                    Date = pay.PaymentDate ?? pay.DueDate,
                    ClientName = pay.StudentSubscription?.Student?.Name ?? "-",
                    PaymentType = pay.PaymentMethod ?? "Desconhecido",
                    TotalAmount = pay.Amount,
                    Reference = $"Fatura #{pay.Id}"
                });
            }

            // Order correctly (newest first)
            return history.OrderByDescending(h => h.Date).ToList();
        }
    }
}
