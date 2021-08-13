using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scheduling.Appointments
{
    public interface IAppointmentsService
    {
        Task Add(Appointment appointment);
        Task<List<AppointmentReadModel>> GetAppointments();
        public Task<List<Doctor>> GetDoctors();
        Task<List<Patient>> GetPatients();
        Task Add(Patient patient);
    }
}