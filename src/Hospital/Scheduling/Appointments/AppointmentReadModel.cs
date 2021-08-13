using System;

namespace Scheduling.Appointments
{
    public class AppointmentReadModel
    {
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public DateTime Date { get; set; }
    }
}