using Akka.Actor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AkkaAPIIS
{
    public class HttpActor : ReceiveActor
    {
        private readonly IActorRef _userActor;
        private readonly HttpListener _listener;

        public HttpActor(IActorRef userActor)
        {
            _userActor = userActor;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8081/");
            _listener.Start();
            Console.WriteLine("API en ejecución en http://localhost:8080/.");

            ReceiveAsync<HttpListenerContext>(async context =>
            {
                var request = context.Request;
                var response = context.Response;
                var url = request.Url.LocalPath;

                switch (request.HttpMethod)
                {
                    case "POST":
                        if (url == "/students")
                        {
                            var userJson = new StreamReader(request.InputStream).ReadToEnd();
                            var user = JsonConvert.DeserializeObject<Student>(userJson);
                            var createUser = new CUStudent { student = user };
                            var result = await _userActor.Ask<string>(createUser);
                            WriteResponse(response, result);
                        }
                        else
                        {
                            WriteResponse(response, "No se encontró el recurso solicitado.", statusCode: 404);
                        }
                        break;
                    case "GET":
                        if (url.StartsWith("/students/"))
                        {
                            var userId = url.Split('/')[2];
                            var readStudent = new Student { Carnet = userId };
                            var result = await _userActor.Ask<Student>(readStudent);
                            WriteResponse(response, JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            WriteResponse(response, "No se encontró el recurso solicitado.", statusCode: 404);
                        }
                        break;
                    case "PUT":
                        if (url.StartsWith("/students/"))
                        {
                            var userId = url.Split('/')[2];
                            var userJson = new StreamReader(request.InputStream).ReadToEnd();
                            var user = JsonConvert.DeserializeObject<Student>(userJson);
                            var updateStudent = new CUStudent { Id = userId, student = user };
                            var result = await _userActor.Ask<string>(updateStudent);
                            WriteResponse(response, result);
                        }
                        else
                        {
                            WriteResponse(response, "No se encontró el recurso solicitado.", statusCode: 404);
                        }
                        break;
                    case "DELETE":
                        if (url.StartsWith("/students/"))
                        {
                            var userId = url.Split('/')[2];
                            var deleteStudent = new Student { Carnet = userId };
                            var result = await _userActor.Ask<string>(deleteStudent);
                            WriteResponse(response, result);
                        }
                        else
                        {
                            WriteResponse(response, "No se encontró el recurso solicitado.", statusCode: 404);
                        }
                        break;
                    default:
                        WriteResponse(response, "Método HTTP no soportado.", statusCode: 405);
                        break;
                }

                context.Response.Close();
            });
        }

        private void WriteResponse(HttpListenerResponse response, string message, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(message);
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        protected override void PostStop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
