using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Scheduling.Appointments
{
    public class CreateCustomerMessageWrapper
    {
        private readonly JObject _jsonDocument;

        public CreateCustomerMessageWrapper(byte[] messageBody)
        {
            var message = Encoding.UTF8.GetString(messageBody);
            _jsonDocument = JObject.Parse(message);
        }

        public Guid Id
        {
            get
            {
                var jsonIdToken = _jsonDocument.SelectToken("Id");
                if (jsonIdToken == null)
                {
                    throw new ArgumentNullException(nameof(Id), $"Customer Id is missing");
                }

                if (!Guid.TryParse(jsonIdToken.Value<string>(), out var id))
                {
                    throw new ArgumentOutOfRangeException(nameof(Id),
                        $"Invalid 'Id' format: {jsonIdToken.Value<string>()}");
                }

                return id;
            }
        }

        public string Name
        {
            get
            {
                var jsonNameToken = _jsonDocument.SelectToken("Name");
                return jsonNameToken == null 
                    ? "Anonymous" 
                    : jsonNameToken.Value<string>();
            }
        }
    }
}