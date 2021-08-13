using System;
using System.Threading.Tasks;

namespace Scheduling.Appointments
{
    public class AppointmentsService : IAppointmentsService
    {
        private readonly IAppointmentsRepository _appointmentsRepository;

        public AppointmentsService(IAppointmentsRepository appointmentsRepository)
        {
            _appointmentsRepository = appointmentsRepository;
        }

        public async Task Add(Appointment appointment)
        {
            appointment.Id = Guid.NewGuid();
            await _appointmentsRepository.Add(appointment);
        }
    }
}