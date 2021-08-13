using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Scheduling.Appointments
{
    public class AppointmentsService : IAppointmentsService
    {
        private readonly SchedulingContext _context;
        private readonly ILogger<AppointmentsService> _logger;
        private readonly string _rabbitHost;
        private const string QueueName = "scheduling-appointment-ended";

        public AppointmentsService(SchedulingContext context, ILogger<AppointmentsService> logger, IOptions<Messaging> messaging)
        {
            _context = context;
            _logger = logger;
            _rabbitHost = messaging.Value.HostName;
        }

        public async Task Add(Appointment appointment)
        {
            appointment.Id = Guid.NewGuid();
            appointment.Date = DateTime.Now;
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            
            PublishEvent(appointment);
        }

        private void PublishEvent(Appointment appointment)
        {
            var factory = new ConnectionFactory{ HostName = _rabbitHost };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var appointmentEnded = new AppointmentEnded
            {
                CustomerId = appointment.CustomerId,
                DoctorId = appointment.DoctorId,
                Date = appointment.Date
            };

            var message = JsonConvert.SerializeObject(appointmentEnded);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "",
                routingKey: QueueName,
                basicProperties: null,
                body: body);
            
            _logger.LogInformation("Appointment ended published");
        }

        public Task<List<AppointmentReadModel>> GetAppointments()
        {
            return _context.Appointments.Join(_context.Customers, a => a.CustomerId, c => c.Id,
                (appointment, customer) => new
                {
                    CustomerName = customer.Name,
                    DoctorId = appointment.DoctorId,
                    Date = appointment.Date
                }).Join(_context.Doctors, arg => arg.DoctorId, d => d.Id, (appointment, doctor) =>
                new AppointmentReadModel
                {
                    DoctorName = doctor.Name,
                    PatientName = appointment.CustomerName,
                    Date = appointment.Date
                }).ToListAsync();
        }

        public Task<List<Doctor>> GetDoctors() => _context.Doctors.ToListAsync();
        
        public Task<List<Patient>> GetPatients() => _context.Customers.ToListAsync();

        public async Task Add(Patient patient)
        {
            await _context.Customers.AddAsync(patient);
            await _context.SaveChangesAsync();
        }
    }
}