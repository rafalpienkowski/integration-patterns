using System;

namespace Scheduling.Appointments
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime Date { get; set; }
    }
}