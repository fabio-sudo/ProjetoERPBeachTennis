using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public interface IBillingService
    {
        Task GenerateMonthlyChargesAsync();
        Task CheckOverduePaymentsAsync();
    }
}
