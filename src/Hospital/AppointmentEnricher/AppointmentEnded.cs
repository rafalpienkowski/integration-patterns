using System;

namespace AppointmentEnricher
{
    public class AppointmentEnded
    {
        public Guid DoctorId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime Date { get; set; }
    }

    public class AppointmentEndedEnriched : AppointmentEnded
    {
        public string CustomerAddress { get; set; }
    }
}