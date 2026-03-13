using System;

namespace ArenaBackend.DTOs
{
    public class PaymentHistoryDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "PDV", "Comanda", "Quadras", "Alunos"
        public DateTime Date { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Reference { get; set; } = string.Empty; // e.g. "Sale #12", "Reserva #3"
    }
}
