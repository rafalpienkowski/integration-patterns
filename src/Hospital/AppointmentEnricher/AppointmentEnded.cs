using System;

namespace Scheduling.Appointments
{
    public class AppointmentEnded
    {
        public Guid DoctorId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime Date { get; set; }
    }
}