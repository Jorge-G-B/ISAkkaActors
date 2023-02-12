using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaAPIIS
{
    public class StudentActor : ReceiveActor
    {
        private readonly IDictionary<string, Student> _users = new Dictionary<string, Student>();

        public StudentActor()
        {
            Receive<CUStudent>(message =>
            {
                if (_users.ContainsKey(message.student.Carnet))
                {
                    Sender.Tell(new Exception($"El usuario con ID {message.student.Carnet} ya existe."));
                }
                else
                {
                    _users.Add(message.student.Carnet, message.student);
                    Sender.Tell("Usuario creado exitosamente.");
                }
            });

            Receive<Student>(message =>
            {
                if (_users.TryGetValue(message.Carnet, out var user))
                {
                    Sender.Tell(user);
                }
                else
                {
                    Sender.Tell(new Exception($"No se encontró el usuario con ID {message.Carnet}."));
                }
            });

            Receive<CUStudent>(message =>
            {
                if (_users.ContainsKey(message.Id))
                {
                    _users[message.Id] = message.student;
                    Sender.Tell("Usuario actualizado exitosamente.");
                }
                else
                {
                    Sender.Tell(new Exception($"No se encontró el usuario con ID {message.Id}."));
                }
            });

            Receive<Student>(message =>
            {
                if (_users.ContainsKey(message.Carnet))
                {
                    _users.Remove(message.Carnet);
                    Sender.Tell("Usuario eliminado exitosamente.");
                }
                else
                {
                    Sender.Tell(new Exception($"No se encontró el usuario con ID {message.Carnet}."));
                }
            });
        }
    }
}
