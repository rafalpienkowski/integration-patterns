using System.Threading.Tasks;

namespace Scheduling.Appointments
{
    public interface IAppointmentsService
    {
        Task Add(Appointment appointment);
    }
}