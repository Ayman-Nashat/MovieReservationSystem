using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface IPaymentService
    {
        Task<Payment?> CreateOrUpdatePaymentIntentAsync(int reservationId);
        Task<Reservation?> UpdatePaymentStatusAsync(string paymentIntentId, bool isSucceeded);
    }
}
