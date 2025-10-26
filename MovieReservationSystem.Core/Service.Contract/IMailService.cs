using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface IMailService
    {
        void SendEmail(Email email);
    }
}
